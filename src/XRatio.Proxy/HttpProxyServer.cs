using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Collections.Concurrent;
using System.Buffers;
using XRatio.Core.Announcements;
using XRatio.Core.Configuration;
using XRatio.Core.Platform;
using XRatio.Core.Simulation;

namespace XRatio.Proxy;

public sealed class HttpProxyServer : IAsyncDisposable
{
    private const int MaximumHeaderBytes = 64 * 1024;
    private const int MaximumTrackerResponseBytes = 4 * 1024 * 1024;
    private const int MaximumOutboundAttempts = 2;
    private const int HeaderReadBufferBytes = 8 * 1024;
    private const int TunnelCopyBufferBytes = 16 * 1024;
    private static readonly TimeSpan DefaultHeaderReadTimeout = TimeSpan.FromSeconds(10);
    private static readonly byte[] ConnectionEstablishedResponse =
        "HTTP/1.1 200 Connection Established\r\n\r\n"u8.ToArray();
    private static readonly IReadOnlySet<string> ExcludedResponseHeaders =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Content-Length", "Transfer-Encoding", "Connection"
        };
    private static readonly TimeSpan OutboundRetryDelay = TimeSpan.FromMilliseconds(250);
    private readonly AnnounceTransformer _transformer;
    private readonly Func<XRatioSettings> _settings;
    private readonly ICertificateAuthorityService? _certificates;
    private readonly Func<bool> _isPaused;
    private readonly IProxyDebugLogger? _debugLogger;
    private readonly Func<bool> _isDebugLogging;
    private readonly HttpClient _httpClient;
    private readonly TimeSpan _headerReadTimeout;
    private readonly SemaphoreSlim _connectionLimit = new(32, 32);
    private readonly ConcurrentDictionary<long, Task> _activeConnections = new();
    private TcpListener? _listener;
    private CancellationTokenSource? _shutdown;
    private Task? _acceptLoop;
    private long _connectionSequence;

    public HttpProxyServer(
        AnnounceTransformer transformer,
        Func<XRatioSettings> settings,
        ICertificateAuthorityService? certificates = null,
        HttpMessageHandler? outboundHandler = null,
        Func<bool>? isPaused = null,
        IProxyDebugLogger? debugLogger = null,
        Func<bool>? isDebugLogging = null,
        TimeSpan? headerReadTimeout = null)
    {
        _transformer = transformer;
        _settings = settings;
        _certificates = certificates;
        _isPaused = isPaused ?? (() => false);
        _debugLogger = debugLogger;
        _isDebugLogging = isDebugLogging ?? (() => false);
        _headerReadTimeout = headerReadTimeout ?? DefaultHeaderReadTimeout;
        if (_headerReadTimeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(headerReadTimeout));
        _httpClient = new HttpClient(outboundHandler ?? new SocketsHttpHandler
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectTimeout = TimeSpan.FromSeconds(15)
        })
        {
            Timeout = TimeSpan.FromSeconds(45)
        };
    }

    public event EventHandler<ProxyEvent>? Activity;

    public bool IsRunning => _listener is not null;

    public int BoundPort => (_listener?.LocalEndpoint as IPEndPoint)?.Port ?? 0;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_listener is not null)
            throw new InvalidOperationException("The proxy is already running.");

        var settings = _settings().Validate();
        var address = settings.OnlyLocalConnections ? IPAddress.Loopback : IPAddress.Any;
        var listener = new TcpListener(address, settings.ListenPort);
        CancellationTokenSource? shutdown = null;
        try
        {
            listener.Start(32);
            shutdown = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _listener = listener;
            _shutdown = shutdown;
            _acceptLoop = AcceptLoopAsync(listener, shutdown.Token);
        }
        catch
        {
            shutdown?.Dispose();
            listener.Stop();
            throw;
        }
        PublishActivity(new ProxyEvent(DateTimeOffset.Now, AnnounceDisposition.Forwarded,
            $"Proxy listening on {address}:{BoundPort}."));
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        var listener = Interlocked.Exchange(ref _listener, null);
        var shutdown = Interlocked.Exchange(ref _shutdown, null);
        if (listener is null)
        {
            shutdown?.Dispose();
            return;
        }

        if (shutdown is not null)
            await shutdown.CancelAsync();
        listener.Stop();
        if (_acceptLoop is not null)
        {
            try
            {
                await _acceptLoop;
            }
            catch (OperationCanceledException)
            {
            }
            catch (SocketException) when (shutdown?.IsCancellationRequested == true)
            {
            }
            catch (ObjectDisposedException) when (shutdown?.IsCancellationRequested == true)
            {
            }
        }
        var activeConnections = _activeConnections.Values.ToArray();
        if (activeConnections.Length > 0)
            await Task.WhenAll(activeConnections);
        shutdown?.Dispose();
        _acceptLoop = null;
    }

    private async Task AcceptLoopAsync(
        TcpListener listener,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Acquire a slot before accepting so a burst of stalled clients is
            // kept in the OS listen backlog instead of becoming one managed
            // task per socket. The handler owns and releases the slot.
            await _connectionLimit.WaitAsync(cancellationToken);
            TcpClient? client = null;
            try
            {
                client = await listener.AcceptTcpClientAsync(cancellationToken);
                var acceptedClient = client;
                client = null;
                TrackConnection(acceptedClient, cancellationToken);
            }
            finally
            {
                if (client is not null)
                {
                    client.Dispose();
                    _connectionLimit.Release();
                }
            }
        }
    }

    private void TrackConnection(TcpClient client, CancellationToken cancellationToken)
    {
        var id = Interlocked.Increment(ref _connectionSequence);
        var task = HandleBoundedAsync(client, cancellationToken);
        if (!_activeConnections.TryAdd(id, task))
        {
            client.Dispose();
            // The handler still owns the semaphore slot and releases it from
            // its finally block. A failed dictionary insertion is not expected
            // with the monotonic connection id, but it must not leak a socket.
            return;
        }
        _ = RemoveCompletedConnectionAsync(id, task);
    }

    private async Task RemoveCompletedConnectionAsync(long id, Task task)
    {
        try
        {
            await task;
        }
        catch (Exception exception)
        {
            PublishActivity(new ProxyEvent(
                DateTimeOffset.Now,
                AnnounceDisposition.RejectedInvalid,
                $"Unexpected connection failure: {DescribeException(exception)}"));
        }
        finally
        {
            _activeConnections.TryRemove(id, out _);
        }
    }

    private async Task HandleBoundedAsync(
        TcpClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            await HandleClientAsync(client, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (OperationCanceledException)
        {
            PublishActivity(new ProxyEvent(
                DateTimeOffset.Now,
                AnnounceDisposition.RejectedInvalid,
                "Connection header or TLS handshake deadline exceeded."));
        }
        catch (Exception exception) when (exception is IOException or SocketException or HttpRequestException or
                                          TaskCanceledException or AuthenticationException or CryptographicException or
                                          InvalidOperationException or ArgumentException)
        {
            PublishActivity(new ProxyEvent(DateTimeOffset.Now, AnnounceDisposition.RejectedInvalid,
                $"Connection failed: {DescribeException(exception)}"));
        }
        finally
        {
            client.Dispose();
            _connectionLimit.Release();
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        await using var clientStream = new BufferedReadStream(client.GetStream());
        var headerBytes = await ReadHeadersWithTimeoutAsync(clientStream, cancellationToken);
        if (headerBytes is null)
        {
            await WriteErrorAsync(clientStream, 431, "Request Header Fields Too Large", cancellationToken);
            return;
        }
        if (!HasHeaderTerminator(headerBytes))
        {
            await WriteErrorAsync(clientStream, 400, "Incomplete request headers", cancellationToken);
            return;
        }

        var headerText = Encoding.Latin1.GetString(headerBytes);
        var lines = headerText.Split(["\r\n", "\n"], StringSplitOptions.None);
        var firstLine = lines[0].Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (firstLine.Length != 3)
        {
            await WriteErrorAsync(clientStream, 400, "Bad Request", cancellationToken);
            return;
        }

        if (firstLine[0].Equals("CONNECT", StringComparison.OrdinalIgnoreCase))
        {
            await HandleConnectAsync(client, clientStream, firstLine[1], cancellationToken);
            return;
        }

        if (!firstLine[0].Equals("GET", StringComparison.OrdinalIgnoreCase) ||
            !Uri.TryCreate(firstLine[1], UriKind.Absolute, out var target))
        {
            await WriteErrorAsync(clientStream, 400, "Only absolute-form HTTP GET is supported", cancellationToken);
            return;
        }

        var effectiveSettings = GetEffectiveSettings(client);
        await ForwardHttpRequestAsync(clientStream, lines, target, effectiveSettings, cancellationToken);
    }

    private async Task HandleConnectAsync(
        TcpClient client,
        Stream clientStream,
        string authority,
        CancellationToken cancellationToken)
    {
        if (!TryParseAuthority(authority, out var host, out var port))
        {
            await WriteErrorAsync(clientStream, 400, "Invalid CONNECT authority", cancellationToken);
            return;
        }

        var settings = GetEffectiveSettings(client);
        if (port != 443)
        {
            if (settings.OnlyTrackerTraffic)
            {
                await WriteErrorAsync(clientStream, 403, "Opaque CONNECT blocked in tracker-only mode", cancellationToken);
                return;
            }
            await TunnelAsync(clientStream, host, port, cancellationToken);
            return;
        }

        if (_certificates is null || !_certificates.Capability.IsSupported)
        {
            await WriteErrorAsync(clientStream, 501, "HTTPS interception is unavailable on this platform", cancellationToken);
            return;
        }
        if (!await _certificates.IsTrustedAsync(cancellationToken))
        {
            await WriteErrorAsync(clientStream, 503,
                "XRatio local CA is not trusted; enable HTTPS explicitly in the application", cancellationToken);
            return;
        }

        using var serverCertificate = await _certificates.GetServerCertificateAsync(host, cancellationToken);
        await clientStream.WriteAsync(
            ConnectionEstablishedResponse,
            cancellationToken);
        await clientStream.FlushAsync(cancellationToken);

        string? tlsServerName = null;
        using var tlsStream = new SslStream(clientStream, leaveInnerStreamOpen: true);
        using var handshakeTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        handshakeTimeout.CancelAfter(_headerReadTimeout);
        await tlsStream.AuthenticateAsServerAsync(
            new SslServerAuthenticationOptions
            {
                ServerCertificateSelectionCallback = (_, serverName) =>
                {
                    tlsServerName = string.IsNullOrWhiteSpace(serverName) ? null : serverName;
                    return serverCertificate;
                },
                ClientCertificateRequired = false,
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck
            },
            handshakeTimeout.Token);

        await using var decryptedStream = new BufferedReadStream(tlsStream);
        var decryptedHeaders = await ReadHeadersWithTimeoutAsync(decryptedStream, cancellationToken);
        if (decryptedHeaders is null)
            return;
        if (!HasHeaderTerminator(decryptedHeaders))
        {
            await WriteErrorAsync(decryptedStream, 400, "Incomplete request headers", cancellationToken);
            return;
        }
        var lines = Encoding.Latin1.GetString(decryptedHeaders)
            .Split(["\r\n", "\n"], StringSplitOptions.None);
        var firstLine = lines[0].Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (firstLine.Length != 3 ||
            !firstLine[0].Equals("GET", StringComparison.OrdinalIgnoreCase) ||
            !Uri.TryCreate(firstLine[1], UriKind.RelativeOrAbsolute, out var requestTarget))
        {
            await WriteErrorAsync(decryptedStream, 400, "Only HTTPS GET tracker requests are supported", cancellationToken);
            return;
        }

        if (!TryGetHostHeader(lines, port, out var headerHost))
        {
            await WriteErrorAsync(decryptedStream, 400, "Invalid HTTPS Host header", cancellationToken);
            return;
        }

        var requestHost = SelectLogicalHost(host, headerHost, tlsServerName);
        Uri target;
        if (requestTarget.IsAbsoluteUri)
        {
            if (!requestTarget.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                (!requestTarget.Host.Equals(host, StringComparison.OrdinalIgnoreCase) &&
                 !requestTarget.Host.Equals(requestHost, StringComparison.OrdinalIgnoreCase)) ||
                requestTarget.Port != port)
            {
                await WriteErrorAsync(
                    decryptedStream,
                    400,
                    "HTTPS request target must match the CONNECT authority",
                    cancellationToken);
                return;
            }
            target = BuildHttpsTarget(requestHost, port, requestTarget.PathAndQuery);
        }
        else
        {
            var resource = firstLine[1].StartsWith('/') ? firstLine[1] : "/" + firstLine[1];
            target = BuildHttpsTarget(requestHost, port, resource);
        }
        await ForwardHttpRequestAsync(decryptedStream, lines, target, settings, cancellationToken);
    }

    private async Task ForwardHttpRequestAsync(
        Stream clientStream,
        string[] lines,
        Uri target,
        XRatioSettings effectiveSettings,
        CancellationToken cancellationToken)
    {
        var result = _transformer.Transform(target, effectiveSettings, paused: _isPaused());
        PublishActivity(new ProxyEvent(
            DateTimeOffset.Now, result.Disposition, result.Message, result.Target ?? target, result.InfoHash));
        if (result.Target is null)
        {
            await WriteErrorAsync(clientStream, 403, "Forbidden", cancellationToken);
            return;
        }

        HttpResponseMessage response;
        try
        {
            response = await SendTrackerRequestAsync(
                result.Target,
                lines,
                result.InfoHash is not null,
                cancellationToken);
        }
        catch (Exception exception) when (IsOutboundFailure(exception) &&
                                          !cancellationToken.IsCancellationRequested)
        {
            PublishActivity(new ProxyEvent(
                DateTimeOffset.Now,
                AnnounceDisposition.RejectedInvalid,
                $"Tracker connection failed for {FormatEndpoint(result.Target)}: {DescribeException(exception)}",
                result.Target,
                result.InfoHash));
            await WriteErrorAsync(clientStream, 502, "Tracker connection failed", cancellationToken);
            return;
        }

        using (response)
        {
            byte[] body;
            try
            {
                body = await ReadBoundedBodyAsync(response.Content, cancellationToken);
            }
            catch (Exception exception) when (IsOutboundFailure(exception) &&
                                              !cancellationToken.IsCancellationRequested)
            {
                PublishActivity(new ProxyEvent(
                    DateTimeOffset.Now,
                    AnnounceDisposition.RejectedInvalid,
                    $"Tracker response failed for {FormatEndpoint(result.Target)}: {DescribeException(exception)}",
                    result.Target,
                    result.InfoHash));
                await WriteErrorAsync(clientStream, 502, "Tracker response failed", cancellationToken);
                return;
            }

            if (result.InfoHash is not null)
                _transformer.ObserveTrackerResponse(result.InfoHash, TrackerResponseParser.Parse(body));
            await WriteResponseAsync(clientStream, response, body, cancellationToken);
        }
    }

    private async Task<HttpResponseMessage> SendTrackerRequestAsync(
        Uri target,
        string[] headerLines,
        bool isTrackerRequest,
        CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        // Keep the optional fallback same-origin and limited to tracker
        // announces; TrackerClient performs that validation for us.
        var fallback = isTrackerRequest
            ? TrackerClient.BuildHttpsFallbackUri(target)
            : null;
        var candidate = target;
        for (var candidateIndex = 0; candidateIndex < (fallback is null ? 1 : 2); candidateIndex++)
        {
            for (var attempt = 1; attempt <= MaximumOutboundAttempts; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, candidate);
                CopyRequestHeaders(headerLines, request);
                try
                {
                    return await _httpClient.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead,
                        cancellationToken);
                }
                catch (Exception exception) when (IsOutboundFailure(exception) &&
                                                  !cancellationToken.IsCancellationRequested)
                {
                    lastException = exception;
                    if (attempt < MaximumOutboundAttempts)
                        await Task.Delay(OutboundRetryDelay, cancellationToken);
                }
            }

            candidate = fallback!;
        }

        throw lastException ?? new InvalidOperationException("The tracker request ended without a response.");
    }

    private static bool IsOutboundFailure(Exception exception) =>
        exception is HttpRequestException or SocketException or IOException or
        TaskCanceledException or AuthenticationException or CryptographicException or InvalidDataException;

    private static Uri BuildHttpsTarget(string host, int port, string resource)
    {
        var baseUri = new UriBuilder(Uri.UriSchemeHttps, host, port).Uri;
        return new Uri(baseUri, resource.StartsWith('/') ? resource : "/" + resource);
    }

    private static string SelectLogicalHost(
        string authorityHost,
        string? headerHost,
        string? tlsServerName)
    {
        if (!IPAddress.TryParse(authorityHost, out _))
            return authorityHost;

        if (IsDnsHost(headerHost))
            return headerHost!;
        if (IsDnsHost(tlsServerName))
            return tlsServerName!;
        return authorityHost;
    }

    private static bool IsDnsHost(string? host) =>
        !string.IsNullOrWhiteSpace(host) && Uri.CheckHostName(host) == UriHostNameType.Dns;

    private static bool TryGetHostHeader(
        string[] lines,
        int expectedPort,
        out string? host)
    {
        host = null;
        string? value = null;
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var colon = line.IndexOf(':');
            if (colon <= 0 || !line.AsSpan(0, colon).Trim().Equals("Host", StringComparison.OrdinalIgnoreCase))
                continue;
            if (value is not null)
                return false;
            value = line[(colon + 1)..].Trim();
        }

        return value is null || TryParseHostHeader(value, expectedPort, out host);
    }

    private static bool TryParseHostHeader(
        string value,
        int expectedPort,
        out string host)
    {
        host = string.Empty;
        if (value.Length == 0 || ContainsWhitespace(value.AsSpan()) ||
            !Uri.TryCreate($"https://{value}", UriKind.Absolute, out var parsed) ||
            parsed.UserInfo.Length > 0 ||
            parsed.AbsolutePath != "/" ||
            parsed.Query.Length > 0 ||
            parsed.Fragment.Length > 0 ||
            parsed.Port != expectedPort ||
            Uri.CheckHostName(parsed.Host) == UriHostNameType.Unknown)
        {
            return false;
        }

        host = parsed.Host;
        return true;
    }

    private static bool ContainsWhitespace(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
                return true;
        }

        return false;
    }

    private static string FormatEndpoint(Uri target)
    {
        var host = target.Host.Contains(':')
            ? $"[{target.Host}]"
            : target.Host;
        return target.IsDefaultPort ? host : $"{host}:{target.Port}";
    }

    private static string DescribeException(Exception exception)
    {
        var messages = new List<string>();
        var current = exception;
        for (var depth = 0; current is not null && depth < 4; depth++, current = current.InnerException)
        {
            var message = current.Message.Trim().Replace('\r', ' ').Replace('\n', ' ');
            if (message.Length == 0)
                continue;
            messages.Add($"{current.GetType().Name}: {message}");
        }

        return messages.Count == 0
            ? exception.GetType().Name
            : string.Join(" -> ", messages);
    }

    private XRatioSettings GetEffectiveSettings(TcpClient client)
    {
        var effectiveSettings = _settings();
        if (client.Client.RemoteEndPoint is IPEndPoint remote && !IPAddress.IsLoopback(remote.Address))
            effectiveSettings = effectiveSettings with { OnlyTrackerTraffic = true };
        return effectiveSettings;
    }

    private static bool TryParseAuthority(string authority, out string host, out int port)
    {
        host = string.Empty;
        port = 0;
        string portText;
        if (authority.Length > 0 && authority[0] == '[')
        {
            var closingBracket = authority.IndexOf(']');
            if (closingBracket <= 1 ||
                closingBracket + 1 >= authority.Length ||
                authority[closingBracket + 1] != ':' ||
                !IPAddress.TryParse(authority[1..closingBracket], out var address) ||
                address.AddressFamily != AddressFamily.InterNetworkV6)
                return false;
            host = authority[1..closingBracket];
            portText = authority[(closingBracket + 2)..];
        }
        else
        {
            var colon = authority.LastIndexOf(':');
            if (colon <= 0 || authority.AsSpan(0, colon).Contains(':'))
                return false;
            host = authority[..colon];
            portText = authority[(colon + 1)..];
            if (ContainsInvalidAuthorityHostCharacter(host.AsSpan()) ||
                Uri.CheckHostName(host) == UriHostNameType.Unknown)
                return false;
        }

        if (portText.Length == 0 ||
            !ContainsOnlyDigits(portText.AsSpan()) ||
            !int.TryParse(portText, out port) ||
            port is < 1 or > 65535)
        {
            host = string.Empty;
            port = 0;
            return false;
        }
        return true;
    }

    private static bool ContainsInvalidAuthorityHostCharacter(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character) || character is '/' or '\\' or '@' or '?' or '#')
                return true;
        }

        return false;
    }

    private static bool ContainsOnlyDigits(ReadOnlySpan<char> value)
    {
        foreach (var character in value)
        {
            if (character is < '0' or > '9')
                return false;
        }

        return true;
    }

    private static bool HasHeaderTerminator(ReadOnlySpan<byte> headers) =>
        headers.EndsWith("\r\n\r\n"u8) || headers.EndsWith("\n\n"u8);

    private static async Task TunnelAsync(
        Stream clientStream,
        string host,
        int port,
        CancellationToken cancellationToken)
    {
        using var remote = new TcpClient();
        await remote.ConnectAsync(host, port, cancellationToken);
        await clientStream.WriteAsync(
            ConnectionEstablishedResponse,
            cancellationToken);
        await clientStream.FlushAsync(cancellationToken);
        await using var remoteStream = remote.GetStream();
        var outbound = clientStream.CopyToAsync(remoteStream, TunnelCopyBufferBytes, cancellationToken);
        var inbound = remoteStream.CopyToAsync(clientStream, TunnelCopyBufferBytes, cancellationToken);
        await Task.WhenAny(outbound, inbound);
    }

    private static void CopyRequestHeaders(string[] lines, HttpRequestMessage request)
    {
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var colon = line.IndexOf(':');
            if (colon <= 0)
                continue;
            var name = line.AsSpan(0, colon).Trim();
            if (!IsForwardedRequestHeader(name))
                continue;
            request.Headers.TryAddWithoutValidation(
                name.ToString(),
                line[(colon + 1)..].Trim());
        }
        request.Headers.ConnectionClose = true;
    }

    private static bool IsForwardedRequestHeader(ReadOnlySpan<char> name) =>
        name.Equals("Accept", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("User-Agent", StringComparison.OrdinalIgnoreCase);

    private static async Task<byte[]?> ReadHeadersAsync(
        BufferedReadStream stream,
        CancellationToken cancellationToken)
    {
        var header = ArrayPool<byte>.Shared.Rent(HeaderReadBufferBytes);
        var readBuffer = ArrayPool<byte>.Shared.Rent(HeaderReadBufferBytes);
        var length = 0;
        var state = 0;
        var previousWasLineFeed = false;
        try
        {
            while (true)
            {
                var read = await stream.ReadAsync(
                    readBuffer.AsMemory(0, HeaderReadBufferBytes),
                    cancellationToken);
                if (read == 0)
                    return CopyHeader(header, length);

                for (var i = 0; i < read; i++)
                {
                    if (length >= MaximumHeaderBytes)
                        return null;

                    if (length == header.Length)
                        header = GrowBuffer(header, length);
                    var current = readBuffer[i];
                    header[length++] = current;

                    var complete = current == '\n' &&
                                   (previousWasLineFeed || state == 3);
                    if (complete)
                    {
                        var remaining = read - i - 1;
                        if (remaining > 0)
                            stream.PushBack(readBuffer.AsSpan(i + 1, remaining));
                        return CopyHeader(header, length);
                    }

                    if (current == '\n')
                    {
                        previousWasLineFeed = true;
                        state = state == 1 ? 2 : 0;
                    }
                    else
                    {
                        previousWasLineFeed = false;
                        state = current == '\r'
                            ? state == 2 ? 3 : 1
                            : 0;
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(readBuffer);
            ArrayPool<byte>.Shared.Return(header);
        }
    }

    private static byte[]? CopyHeader(byte[] buffer, int length) =>
        length == 0 ? [] : buffer.AsSpan(0, length).ToArray();

    private static byte[] GrowBuffer(byte[] buffer, int length)
    {
        var requested = Math.Min(MaximumHeaderBytes, buffer.Length * 2);
        var replacement = ArrayPool<byte>.Shared.Rent(requested);
        buffer.AsSpan(0, length).CopyTo(replacement);
        ArrayPool<byte>.Shared.Return(buffer);
        return replacement;
    }

    private async Task<byte[]?> ReadHeadersWithTimeoutAsync(
        BufferedReadStream stream,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_headerReadTimeout);
        return await ReadHeadersAsync(stream, timeout.Token);
    }

    private static async Task<byte[]> ReadBoundedBodyAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        if (content.Headers.ContentLength > MaximumTrackerResponseBytes)
            throw new InvalidDataException("Tracker response exceeds 4 MiB.");
        await using var source = await content.ReadAsStreamAsync(cancellationToken);
        var contentLength = content.Headers.ContentLength;
        var capacity = contentLength is > 0 and <= MaximumTrackerResponseBytes
            ? (int)contentLength.Value
            : 0;
        using var destination = new MemoryStream(capacity);
        var buffer = ArrayPool<byte>.Shared.Rent(16 * 1024);
        var length = 0;
        try
        {
            while (true)
            {
                var read = await source.ReadAsync(buffer.AsMemory(0, 16 * 1024), cancellationToken);
                if (read == 0)
                    return destination.ToArray();
                if (length > MaximumTrackerResponseBytes - read)
                    throw new InvalidDataException("Tracker response exceeds 4 MiB.");
                destination.Write(buffer, 0, read);
                length += read;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static async Task WriteResponseAsync(
        Stream destination,
        HttpResponseMessage response,
        byte[] body,
        CancellationToken cancellationToken)
    {
        var builder = new StringBuilder();
        builder.Append("HTTP/1.1 ").Append((int)response.StatusCode).Append(' ')
            .Append(response.ReasonPhrase).Append("\r\n");
        AppendHeaders(builder, response.Headers);
        AppendHeaders(builder, response.Content.Headers, excluded: ExcludedResponseHeaders);
        builder.Append("Content-Length: ").Append(body.Length).Append("\r\nConnection: close\r\n\r\n");
        await destination.WriteAsync(Encoding.Latin1.GetBytes(builder.ToString()), cancellationToken);
        await destination.WriteAsync(body, cancellationToken);
    }

    private static void AppendHeaders(
        StringBuilder builder,
        HttpHeaders headers,
        IReadOnlySet<string>? excluded = null)
    {
        foreach (var header in headers)
        {
            if (excluded?.Contains(header.Key) == true ||
                header.Key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase) ||
                header.Key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
                continue;
            foreach (var value in header.Value)
                builder.Append(header.Key).Append(": ").Append(value).Append("\r\n");
        }
    }

    private void PublishActivity(ProxyEvent activity)
    {
        if (_debugLogger is not null)
        {
            try
            {
                if (_isDebugLogging())
                {
                    var target = activity.Target is null ? string.Empty : $" target={FormatTargetForLog(activity.Target)}";
                    _debugLogger.Write(
                        ProxyDebugRedactor.RedactSensitive(
                            $"{activity.Disposition}: {activity.Message}{target}"));
                }
            }
            catch (Exception)
            {
                // A diagnostic sink must never interfere with proxy traffic.
            }
        }

        Activity?.Invoke(this, activity);
    }

    private static string FormatTargetForLog(Uri target)
    {
        var authority = target.IsDefaultPort ? target.Host : $"{target.Host}:{target.Port}";
        var namesOnly = target.Query.Length == 0
            ? target.AbsolutePath
            : target.AbsolutePath + "?" + string.Join('&', target.Query[1..]
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('=', 2)[0] + "=<redacted>"));
        return $"{target.Scheme}://{authority}{namesOnly}";
    }

    private static async Task WriteErrorAsync(
        Stream stream,
        int status,
        string reason,
        CancellationToken cancellationToken)
    {
        var safeReason = reason.Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal);
        var body = Encoding.UTF8.GetBytes(safeReason);
        var header = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 {status} {safeReason}\r\nContent-Type: text/plain; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(header, cancellationToken);
        await stream.WriteAsync(body, cancellationToken);
    }

    /// <summary>
    /// Keeps bytes read past the end of a header available to the next protocol
    /// layer. This lets the header scanner use normal-sized reads without
    /// consuming the first TLS record after CONNECT.
    /// </summary>
    private sealed class BufferedReadStream : Stream
    {
        private readonly Stream _inner;
        private byte[]? _pending;
        private int _pendingOffset;
        private int _pendingCount;
        private bool _disposed;

        public BufferedReadStream(Stream inner)
        {
            _inner = inner;
        }

        public void PushBack(ReadOnlySpan<byte> bytes)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (bytes.IsEmpty)
                return;

            var required = checked(_pendingCount + bytes.Length);
            if (_pending is null || _pending.Length < required)
            {
                var replacement = ArrayPool<byte>.Shared.Rent(
                    Math.Max(HeaderReadBufferBytes, required));
                bytes.CopyTo(replacement);
                if (_pendingCount > 0)
                    _pending.AsSpan(_pendingOffset, _pendingCount)
                        .CopyTo(replacement.AsSpan(bytes.Length));
                ReturnPendingBuffer();
                _pending = replacement;
                _pendingOffset = 0;
                _pendingCount = required;
                return;
            }

            if (_pendingOffset > 0 &&
                _pendingOffset + _pendingCount + bytes.Length <= _pending.Length)
            {
                _pending.AsSpan(_pendingOffset, _pendingCount)
                    .CopyTo(_pending.AsSpan(_pendingOffset + bytes.Length));
                bytes.CopyTo(_pending.AsSpan(_pendingOffset));
                _pendingCount = required;
                return;
            }

            // The common path has no pending bytes. If a caller pushes back
            // while bytes are already buffered, compact into the beginning.
            _pending.AsSpan(_pendingOffset, _pendingCount)
                .CopyTo(_pending.AsSpan(bytes.Length));
            bytes.CopyTo(_pending);
            _pendingOffset = 0;
            _pendingCount = required;
        }

        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer,
            CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_pendingCount == 0)
                return await _inner.ReadAsync(buffer, cancellationToken);

            var count = Math.Min(buffer.Length, _pendingCount);
            _pending!.AsMemory(_pendingOffset, count).CopyTo(buffer);
            _pendingOffset += count;
            _pendingCount -= count;
            if (_pendingCount == 0)
                ReturnPendingBuffer();
            return count;
        }

        public override int Read(Span<byte> buffer)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_pendingCount == 0)
                return _inner.Read(buffer);

            var count = Math.Min(buffer.Length, _pendingCount);
            _pending.AsSpan(_pendingOffset, count).CopyTo(buffer);
            _pendingOffset += count;
            _pendingCount -= count;
            if (_pendingCount == 0)
                ReturnPendingBuffer();
            return count;
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            Read(buffer.AsSpan(offset, count));

        public override ValueTask WriteAsync(
            ReadOnlyMemory<byte> buffer,
            CancellationToken cancellationToken = default) =>
            _inner.WriteAsync(buffer, cancellationToken);

        public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);

        public override void Write(byte[] buffer, int offset, int count) =>
            _inner.Write(buffer, offset, count);

        public override void Flush() => _inner.Flush();

        public override Task FlushAsync(CancellationToken cancellationToken) =>
            _inner.FlushAsync(cancellationToken);

        public override int ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];
            return Read(buffer) == 0 ? -1 : buffer[0];
        }

        public override void WriteByte(byte value) => _inner.WriteByte(value);

        public override long Seek(long offset, SeekOrigin origin) =>
            _inner.Seek(offset, origin);

        public override void SetLength(long value) => _inner.SetLength(value);

        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public override long Length => _inner.Length;

        public override bool CanRead => !_disposed && _inner.CanRead;

        public override bool CanSeek => !_disposed && _inner.CanSeek;

        public override bool CanWrite => !_disposed && _inner.CanWrite;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                ReturnPendingBuffer();
            }
            base.Dispose(disposing);
        }

        private void ReturnPendingBuffer()
        {
            if (_pending is null)
                return;
            ArrayPool<byte>.Shared.Return(_pending);
            _pending = null;
            _pendingOffset = 0;
            _pendingCount = 0;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _httpClient.Dispose();
        _connectionLimit.Dispose();
    }
}

