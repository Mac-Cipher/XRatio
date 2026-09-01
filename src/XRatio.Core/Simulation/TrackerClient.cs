using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using XRatio.Core.Announcements;

namespace XRatio.Core.Simulation;

public interface ITrackerClient
{
    Task<TrackerAnnounceResult> AnnounceAsync(TrackerAnnounce announce, CancellationToken cancellationToken);
}

public sealed class TrackerClient : ITrackerClient, IDisposable
{
    private const int MaxTrackerResponseBytes = 4 * 1024 * 1024;
    private const int ResponseReadBufferBytes = 16 * 1024;
    private readonly TimeSpan _timeout;
    private readonly object _clientGate = new();
    private readonly Dictionary<SimulationProxyOptions, HttpClient> _clients = [];
    private bool _disposed;

    public TrackerClient(TimeSpan? timeout = null)
    {
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public async Task<TrackerAnnounceResult> AnnounceAsync(
        TrackerAnnounce announce,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(announce);
        var client = GetClient(announce.Proxy);
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
            var initialCapacity = response.Content.Headers.ContentLength is { } contentLength &&
                                  contentLength is >= 0 and <= MaxTrackerResponseBytes
                ? checked((int)contentLength)
                : ResponseReadBufferBytes;
            using var destination = new MemoryStream(initialCapacity);
            var buffer = ArrayPool<byte>.Shared.Rent(ResponseReadBufferBytes);
            try
            {
                while (true)
                {
                    var read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                        break;
                    if (destination.Length + read > MaxTrackerResponseBytes)
                        throw new InvalidDataException("Tracker response exceeds 4 MiB.");
                    destination.Write(buffer, 0, read);
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
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
        var query = new StringBuilder(192);
        query.Append("info_hash=");
        AppendPercentEncodedHex(query, announce.InfoHashHex);
        query.Append("&peer_id=");
        AppendPercentEncodedAscii(query, announce.PeerId);
        query.Append("&port=").Append(announce.Port);
        query.Append("&uploaded=").Append(announce.Uploaded);
        query.Append("&downloaded=").Append(announce.Downloaded);
        query.Append("&left=").Append(announce.Left);
        query.Append("&compact=1&no_peer_id=1&numwant=").Append(announce.NumWant);
        query.Append("&key=").Append(Uri.EscapeDataString(announce.Key));
        if (announce.Event != TrackerEvent.None)
            query.Append("&event=").Append(GetEventName(announce.Event));

        return new Uri(string.Concat(announce.Tracker, separator, query.ToString()));
    }

    private HttpClient GetClient(SimulationProxyOptions options)
    {
        lock (_clientGate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_clients.TryGetValue(options, out var client))
                return client;

            var handler = CreateHandler(options);
            client = new HttpClient(handler) { Timeout = _timeout };
            _clients.Add(options, client);
            return client;
        }
    }

    private static HttpClientHandler CreateHandler(SimulationProxyOptions options)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
            // The previous implementation created a new handler for every
            // announce, so cookies could never carry over between requests.
            // Disable the container while reusing handlers to preserve that
            // behavior and avoid an unnecessary per-handler allocation.
            UseCookies = false
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

    private static void AppendPercentEncodedHex(StringBuilder output, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if ((value.Length & 1) != 0)
            throw new FormatException("The info hash must contain pairs of hexadecimal characters.");

        const string hex = "0123456789ABCDEF";
        for (var index = 0; index < value.Length; index += 2)
        {
            if (!TryHex(value[index], out var high) || !TryHex(value[index + 1], out var low))
                throw new FormatException("The info hash must contain only hexadecimal characters.");
            var byteValue = (high << 4) | low;
            output.Append('%').Append(hex[byteValue >> 4]).Append(hex[byteValue & 0x0F]);
        }
    }

    private static void AppendPercentEncodedAscii(StringBuilder output, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        const string hex = "0123456789ABCDEF";
        foreach (var character in value)
        {
            var byteValue = character <= 0x7F ? (byte)character : (byte)'?';
            output.Append('%').Append(hex[byteValue >> 4]).Append(hex[byteValue & 0x0F]);
        }
    }

    private static string GetEventName(TrackerEvent trackerEvent) => trackerEvent switch
    {
        TrackerEvent.Started => "started",
        TrackerEvent.Completed => "completed",
        TrackerEvent.Stopped => "stopped",
        _ => trackerEvent.ToString().ToLowerInvariant()
    };

    private static bool TryHex(char value, out int result)
    {
        result = value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1
        };
        return result >= 0;
    }

    public void Dispose()
    {
        HttpClient[] clients;
        lock (_clientGate)
        {
            if (_disposed)
                return;
            _disposed = true;
            clients = _clients.Values.ToArray();
            _clients.Clear();
        }

        foreach (var client in clients)
            client.Dispose();
    }
}
