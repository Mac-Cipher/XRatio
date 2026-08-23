using System.Net;
using System.Net.Sockets;
using XRatio.Core.Announcements;

namespace XRatio.Core.Simulation;

public interface ITrackerClient
{
    Task<TrackerAnnounceResult> AnnounceAsync(TrackerAnnounce announce, CancellationToken cancellationToken);
}

public sealed class TrackerClient : ITrackerClient
{
    private const int MaxTrackerResponseBytes = 4 * 1024 * 1024;
    private readonly TimeSpan _timeout;

    public TrackerClient(TimeSpan? timeout = null)
    {
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public async Task<TrackerAnnounceResult> AnnounceAsync(
        TrackerAnnounce announce,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(announce);
        using var handler = CreateHandler(announce.Proxy);
        using var client = new HttpClient(handler) { Timeout = _timeout };
        var originalUri = BuildUri(announce);
        var httpsFallback = BuildHttpsFallbackUri(originalUri);
        try
        {
            return await AnnounceAtUriAsync(client, announce, originalUri, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (
            httpsFallback is not null &&
            IsConnectionFailure(exception, cancellationToken))
        {
            return await AnnounceAtUriAsync(client, announce, httpsFallback, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public static Uri? BuildHttpsFallbackUri(Uri trackerUri)
    {
        ArgumentNullException.ThrowIfNull(trackerUri);
        if (trackerUri.Scheme != Uri.UriSchemeHttp ||
            trackerUri.HostNameType != UriHostNameType.Dns ||
            trackerUri.Port == 80)
            return null;

        var builder = new UriBuilder(trackerUri)
        {
            Scheme = Uri.UriSchemeHttps,
            Port = -1
        };
        return builder.Uri;
    }

    private async Task<TrackerAnnounceResult> AnnounceAtUriAsync(
        HttpClient client,
        TrackerAnnounce announce,
        Uri originalUri,
        CancellationToken cancellationToken)
    {
        var currentUri = originalUri;
        HttpResponseMessage? response = null;
        for (var redirect = 0; redirect <= 5; redirect++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, currentUri);
            request.Headers.UserAgent.ParseAdd(announce.Client.UserAgent);
            request.Headers.Accept.ParseAdd("text/plain, */*;q=0.8");
            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
            if (!IsRedirect(response.StatusCode) || response.Headers.Location is null)
                break;
            if (redirect == 5)
                throw new HttpRequestException("Tracker redirect limit exceeded.");
            var next = response.Headers.Location.IsAbsoluteUri
                ? response.Headers.Location
                : new Uri(currentUri, response.Headers.Location);
            response.Dispose();
            response = null;
            if (!HasSameOrigin(originalUri, next))
                throw new HttpRequestException("Tracker redirect crossed the authorized origin.");
            currentUri = next;
        }
        using (response ?? throw new HttpRequestException("Tracker returned no response."))
        {
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > MaxTrackerResponseBytes)
                throw new InvalidDataException("Tracker response exceeds 4 MiB.");
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var destination = new MemoryStream();
            var buffer = new byte[16 * 1024];
            while (true)
            {
                var read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                    break;
                if (destination.Length + read > MaxTrackerResponseBytes)
                    throw new InvalidDataException("Tracker response exceeds 4 MiB.");
                destination.Write(buffer, 0, read);
            }

            var parsed = TrackerResponseParser.Parse(destination.GetBuffer().AsSpan(0, checked((int)destination.Length)));
            if (!string.IsNullOrWhiteSpace(parsed.FailureReason))
                throw new InvalidOperationException($"Tracker rejected the announce: {parsed.FailureReason}");
            return new TrackerAnnounceResult(
                Math.Clamp(parsed.Interval ?? 1800, 30, 7200),
                Math.Max(0, parsed.Complete ?? 0),
                Math.Max(0, parsed.Incomplete ?? 0));
        }
    }

    private static bool IsConnectionFailure(Exception exception, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return false;

        while (exception is not null)
        {
            if (exception is TaskCanceledException or SocketException)
                return true;
            exception = exception.InnerException!;
        }

        return false;
    }

    public static Uri BuildUri(TrackerAnnounce announce)
    {
        var separator = string.IsNullOrEmpty(announce.Tracker.Query) ? "?" : "&";
        var query = string.Join('&',
            $"info_hash={PercentEncode(Convert.FromHexString(announce.InfoHashHex))}",
            $"peer_id={PercentEncode(System.Text.Encoding.ASCII.GetBytes(announce.PeerId))}",
            $"port={announce.Port}",
            $"uploaded={announce.Uploaded}",
            $"downloaded={announce.Downloaded}",
            $"left={announce.Left}",
            $"compact=1",
            $"no_peer_id=1",
            $"numwant={announce.NumWant}",
            $"key={Uri.EscapeDataString(announce.Key)}",
            announce.Event == TrackerEvent.None ? string.Empty : $"event={announce.Event.ToString().ToLowerInvariant()}");
        return new Uri(announce.Tracker + separator + query.TrimEnd('&'));
    }

    private static HttpClientHandler CreateHandler(SimulationProxyOptions options)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All
        };
        if (options.Address is null)
            return handler;

        var proxy = new WebProxy(options.Address);
        if (!string.IsNullOrEmpty(options.Username))
            proxy.Credentials = new NetworkCredential(options.Username, options.Password);
        handler.Proxy = proxy;
        handler.UseProxy = true;
        return handler;
    }

    private static bool IsRedirect(HttpStatusCode statusCode) =>
        statusCode is HttpStatusCode.Moved or HttpStatusCode.Redirect or HttpStatusCode.RedirectMethod or
            HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;

    private static bool HasSameOrigin(Uri original, Uri candidate) =>
        original.Scheme.Equals(candidate.Scheme, StringComparison.OrdinalIgnoreCase) &&
        original.Host.Equals(candidate.Host, StringComparison.OrdinalIgnoreCase) &&
        original.Port == candidate.Port && candidate.UserInfo.Length == 0;

    private static string PercentEncode(ReadOnlySpan<byte> value)
    {
        var chars = new char[value.Length * 3];
        const string hex = "0123456789ABCDEF";
        for (var index = 0; index < value.Length; index++)
        {
            chars[index * 3] = '%';
            chars[(index * 3) + 1] = hex[value[index] >> 4];
            chars[(index * 3) + 2] = hex[value[index] & 0x0F];
        }
        return new string(chars);
    }
}
