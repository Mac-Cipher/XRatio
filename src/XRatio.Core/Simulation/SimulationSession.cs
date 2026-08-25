namespace XRatio.Core.Simulation;

public sealed class SimulationSession : IAsyncDisposable
{
    private readonly ITrackerClient _trackerClient;
    private readonly SimulationOptions _options;
    private readonly ClientProfile _profile;
    private readonly SimulationCounters _counters;
    private readonly string _peerId;
    private readonly string _key;
    private readonly Random _random;
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private CancellationTokenSource? _runCancellation;
    private Task? _runTask;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _nextAnnounce;
    private long _currentUploadRate;
    private long _currentDownloadRate;
    private int _seeders;
    private int _leechers;
    private string? _lastError;
    private bool _completedAnnounced;

    public SimulationSession(SimulationOptions options, ITrackerClient? trackerClient = null, Random? random = null)
    {
        _options = options.Validate();
        _trackerClient = trackerClient ?? new TrackerClient();
        _profile = ClientProfileCatalog.Get(options.ClientProfileId);
        _counters = new SimulationCounters(options.Torrent.TotalSize, options.InitialCompletedPercent);
        _peerId = _profile.CreatePeerId();
        _key = ClientProfile.CreateKey();
        _random = random ?? Random.Shared;
        Id = Guid.NewGuid();
        State = SimulationState.Stopped;
        _currentUploadRate = options.UploadBytesPerSecond;
        _currentDownloadRate = options.DownloadBytesPerSecond;
    }

    public Guid Id { get; }
    public SimulationState State { get; private set; }
    public event EventHandler<SimulationSnapshot>? Updated;
    public event EventHandler<string>? Logged;

    public SimulationSnapshot Snapshot => new(
        Id,
        _options.Torrent.Name,
        _options.Torrent.InfoHashHex,
        _options.Tracker.ToString(),
        _profile.DisplayName,
        State,
        _counters.Uploaded,
        _counters.Downloaded,
        _counters.Left,
        _currentUploadRate,
        _currentDownloadRate,
        _seeders,
        _leechers,
        _startedAt is { } started ? DateTimeOffset.UtcNow - started : TimeSpan.Zero,
        _nextAnnounce,
        _lastError)
    {
        AccountName = _options.AccountName
    };

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State is not SimulationState.Stopped and not SimulationState.Faulted)
                return;
            State = SimulationState.Starting;
            Publish();
            try
            {
                var response = await AnnounceAsync(TrackerEvent.Started, cancellationToken).ConfigureAwait(false);
                ApplyResponse(response);
                _startedAt = DateTimeOffset.UtcNow;
                _runCancellation = new CancellationTokenSource();
                State = SimulationState.Running;
                _runTask = RunAsync(_runCancellation.Token);
                Log("Simulation started.");
                Publish();
            }
            catch (OperationCanceledException)
            {
                State = SimulationState.Stopped;
                _nextAnnounce = null;
                Publish();
                throw;
            }
            catch (Exception exception)
            {
                State = SimulationState.Faulted;
                _lastError = exception.Message;
                Log($"Start failed: {exception.Message}");
                Publish();
                throw;
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State is SimulationState.Stopped or SimulationState.Stopping)
                return;
            State = SimulationState.Stopping;
            Publish();
            try
            {
                if (_runCancellation is not null)
                {
                    await _runCancellation.CancelAsync().ConfigureAwait(false);
                    if (_runTask is not null)
                    {
                        try { await _runTask.ConfigureAwait(false); }
                        catch (OperationCanceledException) { }
                    }
                    _runCancellation.Dispose();
                    _runCancellation = null;
                    _runTask = null;
                }
                await AnnounceAsync(TrackerEvent.Stopped, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _lastError = exception.Message;
                Log($"Stop announce failed: {exception.Message}");
            }
            finally
            {
                State = SimulationState.Stopped;
                _nextAnnounce = null;
                Log("Simulation stopped.");
                Publish();
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    public async Task UpdateNowAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State != SimulationState.Running)
                throw new InvalidOperationException("The simulation is not running.");
            ApplyResponse(await AnnounceAsync(TrackerEvent.None, cancellationToken).ConfigureAwait(false));
            Publish();
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                UpdateRates();
                var completed = _counters.Advance(TimeSpan.FromSeconds(1), _currentUploadRate, _currentDownloadRate);
                if (completed && !_completedAnnounced)
                {
                    ApplyResponse(await AnnounceAsync(TrackerEvent.Completed, cancellationToken).ConfigureAwait(false));
                    _completedAnnounced = true;
                    Log("Torrent reached 100%; completed announce sent.");
                }
                else if (_nextAnnounce is { } next && DateTimeOffset.UtcNow >= next)
                {
                    ApplyResponse(await AnnounceAsync(TrackerEvent.None, cancellationToken).ConfigureAwait(false));
                }

                Publish();
                if (ShouldStop())
                {
                    _ = Task.Run(() => StopAsync(CancellationToken.None), CancellationToken.None);
                    return;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            State = SimulationState.Faulted;
            _lastError = exception.Message;
            Log($"Simulation failed: {exception.Message}");
            Publish();
        }
    }

    private Task<TrackerAnnounceResult> AnnounceAsync(TrackerEvent trackerEvent, CancellationToken cancellationToken) =>
        _trackerClient.AnnounceAsync(new TrackerAnnounce(
            _options.Tracker,
            _options.Torrent.InfoHashHex,
            _peerId,
            _options.Port,
            _counters.Uploaded,
            _counters.Downloaded,
            _counters.Left,
            _options.NumWant,
            _key,
            trackerEvent,
            _profile,
            _options.Proxy), cancellationToken);

    private void ApplyResponse(TrackerAnnounceResult response)
    {
        _seeders = response.Seeders;
        _leechers = response.Leechers;
        _nextAnnounce = DateTimeOffset.UtcNow.AddSeconds(_options.AnnounceIntervalSeconds);
        _lastError = response.Warning;
    }

    private void UpdateRates()
    {
        _currentUploadRate = AddRandom(
            _options.UploadBytesPerSecond,
            _options.RandomUploadEnabled,
            _options.RandomUploadMinimumBytesPerSecond,
            _options.RandomUploadMaximumBytesPerSecond);
        _currentDownloadRate = AddRandom(
            _options.DownloadBytesPerSecond,
            _options.RandomDownloadEnabled,
            _options.RandomDownloadMinimumBytesPerSecond,
            _options.RandomDownloadMaximumBytesPerSecond);
    }

    private long AddRandom(long baseline, bool enabled, long minimum, long maximum)
    {
        if (baseline == 0 || !enabled)
            return baseline;
        var extra = minimum == maximum ? minimum : _random.NextInt64(minimum, maximum + 1);
        return Math.Min(SimulationOptions.MaximumTransferRateBytesPerSecond, baseline + extra);
    }

    private bool ShouldStop()
    {
        var runtime = _startedAt is { } started ? DateTimeOffset.UtcNow - started : TimeSpan.Zero;
        if (_options.MaximumRuntime is { } maxRuntime && runtime >= maxRuntime)
            return true;
        if (_options.MaximumUploadedBytes is { } maxUploaded && _counters.Uploaded >= maxUploaded)
            return true;
        if (_options.MaximumDownloadedBytes is { } maxDownloaded && _counters.Downloaded >= maxDownloaded)
            return true;
        return _options.MaximumRatio is { } maxRatio && _counters.Downloaded > 0 &&
            (double)_counters.Uploaded / _counters.Downloaded >= maxRatio;
    }

    private void Publish() => Updated?.Invoke(this, Snapshot);
    private void Log(string message) => Logged?.Invoke(this, message);

    public async ValueTask DisposeAsync()
    {
        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        _runCancellation?.Dispose();
        _lifecycleGate.Dispose();
    }
}
