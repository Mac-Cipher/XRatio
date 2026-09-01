using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using System.Reflection;
using System.Net.Sockets;
using XRatio.Core.Announcements;
using XRatio.Core.Configuration;
using XRatio.Core.Platform;
using XRatio.Core.Simulation;
using XRatio.Desktop;

namespace XRatio.Desktop.Tests;

public sealed class MainWindowSurfaceTests
{
    [Fact]
    public void TorrentClientLauncher_RejectsMissingOrUnexpectedExecutable()
    {
        Assert.False(TorrentClientDetector.TryOpen(
            new DetectedTorrentClient("qBittorrent", Path.Combine(AppContext.BaseDirectory, "missing", "not-qbittorrent.exe"))));
        Assert.False(TorrentClientDetector.TryOpen(
            new DetectedTorrentClient("qBittorrent", Path.Combine(AppContext.BaseDirectory, "missing", "qbittorrent.txt"))));
    }

    [Fact]
    public void SettingsTooltipsHaveTranslationsForEverySupportedLanguage()
    {
        var tooltipKeys = new[]
        {
            "The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent.",
            "Minimum incomplete peers required before ratio shaping adds calculated upload.",
            "Blocks non-tracker traffic so XRatio stays focused on tracker announce requests.",
            "Keeps the proxy bound to localhost. This required security boundary cannot be disabled.",
            "Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting.",
            "Lower bound for upload credited per actual download during announce shaping.",
            "Upper bound for upload credited per actual download during announce shaping.",
            "Lower bound for the upload multiplier applied to actual upload.",
            "Upper bound for the upload multiplier applied to actual upload.",
            "Maximum extra upload boost used during a shaped announce, in KiB/s.",
            "Percentage chance, from 0 to 100, that the extra upload boost is applied.",
            "Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting.",
            "Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes.",
            "Changes the visual theme without changing proxy behavior.",
            "Changes the interface accent color without changing proxy behavior.",
            "Chooses whether the notification-area icon uses color states or monochrome.",
            "Changes the language used by the XRatio interface.",
            "Starts XRatio automatically with your Windows session.",
            "Keeps an XRatio icon in the Windows notification area.",
            "Starts XRatio hidden in the notification area instead of opening the main window.",
            "Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception.",
            "Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved."
        };

        foreach (var language in UiText.LanguageCodes)
        {
            foreach (var key in tooltipKeys)
            {
                var translated = UiText.Translate(key, language);
                Assert.False(string.IsNullOrWhiteSpace(translated), $"Missing tooltip for {language}: {key}");
                if (!string.Equals(language, UiText.English, StringComparison.Ordinal))
                    Assert.NotEqual(key, translated);
            }
        }
    }

    [Fact]
    public void EveryCanonicalUiKeyHasATranslationForEverySupportedLanguage()
    {
        foreach (var language in UiText.LanguageCodes)
        {
            foreach (var key in UiText.TranslationKeys)
            {
                Assert.True(
                    UiText.HasTranslation(key, language),
                    $"Missing translation for '{key}' in {language}.");
                Assert.False(
                    string.IsNullOrWhiteSpace(UiText.Translate(key, language)),
                    $"Blank translation for '{key}' in {language}.");
            }

            // The compact sidebar action is intentionally the only UI label
            // that stays in English in every locale.
            Assert.Equal("Update", UiText.UpdateIndicatorLabel(language));
        }
    }

    [Fact]
    public void ResetConfigurableSettingsRestoresDefaultsWithoutErasingRuntimeData()
    {
        var simulationForm = new SimulationFormSettings { AccountName = "keep-me" };
        IReadOnlyList<PersistedTorrentState> persistedTorrents =
        [
            new(
                "0123456789abcdef0123456789abcdef01234567",
                "https://tracker.example/announce",
                ActualFirstLeft: 100,
                ActualDownloaded: 20,
                ActualUploaded: 30,
                ActualLeft: 80,
                ReportedDownloaded: 0,
                ReportedUploaded: 40,
                ReportedLeft: 80,
                CompletePeers: 2,
                IncompletePeers: 4,
                LastAnnounce: DateTimeOffset.UtcNow)
        ];
        var current = new XRatioSettings
        {
            ThemeMode = "Dark",
            AccentColor = "Rose",
            TrayIconStyle = "Monochrome",
            Language = UiText.English,
            ShowTrayIcon = false,
            ListenPort = 49152,
            OnlyTrackerTraffic = false,
            ProxyDebugLogging = true,
            StartMinimized = false,
            AutoStart = false,
            CheckUpdatesOnStartup = false,
            MinimumPeers = 22,
            UploadPerDownloadMaximum = 0.9,
            UploadPerUploadMinimum = 9,
            UploadPerUploadMaximum = 12,
            BoostKiBPerSecond = 40,
            BoostChancePercent = 80,
            PretendToSeed = false,
            OnboardingDismissed = true,
            OnboardingCompletedSteps = ["https"],
            LifetimeRuntimeSeconds = 12,
            LifetimeActualDownloaded = 34,
            LifetimeActualUploaded = 56,
            LifetimeReportedDownloaded = 78,
            LifetimeReportedUploaded = 90,
            Sessions = 3,
            SimulationForm = simulationForm,
            PersistedTorrents = persistedTorrents
        };

        var reset = MainWindow.ResetConfigurableSettings(current);
        var defaults = new XRatioSettings();

        Assert.Equal(defaults.ThemeMode, reset.ThemeMode);
        Assert.Equal(defaults.AccentColor, reset.AccentColor);
        Assert.Equal(defaults.TrayIconStyle, reset.TrayIconStyle);
        Assert.Equal(defaults.Language, reset.Language);
        Assert.Equal(defaults.ShowTrayIcon, reset.ShowTrayIcon);
        Assert.Equal(defaults.ListenPort, reset.ListenPort);
        Assert.Equal(defaults.OnlyTrackerTraffic, reset.OnlyTrackerTraffic);
        Assert.Equal(defaults.OnlyLocalConnections, reset.OnlyLocalConnections);
        Assert.Equal(defaults.ProxyDebugLogging, reset.ProxyDebugLogging);
        Assert.Equal(defaults.StartMinimized, reset.StartMinimized);
        Assert.Equal(defaults.AutoStart, reset.AutoStart);
        Assert.Equal(defaults.CheckUpdatesOnStartup, reset.CheckUpdatesOnStartup);
        Assert.Equal(defaults.MinimumPeers, reset.MinimumPeers);
        Assert.Equal(defaults.UploadPerDownloadMinimum, reset.UploadPerDownloadMinimum);
        Assert.Equal(defaults.UploadPerDownloadMaximum, reset.UploadPerDownloadMaximum);
        Assert.Equal(defaults.UploadPerUploadMinimum, reset.UploadPerUploadMinimum);
        Assert.Equal(defaults.UploadPerUploadMaximum, reset.UploadPerUploadMaximum);
        Assert.Equal(defaults.BoostKiBPerSecond, reset.BoostKiBPerSecond);
        Assert.Equal(defaults.BoostChancePercent, reset.BoostChancePercent);
        Assert.Equal(defaults.ReportDownloadAsZero, reset.ReportDownloadAsZero);
        Assert.Equal(defaults.PretendToSeed, reset.PretendToSeed);
        Assert.True(reset.OnboardingDismissed);
        Assert.Equal(current.OnboardingCompletedSteps, reset.OnboardingCompletedSteps);
        Assert.Equal(current.LifetimeRuntimeSeconds, reset.LifetimeRuntimeSeconds);
        Assert.Equal(current.LifetimeActualDownloaded, reset.LifetimeActualDownloaded);
        Assert.Equal(current.LifetimeActualUploaded, reset.LifetimeActualUploaded);
        Assert.Equal(current.LifetimeReportedDownloaded, reset.LifetimeReportedDownloaded);
        Assert.Equal(current.LifetimeReportedUploaded, reset.LifetimeReportedUploaded);
        Assert.Equal(current.Sessions, reset.Sessions);
        Assert.Same(simulationForm, reset.SimulationForm);
        Assert.Same(persistedTorrents, reset.PersistedTorrents);
    }

    [Fact]
    public void Constructor_BuildsEssentialDesktopSurface()
    {
        Assert.True(MainWindow.ShouldStartMinimized(
            trayAvailable: true,
            startMinimizedSetting: true,
            minimizedCommandLine: false));
        Assert.False(MainWindow.ShouldStartMinimized(
            trayAvailable: false,
            startMinimizedSetting: true,
            minimizedCommandLine: true));
        Assert.True(MainWindow.ShouldHideAfterStartup(
            trayAvailable: true,
            startMinimizedSetting: true,
            minimizedCommandLine: false,
            restoreRequested: false));
        Assert.False(MainWindow.ShouldHideAfterStartup(
            trayAvailable: true,
            startMinimizedSetting: true,
            minimizedCommandLine: false,
            restoreRequested: true));
        Assert.True(MainWindow.ShouldHideOnWindowClose(trayAvailable: true));
        Assert.False(MainWindow.ShouldHideOnWindowClose(trayAvailable: false));
        Assert.Equal(UiText.English, new XRatioSettings().Language);
        Assert.Equal("Light", new XRatioSettings().ThemeMode);
        Assert.Equal("Blue", new XRatioSettings().AccentColor);
        Assert.Equal("Color", new XRatioSettings().TrayIconStyle);
        Assert.True(new XRatioSettings().ShowTrayIcon);
        Assert.True(new XRatioSettings().StartMinimized);
        Assert.True(new XRatioSettings().AutoStart);
        Assert.True(new XRatioSettings().CheckUpdatesOnStartup);
        Assert.Equal(ThemePalette.SoftDark, MainWindow.NormalizeThemeMode("Soft Dark"));
        Assert.Equal(AccentPalette.Violet, MainWindow.NormalizeAccentColor("Violet"));
        Assert.Equal("Monochrome", MainWindow.NormalizeTrayIconStyle(" monochrome "));
        Assert.Equal("Color", MainWindow.NormalizeTrayIconStyle("unknown"));
        Assert.Equal(UiText.Japanese, UiText.Normalize("🇯🇵 日本語"));

        if (!OperatingSystem.IsWindows())
            return;

        EnsureAvaloniaSetup();

        using (var guideScreenshot = AssetLoader.Open(new Uri("avares://XRatio/Assets/qbittorrent-proxy-settings.png")))
            Assert.True(guideScreenshot.Length > 0);

        var window = new MainWindow(
            new InMemorySettingsStore(),
            new TestAutostartService(),
            new TestCertificateAuthorityService(),
            shutdown: static () => { });

        Assert.Equal("XRatio", window.Title);
        Assert.Equal("XRatio", MainWindow.ResolveWindowTitle(isWindows: true));
        Assert.Equal("XRatio", MainWindow.ResolveWindowTitle(isWindows: false));
        Assert.True(App.ShouldCreateTrayIcon(isWindows: true));
        Assert.False(App.ShouldCreateTrayIcon(isWindows: false));
        Assert.Equal("XRatio — OFF", App.FormatTrayToolTip(isRunning: false, isPaused: false));
        Assert.Equal("XRatio — ON", App.FormatTrayToolTip(isRunning: true, isPaused: false));
        Assert.Equal("XRatio — ON (paused)", App.FormatTrayToolTip(isRunning: true, isPaused: true));
        Assert.Equal("XRatio — ARRÊTÉ", App.FormatTrayToolTip(
            isRunning: false,
            isPaused: false,
            language: UiText.French));
        Assert.NotEqual(
            "XRatio — ON (paused)",
            App.FormatTrayToolTip(isRunning: true, isPaused: true, language: UiText.Spanish));
        Assert.Equal(1280, window.Width);
        Assert.Equal(800, window.Height);
        Assert.Equal(492, MainWindow.ResolveSimulationTabsMaxHeight(800));
        Assert.Equal(332, MainWindow.ResolveSimulationTabsMaxHeight(640));
        Assert.NotNull(window.Icon);

        var root = Assert.IsType<Grid>(window.Content);
        Assert.Equal(2, root.Children.Count);
        var header = Assert.IsType<Border>(root.Children[0]);
        Assert.DoesNotContain(
            Descendants(header).OfType<Button>(),
            button => Equals(button.Content, "Apply") || Equals(button.Content, "Save changes"));

        var body = Assert.IsType<Grid>(root.Children[1]);
        var sidebar = Assert.IsType<Border>(body.Children[0]);
        Assert.Equal(250, sidebar.Width);
        Assert.Equal(new Thickness(14, 8, 14, 8), sidebar.Padding);
        Assert.Equal(250, body.ColumnDefinitions[0].Width.Value);
        Assert.Contains(
            Descendants(sidebar).OfType<Border>(),
            border => border.CornerRadius == new CornerRadius(18));
        var onboardingNavigation = Descendants(sidebar)
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "OnboardingNavAction"));
        var onboardingSidebarLabel = Descendants(onboardingNavigation)
            .OfType<TextBlock>()
            .Single(text => Equals(text.Tag, "OnboardingSidebarLabel"));
        Assert.Equal("Onboarding", onboardingSidebarLabel.Text);
        Assert.Contains(
            Descendants(sidebar).OfType<TextBlock>(),
            text => Equals(text.Text, "Get started"));
        var onboardingClose = Descendants(sidebar)
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "OnboardingSidebarClose"));
        var onboardingCloseIcon = Assert.IsType<Grid>(onboardingClose.Content);
        Assert.Equal("CloseGlyph", onboardingCloseIcon.Tag);
        Assert.Equal(16, onboardingCloseIcon.Width);
        Assert.Equal(16, onboardingCloseIcon.Height);
        Assert.Equal(2, onboardingCloseIcon.Children.Count);
        Assert.All(
            onboardingCloseIcon.Children.OfType<Border>(),
                 stroke =>
                 {
                     Assert.Equal("CloseGlyphStroke", stroke.Tag);
                     Assert.Equal(1.5, stroke.Height);
                     Assert.Equal(new CornerRadius(0.75), stroke.CornerRadius);
                     Assert.Equal(HorizontalAlignment.Center, stroke.HorizontalAlignment);
                     Assert.Equal(VerticalAlignment.Center, stroke.VerticalAlignment);
                 });
        Assert.Equal(36, onboardingClose.Width);
        Assert.Equal(36, onboardingClose.Height);
        Assert.NotNull(onboardingClose.Template);
        Assert.Equal(Brushes.Transparent, onboardingClose.Background);
        var onboardingChecklist = Descendants(sidebar)
            .OfType<StackPanel>()
            .Single(panel => Equals(panel.Tag, "OnboardingChecklist"));
        Assert.False(onboardingChecklist.IsVisible);
        Assert.Empty(Descendants(onboardingChecklist).OfType<Button>());
        var tabs = Assert.IsType<TabControl>(body.Children[1]);
        var tabItems = tabs.Items.Cast<TabItem>().ToArray();
        Assert.Equal(
            ["Overview", "Interception", "Simulation", "Activity", "Settings", "Platform"],
            tabItems.Select(item => Assert.IsType<string>(item.Tag)).ToArray());
        Assert.All(tabItems, item => Assert.True(item.MinHeight >= 40));
        Assert.All(tabItems, item => Assert.Equal(44, item.MinHeight));
        Assert.Equal(
            ["Monitoring", "Control", "System"],
            tabItems
                .SelectMany(item => Descendants(Assert.IsAssignableFrom<Control>(item.Header)))
                .OfType<TextBlock>()
                .Where(text => Equals(text.Tag, "NavSection"))
                .Select(text => Assert.IsType<string>(text.Text))
                .ToArray());
        Assert.Equal(
            2,
            tabItems
                .SelectMany(item => Descendants(Assert.IsAssignableFrom<Control>(item.Header)))
                .Count(control => Equals(control.Tag, "NavDivider")));
        var overviewContent = Assert.IsAssignableFrom<Control>(tabItems[0].Content);
        var overviewOnboarding = Descendants(overviewContent)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "OverviewOnboardingCard"));
        Assert.True(overviewOnboarding.IsVisible);
        var overviewStepPanel = Descendants(overviewOnboarding)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "OverviewOnboardingStepPanel"));
        Assert.Equal(new CornerRadius(14), overviewStepPanel.CornerRadius);
        Assert.Equal(new Thickness(1), overviewStepPanel.BorderThickness);
        Assert.Equal(VerticalAlignment.Top, overviewStepPanel.VerticalAlignment);
        Assert.Equal(new Thickness(0), overviewStepPanel.Margin);
        var overviewStepMeta = Descendants(overviewStepPanel)
            .OfType<Grid>()
            .Single(grid => grid.Children.OfType<TextBlock>()
                .Any(text => Equals(text.Text, "1 of 4")));
        Assert.All(
            overviewStepMeta.Children.OfType<TextBlock>(),
            text => Assert.Equal(VerticalAlignment.Bottom, text.VerticalAlignment));
        Assert.Contains(
            Descendants(overviewOnboarding).OfType<TextBlock>(),
            text => Equals(text.Text, "Connect your torrent client"));
        var qBittorrentInstructions = Descendants(overviewOnboarding)
            .OfType<TextBlock>()
            .First(text => (text.Text ?? string.Empty)
                .Contains("Open Tools", StringComparison.Ordinal));
        Assert.Contains("Open Tools > Options > Connection.", qBittorrentInstructions.Text);
        Assert.DoesNotContain("Outils", qBittorrentInstructions.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("→", qBittorrentInstructions.Text, StringComparison.Ordinal);
        var otherClientInstructions = Descendants(overviewOnboarding)
            .OfType<TextBlock>()
            .First(text => (text.Text ?? string.Empty)
                .Contains("Open Settings/Preferences", StringComparison.Ordinal));
        Assert.Contains("Open Settings/Preferences > Connection", otherClientInstructions.Text);
        Assert.DoesNotContain("→", otherClientInstructions.Text, StringComparison.Ordinal);
        Assert.Equal(
            4,
            Descendants(overviewOnboarding).OfType<Button>()
                .Count(button => (button.Tag?.ToString() ?? string.Empty)
                    .StartsWith("OnboardingSidebarStep", StringComparison.Ordinal)));
        Assert.All(
            Descendants(overviewOnboarding).OfType<Button>()
                .Where(button => (button.Tag?.ToString() ?? string.Empty)
                    .StartsWith("OnboardingSidebarStep", StringComparison.Ordinal)),
            button =>
            {
                Assert.Equal(new Thickness(1), button.BorderThickness);
                Assert.NotSame(Brushes.Transparent, button.BorderBrush);
            });
        var overviewClose = Descendants(overviewOnboarding)
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "OverviewOnboardingClose"));
        var overviewCloseIcon = Assert.IsType<Grid>(overviewClose.Content);
        Assert.Equal("CloseGlyph", overviewCloseIcon.Tag);
        Assert.Equal(16, overviewCloseIcon.Width);
        Assert.Equal(16, overviewCloseIcon.Height);
        Assert.Equal(2, overviewCloseIcon.Children.Count);
        Assert.All(
            overviewCloseIcon.Children.OfType<Border>(),
            stroke =>
            {
                Assert.Equal("CloseGlyphStroke", stroke.Tag);
                Assert.Equal(1.5, stroke.Height);
                Assert.Equal(new CornerRadius(0.75), stroke.CornerRadius);
                Assert.Equal(HorizontalAlignment.Center, stroke.HorizontalAlignment);
                Assert.Equal(VerticalAlignment.Center, stroke.VerticalAlignment);
            });
        Assert.Equal(36, overviewClose.Width);
        Assert.Equal(36, overviewClose.Height);
        Assert.NotNull(overviewClose.Template);
        var overviewHeader = Descendants(overviewOnboarding)
            .OfType<Grid>()
            .Single(grid => grid.Children.Contains(overviewClose));
        Assert.Equal(36, overviewHeader.ColumnDefinitions[2].Width.Value);
        Assert.Contains(
            Descendants(overviewOnboarding).OfType<Button>(),
            button => Equals(button.Tag, "OverviewOnboardingAction") &&
                      Equals(button.Content, "Setup guide →") &&
                      !button.IsVisible);
        var overviewDone = Descendants(overviewOnboarding)
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "OverviewOnboardingDone"));
        Assert.Equal("Mark as configured", overviewDone.Content);
        Assert.True(overviewDone.MinWidth >= 150);
        Assert.True(overviewDone.MinHeight >= 40);
        Assert.NotSame(Brushes.Transparent, overviewDone.Background);
        Assert.Contains(
            Descendants(overviewOnboarding).OfType<Button>(),
            button => Equals(button.Tag, "OverviewOnboardingPrevious") &&
                      Equals(button.Content, "←"));
        Assert.Contains(
            Descendants(overviewOnboarding).OfType<Button>(),
            button => Equals(button.Tag, "OverviewOnboardingNext") &&
                      Equals(button.Content, "→"));
        Assert.Contains(
            Descendants(overviewOnboarding).OfType<Border>(),
            border => Equals(border.Tag, "OnboardingQbittorrentScreenshot"));
        Assert.Contains(
            Descendants(overviewOnboarding).OfType<Border>(),
            border => Equals(border.Tag, "OnboardingOtherTorrentClients"));
        var qBittorrentScreenshot = Descendants(overviewOnboarding)
            .OfType<Image>()
            .Single();
        Assert.Equal(Stretch.Uniform, qBittorrentScreenshot.Stretch);
        Assert.Equal(540, qBittorrentScreenshot.MaxWidth);
        Assert.Equal(230, qBittorrentScreenshot.MaxHeight);
        tabs.SelectedIndex = 2;
        var selectedNavRow = Descendants(Assert.IsAssignableFrom<Control>(tabItems[2].Header))
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "NavRow"));
        var inactiveNavRow = Descendants(Assert.IsAssignableFrom<Control>(tabItems[1].Header))
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "NavRow"));
        Assert.NotSame(Brushes.Transparent, selectedNavRow.Background);
        Assert.Same(Brushes.Transparent, inactiveNavRow.Background);
        Assert.Equal(new CornerRadius(10), selectedNavRow.CornerRadius);

        var buttons = Descendants(root).OfType<Button>().ToArray();
        Assert.NotEmpty(buttons);
        Assert.All(
            buttons.Where(button =>
                button is not CheckBox &&
                !Equals(button.Tag, "SimulationTimerMinutes") &&
                !Equals(button.Tag, "SimulationTimerHours") &&
                !Equals(button.Tag, "OnboardingSidebarClose") &&
                !Equals(button.Tag, "OverviewOnboardingClose") &&
                !Equals(button.Tag, "InterceptionCoachmarkClose") &&
                !Equals(button.Tag, "SimulationCoachmarkClose")),
            button => Assert.True(button.MinHeight >= 36));
        var guideButton = buttons.Single(button => Equals(button.Tag, "GuideAction"));
        var guideIcon = Assert.Single(
            Descendants(guideButton).OfType<Grid>(),
            icon => Equals(icon.Tag, "GuideIcon"));
        Assert.Equal(16, guideIcon.Width);
        Assert.Equal(16, guideIcon.Height);
        var guideRing = Assert.Single(
            Descendants(guideIcon).OfType<PathIcon>(),
            icon => Equals(icon.Tag, "GuideIconRing"));
        Assert.Equal(16, guideRing.Width);
        Assert.Equal(16, guideRing.Height);
        var guideGeometry = Assert.IsType<StreamGeometry>(guideRing.Data);
        Assert.False(guideGeometry.FillContains(new Point(12, 4)));
        Assert.True(guideGeometry.FillContains(new Point(1, 12)));
        var guideGlyph = Assert.Single(
            Descendants(guideIcon).OfType<TextBlock>(),
            text => Equals(text.Text, "?") && Equals(text.Tag, "NavIcon"));
        Assert.Equal(11, guideGlyph.FontSize);
        Assert.Equal(TextAlignment.Center, guideGlyph.TextAlignment);
        Assert.Equal(VerticalAlignment.Center, guideGlyph.VerticalAlignment);
        Assert.Equal(HorizontalAlignment.Stretch, guideButton.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Center, guideButton.VerticalAlignment);
        Assert.Equal(VerticalAlignment.Center, guideButton.VerticalContentAlignment);
        var bugReportButton = buttons.Single(button => Equals(button.Tag, "BugReportAction"));
        var bugReportIcon = Assert.Single(
            Descendants(bugReportButton).OfType<PathIcon>(),
            icon => Equals(icon.Tag, "BugReportIcon"));
        Assert.Equal(16, bugReportIcon.Width);
        Assert.Equal(16, bugReportIcon.Height);
        var bugReportGeometry = Assert.IsType<StreamGeometry>(bugReportIcon.Data);
        Assert.False(bugReportGeometry.FillContains(new Point(12, 4)));
        Assert.True(bugReportGeometry.FillContains(new Point(1, 12)));
        Assert.Equal(36, bugReportButton.Width);
        Assert.Equal(44, bugReportButton.Height);
        Assert.Equal("Report a bug", Avalonia.Automation.AutomationProperties.GetName(bugReportButton));
        Assert.Equal("Report a bug on GitHub", ToolTip.GetTip(bugReportButton)?.ToString());
        var githubButton = buttons.Single(button => Equals(button.Tag, "GitHubAction"));
        var githubIcon = Assert.Single(Descendants(githubButton).OfType<PathIcon>());
        Assert.Equal(githubIcon.Width, bugReportIcon.Width);
        Assert.Equal(githubIcon.Height, bugReportIcon.Height);
        Assert.Equal(new Thickness(0, 2, 0, 2), githubButton.Margin);
        var supportActionGrid = Descendants(sidebar)
            .OfType<Grid>()
            .Single(grid => grid.Children.OfType<Button>().Any(button => Equals(button.Tag, "BugReportAction")));
        Assert.Equal(4, supportActionGrid.ColumnDefinitions.Count);
        Assert.Single(supportActionGrid.RowDefinitions);
        Assert.Equal(48, supportActionGrid.RowDefinitions[0].Height.Value);
        Assert.Equal(1, Grid.GetColumn(bugReportButton));
        Assert.Equal(2, Grid.GetColumn(githubButton));
        var onboardingOverlay = Assert.IsType<Border>(body.Children[2]);
        Assert.Equal("OnboardingOverlay", onboardingOverlay.Tag);
        Assert.False(onboardingOverlay.IsVisible);
        Assert.Contains(
            Descendants(onboardingOverlay).OfType<TextBlock>(),
            text => Equals(text.Text, "Quick setup"));
        Assert.Contains(
            Descendants(onboardingOverlay).OfType<TextBlock>(),
            text => Equals(text.Text, "Connect your torrent client"));
        var interceptionCoachmark = body.Children
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "InterceptionOnboardingCoachmark"));
        Assert.False(interceptionCoachmark.IsVisible);
        Assert.Contains(
            Descendants(interceptionCoachmark).OfType<TextBlock>(),
            text => Equals(text.Text, "How to use Interception"));
        Assert.Contains(
            Descendants(interceptionCoachmark).OfType<Button>(),
            button => Equals(button.Tag, "InterceptionCoachmarkDone") &&
                      Equals(button.Content, "Got it"));
        Assert.Equal(new CornerRadius(18), interceptionCoachmark.CornerRadius);
        var simulationCoachmark = body.Children
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationOnboardingCoachmark"));
        Assert.False(simulationCoachmark.IsVisible);
        Assert.Equal(new CornerRadius(18), simulationCoachmark.CornerRadius);
        Assert.Contains(
            Descendants(simulationCoachmark).OfType<TextBlock>(),
            text => Equals(text.Text, "How to use Simulation"));
        Assert.Contains(
            Descendants(simulationCoachmark).OfType<Button>(),
            button => Equals(button.Tag, "SimulationCoachmarkDone") &&
                      Equals(button.Content, "Got it"));
        Assert.Contains(
            Descendants(onboardingOverlay).OfType<Button>(),
            button => Equals(button.Tag, "OnboardingPrevious") &&
                      (button.Content?.ToString() ?? string.Empty).Contains("←", StringComparison.Ordinal));
        Assert.Contains(
            Descendants(onboardingOverlay).OfType<Button>(),
            button => Equals(button.Tag, "OnboardingNext") &&
                      (button.Content?.ToString() ?? string.Empty).Contains("→", StringComparison.Ordinal));
        Assert.Contains(
            Descendants(onboardingOverlay).OfType<Button>(),
            button => Equals(button.Tag, "OnboardingMarkDone") &&
                      Equals(button.Content, "✓"));
        var simulation = Assert.IsType<Border>(tabItems[2].Content);
        var simulationControls = Descendants(simulation).ToHashSet();
        Assert.All(
            Descendants(root).OfType<TextBox>().Where(input => !simulationControls.Contains(input)),
            input => Assert.True(input.MinHeight >= 36));
        Assert.All(
            Descendants(root).OfType<CheckBox>().Where(checkBox => !simulationControls.Contains(checkBox)),
            checkBox => Assert.True(checkBox.MinHeight >= 36));
        Assert.All(
            Descendants(simulation).Where(control => control is TextBox or ComboBox or CheckBox),
            control => Assert.True(control.MinHeight >= 32));
        Assert.True(ContainsText(simulation, "Torrent file"));
        Assert.True(ContainsText(simulation, "Show full path and tracker URL"));
        Assert.Contains(
            Descendants(simulation).OfType<Button>().Select(button => button.Content),
            content => Equals(content, "Browse…"));
        Assert.True(ContainsText(simulation, "No simulation sessions"));
        Assert.True(ContainsText(simulation, "Torrent info"));
        Assert.True(ContainsText(simulation, "Account"));
        Assert.True(ContainsText(simulation, "Speed options"));
        Assert.True(ContainsText(simulation, "Upload speed (kB/s)"));
        Assert.True(ContainsText(simulation, "Download speed (kB/s)"));
        Assert.True(ContainsText(simulation, "Options"));
        Assert.True(ContainsText(simulation, "Tracker identity"));
        Assert.True(ContainsText(simulation, "Outbound proxy"));
        Assert.Equal(
            2,
            Descendants(simulation).OfType<CheckBox>().Count(checkBox => Equals(checkBox.Content, "+ Random values")));
        Assert.All(
            Descendants(simulation).OfType<CheckBox>().Where(checkBox => Equals(checkBox.Content, "+ Random values")),
            checkBox => Assert.True(checkBox.IsChecked));
        var simulationValues = Descendants(simulation).OfType<TextBox>().Select(input => input.Text).ToArray();
        Assert.Contains("0", simulationValues);
        Assert.Contains("10000", simulationValues);
        Assert.Contains("2500", simulationValues);
        Assert.Contains("5000", simulationValues);
        Assert.Contains("15000", simulationValues);
        Assert.Contains("1000", simulationValues);
        Assert.DoesNotContain(
            Descendants(simulation).OfType<Border>(),
            border => Equals(border.Tag, "SimulationSessionsDivider"));
        var simulationSessionsHeader = Descendants(simulation)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationSessionsHeader"));
        Assert.Equal(new Thickness(0, 0, 0, 1), simulationSessionsHeader.BorderThickness);
        Assert.NotEqual(Brushes.Transparent, simulationSessionsHeader.BorderBrush);
        Assert.Equal(new CornerRadius(0), simulationSessionsHeader.CornerRadius);
        Assert.True(simulationSessionsHeader.ClipToBounds);
        Assert.Equal(42, simulationSessionsHeader.Height);
        Assert.Equal(42, simulationSessionsHeader.MinHeight);
        Assert.Equal(42, simulationSessionsHeader.MaxHeight);
        Assert.NotEqual(Brushes.Transparent, simulationSessionsHeader.Background);
        var simulationSessionsLabel = Descendants(simulationSessionsHeader)
            .OfType<TextBlock>()
            .Single(textBlock => Equals(textBlock.Tag, "SimulationSessionsLabel"));
        Assert.Equal("Simulation sessions", simulationSessionsLabel.Text);
        Assert.Equal(FontWeight.SemiBold, simulationSessionsLabel.FontWeight);
        Assert.NotNull(simulationSessionsLabel.Foreground);
        var simulationSessionsSurface = Descendants(simulation)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationSessionsSurface"));
        Assert.Equal(new Thickness(1, 1, 1, 0), simulationSessionsSurface.BorderThickness);
        Assert.NotEqual(Brushes.Transparent, simulationSessionsSurface.BorderBrush);
        Assert.Equal(new CornerRadius(0), simulationSessionsSurface.CornerRadius);
        Assert.Equal(new Thickness(0), simulationSessionsSurface.Margin);
        Assert.Equal(new Thickness(0), simulationSessionsSurface.Padding);
        Assert.True(simulationSessionsSurface.ClipToBounds);
        Assert.NotNull(simulationSessionsSurface.Background);
        var resizeSplitter = Descendants(simulationSessionsSurface)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationSessionsResizeSplitter"));
        Assert.NotNull(resizeSplitter.Cursor);
        var simulationSessionsListSurface = Descendants(simulationSessionsSurface)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationSessionsListSurface"));
        Assert.Equal(176, simulationSessionsListSurface.Height);
        Assert.Equal(176, simulationSessionsListSurface.MinHeight);
        Assert.Equal(176, simulationSessionsListSurface.MaxHeight);
        Assert.Equal(new Thickness(0), simulationSessionsListSurface.BorderThickness);
        Assert.Equal(Brushes.Transparent, simulationSessionsListSurface.BorderBrush);
        Assert.Equal(new CornerRadius(0), simulationSessionsListSurface.CornerRadius);
        var simulationSessionsBody = Descendants(simulationSessionsListSurface)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationSessionsBody"));
        Assert.Equal(new Thickness(16, 12), simulationSessionsBody.Padding);
        Assert.NotNull(simulationSessionsBody.Background);
        var simulationCommandFooter = Descendants(simulationSessionsSurface)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationSessionsCommandFooter"));
        Assert.Equal(new Thickness(0, 1, 0, 0), simulationCommandFooter.BorderThickness);
        Assert.NotEqual(Brushes.Transparent, simulationCommandFooter.BorderBrush);
        Assert.Equal(new CornerRadius(0), simulationCommandFooter.CornerRadius);
        Assert.NotNull(simulationCommandFooter.Background);
        Assert.Contains(
            Descendants(simulation).OfType<ComboBox>().Select(comboBox => comboBox.SelectedItem),
            selected => Equals(selected, "qBittorrent 5.2.3"));
        var stopSelector = Descendants(simulation)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.Contains("Timer", StringComparer.Ordinal));
        Assert.Equal("Never", stopSelector.SelectedItem);
        var stopValue = Descendants(simulation)
            .OfType<TextBox>()
            .Single(input => input.PlaceholderText == "Not used");
        var stopValueEditor = Descendants(simulation)
            .OfType<Grid>()
            .Single(grid => grid.Children.Contains(stopValue) &&
                            grid.Children.OfType<Border>().Any(border =>
                                Equals(border.Tag, "SimulationTimerUnitSelector")));
        Assert.False(stopValueEditor.IsVisible);
        var timerUnitSelector = Descendants(simulation)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SimulationTimerUnitSelector"));
        var timerMinutes = Descendants(timerUnitSelector)
            .OfType<ToggleButton>()
            .Single(button => Equals(button.Tag, "SimulationTimerMinutes"));
        var timerHours = Descendants(timerUnitSelector)
            .OfType<ToggleButton>()
            .Single(button => Equals(button.Tag, "SimulationTimerHours"));
        Assert.False(timerUnitSelector.IsVisible);
        Assert.True(timerMinutes.IsChecked);
        Assert.False(timerHours.IsChecked);
        Assert.Equal("Minutes", timerMinutes.Content);
        Assert.Equal("Hours", timerHours.Content);
        Assert.Equal(108, timerUnitSelector.Width);
        Assert.Equal(32, timerUnitSelector.Height);
        Assert.Equal(32, timerUnitSelector.MinHeight);
        Assert.All(
            new[] { timerMinutes, timerHours },
            timer =>
            {
                Assert.Equal(50, timer.Width);
                Assert.Equal(50, timer.MinWidth);
                Assert.Equal(26, timer.Height);
                Assert.Equal(26, timer.MinHeight);
            });
        Assert.True(ContainsText(simulation,
            "Leave Never selected for manual stopping, or choose a rule above to stop automatically."));
        stopSelector.SelectedIndex = 1;
        Assert.True(stopValueEditor.IsVisible);
        Assert.True(timerUnitSelector.IsVisible);
        Assert.Equal("Duration", stopValue.PlaceholderText);
        Assert.True(ContainsText(simulation,
            "Timer starts when Start is pressed and stops this session after the selected duration."));
        timerHours.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.False(timerMinutes.IsChecked);
        Assert.True(timerHours.IsChecked);
        stopSelector.SelectedIndex = 0;
        Assert.False(stopValueEditor.IsVisible);
        var simulationButtons = Descendants(simulation).OfType<Button>().ToArray();
        var actionGrid = Descendants(simulation)
            .OfType<Grid>()
            .Single(grid =>
                grid.Children.OfType<Button>().Count() == 3 &&
                grid.Children.OfType<Button>().Any(button => Equals(button.Content, "Manual update")));
        Assert.Equal(3, actionGrid.ColumnDefinitions.Count);
        Assert.Single(actionGrid.RowDefinitions);
        Assert.Contains(actionGrid.Children.OfType<Button>(), button => Equals(button.Content, "▶  Start"));
        Assert.DoesNotContain(actionGrid.Children.OfType<Button>(), button => Equals(button.Content, "Stop"));
        Assert.Equal(0, Grid.GetRow(simulationButtons.Single(button => Equals(button.Content, "Manual update"))));
        Assert.Equal(2, Grid.GetColumn(simulationButtons.Single(button => Equals(button.Content, "Remove…"))));
        var simulationCommandBar = Descendants(simulation)
            .OfType<Grid>()
            .Single(grid => Equals(grid.Tag, "SimulationCommandBar"));
        Assert.Equal(new Thickness(0), simulationCommandBar.Margin);
        Assert.Contains(
            Descendants(simulationSessionsSurface).OfType<Button>(),
            button => Equals(button.Content, "Add session"));

        var navigationScroll = Descendants(sidebar)
            .OfType<ScrollViewer>()
            .Single(viewer => Descendants(viewer)
                .OfType<StackPanel>()
                .Any(panel => Equals(panel.Tag, "OnboardingChecklist")));
        Assert.True(navigationScroll.ClipToBounds);
        root.Measure(new Size(980, 574));
        root.Arrange(new Rect(0, 0, 980, 574));
        var supportLabel = Descendants(sidebar)
            .OfType<TextBlock>()
            .Single(text => Equals(text.Text, "Support"));
        var supportTop = supportLabel.TranslatePoint(new Point(0, 0), root);
        var scrollBottom = navigationScroll.TranslatePoint(
            new Point(0, navigationScroll.Bounds.Height),
            root);
        Assert.NotNull(supportTop);
        Assert.NotNull(scrollBottom);
        Assert.True(
            supportTop.Value.Y >= scrollBottom.Value.Y,
            "Support must remain in its own fixed row below the scrollable onboarding area.");

        root.Measure(new Size(1280, 770));
        root.Arrange(new Rect(0, 0, 1280, 770));
        var navigationIcons = Descendants(sidebar)
            .OfType<TextBlock>()
            .Where(text => Equals(text.Tag, "NavIcon"))
            .ToArray();
        Assert.Equal(8, navigationIcons.Length);
        Assert.All(
            navigationIcons,
            icon =>
            {
                Assert.Equal(16, icon.Width);
                Assert.Equal(TextAlignment.Center, icon.TextAlignment);
                Assert.Equal(VerticalAlignment.Center, icon.VerticalAlignment);
            });
        var navigationRows = Descendants(sidebar)
            .OfType<Border>()
            .Where(border => Equals(border.Tag, "NavRow"))
            .ToArray();
        Assert.Equal(8, navigationRows.Length);
        Assert.All(
            navigationRows,
            row => Assert.Equal(HorizontalAlignment.Stretch, row.HorizontalAlignment));
        var onboardingNavRow = Descendants(onboardingNavigation)
            .OfType<Border>()
            .Single(row => Equals(row.Tag, "NavRow"));
        Assert.All(
            navigationRows.Where(row => !ReferenceEquals(row, onboardingNavRow)),
            row => Assert.Equal(new Thickness(12, 8, 12, 8), row.Padding));
        Assert.Equal(new Thickness(12, 6, 8, 6), onboardingNavRow.Padding);

        var supportActions = new[]
        {
            guideButton,
            bugReportButton,
            githubButton,
            Descendants(sidebar)
                .OfType<Button>()
                .Single(button => Equals(button.Tag, "UpdateAction"))
        };
        var updateIndicator = supportActions[^1];
        Assert.All(
            supportActions,
            action =>
            {
                Assert.Equal(44, action.MinHeight);
                Assert.Equal(new Thickness(0, 2, 0, 2), action.Margin);
            });
        Assert.Equal(VerticalAlignment.Center, guideButton.VerticalAlignment);
        Assert.Equal(VerticalAlignment.Center, guideButton.VerticalContentAlignment);
        Assert.All(
            supportActions.Skip(1),
            action =>
            {
                Assert.Equal(VerticalAlignment.Center, action.VerticalAlignment);
                Assert.Equal(VerticalAlignment.Center, action.VerticalContentAlignment);
            });
        Assert.All(
            supportActions.Skip(1),
            action => Assert.Equal(44, action.Height));

        tabs.SelectedIndex = 2;
        root.Measure(new Size(1280, 770));
        root.Arrange(new Rect(0, 0, 1280, 770));
        var sessionsTopLeft = simulationSessionsSurface.TranslatePoint(new Point(0, 0), simulation);
        var sessionsHeaderTopLeft = simulationSessionsHeader.TranslatePoint(new Point(0, 0), simulation);
        Assert.NotNull(sessionsHeaderTopLeft);
        Assert.NotNull(sessionsTopLeft);
        Assert.InRange(sessionsHeaderTopLeft.Value.Y - sessionsTopLeft.Value.Y, 0, 12);
        foreach (var action in simulationButtons.Where(button =>
                     Equals(button.Content, "Add session") ||
                     Equals(button.Content, "▶  Start") ||
                     Equals(button.Content, "Manual update") ||
                     Equals(button.Content, "Remove…")))
        {
            var bottomRight = action.TranslatePoint(
                new Point(action.Bounds.Width, action.Bounds.Height),
                root);
            Assert.NotNull(bottomRight);
            Assert.InRange(bottomRight.Value.X, 0, root.Bounds.Width);
            Assert.InRange(bottomRight.Value.Y, 0, root.Bounds.Height);
        }
        var guideBottomRight = guideButton.TranslatePoint(
            new Point(guideButton.Bounds.Width, guideButton.Bounds.Height),
            root);
        Assert.NotNull(guideBottomRight);
        Assert.InRange(guideBottomRight.Value.X, 0, root.Bounds.Width);
        Assert.InRange(guideBottomRight.Value.Y, 0, root.Bounds.Height);
        Assert.All(
            Descendants(simulation).OfType<ScrollViewer>(),
            viewer => Assert.NotEqual(
                Avalonia.Controls.Primitives.ScrollBarVisibility.Visible,
                viewer.HorizontalScrollBarVisibility));
        Assert.False(MainWindow.ShouldShowStopAction(SimulationState.Stopped));
        Assert.True(MainWindow.ShouldShowStopAction(SimulationState.Starting));
        Assert.True(MainWindow.ShouldShowStopAction(SimulationState.Running));
        Assert.False(MainWindow.ShouldShowStopAction(SimulationState.Stopping));
        Assert.Contains(
            "existing session is selected",
            MainWindow.ExistingSimulationFeedback,
            StringComparison.Ordinal);

        var overview = Assert.IsType<Border>(tabItems[0].Content);
        Assert.True(ContainsText(overview, "Current runtime status."));
        Assert.False(ContainsText(overview, "Proxy engine"));
        Assert.False(ContainsText(overview, "Simulator"));

        var activity = Assert.IsType<Border>(tabItems[3].Content);
        Assert.True(ContainsText(activity, "Time"));
        Assert.True(ContainsText(activity, "Level · source"));
        Assert.True(ContainsText(activity, "Event details"));

        var options = Assert.IsType<Grid>(tabItems[4].Content);
        var settingsCheckBoxes = Descendants(options)
            .OfType<CheckBox>()
            .Where(checkBox => !string.IsNullOrWhiteSpace(ToolTip.GetTip(checkBox)?.ToString()))
            .ToArray();
        Assert.NotEmpty(settingsCheckBoxes);
        foreach (var checkBox in settingsCheckBoxes)
        {
            checkBox.ApplyTemplate();
            Assert.Equal("Arrow", checkBox.Cursor?.ToString());
            var visuals = Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(checkBox).ToArray();
            Assert.Contains(
                visuals.OfType<Avalonia.Controls.Presenters.ContentPresenter>(),
                presenter => presenter.Cursor?.ToString() == "Help");
            Assert.DoesNotContain(
                visuals.OfType<Avalonia.Controls.Shapes.Path>(),
                path => path.Cursor?.ToString() == "Help");
        }
        Assert.True(ContainsText(options, "Replay the guided setup at any time. Your completed steps stay checked."));
        var onboardingStatus = Descendants(options)
            .OfType<TextBlock>()
            .Single(textBlock => Equals(textBlock.Text, "Loading onboarding…"));
        Assert.Equal(VerticalAlignment.Center, onboardingStatus.VerticalAlignment);
        Assert.Contains(
            Descendants(options).OfType<Button>(),
            button => Equals(button.Tag, "RestoreOnboarding") && Equals(button.Content, "Show onboarding again"));
        Assert.True(ContainsText(options, "Write redacted proxy debug log"));
        Assert.True(ContainsText(options, "Pretend to seed (completed torrents only)"));
        var pretendSeed = Descendants(options)
            .OfType<CheckBox>()
            .Single(checkBox => Equals(checkBox.Content, "Pretend to seed (completed torrents only)"));
        Assert.True(pretendSeed.IsChecked);
        Assert.Contains(
            "does not increase your ratio",
            ToolTip.GetTip(pretendSeed)?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
        Assert.True(ContainsText(options, "Color mode uses a red X when stopped and orange when paused; Monochrome keeps the whole icon neutral."));
        Assert.True(ContainsText(options,
            "Minimum values must not exceed maximum values. Changing these values affects tracker reporting; use Pause or Stop for temporary control."));
        var reportDownload = Descendants(options)
            .OfType<CheckBox>()
            .Single(checkBox => Equals(checkBox.Content, "Report download as zero"));
        Assert.True(reportDownload.IsChecked);
        Assert.False(reportDownload.IsEnabled);
        var proxyPort = Descendants(options)
            .OfType<TextBox>()
            .Single(textBox => (ToolTip.GetTip(textBox)?.ToString() ?? string.Empty)
                .Contains("localhost port", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            "localhost port",
            ToolTip.GetTip(proxyPort)?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
        var settingsTextBoxes = Descendants(options)
            .OfType<TextBox>()
            .Where(textBox => !string.IsNullOrWhiteSpace(ToolTip.GetTip(textBox)?.ToString()))
            .ToArray();
        Assert.NotEmpty(settingsTextBoxes);
        foreach (var textBox in settingsTextBoxes)
        {
            textBox.ApplyTemplate();
            Assert.Equal("Ibeam", textBox.Cursor?.ToString());
            Assert.DoesNotContain(
                Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(textBox)
                    .OfType<Avalonia.Input.InputElement>(),
                visual => visual.Cursor?.ToString() == "Help");
        }
        var proxyPortLabel = Descendants(options)
            .OfType<TextBlock>()
            .Single(textBlock => Equals(textBlock.Text, "HTTP proxy port"));
        Assert.Contains(
            "localhost port",
            ToolTip.GetTip(proxyPortLabel)?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(proxyPortLabel.Cursor);
        Assert.Contains(
            "redacted proxy diagnostics",
            ToolTip.GetTip(Descendants(options)
                .OfType<CheckBox>()
                .Single(checkBox => Equals(checkBox.Content, "Write redacted proxy debug log")))?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "retained for up to 7 days",
            ToolTip.GetTip(Descendants(options)
                .OfType<CheckBox>()
                .Single(checkBox => Equals(checkBox.Content, "Write redacted proxy debug log")))?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "rotated at 1 MiB",
            ToolTip.GetTip(Descendants(options)
                .OfType<CheckBox>()
                .Single(checkBox => Equals(checkBox.Content, "Write redacted proxy debug log")))?.ToString() ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
        Assert.True(ContainsText(options, "Loading settings…"));
        Assert.True(ContainsText(options, AppVersion.Display));
        Assert.Contains(
            Descendants(options).OfType<Button>().Select(button => button.Content),
            content => Equals(content, "Save changes"));
        Assert.Contains(
            Descendants(options).OfType<Button>(),
            button => Equals(button.Tag, "ResetSettings") && Equals(button.Content, "Reset to defaults"));
        var resetSettings = Descendants(options)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SettingsResetSection"));
        Assert.Contains(
            Descendants(resetSettings).OfType<Button>(),
            button => Equals(button.Tag, "ResetSettings"));
        var settingsActionBar = Descendants(options)
            .OfType<Border>()
            .Single(border => Equals(border.Tag, "SettingsActionBar"));
        Assert.DoesNotContain(
            Descendants(settingsActionBar).OfType<Button>(),
            button => Equals(button.Tag, "ResetSettings"));
        Assert.Contains(
            Descendants(options).OfType<Button>().Select(button => button.Content),
            content => Equals(content, "Check for updates"));
        Assert.Contains(
            Descendants(options).OfType<Button>().Select(button => button.Content),
            content => Equals(content, "Download update"));
        Assert.DoesNotContain(
            Descendants(options).OfType<Button>(),
            button => Equals(button.Tag, "UpdateAction"));
        var checkUpdatesOnStartup = Descendants(options)
            .OfType<CheckBox>()
            .Single(checkBox => Equals(checkBox.Tag, "CheckUpdatesOnStartup"));
        Assert.True(checkUpdatesOnStartup.IsChecked);
        Assert.Equal("Check for updates at startup", checkUpdatesOnStartup.Content);
        Assert.Equal(36, updateIndicator.Width);
        Assert.Equal(36, updateIndicator.MinWidth);
        Assert.Equal(44, updateIndicator.Height);
        Assert.Equal(44, updateIndicator.MinHeight);
        Assert.Equal(new CornerRadius(18), updateIndicator.CornerRadius);
        Assert.Equal(new Thickness(0, 2, 0, 2), updateIndicator.Margin);
        Assert.Equal(Brushes.Transparent, updateIndicator.Background);
        Assert.NotEqual(Brushes.Transparent, updateIndicator.BorderBrush);
        Assert.Equal(new Thickness(2), updateIndicator.BorderThickness);
        Assert.False(updateIndicator.IsVisible);
        Assert.Contains(
            Descendants(updateIndicator).OfType<PathIcon>(),
            icon => icon.Width == 17 && icon.Height == 17 &&
                    icon.HorizontalAlignment == HorizontalAlignment.Center &&
                    icon.VerticalAlignment == VerticalAlignment.Center &&
                    icon.Margin == new Thickness(0, -1, 0, 1));
        Assert.NotNull(updateIndicator.Template);
        Assert.NotNull(updateIndicator.Transitions);
        Assert.Contains(
            updateIndicator.Transitions!,
            transition => transition.GetType().Name == "DoubleTransition");
        Assert.True(
            MainWindow.ResolveUpdateIndicatorExpandedWidth("Mise à jour") >
            MainWindow.ResolveUpdateIndicatorExpandedWidth("Update"));
        Assert.InRange(
            MainWindow.ResolveUpdateIndicatorExpandedWidth("A very long translated update label"),
            88,
            128);
        Assert.Equal("Update", UiText.UpdateIndicatorLabel(UiText.English));
        Assert.Equal("Update", UiText.UpdateIndicatorLabel(UiText.French));
        Assert.Equal("Update", UiText.UpdateIndicatorLabel(UiText.Chinese));
        Assert.Equal(
            "Télécharger la nouvelle version",
            UiText.Translate("Download the new version", UiText.French));
        Assert.Equal(
            "Ouvrir le signalement dans le navigateur",
            UiText.Translate("Open bug report in browser", UiText.French));
        Assert.Equal(
            "Cette action va ouvrir le téléchargement vérifié de la mise à jour dans votre navigateur par défaut.",
            UiText.Translate(
                "This will open the verified update download in your default browser.",
                UiText.French));
        Assert.Equal(
            "Installer la mise à jour",
            UiText.Translate("Install update", UiText.French));
        var themeSelector = Descendants(options)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.SequenceEqual(ThemePalette.Options));
        Assert.Equal(0, themeSelector.SelectedIndex);
        var languageSelector = Descendants(options)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.SequenceEqual(UiText.LanguageLabels));
        Assert.Equal(0, languageSelector.SelectedIndex);
        var accentSelector = Descendants(options)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.SequenceEqual(AccentPalette.Options));
        Assert.Equal(0, accentSelector.SelectedIndex);
        var trayIconSelector = Descendants(options)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.SequenceEqual(MainWindow.TrayIconStyles));
        Assert.Equal(0, trayIconSelector.SelectedIndex);
        var selectors = new[] { themeSelector, languageSelector, accentSelector, trayIconSelector };
        foreach (var selector in selectors)
        {
            selector.ApplyTemplate();
            Assert.Equal("Arrow", selector.Cursor?.ToString());
            Assert.DoesNotContain(
                Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(selector)
                    .OfType<Avalonia.Input.InputElement>(),
                visual => visual.Cursor?.ToString() == "Help");
        }
        using var stopIconData = new MemoryStream();
        TrayIconRenderer.CreateStopIcon().Save(stopIconData);
        Assert.True(stopIconData.Length > 16);

        using var pauseIconData = new MemoryStream();
        TrayIconRenderer.CreatePauseIcon().Save(pauseIconData);
        Assert.True(pauseIconData.Length > 16);
        using var monochromeIconData = new MemoryStream();
        TrayIconRenderer.CreateMonochromeIcon().Save(monochromeIconData);
        Assert.True(monochromeIconData.Length > 16);
        using var updateBitmap = TrayIconRenderer.RenderIcon("#E5484D", updateAvailable: true);
        using var updateFramebuffer = updateBitmap.Lock();
        var blueBadgePixels = 0;
        for (var y = 0; y < updateFramebuffer.Size.Height; y++)
        {
            for (var x = 0; x < updateFramebuffer.Size.Width; x++)
            {
                var pixel = IntPtr.Add(updateFramebuffer.Address, y * updateFramebuffer.RowBytes + x * 4);
                var red = System.Runtime.InteropServices.Marshal.ReadByte(pixel);
                var green = System.Runtime.InteropServices.Marshal.ReadByte(IntPtr.Add(pixel, 1));
                var blue = System.Runtime.InteropServices.Marshal.ReadByte(IntPtr.Add(pixel, 2));
                if (blue > 180 && green > 80 && red < 80)
                    blueBadgePixels++;
            }
        }
        Assert.True(blueBadgePixels >= 20);
        Assert.Equal("Vue d’ensemble", UiText.TranslateMessage("Overview", UiText.French));
        Assert.Equal("Overview", UiText.TranslateMessage("Vue d’ensemble", UiText.English));
        Assert.Equal("Sombre doux", UiText.Translate("Dim", UiText.French));
        Assert.Equal("Sombre feutré", UiText.Translate("Soft Dark", UiText.French));
        Assert.Equal("Icône de notification", UiText.Translate("Tray icon", UiText.French));
        Assert.Equal("Couleur", UiText.Translate("Color", UiText.French));
        Assert.Equal("Afficher l’icône dans la zone de notification", UiText.Translate("Show icon in notification area", UiText.French));
        Assert.Equal("Vista general", UiText.Translate("Overview", UiText.Spanish));
        Assert.Equal("Übersicht", UiText.Translate("Overview", UiText.German));
        Assert.Equal("Panoramica", UiText.Translate("Overview", UiText.Italian));
        Assert.Equal("Visão geral", UiText.Translate("Overview", UiText.Portuguese));
        Assert.Equal("🇺🇸 English", UiText.LanguageLabels[0]);
        Assert.Equal("🇫🇷 Français", UiText.LanguageLabels[1]);
        Assert.Equal("US", UiText.FlagCodeAt(0));
        Assert.Equal("FR", UiText.FlagCodeAt(1));
        Assert.Equal("English", UiText.DisplayNameAt(0));
        Assert.Equal("Français", UiText.DisplayNameAt(1));
        Assert.Equal(1, UiText.LanguageIndex("🇫🇷 Français"));
        Assert.Equal(
            "HTTP/HTTPS actif sur 127.0.0.1:3773",
            UiText.TranslateMessage("HTTP/HTTPS active on 127.0.0.1:3773", UiText.French));
        Assert.Equal(
            "HTTP/HTTPS active on 127.0.0.1:3773",
            UiText.TranslateMessage("HTTP/HTTPS actif sur 127.0.0.1:3773", UiText.English));
        languageSelector.SelectedIndex = 1;
        Assert.True(ContainsText(root, "Vue d’ensemble"));
        Assert.Equal("🇫🇷 Français", languageSelector.SelectedItem);
        languageSelector.SelectedIndex = 2;
        Assert.True(ContainsText(root, "Vista general"));
        Assert.Equal("🇪🇸 Español", languageSelector.SelectedItem);
        languageSelector.SelectedIndex = 0;
        Assert.True(ContainsText(root, "Overview"));
        Assert.Equal("🇺🇸 English", languageSelector.SelectedItem);

        var platform = Assert.IsType<ScrollViewer>(tabItems[5].Content);
        Assert.True(ContainsText(platform, "Start automatically with the user session"));
        Assert.True(ContainsText(platform, "Show icon in notification area"));
        var platformButtons = Descendants(platform)
            .OfType<Button>()
            .Select(button => button.Content)
            .ToArray();
        Assert.Contains("Trust CA and enable", platformButtons);
        Assert.Contains("Remove CA trust…", platformButtons);

        Assert.Equal("…\\sample.torrent", MainWindow.MaskLocalPath(@"C:\Users\Person\sample.torrent"));
        Assert.Equal(
            "https://tracker.example/announce/••••••••",
            MainWindow.MaskTrackerUrl("https://tracker.example/announce/a5cec0505c4f7fc403964aac30f80b32"));
        Assert.Equal(
            "https://tracker.example/announce",
            MainWindow.MaskTrackerUrl("https://tracker.example/announce?passkey=a5cec0505c4f7fc403964aac30f80b32"));
        Assert.Equal(MainWindow.ActivityLevel.Error, MainWindow.InferActivityLevel("Startup error: port busy"));
        Assert.Equal("Startup", MainWindow.InferActivitySource("Startup error: port busy"));
        Assert.Contains(
            "Port 3773 is already in use",
            MainWindow.DescribeStartupFailure(
                new SocketException((int)SocketError.AddressAlreadyInUse),
                3773));
        Assert.Throws<ArgumentException>(() => MainWindow.ValidateSettingsRanges(new XRatioSettings
            {
                UploadPerDownloadMinimum = 2,
                UploadPerDownloadMaximum = 1
            }));

        var trayMenu = App.BuildTrayMenu(window);
        var trayItems = trayMenu.Items.ToArray();
        Assert.Equal(4, trayItems.Length);
        Assert.Equal("Show XRatio", Assert.IsType<NativeMenuItem>(trayItems[0]).Header);
        Assert.Equal("Pause / resume rewriting", Assert.IsType<NativeMenuItem>(trayItems[1]).Header);
        Assert.IsType<NativeMenuItemSeparator>(trayItems[2]);
        Assert.Equal("Exit", Assert.IsType<NativeMenuItem>(trayItems[3]).Header);

        languageSelector.SelectedIndex = 2;
        Assert.Equal("Onboarding", onboardingSidebarLabel.Text);
        languageSelector.SelectedIndex = 0;
    }

    [Fact]
    public void DismissedOnboarding_UpdatesReplayStatusInsteadOfStayingInLoadingState()
    {
        if (!OperatingSystem.IsWindows())
            return;

        EnsureAvaloniaSetup();

        var window = new MainWindow(
            new InMemorySettingsStore(),
            new TestAutostartService(),
            new TestCertificateAuthorityService(),
            shutdown: static () => { });
        var settingsLoaded = typeof(MainWindow).GetField(
            "_settingsLoaded",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var settings = typeof(MainWindow).GetField(
            "_settings",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        settingsLoaded.SetValue(window, true);
        settings.SetValue(window, new XRatioSettings { OnboardingDismissed = true });
        typeof(MainWindow)
            .GetMethod("RefreshOnboarding", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(window, null);

        var root = Assert.IsType<Grid>(window.Content);
        var body = Assert.IsType<Grid>(root.Children[1]);
        var options = Assert.IsType<TabControl>(body.Children[1])
            .Items
            .Cast<TabItem>()
            .Single(item => Equals(item.Tag, "Settings"));
        var status = Descendants(Assert.IsAssignableFrom<Control>(options.Content))
            .OfType<TextBlock>()
            .Single(text => (text.Text ?? string.Empty)
                .Contains("hidden", StringComparison.OrdinalIgnoreCase));
        var replay = Descendants(Assert.IsAssignableFrom<Control>(options.Content))
            .OfType<Button>()
            .Single(button => Equals(button.Tag, "RestoreOnboarding"));

        Assert.Contains("hidden", status.Text, StringComparison.OrdinalIgnoreCase);
        Assert.True(replay.IsEnabled);
    }

    [Fact]
    public void TrayHoverAndMenuStringsHaveTranslationsForEverySupportedLanguage()
    {
        var trayKeys = new[]
        {
            "XRatio — OFF",
            "XRatio — ON",
            "XRatio — ON (paused)",
            "Show XRatio",
            "Pause / resume rewriting",
            "Exit"
        };

        foreach (var language in UiText.LanguageCodes)
        {
            foreach (var key in trayKeys)
            {
                var translated = UiText.Translate(key, language);
                Assert.False(string.IsNullOrWhiteSpace(translated), $"Missing tray text for {language}: {key}");
                if (!string.Equals(language, UiText.English, StringComparison.Ordinal))
                    Assert.NotEqual(key, translated);
            }
        }
    }

    [Fact]
    public void GuideTitlesAndDynamicHoverMessagesLocalizeOutsideEnglish()
    {
        const string guideTitle = "XRatio Guide · Overview guide";
        const string frenchGuideTitle = "Guide XRatio · Vue d’ensemble guide";

        foreach (var language in UiText.LanguageCodes.Where(language => language != UiText.English))
        {
            var translated = UiText.TranslateMessage(guideTitle, language);
            Assert.NotEqual(guideTitle, translated);
            Assert.Contains("XRatio", translated, StringComparison.Ordinal);
        }

        Assert.NotEqual(
            frenchGuideTitle,
            UiText.TranslateMessage(frenchGuideTitle, UiText.Spanish));
        Assert.NotEqual(
            "Update available: v1.2.3",
            UiText.TranslateMessage("Update available: v1.2.3", UiText.German));
    }

    [Fact]
    public void RuntimeStatusBadgeLocalizesItsDynamicPortForEveryLanguage()
    {
        const string activeStatus = "HTTP/HTTPS active on 127.0.0.1:3773";
        const string httpOnlyStatus = "HTTP active on 127.0.0.1:3773";
        const string pausedStatus = "Paused on 127.0.0.1:3773";

        foreach (var language in UiText.LanguageCodes.Where(language => language != UiText.English))
        {
            var active = UiText.TranslateMessage(activeStatus, language);
            var httpOnly = UiText.TranslateMessage(httpOnlyStatus, language);
            var paused = UiText.TranslateMessage(pausedStatus, language);

            Assert.NotEqual(activeStatus, active);
            Assert.NotEqual(httpOnlyStatus, httpOnly);
            Assert.NotEqual(pausedStatus, paused);
            Assert.Contains("127.0.0.1:3773", active, StringComparison.Ordinal);
            Assert.Contains("127.0.0.1:3773", httpOnly, StringComparison.Ordinal);
            Assert.Contains("127.0.0.1:3773", paused, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PointerPressedOutsideFocusableControlClearsFocus()
    {
        var root = new Grid();
        var input = new TextBox();
        var toggle = new CheckBox();
        var selector = new ComboBox();
        var label = new TextBlock();
        root.Children.Add(input);
        root.Children.Add(toggle);
        root.Children.Add(selector);
        root.Children.Add(label);

        Assert.False(MainWindow.ShouldClearFocusForPointer(input, root));
        Assert.False(MainWindow.ShouldClearFocusForPointer(toggle, root));
        Assert.False(MainWindow.ShouldClearFocusForPointer(selector, root));
        Assert.True(MainWindow.ShouldClearFocusForPointer(label, root));
    }

    [Fact]
    public void SimulationCompletedPercent_DefaultsToZeroWithoutErasingExplicitOverrides()
    {
        Assert.Equal(
            "0",
            MainWindow.ResolveSimulationCompletedPercent(new SimulationFormSettings()));
        Assert.Equal(
            "0",
            MainWindow.ResolveSimulationCompletedPercent(new SimulationFormSettings
            {
                CompletedPercent = "100"
            }));
        Assert.Equal(
            "37.5",
            MainWindow.ResolveSimulationCompletedPercent(new SimulationFormSettings
            {
                CompletedPercent = "37.5"
            }));
        Assert.Equal(
            "100",
            MainWindow.ResolveSimulationCompletedPercent(new SimulationFormSettings
            {
                CompletedPercent = "100",
                CompletedPercentCustomized = true
            }));
    }

    [Fact]
    public void SimulationTimerDuration_UsesSelectedUnit()
    {
        Assert.Equal(TimeSpan.FromMinutes(90), MainWindow.ResolveSimulationTimerDuration(90, "Minutes"));
        Assert.Equal(TimeSpan.FromHours(2), MainWindow.ResolveSimulationTimerDuration(2, "Hours"));
        Assert.Equal(TimeSpan.FromMinutes(2), MainWindow.ResolveSimulationTimerDuration(2, "unknown"));
    }

    private static bool ContainsText(Control root, string text) =>
        Descendants(root).OfType<TextBlock>().Any(block => Equals(block.Text, text)) ||
        Descendants(root).OfType<CheckBox>().Any(checkBox => Equals(checkBox.Content, text));

    private static readonly object AvaloniaSetupGate = new();
    private static bool _avaloniaSetup;

    private static void EnsureAvaloniaSetup()
    {
        lock (AvaloniaSetupGate)
        {
            if (_avaloniaSetup)
                return;
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .SetupWithoutStarting();
            _avaloniaSetup = true;
        }
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        yield return root;

        if (root is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                foreach (var descendant in Descendants(child))
                    yield return descendant;
            }
        }

        if (root is ContentControl contentControl && contentControl.Content is Control content)
        {
            foreach (var descendant in Descendants(content))
                yield return descendant;
        }

        if (root is Decorator decorator && decorator.Child is Control decoratedChild)
        {
            foreach (var descendant in Descendants(decoratedChild))
                yield return descendant;
        }

        if (root is ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items.OfType<Control>())
            {
                foreach (var descendant in Descendants(item))
                    yield return descendant;
            }
        }
    }

    private sealed class InMemorySettingsStore : ISettingsStore
    {
        public SettingsLoadSource LastLoadSource => SettingsLoadSource.Defaults;

        public Task<XRatioSettings> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new XRatioSettings());

        public Task SaveAsync(
            XRatioSettings settings,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestAutostartService : IAutostartService
    {
        public PlatformCapability Capability { get; } = new(true, "Test autostart");

        public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task SetEnabledAsync(
            bool enabled,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestCertificateAuthorityService : ICertificateAuthorityService
    {
        public PlatformCapability Capability { get; } = new(false, "Test certificate service");

        public Task<bool> IsTrustedAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task RequestTrustAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RemoveTrustAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<System.Security.Cryptography.X509Certificates.X509Certificate2> GetServerCertificateAsync(
            string host,
            CancellationToken cancellationToken = default) =>
            throw new PlatformNotSupportedException();
    }
}

