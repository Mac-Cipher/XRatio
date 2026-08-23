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
        var normalizedAccent = AccentPalette.Normalize(accentColor);
        if (Current is not { } application)
            return;

        application.RequestedThemeVariant = ThemePalette.UsesDarkControls(normalizedTheme)
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
        application.Resources["SystemAccentColor"] = Color.Parse(AccentPalette.Primary(normalizedAccent, dark, dim));
        application.Resources["SystemAccentColorLight1"] = Color.Parse(AccentPalette.Light1(normalizedAccent, dark, dim));
        application.Resources["SystemAccentColorDark1"] = Color.Parse(AccentPalette.Dark1(normalizedAccent, dark, dim));
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
            // The Windows tray path is validated end-to-end. Keep non-Windows
            // backends window-only until their native tray integration is tested.
            if (ShouldCreateTrayIcon(OperatingSystem.IsWindows()))
                _trayIcon = CreateTrayIcon(window, desktop);
        }
        base.OnFrameworkInitializationCompleted();
    }

    private static TrayIcon CreateTrayIcon(
        MainWindow window,
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        var tray = new TrayIcon
        {
            Icon = CreateAppIcon(),
            ToolTipText = "XRatio",
            Menu = BuildTrayMenu(window),
            IsVisible = true
        };
        tray.Clicked += (_, _) => window.ShowFromTray();
        return tray;
    }

    internal static WindowIcon CreateAppIcon()
    {
        using var iconStream = AssetLoader.Open(new Uri("avares://XRatio/Assets/XRatio-app-icon-v5.png"));
        return new WindowIcon(iconStream);
    }

    internal static NativeMenu BuildTrayMenu(MainWindow window)
    {
        ArgumentNullException.ThrowIfNull(window);
        var show = new NativeMenuItem { Header = "Show XRatio" };
        show.Click += (_, _) => window.ShowFromTray();
        var pause = new NativeMenuItem { Header = "Pause / resume rewriting" };
        pause.Click += (_, _) => window.TogglePause();
        var exit = new NativeMenuItem { Header = "Exit" };
        exit.Click += async (_, _) => await window.PrepareForExitAsync();
        return new NativeMenu
        {
            Items = { show, pause, new NativeMenuItemSeparator(), exit }
        };
    }

    internal static bool ShouldCreateTrayIcon(bool isWindows) => isWindows;
}
