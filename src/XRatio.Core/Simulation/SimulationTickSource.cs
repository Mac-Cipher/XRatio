using System.Threading.Channels;

namespace XRatio.Core.Simulation;

/// <summary>
/// Supplies one shared clock tick to all running simulations. A source owns
/// one timer regardless of the number of subscriptions; each subscription
/// receives a coalescing signal so a slow tracker request cannot build an
/// unbounded backlog.
/// </summary>
public interface ISimulationTickSource
{
    ValueTask<ISimulationTickSubscription> SubscribeAsync(
        CancellationToken cancellationToken = default);
}

public interface ISimulationTickSubscription : IAsyncDisposable
{
    ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default);
}

public sealed class SimulationTickHub : ISimulationTickSource, IAsyncDisposable
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(1);
    public static SimulationTickHub Shared { get; } = new();
    private readonly TimeSpan _interval;
    private readonly TimeProvider _timeProvider;
    private readonly object _gate = new();
    private readonly HashSet<Subscription> _subscriptions = [];
    private readonly SemaphoreSlim _lifecycleGate = new(1, 1);
    private CancellationTokenSource? _runCancellation;
    private Task? _runTask;
    private bool _disposed;

    public SimulationTickHub(TimeSpan? interval = null, TimeProvider? timeProvider = null)
    {
        _interval = interval ?? DefaultInterval;
        if (_interval <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(interval));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async ValueTask<ISimulationTickSubscription> SubscribeAsync(
        CancellationToken cancellationToken = default)
    {
        await _lifecycleGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                var subscription = new Subscription(this);
                _subscriptions.Add(subscription);
                if (_runTask is null)
                {
                    _runCancellation = new CancellationTokenSource();
                    _runTask = RunAsync(_runCancellation.Token);
                }

                return subscription;
            }
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
            using var timer = new PeriodicTimer(_interval, _timeProvider);
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                // Publishing under the lock keeps the subscription set stable
                // without allocating a snapshot on every shared tick.
                lock (_gate)
                {
                    foreach (var subscription in _subscriptions)
                        subscription.PublishTick();
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async ValueTask UnsubscribeAsync(Subscription subscription)
    {
        await _lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            Task? runTask = null;
            CancellationTokenSource? runCancellation = null;
            lock (_gate)
            {
                if (!_subscriptions.Remove(subscription))
                    return;
                subscription.Complete();
                if (_subscriptions.Count == 0)
                {
                    runTask = _runTask;
                    runCancellation = _runCancellation;
                    _runTask = null;
                    _runCancellation = null;
                    runCancellation?.Cancel();
                }
            }

            if (runTask is not null)
            {
                try
                {
                    await runTask.ConfigureAwait(false);
                }
                finally
                {
                    runCancellation?.Dispose();
                }
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _lifecycleGate.WaitAsync().ConfigureAwait(false);
        try
        {
            Task? runTask;
            CancellationTokenSource? runCancellation;
            lock (_gate)
            {
                if (_disposed)
                    return;
                _disposed = true;
                foreach (var subscription in _subscriptions)
                    subscription.Complete();
                _subscriptions.Clear();
                runTask = _runTask;
                runCancellation = _runCancellation;
                _runTask = null;
                _runCancellation = null;
                runCancellation?.Cancel();
            }

            if (runTask is not null)
            {
                try
                {
                    await runTask.ConfigureAwait(false);
                }
                finally
                {
                    runCancellation?.Dispose();
                }
            }
        }
        finally
        {
            _lifecycleGate.Release();
        }
    }

    private sealed class Subscription : ISimulationTickSubscription
    {
        private readonly SimulationTickHub _owner;
        private readonly Channel<bool> _ticks = Channel.CreateBounded<bool>(
            new BoundedChannelOptions(1)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.DropOldest
            });
        private int _disposed;

        public Subscription(SimulationTickHub owner)
        {
            _owner = owner;
        }

        public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
            return WaitCoreAsync(cancellationToken);
        }

        private async ValueTask<bool> WaitCoreAsync(CancellationToken cancellationToken)
        {
            if (!await _ticks.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                return false;
            _ticks.Reader.TryRead(out _);
            return true;
        }

        public void PublishTick() => _ticks.Writer.TryWrite(true);

        public void Complete()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
                _ticks.Writer.TryComplete();
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;
            _ticks.Writer.TryComplete();
            await _owner.UnsubscribeAsync(this).ConfigureAwait(false);
        }
    }
}
