using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Layout;
using System.Net.Sockets;
using XRatio.Core.Configuration;
using XRatio.Core.Platform;
using XRatio.Core.Simulation;
using XRatio.Desktop;

namespace XRatio.Desktop.Tests;

public sealed class MainWindowSurfaceTests
{
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
        Assert.Equal(UiText.French, new XRatioSettings().Language);
        Assert.Equal("Light", new XRatioSettings().ThemeMode);
        Assert.Equal("Blue", new XRatioSettings().AccentColor);

        if (!OperatingSystem.IsWindows())
            return;

        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .SetupWithoutStarting();

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
        Assert.Equal(216, sidebar.Width);
        var tabs = Assert.IsType<TabControl>(body.Children[1]);
        var tabItems = tabs.Items.Cast<TabItem>().ToArray();
        Assert.Equal(
            ["Overview", "Interception", "Simulation", "Activity", "Settings", "Platform"],
            tabItems.Select(item => Assert.IsType<string>(item.Tag)).ToArray());
        Assert.All(tabItems, item => Assert.True(item.MinHeight >= 40));
        Assert.All(tabItems, item => Assert.Equal(44, item.MinHeight));
        Assert.All(
            tabItems,
            item => Assert.DoesNotContain(
                Descendants(Assert.IsAssignableFrom<Control>(item.Header)).OfType<TextBlock>(),
                text => Equals(text.Tag, "NavSection")));

        var buttons = Descendants(root).OfType<Button>().ToArray();
        Assert.NotEmpty(buttons);
        Assert.All(buttons.Where(button => button is not CheckBox), button => Assert.True(button.MinHeight >= 36));
        var guideButton = buttons.Single(button => Equals(button.Tag, "GuideAction"));
        Assert.Same(guideButton, body.Children[2]);
        Assert.Equal("\uE897", guideButton.Content);
        Assert.Equal(HorizontalAlignment.Left, guideButton.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Bottom, guideButton.VerticalAlignment);
        Assert.Equal(new Thickness(16, 0, 0, 14), guideButton.Margin);
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
        Assert.Contains("50000", simulationValues);
        Assert.Contains("5000", simulationValues);
        Assert.Contains("10000", simulationValues);
        Assert.Contains("12500", simulationValues);
        Assert.Contains(
            Descendants(simulation).OfType<ComboBox>().Select(comboBox => comboBox.SelectedItem),
            selected => Equals(selected, "qBittorrent 5.2.3"));
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

        tabs.SelectedIndex = 2;
        root.Measure(new Size(1280, 770));
        root.Arrange(new Rect(0, 0, 1280, 770));
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
        Assert.True(ContainsText(options, "Write redacted proxy debug log"));
        Assert.True(ContainsText(options, "Loading settings…"));
        Assert.Contains(
            Descendants(options).OfType<Button>().Select(button => button.Content),
            content => Equals(content, "Save changes"));
        var themeSelector = Descendants(options)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.SequenceEqual(["Light", "Dim", "Dark"]));
        Assert.Equal(0, themeSelector.SelectedIndex);
        var languageSelector = Descendants(options)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.SequenceEqual([UiText.English, UiText.French]));
        Assert.Equal(0, languageSelector.SelectedIndex);
        var accentSelector = Descendants(options)
            .OfType<ComboBox>()
            .Single(comboBox => comboBox.ItemsSource is IEnumerable<string> values &&
                                values.SequenceEqual(["Blue", "Teal"]));
        Assert.Equal(0, accentSelector.SelectedIndex);
        Assert.Equal("Vue d’ensemble", UiText.TranslateMessage("Overview", UiText.French));
        Assert.Equal("Overview", UiText.TranslateMessage("Vue d’ensemble", UiText.English));
        Assert.Equal("Sombre doux", UiText.Translate("Dim", UiText.French));
        Assert.Equal(
            "HTTP/HTTPS actif sur 127.0.0.1:3773",
            UiText.TranslateMessage("HTTP/HTTPS active on 127.0.0.1:3773", UiText.French));
        Assert.Equal(
            "HTTP/HTTPS active on 127.0.0.1:3773",
            UiText.TranslateMessage("HTTP/HTTPS actif sur 127.0.0.1:3773", UiText.English));
        languageSelector.SelectedIndex = 1;
        Assert.True(ContainsText(root, "Vue d’ensemble"));
        Assert.Equal("Français", languageSelector.SelectedItem);
        languageSelector.SelectedIndex = 0;
        Assert.True(ContainsText(root, "Overview"));
        Assert.Equal("English", languageSelector.SelectedItem);

        var platform = Assert.IsType<ScrollViewer>(tabItems[5].Content);
        Assert.True(ContainsText(platform, "Start automatically with the user session"));
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
    }

    private static bool ContainsText(Control root, string text) =>
        Descendants(root).OfType<TextBlock>().Any(block => Equals(block.Text, text)) ||
        Descendants(root).OfType<CheckBox>().Any(checkBox => Equals(checkBox.Content, text));

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

