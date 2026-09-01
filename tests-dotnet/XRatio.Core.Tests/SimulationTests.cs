using XRatio.Core.Simulation;
using XRatio.Core.Torrents;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;

namespace XRatio.Core.Tests;

public sealed class SimulationTests
{
    [Fact]
    public void Snapshot_ComputesDownloadCompletionPercent()
    {
        var snapshot = new SimulationSnapshot(
            Guid.NewGuid(), "torrent", "hash", "https://tracker.test", "client",
            SimulationState.Running, 0, 250, 750, 0, 0, 0, 0,
            TimeSpan.Zero, null, null)
        {
            AccountName = "test-account"
        };

        Assert.Equal(25, snapshot.CompletionPercent);
        Assert.Equal("tracker.test", snapshot.TrackerName);
        Assert.Equal("test-account", snapshot.AccountName);
    }

    [Fact]
    public void Defaults_MatchRatioMasterReference()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024, 1, true, [tracker]),
            Tracker = tracker,
            AccountName = "  test-account  "
        };

        Assert.Equal("qbittorrent-5.2", options.ClientProfileId);
        Assert.Equal(10_000 * 1024L, options.UploadBytesPerSecond);
        Assert.Equal(2_500 * 1024L, options.DownloadBytesPerSecond);
        Assert.True(options.RandomUploadEnabled);
        Assert.Equal(5_000 * 1024L, options.RandomUploadMinimumBytesPerSecond);
        Assert.Equal(15_000 * 1024L, options.RandomUploadMaximumBytesPerSecond);
        Assert.True(options.RandomDownloadEnabled);
        Assert.Equal(1_000 * 1024L, options.RandomDownloadMinimumBytesPerSecond);
        Assert.Equal(5_000 * 1024L, options.RandomDownloadMaximumBytesPerSecond);
        Assert.Equal(0, options.InitialCompletedPercent);
        Assert.Equal("  test-account  ", options.AccountName);

        var profile = ClientProfileCatalog.Get(options.ClientProfileId);
        Assert.Equal("qBittorrent 5.2.3", profile.DisplayName);
        Assert.Equal("qBittorrent/5.2.3", profile.UserAgent);
    }

    [Fact]
    public void Counters_AdvanceWithoutExceedingTorrentSize()
    {
        var counters = new SimulationCounters(1000, 50);

        var completed = counters.Advance(TimeSpan.FromSeconds(2), 100, 400);

        Assert.True(completed);
        Assert.Equal(200, counters.Uploaded);
        Assert.Equal(1000, counters.Downloaded);
        Assert.Equal(0, counters.Left);
    }

    [Fact]
    public void ClientCatalog_ProvidesRatioMasterStyleFamiliesAndValidPeerIds()
    {
        Assert.Equal(18, ClientProfileCatalog.All.Count);
        Assert.All(ClientProfileCatalog.All, profile => Assert.Equal(20, profile.CreatePeerId().Length));
    }

    [Fact]
    public void TrackerUri_ContainsBinaryHashCountersAndEvent()
    {
        var profile = ClientProfileCatalog.Get("qbittorrent-5.1");
        var uri = TrackerClient.BuildUri(new TrackerAnnounce(
            new Uri("https://tracker.test/announce"),
            "00112233445566778899AABBCCDDEEFF00112233",
            "-qB5100-123456789012",
            6881,
            123,
            45,
            67,
            200,
            "deadbeef",
            TrackerEvent.Started,
            profile,
            new SimulationProxyOptions()));

        Assert.Contains("info_hash=%00%11%22", uri.Query, StringComparison.Ordinal);
        Assert.Contains("uploaded=123", uri.Query, StringComparison.Ordinal);
        Assert.Contains("event=started", uri.Query, StringComparison.Ordinal);
    }

    [Fact]
    public void TrackerUri_BuildsHttpsFallbackForNonStandardHttpPort()
    {
        var fallback = TrackerClient.BuildHttpsFallbackUri(
            new Uri("http://tracker.test:2710/passkey/announce?info_hash=abc"));

        Assert.Equal(
            "https://tracker.test/passkey/announce?info_hash=abc",
            fallback?.ToString());
        Assert.Null(TrackerClient.BuildHttpsFallbackUri(new Uri("http://tracker.test/announce")));
        Assert.Null(TrackerClient.BuildHttpsFallbackUri(new Uri("https://tracker.test:2710/announce")));
    }

    [Fact]
    public async Task Session_SendsStartedUpdateAndStoppedLifecycle()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var fake = new RecordingTrackerClient();
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024, 1, true, [tracker]),
            Tracker = tracker,
            AccountName = "test-account",
            UploadBytesPerSecond = 0,
            DownloadBytesPerSecond = 0
        };
        await using var session = new SimulationSession(options, fake);
        Assert.Equal("test-account", session.Snapshot.AccountName);
        Assert.Equal("tracker.test", session.Snapshot.TrackerName);

        await session.StartAsync();
        await session.UpdateNowAsync();
        await session.StopAsync();

        Assert.Equal([TrackerEvent.Started, TrackerEvent.None, TrackerEvent.Stopped], fake.Events);
        Assert.Equal(SimulationState.Stopped, session.State);
    }

    [Fact]
    public async Task Sessions_UseSharedTickSourceAndUnsubscribeOnStop()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var source = new ManualTickSource();
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024 * 1024, 1, true, [tracker]),
            Tracker = tracker,
            UploadBytesPerSecond = 100,
            DownloadBytesPerSecond = 0,
            RandomUploadEnabled = false,
            RandomDownloadEnabled = false
        };
        await using var first = new SimulationSession(options, new RecordingTrackerClient(), tickSource: source);
        await using var second = new SimulationSession(options, new RecordingTrackerClient(), tickSource: source);
        var firstAdvanced = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondAdvanced = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        first.Updated += (_, snapshot) =>
        {
            if (snapshot.Uploaded > 0)
                firstAdvanced.TrySetResult();
        };
        second.Updated += (_, snapshot) =>
        {
            if (snapshot.Uploaded > 0)
                secondAdvanced.TrySetResult();
        };

        await first.StartAsync();
        await second.StartAsync();
        Assert.Equal(2, source.SubscriptionCount);

        source.Tick();
        await firstAdvanced.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await secondAdvanced.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Assert.Equal(100, first.Snapshot.Uploaded);
        Assert.Equal(100, second.Snapshot.Uploaded);

        await first.StopAsync();
        Assert.Equal(1, source.SubscriptionCount);
        await second.StopAsync();
        Assert.Equal(0, source.SubscriptionCount);
    }

    [Fact]
    public async Task TickHub_StopsTimerWhenLastSubscriberLeaves()
    {
        await using var hub = new SimulationTickHub(TimeSpan.FromMilliseconds(10));
        var first = await hub.SubscribeAsync();
        var second = await hub.SubscribeAsync();

        Assert.True(await first.WaitForNextTickAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(2)));
        Assert.True(await second.WaitForNextTickAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(2)));

        await first.DisposeAsync();
        Assert.True(await second.WaitForNextTickAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(2)));
        await second.DisposeAsync();

        await hub.DisposeAsync();
        await hub.DisposeAsync();
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
            await hub.SubscribeAsync().AsTask());
    }

    [Fact]
    public async Task Session_AddsIndependentAbsoluteRandomSpeedsAndKeepsZeroDownload()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024 * 1024, 1, true, [tracker]),
            Tracker = tracker,
            UploadBytesPerSecond = 100,
            DownloadBytesPerSecond = 0,
            RandomUploadEnabled = true,
            RandomUploadMinimumBytesPerSecond = 25,
            RandomUploadMaximumBytesPerSecond = 25,
            RandomDownloadEnabled = true,
            RandomDownloadMinimumBytesPerSecond = 50,
            RandomDownloadMaximumBytesPerSecond = 50
        };
        await using var session = new SimulationSession(options, new RecordingTrackerClient(), new Random(7));

        await session.StartAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(4));
        while (session.Snapshot.UploadRate != 125)
            await Task.Delay(25, timeout.Token);

        Assert.Equal(125, session.Snapshot.UploadRate);
        Assert.Equal(0, session.Snapshot.DownloadRate);
        await session.StopAsync();
    }

    [Fact]
    public async Task Session_AutomaticallyStopsAfterMaximumRuntime()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var fake = new RecordingTrackerClient();
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024, 1, true, [tracker]),
            Tracker = tracker,
            UploadBytesPerSecond = 0,
            DownloadBytesPerSecond = 0,
            MaximumRuntime = TimeSpan.FromSeconds(1)
        };
        await using var session = new SimulationSession(options, fake);
        var stopped = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        session.Updated += (_, snapshot) =>
        {
            if (snapshot.State == SimulationState.Stopped && fake.Events.Contains(TrackerEvent.Stopped))
                stopped.TrySetResult();
        };

        await session.StartAsync();
        await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(SimulationState.Stopped, session.State);
        Assert.Equal([TrackerEvent.Started, TrackerEvent.Stopped], fake.Events);
    }

    [Fact]
    public void Options_RejectReversedRandomSpeedRange()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024, 1, true, [tracker]),
            Tracker = tracker,
            RandomUploadMinimumBytesPerSecond = 20,
            RandomUploadMaximumBytesPerSecond = 10
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => options.Validate());
    }

    [Fact]
    public void Options_RejectNonFiniteValuesAndProxyCredentialsInAddress()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024, 1, true, [tracker]);

        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationOptions
        {
            Torrent = torrent,
            Tracker = tracker,
            InitialCompletedPercent = double.NaN
        }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationOptions
        {
            Torrent = torrent,
            Tracker = tracker,
            MaximumRatio = double.PositiveInfinity
        }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationOptions
        {
            Torrent = torrent,
            Tracker = tracker,
            AnnounceIntervalSeconds = 29
        }.Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimulationOptions
        {
            Torrent = torrent,
            Tracker = tracker,
            AnnounceIntervalSeconds = 86401
        }.Validate());
        Assert.Throws<ArgumentException>(() => new SimulationOptions
        {
            Torrent = torrent,
            Tracker = tracker,
            Proxy = new SimulationProxyOptions { Address = new Uri("http://user:password@proxy.test:8080") }
        }.Validate());
    }

    [Fact]
    public async Task ConcurrentStop_WaitsForPendingStartAndLeavesSessionStopped()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var fake = new BlockingStartTrackerClient();
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024, 1, true, [tracker]),
            Tracker = tracker,
            UploadBytesPerSecond = 0,
            DownloadBytesPerSecond = 0
        };
        await using var session = new SimulationSession(options, fake);

        var start = session.StartAsync();
        await fake.StartObserved.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var stop = session.StopAsync();
        fake.ReleaseStart.TrySetResult();
        await Task.WhenAll(start, stop).WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(SimulationState.Stopped, session.State);
        Assert.Equal([TrackerEvent.Started, TrackerEvent.Stopped], fake.Events);
    }

    [Fact]
    public async Task CancelledStart_RestoresStoppedState()
    {
        var tracker = new Uri("https://tracker.test/announce");
        var options = new SimulationOptions
        {
            Torrent = new TorrentMetadata("demo.torrent", "Demo", new string('A', 40), 1024, 1, true, [tracker]),
            Tracker = tracker
        };
        await using var session = new SimulationSession(options, new CancellationTrackerClient());
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => session.StartAsync(cancellation.Token));

        Assert.Equal(SimulationState.Stopped, session.State);
    }

    [Fact]
    public async Task SessionStore_PreservesRatioMasterSpeedRanges()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"xratio-sim-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var store = new SimulationSessionStore(directory);
            var saved = new SavedSimulationSession
            {
                TorrentPath = "demo.torrent",
                Tracker = "https://tracker.test/announce",
                AccountName = "test-account",
                RandomUploadEnabled = true,
                RandomUploadMinimumBytesPerSecond = 1024,
                RandomUploadMaximumBytesPerSecond = 10 * 1024,
                RandomDownloadEnabled = true,
                RandomDownloadMinimumBytesPerSecond = 5 * 1024,
                RandomDownloadMaximumBytesPerSecond = 12_500 * 1024,
                AnnounceIntervalSeconds = 900
            };

            await store.SaveAsync([saved, saved]);
            var loaded = Assert.Single(await store.LoadAsync());

            Assert.True(loaded.RandomUploadEnabled);
            Assert.Equal(1024, loaded.RandomUploadMinimumBytesPerSecond);
            Assert.Equal(10 * 1024, loaded.RandomUploadMaximumBytesPerSecond);
            Assert.True(loaded.RandomDownloadEnabled);
            Assert.Equal(5 * 1024, loaded.RandomDownloadMinimumBytesPerSecond);
            Assert.Equal(12_500 * 1024, loaded.RandomDownloadMaximumBytesPerSecond);
            Assert.Equal(900, loaded.AnnounceIntervalSeconds);
            Assert.Equal("test-account", loaded.AccountName);
        }
        finally
        {
            var settingsPath = Path.Combine(directory, "simulations.json");
            if (File.Exists(settingsPath))
                File.Delete(settingsPath);
            Directory.Delete(directory);
        }
    }

    [Fact]
    public async Task SessionStore_IgnoresFilesWithTooManySessions()
    {
        var directory = Path.Combine(
            Environment.CurrentDirectory,
            "work",
            "tmp",
            "XRatio.SimulationTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var item = "{\"torrentPath\":\"demo.torrent\",\"tracker\":\"https://tracker.test/announce\"}";
            await File.WriteAllTextAsync(
                Path.Combine(directory, "simulations.json"),
                "[" + string.Join(',', Enumerable.Repeat(item, 129)) + "]");

            var loaded = await new SimulationSessionStore(directory).LoadAsync();

            Assert.Empty(loaded);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SessionStore_IgnoresCorruptBackup()
    {
        var directory = Path.Combine(
            Environment.CurrentDirectory,
            "work",
            "tmp",
            "XRatio.SimulationTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory, "simulations.json.bak"),
                "{not-json");

            var loaded = await new SimulationSessionStore(directory).LoadAsync();

            Assert.Empty(loaded);
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task TrackerClient_SendsAnnounceToLocalHttpTracker()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var observed = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var server = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync(timeout.Token);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, false, leaveOpen: true);
            var requestLine = await reader.ReadLineAsync(timeout.Token) ?? string.Empty;
            while (!string.IsNullOrEmpty(await reader.ReadLineAsync(timeout.Token)))
            {
            }
            observed.SetResult(requestLine);
            var body = Encoding.ASCII.GetBytes("d8:completei2e10:incompletei3e8:intervali60ee");
            var headers = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\nContent-Length: {body.Length}\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(headers, timeout.Token);
            await stream.WriteAsync(body, timeout.Token);
        }, timeout.Token);
        try
        {
            var tracker = new Uri($"http://127.0.0.1:{port}/announce");
            var result = await new TrackerClient().AnnounceAsync(new TrackerAnnounce(
                tracker,
                "00112233445566778899AABBCCDDEEFF00112233",
                "-qB5100-123456789012",
                6881,
                123,
                45,
                67,
                200,
                "deadbeef",
                TrackerEvent.Started,
                ClientProfileCatalog.Get("qbittorrent-5.1"),
                new SimulationProxyOptions()), timeout.Token);

            Assert.Contains("GET /announce?", await observed.Task.WaitAsync(timeout.Token), StringComparison.Ordinal);
            Assert.Equal(60, result.IntervalSeconds);
            Assert.Equal(2, result.Seeders);
            Assert.Equal(3, result.Leechers);
            await server;
        }
        finally
        {
            listener.Stop();
        }
    }

    [Fact]
    public async Task TrackerClient_RejectsRedirectToAnotherOrigin()
    {
        var source = new TcpListener(IPAddress.Loopback, 0);
        var destination = new TcpListener(IPAddress.Loopback, 0);
        source.Start();
        destination.Start();
        var sourcePort = ((IPEndPoint)source.LocalEndpoint).Port;
        var destinationPort = ((IPEndPoint)destination.LocalEndpoint).Port;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var server = Task.Run(async () =>
        {
            using var client = await source.AcceptTcpClientAsync(timeout.Token);
            await using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, false, leaveOpen: true);
            while (!string.IsNullOrEmpty(await reader.ReadLineAsync(timeout.Token)))
            {
            }
            var response = Encoding.ASCII.GetBytes(
                $"HTTP/1.1 302 Found\r\nLocation: http://127.0.0.1:{destinationPort}/announce\r\n" +
                "Content-Length: 0\r\nConnection: close\r\n\r\n");
            await stream.WriteAsync(response, timeout.Token);
        }, timeout.Token);
        try
        {
            var announce = new TrackerAnnounce(
                new Uri($"http://127.0.0.1:{sourcePort}/announce"),
                "00112233445566778899AABBCCDDEEFF00112233",
                "-qB5100-123456789012",
                6881,
                1,
                2,
                3,
                200,
                "deadbeef",
                TrackerEvent.Started,
                ClientProfileCatalog.Get("qbittorrent-5.1"),
                new SimulationProxyOptions());

            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => new TrackerClient().AnnounceAsync(announce, timeout.Token));

            Assert.Contains("crossed the authorized origin", exception.Message, StringComparison.Ordinal);
            await server;
            using var noConnection = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await destination.AcceptTcpClientAsync(noConnection.Token));
        }
        finally
        {
            source.Stop();
            destination.Stop();
        }
    }

    private sealed class RecordingTrackerClient : ITrackerClient
    {
        public List<TrackerEvent> Events { get; } = [];

        public Task<TrackerAnnounceResult> AnnounceAsync(
            TrackerAnnounce announce,
            CancellationToken cancellationToken)
        {
            Events.Add(announce.Event);
            return Task.FromResult(new TrackerAnnounceResult(1800, 12, 4));
        }
    }

    private sealed class BlockingStartTrackerClient : ITrackerClient
    {
        public TaskCompletionSource StartObserved { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseStart { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public List<TrackerEvent> Events { get; } = [];

        public async Task<TrackerAnnounceResult> AnnounceAsync(
            TrackerAnnounce announce,
            CancellationToken cancellationToken)
        {
            Events.Add(announce.Event);
            if (announce.Event == TrackerEvent.Started)
            {
                StartObserved.TrySetResult();
                await ReleaseStart.Task.WaitAsync(cancellationToken);
            }
            return new TrackerAnnounceResult(1800, 12, 4);
        }
    }

    private sealed class CancellationTrackerClient : ITrackerClient
    {
        public async Task<TrackerAnnounceResult> AnnounceAsync(
            TrackerAnnounce announce,
            CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new TrackerAnnounceResult(1800, 0, 0);
        }
    }

    private sealed class ManualTickSource : ISimulationTickSource
    {
        private readonly object _gate = new();
        private readonly List<ManualTickSubscription> _subscriptions = [];

        public int SubscriptionCount
        {
            get
            {
                lock (_gate)
                    return _subscriptions.Count;
            }
        }

        public ValueTask<ISimulationTickSubscription> SubscribeAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var subscription = new ManualTickSubscription(this);
            lock (_gate)
                _subscriptions.Add(subscription);
            return ValueTask.FromResult<ISimulationTickSubscription>(subscription);
        }

        public void Tick()
        {
            ManualTickSubscription[] subscriptions;
            lock (_gate)
                subscriptions = [.. _subscriptions];
            foreach (var subscription in subscriptions)
                subscription.Publish();
        }

        private void Remove(ManualTickSubscription subscription)
        {
            lock (_gate)
                _subscriptions.Remove(subscription);
        }

        private sealed class ManualTickSubscription : ISimulationTickSubscription
        {
            private readonly ManualTickSource _owner;
            private readonly Channel<bool> _ticks = Channel.CreateBounded<bool>(1);
            private int _disposed;

            public ManualTickSubscription(ManualTickSource owner)
            {
                _owner = owner;
            }

            public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default) =>
                WaitCoreAsync(cancellationToken);

            private async ValueTask<bool> WaitCoreAsync(CancellationToken cancellationToken)
            {
                if (!await _ticks.Reader.WaitToReadAsync(cancellationToken))
                    return false;
                _ticks.Reader.TryRead(out _);
                return true;
            }

            public void Publish() => _ticks.Writer.TryWrite(true);

            public ValueTask DisposeAsync()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    _ticks.Writer.TryComplete();
                    _owner.Remove(this);
                }
                return ValueTask.CompletedTask;
            }
        }
    }
}
