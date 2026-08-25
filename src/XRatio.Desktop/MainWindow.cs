using System.Globalization;
using System.Net.Sockets;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using XRatio.Core.Announcements;
using XRatio.Core.Configuration;
using XRatio.Core.Platform;
using XRatio.Core.Simulation;
using XRatio.Core.Torrents;
using XRatio.Proxy;

namespace XRatio.Desktop;

/*
 * IMPECCABLE DIRECTION · seed 17816432 · assigned direction 4
 * THESIS: XRatio is a local ratio control plane; the first viewport reads like an instrument, not a card dashboard.
 * OWN-WORLD: spectral observation console — ruled surfaces, restrained cyan/green signals, compact humanist UI type, and data-only tabular numerals.
 * STORY: confirm the local proxy channel, distinguish interception from simulation, then move directly to the relevant control surface.
 * FIRST VIEWPORT: a two-column runtime readout with one large proxy channel, operating modes, explicit failure recovery, and a quiet trust note.
 * FORM: operate / observation console, assigned direction 4, concept seed 17816432.
 * FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance.
 */
public sealed class MainWindow : Window
{
    private const string RepositoryUrl = "https://github.com/Mac-Cipher/XRatio";

    private readonly ISettingsStore _store;
    private readonly IAutostartService _autostart;
    private readonly ICertificateAuthorityService _certificates;
    private readonly IProxyDebugLogger? _debugLogger;
    private readonly SimulationSessionStore? _simulationStore;
    private readonly Action _shutdown;
    private readonly AnnounceTransformer _transformer = new();
    private readonly SemaphoreSlim _settingsSaveGate = new(1, 1);
    private readonly ListBox _activity = new();
    private readonly TabControl _tabs = new();
    private readonly ContentPresenter _tabContentHost = new();
    private readonly List<(Button Button, Border Row)> _navigationItems = [];
    private readonly ListBox _torrents = new();
    private readonly ListBox _simulations = new();
    private readonly StackPanel _torrentsEmptyState = new();
    private readonly StackPanel _simulationsEmptyState = new();
    private readonly Grid _simulationActions = new();
    private readonly Button _simulationPrimaryAction = new();
    private readonly Button _simulationUpdateAction = new();
    private readonly Button _simulationRemoveAction = new();
    private readonly Button _simulationAddAction = new();
    private readonly TextBlock _simulationAddFeedback = new();
    private readonly TextBlock _status = new();
    private readonly Border _statusIndicator = new();
    private readonly Border _startupFailureBanner = new();
    private readonly TextBlock _startupFailureTitle = new();
    private readonly TextBlock _startupFailureDetail = new();
    private readonly Button _toggle = new();
    private readonly Button _pause = new();
    private readonly Button _hide = new();
    private readonly TextBox _port = new();
    private readonly TextBox _minimumPeers = new();
    private readonly TextBox _downloadRatioMin = new();
    private readonly TextBox _downloadRatioMax = new();
    private readonly TextBox _uploadRatioMin = new();
    private readonly TextBox _uploadRatioMax = new();
    private readonly TextBox _boost = new();
    private readonly TextBox _boostChance = new();
    private readonly CheckBox _onlyTrackers = new();
    private readonly CheckBox _onlyLocal = new();
    private readonly CheckBox _proxyDebugLogging = new();
    private readonly CheckBox _noDownload = new();
    private readonly CheckBox _pretendSeed = new();
    private readonly CheckBox _autoStart = new();
    private readonly CheckBox _startMinimized = new();
    private readonly CheckBox _showTrayIcon = new();
    private readonly CheckBox _certificateConsent = new();
    private readonly TextBlock _certificateStatus = new();
    private readonly TextBlock _certificateStatusDetail = new();
    private readonly Button _trustCertificate = new();
    private readonly Button _removeCertificate = new();
    private readonly Button _settingsSaveAction = new();
    private readonly TextBlock _settingsSaveStatus = new();
    private readonly Button _checkUpdates = new();
    private readonly Button _downloadUpdate = new();
    private readonly TextBlock _updateStatus = new();
    private readonly ComboBox _themeMode = new();
    private readonly ComboBox _accentColor = new();
    private readonly ComboBox _trayIconStyle = new();
    private readonly ComboBox _languageMode = new();
    private readonly TextBlock _overviewProxyKpi = new();
    private readonly TextBlock _overviewTorrentKpi = new();
    private readonly TextBlock _overviewSimulationKpi = new();
    private readonly TextBlock _overviewReportedKpi = new();
    private readonly TextBox _torrentPath = new();
    private readonly CheckBox _simulationRevealPrivateValues = new();
    private readonly TextBox _simulationAccountName = new();
    private readonly ComboBox _simulationTracker = new();
    private readonly ComboBox _simulationClient = new();
    private readonly ComboBox _simulationStopMode = new();
    private readonly TextBox _simulationInfoHash = new();
    private readonly TextBox _simulationInfoSize = new();
    private readonly TextBox _simulationUploadRate = new();
    private readonly TextBox _simulationDownloadRate = new();
    private readonly TextBox _simulationCompleted = new();
    private readonly CheckBox _simulationRandomUpload = new();
    private readonly TextBox _simulationRandomUploadMin = new();
    private readonly TextBox _simulationRandomUploadMax = new();
    private readonly CheckBox _simulationRandomDownload = new();
    private readonly TextBox _simulationRandomDownloadMin = new();
    private readonly TextBox _simulationRandomDownloadMax = new();
    private readonly TextBox _simulationPort = new();
    private readonly TextBox _simulationNumWant = new();
    private readonly TextBox _simulationAnnounceInterval = new();
    private readonly TextBox _simulationStopValue = new();
    private readonly TextBox _simulationProxyAddress = new();
    private readonly TextBox _simulationProxyUsername = new();
    private readonly Dictionary<Guid, SimulationEntry> _simulationEntries = [];
    private readonly SemaphoreSlim _updateCheckGate = new(1, 1);
    private TorrentMetadata? _pendingTorrent;
    private XRatioSettings _settings = new();
    private string _language = UiText.English;
    private HttpProxyServer? _proxy;
    private bool _exiting;
    private bool _paused;
    private bool _sessionPersisted;
    private bool _startupInitializationStarted;
    private bool _settingsLoaded;
    private bool _restoringSimulationForm;
    private bool _suppressSettingsDirty;
    private bool _settingsDirty;
    private bool _restoreRequested;
    private bool _suppressLanguageSelection;
    private Uri? _latestReleaseUri;
    private Uri? _latestDownloadUri;
    private int _torrentPersistenceRequested;
    private int _torrentPersistenceWriterRunning;
    private CancellationTokenSource? _simulationFormSaveCancellation;
    private DateTimeOffset _sessionStarted;

    internal static readonly IReadOnlyList<string> TrayIconStyles = ["Color", "Monochrome"];

    public MainWindow(
        ISettingsStore store,
        IAutostartService autostart,
        ICertificateAuthorityService certificates,
        Action shutdown,
        IProxyDebugLogger? debugLogger = null,
        SimulationSessionStore? simulationStore = null)
    {
        _store = store;
        _autostart = autostart;
        _certificates = certificates;
        _debugLogger = debugLogger;
        _simulationStore = simulationStore;
        _shutdown = shutdown;
        Title = ResolveWindowTitle(OperatingSystem.IsWindows());
        Width = 1280;
        Height = 800;
        MinWidth = 980;
        MinHeight = 640;
        Background = XRatioPalette.Canvas;
        FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI");
        Icon = App.CreateAppIcon();
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Content = BuildContent();
        AddHandler(
            InputElement.PointerPressedEvent,
            OnWindowPointerPressed,
            RoutingStrategies.Tunnel);
        Closing += OnClosing;
        Opened += OnOpened;
    }

    internal static string ResolveWindowTitle(bool isWindows) => "XRatio";

    internal static string NormalizeThemeMode(string? themeMode) =>
        ThemePalette.Normalize(themeMode);

    internal static string NormalizeAccentColor(string? accentColor) =>
        AccentPalette.Normalize(accentColor);

    internal static string NormalizeTrayIconStyle(string? trayIconStyle) =>
        string.Equals(trayIconStyle?.Trim(), "Monochrome", StringComparison.OrdinalIgnoreCase)
            ? "Monochrome"
            : "Color";

    internal const string ExistingSimulationFeedback =
        "Already added — the existing session is selected.";

    internal static double ResolveSimulationTabsMaxHeight(double windowHeight) =>
        Math.Max(280, windowHeight - 308);

    internal bool IsProxyRunning => _proxy?.IsRunning == true;

    internal bool IsProxyPaused => _paused;

    internal bool IsTrayIconEnabled => IsTrayAvailable() && _settings.ShowTrayIcon;

    internal bool UseMonochromeTrayIcon =>
        string.Equals(_settings.TrayIconStyle, "Monochrome", StringComparison.Ordinal);

    internal event Action<bool, bool>? RuntimeStateChanged;

    private void OnWindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!ShouldClearFocusForPointer(e.Source as Visual, this))
            return;

        FocusManager?.Focus(null, NavigationMethod.Pointer, e.KeyModifiers);
    }

    internal static bool ShouldClearFocusForPointer(Visual? source, Visual root)
    {
        for (var current = source; current is not null; current = current.GetVisualParent())
        {
            if (ReferenceEquals(current, root))
                return true;

            if (current is IInputElement input &&
                input.Focusable &&
                input.IsEnabled &&
                input.IsHitTestVisible &&
                IsInteractiveFocusControl(current))
                return false;
        }

        return false;
    }

    private static bool IsInteractiveFocusControl(Visual visual) =>
        visual is TextBox or
            ComboBox or
            ComboBoxItem or
            CheckBox or
            ToggleButton or
            Button or
            ListBoxItem or
            TabItem or
            MenuItem;

    internal static bool ShouldStartMinimized(
        bool trayAvailable,
        bool startMinimizedSetting,
        bool minimizedCommandLine) =>
        trayAvailable && (startMinimizedSetting || minimizedCommandLine);

    internal static bool ShouldHideAfterStartup(
        bool trayAvailable,
        bool startMinimizedSetting,
        bool minimizedCommandLine,
        bool restoreRequested) =>
        !restoreRequested && ShouldStartMinimized(
            trayAvailable,
            startMinimizedSetting,
            minimizedCommandLine);

    internal static bool ShouldHideOnWindowClose(bool trayAvailable) => trayAvailable;

    internal static string MaskLocalPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;
        var fileName = Path.GetFileName(path);
        return string.IsNullOrWhiteSpace(fileName) ? "Selected torrent" : $"…\\{fileName}";
    }

    internal static string MaskTrackerUrl(string? tracker)
    {
        if (!Uri.TryCreate(tracker, UriKind.Absolute, out var uri))
            return tracker ?? string.Empty;
        var builder = new UriBuilder(uri)
        {
            UserName = string.Empty,
            Password = string.Empty,
            Query = string.Empty,
            Fragment = string.Empty
        };
        var segments = builder.Path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 1 && LooksSensitive(segments[^1]))
            builder.Path = "/" + string.Join('/', segments[..^1]) + "/••••••••";
        return builder.Uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.Unescaped);
    }

    internal static string DescribeStartupFailure(Exception exception, int port)
    {
        var socket = FindSocketException(exception);
        return socket?.SocketErrorCode == SocketError.AddressAlreadyInUse ||
               exception.Message.Contains("socket", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("address", StringComparison.OrdinalIgnoreCase)
            ? $"Port {port} is already in use. Interception is stopped until you choose a free port or close the other listener."
            : $"Interception is stopped. {exception.Message}";
    }

    private static bool LooksSensitive(string value) =>
        value.Length >= 20 && value.All(char.IsLetterOrDigit);

    private static SocketException? FindSocketException(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is SocketException socket)
                return socket;
            exception = exception.InnerException;
        }
        return null;
    }

    private Control BuildContent()
    {
        _status.Text = "Loading configuration…";
        _status.Foreground = XRatioPalette.Muted;
        _status.FontSize = 12;
        _status.TextTrimming = TextTrimming.CharacterEllipsis;
        _status.MaxWidth = 250;
        _status.VerticalAlignment = VerticalAlignment.Center;
        StyleButton(_toggle, ButtonTone.Primary, minWidth: 72);
        _toggle.Content = "Start";
        _toggle.Click += async (_, _) => await ToggleProxyAsync();
        StyleButton(_pause, ButtonTone.Secondary, minWidth: 72);
        _pause.Content = "Pause";
        _pause.IsEnabled = false;
        _pause.Click += (_, _) => TogglePause();
        _settingsSaveAction.Content = "Save changes";
        StyleButton(_settingsSaveAction, ButtonTone.Primary, minWidth: 112);
        _settingsSaveAction.IsEnabled = false;
        _settingsSaveAction.Click += async (_, _) => await SaveAndApplyAsync();
        StyleButton(_hide, ButtonTone.Quiet, minWidth: 72);
        _hide.Content = "To tray";
        _hide.IsVisible = ShouldHideOnWindowClose(IsTrayAvailable());
        _hide.Click += (_, _) => Hide();

        _tabs.Background = Brushes.Transparent;
        _tabs.BorderThickness = new Thickness(0);
        _tabs.Padding = new Thickness(0);
        _tabs.Margin = new Thickness(0);
        _tabs.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _tabs.VerticalContentAlignment = VerticalAlignment.Stretch;
        _tabs.Template = new FuncControlTemplate<TabControl>((_, _) => new Border
        {
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Child = _tabContentHost
        });
        _tabs.Items.Add(CreateTabItem("\uE80F", "Overview", BuildOverviewTab(), "Monitoring"));
        _tabs.Items.Add(CreateTabItem("\uE895", "Interception", BuildTorrentsTab()));
        _tabs.Items.Add(CreateTabItem("\uE768", "Simulation", BuildSimulationTab(), "Control", divider: true));
        _tabs.Items.Add(CreateTabItem("\uE81C", "Activity", BuildActivityTab()));
        _tabs.Items.Add(CreateTabItem("\uE713", "Settings", BuildOptionsTab(), "System", divider: true));
        _tabs.Items.Add(CreateTabItem("\uE83D", "Platform", BuildPlatformTab()));
        _tabContentHost.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _tabContentHost.VerticalContentAlignment = VerticalAlignment.Stretch;
        _tabContentHost.Content = (_tabs.SelectedItem as TabItem)?.Content;
        _tabs.SelectionChanged += (_, _) =>
        {
            _tabContentHost.Content = (_tabs.SelectedItem as TabItem)?.Content;
            RefreshNavigationStyles();
        };

        var navigation = BuildNavigation();
        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("250,*"),
            Children =
            {
                Place(navigation, column: 0),
                Place(_tabs, column: 1)
            }
        };
        RefreshNavigationStyles();

        return new Grid
        {
            Background = XRatioPalette.Canvas,
            RowDefinitions = new RowDefinitions("66,*"),
            Children =
            {
                BuildHeader(),
                Place(body, row: 1)
            }
        };
    }

    private void ApplyTheme(string? themeMode, string? accentColor = null)
    {
        var normalized = NormalizeThemeMode(themeMode);
        var normalizedAccent = NormalizeAccentColor(accentColor ?? _settings.AccentColor);
        XRatioPalette.Apply(normalized, normalizedAccent);
        App.ApplyThemeVariant(normalized, normalizedAccent);
    }

    private string SelectedThemeMode() =>
        _themeMode.SelectedIndex >= 0 && _themeMode.SelectedIndex < ThemePalette.Options.Count
            ? ThemePalette.Options[_themeMode.SelectedIndex]
            : ThemePalette.Light;

    private string SelectedAccentColor() =>
        _accentColor.SelectedIndex >= 0 && _accentColor.SelectedIndex < AccentPalette.Options.Count
            ? AccentPalette.Options[_accentColor.SelectedIndex]
            : AccentPalette.Blue;

    private string SelectedLanguage() =>
        UiText.At(_languageMode.SelectedIndex);

    private void ApplyLocalization(Control? root = null)
    {
        var surface = root ?? Content as Control;
        if (surface is null)
            return;

        _suppressLanguageSelection = true;
        try
        {
            foreach (var control in Descendants(surface))
            {
                switch (control)
                {
                    case TextBlock textBlock:
                        textBlock.Text = UiText.TranslateMessage(textBlock.Text ?? string.Empty, _language);
                        break;
                    case Button button when button.Content is string buttonText:
                        button.Content = UiText.TranslateMessage(buttonText, _language);
                        break;
                    case CheckBox checkBox when checkBox.Content is string checkBoxText:
                        checkBox.Content = UiText.TranslateMessage(checkBoxText, _language);
                        break;
                    case TextBox textBox:
                        textBox.PlaceholderText = UiText.TranslateMessage(textBox.PlaceholderText ?? string.Empty, _language);
                        break;
                    case TabItem tabItem when tabItem.Header is Control header:
                        foreach (var headerText in Descendants(header).OfType<TextBlock>())
                            headerText.Text = UiText.TranslateMessage(headerText.Text ?? string.Empty, _language);
                        break;
                    case MenuItem menuItem when menuItem.Header is string menuText:
                        menuItem.Header = UiText.TranslateMessage(menuText, _language);
                        break;
                    case TabItem tabItem when tabItem.Header is string tabText:
                        tabItem.Header = UiText.TranslateMessage(tabText, _language);
                        break;
                    case ComboBox comboBox:
                        LocalizeComboItems(comboBox);
                        break;
                }
            }
        }
        finally
        {
            _suppressLanguageSelection = false;
        }

        if (root is Window window)
            window.Title = UiText.TranslateMessage(window.Title ?? string.Empty, _language);
    }

    private string L(string text) => UiText.TranslateMessage(text, _language);

    private void LocalizeComboItems(ComboBox comboBox)
    {
        if (comboBox.ItemsSource is not IEnumerable<string> items)
            return;

        var values = items.ToArray();
        if (values.Length == 0)
            return;

        var selectedIndex = comboBox.SelectedIndex;
        var translated = values
            .Select(value => UiText.TranslateAny(value, _language))
            .ToArray();
        if (values.SequenceEqual(translated, StringComparer.Ordinal))
            return;

        comboBox.ItemsSource = translated;
        if (selectedIndex >= 0 && selectedIndex < translated.Length)
            comboBox.SelectedIndex = selectedIndex;
    }

    private void RefreshNavigationStyles()
    {
        foreach (var (button, row) in _navigationItems)
        {
            var selected = button.Tag is int tabIndex && tabIndex == _tabs.SelectedIndex;
            row.Background = selected ? XRatioPalette.NavSelected : Brushes.Transparent;
            row.BorderBrush = Brushes.Transparent;
            row.BorderThickness = new Thickness(0);
            row.CornerRadius = new CornerRadius(10);

            foreach (var text in Descendants(row).OfType<TextBlock>())
            {
                text.Foreground = text.Tag switch
                {
                    "NavIcon" => selected ? XRatioPalette.Accent : XRatioPalette.Muted,
                    _ => selected ? XRatioPalette.Accent : XRatioPalette.Ink
                };
            }
        }

        foreach (var tab in _tabs.Items.OfType<TabItem>())
        {
            var selected = ReferenceEquals(tab, _tabs.SelectedItem);
            tab.Background = Brushes.Transparent;
            tab.BorderBrush = Brushes.Transparent;
            tab.BorderThickness = new Thickness(0);
            tab.CornerRadius = new CornerRadius(0);
            if (tab.Header is Control header)
            {
                var row = Descendants(header)
                    .OfType<Border>()
                    .FirstOrDefault(border => Equals(border.Tag, "NavRow"));
                if (row is not null)
                {
                    row.Background = selected ? XRatioPalette.NeutralSoft : Brushes.Transparent;
                    row.BorderBrush = Brushes.Transparent;
                    row.BorderThickness = new Thickness(0);
                    row.CornerRadius = new CornerRadius(10);
                }

                foreach (var text in Descendants(header).OfType<TextBlock>())
                {
                    text.Foreground = text.Tag switch
                    {
                        "NavIcon" => selected ? XRatioPalette.Accent : XRatioPalette.Muted,
                        _ => selected ? XRatioPalette.Accent : XRatioPalette.Ink
                    };
                }

            }
        }
    }

    private Control BuildHeader()
    {
        var brand = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                BuildLogo(),
                new StackPanel
                {
                    Spacing = 1,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "XRatio",
                            FontSize = 15,
                            FontWeight = FontWeight.Bold,
                            Foreground = XRatioPalette.Ink
                        },
                        new TextBlock
                        {
                            Text = "LOCAL RATIO CONTROL",
                            FontSize = 8.5,
                            FontWeight = FontWeight.SemiBold,
                            Foreground = XRatioPalette.Subtle,
                            LetterSpacing = 1.1
                        }
                    }
                },
                BuildGitHubRepositoryButton(28)
            }
        };

        _statusIndicator.Width = 7;
        _statusIndicator.Height = 7;
        _statusIndicator.CornerRadius = new CornerRadius(4);
        _statusIndicator.Background = XRatioPalette.Subtle;
        _statusIndicator.VerticalAlignment = VerticalAlignment.Center;
        var statusBadge = new Border
        {
            Background = XRatioPalette.MetricSurface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 6),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children = { _statusIndicator, _status }
            }
        };

        var surfaceHint = new TextBlock
        {
            Text = "LOCAL / MONITORING",
            FontSize = 9,
            FontWeight = FontWeight.SemiBold,
            Foreground = XRatioPalette.Subtle,
            LetterSpacing = 1.3,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center
        };

        var actions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _toggle, _pause, _hide }
        };

        return new Border
        {
            Background = XRatioPalette.Topbar,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(18, 10),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
                ColumnSpacing = 14,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    Place(brand),
                    Place(surfaceHint, column: 1),
                    Place(statusBadge, column: 2),
                    Place(actions, column: 3)
                }
            }
        };
    }

    private static Control BuildLogo()
    {
        using var iconStream = AssetLoader.Open(new Uri("avares://XRatio/Assets/XRatio-app-icon-v5.png"));
        var bitmap = new Bitmap(iconStream);
        return new Border
        {
            Width = 34,
            Height = 34,
            CornerRadius = new CornerRadius(9),
            ClipToBounds = true,
            Child = new Image
            {
                Source = bitmap,
                Stretch = Stretch.UniformToFill
            }
        };
    }

    private async Task ShowGuideAsync(TabControl tabs)
    {
        var tabName = tabs.SelectedItem is TabItem { Tag: string tag }
            ? tag
            : "Overview";
        var dialog = new Window
        {
            Title = $"XRatio Guide · {tabName}",
            Width = 720,
            Height = 620,
            MinWidth = 540,
            MinHeight = 420,
            CanResize = true,
            Background = XRatioPalette.Canvas,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var close = CreateButton("Close", ButtonTone.Secondary, 88);
        close.Click += (_, _) => dialog.Close();
        dialog.Content = BuildGuideDialog(tabName, close);
        ApplyLocalization(dialog);
        await dialog.ShowDialog(this);
    }

    private static Control BuildGuideDialog(string tabName, Button close)
    {
        var page = ResolveGuidePage(tabName);
        var sections = new StackPanel
        {
            Spacing = 12,
            MaxWidth = 680,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        foreach (var section in page.Sections)
            sections.Children.Add(BuildGuideSection(section));

        var footer = new Border
        {
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(0, 14, 0, 0),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Children = { close }
            }
        };

        return new Border
        {
            Padding = new Thickness(24),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto"),
                RowSpacing = 18,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 5,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = page.Title,
                                FontSize = 24,
                                FontWeight = FontWeight.SemiBold,
                                Foreground = XRatioPalette.Ink,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap
                            },
                            new TextBlock
                            {
                                Text = page.Intro,
                                FontSize = 12.5,
                                Foreground = XRatioPalette.Muted,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                                MaxWidth = 680
                            }
                        }
                    },
                    Place(
                        new ScrollViewer
                        {
                            Content = sections,
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                        },
                        row: 1),
                    Place(footer, row: 2)
                }
            }
        };
    }

    private static Border BuildGuideSection(GuideSection section)
    {
        var content = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock
                {
                    Text = section.Title,
                    FontSize = 14,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = XRatioPalette.Ink
                },
                new TextBlock
                {
                    Text = section.Description,
                    FontSize = 12,
                    Foreground = XRatioPalette.Muted,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }
        };
        foreach (var step in section.Steps)
        {
            content.Children.Add(new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                Children =
                {
                    new Border
                    {
                        Width = 5,
                        Height = 5,
                        CornerRadius = new CornerRadius(3),
                        Background = XRatioPalette.Accent,
                        Margin = new Thickness(2, 7, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    },
                    new TextBlock
                    {
                        Text = step,
                        FontSize = 12,
                        Foreground = XRatioPalette.Ink,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                }
            });
        }

        if (!string.IsNullOrWhiteSpace(section.ImageAsset))
        {
            using var imageStream = AssetLoader.Open(new Uri(section.ImageAsset));
            var screenshot = new Bitmap(imageStream);
            content.Children.Add(new Border
            {
                Background = XRatioPalette.SurfaceRaised,
                BorderBrush = XRatioPalette.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Child = new Image
                {
                    Source = screenshot,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    MaxWidth = 640
                }
            });
        }

        return new Border
        {
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = content
        };
    }

    private static GuidePage ResolveGuidePage(string tabName) => tabName switch
    {
        "Simulation" => new(
            "Simulation guide",
            "Build a session in a few steps, then control it from the session list.",
            [
                new GuideSection(
                    "1. Import a torrent",
                    "The torrent file provides the metadata and the tracker list used by the session.",
                    [
                        "In Torrent file, click Browse… and select one .torrent file.",
                        "Check the selected tracker, info hash and size in Torrent info."
                    ]),
                new GuideSection(
                    "2. Set the announce profile",
                    "The main form contains the values that will be announced to the selected tracker.",
                    [
                        "Set upload and download speeds, or keep + Random values enabled to vary them between the configured limits.",
                        "Choose the client profile and the finished percentage in Options.",
                        "Use the Stop controls only when the session should end automatically."
                    ]),
                new GuideSection(
                    "3. Review Advanced",
                    "Advanced contains the tracker identity and optional outbound proxy settings.",
                    [
                        "Listening port and Peers requested control the identity sent by the session.",
                        "Proxy address is optional; enter an absolute address when the tracker connection must go through a proxy."
                    ]),
                new GuideSection(
                    "4. Add, then start",
                    "Adding saves the configuration. Starting is the explicit action that begins tracker communication.",
                    [
                        "Click Add session to create the session in the list.",
                        "Select the new session, then click ▶  Start. The action changes to ■  Stop while it is running."
                    ]),
                new GuideSection(
                    "5. Monitor the session",
                    "The session row shows state, ratio, transfer counters, peers and the next announce time.",
                    [
                        "Use Manual update for an immediate update while the session is running.",
                        "Select a stopped session and click Remove when it is no longer needed."
                    ]),
                new GuideSection(
                    "Safety note",
                    "Tracker rules still apply to every session.",
                    [
                        "Use simulation only with torrents and trackers for which you are authorized."
                    ])
            ]),
        "Interception" => new(
            "Interception guide",
            "Follow tracker activity observed through the local proxy.",
            [
                new GuideSection(
                    "Start the proxy",
                    "Use Start in the top bar after checking the proxy settings.",
                    ["The status indicator in the header shows whether the proxy is active or paused."]),
                new GuideSection(
                    "Read tracked sessions",
                    "Each row contains the torrent hash, tracker, peers, status, counters and the last announce time.",
                    ["Select a row to access its available actions, including copying the info hash or resetting statistics."])
            ]),
        "Activity" => new(
            "Activity guide",
            "Use the event stream to understand what the proxy and simulations are doing.",
            [
                new GuideSection(
                    "Read the latest events",
                    "New entries are added as proxy decisions, imports and simulation actions happen.",
                    ["Scroll to review recent events; the list keeps the latest 500 entries."])
            ]),
        "Settings" => new(
            "Settings guide",
            "Adjust the local proxy behavior and save the changes from this tab.",
            [
                new GuideSection(
                    "Change a value",
                    "Connection, ratio shaping and reporting options are grouped by purpose.",
                    ["Edit the fields, review the toggles, then click Save changes in the Settings tab."]),
                BuildQbittorrentGuideSection(),
                new GuideSection(
                    "Keep the scope clear",
                    "XRatio listens locally and does not handle payload or peer traffic.",
                    [
                        "Keep Listen on localhost only enabled unless you have a specific, authorized reason to change the deployment boundary.",
                        "Use only torrents and trackers for which you are authorized, and follow the tracker rules."
                    ])
            ]),
        "Platform" => new(
            "Platform guide",
            "Manage system integration and HTTPS trust for the current machine.",
            [
                new GuideSection(
                    "HTTPS interception",
                    "The installation CA is used only to inspect HTTPS tracker traffic through the local proxy.",
                    ["Enable HTTPS interception when needed, and remove CA trust when XRatio should no longer be trusted by the current Windows user."]),
                new GuideSection(
                    "Startup behavior",
                    "Configure whether XRatio starts with the user session and whether it opens minimized to the tray.",
                    ["Review the platform capability text before enabling an integration."])
            ]),
        "Overview" => new(
            "Overview guide",
            "Use this tab to check the current runtime at a glance.",
            [
                new GuideSection(
                    "Read the status",
                    "The summary shows proxy state, tracked torrents, active versus configured simulations and reported upload.",
                    ["Start or pause the proxy from the top bar; the overview updates as activity changes."]),
                BuildQbittorrentGuideSection()
            ]),
        _ => new(
            $"{tabName} guide",
            "This guide follows the active XRatio tab.",
            [
                new GuideSection(
                    "Get started",
                    "Use the visible controls from top to bottom, then check the activity and status feedback.",
                    ["If an action is unavailable, select the relevant row or complete the required fields first."])
            ])
    };

    private static GuideSection BuildQbittorrentGuideSection() => new(
        "Configure the qBittorrent client",
        "Route qBittorrent tracker announces through the local XRatio proxy before checking the ratio.",
        [
            "Start XRatio and verify that the header shows HTTP/HTTPS active on 127.0.0.1:3773.",
            "In qBittorrent, open Tools > Options > Connection.",
            "Under Proxy Server, choose HTTP, set Host to 127.0.0.1 and Port to 3773.",
            "Enable Perform hostname lookup via proxy and Use proxy for BitTorrent purposes. Leave Use proxy for peer connections disabled because XRatio handles tracker announces only.",
            "In XRatio Settings > Announce behavior, use Report download as zero or Pretend to seed only when that reporting mode is allowed for your test tracker; these options change the announce values and do not freeze a tracker-owned ratio.",
            "Click Apply, then OK. Check the Interception tab in XRatio for the next tracker announce.",
            "If the ratio still changes, check the port, proxy type and tracker policy. A proxy cannot force a tracker to accept or freeze a ratio."
        ],
        "avares://XRatio/Assets/qbittorrent-proxy-settings.png");

    private Control BuildActivityTab()
    {
        ConfigureList(_activity);
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("72,86,*"),
            ColumnSpacing = 10,
            Margin = new Thickness(10, 0, 10, 6),
            Children =
            {
                BuildActivityHeader("Time"),
                Place(BuildActivityHeader("Level · source"), column: 1),
                Place(BuildActivityHeader("Event details"), column: 2)
            }
        };
        var content = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Children = { header, Place(BuildListSurface(_activity), row: 1) }
        };
        return BuildTabLayout(
            "Activity",
            "A timestamped view of proxy, simulation and configuration events.",
            content);
    }

    private static TextBlock BuildActivityHeader(string text) => new()
    {
        Text = text,
        FontSize = 10.5,
        FontWeight = FontWeight.SemiBold,
        Foreground = XRatioPalette.Muted
    };

    private Control BuildOverviewTab()
    {
        _startupFailureTitle.Text = "Interception could not start";
        _startupFailureTitle.FontSize = 14;
        _startupFailureTitle.FontWeight = FontWeight.SemiBold;
        _startupFailureTitle.Foreground = XRatioPalette.Danger;
        _startupFailureDetail.FontSize = 12;
        _startupFailureDetail.Foreground = XRatioPalette.Ink;
        _startupFailureDetail.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _overviewReportedKpi.FontSize = 14;
        _overviewReportedKpi.FontWeight = FontWeight.Bold;
        _overviewReportedKpi.Foreground = XRatioPalette.Ink;
        _overviewReportedKpi.FontFeatures = XRatioPalette.TabularNumbers;
        var resolveStartup = CreateButton("Open Settings", ButtonTone.Secondary, 116);
        resolveStartup.Click += (_, _) =>
        {
            _tabs.SelectedIndex = 4;
            _port.Focus();
        };
        _startupFailureBanner.Background = XRatioPalette.DangerSoft;
        _startupFailureBanner.BorderBrush = XRatioPalette.DangerBorder;
        _startupFailureBanner.BorderThickness = new Thickness(1);
        _startupFailureBanner.CornerRadius = new CornerRadius(7);
        _startupFailureBanner.Padding = new Thickness(14, 12);
        _startupFailureBanner.IsVisible = false;
        _startupFailureBanner.Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 16,
            Children =
            {
                new StackPanel
                {
                    Spacing = 3,
                    Children = { _startupFailureTitle, _startupFailureDetail }
                },
                Place(resolveStartup, column: 1)
            }
        };

        var runtime = new Border
        {
            MinWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
                Children =
                {
                    BuildRuntimeHero(),
                    Place(
                        new Grid
                        {
                            ColumnDefinitions = new ColumnDefinitions("*,*"),
                            Children =
                            {
                                BuildRuntimeRow("Tracked torrents", "Announcements observed", _overviewTorrentKpi, true),
                                Place(BuildRuntimeRow("Simulations", "Active / configured", _overviewSimulationKpi, false), column: 1)
                            }
                        },
                        row: 1),
                    Place(
                        new Border
                        {
                            BorderBrush = XRatioPalette.Border,
                            BorderThickness = new Thickness(0, 1, 0, 0),
                            Padding = new Thickness(16, 12),
                            Child = new Grid
                            {
                                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                                Children =
                                {
                                    new StackPanel
                                    {
                                        Spacing = 2,
                                        Children =
                                        {
                                            new TextBlock
                                            {
                                                Text = "Reported upload",
                                                FontSize = 11,
                                                FontWeight = FontWeight.SemiBold,
                                                Foreground = XRatioPalette.Muted
                                            },
                                            new TextBlock
                                            {
                                                Text = "Current session",
                                                FontSize = 10.5,
                                                Foreground = XRatioPalette.Subtle
                                            }
                                        }
                                    },
                                    Place(_overviewReportedKpi, column: 1)
                                }
                            }
                        },
                        row: 2)
                }
            }
        };

        var modes = new Border
        {
            Background = XRatioPalette.SurfaceRaised,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = new StackPanel
            {
                Children =
                {
                    new Border
                    {
                        Padding = new Thickness(18, 16, 18, 12),
                        Child = new StackPanel
                        {
                            Spacing = 3,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = "OPERATING MODES",
                                    FontSize = 9,
                                    FontWeight = FontWeight.Bold,
                                    Foreground = XRatioPalette.Accent,
                                    LetterSpacing = 1.25
                                },
                                new TextBlock
                                {
                                    Text = "Two paths, one local control plane.",
                                    FontSize = 14,
                                    FontWeight = FontWeight.Bold,
                                    Foreground = XRatioPalette.Ink,
                                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                                }
                            }
                        }
                    },
                    BuildModeRow(
                        "Interception",
                        "Rewrite tracker announces from a real client through the local proxy.",
                        "LOCAL",
                        XRatioPalette.Accent,
                        divider: true),
                    BuildModeRow(
                        "Simulation",
                        "Run independent .torrent sessions with controlled counters and rates.",
                        "CONTROLLED",
                        XRatioPalette.Positive,
                        divider: false)
                }
            }
        };
        var trust = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(2, 2, 0, 0),
            Children =
            {
                new Border
                {
                    Width = 6,
                    Height = 6,
                    CornerRadius = new CornerRadius(3),
                    Background = XRatioPalette.Positive,
                    VerticalAlignment = VerticalAlignment.Center
                },
                new TextBlock
                {
                    Text = "Tracker announces only — payloads and peer traffic remain untouched.",
                    FontSize = 12,
                    Foreground = XRatioPalette.Muted,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }
        };
        var content = new Grid
        {
            MaxWidth = 980,
            HorizontalAlignment = HorizontalAlignment.Left,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("1.45*,1*"),
            ColumnSpacing = 16,
            RowSpacing = 14,
            Children =
            {
                Place(_startupFailureBanner, column: 0),
                Place(runtime, row: 1, column: 0),
                Place(modes, row: 1, column: 1),
                Place(trust, row: 2, column: 0)
            }
        };
        Grid.SetColumnSpan(_startupFailureBanner, 2);
        Grid.SetColumnSpan(trust, 2);
        UpdateOverviewMetrics();
        return BuildTabLayout("Overview", "Current runtime status.", content);
    }

    private Border BuildRuntimeHero()
    {
        _overviewProxyKpi.FontSize = 29;
        _overviewProxyKpi.FontWeight = FontWeight.Bold;
        _overviewProxyKpi.Foreground = XRatioPalette.Ink;
        _overviewProxyKpi.FontFeatures = XRatioPalette.TabularNumbers;
        return new Border
        {
            Background = XRatioPalette.MetricSurface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(18, 17, 18, 16),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 4,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "PROXY CHANNEL",
                                FontSize = 9,
                                FontWeight = FontWeight.Bold,
                                Foreground = XRatioPalette.Accent,
                                LetterSpacing = 1.25
                            },
                            _overviewProxyKpi,
                            new TextBlock
                            {
                                Text = "Local tracker interception · HTTP / HTTPS",
                                FontSize = 11,
                                Foreground = XRatioPalette.Muted
                            }
                        }
                    },
                    Place(
                        new Border
                        {
                            Width = 10,
                            Height = 10,
                            CornerRadius = new CornerRadius(5),
                            Background = XRatioPalette.Subtle,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, 5, 0, 0)
                        },
                        column: 1)
                }
            }
        };
    }

    private static Border BuildModeRow(
        string title,
        string description,
        string state,
        SolidColorBrush stateBrush,
        bool divider)
    {
        return new Border
        {
            BorderBrush = XRatioPalette.Border,
            BorderThickness = divider ? new Thickness(0, 1, 0, 0) : new Thickness(0),
            Padding = new Thickness(18, 15),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                ColumnSpacing = 14,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 4,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = title,
                                FontSize = 12.5,
                                FontWeight = FontWeight.Bold,
                                Foreground = XRatioPalette.Ink
                            },
                            new TextBlock
                            {
                                Text = description,
                                FontSize = 11,
                                Foreground = XRatioPalette.Muted,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap
                            }
                        }
                    },
                    Place(
                        new TextBlock
                        {
                            Text = state,
                            FontSize = 9,
                            FontWeight = FontWeight.Bold,
                            Foreground = stateBrush,
                            LetterSpacing = 1,
                            VerticalAlignment = VerticalAlignment.Top
                        },
                        column: 1)
                }
            }
        };
    }

    private Control BuildSimulationTab()
    {
        ConfigureTextBox(_torrentPath, "Select a .torrent file");
        _torrentPath.IsReadOnly = true;
        _torrentPath.Width = double.NaN;
        _torrentPath.MinWidth = 0;
        _torrentPath.HorizontalAlignment = HorizontalAlignment.Stretch;
        ConfigureTextBox(_simulationInfoHash, "Info hash");
        ConfigureTextBox(_simulationInfoSize, "Size");
        ConfigureTextBox(_simulationAccountName, "Optional");
        _simulationAccountName.MaxLength = 128;
        _simulationInfoHash.IsReadOnly = true;
        _simulationInfoSize.IsReadOnly = true;
        _simulationInfoHash.Width = double.NaN;
        _simulationInfoHash.MinWidth = 0;
        _simulationInfoHash.HorizontalAlignment = HorizontalAlignment.Stretch;
        _simulationInfoSize.Width = 120;
        _simulationInfoSize.MinWidth = 120;
        ConfigureTextBox(_simulationUploadRate, SimulationDefaults.UploadKiBPerSecond.ToString(CultureInfo.InvariantCulture));
        ConfigureTextBox(_simulationDownloadRate, SimulationDefaults.DownloadKiBPerSecond.ToString(CultureInfo.InvariantCulture));
        ConfigureTextBox(_simulationCompleted, SimulationDefaults.InitialCompletedPercent.ToString(CultureInfo.InvariantCulture));
        ConfigureSimulationSpeedInput(_simulationRandomUploadMin, SimulationDefaults.RandomUploadMinimumKiBPerSecond.ToString(CultureInfo.InvariantCulture));
        ConfigureSimulationSpeedInput(_simulationRandomUploadMax, SimulationDefaults.RandomUploadMaximumKiBPerSecond.ToString(CultureInfo.InvariantCulture));
        ConfigureSimulationSpeedInput(_simulationRandomDownloadMin, SimulationDefaults.RandomDownloadMinimumKiBPerSecond.ToString(CultureInfo.InvariantCulture));
        ConfigureSimulationSpeedInput(_simulationRandomDownloadMax, SimulationDefaults.RandomDownloadMaximumKiBPerSecond.ToString(CultureInfo.InvariantCulture));
        ConfigureCheckBox(_simulationRandomUpload);
        ConfigureCheckBox(_simulationRandomDownload);
        _simulationRandomUpload.Content = "+ Random values";
        _simulationRandomDownload.Content = "+ Random values";
        _simulationUploadRate.Text = SimulationDefaults.UploadKiBPerSecond.ToString(CultureInfo.InvariantCulture);
        _simulationDownloadRate.Text = SimulationDefaults.DownloadKiBPerSecond.ToString(CultureInfo.InvariantCulture);
        _simulationCompleted.Text = SimulationDefaults.InitialCompletedPercent.ToString(CultureInfo.InvariantCulture);
        _simulationRandomUpload.IsChecked = true;
        _simulationRandomDownload.IsChecked = true;
        _simulationRandomUploadMin.Text = SimulationDefaults.RandomUploadMinimumKiBPerSecond.ToString(CultureInfo.InvariantCulture);
        _simulationRandomUploadMax.Text = SimulationDefaults.RandomUploadMaximumKiBPerSecond.ToString(CultureInfo.InvariantCulture);
        _simulationRandomDownloadMin.Text = SimulationDefaults.RandomDownloadMinimumKiBPerSecond.ToString(CultureInfo.InvariantCulture);
        _simulationRandomDownloadMax.Text = SimulationDefaults.RandomDownloadMaximumKiBPerSecond.ToString(CultureInfo.InvariantCulture);
        ConfigureTextBox(_simulationPort, "6881");
        ConfigureTextBox(_simulationNumWant, "200");
        ConfigureTextBox(_simulationAnnounceInterval, "1800");
        ConfigureTextBox(_simulationStopValue, "Value");
        ConfigureTextBox(_simulationProxyAddress, "http://127.0.0.1:8080");
        ConfigureTextBox(_simulationProxyUsername, "Optional");
        _simulationPort.Text = "6881";
        _simulationNumWant.Text = "200";
        _simulationAnnounceInterval.Text = "1800";
        _simulationStopValue.IsEnabled = false;
        ConfigureComboBox(_simulationTracker, 320);
        _simulationTracker.Width = double.NaN;
        _simulationTracker.HorizontalAlignment = HorizontalAlignment.Stretch;
        _simulationRevealPrivateValues.Content = "Show full path and tracker URL";
        ConfigureCheckBox(_simulationRevealPrivateValues);
        _simulationRevealPrivateValues.IsChecked = false;
        _simulationRevealPrivateValues.PropertyChanged += (_, args) =>
        {
            if (args.Property == ToggleButton.IsCheckedProperty)
                UpdateSimulationPrivacyDisplay();
        };
        UpdateSimulationPrivacyDisplay();
        ConfigureComboBox(_simulationClient, 220);
        ConfigureComboBox(_simulationStopMode, 180);
        ConfigureCompactSimulationControls(
            _torrentPath,
            _simulationAccountName,
            _simulationInfoHash,
            _simulationInfoSize,
            _simulationUploadRate,
            _simulationDownloadRate,
            _simulationCompleted,
            _simulationRandomUploadMin,
            _simulationRandomUploadMax,
            _simulationRandomDownloadMin,
            _simulationRandomDownloadMax,
            _simulationPort,
            _simulationNumWant,
            _simulationAnnounceInterval,
            _simulationStopValue,
            _simulationProxyAddress,
            _simulationProxyUsername,
            _simulationTracker,
            _simulationClient,
            _simulationStopMode,
            _simulationRandomUpload,
            _simulationRandomDownload);
        _simulationAccountName.Width = 180;
        _simulationAccountName.MinWidth = 180;
        _simulationAccountName.MaxWidth = 180;
        _simulationAccountName.HorizontalAlignment = HorizontalAlignment.Left;
        _simulationClient.ItemsSource = ClientProfileCatalog.All.Select(profile => profile.DisplayName).ToArray();
        _simulationClient.SelectedIndex = ClientProfileCatalog.All
            .Select((profile, index) => (profile, index))
            .First(item => item.profile.Id == SimulationDefaults.ClientProfileId)
            .index;
        _simulationStopMode.ItemsSource = new[]
        {
            "Never",
            "After minutes",
            "Uploaded MiB",
            "Downloaded MiB",
            "Ratio"
        };
        _simulationStopMode.SelectedIndex = 0;
        _simulationStopMode.SelectionChanged += (_, _) =>
            _simulationStopValue.IsEnabled = _simulationStopMode.SelectedIndex > 0;

        var choose = CreateButton("Browse…", ButtonTone.Primary, 96);
        choose.Click += async (_, _) => await ChooseTorrentAsync();
        _simulationAddAction.Content = "Add session";
        StyleButton(_simulationAddAction, ButtonTone.Primary, 112);
        _simulationAddAction.Click += async (_, _) => await AddSimulationAsync();
        _simulationAddFeedback.FontSize = 11;
        _simulationAddFeedback.Foreground = XRatioPalette.Muted;
        _simulationAddFeedback.VerticalAlignment = VerticalAlignment.Center;
        _simulationAddFeedback.TextTrimming = TextTrimming.CharacterEllipsis;
        var fileRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 8,
            Children = { _torrentPath, Place(choose, column: 1) }
        };
        var torrentFile = BuildCompactGroup(
            "Torrent file",
            new StackPanel
            {
                Spacing = 4,
                Children = { fileRow, _simulationRevealPrivateValues }
            });
        var torrentInfo = BuildCompactGroup(
            "Torrent info",
            new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    BuildCompactFieldRow("Account", _simulationAccountName),
                    BuildCompactFieldRow("Tracker", _simulationTracker),
                    BuildTorrentIdentityRow(),
                    new TextBlock
                    {
                        Text = "The account label stays local; the tracker name is read automatically from the announce URL.",
                        FontSize = 10,
                        Foreground = XRatioPalette.Muted,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    }
                }
            });
        var speeds = BuildCompactGroup(
            "Speed options",
            new StackPanel
            {
                Spacing = 6,
                Children =
                {
                BuildSimulationSpeedRow(
                    "Upload speed (kB/s)",
                    _simulationUploadRate,
                    _simulationRandomUpload,
                    _simulationRandomUploadMin,
                    _simulationRandomUploadMax),
                BuildSimulationSpeedRow(
                    "Download speed (kB/s)",
                    _simulationDownloadRate,
                    _simulationRandomDownload,
                    _simulationRandomDownloadMin,
                    _simulationRandomDownloadMax)
                }
            });
        var options = BuildCompactGroup("Options", BuildSimulationOptionsGrid());
        var safetyNote = new TextBlock
        {
            Text = "The tracker is contacted only after the session is added and Start is pressed.",
            FontSize = 11.5,
            Foreground = XRatioPalette.Muted,
            Margin = new Thickness(2, 2, 0, 0)
        };
        var main = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 6, 0, 8),
            Children = { torrentFile, torrentInfo, speeds, options, safetyNote }
        };
        var advanced = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 6, 0, 8),
            Children =
            {
                BuildCompactGroup(
                    "Tracker identity",
                    BuildFieldGrid(
                        ("Listening port", _simulationPort),
                        ("Peers requested", _simulationNumWant))),
                BuildCompactGroup(
                    "Outbound proxy",
                    BuildFieldGrid(
                        ("Proxy address", _simulationProxyAddress),
                        ("Proxy username", _simulationProxyUsername)))
            }
        };
        var modeTabs = new TabControl
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Items =
            {
                new TabItem
                {
                    Header = "Main",
                    Content = BuildVerticalSimulationScroller(main),
                    Padding = new Thickness(12, 6)
                },
                new TabItem
                {
                    Header = "Advanced",
                    Content = BuildVerticalSimulationScroller(advanced),
                    Padding = new Thickness(12, 6)
                }
            }
        };
        modeTabs.MaxHeight = ResolveSimulationTabsMaxHeight(Height);
        SizeChanged += (_, args) => modeTabs.MaxHeight = ResolveSimulationTabsMaxHeight(args.NewSize.Height);

        ConfigureList(_simulations);
        _simulations.ClipToBounds = true;
        ScrollViewer.SetHorizontalScrollBarVisibility(_simulations, ScrollBarVisibility.Disabled);
        ScrollViewer.SetVerticalScrollBarVisibility(_simulations, ScrollBarVisibility.Auto);
        _simulations.SelectionChanged += (_, _) => UpdateSimulationActionState();
        _simulationsEmptyState.IsHitTestVisible = false;
        _simulationsEmptyState.HorizontalAlignment = HorizontalAlignment.Center;
        _simulationsEmptyState.VerticalAlignment = VerticalAlignment.Center;
        _simulationsEmptyState.Spacing = 5;
        _simulationsEmptyState.Children.Add(new TextBlock
        {
            Text = "No simulation sessions",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = XRatioPalette.Ink,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        _simulationsEmptyState.Children.Add(new TextBlock
        {
            Text = "Choose a .torrent, configure its announce profile, then add a session.",
            FontSize = 12,
            Foreground = XRatioPalette.Muted,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 300,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        _simulationPrimaryAction.Content = "▶  Start";
        StyleButton(_simulationPrimaryAction, ButtonTone.Primary, 112);
        _simulationUpdateAction.Content = "Manual update";
        StyleButton(_simulationUpdateAction, ButtonTone.Secondary, 128);
        _simulationRemoveAction.Content = "Remove…";
        StyleButton(_simulationRemoveAction, ButtonTone.Danger, 104);
        _simulationPrimaryAction.Click += async (_, _) => await ToggleSelectedSimulationAsync();
        _simulationUpdateAction.Click += async (_, _) => await UpdateSelectedSimulationAsync();
        _simulationRemoveAction.Click += async (_, _) => await RemoveSelectedSimulationAsync();
        _simulationActions.ColumnDefinitions = new ColumnDefinitions("Auto,Auto,Auto");
        _simulationActions.RowDefinitions = new RowDefinitions("Auto");
        _simulationActions.ColumnSpacing = 8;
        _simulationActions.Children.Add(Place(_simulationPrimaryAction));
        _simulationActions.Children.Add(Place(_simulationUpdateAction, column: 1));
        _simulationActions.Children.Add(Place(_simulationRemoveAction, column: 2));
        _simulationActions.IsEnabled = false;
        _simulationActions.HorizontalAlignment = HorizontalAlignment.Right;
        var sessions = BuildCompactGroup(
            "Simulation sessions",
            new Grid
            {
                MinHeight = 120,
                ClipToBounds = true,
                Children = { _simulations, _simulationsEmptyState }
            });
        var commandBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 12,
            Children =
            {
                _simulationAddAction,
                Place(_simulationAddFeedback, column: 1),
                Place(_simulationActions, column: 2)
            }
        };
        HookSimulationFormPersistence();
        return new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(16, 10, 16, 12),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*,Auto"),
                RowSpacing = 8,
                Children =
                {
                    modeTabs,
                    Place(sessions, row: 1),
                    Place(commandBar, row: 2)
                }
            }
        };
    }

    private async Task ChooseTorrentAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose a torrent",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("BitTorrent metadata") { Patterns = ["*.torrent"] }
            ]
        });
        if (files.Count == 0)
            return;
        try
        {
            _pendingTorrent = TorrentMetadata.Load(files[0].Path.LocalPath);
            _torrentPath.Tag = _pendingTorrent.SourcePath;
            UpdateSimulationPrivacyDisplay();
            _simulationTracker.ItemsSource = _pendingTorrent.Trackers.Select(uri => uri.ToString()).ToArray();
            _simulationTracker.SelectedIndex = 0;
            _simulationInfoHash.Text = _pendingTorrent.InfoHashHex;
            _simulationInfoSize.Text = FormatBytes(_pendingTorrent.TotalSize);
            AddActivity($"Loaded torrent: {_pendingTorrent.Name} · {FormatBytes(_pendingTorrent.TotalSize)} · {_pendingTorrent.Trackers.Count} tracker(s).");
        }
        catch (Exception exception)
        {
            _pendingTorrent = null;
            _torrentPath.Tag = null;
            _torrentPath.Text = string.Empty;
            _simulationTracker.ItemsSource = null;
            _simulationInfoHash.Text = string.Empty;
            _simulationInfoSize.Text = string.Empty;
            AddActivity($"Torrent import failed: {exception.Message}");
        }
    }

    private void UpdateSimulationPrivacyDisplay()
    {
        var reveal = _simulationRevealPrivateValues.IsChecked == true;
        _simulationTracker.ItemTemplate = new FuncDataTemplate<string>((value, _) => new TextBlock
        {
            Text = reveal ? value : MaskTrackerUrl(value),
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        if (_torrentPath.Tag is string path)
            _torrentPath.Text = reveal ? path : MaskLocalPath(path);
    }

    private void HookSimulationFormPersistence()
    {
        foreach (var input in new[]
                 {
            _torrentPath,
            _simulationAccountName,
            _simulationUploadRate,
                     _simulationDownloadRate,
                     _simulationCompleted,
                     _simulationRandomUploadMin,
                     _simulationRandomUploadMax,
                     _simulationRandomDownloadMin,
                     _simulationRandomDownloadMax,
                     _simulationPort,
                     _simulationNumWant,
                     _simulationAnnounceInterval,
                     _simulationStopValue,
            _simulationProxyAddress,
            _simulationProxyUsername
                 })
            input.TextChanged += (_, _) => QueueSimulationFormPersistence();

        foreach (var checkBox in new[] { _simulationRandomUpload, _simulationRandomDownload })
            checkBox.PropertyChanged += (_, args) =>
            {
                if (args.Property == ToggleButton.IsCheckedProperty)
                    QueueSimulationFormPersistence();
            };

        foreach (var comboBox in new[] { _simulationTracker, _simulationClient, _simulationStopMode })
            comboBox.SelectionChanged += (_, _) => QueueSimulationFormPersistence();
    }

    private SimulationFormSettings CaptureSimulationFormSettings()
    {
        var clientProfileId = _simulationClient.SelectedIndex is >= 0 and < int.MaxValue &&
                              _simulationClient.SelectedIndex < ClientProfileCatalog.All.Count
            ? ClientProfileCatalog.All[_simulationClient.SelectedIndex].Id
            : SimulationDefaults.ClientProfileId;
        return new SimulationFormSettings
        {
            TorrentPath = NullIfWhiteSpace(_torrentPath.Tag as string ?? _torrentPath.Text),
            Tracker = _simulationTracker.SelectedItem as string,
            AccountName = _simulationAccountName.Text ?? string.Empty,
            ClientProfileId = clientProfileId,
            UploadKiBPerSecond = _simulationUploadRate.Text ?? string.Empty,
            DownloadKiBPerSecond = _simulationDownloadRate.Text ?? string.Empty,
            RandomUploadEnabled = _simulationRandomUpload.IsChecked == true,
            RandomUploadMinimumKiBPerSecond = _simulationRandomUploadMin.Text ?? string.Empty,
            RandomUploadMaximumKiBPerSecond = _simulationRandomUploadMax.Text ?? string.Empty,
            RandomDownloadEnabled = _simulationRandomDownload.IsChecked == true,
            RandomDownloadMinimumKiBPerSecond = _simulationRandomDownloadMin.Text ?? string.Empty,
            RandomDownloadMaximumKiBPerSecond = _simulationRandomDownloadMax.Text ?? string.Empty,
            CompletedPercent = _simulationCompleted.Text ?? string.Empty,
            ListeningPort = _simulationPort.Text ?? string.Empty,
            PeersRequested = _simulationNumWant.Text ?? string.Empty,
            AnnounceIntervalSeconds = _simulationAnnounceInterval.Text ?? string.Empty,
            StopMode = Math.Clamp(_simulationStopMode.SelectedIndex, 0, 4),
            StopValue = _simulationStopValue.Text ?? string.Empty,
            ProxyAddress = _simulationProxyAddress.Text ?? string.Empty,
            ProxyUsername = _simulationProxyUsername.Text ?? string.Empty
        };
    }

    private async Task RestoreSimulationFormAsync(SimulationFormSettings settings)
    {
        _restoringSimulationForm = true;
        try
        {
            _simulationUploadRate.Text = settings.UploadKiBPerSecond;
            _simulationDownloadRate.Text = settings.DownloadKiBPerSecond;
            _simulationAccountName.Text = settings.AccountName;
            _simulationRandomUpload.IsChecked = settings.RandomUploadEnabled;
            _simulationRandomUploadMin.Text = settings.RandomUploadMinimumKiBPerSecond;
            _simulationRandomUploadMax.Text = settings.RandomUploadMaximumKiBPerSecond;
            _simulationRandomDownload.IsChecked = settings.RandomDownloadEnabled;
            _simulationRandomDownloadMin.Text = settings.RandomDownloadMinimumKiBPerSecond;
            _simulationRandomDownloadMax.Text = settings.RandomDownloadMaximumKiBPerSecond;
            _simulationCompleted.Text = settings.CompletedPercent;
            _simulationPort.Text = settings.ListeningPort;
            _simulationNumWant.Text = settings.PeersRequested;
            _simulationAnnounceInterval.Text = settings.AnnounceIntervalSeconds;
            _simulationStopMode.SelectedIndex = Math.Clamp(settings.StopMode, 0, 4);
            _simulationStopValue.Text = settings.StopValue;
            _simulationProxyAddress.Text = settings.ProxyAddress;
            _simulationProxyUsername.Text = settings.ProxyUsername;

            var clientIndex = ClientProfileCatalog.All
                .Select((profile, index) => (profile, index))
                .FirstOrDefault(item => item.profile.Id == settings.ClientProfileId)
                .index;
            _simulationClient.SelectedIndex = clientIndex;

            if (!string.IsNullOrWhiteSpace(settings.TorrentPath) && File.Exists(settings.TorrentPath))
            {
                _pendingTorrent = TorrentMetadata.Load(settings.TorrentPath);
                _torrentPath.Tag = _pendingTorrent.SourcePath;
                UpdateSimulationPrivacyDisplay();
                var trackers = _pendingTorrent.Trackers.Select(uri => uri.ToString()).ToArray();
                _simulationTracker.ItemsSource = trackers;
                _simulationTracker.SelectedIndex = Math.Max(0, Array.IndexOf(trackers, settings.Tracker));
                _simulationInfoHash.Text = _pendingTorrent.InfoHashHex;
                _simulationInfoSize.Text = FormatBytes(_pendingTorrent.TotalSize);
            }
        }
        catch (Exception exception)
        {
            AddActivity($"Could not restore simulation form: {exception.Message}");
        }
        finally
        {
            _restoringSimulationForm = false;
        }

        await Task.CompletedTask;
    }

    private void QueueSimulationFormPersistence()
    {
        if (!_settingsLoaded || _restoringSimulationForm || _exiting)
            return;

        _settings = _settings with { SimulationForm = CaptureSimulationFormSettings() };
        _simulationFormSaveCancellation?.Cancel();
        _simulationFormSaveCancellation?.Dispose();
        _simulationFormSaveCancellation = new CancellationTokenSource();
        _ = PersistSimulationFormAfterDelayAsync(_simulationFormSaveCancellation.Token);
    }

    private async Task PersistSimulationFormAfterDelayAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(300, cancellationToken);
            await _settingsSaveGate.WaitAsync(cancellationToken);
            try
            {
                await _store.SaveAsync(_settings, cancellationToken);
            }
            finally
            {
                _settingsSaveGate.Release();
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Dispatcher.UIThread.Post(() => AddActivity($"Simulation settings persistence error: {exception.Message}"));
        }
    }

    private async Task AddSimulationAsync()
    {
        SimulationEntry? addedEntry = null;
        try
        {
            if (_pendingTorrent is null)
                throw new InvalidOperationException("Choose a .torrent file first.");
            if (_simulationTracker.SelectedItem is not string trackerText ||
                !Uri.TryCreate(trackerText, UriKind.Absolute, out var tracker))
                throw new InvalidOperationException("Choose a tracker.");
            if (_simulationClient.SelectedIndex < 0 || _simulationClient.SelectedIndex >= ClientProfileCatalog.All.Count)
                throw new InvalidOperationException("Choose a client profile.");

            var stopMode = _simulationStopMode.SelectedIndex;
            var stopValue = stopMode > 0
                ? ParseRequiredPositiveDouble(_simulationStopValue, "Stop value")
                : (double?)null;

            var options = new SimulationOptions
            {
                Torrent = _pendingTorrent,
                Tracker = tracker,
                AccountName = NullIfWhiteSpace(_simulationAccountName.Text),
                ClientProfileId = ClientProfileCatalog.All[_simulationClient.SelectedIndex].Id,
                UploadBytesPerSecond = ParseKiBPerSecond(_simulationUploadRate, "Upload rate"),
                DownloadBytesPerSecond = ParseKiBPerSecond(_simulationDownloadRate, "Download rate"),
                RandomUploadEnabled = _simulationRandomUpload.IsChecked == true,
                RandomUploadMinimumBytesPerSecond = ParseKiBPerSecond(_simulationRandomUploadMin, "Minimum random upload"),
                RandomUploadMaximumBytesPerSecond = ParseKiBPerSecond(_simulationRandomUploadMax, "Maximum random upload"),
                RandomDownloadEnabled = _simulationRandomDownload.IsChecked == true,
                RandomDownloadMinimumBytesPerSecond = ParseKiBPerSecond(_simulationRandomDownloadMin, "Minimum random download"),
                RandomDownloadMaximumBytesPerSecond = ParseKiBPerSecond(_simulationRandomDownloadMax, "Maximum random download"),
                InitialCompletedPercent = ParseDouble(_simulationCompleted, "Completed percentage"),
                Port = ParseInt(_simulationPort, "Listening port"),
                NumWant = ParseInt(_simulationNumWant, "Peers requested"),
                AnnounceIntervalSeconds = ParseInt(_simulationAnnounceInterval, "Update interval"),
                MaximumRuntime = stopMode == 1 ? TimeSpan.FromMinutes(stopValue!.Value) : null,
                MaximumUploadedBytes = stopMode == 2 ? MiBToBytes(stopValue!.Value, "Stop value") : null,
                MaximumDownloadedBytes = stopMode == 3 ? MiBToBytes(stopValue!.Value, "Stop value") : null,
                MaximumRatio = stopMode == 4 ? stopValue : null,
                Proxy = new SimulationProxyOptions
                {
                    Address = ParseOptionalUri(_simulationProxyAddress, "Proxy address"),
                    Username = NullIfWhiteSpace(_simulationProxyUsername.Text)
                }
            }.Validate();
            var savedOptions = SavedSimulationSession.FromOptions(options);
            var existingEntry = _simulationEntries.Values.FirstOrDefault(entry =>
                SavedSimulationSession.FromOptions(entry.Options) == savedOptions);
            if (existingEntry is not null)
            {
                RefreshSimulationRows(existingEntry.Session.Id);
                ShowSimulationAddFeedback(ExistingSimulationFeedback, XRatioPalette.Warning);
                AddActivity("This exact simulation already exists; selected the existing session.");
                return;
            }
            addedEntry = AddSimulationEntry(options);
            await PersistSimulationsAsync();
            ShowSimulationAddFeedback("Session added.", XRatioPalette.Positive);
            AddActivity($"Simulation added: {options.Torrent.Name}. Press Start to contact the tracker.");
        }
        catch (Exception exception)
        {
            if (addedEntry is not null)
            {
                _simulationEntries.Remove(addedEntry.Session.Id);
                addedEntry.Session.Updated -= OnSimulationUpdated;
                addedEntry.Session.Logged -= OnSimulationLogged;
                await addedEntry.Session.DisposeAsync();
                RefreshSimulationRows();
            }
            ShowSimulationAddFeedback("Could not add session.", XRatioPalette.Danger);
            AddActivity($"Could not add simulation: {exception.Message}");
        }
    }

    private void ShowSimulationAddFeedback(string message, IBrush color)
    {
        _simulationAddFeedback.Text = UiText.TranslateMessage(message, _language);
        _simulationAddFeedback.Foreground = color;
    }

    private SimulationEntry AddSimulationEntry(SimulationOptions options)
    {
        var session = new SimulationSession(options);
        var entry = new SimulationEntry(session, options);
        _simulationEntries.Add(session.Id, entry);
        session.Updated += OnSimulationUpdated;
        session.Logged += OnSimulationLogged;
        RefreshSimulationRows(session.Id);
        return entry;
    }

    private async Task LoadSimulationsAsync()
    {
        if (_simulationStore is null)
            return;
        var savedSessions = await _simulationStore.LoadAsync();
        var uniqueSessions = savedSessions.Distinct().ToArray();
        foreach (var saved in uniqueSessions)
        {
            try
            {
                AddSimulationEntry(saved.ToOptions());
            }
            catch (Exception exception)
            {
                AddActivity($"Skipped saved simulation: {exception.Message}");
            }
        }
        if (uniqueSessions.Length != savedSessions.Count)
        {
            await _simulationStore.SaveAsync(uniqueSessions);
            AddActivity($"Removed {savedSessions.Count - uniqueSessions.Length} duplicate saved simulation(s).");
        }
        if (_simulationEntries.Count > 0)
            AddActivity($"Restored {_simulationEntries.Count} stopped simulation session(s).");
    }

    private Task PersistSimulationsAsync() => _simulationStore is null
        ? Task.CompletedTask
        : _simulationStore.SaveAsync(_simulationEntries.Values
            .Select(entry => SavedSimulationSession.FromOptions(entry.Options))
            .ToArray());

    private async Task StartSelectedSimulationAsync()
    {
        if (GetSelectedSimulation() is not { } entry)
            return;
        try
        {
            await entry.Session.StartAsync();
        }
        catch (Exception exception)
        {
            AddActivity($"Simulation start failed: {exception.Message}");
        }
    }

    private async Task ToggleSelectedSimulationAsync()
    {
        if (GetSelectedSimulation() is not { } entry)
            return;

        if (ShouldShowStopAction(entry.Session.State))
            await StopSelectedSimulationAsync();
        else if (entry.Session.State is not SimulationState.Stopping)
            await StartSelectedSimulationAsync();
    }

    internal static bool ShouldShowStopAction(SimulationState state) =>
        state is SimulationState.Starting or SimulationState.Running;

    private async Task StopSelectedSimulationAsync()
    {
        if (GetSelectedSimulation() is { } entry)
            await entry.Session.StopAsync();
    }

    private async Task UpdateSelectedSimulationAsync()
    {
        if (GetSelectedSimulation() is not { } entry)
            return;
        try
        {
            await entry.Session.UpdateNowAsync();
        }
        catch (Exception exception)
        {
            AddActivity($"Simulation update failed: {exception.Message}");
        }
    }

    private async Task RemoveSelectedSimulationAsync()
    {
        if (GetSelectedSimulationRow() is not { } row ||
            !_simulationEntries.TryGetValue(row.Id, out var entry))
        {
            AddActivity("Select a simulation session first.");
            return;
        }
        if (!await ConfirmDangerousActionAsync(
                "Remove simulation",
                $"Remove the stopped simulation “{entry.Options.Torrent.Name}”? This does not delete the .torrent file.",
                "Remove"))
            return;
        if (!_simulationEntries.Remove(row.Id, out entry))
            return;
        entry.Session.Updated -= OnSimulationUpdated;
        entry.Session.Logged -= OnSimulationLogged;
        await entry.Session.DisposeAsync();
        await PersistSimulationsAsync();
        RefreshSimulationRows();
        AddActivity($"Removed simulation: {entry.Options.Torrent.Name}.");
    }

    private SimulationEntry? GetSelectedSimulation()
    {
        if (GetSelectedSimulationRow() is { } row && _simulationEntries.TryGetValue(row.Id, out var entry))
            return entry;
        AddActivity("Select a simulation session first.");
        return null;
    }

    private SimulationRow? GetSelectedSimulationRow() => _simulations.SelectedItem switch
    {
        SimulationRow row => row,
        ListBoxItem { Tag: SimulationRow row } => row,
        _ => null
    };

    private void OnSimulationUpdated(object? sender, SimulationSnapshot snapshot) =>
        Dispatcher.UIThread.Post(() => RefreshSimulationRows(snapshot.Id));

    private void OnSimulationLogged(object? sender, string message)
    {
        var name = sender is SimulationSession session ? session.Snapshot.Name : "Simulation";
        Dispatcher.UIThread.Post(() => AddActivity($"{name}: {message}"));
    }

    private void RefreshSimulationRows(Guid? selectedId = null)
    {
        if (selectedId is null && GetSelectedSimulationRow() is { } selected)
            selectedId = selected.Id;
        _simulations.Items.Clear();
        foreach (var entry in _simulationEntries.Values.OrderBy(value => value.Options.Torrent.Name, StringComparer.OrdinalIgnoreCase))
        {
            var row = new SimulationRow(entry.Session.Snapshot);
            _simulations.Items.Add(BuildSimulationListItem(row));
        }
        _simulationsEmptyState.IsVisible = _simulationEntries.Count == 0;
        StyleButton(
            _simulationAddAction,
            _simulationEntries.Count == 0 ? ButtonTone.Primary : ButtonTone.Secondary,
            minWidth: 112);
        if (selectedId is { } id)
            _simulations.SelectedItem = _simulations.Items
                .OfType<ListBoxItem>()
                .FirstOrDefault(item => item.Tag is SimulationRow row && row.Id == id);
        if (_simulations.SelectedItem is ListBoxItem selectedItem)
            selectedItem.BringIntoView();
        UpdateSimulationActionState();
        UpdateOverviewMetrics();
        // Rows are rebuilt from snapshots, so apply the active language to the
        // newly-created controls as well as the static simulation surface.
        ApplyLocalization(_simulations);
    }

    private void UpdateSimulationActionState()
    {
        var entry = GetSelectedSimulationRow() is { } row &&
                    _simulationEntries.TryGetValue(row.Id, out var selected)
            ? selected
            : null;
        _simulationActions.IsEnabled = entry is not null;
        if (entry is null)
            return;

        var state = entry.Session.State;
        var showStop = ShouldShowStopAction(state);
        _simulationPrimaryAction.Content = L(showStop ? "■  Stop" : "▶  Start");
        StyleButton(
            _simulationPrimaryAction,
            showStop ? ButtonTone.DangerStrong : ButtonTone.Primary,
            minWidth: 112);
        _simulationPrimaryAction.IsEnabled = state is not SimulationState.Stopping;
        _simulationUpdateAction.IsEnabled = state == SimulationState.Running;
        _simulationRemoveAction.IsEnabled = state is not SimulationState.Starting and not SimulationState.Stopping;
        StyleButton(_simulationRemoveAction, ButtonTone.Danger, minWidth: 104);
    }

    private static ListBoxItem BuildSimulationListItem(SimulationRow row)
    {
        var snapshot = row.Snapshot;
        var accountName = string.IsNullOrWhiteSpace(snapshot.AccountName)
            ? null
            : snapshot.AccountName.Trim();
        var identity = new StackPanel
        {
            Spacing = 1,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new TextBlock
                {
                    Text = snapshot.Name,
                    Foreground = XRatioPalette.Ink,
                    FontSize = 11.5,
                    FontWeight = FontWeight.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis
                }
            }
        };
        var trackerIdentity = accountName is null
            ? snapshot.TrackerName
            : $"{accountName} · {snapshot.TrackerName}";
        identity.Children.Add(new TextBlock
        {
            Text = trackerIdentity,
            Foreground = XRatioPalette.Subtle,
            FontSize = 9.5,
            TextTrimming = TextTrimming.CharacterEllipsis
        });
        var total = snapshot.Downloaded + snapshot.Left;
        var ratio = double.IsPositiveInfinity(snapshot.Ratio)
            ? "∞"
            : snapshot.Ratio.ToString("0.000", CultureInfo.InvariantCulture);
        var next = snapshot.NextAnnounce?.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture) ?? "—";
        var (statusText, statusForeground, statusBackground) = snapshot.State switch
        {
            SimulationState.Running => ("●  Running", XRatioPalette.Positive, XRatioPalette.PositiveSoft),
            SimulationState.Starting => ("▶  Starting", XRatioPalette.Accent, XRatioPalette.AccentSoft),
            SimulationState.Stopping => ("■  Stopping", XRatioPalette.Danger, XRatioPalette.DangerSoft),
            SimulationState.Faulted => ("!  Error", XRatioPalette.Danger, XRatioPalette.DangerSoft),
            _ => ("■  Stopped", XRatioPalette.Muted, XRatioPalette.NeutralSoft)
        };
        var progress = new ProgressBar
        {
            Minimum = 0,
            Maximum = 100,
            Value = snapshot.CompletionPercent,
            Height = 4,
            Background = XRatioPalette.Border,
            Foreground = snapshot.CompletionPercent >= 100 ? XRatioPalette.Positive : XRatioPalette.Accent,
            VerticalAlignment = VerticalAlignment.Center
        };
        var content = new StackPanel
        {
            Spacing = 3,
            Margin = new Thickness(4, 3),
            Children =
            {
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                    ColumnSpacing = 8,
                    Children =
                    {
                        new Border
                        {
                            Background = statusBackground,
                            CornerRadius = new CornerRadius(4),
                            Padding = new Thickness(6, 2),
                            Child = new TextBlock
                            {
                                Text = statusText,
                                Foreground = statusForeground,
                                FontSize = 9,
                                FontWeight = FontWeight.SemiBold
                            }
                        },
                        Place(identity, column: 1)
                    }
                },
                new Border
                {
                    Background = XRatioPalette.MetricSurface,
                    BorderBrush = XRatioPalette.Border,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Child = new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("0.72*,1.25*,1.25*,1.18*,0.9*"),
                        Children =
                        {
                            BuildSimulationMetric("Ratio", ratio, null, divider: true),
                            Place(BuildSimulationMetric(
                                "Uploaded",
                                $"↑  {FormatBytes(snapshot.Uploaded)}",
                                $"{FormatBytes(snapshot.UploadRate)}/s",
                                divider: true), column: 1),
                            Place(BuildSimulationMetric(
                                "Downloaded",
                                $"↓  {FormatBytes(snapshot.Downloaded)}",
                                $"{FormatBytes(snapshot.DownloadRate)}/s",
                                divider: true), column: 2),
                            Place(BuildSimulationMetric(
                                "Peers",
                                $"{snapshot.Seeders} seeders",
                                $"{snapshot.Leechers} leechers",
                                divider: true), column: 3),
                            Place(BuildSimulationMetric("Next announce", next, null, divider: false), column: 4)
                        }
                    }
                },
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    ColumnSpacing = 8,
                    Children =
                    {
                        new StackPanel
                        {
                            Spacing = 2,
                            Children =
                            {
                                new TextBlock
                                {
                                    Text = $"Downloaded {FormatBytes(snapshot.Downloaded)} of {FormatBytes(total)}",
                                    Foreground = XRatioPalette.Subtle,
                                    FontSize = 8.5,
                                    FontFeatures = XRatioPalette.TabularNumbers
                                },
                                progress
                            }
                        },
                        Place(new TextBlock
                        {
                            Text = $"{snapshot.CompletionPercent:0.0}%",
                            Foreground = XRatioPalette.Ink,
                            FontSize = 9.5,
                            FontWeight = FontWeight.SemiBold,
                            FontFeatures = XRatioPalette.TabularNumbers,
                            VerticalAlignment = VerticalAlignment.Bottom
                        }, column: 1)
                    }
                }
            }
        };
        return new ListBoxItem
        {
            Tag = row,
            Content = content,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(0),
            MinHeight = 80
        };
    }

    private static Border BuildSimulationMetric(
        string label,
        string value,
        string? detail,
        bool divider)
    {
        var stack = new StackPanel
        {
            Spacing = 0,
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    Foreground = XRatioPalette.Subtle,
                    FontSize = 7.5,
                    FontWeight = FontWeight.Medium
                },
                new TextBlock
                {
                    Text = detail is null ? value : $"{value}  ·  {detail}",
                    Foreground = XRatioPalette.Ink,
                    FontSize = 9.5,
                    FontWeight = FontWeight.SemiBold,
                    FontFeatures = XRatioPalette.TabularNumbers,
                    TextTrimming = TextTrimming.CharacterEllipsis
                }
            }
        };
        return new Border
        {
            BorderBrush = XRatioPalette.Border,
            BorderThickness = divider ? new Thickness(0, 0, 1, 0) : new Thickness(0),
            Padding = new Thickness(6, 2),
            MinHeight = 30,
            Child = stack
        };
    }

    private async Task StopAllSimulationsAsync()
    {
        foreach (var entry in _simulationEntries.Values)
        {
            entry.Session.Updated -= OnSimulationUpdated;
            entry.Session.Logged -= OnSimulationLogged;
            await entry.Session.DisposeAsync();
        }
    }

    private static long ParseKiBPerSecond(TextBox input, string name)
    {
        var value = ParseDouble(input, name);
        if (value < 0 || value > SimulationOptions.MaximumTransferRateBytesPerSecond / 1024d)
            throw new ArgumentOutOfRangeException(name, "Rate is outside the supported range.");
        return checked((long)Math.Round(value * 1024));
    }

    private Control BuildTorrentsTab()
    {
        var copyHash = new MenuItem { Header = "Copy Info Hash" };
        copyHash.Click += async (_, _) => await CopySelectedTorrentHashAsync();
        var resetStatistics = new MenuItem { Header = "Reset Statistics" };
        resetStatistics.Click += async (_, _) => await ResetSelectedTorrentAsync();
        ConfigureContextMenuItem(copyHash);
        ConfigureContextMenuItem(resetStatistics);
        _torrents.ContextMenu = new ContextMenu
        {
            Items = { copyHash, resetStatistics },
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(4)
        };

        ConfigureList(_torrents);
        _torrentsEmptyState.IsHitTestVisible = false;
        _torrentsEmptyState.HorizontalAlignment = HorizontalAlignment.Center;
        _torrentsEmptyState.VerticalAlignment = VerticalAlignment.Center;
        _torrentsEmptyState.Spacing = 4;
        _torrentsEmptyState.Children.Add(new TextBlock
        {
            Text = "No tracked torrents yet",
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = XRatioPalette.Ink,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        _torrentsEmptyState.Children.Add(new TextBlock
        {
            Text = "Tracker announcements will appear here automatically.",
            FontSize = 12,
            Foreground = XRatioPalette.Muted,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        var torrentSurface = new Border
        {
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(12),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                RowSpacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Hash · tracker · peers · status · transfer counters · last announce",
                        FontSize = 12,
                        Foreground = XRatioPalette.Muted,
                        FontFeatures = XRatioPalette.TabularNumbers,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    },
                    Place(_torrents, row: 1),
                    Place(_torrentsEmptyState, row: 1)
                }
            }
        };
        return BuildTabLayout(
            "Interception",
            "Tracked sessions stay visible here as announcements arrive.",
            torrentSurface);
    }

    private Control BuildOptionsTab()
    {
        ConfigureTextBox(_port, "e.g. 3773");
        ConfigureTextBox(_minimumPeers, "e.g. 5");
        ConfigureTextBox(_downloadRatioMin, "e.g. 0");
        ConfigureTextBox(_downloadRatioMax, "e.g. 0.05");
        ConfigureTextBox(_uploadRatioMin, "e.g. 4");
        ConfigureTextBox(_uploadRatioMax, "e.g. 8");
        ConfigureTextBox(_boost, "e.g. 15");
        ConfigureTextBox(_boostChance, "e.g. 5");
        _onlyTrackers.Content = "Accept tracker traffic only";
        _onlyLocal.Content = "Listen on localhost only (required)";
        _proxyDebugLogging.Content = "Write redacted proxy debug log";
        _noDownload.Content = "Report download as zero";
        _pretendSeed.Content = "Pretend to seed";
        ConfigureCheckBox(_onlyTrackers);
        ConfigureCheckBox(_onlyLocal);
        _onlyLocal.IsEnabled = false;
        ConfigureCheckBox(_proxyDebugLogging);
        ConfigureCheckBox(_noDownload);
        ConfigureCheckBox(_pretendSeed);
        ConfigureComboBox(_themeMode, 180);
        _themeMode.ItemsSource = ThemePalette.Options;
        _themeMode.SelectedIndex = 0;
        _themeMode.SelectionChanged += (_, _) =>
        {
            ApplyTheme(SelectedThemeMode(), SelectedAccentColor());
            if (!_suppressLanguageSelection)
                MarkSettingsDirty();
        };
        ConfigureComboBox(_accentColor, 180);
        _accentColor.ItemsSource = AccentPalette.Options;
        _accentColor.SelectedIndex = 0;
        _accentColor.SelectionChanged += (_, _) =>
        {
            ApplyTheme(SelectedThemeMode(), SelectedAccentColor());
            if (!_suppressLanguageSelection)
                MarkSettingsDirty();
        };
        ConfigureComboBox(_trayIconStyle, 180);
        _trayIconStyle.ItemsSource = TrayIconStyles;
        _trayIconStyle.SelectedIndex = 0;
        _trayIconStyle.SelectionChanged += (_, _) =>
        {
            if (!_suppressLanguageSelection)
                MarkSettingsDirty();
        };
        ConfigureComboBox(_languageMode, 180);
        _languageMode.ItemTemplate = new FuncDataTemplate<string>((value, _) => BuildLanguageOption(value));
        _languageMode.ItemsSource = UiText.LanguageLabels;
        _languageMode.SelectedIndex = 0;
        _languageMode.SelectionChanged += (_, _) =>
        {
            if (_suppressLanguageSelection)
                return;
            _language = SelectedLanguage();
            ApplyLocalization();
            RefreshTorrents();
            RefreshSimulationRows();
            MarkSettingsDirty();
        };
        HookSettingsDirtyState();

        StyleButton(_checkUpdates, ButtonTone.Secondary, minWidth: 190);
        _checkUpdates.Content = "Check for updates";
        _checkUpdates.Click += async (_, _) => await CheckForUpdatesAsync(startup: false);
        StyleButton(_downloadUpdate, ButtonTone.Primary, minWidth: 170);
        _downloadUpdate.Content = "Download update";
        _downloadUpdate.IsVisible = false;
        _downloadUpdate.Click += async (_, _) => await OpenLatestReleaseAsync();
        _updateStatus.Text = "Not checked yet";
        _updateStatus.Foreground = XRatioPalette.Muted;
        _updateStatus.FontSize = 12;
        _updateStatus.VerticalAlignment = VerticalAlignment.Center;
        _updateStatus.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        var githubRepository = BuildGitHubRepositoryButton();

        var versionDisplay = new Border
        {
            Background = XRatioPalette.AccentSoft,
            BorderBrush = XRatioPalette.Accent,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(99),
            Padding = new Thickness(10, 4),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new TextBlock
            {
                Text = AppVersion.Display,
                Foreground = XRatioPalette.Accent,
                FontSize = 12.5,
                FontWeight = FontWeight.SemiBold,
                FontFeatures = XRatioPalette.TabularNumbers,
                VerticalAlignment = VerticalAlignment.Center
            }
        };

        var updates = BuildSettingsSection(
            "Updates",
            "Check the official GitHub release without changing files automatically.",
            BuildSettingsBody(
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 18,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Current version",
                            FontSize = 13,
                            Foreground = XRatioPalette.Ink,
                            Width = 220,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        versionDisplay
                    }
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 12,
                    Children = { _checkUpdates, _downloadUpdate, githubRepository, _updateStatus }
                }),
            bottomPadding: 10);

        var appearance = BuildSettingsSection(
            "Appearance",
            "Choose the visual mode and signal color for the XRatio control plane. Blue is the default; the hierarchy stays the same in all themes.",
            BuildSettingsBody(
                BuildFieldGrid(
                    ("Theme", (Control)_themeMode),
                    ("Accent color", (Control)_accentColor),
                    ("Tray icon", (Control)_trayIconStyle),
                    ("Language", (Control)_languageMode)),
                new TextBlock
                {
                    Text = "Choose the language used by the XRatio interface.",
                    Foreground = XRatioPalette.Muted,
                    FontSize = 11.5,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                },
                new TextBlock
                {
                    Text = "Color mode uses a red X when stopped and orange when paused; Monochrome keeps the whole icon neutral.",
                    Foreground = XRatioPalette.Muted,
                    FontSize = 11.5,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }));

        var connection = BuildSettingsSection(
            "Connection",
            "Use a free localhost port from 1 to 65534. Minimum leechers must be between 0 and 100.",
            BuildSettingsBody(
                BuildFieldGrid(
                    ("HTTP proxy port", (Control)_port),
                    ("Minimum leechers", (Control)_minimumPeers)),
                BuildToggleGroup(_onlyTrackers, _onlyLocal, _proxyDebugLogging)));

        var ratio = BuildSettingsSection(
            "Ratio shaping",
            "Minimum values must not exceed maximum values. Multipliers and boost values cannot be negative.",
            BuildFieldGrid(
                ("Upload/download multiplier min", (Control)_downloadRatioMin),
                ("Upload/download multiplier max", (Control)_downloadRatioMax),
                ("Upload/upload multiplier min", (Control)_uploadRatioMin),
                ("Upload/upload multiplier max", (Control)_uploadRatioMax),
                ("Boost maximum (KiB/s)", (Control)_boost),
                ("Boost chance (%)", (Control)_boostChance)));

        var announce = BuildSettingsSection(
            "Announce behavior",
            "Choose the information the proxy reports to trackers.",
            BuildToggleGroup(_noDownload, _pretendSeed));

        var content = new StackPanel
        {
            Spacing = 14,
            Margin = new Thickness(28, 24, 28, 0),
            MaxWidth = 820,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                BuildTabHeading("Settings", "Tune the proxy while keeping safe defaults."),
                appearance,
                connection,
                ratio,
                announce,
                updates
            }
        };
        _settingsSaveStatus.Text = "Loading settings…";
        _settingsSaveStatus.FontSize = 12;
        _settingsSaveStatus.Foreground = XRatioPalette.Muted;
        _settingsSaveStatus.VerticalAlignment = VerticalAlignment.Center;
        var actionBar = new Border
        {
            Background = XRatioPalette.Topbar,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(28, 10),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Children = { _settingsSaveAction, _settingsSaveStatus }
            }
        };
        return new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Children =
            {
                new ScrollViewer
                {
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Content = content
                },
                Place(actionBar, row: 1)
            }
        };
    }

    private void HookSettingsDirtyState()
    {
        foreach (var input in new[]
                 {
                     _port, _minimumPeers, _downloadRatioMin, _downloadRatioMax,
                     _uploadRatioMin, _uploadRatioMax, _boost, _boostChance
                 })
            input.TextChanged += (_, _) => MarkSettingsDirty();

        foreach (var checkBox in new[]
                 {
                     _onlyTrackers, _proxyDebugLogging, _noDownload, _pretendSeed
                 })
            checkBox.PropertyChanged += (_, args) =>
            {
                if (args.Property == ToggleButton.IsCheckedProperty)
                    MarkSettingsDirty();
            };
    }

    private void MarkSettingsDirty()
    {
        if (_suppressSettingsDirty || !_settingsLoaded)
            return;
        _settingsDirty = true;
        try
        {
            ValidateSettingsRanges(ReadForm()).Validate();
            _settingsSaveStatus.Text = L("Unsaved changes");
            _settingsSaveStatus.Foreground = XRatioPalette.Warning;
            _settingsSaveAction.IsEnabled = true;
        }
        catch (Exception exception)
        {
            _settingsSaveStatus.Text = L(FriendlyValidationMessage(exception));
            _settingsSaveStatus.Foreground = XRatioPalette.Danger;
            _settingsSaveAction.IsEnabled = false;
        }
    }

    private async Task CheckForUpdatesAsync(bool startup)
    {
        if (!await _updateCheckGate.WaitAsync(0))
            return;

        try
        {
            if (startup || !_exiting)
                _updateStatus.Text = L("Checking for updates…");
            _updateStatus.Foreground = XRatioPalette.Muted;
            _checkUpdates.IsEnabled = false;
            _downloadUpdate.IsVisible = false;
            _latestReleaseUri = null;
            _latestDownloadUri = null;

            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(9));
            var result = await UpdateChecker.CheckAsync(AppVersion.Current, cancellation.Token);
            if (_exiting)
                return;

            if (result.Error is not null)
            {
                _updateStatus.Text = L("Unable to check for updates");
                _updateStatus.Foreground = XRatioPalette.Warning;
                return;
            }

            if (result.IsUpdateAvailable && result.LatestVersion is not null)
            {
                _latestReleaseUri = result.ReleaseUri;
                _latestDownloadUri = result.DownloadUri;
                _updateStatus.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    L("Update available: {0}"),
                    $"v{result.LatestVersion}");
                _updateStatus.Foreground = XRatioPalette.Accent;
                _downloadUpdate.Content = L("Download update");
                _downloadUpdate.IsVisible = _latestDownloadUri is not null || _latestReleaseUri is not null;
                if (_latestReleaseUri is not null)
                    ToolTip.SetTip(_updateStatus, _latestReleaseUri.ToString());
            }
            else
            {
                _updateStatus.Text = L("You are up to date");
                _updateStatus.Foreground = XRatioPalette.Positive;
                _downloadUpdate.IsVisible = false;
                ToolTip.SetTip(_updateStatus, null);
            }
        }
        catch (Exception) when (!_exiting)
        {
            _updateStatus.Text = L("Unable to check for updates");
            _updateStatus.Foreground = XRatioPalette.Warning;
            _downloadUpdate.IsVisible = false;
        }
        finally
        {
            if (!_exiting)
            {
                _checkUpdates.IsEnabled = true;
                _checkUpdates.Content = L("Check for updates");
            }
            _updateCheckGate.Release();
        }
    }

    private async Task OpenLatestReleaseAsync()
    {
        var uri = _latestDownloadUri ?? _latestReleaseUri;
        if (uri is null)
            return;

        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher is null || !await launcher.LaunchUriAsync(uri))
            {
                _updateStatus.Text = L("Could not open update download");
                _updateStatus.Foreground = XRatioPalette.Warning;
            }
        }
        catch (Exception) when (!_exiting)
        {
            _updateStatus.Text = L("Could not open update download");
            _updateStatus.Foreground = XRatioPalette.Warning;
        }
    }

    private Button BuildGitHubRepositoryButton(double size = 36)
    {
        var button = new Button
        {
            Content = new PathIcon
            {
                Data = StreamGeometry.Parse(
                    "M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61-.546-1.387-1.333-1.757-1.333-1.757-1.089-.745.084-.729.084-.729 1.205.084 1.84 1.236 1.84 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.21 0 1.595-.015 2.875-.015 3.265 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12z"),
                Width = 16,
                Height = 16,
                Foreground = XRatioPalette.Muted
            }
        };
        ConfigureGuideButton(button);
        if (size != 36)
        {
            button.Width = size;
            button.MinWidth = size;
            button.CornerRadius = new CornerRadius(Math.Min(size, 36) / 2);
            if (button.Content is PathIcon icon)
            {
                icon.Width = size * 0.5;
                icon.Height = size * 0.5;
            }
        }
        ToolTip.SetTip(button, "Open XRatio on GitHub");
        button.Click += async (_, _) => await OpenRepositoryAsync();
        return button;
    }

    private async Task OpenRepositoryAsync()
    {
        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher is null || !await launcher.LaunchUriAsync(new Uri(RepositoryUrl)))
                _updateStatus.Text = L("Could not open GitHub repository");
        }
        catch (Exception) when (!_exiting)
        {
            _updateStatus.Text = L("Could not open GitHub repository");
        }
    }

    private void MarkSettingsSaved()
    {
        _settingsDirty = false;
        _settingsSaveStatus.Text = L("All changes saved");
        _settingsSaveStatus.Foreground = XRatioPalette.Positive;
        _settingsSaveAction.IsEnabled = false;
    }

    internal static XRatioSettings ValidateSettingsRanges(XRatioSettings settings)
    {
        if (settings.UploadPerDownloadMinimum > settings.UploadPerDownloadMaximum)
            throw new ArgumentException("Upload/download minimum cannot exceed its maximum.");
        if (settings.UploadPerUploadMinimum > settings.UploadPerUploadMaximum)
            throw new ArgumentException("Upload/upload minimum cannot exceed its maximum.");
        return settings;
    }

    private static string FriendlyValidationMessage(Exception exception) => exception switch
    {
        ArgumentOutOfRangeException { ParamName: nameof(XRatioSettings.ListenPort) } =>
            "Proxy port must be between 1 and 65534.",
        ArgumentOutOfRangeException { ParamName: nameof(XRatioSettings.MinimumPeers) } =>
            "Minimum leechers must be between 0 and 100.",
        ArgumentOutOfRangeException { ParamName: nameof(XRatioSettings.BoostChancePercent) } =>
            "Boost chance must be between 0 and 100%.",
        ArgumentOutOfRangeException { ParamName: nameof(XRatioSettings) } =>
            "Multipliers and boost values cannot be negative.",
        _ => exception.Message
    };

    private Control BuildPlatformTab()
    {
        _autoStart.Content = "Start automatically with the user session";
        ConfigureCheckBox(_autoStart);
        _autoStart.IsEnabled = _autostart.Capability.IsSupported;
        _showTrayIcon.Content = "Show icon in notification area";
        ConfigureCheckBox(_showTrayIcon);
        _showTrayIcon.IsEnabled = IsTrayAvailable();
        _startMinimized.Content = "Start minimized to tray";
        ConfigureCheckBox(_startMinimized);
        _startMinimized.IsEnabled = IsTrayAvailable();
        _showTrayIcon.PropertyChanged += (_, args) =>
        {
            if (args.Property != ToggleButton.IsCheckedProperty)
                return;
            _startMinimized.IsEnabled = IsTrayAvailable() && _showTrayIcon.IsChecked == true;
            if (_showTrayIcon.IsChecked != true)
                _startMinimized.IsChecked = false;
            MarkSettingsDirty();
        };
        _startMinimized.PropertyChanged += (_, args) =>
        {
            if (args.Property == ToggleButton.IsCheckedProperty)
                MarkSettingsDirty();
        };
        _certificateConsent.Content =
            "I understand that XRatio will add its installation CA to my Windows user trust store.";
        ConfigureCheckBox(_certificateConsent);
        _certificateConsent.IsVisible = _certificates.Capability.IsSupported;
        _trustCertificate.Content = "Trust CA and enable";
        StyleButton(_trustCertificate, ButtonTone.Primary, 168);
        _trustCertificate.IsEnabled = false;
        _trustCertificate.Click += async (_, _) => await EnableHttpsAsync();
        _removeCertificate.Content = "Remove CA trust…";
        StyleButton(_removeCertificate, ButtonTone.Danger, 152);
        _removeCertificate.IsEnabled = _certificates.Capability.IsSupported;
        _removeCertificate.Click += async (_, _) => await DisableHttpsAsync();
        _certificateStatus.Foreground = XRatioPalette.Ink;
        _certificateStatus.FontSize = 14;
        _certificateStatus.FontWeight = FontWeight.SemiBold;
        _certificateStatus.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _certificateStatusDetail.Foreground = XRatioPalette.Muted;
        _certificateStatusDetail.FontSize = 12;
        _certificateStatusDetail.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _certificateConsent.PropertyChanged += (_, args) =>
        {
            if (args.Property == ToggleButton.IsCheckedProperty)
                _trustCertificate.IsEnabled = _certificates.Capability.IsSupported &&
                                                    _certificateConsent.IsVisible &&
                                                    _certificateConsent.IsChecked == true;
        };
        var startup = BuildSettingsSection(
            "Startup",
            "Choose how XRatio should behave when your session begins.",
            BuildSettingsBody(
                new TextBlock
                {
                    Text = $"Autostart: {_autostart.Capability.Description}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = XRatioPalette.Muted,
                    FontSize = 12
                },
                _autoStart,
                _showTrayIcon,
                _startMinimized));

        var certificateActions = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children = { _trustCertificate, _removeCertificate }
        };
        var https = BuildSettingsSection(
            "HTTPS interception",
            "Trust is explicit and scoped to the current Windows user.",
            BuildSettingsBody(
                new TextBlock
                {
                    Text = $"Certificates: {_certificates.Capability.Description}",
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Foreground = XRatioPalette.Muted,
                    FontSize = 12
                },
                _certificateStatus,
                _certificateStatusDetail,
                _certificateConsent,
                certificateActions));

        var content = new StackPanel
        {
            Margin = new Thickness(28, 24),
            Spacing = 14,
            MaxWidth = 820,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                BuildTabHeading("Platform", "System integrations and HTTPS trust live here."),
                startup,
                https
            }
        };
        return new ScrollViewer
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Content = content
        };
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        if (_startupInitializationStarted)
            return;
        _startupInitializationStarted = true;

        try
        {
            _sessionStarted = DateTimeOffset.UtcNow;
            var loadedSettings = await _store.LoadAsync();
            _settings = SessionStatistics.StartSession(loadedSettings);
            _language = UiText.Normalize(_settings.Language);
            ApplyTheme(_settings.ThemeMode, _settings.AccentColor);
            _transformer.Restore(_settings.PersistedTorrents);
            await _store.SaveAsync(_settings);
            _suppressSettingsDirty = true;
            try
            {
                PopulateForm(_settings);
            }
            finally
            {
                _suppressSettingsDirty = false;
            }
            ApplyLocalization();
            await RestoreSimulationFormAsync(_settings.SimulationForm);
            _settingsLoaded = true;
            MarkSettingsSaved();
            await LoadSimulationsAsync();
            AddActivity(_store.LastLoadSource switch
            {
                SettingsLoadSource.LegacyTcl =>
                    "Imported settings.dat into settings.json; the Tcl file was left unchanged.",
                SettingsLoadSource.LegacyTclBackup =>
                    "Imported settings.dat.bak after the primary Tcl settings were invalid; both Tcl files were left unchanged.",
                SettingsLoadSource.JsonBackup =>
                    "Loaded the JSON settings backup because settings.json was invalid.",
                SettingsLoadSource.Defaults => "Using default configuration.",
                _ => "Configuration loaded."
            });
            if (_autostart.Capability.IsSupported)
                _autoStart.IsChecked = await _autostart.IsEnabledAsync();
            await RefreshCertificateStatusAsync();
            await StartProxyAsync();
            // Initialization can update bound controls asynchronously. Reassert
            // the clean baseline once the first runtime state is ready.
            MarkSettingsSaved();
            _ = CheckForUpdatesAsync(startup: true);
            if (ShouldHideAfterStartup(
                    IsTrayIconEnabled,
                    _settings.StartMinimized,
                    Environment.GetCommandLineArgs().Contains("--minimized"),
                    _restoreRequested))
                Hide();
        }
        catch (Exception exception)
        {
            ShowStartupFailure(exception);
        }
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (_exiting)
            return;
        e.Cancel = true;
        if (ShouldHideOnWindowClose(IsTrayIconEnabled))
        {
            Hide();
            return;
        }

        // A non-Windows platform currently has no validated native tray
        // backend. Closing its only window must therefore exit instead of
        // hiding the process with no discoverable way to restore it.
        await PrepareForExitAsync();
    }

    public async Task ToggleProxyAsync()
    {
        try
        {
            if (_proxy?.IsRunning == true)
                await StopProxyAsync();
            else
                await StartProxyAsync();
        }
        catch (Exception exception)
        {
            ShowStartupFailure(exception);
            _toggle.Content = L("Start");
            StyleButton(_toggle, ButtonTone.Primary, minWidth: 72);
            UpdateOverviewMetrics();
        }
    }

    public void ShowFromTray()
    {
        if (_exiting)
            return;

        _restoreRequested = true;
        _tabs.SelectedIndex = 0;
        WindowState = WindowState.Normal;
        Show();
        Activate();
    }

    public void TogglePause()
    {
        if (_proxy?.IsRunning != true)
            return;

        _paused = !_paused;
        _pause.Content = L(_paused ? "Resume" : "Pause");
        _status.Text = L(_paused
            ? $"Paused on 127.0.0.1:{_proxy?.BoundPort ?? _settings.ListenPort}"
            : _proxy?.IsRunning == true
                ? $"HTTP/HTTPS active on 127.0.0.1:{_proxy.BoundPort}"
                : "Proxy stopped");
        AddActivity(_paused
            ? "Rewriting paused; counters will not regress below previously reported values."
            : "Rewriting resumed.");
        UpdateOverviewMetrics();
        NotifyRuntimeStateChanged();
    }

    public async Task PrepareForExitAsync()
    {
        _exiting = true;
        _simulationFormSaveCancellation?.Cancel();
        if (_settingsLoaded)
            _settings = _settings with { SimulationForm = CaptureSimulationFormSettings() };
        await PersistSimulationsAsync();
        await StopAllSimulationsAsync();
        await PersistSessionTotalsAsync();
        await StopProxyAsync();
        if (_certificates is IDisposable disposableCertificates)
            disposableCertificates.Dispose();
        Close();
        _shutdown();
    }

    private async Task PersistSessionTotalsAsync()
    {
        if (_sessionPersisted)
            return;
        _sessionPersisted = true;
        await _settingsSaveGate.WaitAsync();
        try
        {
            var snapshots = _transformer.GetSnapshots();
            _settings = _settings with
            {
                PersistedTorrents = _transformer.GetPersistedStates()
            };
            _settings = SessionStatistics.AddSessionTotals(
                _settings,
                snapshots,
                DateTimeOffset.UtcNow - _sessionStarted);
            await _store.SaveAsync(_settings);
        }
        finally
        {
            _settingsSaveGate.Release();
        }
    }

    private async Task EnableHttpsAsync()
    {
        if (_certificateConsent.IsChecked != true)
        {
            AddActivity("HTTPS was not enabled: explicit CA trust confirmation is required.");
            return;
        }
        try
        {
            await _certificates.RequestTrustAsync();
            await RefreshCertificateStatusAsync();
            AddActivity("HTTPS interception enabled for the current Windows user.");
        }
        catch (Exception exception)
        {
            AddActivity($"Could not enable HTTPS: {exception.Message}");
        }
    }

    private async Task DisableHttpsAsync()
    {
        if (!await ConfirmDangerousActionAsync(
                "Remove CA trust",
                "Remove XRatio's CA from the current Windows user trust store? HTTPS tracker interception will stop.",
                "Remove trust"))
            return;
        try
        {
            await _certificates.RemoveTrustAsync();
            _certificateConsent.IsChecked = false;
            await RefreshCertificateStatusAsync();
            AddActivity("XRatio CA trust removed from the current Windows user.");
        }
        catch (Exception exception)
        {
            AddActivity($"Could not remove CA trust: {exception.Message}");
        }
    }

    private async Task RefreshCertificateStatusAsync()
    {
        if (!_certificates.Capability.IsSupported)
        {
            _certificateStatus.Text = L("Unavailable on this platform");
            _certificateStatus.Foreground = XRatioPalette.Muted;
            _certificateStatusDetail.Text = L("XRatio cannot install or inspect a user-scoped CA here.");
            _certificateConsent.IsVisible = false;
            _trustCertificate.IsVisible = false;
            _removeCertificate.IsVisible = false;
            return;
        }

        var trusted = await _certificates.IsTrustedAsync();
        _certificateStatus.Text = L(trusted ? "Trusted and enabled" : "Not trusted — HTTPS interception is off");
        _certificateStatus.Foreground = trusted ? XRatioPalette.Positive : XRatioPalette.Warning;
        _certificateStatusDetail.Text = L(trusted
            ? "The installation CA is trusted for the current Windows user, so HTTPS tracker announces can be inspected."
            : "HTTP interception still works. Trust the installation CA only if you need HTTPS tracker interception.");
        _certificateConsent.IsVisible = !trusted;
        _trustCertificate.IsVisible = !trusted;
        _trustCertificate.IsEnabled = !trusted && _certificateConsent.IsChecked == true;
        _removeCertificate.IsVisible = trusted;
        _removeCertificate.IsEnabled = trusted;
    }

    private async Task SaveAndApplyAsync()
    {
        try
        {
            var previousPort = _settings.ListenPort;
            var previousLocalOnly = _settings.OnlyLocalConnections;
            _settings = ValidateSettingsRanges(ReadForm()).Validate();
            await SaveSettingsAsync();
            if (_autostart.Capability.IsSupported)
                await _autostart.SetEnabledAsync(_settings.AutoStart);
            if (_proxy?.IsRunning == true &&
                (previousPort != _settings.ListenPort || previousLocalOnly != _settings.OnlyLocalConnections))
            {
                await StopProxyAsync();
                await StartProxyAsync();
            }
            UpdateTrayAvailabilityControls();
            NotifyRuntimeStateChanged();
            MarkSettingsSaved();
            AddActivity("Configuration saved.");
        }
        catch (Exception exception)
        {
            _settingsSaveStatus.Text = L(FriendlyValidationMessage(exception));
            _settingsSaveStatus.Foreground = XRatioPalette.Danger;
            _settingsSaveAction.IsEnabled = _settingsDirty;
            AddActivity($"Configuration error: {exception.Message}");
        }
    }

    private async Task StartProxyAsync()
    {
        if (_proxy?.IsRunning == true)
            return;
        _proxy = new HttpProxyServer(
            _transformer,
            () => _settings,
            _certificates,
            isPaused: () => _paused,
            debugLogger: _debugLogger,
            isDebugLogging: () => _settings.ProxyDebugLogging);
        _proxy.Activity += OnProxyActivity;
        try
        {
            await _proxy.StartAsync();
            ClearStartupFailure();
            _toggle.Content = L("Stop");
            StyleButton(_toggle, ButtonTone.Danger, minWidth: 72);
            _status.Text = L($"HTTP/HTTPS active on 127.0.0.1:{_proxy.BoundPort}");
            UpdateOverviewMetrics();
            NotifyRuntimeStateChanged();
        }
        catch
        {
            var failedProxy = _proxy;
            _proxy = null;
            try
            {
                await failedProxy.DisposeAsync();
            }
            catch (Exception cleanupException)
            {
                AddActivity($"Proxy cleanup error: {cleanupException.Message}");
            }
            throw;
        }
    }

    private async Task StopProxyAsync()
    {
        if (_proxy is null)
            return;
        _proxy.Activity -= OnProxyActivity;
        await _proxy.DisposeAsync();
        _proxy = null;
        _toggle.Content = L("Start");
        StyleButton(_toggle, ButtonTone.Primary, minWidth: 72);
        _status.Text = L("Proxy stopped");
        UpdateOverviewMetrics();
        NotifyRuntimeStateChanged();
    }

    private void ShowStartupFailure(Exception exception)
    {
        var detail = DescribeStartupFailure(exception, _settings.ListenPort);
        _startupFailureDetail.Text = L(detail);
        _startupFailureBanner.IsVisible = true;
        _status.Text = L("Interception needs attention");
        _status.Foreground = XRatioPalette.Danger;
        _statusIndicator.Background = XRatioPalette.Danger;
        _toggle.Content = L("Retry");
        AddActivity($"Startup error: {detail}", ActivityLevel.Error, "Startup");
        UpdateOverviewMetrics();
        NotifyRuntimeStateChanged();
    }

    private void NotifyRuntimeStateChanged() =>
        RuntimeStateChanged?.Invoke(IsProxyRunning, IsProxyPaused);

    private void ClearStartupFailure()
    {
        _startupFailureBanner.IsVisible = false;
        _status.Foreground = XRatioPalette.Muted;
        _statusIndicator.Background = XRatioPalette.Positive;
    }

    private void OnProxyActivity(object? sender, ProxyEvent activity) =>
        Dispatcher.UIThread.Post(() =>
        {
            AddActivity(
                $"{activity.Disposition}: {activity.Message}",
                activity.Disposition.ToString().Contains("fail", StringComparison.OrdinalIgnoreCase)
                    ? ActivityLevel.Error
                    : ActivityLevel.Info,
                "Proxy",
                activity.Timestamp);
            RefreshTorrents();
            RequestTorrentStatePersistence();
        });

    private void RequestTorrentStatePersistence()
    {
        if (_exiting)
            return;
        Volatile.Write(ref _torrentPersistenceRequested, 1);
        if (Interlocked.CompareExchange(ref _torrentPersistenceWriterRunning, 1, 0) == 0)
            _ = RunTorrentStatePersistenceWriterAsync();
    }

    private async Task RunTorrentStatePersistenceWriterAsync()
    {
        try
        {
            do
            {
                Interlocked.Exchange(ref _torrentPersistenceRequested, 0);
                await Task.Delay(TimeSpan.FromMilliseconds(250));
                await PersistTorrentStatesAsync();
            }
            while (!_exiting && Volatile.Read(ref _torrentPersistenceRequested) != 0);
        }
        finally
        {
            Interlocked.Exchange(ref _torrentPersistenceWriterRunning, 0);
            if (!_exiting && Volatile.Read(ref _torrentPersistenceRequested) != 0)
                RequestTorrentStatePersistence();
        }
    }

    private async Task PersistTorrentStatesAsync()
    {
        if (_exiting)
            return;

        await _settingsSaveGate.WaitAsync();
        try
        {
            if (_exiting)
                return;

            _settings = _settings with
            {
                PersistedTorrents = _transformer.GetPersistedStates()
            };
            await _store.SaveAsync(_settings);
        }
        catch (Exception exception)
        {
            Dispatcher.UIThread.Post(() => AddActivity($"State persistence error: {exception.Message}"));
        }
        finally
        {
            _settingsSaveGate.Release();
        }
    }

    private async Task SaveSettingsAsync()
    {
        await _settingsSaveGate.WaitAsync();
        try
        {
            await _store.SaveAsync(_settings);
        }
        finally
        {
            _settingsSaveGate.Release();
        }
    }

    private void RefreshTorrents()
    {
        var selectedHash = (_torrents.SelectedItem as TorrentRow)?.Snapshot.InfoHash;
        _torrents.Items.Clear();
        foreach (var torrent in _transformer.GetSnapshots())
        {
            var row = new TorrentRow(torrent, L(GetTorrentStatus(torrent)));
            _torrents.Items.Add(row);
            if (torrent.InfoHash == selectedHash)
                _torrents.SelectedItem = row;
        }
        _torrentsEmptyState.IsVisible = _torrents.ItemCount == 0;
        UpdateOverviewMetrics();
    }

    private string GetTorrentStatus(TorrentSnapshot torrent)
    {
        if (_paused)
            return "Paused";
        if (_transformer.IsSeedOnlyStandby)
            return "Seed-only standby";
        return torrent.IncompletePeers < _settings.MinimumPeers
            ? "Waiting for leechers"
            : "Ready";
    }

    private async Task CopySelectedTorrentHashAsync()
    {
        if (_torrents.SelectedItem is not TorrentRow row)
        {
            AddActivity("Select a torrent before copying its info hash.");
            return;
        }

        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard is null)
        {
            AddActivity("Clipboard is unavailable.");
            return;
        }
        await clipboard.SetTextAsync(row.Snapshot.InfoHash);
        AddActivity($"Copied info hash to clipboard: {row.Snapshot.InfoHash}");
    }

    private async Task ResetSelectedTorrentAsync()
    {
        if (_torrents.SelectedItem is not TorrentRow row)
        {
            AddActivity("Select a torrent before resetting its statistics.");
            return;
        }
        if (!await ConfirmResetAsync(row.Snapshot.InfoHash))
            return;

        if (_transformer.ResetTorrent(row.Snapshot.InfoHash))
        {
            RefreshTorrents();
            AddActivity($"Reset stats for torrent hash: {AbbreviateHash(row.Snapshot.InfoHash)}");
        }
    }

    private async Task<bool> ConfirmResetAsync(string infoHash)
    {
        var confirmed = false;
        var dialog = new Window
        {
            Title = "Reset Statistics",
            Width = 480,
            Height = 210,
            CanResize = false,
            Background = XRatioPalette.Canvas,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var yes = CreateButton("Reset", ButtonTone.Primary, 90);
        var cancel = CreateButton("Cancel", ButtonTone.Secondary, 90);
        yes.Click += (_, _) =>
        {
            confirmed = true;
            dialog.Close();
        };
        cancel.Click += (_, _) => dialog.Close();
        dialog.Content = new Border
        {
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(10),
            Padding = new Thickness(18),
            Child = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Reset all tracked statistics for {AbbreviateHash(infoHash)}?",
                        Foreground = XRatioPalette.Ink,
                        FontSize = 14,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { cancel, yes }
                    }
                }
            }
        };
        ApplyLocalization(dialog);
        await dialog.ShowDialog(this);
        return confirmed;
    }

    private async Task<bool> ConfirmDangerousActionAsync(string title, string message, string confirmLabel)
    {
        var confirmed = false;
        var dialog = new Window
        {
            Title = title,
            Width = 500,
            Height = 220,
            CanResize = false,
            Background = XRatioPalette.Canvas,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        var confirm = CreateButton(confirmLabel, ButtonTone.DangerStrong, 112);
        var cancel = CreateButton("Cancel", ButtonTone.Secondary, 90);
        confirm.Click += (_, _) =>
        {
            confirmed = true;
            dialog.Close();
        };
        cancel.Click += (_, _) => dialog.Close();
        dialog.Content = new Border
        {
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(10),
            Padding = new Thickness(18),
            Child = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        Foreground = XRatioPalette.Ink,
                        FontSize = 14,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Spacing = 8,
                        Children = { cancel, confirm }
                    }
                }
            }
        };
        ApplyLocalization(dialog);
        await dialog.ShowDialog(this);
        return confirmed;
    }

    private static string AbbreviateHash(string infoHash) =>
        infoHash.Length <= 8 ? infoHash : $"{infoHash[..8]}…";

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        double value = bytes;
        var index = 0;
        while (Math.Abs(value) >= 1024 && index < suffixes.Length - 1)
        {
            value /= 1024;
            index++;
        }
        return index == 0
            ? $"{value:0} {suffixes[index]}"
            : $"{value:0.0} {suffixes[index]}";
    }

    private void AddActivity(
        string message,
        ActivityLevel? level = null,
        string? source = null,
        DateTimeOffset? timestamp = null)
    {
        message = UiText.TranslateMessage(message, _language);
        var entry = ActivityEntry.Create(
            message,
            level ?? InferActivityLevel(message),
            source ?? InferActivitySource(message),
            timestamp ?? DateTimeOffset.Now);
        _activity.Items.Add(BuildActivityItem(entry));
        if (_activity.ItemCount > 500)
            _activity.Items.RemoveAt(0);
        _activity.ScrollIntoView(_activity.ItemCount - 1);
        ApplyLocalization(_activity);
    }

    private ListBoxItem BuildActivityItem(ActivityEntry entry)
    {
        var color = entry.Level switch
        {
            ActivityLevel.Error => XRatioPalette.Danger,
            ActivityLevel.Warning => XRatioPalette.Warning,
            ActivityLevel.Success => XRatioPalette.Positive,
            _ => XRatioPalette.Muted
        };
        var openSettings = CreateButton("Open Settings", ButtonTone.Quiet, 112);
        openSettings.IsVisible = entry.Level == ActivityLevel.Error &&
                                 (entry.Detail.Contains("port", StringComparison.OrdinalIgnoreCase) ||
                                  entry.Detail.Contains("socket", StringComparison.OrdinalIgnoreCase));
        openSettings.Click += (_, _) =>
        {
            _tabs.SelectedIndex = 4;
            _port.Focus();
        };
        var details = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock
                {
                    Text = entry.Summary,
                    FontSize = 12.5,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = XRatioPalette.Ink,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                },
                new TextBlock
                {
                    Text = entry.Detail,
                    IsVisible = !string.IsNullOrWhiteSpace(entry.Detail),
                    FontSize = 11.5,
                    Foreground = XRatioPalette.Muted,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }
        };
        return new ListBoxItem
        {
            Tag = entry,
            Padding = new Thickness(10, 8),
            Content = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("72,86,*,Auto"),
                ColumnSpacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Text = entry.Timestamp.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                        FontSize = 11.5,
                        Foreground = XRatioPalette.Muted,
                        FontFeatures = XRatioPalette.TabularNumbers,
                        VerticalAlignment = VerticalAlignment.Top
                    },
                    Place(new TextBlock
                    {
                        Text = $"{entry.Level.ToString().ToUpperInvariant()} · {entry.Source}",
                        FontSize = 10.5,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = color,
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        VerticalAlignment = VerticalAlignment.Top
                    }, column: 1),
                    Place(details, column: 2),
                    Place(openSettings, column: 3)
                }
            }
        };
    }

    internal static ActivityLevel InferActivityLevel(string message) =>
        message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("could not", StringComparison.OrdinalIgnoreCase)
            ? ActivityLevel.Error
            : message.Contains("skipped", StringComparison.OrdinalIgnoreCase) ||
              message.Contains("required", StringComparison.OrdinalIgnoreCase)
                ? ActivityLevel.Warning
                : message.Contains("saved", StringComparison.OrdinalIgnoreCase) ||
                  message.Contains("enabled", StringComparison.OrdinalIgnoreCase)
                    ? ActivityLevel.Success
                    : ActivityLevel.Info;

    internal static string InferActivitySource(string message) =>
        message.Contains("simulation", StringComparison.OrdinalIgnoreCase) ? "Simulation" :
        message.StartsWith("Torrent", StringComparison.OrdinalIgnoreCase) ||
        message.StartsWith("Loaded torrent", StringComparison.OrdinalIgnoreCase) ? "Torrent" :
        message.StartsWith("HTTPS", StringComparison.OrdinalIgnoreCase) ||
        message.Contains("CA trust", StringComparison.OrdinalIgnoreCase) ? "HTTPS" :
        message.StartsWith("Configuration", StringComparison.OrdinalIgnoreCase) ? "Settings" :
        message.StartsWith("Startup", StringComparison.OrdinalIgnoreCase) ? "Startup" : "System";

    private void PopulateForm(XRatioSettings settings)
    {
        _themeMode.SelectedIndex = NormalizeThemeMode(settings.ThemeMode) switch
        {
            var mode when ThemePalette.IndexOf(mode) >= 0 => ThemePalette.IndexOf(mode),
            _ => 0
        };
        _accentColor.SelectedIndex = Math.Max(0, AccentPalette.IndexOf(NormalizeAccentColor(settings.AccentColor)));
        _trayIconStyle.SelectedIndex = Math.Max(
            0,
            Array.IndexOf(
                TrayIconStyles.ToArray(),
                NormalizeTrayIconStyle(settings.TrayIconStyle)));
        _languageMode.SelectedIndex = UiText.IndexOf(settings.Language);
        _port.Text = settings.ListenPort.ToString(CultureInfo.InvariantCulture);
        _minimumPeers.Text = settings.MinimumPeers.ToString(CultureInfo.InvariantCulture);
        _downloadRatioMin.Text = settings.UploadPerDownloadMinimum.ToString(CultureInfo.InvariantCulture);
        _downloadRatioMax.Text = settings.UploadPerDownloadMaximum.ToString(CultureInfo.InvariantCulture);
        _uploadRatioMin.Text = settings.UploadPerUploadMinimum.ToString(CultureInfo.InvariantCulture);
        _uploadRatioMax.Text = settings.UploadPerUploadMaximum.ToString(CultureInfo.InvariantCulture);
        _boost.Text = settings.BoostKiBPerSecond.ToString(CultureInfo.InvariantCulture);
        _boostChance.Text = settings.BoostChancePercent.ToString(CultureInfo.InvariantCulture);
        _onlyTrackers.IsChecked = settings.OnlyTrackerTraffic;
        _onlyLocal.IsChecked = settings.OnlyLocalConnections;
        _proxyDebugLogging.IsChecked = settings.ProxyDebugLogging;
        _noDownload.IsChecked = settings.ReportDownloadAsZero;
        _pretendSeed.IsChecked = settings.PretendToSeed;
        _autoStart.IsChecked = settings.AutoStart;
        _showTrayIcon.IsChecked = IsTrayAvailable() && settings.ShowTrayIcon;
        _startMinimized.IsEnabled = IsTrayAvailable() && _showTrayIcon.IsChecked == true;
        _startMinimized.IsChecked = _startMinimized.IsEnabled && settings.StartMinimized;
        UpdateTrayAvailabilityControls();
    }

    private XRatioSettings ReadForm() => _settings with
    {
        ThemeMode = SelectedThemeMode(),
        AccentColor = SelectedAccentColor(),
        TrayIconStyle = NormalizeTrayIconStyle(
            _trayIconStyle.SelectedIndex >= 0 && _trayIconStyle.SelectedIndex < TrayIconStyles.Count
                ? TrayIconStyles[_trayIconStyle.SelectedIndex]
                : null),
        Language = UiText.Normalize(_language),
        ListenPort = ParseInt(_port, "HTTP proxy port"),
        MinimumPeers = ParseInt(_minimumPeers, "Minimum leechers"),
        UploadPerDownloadMinimum = ParseDouble(_downloadRatioMin, "Upload/download minimum"),
        UploadPerDownloadMaximum = ParseDouble(_downloadRatioMax, "Upload/download maximum"),
        UploadPerUploadMinimum = ParseDouble(_uploadRatioMin, "Upload/upload minimum"),
        UploadPerUploadMaximum = ParseDouble(_uploadRatioMax, "Upload/upload maximum"),
        BoostKiBPerSecond = ParseDouble(_boost, "Boost"),
        BoostChancePercent = ParseInt(_boostChance, "Boost chance"),
        OnlyTrackerTraffic = _onlyTrackers.IsChecked == true,
        OnlyLocalConnections = true,
        ProxyDebugLogging = _proxyDebugLogging.IsChecked == true,
        ReportDownloadAsZero = _noDownload.IsChecked == true,
        PretendToSeed = _pretendSeed.IsChecked == true,
        AutoStart = _autoStart.IsChecked == true,
        ShowTrayIcon = IsTrayAvailable() && _showTrayIcon.IsChecked == true,
        StartMinimized = IsTrayAvailable() && _showTrayIcon.IsChecked == true &&
                         _startMinimized.IsChecked == true
    };

    private static bool IsTrayAvailable() => App.ShouldCreateTrayIcon(OperatingSystem.IsWindows());

    private void UpdateTrayAvailabilityControls()
    {
        _hide.IsVisible = IsTrayAvailable() && _settings.ShowTrayIcon;
        _startMinimized.IsEnabled = IsTrayAvailable() && _showTrayIcon.IsChecked == true;
    }

    private static int ParseInt(TextBox input, string name) =>
        int.TryParse(input.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new ArgumentException($"{name} must be an integer.");

    private static double ParseDouble(TextBox input, string name)
    {
        if (!double.TryParse(input.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
            !double.IsFinite(value))
        {
            throw new ArgumentException($"{name} must be a finite number using '.' as decimal separator.");
        }
        return value;
    }

    private static double ParseRequiredPositiveDouble(TextBox input, string name)
    {
        var value = ParseDouble(input, name);
        if (value <= 0)
            throw new ArgumentOutOfRangeException(name, $"{name} must be greater than zero.");
        return value;
    }

    private static long MiBToBytes(double value, string name) =>
        value <= 0
            ? throw new ArgumentOutOfRangeException(name, $"{name} must be greater than zero.")
            : checked((long)Math.Round(value * 1024 * 1024));

    private static Uri? ParseOptionalUri(TextBox input, string name)
    {
        if (string.IsNullOrWhiteSpace(input.Text))
            return null;
        return Uri.TryCreate(input.Text.Trim(), UriKind.Absolute, out var uri)
            ? uri
            : throw new ArgumentException($"{name} must be an absolute URI.");
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private Control BuildTabLayout(string title, string subtitle, Control surface)
    {
        return new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(30, 26, 30, 30),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                RowSpacing = 18,
                Children =
                {
                    BuildTabHeading(title, subtitle),
                    Place(surface, row: 1)
                }
            }
        };
    }

    private static Control BuildTabHeading(string title, string subtitle)
    {
        return new StackPanel
        {
            Spacing = 5,
            Children =
            {
                new TextBlock
                {
                    Text = title,
                    FontSize = 25,
                    FontWeight = FontWeight.Bold,
                    Foreground = XRatioPalette.Ink,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                },
                new TextBlock
                {
                    Text = subtitle,
                    FontSize = 12,
                    Foreground = XRatioPalette.Muted,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }
        };
    }

    private static Border BuildListSurface(Control list)
    {
        return new Border
        {
            Background = XRatioPalette.SurfaceRaised,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(14),
            Child = list
        };
    }

    private static Border BuildPanelSection(string title, string subtitle, Control body)
    {
        return new Border
        {
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(20),
            Child = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 3,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = title,
                                FontSize = 13,
                                FontWeight = FontWeight.Bold,
                                Foreground = XRatioPalette.Ink
                            },
                            new TextBlock
                            {
                                Text = subtitle,
                                FontSize = 11.5,
                                Foreground = XRatioPalette.Muted,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap
                            }
                        }
                    },
                    body
                }
            }
        };
    }

    private static Border BuildSettingsSection(
        string title,
        string subtitle,
        Control body,
        double bottomPadding = 22)
    {
        return new Border
        {
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(0, 18, 0, bottomPadding),
            Margin = new Thickness(0),
            Child = new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 3,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = title,
                                FontSize = 13,
                                FontWeight = FontWeight.Bold,
                                Foreground = XRatioPalette.Ink
                            },
                            new TextBlock
                            {
                                Text = subtitle,
                                FontSize = 11.5,
                                Foreground = XRatioPalette.Muted,
                                TextWrapping = Avalonia.Media.TextWrapping.Wrap
                            }
                        }
                    },
                    body
                }
            }
        };
    }

    private static StackPanel BuildSettingsBody(params Control[] controls)
    {
        var body = new StackPanel { Spacing = 12 };
        foreach (var control in controls)
            body.Children.Add(control);
        return body;
    }

    private static Grid BuildFieldGrid(params (string Label, Control Editor)[] fields)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,*"),
            RowSpacing = 10,
            ColumnSpacing = 18
        };
        for (var index = 0; index < fields.Length; index++)
            grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

        for (var index = 0; index < fields.Length; index++)
            AddField(grid, index, fields[index].Label, fields[index].Editor);
        return grid;
    }

    private static Border BuildCompactGroup(string title, Control body) =>
        new()
        {
            Background = XRatioPalette.SurfaceRaised,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 9),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                RowSpacing = 9,
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontSize = 11,
                        FontWeight = FontWeight.Bold,
                        Foreground = XRatioPalette.Ink
                    },
                    Place(body, row: 1)
                }
            }
        };

    private static Grid BuildCompactFieldRow(string label, Control editor) =>
        new()
        {
            ColumnDefinitions = new ColumnDefinitions("90,*"),
            ColumnSpacing = 8,
            Children =
            {
                CreateCompactLabel(label),
                Place(editor, column: 1)
            }
        };

    private Grid BuildTorrentIdentityRow() =>
        new()
        {
            ColumnDefinitions = new ColumnDefinitions("90,*,36,120"),
            ColumnSpacing = 8,
            Children =
            {
                CreateCompactLabel("Hash"),
                Place(_simulationInfoHash, column: 1),
                Place(CreateCompactLabel("Size"), column: 2),
                Place(_simulationInfoSize, column: 3)
            }
        };

    private Control BuildSimulationOptionsGrid()
    {
        _simulationAnnounceInterval.Width = 88;
        _simulationAnnounceInterval.MinWidth = 88;
        _simulationCompleted.Width = 72;
        _simulationCompleted.MinWidth = 72;
        _simulationStopValue.Width = 100;
        _simulationStopValue.MinWidth = 100;
        var primary = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,88,24,Auto,220,24,Auto,72,*"),
            ColumnSpacing = 8,
            Children =
            {
                CreateCompactLabel("Update interval (s)"),
                Place(_simulationAnnounceInterval, column: 1),
                Place(CreateCompactLabel("Client"), column: 3),
                Place(_simulationClient, column: 4),
                Place(CreateCompactLabel("Finished (%)"), column: 6),
                Place(_simulationCompleted, column: 7)
            }
        };
        var stop = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,180,100,*"),
            ColumnSpacing = 8,
            Children =
            {
                CreateCompactLabel("Stop"),
                Place(_simulationStopMode, column: 1),
                Place(_simulationStopValue, column: 2)
            }
        };
        return new StackPanel { Spacing = 6, Children = { primary, stop } };
    }

    private static TextBlock CreateCompactLabel(string text) =>
        new()
        {
            Text = text,
            FontSize = 12,
            Foreground = XRatioPalette.Ink,
            VerticalAlignment = VerticalAlignment.Center
        };

    private static Grid BuildSimulationSpeedRow(
        string label,
        TextBox baseline,
        CheckBox randomEnabled,
        TextBox minimum,
        TextBox maximum)
    {
        baseline.Width = 96;
        baseline.MinWidth = 96;
        return new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("150,96,144,Auto,64,Auto,64,*"),
            ColumnSpacing = 8,
            Children =
            {
                new TextBlock
                {
                    Text = label,
                    Foreground = XRatioPalette.Ink,
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Place(baseline, column: 1),
                Place(randomEnabled, column: 2),
                Place(new TextBlock
                {
                    Text = "Min",
                    Foreground = XRatioPalette.Muted,
                    VerticalAlignment = VerticalAlignment.Center
                }, column: 3),
                Place(minimum, column: 4),
                Place(new TextBlock
                {
                    Text = "Max",
                    Foreground = XRatioPalette.Muted,
                    VerticalAlignment = VerticalAlignment.Center
                }, column: 5),
                Place(maximum, column: 6)
            }
        };
    }

    private static ScrollViewer BuildVerticalSimulationScroller(Control content) =>
        new()
        {
            Content = content,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
        };

    private static void ConfigureCompactSimulationControls(params Control[] controls)
    {
        foreach (var control in controls)
        {
            control.MinHeight = 32;
            if (control is TextBox textBox)
                textBox.Padding = new Thickness(8, 4);
        }
    }

    private static StackPanel BuildToggleGroup(params CheckBox[] toggles)
    {
        var group = new StackPanel { Spacing = 8 };
        foreach (var toggle in toggles)
            group.Children.Add(toggle);
        return group;
    }

    private static void ConfigureList(ListBox list)
    {
        list.Background = Brushes.Transparent;
        list.BorderThickness = new Thickness(0);
        list.Foreground = XRatioPalette.Ink;
        list.FontSize = 12;
        list.FontFeatures = XRatioPalette.TabularNumbers;
        list.Padding = new Thickness(0);
    }

    private static Border BuildRuntimeRow(
        string label,
        string caption,
        TextBlock value,
        bool divider)
    {
        value.Text = "—";
        value.FontSize = 19;
        value.FontWeight = FontWeight.Bold;
        value.Foreground = XRatioPalette.Ink;
        value.FontFeatures = XRatioPalette.TabularNumbers;
        value.VerticalAlignment = VerticalAlignment.Center;
        return new Border
        {
            BorderBrush = XRatioPalette.Border,
            BorderThickness = divider ? new Thickness(0, 0, 0, 1) : new Thickness(0),
            Padding = new Thickness(18, 15),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                ColumnSpacing = 24,
                Children =
                {
                    new StackPanel
                    {
                        Spacing = 3,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = label,
                                Foreground = XRatioPalette.Ink,
                                FontSize = 11,
                                FontWeight = FontWeight.SemiBold
                            },
                            new TextBlock
                            {
                                Text = caption,
                                Foreground = XRatioPalette.Subtle,
                                FontSize = 10.5
                            }
                        }
                    },
                    Place(value, column: 1)
                }
            }
        };
    }

    private void UpdateOverviewMetrics()
    {
        _overviewProxyKpi.Text = L(_proxy?.IsRunning == true ? _paused ? "Paused" : "Active" : "Stopped");
        _pause.IsEnabled = _proxy?.IsRunning == true;
        _statusIndicator.Background = _startupFailureBanner.IsVisible
            ? XRatioPalette.Danger
            : _proxy?.IsRunning == true
                ? _paused ? XRatioPalette.Warning : XRatioPalette.Positive
                : XRatioPalette.Subtle;
        var torrents = _transformer.GetSnapshots();
        _overviewTorrentKpi.Text = torrents.Count.ToString(CultureInfo.InvariantCulture);
        var active = _simulationEntries.Values.Count(entry => entry.Session.State == SimulationState.Running);
        _overviewSimulationKpi.Text = $"{active} / {_simulationEntries.Count}";
        _overviewReportedKpi.Text = FormatBytes(torrents.Sum(torrent => torrent.ReportedUploadedTotal));
    }

    private static void ConfigureTextBox(TextBox input, string watermark)
    {
        input.Background = XRatioPalette.SurfaceRaised;
        input.BorderBrush = XRatioPalette.Border;
        input.BorderThickness = new Thickness(1);
        input.CornerRadius = new CornerRadius(4);
        input.Foreground = XRatioPalette.Ink;
        input.FontSize = 13;
        input.FontFeatures = XRatioPalette.TabularNumbers;
        input.MinHeight = 36;
        input.MinWidth = 180;
        input.Width = 220;
        input.Padding = new Thickness(11, 7);
        input.PlaceholderText = watermark;
        input.HorizontalAlignment = HorizontalAlignment.Left;
    }

    private static void ConfigureSimulationSpeedInput(TextBox input, string watermark)
    {
        ConfigureTextBox(input, watermark);
        input.MinWidth = 64;
        input.Width = 64;
        input.Padding = new Thickness(8, 6);
    }

    private static void ConfigureComboBox(ComboBox comboBox, double width)
    {
        comboBox.Background = XRatioPalette.SurfaceRaised;
        comboBox.BorderBrush = XRatioPalette.Border;
        comboBox.BorderThickness = new Thickness(1);
        comboBox.CornerRadius = new CornerRadius(4);
        comboBox.Foreground = XRatioPalette.Ink;
        comboBox.MinHeight = 36;
        comboBox.Width = width;
        comboBox.HorizontalAlignment = HorizontalAlignment.Left;
    }

    private static Control BuildLanguageOption(string? value)
    {
        var index = UiText.LanguageIndex(value);
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                BuildFlagIcon(UiText.FlagCodeAt(index)),
                new TextBlock
                {
                    Text = UiText.DisplayNameAt(index),
                    Foreground = XRatioPalette.Ink,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };
    }

    private static Control BuildFlagIcon(string code)
    {
        const double width = 22;
        const double height = 14;
        var canvas = new Canvas
        {
            Width = width,
            Height = height,
            ClipToBounds = true,
            Margin = new Thickness(0, 0, 1, 0),
            IsHitTestVisible = false
        };

        void Add(Control control, double left = 0, double top = 0)
        {
            Canvas.SetLeft(control, left);
            Canvas.SetTop(control, top);
            canvas.Children.Add(control);
        }

        Border Block(string color, double blockWidth = width, double blockHeight = height) =>
            new()
            {
                Background = new SolidColorBrush(Color.Parse(color)),
                Width = blockWidth,
                Height = blockHeight
            };

        void Band(string color, double top, double bandHeight) => Add(Block(color, width, bandHeight), 0, top);

        switch (code)
        {
            case "US":
                Add(Block("#FFFFFF"));
                for (var stripe = 0; stripe < 7; stripe++)
                    Band(stripe % 2 == 0 ? "#C83B45" : "#FFFFFF", stripe * 2, 2);
                Add(Block("#315AA8", 10, 8));
                break;
            case "FR":
                Add(Block("#315AA8", 7, height));
                Add(Block("#FFFFFF", 8, height), 7);
                Add(Block("#C83B45", 7, height), 15);
                break;
            case "ES":
                Band("#C83B45", 0, 3);
                Band("#F1C453", 3, 8);
                Band("#C83B45", 11, 3);
                break;
            case "DE":
                Band("#20242B", 0, 4.67);
                Band("#C84A4A", 4.67, 4.67);
                Band("#E4B84C", 9.34, 4.66);
                break;
            case "IT":
                Add(Block("#4B9B6B", 7, height));
                Add(Block("#FFFFFF", 8, height), 7);
                Add(Block("#C83B45", 7, height), 15);
                break;
            case "PT":
                Add(Block("#3D8D62", 8, height));
                Add(Block("#C83B45", 14, height), 8);
                Add(new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#F1C453")),
                    Width = 5,
                    Height = 5,
                    CornerRadius = new CornerRadius(3)
                }, 6, 4.5);
                break;
            case "JP":
                Add(Block("#FFFFFF"));
                Add(new Border
                {
                    Background = new SolidColorBrush(Color.Parse("#C83B45")),
                    Width = 8,
                    Height = 8,
                    CornerRadius = new CornerRadius(4)
                }, 7, 3);
                break;
            case "CN":
                Add(Block("#C83B45"));
                Add(new TextBlock
                {
                    Text = "★",
                    Foreground = new SolidColorBrush(Color.Parse("#F1C453")),
                    FontSize = 8,
                    FontWeight = FontWeight.Bold,
                    Width = 9,
                    Height = 9,
                    TextAlignment = TextAlignment.Center
                }, 2, 1);
                break;
            case "SA":
                Add(Block("#2F8A68"));
                Add(Block("#FFFFFF", 12, 1), 5, 7);
                break;
            case "RU":
                Band("#FFFFFF", 0, 4.67);
                Band("#3F68A8", 4.67, 4.67);
                Band("#C83B45", 9.34, 4.66);
                break;
            default:
                Add(Block("#8994A6"));
                break;
        }

        Add(new Border
        {
            Width = width,
            Height = height,
            BorderBrush = new SolidColorBrush(Color.Parse("#728096")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(2),
            Background = Brushes.Transparent
        });
        return canvas;
    }

    private static void ConfigureCheckBox(CheckBox checkBox)
    {
        checkBox.Foreground = XRatioPalette.Ink;
        checkBox.FontSize = 13;
        checkBox.HorizontalAlignment = HorizontalAlignment.Left;
        checkBox.MinHeight = 36;
        checkBox.Margin = new Thickness(0);
    }

    private static void ConfigureContextMenuItem(MenuItem item)
    {
        item.Foreground = XRatioPalette.Ink;
        item.FontSize = 12;
        item.MinHeight = 36;
        item.Padding = new Thickness(10, 8);
    }

    private static void ConfigureGuideButton(Button button)
    {
        button.Classes.Add("guide-action");
        button.Background = Brushes.Transparent;
        button.BorderBrush = XRatioPalette.Border;
        button.BorderThickness = new Thickness(1);
        button.CornerRadius = new CornerRadius(17);
        button.Foreground = XRatioPalette.Muted;
        button.FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets");
        button.FontSize = 15;
        button.FontWeight = FontWeight.SemiBold;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        button.VerticalContentAlignment = VerticalAlignment.Center;
        button.Width = 36;
        button.MinWidth = 36;
        button.Height = 36;
        button.MinHeight = 36;
        button.Padding = new Thickness(0);
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("guide-action").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, Brushes.Transparent),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.ForegroundProperty, XRatioPalette.Accent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(17))
            }
        });
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("guide-action").Class(":pressed"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.AccentSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.ForegroundProperty, XRatioPalette.Accent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(17))
            }
        });
    }

    private static Button CreateButton(string content, ButtonTone tone, double minWidth)
    {
        var button = new Button { Content = content };
        StyleButton(button, tone, minWidth);
        return button;
    }

    private static void StyleButton(Button button, ButtonTone tone, double minWidth)
    {
        button.Background = tone switch
        {
            ButtonTone.Primary => XRatioPalette.Accent,
            ButtonTone.DangerStrong => XRatioPalette.Danger,
            ButtonTone.Danger => XRatioPalette.DangerSoft,
            ButtonTone.Secondary => XRatioPalette.SurfaceRaised,
            _ => Brushes.Transparent
        };
        button.BorderBrush = tone switch
        {
            ButtonTone.Primary => XRatioPalette.Accent,
            ButtonTone.DangerStrong => XRatioPalette.Danger,
            ButtonTone.Danger => XRatioPalette.DangerBorder,
            ButtonTone.Secondary => XRatioPalette.Border,
            _ => Brushes.Transparent
        };
        button.BorderThickness = tone is ButtonTone.Primary or ButtonTone.DangerStrong
            ? new Thickness(0)
            : new Thickness(1);
        button.CornerRadius = new CornerRadius(5);
        button.Foreground = tone switch
        {
            ButtonTone.Primary => XRatioPalette.OnAccent,
            ButtonTone.DangerStrong => XRatioPalette.OnAccent,
            ButtonTone.Danger => XRatioPalette.Danger,
            _ => XRatioPalette.Ink
        };
        button.FontSize = 11.5;
        button.FontWeight = FontWeight.SemiBold;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        button.VerticalContentAlignment = VerticalAlignment.Center;
        button.MinHeight = 36;
        button.MinWidth = minWidth;
        button.Padding = new Thickness(13, 7);
    }

    private static void AddField(Grid grid, int row, string label, Control editor)
    {
        grid.Children.Add(Place(new TextBlock
        {
            Text = label,
            FontSize = 13,
            Foreground = XRatioPalette.Ink,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        }, row));
        grid.Children.Add(Place(editor, row, 1));
    }

    private enum ButtonTone
    {
        Primary,
        Danger,
        DangerStrong,
        Secondary,
        Quiet
    }

    private static class XRatioPalette
    {
        public static readonly SolidColorBrush Canvas = Brush("#F4F6FA");
        public static readonly SolidColorBrush Topbar = Brush("#FFFFFF");
        public static readonly SolidColorBrush Sidebar = Brush("#E9EEF7");
        public static readonly SolidColorBrush NavCanvas = Brush("#F4F6FA");
        public static readonly SolidColorBrush NavPanel = Brush("#FFFFFF");
        public static readonly SolidColorBrush NavBorder = Brush("#D8E1EC");
        public static readonly SolidColorBrush NavSelected = Brush("#E7EDF3");
        public static readonly SolidColorBrush Surface = Brush("#FFFFFF");
        public static readonly SolidColorBrush SurfaceRaised = Brush("#F8FAFD");
        public static readonly SolidColorBrush MetricSurface = Brush("#F3F6FA");
        public static readonly SolidColorBrush Ink = Brush("#122034");
        public static readonly SolidColorBrush Muted = Brush("#5C6B7E");
        public static readonly SolidColorBrush Subtle = Brush("#74849A");
        public static readonly SolidColorBrush Border = Brush("#D8E1EC");
        public static readonly SolidColorBrush Accent = Brush(AccentPalette.Primary(AccentPalette.Blue, dark: false));
        public static readonly SolidColorBrush AccentSoft = Brush(AccentPalette.Soft(AccentPalette.Blue, dark: false));
        public static readonly SolidColorBrush Positive = Brush("#0B8F68");
        public static readonly SolidColorBrush PositiveSoft = Brush("#DCF7EB");
        public static readonly SolidColorBrush Warning = Brush("#A66A00");
        public static readonly SolidColorBrush Danger = Brush("#B42333");
        public static readonly SolidColorBrush DangerSoft = Brush("#FDE9EC");
        public static readonly SolidColorBrush DangerBorder = Brush("#E9A9B2");
        public static readonly SolidColorBrush NeutralSoft = Brush("#E7EDF3");
        public static readonly SolidColorBrush OnAccent = Brush("#FFFFFF");
        public static readonly FontFeatureCollection TabularNumbers =
            FontFeatureCollection.Parse("tnum");

        internal static void Apply(string themeMode, string? accentColor = null)
        {
            var normalized = ThemePalette.Normalize(themeMode);
            var dark = normalized == ThemePalette.Dark;
            var dim = normalized == ThemePalette.Dim;
            var softDark = normalized == ThemePalette.SoftDark;
            Set(Canvas, dark ? "#0B1120" : softDark ? "#171A20" : dim ? "#253247" : "#F4F6FA");
            Set(Topbar, dark ? "#101827" : softDark ? "#1D2129" : dim ? "#2D3A50" : "#FFFFFF");
            Set(Sidebar, dark ? "#0D1625" : softDark ? "#20252E" : dim ? "#202C40" : "#E9EEF7");
            // The navigation rail is part of the same open canvas as the work
            // area. Keep the inset rounded panel as the only navigation
            // surface so dark themes do not show a hard vertical color seam.
            Set(NavCanvas, dark ? "#0B1120" : softDark ? "#171A20" : dim ? "#253247" : "#F4F6FA");
            Set(NavPanel, dark || dim ? "#171717" : softDark ? "#1B1E24" : "#FFFFFF");
            Set(NavBorder, dark || dim ? "#2A2A2A" : softDark ? "#343A44" : "#D8E1EC");
            Set(NavSelected, dark || dim ? "#2B2B2B" : softDark ? "#2A3038" : "#E7EDF3");
            Set(Surface, dark ? "#131F30" : softDark ? "#242A33" : dim ? "#31405A" : "#FFFFFF");
            Set(SurfaceRaised, dark ? "#172638" : softDark ? "#2A303A" : dim ? "#384963" : "#F8FAFD");
            Set(MetricSurface, dark ? "#0F1A29" : softDark ? "#1F252E" : dim ? "#2B3950" : "#F3F6FA");
            Set(Ink, dark ? "#F4F8FD" : softDark ? "#E9EDF3" : dim ? "#F2F6FB" : "#122034");
            Set(Muted, dark ? "#9FB2C8" : softDark ? "#B6C0CE" : dim ? "#C0CBD9" : "#5C6B7E");
            Set(Subtle, dark ? "#7086A0" : softDark ? "#929EAE" : dim ? "#A7B5C8" : "#74849A");
            Set(Border, dark ? "#25364A" : softDark ? "#3A424F" : dim ? "#52637A" : "#D8E1EC");
            Set(Accent, AccentPalette.Primary(accentColor, dark, dim, softDark));
            Set(AccentSoft, AccentPalette.Soft(accentColor, dark, dim, softDark));
            Set(Positive, dark ? "#61E6B0" : softDark ? "#70D6A5" : dim ? "#66D9B0" : "#0B8F68");
            Set(PositiveSoft, dark ? "#153A34" : softDark ? "#214238" : dim ? "#244C44" : "#DCF7EB");
            Set(Warning, dark ? "#F3C56C" : softDark ? "#E6BF74" : dim ? "#F4C66E" : "#A66A00");
            Set(Danger, dark ? "#FF7A8A" : softDark ? "#F099A8" : dim ? "#FF8997" : "#B42333");
            Set(DangerSoft, dark ? "#40222B" : softDark ? "#4A2D35" : dim ? "#59323D" : "#FDE9EC");
            Set(DangerBorder, dark ? "#7D3848" : softDark ? "#955463" : dim ? "#A85E6C" : "#E9A9B2");
            Set(NeutralSoft, dark ? "#243449" : softDark ? "#303740" : dim ? "#3A4A61" : "#E7EDF3");
            Set(OnAccent, dark ? "#07151A" : softDark ? "#10161D" : dim ? "#081425" : "#FFFFFF");
        }

        private static SolidColorBrush Brush(string hex) =>
            new(Color.Parse(hex));

        private static void Set(SolidColorBrush brush, string hex) =>
            brush.Color = Color.Parse(hex);
    }

    private Control BuildNavigation()
    {
        var panel = BuildNavigationPanel(
            BuildNavigationGroup(
                "Monitoring",
                divider: false,
                (0, "\uE80F", "Overview"),
                (1, "\uE895", "Interception")),
            BuildNavigationGroup(
                "Control",
                divider: true,
                (2, "\uE768", "Simulation"),
                (3, "\uE81C", "Activity")),
            BuildNavigationGroup(
                "System",
                divider: true,
                (4, "\uE713", "Settings"),
                (5, "\uE83D", "Platform")),
            BuildNavigationGroup(
                "Support",
                divider: true,
                (null, "\uE897", "Guide")));

        return new Border
        {
            Width = 250,
            Background = XRatioPalette.NavCanvas,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(14, 8, 14, 8),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = panel
        };
    }

    private Border BuildNavigationPanel(params Control[] groups)
    {
        var upperGroups = new StackPanel { Spacing = 0 };
        foreach (var group in groups.Take(Math.Max(0, groups.Length - 1)))
            upperGroups.Children.Add(group);

        var content = new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Children =
            {
                upperGroups,
                Place(groups[^1], row: 1)
            }
        };

        return new Border
        {
            Background = XRatioPalette.NavPanel,
            BorderBrush = XRatioPalette.NavBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(8, 10, 8, 12),
            MinHeight = 390,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Child = content
        };
    }

    private StackPanel BuildNavigationGroup(
        string section,
        bool divider,
        params (int? Index, string Glyph, string Label)[] items)
    {
        var group = new StackPanel { Spacing = 0 };
        group.Children.Add(BuildNavigationSection(section, divider));
        foreach (var (index, glyph, label) in items)
            group.Children.Add(BuildNavigationButton(index, glyph, label));
        return group;
    }

    private static StackPanel BuildNavigationSection(string section, bool divider)
    {
        var header = new StackPanel
        {
            Spacing = 5,
            Margin = new Thickness(4, divider ? 16 : 5, 4, 4)
        };
        if (divider)
        {
            header.Children.Add(new Border
            {
                Tag = "NavDivider",
                Height = 1,
                Background = XRatioPalette.NavBorder,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 3)
            });
        }

        header.Children.Add(new TextBlock
        {
            Tag = "NavSection",
            Text = section,
            FontSize = 10,
            FontWeight = FontWeight.SemiBold,
            Foreground = XRatioPalette.Subtle,
            LetterSpacing = 0.2,
            VerticalAlignment = VerticalAlignment.Center
        });
        return header;
    }

    private Button BuildNavigationButton(int? index, string glyph, string label)
    {
        var row = BuildNavRow(glyph, label);
        var button = new Button
        {
            Tag = index is int tabIndex ? tabIndex : "GuideAction",
            Content = row,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(0),
            Margin = new Thickness(0, 2),
            MinHeight = 44,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        button.Classes.Add("nav-button");
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("nav-button").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, Brushes.Transparent),
                new Setter(Button.BorderBrushProperty, XRatioPalette.NavBorder)
            }
        });
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("nav-button").Class(":focus-visible"))
        {
            Setters =
            {
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1))
            }
        });
        button.Click += async (_, _) =>
        {
            if (index is int tabIndex)
                _tabs.SelectedIndex = tabIndex;
            else
                await ShowGuideAsync(_tabs);
        };
        _navigationItems.Add((button, row));
        return button;
    }

    private static Border BuildNavRow(string glyph, string label) =>
        new()
        {
            Tag = "NavRow",
            Padding = new Thickness(12, 8, 12, 8),
            MinHeight = 44,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            CornerRadius = new CornerRadius(10),
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Tag = "NavIcon",
                        Text = glyph,
                        FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                        FontSize = 14,
                        Width = 16,
                        ClipToBounds = true,
                        TextAlignment = TextAlignment.Center,
                        Foreground = XRatioPalette.Muted,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    new TextBlock
                    {
                        Tag = "NavLabel",
                        Text = label,
                        FontSize = 12.5,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = XRatioPalette.Ink,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };

    private static Control BuildNavHeader(
        string glyph,
        string label,
        string? section = null,
        bool divider = false)
    {
        var row = BuildNavRow(glyph, label);

        if (string.IsNullOrWhiteSpace(section))
            return row;

        var sectionHeader = new StackPanel
        {
            Spacing = 5,
            Margin = new Thickness(4, divider ? 12 : 5, 4, 4)
        };
        if (divider)
        {
            sectionHeader.Children.Add(new Border
            {
                Tag = "NavDivider",
                Height = 1,
                Background = XRatioPalette.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 3)
            });
        }

        sectionHeader.Children.Add(new TextBlock
        {
            Tag = "NavSection",
            Text = section,
            FontSize = 10,
            FontWeight = FontWeight.SemiBold,
            Foreground = XRatioPalette.Subtle,
            LetterSpacing = 0.2,
            VerticalAlignment = VerticalAlignment.Center
        });

        return new StackPanel
        {
            Spacing = 0,
            Children = { sectionHeader, row }
        };
    }

    private static TabItem CreateTabItem(
        string glyph,
        string header,
        Control content,
        string? section = null,
        bool divider = false) =>
        new()
        {
            Header = BuildNavHeader(glyph, header, section, divider),
            Tag = header,
            Content = content,
            Template = new FuncControlTemplate<TabItem>((tab, _) => new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Child = new ContentPresenter
                {
                    Name = "PART_HeaderPresenter",
                    Content = tab.Header,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch
                }
            }),
            MinWidth = 184,
            MinHeight = 44,
            Margin = new Thickness(8, 1, 8, 1),
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            HorizontalContentAlignment = HorizontalAlignment.Stretch
        };

    private static T Place<T>(T control, int row = 0, int column = 0) where T : Control
    {
        Grid.SetRow(control, row);
        Grid.SetColumn(control, column);
        return control;
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

    private sealed record TorrentRow(TorrentSnapshot Snapshot, string Status)
    {
        public override string ToString()
        {
            var lastAnnounce = Snapshot.LastAnnounce?.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                               ?? "—";
            return $"{Snapshot.InfoHash} | {MaskTrackerUrl(Snapshot.Tracker)} | " +
                   $"{Snapshot.CompletePeers}/{Snapshot.IncompletePeers} | {Status} | " +
                   $"{FormatBytes(Snapshot.ActualDownloaded)}/{FormatBytes(Snapshot.ActualUploaded)}/{FormatBytes(Snapshot.ActualLeft)} | " +
                   $"{FormatBytes(Snapshot.ReportedDownloaded)}/{FormatBytes(Snapshot.ReportedUploaded)}/{FormatBytes(Snapshot.ReportedLeft)} | " +
                   lastAnnounce;
        }
    }

    private sealed record SimulationEntry(SimulationSession Session, SimulationOptions Options);

    internal enum ActivityLevel
    {
        Info,
        Success,
        Warning,
        Error
    }

    private sealed record ActivityEntry(
        DateTimeOffset Timestamp,
        ActivityLevel Level,
        string Source,
        string Summary,
        string Detail)
    {
        public static ActivityEntry Create(
            string message,
            ActivityLevel level,
            string source,
            DateTimeOffset timestamp)
        {
            var separator = message.IndexOf(':');
            return separator is > 0 and < 48
                ? new ActivityEntry(
                    timestamp,
                    level,
                    source,
                    message[..separator].Trim(),
                    message[(separator + 1)..].Trim())
                : new ActivityEntry(timestamp, level, source, message.Trim(), string.Empty);
        }
    }

    private sealed record GuidePage(
        string Title,
        string Intro,
        IReadOnlyList<GuideSection> Sections);

    private sealed record GuideSection(
        string Title,
        string Description,
        IReadOnlyList<string> Steps,
        string? ImageAsset = null);

    private sealed record SimulationRow(SimulationSnapshot Snapshot)
    {
        public Guid Id => Snapshot.Id;

        public override string ToString()
        {
            var ratio = double.IsPositiveInfinity(Snapshot.Ratio)
                ? "∞"
                : Snapshot.Ratio.ToString("0.000", CultureInfo.InvariantCulture);
            var next = Snapshot.NextAnnounce?.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture) ?? "—";
            return $"{Snapshot.State}: {Snapshot.Name}. Ratio {ratio}. " +
                   $"Uploaded {FormatBytes(Snapshot.Uploaded)} at {FormatBytes(Snapshot.UploadRate)}/s. " +
                   $"Downloaded {FormatBytes(Snapshot.Downloaded)} at {FormatBytes(Snapshot.DownloadRate)}/s. " +
                   $"Peers: {Snapshot.Seeders} seeders, {Snapshot.Leechers} leechers. Next announce: {next}.";
        }
    }
}
