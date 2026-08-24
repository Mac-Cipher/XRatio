using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using XRatio.Core.Announcements;
using XRatio.Core.Configuration;

namespace XRatio.Proxy.Tests;

public sealed class HttpProxyServerTests
{
    [Fact]
    public async Task Proxy_RewritesAndForwardsHttpAnnounce()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var tracker = new TcpListener(IPAddress.Loopback, 0);
        tracker.Start();
        var trackerPort = ((IPEndPoint)tracker.LocalEndpoint).Port;
        var proxyPort = ReservePort();
        var observedRequest = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var trackerTask = RunTrackerAsync(tracker, observedRequest, timeout.Token);
        var debugLog = new CapturingDebugLogger();

        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings
            {
                ListenPort = proxyPort,
                ReportDownloadAsZero = true,
                PretendToSeed = true
            },
            debugLogger: debugLog,
            isDebugLogging: () => true);
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        var absoluteTarget =
            $"http://127.0.0.1:{trackerPort}/announce?info_hash=abc&passkey=secret&downloaded=50&uploaded=20&left=700";
        var request = Encoding.ASCII.GetBytes(
            $"GET {absoluteTarget} HTTP/1.1\r\nHost: 127.0.0.1:{trackerPort}\r\nConnection: close\r\n\r\n");
        await stream.WriteAsync(request, timeout.Token);
        using var responseBuffer = new MemoryStream();
        await stream.CopyToAsync(responseBuffer, timeout.Token);

        var trackerRequest = await observedRequest.Task.WaitAsync(timeout.Token);
        Assert.Contains("downloaded=0", trackerRequest, StringComparison.Ordinal);
        Assert.Contains("uploaded=20", trackerRequest, StringComparison.Ordinal);
        Assert.Contains("left=0", trackerRequest, StringComparison.Ordinal);
        Assert.StartsWith("HTTP/1.1 200", Encoding.ASCII.GetString(responseBuffer.ToArray()), StringComparison.Ordinal);
        var announceLog = Assert.Single(
            debugLog.Messages,
            message => message.Contains("Announce statistics rewritten", StringComparison.Ordinal));
        Assert.Contains("passkey=<redacted>", announceLog, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", announceLog, StringComparison.Ordinal);

        await proxy.StopAsync();
        await trackerTask;
    }

    [Fact]
    public async Task Proxy_DoesNotWriteDebugLogWhenDisabled()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var debugLog = new CapturingDebugLogger();
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort },
            debugLogger: debugLog,
            isDebugLogging: () => false);

        await proxy.StartAsync(timeout.Token);
        Assert.Empty(debugLog.Messages);
        await proxy.StopAsync();
    }

    [Fact]
    public async Task Proxy_RetriesTransientTrackerFailure()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var handler = new FlakyTrackerHandler();
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort },
            outboundHandler: handler);
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(
            Encoding.ASCII.GetBytes(
                "GET http://tracker.test/announce?info_hash=abc&downloaded=1&uploaded=1&left=1 HTTP/1.1\r\n" +
                "Host: tracker.test\r\nConnection: close\r\n\r\n"),
            timeout.Token);
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);

        Assert.Equal(2, handler.Attempts);
        Assert.StartsWith("HTTP/1.1 200", Encoding.ASCII.GetString(response.ToArray()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Proxy_FallsBackToHttpsWhenHttpTrackerPortIsUnavailable()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var handler = new HttpToHttpsFallbackHandler();
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort },
            outboundHandler: handler);
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(
            Encoding.ASCII.GetBytes(
                "GET http://tracker.test:2710/announce?info_hash=abc&downloaded=1&uploaded=1&left=1 HTTP/1.1\r\n" +
                "Host: tracker.test:2710\r\nConnection: close\r\n\r\n"),
            timeout.Token);
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);

        Assert.Equal(3, handler.Requests.Count);
        Assert.All(handler.Requests.Take(2), request =>
            Assert.Equal("http", request.Scheme, ignoreCase: true));
        Assert.Equal("https", handler.Requests[2].Scheme, ignoreCase: true);
        Assert.Equal(443, handler.Requests[2].Port);
        Assert.StartsWith("HTTP/1.1 200", Encoding.ASCII.GetString(response.ToArray()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Proxy_ReturnsBadGatewayWithOutboundFailureDetails()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var handler = new FailedTrackerHandler();
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort },
            outboundHandler: handler);
        var activities = new List<ProxyEvent>();
        proxy.Activity += (_, activity) => activities.Add(activity);
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(
            Encoding.ASCII.GetBytes(
                "GET http://tracker.test/announce?info_hash=abc&downloaded=1&uploaded=1&left=1 HTTP/1.1\r\n" +
                "Host: tracker.test\r\nConnection: close\r\n\r\n"),
            timeout.Token);
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);

        var failure = Assert.Single(
            activities,
            activity => activity.Disposition == AnnounceDisposition.RejectedInvalid &&
                        activity.Message.StartsWith("Tracker connection failed", StringComparison.Ordinal));
        Assert.Equal(2, handler.Attempts);
        Assert.Equal("tracker.test", failure.Target!.Host);
        Assert.Contains("certificate chain unavailable", failure.Message, StringComparison.Ordinal);
        Assert.StartsWith("HTTP/1.1 502", Encoding.ASCII.GetString(response.ToArray()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Proxy_RejectsOversizedChunkedTrackerResponse()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort },
            outboundHandler: new OversizedTrackerHandler());
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(Encoding.ASCII.GetBytes(
            "GET http://tracker.test/announce?info_hash=a&downloaded=1&uploaded=1&left=1 HTTP/1.1\r\n" +
            "Host: tracker.test\r\nConnection: close\r\n\r\n"), timeout.Token);
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);

        Assert.StartsWith("HTTP/1.1 502", Encoding.ASCII.GetString(response.ToArray()), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Proxy_ForwardsOnlyExplicitlyAllowedRequestHeaders()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var handler = new CapturingHeadersHandler();
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort },
            outboundHandler: handler);
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(Encoding.ASCII.GetBytes(
            "GET http://tracker.test/announce?info_hash=a&downloaded=1&uploaded=1&left=1 HTTP/1.1\r\n" +
            "Host: tracker.test\r\nUser-Agent: XRatio-Test\r\nAuthorization: Bearer secret\r\n" +
            "Proxy-Authorization: Basic secret\r\nCookie: session=secret\r\nX-Custom: secret\r\nConnection: close\r\n\r\n"), timeout.Token);
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);

        Assert.Contains("User-Agent", handler.HeaderNames);
        Assert.DoesNotContain("Authorization", handler.HeaderNames);
        Assert.DoesNotContain("Proxy-Authorization", handler.HeaderNames);
        Assert.DoesNotContain("Cookie", handler.HeaderNames);
        Assert.DoesNotContain("X-Custom", handler.HeaderNames);
    }

    [Fact]
    public async Task Proxy_FailsClosedForConnectUntilCertificateAuthorityIsImplemented()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort });
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(
            Encoding.ASCII.GetBytes("CONNECT tracker.test:443 HTTP/1.1\r\nHost: tracker.test:443\r\n\r\n"),
            timeout.Token);
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);

        Assert.StartsWith("HTTP/1.1 501", Encoding.ASCII.GetString(response.ToArray()), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("CONNECT tracker.test HTTP/1.1\r\nHost: tracker.test\r\n\r\n")]
    [InlineData("CONNECT :443 HTTP/1.1\r\nHost: :443\r\n\r\n")]
    [InlineData("CONNECT tracker.test:not-a-port HTTP/1.1\r\nHost: tracker.test\r\n\r\n")]
    [InlineData("CONNECT user@tracker.test:443 HTTP/1.1\r\nHost: tracker.test\r\n\r\n")]
    public async Task Proxy_RejectsMalformedConnectAuthority(string request)
    {
        var response = await SendRawRequestAsync(request);

        Assert.StartsWith("HTTP/1.1 400", response, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Proxy_RejectsConnectWhenHeadersEndBeforeTerminator()
    {
        var response = await SendRawRequestAsync(
            "CONNECT tracker.test:443 HTTP/1.1\r\nHost: tracker.test:443\r\n",
            endRequestStream: true);

        Assert.StartsWith("HTTP/1.1 400", response, StringComparison.Ordinal);
        Assert.Contains("Incomplete request headers", response, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StopAsync_CancelsAndAwaitsIncompleteClient()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort });
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await client.GetStream().WriteAsync(
            Encoding.ASCII.GetBytes("GET http://tracker.test/announce?info_hash=abc"),
            timeout.Token);

        await proxy.StopAsync().WaitAsync(timeout.Token);

        Assert.False(proxy.IsRunning);
    }

    [Fact]
    public async Task StartAsync_WhenPortIsOccupied_DisposeDoesNotMaskTheSocketFailure()
    {
        var occupiedPort = new TcpListener(IPAddress.Loopback, 0);
        occupiedPort.Start();
        var port = ((IPEndPoint)occupiedPort.LocalEndpoint).Port;
        var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = port });

        try
        {
            await Assert.ThrowsAsync<SocketException>(() => proxy.StartAsync());
            var disposeFailure = await Record.ExceptionAsync(
                async () => await proxy.DisposeAsync());

            Assert.Null(disposeFailure);
            Assert.False(proxy.IsRunning);
        }
        finally
        {
            occupiedPort.Stop();
        }
    }

    [Fact]
    public async Task Proxy_ClosesSlowHeaderConnectionAfterDeadline()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort },
            headerReadTimeout: TimeSpan.FromMilliseconds(100));
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(Encoding.ASCII.GetBytes("GET http://tracker.test/"), timeout.Token);
        var buffer = new byte[1];

        var read = await stream.ReadAsync(buffer, timeout.Token);

        Assert.Equal(0, read);
    }

    private static async Task<string> SendRawRequestAsync(string request, bool endRequestStream = false)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var proxyPort = ReservePort();
        await using var proxy = new HttpProxyServer(
            new AnnounceTransformer(),
            () => new XRatioSettings { ListenPort = proxyPort });
        await proxy.StartAsync(timeout.Token);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, proxyPort, timeout.Token);
        await using var stream = client.GetStream();
        await stream.WriteAsync(Encoding.ASCII.GetBytes(request), timeout.Token);
        if (endRequestStream)
            client.Client.Shutdown(SocketShutdown.Send);
        using var response = new MemoryStream();
        await stream.CopyToAsync(response, timeout.Token);
        return Encoding.ASCII.GetString(response.ToArray());
    }

    private static int ReservePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private sealed class CapturingDebugLogger : IProxyDebugLogger
    {
        public List<string> Messages { get; } = [];

        public void Write(string message) => Messages.Add(message);
    }

    private sealed class FlakyTrackerHandler : HttpMessageHandler
    {
        public int Attempts => Volatile.Read(ref _attempts);

        private int _attempts;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _attempts) == 1)
                throw new HttpRequestException(
                    "transient TLS handshake failure",
                    new AuthenticationException("remote party reset handshake"));

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(
                    Encoding.ASCII.GetBytes("d8:completei1e10:incompletei1e8:intervali60ee"))
            });
        }
    }

    private sealed class FailedTrackerHandler : HttpMessageHandler
    {
        public int Attempts => Volatile.Read(ref _attempts);

        private int _attempts;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _attempts);
            throw new HttpRequestException(
                "The SSL connection could not be established",
                new AuthenticationException("certificate chain unavailable"));
        }
    }

    private sealed class OversizedTrackerHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[(4 * 1024 * 1024) + 1])
            });
    }

    private sealed class CapturingHeadersHandler : HttpMessageHandler
    {
        public IReadOnlySet<string> HeaderNames { get; private set; } = new HashSet<string>();

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HeaderNames = request.Headers.Select(header => header.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Encoding.ASCII.GetBytes("d8:intervali60ee"))
            });
        }
    }

    private sealed class HttpToHttpsFallbackHandler : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var target = request.RequestUri ?? throw new InvalidOperationException("Request URI is missing.");
            Requests.Add(target);
            if (target.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                throw new HttpRequestException("tracker HTTP port unavailable");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(
                    Encoding.ASCII.GetBytes("d8:completei1e10:incompletei2e8:intervali60ee"))
            });
        }
    }

    private static async Task RunTrackerAsync(
        TcpListener tracker,
        TaskCompletionSource<string> observed,
        CancellationToken cancellationToken)
    {
        try
        {
            using var client = await tracker.AcceptTcpClientAsync(cancellationToken);
            await using var stream = client.GetStream();
            var buffer = new byte[4096];
            var count = await stream.ReadAsync(buffer, cancellationToken);
            observed.TrySetResult(Encoding.Latin1.GetString(buffer, 0, count));
            var body = Encoding.ASCII.GetBytes("d8:completei1e10:incompletei6e8:intervali1800ee");
            var header = Encoding.ASCII.GetBytes(
                $"HTTP/1.1 200 OK\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(header, cancellationToken);
            await stream.WriteAsync(body, cancellationToken);
        }
        finally
        {
            tracker.Stop();
        }
    }
}

