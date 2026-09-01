using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using XRatio.Core.Configuration;
using XRatio.Core.Platform;
using XRatio.Core.Simulation;
using XRatio.Desktop.Platform;
using XRatio.Proxy;

namespace XRatio.Desktop;

public sealed class App : Application
{
    private TrayIcon? _trayIcon;
    private WindowIcon? _colorTrayIcon;
    private WindowIcon? _monochromeTrayIcon;
    private WindowIcon? _stopTrayIcon;
    private WindowIcon? _stopTrayIconWithUpdate;
    private WindowIcon? _pauseTrayIcon;
    private WindowIcon? _pauseTrayIconWithUpdate;
    private WindowIcon? _colorTrayIconWithUpdate;
    private WindowIcon? _monochromeTrayIconWithUpdate;

    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Light;
        Styles.Add(new FluentTheme());
        ApplyThemeVariant("Light", AccentPalette.Blue);
    }

    internal static void ApplyThemeVariant(string themeMode, string? accentColor = null)
    {
        var normalizedTheme = ThemePalette.Normalize(themeMode);
        var dark = normalizedTheme == ThemePalette.Dark;
        var dim = normalizedTheme == ThemePalette.Dim;
        var softDark = normalizedTheme == ThemePalette.SoftDark;
        var normalizedAccent = AccentPalette.Normalize(accentColor);
        if (Current is not { } application)
            return;

        application.RequestedThemeVariant = ThemePalette.UsesDarkControls(normalizedTheme)
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
        application.Resources["SystemAccentColor"] = Color.Parse(AccentPalette.Primary(normalizedAccent, dark, dim, softDark));
        application.Resources["SystemAccentColorLight1"] = Color.Parse(AccentPalette.Light1(normalizedAccent, dark, dim, softDark));
        application.Resources["SystemAccentColorDark1"] = Color.Parse(AccentPalette.Dark1(normalizedAccent, dark, dim, softDark));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var profileDirectory = ProfileDirectory.GetDefault();
            var store = new JsonSettingsStore(profileDirectory);
            var simulationStore = new SimulationSessionStore(profileDirectory);
            var autostart = PlatformServices.CreateAutostart();
            var certificates = PlatformServices.CreateCertificateAuthority(profileDirectory);
            var debugLogger = new FileProxyDebugLogger(
                Path.Combine(profileDirectory, "proxy_debug.log"));
            var window = new MainWindow(
                store,
                autostart,
                certificates,
                () => desktop.Shutdown(),
                debugLogger,
                simulationStore);
            desktop.MainWindow = window;
            Program.StartActivationListener(window);
            // The Windows tray path is validated end-to-end. Keep non-Windows
            // backends window-only until their native tray integration is tested.
            if (ShouldCreateTrayIcon(OperatingSystem.IsWindows()))
                _trayIcon = CreateTrayIcon(window);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private TrayIcon CreateTrayIcon(MainWindow window)
    {
        var tray = new TrayIcon
        {
            Icon = GetTrayIcon(
                window.IsProxyRunning,
                window.IsProxyPaused,
                window.UseMonochromeTrayIcon,
                window.IsUpdateAvailable),
            ToolTipText = FormatTrayToolTip(
                window.IsProxyRunning,
                window.IsProxyPaused,
                window.CurrentLanguage),
            Menu = BuildTrayMenu(window),
            IsVisible = window.IsTrayIconEnabled
        };
        window.RuntimeStateChanged += (isRunning, isPaused) =>
        {
            tray.ToolTipText = FormatTrayToolTip(isRunning, isPaused, window.CurrentLanguage);
            tray.Icon = GetTrayIcon(
                isRunning,
                isPaused,
                window.UseMonochromeTrayIcon,
                window.IsUpdateAvailable);
            tray.IsVisible = window.IsTrayIconEnabled;
        };
        window.UpdateAvailabilityChanged += updateAvailable =>
        {
            tray.Icon = GetTrayIcon(
                window.IsProxyRunning,
                window.IsProxyPaused,
                window.UseMonochromeTrayIcon,
                updateAvailable);
            tray.IsVisible = window.IsTrayIconEnabled;
        };
        window.LanguageChanged += language =>
        {
            tray.ToolTipText = FormatTrayToolTip(
                window.IsProxyRunning,
                window.IsProxyPaused,
                language);
        };
        tray.Clicked += (_, _) => window.ShowFromTray();
        return tray;
    }

    private WindowIcon GetTrayIcon(
        bool isRunning,
        bool isPaused,
        bool monochrome,
        bool updateAvailable)
    {
        // Monochrome is an explicit user override: it must not leak the
        // red/orange state colors into the notification area. The blue update
        // badge remains visible because it is a separate actionable signal.
        if (monochrome)
            return updateAvailable
                ? _monochromeTrayIconWithUpdate ??= TrayIconRenderer.CreateMonochromeIcon(updateAvailable: true)
                : _monochromeTrayIcon ??= TrayIconRenderer.CreateMonochromeIcon();
        if (!isRunning)
            return updateAvailable
                ? _stopTrayIconWithUpdate ??= TrayIconRenderer.CreateStopIcon(updateAvailable: true)
                : _stopTrayIcon ??= TrayIconRenderer.CreateStopIcon();
        if (isPaused)
            return updateAvailable
                ? _pauseTrayIconWithUpdate ??= TrayIconRenderer.CreatePauseIcon(updateAvailable: true)
                : _pauseTrayIcon ??= TrayIconRenderer.CreatePauseIcon();
        return updateAvailable
            ? _colorTrayIconWithUpdate ??= TrayIconRenderer.CreateColorIcon(updateAvailable: true)
            : _colorTrayIcon ??= CreateAppIcon();
    }

    internal static string FormatTrayToolTip(
        bool isRunning,
        bool isPaused,
        string? language = null) =>
        UiText.Translate(
            !isRunning
                ? "XRatio — OFF"
                : isPaused
                    ? "XRatio — ON (paused)"
                    : "XRatio — ON",
            language ?? UiText.English);

    internal static WindowIcon CreateAppIcon()
    {
        using var iconStream = AssetLoader.Open(new Uri("avares://XRatio/Assets/XRatio-app-icon-v5.png"));
        return new WindowIcon(iconStream);
    }

    internal static NativeMenu BuildTrayMenu(MainWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        var show = new NativeMenuItem();
        show.Click += (_, _) => window.ShowFromTray();
        var pause = new NativeMenuItem();
        pause.Click += (_, _) => window.TogglePause();
        var exit = new NativeMenuItem();
        exit.Click += async (_, _) => await window.PrepareForExitAsync();

        void ApplyLanguage(string language)
        {
            show.Header = UiText.Translate("Show XRatio", language);
            pause.Header = UiText.Translate("Pause / resume rewriting", language);
            exit.Header = UiText.Translate("Exit", language);
        }

        ApplyLanguage(window.CurrentLanguage);
        window.LanguageChanged += ApplyLanguage;
        return new NativeMenu
        {
            Items = { show, pause, new NativeMenuItemSeparator(), exit }
        };
    }

    internal static bool ShouldCreateTrayIcon(bool isWindows) => isWindows;
}
