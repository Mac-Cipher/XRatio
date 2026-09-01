using Avalonia;
using Avalonia.Threading;

namespace XRatio.Desktop;

internal static class Program
{
    private const string SingleInstanceMutexName = "Local\\XRatio.Desktop.SingleInstance";
    private const string ActivateEventName = "Local\\XRatio.Desktop.Activate";
    private static Mutex? _instanceMutex;
    private static EventWaitHandle? _activateEvent;
    private static CancellationTokenSource? _activationCancellation;
    private static Task? _activationTask;
    private static bool _ownsInstanceMutex;

    [STAThread]
    public static void Main(string[] args)
    {
        if (UpdateInstaller.TryRunApplyCommand(args, out var updaterExitCode))
        {
            Environment.ExitCode = updaterExitCode;
            return;
        }

        if (OperatingSystem.IsWindows() && !TryAcquireSingleInstance())
            return;

        UpdateInstaller.ScheduleStaleArtifactCleanup(UpdateInstaller.GetCurrentExecutablePath());

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            StopActivationListener();
            ReleaseSingleInstance();
        }
    }

    internal static void StartActivationListener(MainWindow window)
    {
        if (!OperatingSystem.IsWindows() || _activateEvent is null || _activationTask is not null)
            return;

        _activationCancellation = new CancellationTokenSource();
        var cancellationToken = _activationCancellation.Token;
        _activationTask = Task.Run(() =>
        {
            while (WaitForActivation(_activateEvent, cancellationToken))
                Dispatcher.UIThread.Post(window.ShowFromTray);
        }, cancellationToken);
    }

    /// <summary>
    /// Waits for the named activation event without polling. The shutdown path
    /// signals the same event after cancelling the token, which wakes this
    /// blocking wait immediately and keeps the listener at zero CPU while idle.
    /// </summary>
    internal static bool WaitForActivation(
        EventWaitHandle activationEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(activationEvent);
        if (cancellationToken.IsCancellationRequested)
            return false;

        try
        {
            return activationEvent.WaitOne() && !cancellationToken.IsCancellationRequested;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    private static bool TryAcquireSingleInstance()
    {
        try
        {
            _activateEvent = new EventWaitHandle(
                initialState: false,
                EventResetMode.AutoReset,
                ActivateEventName);
            _instanceMutex = new Mutex(initiallyOwned: false, SingleInstanceMutexName);
            try
            {
                _ownsInstanceMutex = _instanceMutex.WaitOne(0);
            }
            catch (AbandonedMutexException)
            {
                _ownsInstanceMutex = true;
            }

            if (_ownsInstanceMutex)
                return true;

            // A second launch is only a wake-up request. The existing process
            // owns the mutex and its listener will restore the dashboard.
            _activateEvent.Set();
            _activateEvent.Dispose();
            _activateEvent = null;
            _instanceMutex.Dispose();
            _instanceMutex = null;
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            // If Windows refuses the named objects before a mutex is acquired,
            // keep the app usable rather than failing before Avalonia can show
            // its recovery surface. A process that already saw the mutex must
            // still exit instead of risking a duplicate proxy.
            var secondaryLaunch = _instanceMutex is not null && !_ownsInstanceMutex;
            _activateEvent?.Dispose();
            _activateEvent = null;
            _instanceMutex?.Dispose();
            _instanceMutex = null;
            return !secondaryLaunch;
        }
    }

    private static void StopActivationListener()
    {
        _activationCancellation?.Cancel();
        try
        {
            _activateEvent?.Set();
        }
        catch (ObjectDisposedException)
        {
            // The event can already be gone during an early startup failure.
        }

        try
        {
            _activationTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (AggregateException)
        {
            // The listener is best-effort during process teardown.
        }

        _activationCancellation?.Dispose();
        _activationCancellation = null;
        _activationTask = null;
        _activateEvent?.Dispose();
        _activateEvent = null;
    }

    private static void ReleaseSingleInstance()
    {
        if (_instanceMutex is null)
            return;

        if (_ownsInstanceMutex)
        {
            try
            {
                _instanceMutex.ReleaseMutex();
            }
            catch (ApplicationException)
            {
                // The OS releases an abandoned mutex when the process exits.
            }
        }

        _ownsInstanceMutex = false;
        _instanceMutex.Dispose();
        _instanceMutex = null;
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}
