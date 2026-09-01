using System.Globalization;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
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
    private const string BugReportUrl = "https://github.com/Mac-Cipher/XRatio/issues/new";
    private const int SimulationTimerStopMode = 1;
    private const string SimulationTimerMinutes = "Minutes";
    private const string SimulationTimerHours = "Hours";
    private const double SimulationTimerUnitSelectorWidth = 108;
    private const double CloseButtonSize = 36;
    private const double CloseGlyphSize = 16;
    private const double SimulationSessionsHeaderHeight = 42;
    private const double SimulationSessionsSurfaceHeight = 176;
    private const double UpdateIndicatorCollapsedWidth = 36;
    private const double UpdateIndicatorMinExpandedWidth = 88;
    private const double UpdateIndicatorMaxExpandedWidth = 128;
    private const double UpdateIndicatorTransitionMilliseconds = 170;
    private const int ProxyActivityBatchSize = 128;
    private static readonly string[] ByteSuffixes = ["B", "KB", "MB", "GB", "TB"];

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
    private readonly TorrentNameCatalog _torrentNames = new();
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
    private readonly TextBlock _autostartCapability = new();
    private readonly TextBlock _certificateCapability = new();
    private readonly ScrollViewer _settingsScroller = new();
    private readonly ScrollViewer _platformScroller = new();
    private readonly ScrollViewer _overviewScroller = new();
    private readonly Button _trustCertificate = new();
    private readonly Button _removeCertificate = new();
    private readonly Button _settingsResetAction = new();
    private readonly Button _settingsSaveAction = new();
    private readonly TextBlock _settingsSaveStatus = new();
    private readonly Button _checkUpdates = new();
    private readonly Button _downloadUpdate = new();
    private readonly Button _updateIndicator = new();
    private readonly Button _bugReportButton = new();
    private readonly PathIcon _updateIndicatorIcon = new();
    private readonly TextBlock _updateIndicatorLabel = new();
    private bool _updateIndicatorPointerOver;
    private bool _updateIndicatorFocused;
    private readonly CheckBox _checkUpdatesOnStartup = new();
    private readonly TextBlock _updateStatus = new();
    private readonly Border _onboardingOverlay = new();
    private readonly TextBlock _onboardingTitle = new();
    private readonly TextBlock _onboardingIntro = new();
    private readonly TextBlock _onboardingStepCounter = new();
    private readonly TextBlock _onboardingStepNumber = new();
    private readonly TextBlock _onboardingStepStatusIcon = new();
    private readonly TextBlock _onboardingStepStatus = new();
    private readonly TextBlock _onboardingStepTitle = new();
    private readonly TextBlock _onboardingStepDescription = new();
    private readonly TextBlock _onboardingStepDetail = new();
    private readonly StackPanel _onboardingDots = new();
    private readonly Button _onboardingAction = new();
    private readonly Button _onboardingMarkDone = new();
    private readonly Button _onboardingPrevious = new();
    private readonly Button _onboardingNext = new();
    private readonly Button _onboardingClose = new();
    private readonly Button _restoreOnboarding = new();
    private readonly TextBlock _onboardingSettingsStatus = new();
    private readonly TextBlock _onboardingChecklistProgress = new();
    private readonly StackPanel _onboardingChecklistContainer = new();
    private readonly StackPanel _onboardingNavigationGroup = new();
    private readonly Button _onboardingSidebarClose = new();
    private readonly TextBlock _onboardingSidebarCounter = new();
    private readonly TextBlock _onboardingSidebarStatusIcon = new();
    private readonly TextBlock _onboardingSidebarTitle = new();
    private readonly TextBlock _onboardingSidebarDescription = new();
    private readonly TextBlock _onboardingSidebarDetail = new();
    private readonly StackPanel _onboardingSidebarDots = new();
    private readonly Button _onboardingSidebarAction = new();
    private readonly Button _onboardingSidebarDone = new();
    private readonly Button _onboardingSidebarPrevious = new();
    private readonly Button _onboardingSidebarNext = new();
    private readonly List<OnboardingSidebarRowView> _onboardingSidebarRows = [];
    private readonly ComboBox _themeMode = new();
    private readonly ComboBox _accentColor = new();
    private readonly ComboBox _trayIconStyle = new();
    private readonly ComboBox _languageMode = new();
    private readonly TextBlock _overviewProxyKpi = new();
    private readonly TextBlock _overviewTorrentKpi = new();
    private readonly TextBlock _overviewSimulationKpi = new();
    private readonly TextBlock _overviewReportedKpi = new();
    private readonly TextBlock _overviewOnboardingCounter = new();
    private readonly TextBlock _overviewOnboardingProgress = new();
    private readonly TextBlock _overviewOnboardingStatusIcon = new();
    private readonly TextBlock _overviewOnboardingTitle = new();
    private readonly TextBlock _overviewOnboardingDescription = new();
    private readonly TextBlock _overviewOnboardingDetail = new();
    private readonly StackPanel _overviewOnboardingDots = new();
    private readonly Button _overviewOnboardingAction = new();
    private readonly Button _overviewOnboardingDone = new();
    private readonly Button _overviewOnboardingPrevious = new();
    private readonly Button _overviewOnboardingNext = new();
    private readonly Button _overviewOnboardingClose = new();
    private readonly Border _overviewOnboardingCard = new();
    private readonly Border _interceptionOnboardingCoachmark = new();
    private readonly TextBlock _interceptionCoachmarkTitle = new();
    private readonly TextBlock _interceptionCoachmarkSteps = new();
    private readonly TextBlock _interceptionCoachmarkTroubleshooting = new();
    private readonly Button _interceptionCoachmarkClose = new();
    private readonly Button _interceptionCoachmarkDone = new();
    private readonly Border _simulationOnboardingCoachmark = new();
    private readonly TextBlock _simulationCoachmarkTitle = new();
    private readonly TextBlock _simulationCoachmarkSteps = new();
    private readonly TextBlock _simulationCoachmarkTroubleshooting = new();
    private readonly Button _simulationCoachmarkClose = new();
    private readonly Button _simulationCoachmarkDone = new();
    private readonly Border _overviewTorrentClientScreenshot = new();
    private readonly TextBlock _overviewTorrentClientScreenshotTitle = new();
    private readonly Border _overviewOtherTorrentClientsHint = new();
    private readonly TextBlock _overviewOtherTorrentClientsTitle = new();
    private readonly TextBlock _overviewOtherTorrentClientsDescription = new();
    private readonly TextBox _torrentPath = new();
    private readonly CheckBox _simulationRevealPrivateValues = new();
    private readonly TextBox _simulationAccountName = new();
    private readonly ComboBox _simulationTracker = new();
    private readonly ComboBox _simulationClient = new();
    private readonly ComboBox _simulationStopMode = new();
    private readonly Grid _simulationStopValueEditor = new();
    private readonly ToggleButton _simulationTimerMinutes = new();
    private readonly ToggleButton _simulationTimerHours = new();
    private readonly Border _simulationTimerUnitSelector = new();
    private readonly TextBlock _simulationStopHint = new();
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
    private readonly ConcurrentQueue<ProxyEvent> _pendingProxyActivities = new();
    private readonly object _simulationRefreshGate = new();
    private readonly SemaphoreSlim _updateCheckGate = new(1, 1);
    private TorrentMetadata? _pendingTorrent;
    private XRatioSettings _settings = new();
    private string _language = UiText.English;
    private string _statusCanonicalText = "Loading configuration…";
    private HttpProxyServer? _proxy;
    private bool _exiting;
    private bool _paused;
    private bool _sessionPersisted;
    private bool _startupInitializationStarted;
    private bool _settingsLoaded;
    private bool _restoringSimulationForm;
    private string _simulationTimerUnit = SimulationTimerMinutes;
    private bool _simulationCompletedCustomized;
    private bool _suppressSettingsDirty;
    private bool _settingsDirty;
    private bool _settingsResetInProgress;
    private bool _restoreRequested;
    private bool _suppressLanguageSelection;
    private bool _ratioShapingWarningAcknowledged;
    private bool _ratioShapingWarningShowing;
    private Uri? _latestReleaseUri;
    private Uri? _latestDownloadUri;
    private UpdateCheckResult? _latestUpdate;
    private bool _updateInstallInProgress;
    private bool _onboardingBuilt;
    private bool _onboardingAutoDismissPending;
    private bool _onboardingReplayActive;
    private bool _certificateTrusted;
    private bool _torrentClientDetectionComplete;
    private DetectedTorrentClient? _detectedTorrentClient;
    private int _onboardingStepIndex;
    private int _torrentPersistenceRequested;
    private int _torrentPersistenceWriterRunning;
    private int _proxyActivityDrainScheduled;
    private bool _simulationRefreshScheduled;
    private bool _simulationRowsRefreshPending;
    private bool _torrentsRefreshPending;
    private Guid? _pendingSimulationRefreshId;
    private CancellationTokenSource? _simulationFormSaveCancellation;
    private double _simulationSessionsHeight = SimulationSessionsSurfaceHeight;
    private bool _isDraggingSimulationSessions;
    private DateTimeOffset _sessionStarted;

    internal static readonly IReadOnlyList<string> TrayIconStyles = ["Color", "Monochrome"];

    private static class OnboardingStepIds
    {
        public const string Qbittorrent = "qbittorrent";
        public const string Https = "https";
        public const string Interception = "interception";
        public const string Simulation = "simulation";
    }

    private enum OnboardingAction
    {
        OpenGuide,
        OpenPlatform,
        OpenInterception,
        OpenSimulation
    }

    private sealed class OnboardingSidebarRowView
    {
        public OnboardingSidebarRowView(
            int stepIndex,
            Button button,
            Border statusBadge,
            TextBlock status,
            TextBlock title,
            TextBlock meta,
            Border metaPill,
            TextBlock chevron)
        {
            StepIndex = stepIndex;
            Button = button;
            StatusBadge = statusBadge;
            Status = status;
            Title = title;
            Meta = meta;
            MetaPill = metaPill;
            Chevron = chevron;
        }

        public int StepIndex { get; }
        public Button Button { get; }
        public Border StatusBadge { get; }
        public TextBlock Status { get; }
        public TextBlock Title { get; }
        public TextBlock Meta { get; }
        public Border MetaPill { get; }
        public TextBlock Chevron { get; }
    }

    private sealed record OnboardingStep(
        string Id,
        string Title,
        string Description,
        string Detail,
        string ActionLabel,
        OnboardingAction Action,
        bool Optional = false);

    private static readonly IReadOnlyList<OnboardingStep> OnboardingSteps =
    [
        new(
            OnboardingStepIds.Qbittorrent,
            "Connect your torrent client",
            "Configure qBittorrent or another torrent client to use XRatio as its HTTP proxy.",
            "Host 127.0.0.1 · use the XRatio port",
            "Open setup guide",
            OnboardingAction.OpenGuide),
        new(
            OnboardingStepIds.Https,
            "Enable HTTPS when needed",
            "Optional: trust the local CA for HTTPS trackers.",
            "Platform > HTTPS interception",
            "Open Platform",
            OnboardingAction.OpenPlatform,
            Optional: true),
        new(
            OnboardingStepIds.Interception,
            "Use interception",
            "Open the live tracker view and learn what appears there.",
            "Torrent · tracker · peers · counters · last announce",
            "Show me how",
            OnboardingAction.OpenInterception),
        new(
            OnboardingStepIds.Simulation,
            "Use simulation",
            "Create and run an independent tracker session from a .torrent file.",
            "Choose file · add session · start · monitor",
            "Show me how",
            OnboardingAction.OpenSimulation)
    ];

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

    internal static string NormalizeSimulationTimerUnit(string? unit) =>
        string.Equals(unit?.Trim(), SimulationTimerHours, StringComparison.OrdinalIgnoreCase)
            ? SimulationTimerHours
            : SimulationTimerMinutes;

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

    internal bool IsUpdateAvailable => _latestUpdate?.IsUpdateAvailable == true;

    internal bool IsTrayIconEnabled => IsTrayAvailable() && _settings.ShowTrayIcon;

    internal bool UseMonochromeTrayIcon =>
        string.Equals(_settings.TrayIconStyle, "Monochrome", StringComparison.Ordinal);

    internal string CurrentLanguage => _language;

    internal event Action<bool, bool>? RuntimeStateChanged;

    internal event Action<bool>? UpdateAvailabilityChanged;

    internal event Action<string>? LanguageChanged;

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
        SetStatus("Loading configuration…");
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
        _settingsResetAction.Content = "Reset to defaults";
        _settingsResetAction.Tag = "ResetSettings";
        StyleButton(_settingsResetAction, ButtonTone.Danger, minWidth: 148);
        _settingsResetAction.IsEnabled = false;
        _settingsResetAction.Click += async (_, _) => await ResetSettingsAsync();
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
            if (_tabs.SelectedIndex != 1)
                _interceptionOnboardingCoachmark.IsVisible = false;
            if (_tabs.SelectedIndex != 2)
                _simulationOnboardingCoachmark.IsVisible = false;
            RefreshNavigationStyles();
            if (_tabs.SelectedIndex == 2 && _simulationRowsRefreshPending)
            {
                _simulationRowsRefreshPending = false;
                RefreshSimulationRows();
            }
            if (_tabs.SelectedIndex == 1 && _torrentsRefreshPending)
            {
                _torrentsRefreshPending = false;
                RefreshTorrents();
            }
        };

        var navigation = BuildNavigation();
        var body = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("250,*"),
            Children =
            {
                Place(navigation, column: 0),
                Place(_tabs, column: 1),
                Place(BuildOnboardingOverlayReference(), column: 0),
                Place(BuildInterceptionOnboardingCoachmark(), column: 1),
                Place(BuildSimulationOnboardingCoachmark(), column: 1)
            }
        };
        Grid.SetColumnSpan(_onboardingOverlay, 2);
        _onboardingOverlay.SetValue(Panel.ZIndexProperty, 100);
        _interceptionOnboardingCoachmark.SetValue(Panel.ZIndexProperty, 80);
        _simulationOnboardingCoachmark.SetValue(Panel.ZIndexProperty, 80);
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
                if (ToolTip.GetTip(control) is string tooltipText)
                    ToolTip.SetTip(control, UiText.TranslateMessage(tooltipText, _language));

                switch (control)
                {
                    case TextBlock textBlock when Equals(textBlock.Tag, "OnboardingSidebarLabel"):
                        // The compact navigation label is a product name, not a
                        // localized sentence. Keep it stable in every locale.
                        break;
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

        ApplySettingsTooltips();
        _status.Text = L(_statusCanonicalText);
        _autostartCapability.Text = $"{L("Autostart")}: {L(_autostart.Capability.Description)}";
        _certificateCapability.Text = $"{L("Certificates")}: {L(_certificates.Capability.Description)}";
        // Keep the compact action label anchored to the canonical key. A
        // previous locale can have already translated the TextBlock, so using
        // the current value here would make language switches one-way.
        _updateIndicatorLabel.Text = UiText.UpdateIndicatorLabel(_language);
        RefreshUpdateIndicatorState();
        RefreshOnboarding();

        if (root is Window window)
            window.Title = UiText.TranslateMessage(window.Title ?? string.Empty, _language);
    }

    private string L(string text) => UiText.TranslateMessage(text, _language);

    private void SetStatus(string canonicalText)
    {
        _statusCanonicalText = canonicalText;
        _status.Text = L(canonicalText);
    }

    private void ApplySettingsTooltips()
    {
        SetTooltip(_port, "The localhost port used by XRatio's HTTP proxy. Keep it free and use the same port in qBittorrent.");
        SetTooltip(_minimumPeers, "Minimum incomplete peers required before ratio shaping adds calculated upload.");
        SetTooltip(_onlyTrackers, "Blocks non-tracker traffic so XRatio stays focused on tracker announce requests.");
        SetTooltip(_onlyLocal, "Keeps the proxy bound to localhost. This required security boundary cannot be disabled.");
        SetTooltip(_proxyDebugLogging, "Writes redacted proxy diagnostics to %APPDATA%\\XRatio\\proxy_debug.log. Log files are retained for up to 7 days and rotated at 1 MiB. Enable only while troubleshooting.");
        SetTooltip(_downloadRatioMin, "Lower bound for upload credited per actual download during announce shaping.");
        SetTooltip(_downloadRatioMax, "Upper bound for upload credited per actual download during announce shaping.");
        SetTooltip(_uploadRatioMin, "Lower bound for the upload multiplier applied to actual upload.");
        SetTooltip(_uploadRatioMax, "Upper bound for the upload multiplier applied to actual upload.");
        SetTooltip(_boost, "Maximum extra upload boost used during a shaped announce, in KiB/s.");
        SetTooltip(_boostChance, "Percentage chance, from 0 to 100, that the extra upload boost is applied.");
        SetTooltip(_noDownload, "Always enabled: reports zero downloaded bytes. Use Pause or Stop to suspend rewriting.");
        SetTooltip(_pretendSeed, "Does not increase your ratio. When enabled, completed torrents are reported with left=0 so the tracker sees them as seeding; active downloads keep their remaining bytes.");
        SetTooltip(_themeMode, "Changes the visual theme without changing proxy behavior.");
        SetTooltip(_accentColor, "Changes the interface accent color without changing proxy behavior.");
        SetTooltip(_trayIconStyle, "Chooses whether the notification-area icon uses color states or monochrome.");
        SetTooltip(_languageMode, "Changes the language used by the XRatio interface.");
        SetTooltip(_autoStart, "Starts XRatio automatically with your Windows session.");
        SetTooltip(_showTrayIcon, "Keeps an XRatio icon in the Windows notification area.");
        SetTooltip(_startMinimized, "Starts XRatio hidden in the notification area instead of opening the main window.");
        SetTooltip(_certificateConsent, "Confirms that XRatio may add its local CA to the current Windows user's trust store for HTTPS interception.");
        SetTooltip(_settingsResetAction, "Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved.");
        // Keep this action on the native arrow cursor. The compact button
        // reveals its text on hover, so the tooltip remains supplemental and
        // must not turn the pointer into the misleading help cursor.
        ToolTip.SetTip(_updateIndicator, L("Download the new version"));
        ToolTip.SetTip(_bugReportButton, L("Report a bug on GitHub"));
        AutomationProperties.SetName(_bugReportButton, L("Report a bug"));
        SetTooltip(_checkUpdatesOnStartup, "Checks GitHub automatically when XRatio starts.");
    }

    private void SetTooltip(Control control, string message)
    {
        var localized = L(message);
        ToolTip.SetTip(control, localized);
        var helpCursor = new Cursor(StandardCursorType.Help);
        var textOnly = control is CheckBox;
        if (control is ComboBox comboBox)
        {
            // A selector is an interactive control, not a help target. Keep the
            // tooltip, but never replace the normal arrow with the question-mark
            // cursor while the dropdown or its selected value is under the pointer.
            ApplyNormalCursor(comboBox);
            comboBox.TemplateApplied -= OnTooltipComboBoxTemplateApplied;
            comboBox.TemplateApplied += OnTooltipComboBoxTemplateApplied;
        }
        else if (control is TextBox textBox)
        {
            // Text fields are edited or selected, not help targets. Keep the
            // tooltip on the field label while using the text-selection cursor
            // over the editable value itself.
            ApplyTextBoxCursor(textBox);
            textBox.TemplateApplied -= OnTooltipTextBoxTemplateApplied;
            textBox.TemplateApplied += OnTooltipTextBoxTemplateApplied;
        }
        else
        {
            ApplyTooltipCursor(control, helpCursor, textOnly);
        }
        if (control is CheckBox checkBox)
        {
            // Fluent creates the checkbox glyph and content presenter only
            // when the template is applied. Reapply the split cursor then.
            checkBox.TemplateApplied -= OnTooltipCheckBoxTemplateApplied;
            checkBox.TemplateApplied += OnTooltipCheckBoxTemplateApplied;
        }

        if (control.GetVisualParent() is not Grid grid)
            return;

        var row = Grid.GetRow(control);
        foreach (var label in grid.Children.OfType<TextBlock>())
        {
            if (Grid.GetRow(label) == row)
            {
                ToolTip.SetTip(label, localized);
                label.Cursor = helpCursor;
            }
        }
    }

    private void OnTooltipCheckBoxTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is CheckBox checkBox)
            ApplyTooltipCursor(checkBox, new Cursor(StandardCursorType.Help), textOnly: true);
    }

    private static void OnTooltipComboBoxTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is ComboBox comboBox)
            ApplyNormalCursor(comboBox);
    }

    private static void OnTooltipTextBoxTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is TextBox textBox)
            ApplyTextBoxCursor(textBox);
    }

    private static void ApplyNormalCursor(Control control)
    {
        var arrowCursor = new Cursor(StandardCursorType.Arrow);
        control.Cursor = arrowCursor;
        foreach (var visual in control.GetVisualDescendants())
        {
            if (visual is InputElement inputElement)
                inputElement.Cursor = arrowCursor;
        }
    }

    private static void ApplyTextBoxCursor(TextBox textBox)
    {
        var textCursor = new Cursor(StandardCursorType.Ibeam);
        textBox.Cursor = textCursor;
        foreach (var visual in textBox.GetVisualDescendants())
        {
            if (visual is InputElement inputElement)
                inputElement.Cursor = textCursor;
        }
    }

    private static void ApplyTooltipCursor(Control control, Cursor helpCursor, bool textOnly)
    {
        var defaultCursor = textOnly
            ? new Cursor(StandardCursorType.Arrow)
            : helpCursor;
        control.Cursor = defaultCursor;
        foreach (var visual in control.GetVisualDescendants())
        {
            if (visual is not InputElement inputElement)
                continue;

            inputElement.Cursor = textOnly && IsTooltipTextVisual(visual)
                ? helpCursor
                : defaultCursor;
        }
    }

    private static bool IsTooltipTextVisual(Visual visual) =>
        visual is TextBlock or ContentPresenter ||
        visual.GetType().Name.Contains("TextPresenter", StringComparison.Ordinal);

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
                if (text.Tag is "OnboardingChecklistStatus" or
                    "OnboardingChecklistAction" or
                    "OnboardingChecklistProgress")
                    continue;

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
                            Foreground = XRatioPalette.Ink,
                            VerticalAlignment = VerticalAlignment.Center
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
                }
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

    private Control BuildOnboardingOverlay()
    {
        _onboardingOverlay.Tag = "OnboardingOverlay";
        _onboardingOverlay.IsVisible = false;
        _onboardingOverlay.HorizontalAlignment = HorizontalAlignment.Stretch;
        _onboardingOverlay.VerticalAlignment = VerticalAlignment.Stretch;
        _onboardingOverlay.Background = Brushes.Transparent;

        _onboardingClose.Tag = "OnboardingClose";
        _onboardingClose.Content = BuildCloseGlyph(CloseGlyphSize);
        ConfigureGuideButton(_onboardingClose);
        _onboardingClose.FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets");
        _onboardingClose.FontSize = 13;
        _onboardingClose.Foreground = XRatioPalette.Subtle;
        _onboardingClose.Click += async (_, _) => await DismissOnboardingAsync();

        _onboardingTitle.Text = "Onboarding";
        _onboardingTitle.FontSize = 20;
        _onboardingTitle.FontWeight = FontWeight.Bold;
        _onboardingTitle.Foreground = XRatioPalette.Ink;
        _onboardingTitle.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _onboardingIntro.Text = "Quick setup";
        _onboardingIntro.FontSize = 11.5;
        _onboardingIntro.Foreground = XRatioPalette.Muted;
        _onboardingIntro.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _onboardingStepCounter.FontSize = 11.5;
        _onboardingStepCounter.FontWeight = FontWeight.SemiBold;
        _onboardingStepCounter.Foreground = XRatioPalette.Accent;
        _onboardingStepCounter.VerticalAlignment = VerticalAlignment.Center;

        _onboardingDots.Orientation = Orientation.Horizontal;
        _onboardingDots.Spacing = 6;
        _onboardingDots.VerticalAlignment = VerticalAlignment.Center;
        for (var index = 0; index < OnboardingSteps.Count; index++)
        {
            _onboardingDots.Children.Add(new Border
            {
                Tag = index,
                Width = 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = XRatioPalette.Border,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        _onboardingStepTitle.FontSize = 17;
        _onboardingStepTitle.FontWeight = FontWeight.SemiBold;
        _onboardingStepTitle.Foreground = XRatioPalette.Ink;
        _onboardingStepTitle.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _onboardingStepDescription.FontSize = 12;
        _onboardingStepDescription.Foreground = XRatioPalette.Muted;
        _onboardingStepDescription.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _onboardingStepDetail.FontSize = 11.5;
        _onboardingStepDetail.Foreground = XRatioPalette.Ink;
        _onboardingStepDetail.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        var stepHeader = new StackPanel
        {
            Spacing = 4,
            Children = { _onboardingStepTitle, _onboardingStepDescription }
        };

        var detail = new Border
        {
            Background = XRatioPalette.MetricSurface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(7),
            Padding = new Thickness(10, 8),
            Child = _onboardingStepDetail
        };

        _onboardingAction.Tag = "OnboardingAction";
        StyleButton(_onboardingAction, ButtonTone.Secondary, 178);
        _onboardingAction.Click += async (_, _) => await RunOnboardingActionAsync();

        _onboardingMarkDone.Tag = "OnboardingMarkDone";
        _onboardingMarkDone.Content = "✓";
        StyleButton(_onboardingMarkDone, ButtonTone.Quiet, 36);
        _onboardingMarkDone.Width = 36;
        _onboardingMarkDone.MinWidth = 36;
        _onboardingMarkDone.Padding = new Thickness(0);
        _onboardingMarkDone.CornerRadius = new CornerRadius(18);
        ToolTip.SetTip(_onboardingMarkDone, L("Mark as done"));
        _onboardingMarkDone.Click += async (_, _) => await MarkCurrentOnboardingStepAsync();

        var stepActions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 8,
            Children = { _onboardingAction, Place(_onboardingMarkDone, column: 1) }
        };

        var stepCard = new Border
        {
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14),
            Child = new StackPanel
            {
                Spacing = 10,
                Children = { stepHeader, detail, stepActions }
            }
        };

        _onboardingPrevious.Tag = "OnboardingPrevious";
        _onboardingPrevious.Content = "←";
        StyleButton(_onboardingPrevious, ButtonTone.Quiet, 36);
        _onboardingPrevious.Width = 36;
        _onboardingPrevious.MinWidth = 36;
        _onboardingPrevious.Padding = new Thickness(0);
        _onboardingPrevious.Click += (_, _) =>
        {
            if (_onboardingStepIndex <= 0)
                return;
            _onboardingStepIndex--;
            RefreshOnboarding();
        };

        _onboardingNext.Tag = "OnboardingNext";
        _onboardingNext.Content = "→";
        StyleButton(_onboardingNext, ButtonTone.Primary, 36);
        _onboardingNext.Width = 36;
        _onboardingNext.MinWidth = 36;
        _onboardingNext.Padding = new Thickness(0);
        _onboardingNext.Click += async (_, _) => await MoveToNextOnboardingStepAsync();

        var footer = new Border
        {
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(0, 12, 0, 0),
            Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                Children =
                {
                    _onboardingPrevious,
                    Place(_onboardingDots, column: 1),
                    Place(_onboardingNext, column: 2)
                }
            }
        };

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 12,
            Children =
            {
                new TextBlock
                {
                    Text = "ONBOARDING",
                    FontSize = 9.5,
                    FontWeight = FontWeight.Bold,
                    Foreground = XRatioPalette.Accent,
                    LetterSpacing = 1.15,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Place(_onboardingClose, column: 1)
            }
        };

        var progress = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto"),
            Children =
            {
                _onboardingStepCounter
            }
        };

        var cardContent = new StackPanel
        {
            Spacing = 10,
            Children =
            {
                header,
                new StackPanel
                {
                    Spacing = 5,
                    Children = { _onboardingTitle }
                },
                progress,
                stepCard,
                footer
            }
        };

        var card = new Border
        {
            Tag = "OnboardingCard",
            Width = 460,
            MinWidth = 320,
            MaxWidth = 500,
            MaxHeight = 500,
            Margin = new Thickness(16),
            Padding = new Thickness(16),
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = cardContent
            }
        };

        _onboardingOverlay.Child = new Grid
        {
            Children =
            {
                new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(142, 8, 14, 24)),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                },
                card
            }
        };
        _onboardingBuilt = true;
        RefreshOnboarding();
        return _onboardingOverlay;
    }

    // Reference-led overlay: one quiet surface, one concrete example, and a
    // single primary action. The previous implementation nested a second card
    // inside the dialog, which made the tour feel like a settings form.
    private Control BuildOnboardingOverlayReference()
    {
        _onboardingOverlay.Tag = "OnboardingOverlay";
        _onboardingOverlay.IsVisible = false;
        _onboardingOverlay.HorizontalAlignment = HorizontalAlignment.Stretch;
        _onboardingOverlay.VerticalAlignment = VerticalAlignment.Stretch;
        _onboardingOverlay.Background = Brushes.Transparent;

        _onboardingClose.Tag = "OnboardingClose";
        _onboardingClose.Content = BuildCloseGlyph(CloseGlyphSize);
        ConfigureGuideButton(_onboardingClose);
        _onboardingClose.FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets");
        _onboardingClose.FontSize = 13;
        _onboardingClose.Foreground = XRatioPalette.ReferenceMuted;
        _onboardingClose.Click += async (_, _) => await DismissOnboardingAsync();

        // The modal follows the assistant-ui card: the step title is the only
        // visible heading. Keep the legacy title copy hidden for automation
        // and localization compatibility so it cannot create a duplicate
        // heading in the card.
        _onboardingTitle.Text = "Quick setup";
        _onboardingTitle.FontSize = 15;
        _onboardingTitle.FontWeight = FontWeight.SemiBold;
        _onboardingTitle.Foreground = XRatioPalette.ReferenceText;
        _onboardingTitle.VerticalAlignment = VerticalAlignment.Center;
        _onboardingTitle.IsVisible = false;

        // Keep the legacy copy available to localization/tests, but do not put
        // a second paragraph in the focused card.
        _onboardingIntro.Text = "Quick setup";
        _onboardingIntro.IsVisible = false;

        _onboardingStepCounter.FontSize = 10.5;
        _onboardingStepCounter.FontWeight = FontWeight.SemiBold;
        _onboardingStepCounter.Foreground = XRatioPalette.ReferenceMuted;
        _onboardingStepCounter.VerticalAlignment = VerticalAlignment.Center;
        _onboardingStepStatusIcon.FontSize = 12;
        _onboardingStepStatusIcon.FontWeight = FontWeight.Bold;
        _onboardingStepStatusIcon.VerticalAlignment = VerticalAlignment.Center;
        _onboardingStepStatusIcon.IsVisible = false;
        _onboardingStepStatus.FontSize = 10.5;
        _onboardingStepStatus.FontWeight = FontWeight.SemiBold;
        _onboardingStepStatus.VerticalAlignment = VerticalAlignment.Center;
        _onboardingStepStatus.IsVisible = false;

        _onboardingDots.Orientation = Orientation.Horizontal;
        _onboardingDots.Spacing = 5;
        _onboardingDots.HorizontalAlignment = HorizontalAlignment.Center;
        _onboardingDots.VerticalAlignment = VerticalAlignment.Center;
        _onboardingDots.Children.Clear();
        for (var index = 0; index < OnboardingSteps.Count; index++)
        {
            _onboardingDots.Children.Add(new Border
            {
                Tag = index,
                Width = 5,
                Height = 5,
                CornerRadius = new CornerRadius(3),
                Background = XRatioPalette.ReferenceBorder,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        _onboardingStepTitle.FontSize = 16;
        _onboardingStepTitle.FontWeight = FontWeight.SemiBold;
        _onboardingStepTitle.Foreground = XRatioPalette.ReferenceText;
        _onboardingStepTitle.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _onboardingStepDescription.FontSize = 12.5;
        _onboardingStepDescription.Foreground = XRatioPalette.ReferenceMuted;
        _onboardingStepDescription.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _onboardingStepDetail.FontSize = 12;
        _onboardingStepDetail.Foreground = XRatioPalette.ReferenceMuted;
        _onboardingStepDetail.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        var stepHeader = new StackPanel
        {
            Spacing = 6,
            Children = { _onboardingStepTitle, _onboardingStepDescription }
        };

        _onboardingAction.Tag = "OnboardingAction";
        StyleButton(_onboardingAction, ButtonTone.Secondary, 0);
        _onboardingAction.MinHeight = 36;
        _onboardingAction.HorizontalAlignment = HorizontalAlignment.Stretch;
        _onboardingAction.HorizontalContentAlignment = HorizontalAlignment.Left;
        _onboardingAction.Padding = new Thickness(11, 6);
        _onboardingAction.CornerRadius = new CornerRadius(10);
        _onboardingAction.Background = XRatioPalette.ReferenceField;
        _onboardingAction.BorderBrush = XRatioPalette.ReferenceBorder;
        _onboardingAction.BorderThickness = new Thickness(1);
        _onboardingAction.Foreground = XRatioPalette.ReferenceText;
        _onboardingAction.Click += async (_, _) => await RunOnboardingActionAsync();

        _onboardingMarkDone.Tag = "OnboardingMarkDone";
        _onboardingMarkDone.Content = "✓";
        StyleButton(_onboardingMarkDone, ButtonTone.Quiet, 34);
        _onboardingMarkDone.Width = 34;
        _onboardingMarkDone.MinWidth = 34;
        _onboardingMarkDone.Height = 34;
        _onboardingMarkDone.MinHeight = 36;
        _onboardingMarkDone.Padding = new Thickness(0);
        _onboardingMarkDone.CornerRadius = new CornerRadius(17);
        _onboardingMarkDone.Background = XRatioPalette.ReferenceField;
        _onboardingMarkDone.BorderBrush = XRatioPalette.ReferenceBorder;
        _onboardingMarkDone.BorderThickness = new Thickness(1);
        _onboardingMarkDone.Foreground = XRatioPalette.ReferenceMuted;
        ToolTip.SetTip(_onboardingMarkDone, L("Mark as done"));
        _onboardingMarkDone.Click += async (_, _) => await MarkCurrentOnboardingStepAsync();

        var actionRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 7,
            Children = { _onboardingAction, Place(_onboardingMarkDone, column: 1) }
        };

        _onboardingPrevious.Tag = "OnboardingPrevious";
        _onboardingPrevious.Content = "←";
        StyleButton(_onboardingPrevious, ButtonTone.Quiet, 30);
        _onboardingPrevious.Width = 30;
        _onboardingPrevious.MinWidth = 30;
        _onboardingPrevious.Height = 30;
        _onboardingPrevious.MinHeight = 36;
        _onboardingPrevious.Padding = new Thickness(0);
        _onboardingPrevious.CornerRadius = new CornerRadius(15);
        _onboardingPrevious.Background = XRatioPalette.ReferenceField;
        _onboardingPrevious.BorderBrush = XRatioPalette.ReferenceBorder;
        _onboardingPrevious.BorderThickness = new Thickness(1);
        _onboardingPrevious.Foreground = XRatioPalette.ReferenceText;
        _onboardingPrevious.Classes.Add("reference-pager");
        _onboardingPrevious.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("reference-pager").Class(":disabled"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.ReferenceField),
                new Setter(Button.BorderBrushProperty, XRatioPalette.ReferenceBorder),
                new Setter(Button.ForegroundProperty, XRatioPalette.ReferenceSubtle),
                new Setter(Button.OpacityProperty, 0.58)
            }
        });
        _onboardingPrevious.Click += (_, _) =>
        {
            if (_onboardingStepIndex <= 0)
                return;
            _onboardingStepIndex--;
            RefreshOnboarding();
        };

        _onboardingNext.Tag = "OnboardingNext";
        _onboardingNext.Content = "→";
        StyleButton(_onboardingNext, ButtonTone.Quiet, 30);
        _onboardingNext.Width = 30;
        _onboardingNext.MinWidth = 30;
        _onboardingNext.Height = 30;
        _onboardingNext.MinHeight = 36;
        _onboardingNext.Padding = new Thickness(0);
        _onboardingNext.CornerRadius = new CornerRadius(15);
        _onboardingNext.Background = XRatioPalette.ReferenceField;
        _onboardingNext.BorderBrush = XRatioPalette.ReferenceBorder;
        _onboardingNext.BorderThickness = new Thickness(1);
        _onboardingNext.Foreground = XRatioPalette.ReferenceText;
        _onboardingNext.Click += async (_, _) => await MoveToNextOnboardingStepAsync();

        var status = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _onboardingStepStatusIcon, _onboardingStepStatus }
        };

        var progress = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Children =
            {
                _onboardingStepCounter,
                Place(status, column: 2)
            }
        };

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 8,
            Margin = new Thickness(0, 2, 0, 0),
            Children =
            {
                _onboardingPrevious,
                Place(_onboardingDots, column: 1),
                Place(_onboardingNext, column: 2)
            }
        };

        var cardContent = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    ColumnSpacing = 8,
                    Children =
                    {
                        Place(_onboardingClose, column: 1)
                    }
                },
                new StackPanel
                {
                    IsVisible = false,
                    Children = { _onboardingTitle, _onboardingIntro }
                },
                progress,
                stepHeader,
                actionRow,
                footer
            }
        };

        var card = new Border
        {
            Tag = "OnboardingCard",
            Width = 384,
            MinWidth = 320,
            MaxWidth = 384,
            MaxHeight = 430,
            Margin = new Thickness(16),
            Padding = new Thickness(20),
            Background = XRatioPalette.ReferenceSurface,
            BorderBrush = XRatioPalette.ReferenceBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = cardContent
            }
        };

        _onboardingOverlay.Child = new Grid
        {
            Children =
            {
                new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(172, 8, 14, 24)),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                },
                card
            }
        };
        _onboardingBuilt = true;
        RefreshOnboarding();
        return _onboardingOverlay;
    }

    private void RefreshOnboarding()
    {
        if (!_onboardingBuilt || OnboardingSteps.Count == 0)
            return;

        var onboardingVisible = !_settingsLoaded || !_settings.OnboardingDismissed;
        _onboardingNavigationGroup.IsVisible = onboardingVisible;
        _overviewOnboardingCard.IsVisible = onboardingVisible;
        // The Settings tab remains visible even when the tour is dismissed.
        // Refresh its replay status before the hidden-surface fast path so the
        // button cannot stay disabled with the initial loading label forever.
        UpdateOnboardingSettingsStatus();
        if (!onboardingVisible)
        {
            _onboardingOverlay.IsVisible = false;
            _interceptionOnboardingCoachmark.IsVisible = false;
            _simulationOnboardingCoachmark.IsVisible = false;
            // Once onboarding is dismissed there is no visible onboarding
            // surface to update on every tracker event. The next explicit
            // replay calls RefreshOnboarding again after making the surfaces
            // visible, so skip the expensive tree-wide state pass here.
            return;
        }

        _onboardingStepIndex = Math.Clamp(_onboardingStepIndex, 0, OnboardingSteps.Count - 1);
        var step = OnboardingSteps[_onboardingStepIndex];
        var complete = IsOnboardingStepComplete(step);
        _onboardingStepCounter.Text = $"{_onboardingStepIndex + 1} {L("of")} {OnboardingSteps.Count}";
        _onboardingStepNumber.Text = (_onboardingStepIndex + 1).ToString(CultureInfo.InvariantCulture);
        _onboardingStepStatusIcon.Text = complete ? "✓" : "○";
        _onboardingStepStatusIcon.Foreground = complete ? XRatioPalette.Positive : XRatioPalette.Subtle;
        _onboardingStepStatusIcon.IsVisible = false;
        _onboardingStepStatus.Text = L(complete ? "Completed" : step.Optional ? "Optional" : "To do");
        _onboardingStepStatus.Foreground = complete ? XRatioPalette.Positive : XRatioPalette.Muted;
        _onboardingStepStatus.IsVisible = false;
        var description = step.Description;
        var detailText = SidebarOnboardingDetail(step);
        var actionLabel = step.ActionLabel;
        if (step.Id == OnboardingStepIds.Qbittorrent && _torrentClientDetectionComplete)
        {
            description = QbittorrentOnboardingDescription();
            detailText = QbittorrentOnboardingDetail();
            if (_detectedTorrentClient is not null)
            {
                actionLabel = "Open qBittorrent";
            }
        }
        _onboardingStepTitle.Text = L(step.Title);
        _onboardingStepDescription.Text = L(description);
        _onboardingStepDetail.Text = L(detailText);
        _onboardingAction.Content = L(actionLabel);
        _onboardingAction.IsEnabled = true;
        _onboardingMarkDone.Content = "✓";
        _onboardingMarkDone.IsVisible = !complete;
        _onboardingMarkDone.IsEnabled = !complete;
        ToolTip.SetTip(_onboardingMarkDone, L(complete ? "Completed" : "Mark as done"));
        // Keep the left arrow visible even on the first step. It is a quiet
        // affordance there (the click handler is a no-op), matching the
        // always-present navigation arrows in the reference cards.
        _onboardingPrevious.IsEnabled = true;
        _onboardingPrevious.Opacity = _onboardingStepIndex > 0 ? 1 : 0.45;
        _onboardingNext.Content = "→";
        ToolTip.SetTip(_onboardingClose, L("Close onboarding"));

        foreach (var dot in _onboardingDots.Children.OfType<Border>())
        {
            if (dot.Tag is not int dotIndex)
                continue;
            var dotComplete = IsOnboardingStepComplete(OnboardingSteps[dotIndex]);
            var active = dotIndex == _onboardingStepIndex;
            dot.Width = active ? 24 : 8;
            dot.Background = dotComplete
                ? XRatioPalette.ReferenceGreen
                : active
                    ? XRatioPalette.ReferenceMuted
                    : XRatioPalette.ReferenceBorder;
            dot.CornerRadius = new CornerRadius(active ? 4 : 4);
        }

        RefreshOnboardingChecklist();
        RefreshOverviewOnboarding();

        if (_settingsLoaded &&
            !_settings.OnboardingDismissed &&
            !_onboardingReplayActive &&
            !_onboardingAutoDismissPending &&
            OnboardingSteps.All(IsOnboardingStepComplete))
        {
            _onboardingAutoDismissPending = true;
            Dispatcher.UIThread.Post(async () =>
            {
                try
                {
                    await DismissOnboardingAsync();
                }
                finally
                {
                    _onboardingAutoDismissPending = false;
                }
            });
        }
    }

    private void RefreshOnboardingChecklist()
    {
        if (_onboardingSidebarDots.Children.Count != OnboardingSteps.Count ||
            _onboardingSidebarRows.Count != OnboardingSteps.Count)
            return;

        var completedCount = OnboardingSteps.Count(IsOnboardingStepComplete);
        _onboardingChecklistProgress.Text =
            $"{completedCount}/{OnboardingSteps.Count}";
        _overviewOnboardingProgress.Text = $"{completedCount}/{OnboardingSteps.Count}";
        _onboardingChecklistContainer.IsVisible = false;

        var step = OnboardingSteps[_onboardingStepIndex];
        var complete = IsOnboardingStepComplete(step);
        var description = SidebarOnboardingDescription(step);
        var detailText = SidebarOnboardingDetail(step);
        if (step.Id == OnboardingStepIds.Qbittorrent && _torrentClientDetectionComplete)
        {
            description = QbittorrentOnboardingDescription();
            detailText = QbittorrentOnboardingDetail();
        }

        _onboardingSidebarCounter.Text =
            $"{_onboardingStepIndex + 1} {L("of")} {OnboardingSteps.Count}";
        _onboardingSidebarStatusIcon.Text = complete ? "✓" : string.Empty;
        _onboardingSidebarStatusIcon.Foreground = complete ? XRatioPalette.Positive : XRatioPalette.Subtle;
        _onboardingSidebarTitle.Text = L(step.Title);
        _onboardingSidebarDescription.Text = L(description);
        _onboardingSidebarDetail.Text = L(detailText);
        _onboardingSidebarAction.Content = L(SidebarOnboardingActionLabel(step));
        _onboardingSidebarDone.Content = "✓";
        _onboardingSidebarDone.IsVisible = !complete;
        _onboardingSidebarDone.IsEnabled = !complete;
        _onboardingSidebarPrevious.IsEnabled = _onboardingStepIndex > 0;
        _onboardingSidebarNext.Content = "→";
        ToolTip.SetTip(_onboardingSidebarClose, L("Close onboarding"));
        ToolTip.SetTip(_onboardingSidebarPrevious, L("←  Back"));
        ToolTip.SetTip(_onboardingSidebarNext, L("Next step  →"));
        ToolTip.SetTip(_onboardingSidebarDone, L(complete ? "Completed" : "Mark as done"));
        ToolTip.SetTip(
            _onboardingSidebarAction,
            $"{L(step.Title)}: {L(description)}");

        foreach (var dot in _onboardingSidebarDots.Children.OfType<Border>())
        {
            if (dot.Tag is not int dotIndex)
                continue;
            var dotComplete = IsOnboardingStepComplete(OnboardingSteps[dotIndex]);
            var active = dotIndex == _onboardingStepIndex;
            dot.Width = active ? 18 : 5;
            dot.Background = dotComplete
                ? XRatioPalette.Positive
                : active
                    ? XRatioPalette.Accent
                    : XRatioPalette.Border;
        }

        foreach (var row in _onboardingSidebarRows)
        {
            var rowStep = OnboardingSteps[row.StepIndex];
            var rowComplete = IsOnboardingStepComplete(rowStep);
            var rowActive = row.StepIndex == _onboardingStepIndex;
            var rowAction = SidebarOnboardingActionLabel(rowStep);

            row.Button.Background = rowActive
                ? XRatioPalette.NeutralSoft
                : XRatioPalette.Surface;
            row.Button.BorderBrush = rowActive
                ? XRatioPalette.Accent
                : XRatioPalette.Border;
            row.Button.BorderThickness = new Thickness(1);
            row.Status.Text = rowComplete
                ? "✓"
                : (row.StepIndex + 1).ToString(CultureInfo.InvariantCulture);
            row.Status.Foreground = rowComplete
                ? XRatioPalette.OnAccent
                : rowActive
                    ? XRatioPalette.Accent
                    : XRatioPalette.Muted;
            row.StatusBadge.Background = rowComplete
                ? XRatioPalette.Positive
                : rowActive
                    ? XRatioPalette.AccentSoft
                    : XRatioPalette.Surface;
            row.StatusBadge.BorderBrush = rowComplete
                ? XRatioPalette.Positive
                : rowActive
                    ? XRatioPalette.Accent
                    : XRatioPalette.Border;
            row.Title.Foreground = XRatioPalette.Ink;
            row.Meta.Text = L(rowComplete ? "Completed" : rowAction);
            row.Meta.Foreground = rowComplete
                ? XRatioPalette.Positive
                : rowActive
                    ? XRatioPalette.Accent
                    : XRatioPalette.Subtle;
            // Keep the action label readable without nesting a second colored
            // badge inside the capsule. Completion is already communicated by
            // the leading checkmark and the status color.
            row.MetaPill.Background = Brushes.Transparent;
            row.MetaPill.BorderBrush = Brushes.Transparent;
            row.Chevron.Foreground = rowActive ? XRatioPalette.Accent : XRatioPalette.Subtle;
            ToolTip.SetTip(
                row.Button,
                $"{L(rowStep.Title)}: {L(SidebarOnboardingDescription(rowStep))}");
        }

    }

    private void RefreshOverviewOnboarding()
    {
        if (_overviewOnboardingDots.Children.Count != OnboardingSteps.Count)
            return;

        var step = OnboardingSteps[_onboardingStepIndex];
        var complete = IsOnboardingStepComplete(step);
        var description = SidebarOnboardingDescription(step);
        if (step.Id == OnboardingStepIds.Qbittorrent && _torrentClientDetectionComplete)
            description = QbittorrentOnboardingDescription();
        var detail = SidebarOnboardingDetail(step);
        if (step.Id == OnboardingStepIds.Qbittorrent && _torrentClientDetectionComplete)
            detail = QbittorrentOnboardingDetail();

        _overviewOnboardingCounter.Text =
            $"{_onboardingStepIndex + 1} {L("of")} {OnboardingSteps.Count}";
        _overviewOnboardingStatusIcon.Text = complete ? "✓" : string.Empty;
        _overviewOnboardingStatusIcon.Foreground = complete
            ? XRatioPalette.Positive
            : XRatioPalette.Subtle;
        _overviewOnboardingTitle.Text = L(step.Title);
        _overviewOnboardingDescription.Text = L(description);
        _overviewOnboardingDetail.Text = L(detail);
        _overviewOnboardingAction.IsVisible = false;
        var showTorrentClientHelp = step.Id == OnboardingStepIds.Qbittorrent;
        _overviewTorrentClientScreenshot.IsVisible = showTorrentClientHelp;
        _overviewOtherTorrentClientsHint.IsVisible = showTorrentClientHelp;
        _overviewTorrentClientScreenshotTitle.Text = L("qBITTORRENT — PROXY SERVER (FULL VIEW)");
        _overviewOtherTorrentClientsTitle.Text = L("OTHER CLIENTS — DELUGE, TRANSMISSION, TIXATI, BIGLYBT, VUZE…");
        _overviewOtherTorrentClientsDescription.Text = string.Join(
            "\n",
            L("1. Open Settings/Preferences > Connection, Network or Proxy."),
            string.Format(
                CultureInfo.InvariantCulture,
                L("2. Select HTTP. Enter server 127.0.0.1 and port {0}."),
                _settings.ListenPort),
            L("3. Enable the proxy for tracker/BitTorrent traffic. Leave peer connections disabled when that option is separate."));
        _overviewOnboardingAction.Content = L(SidebarOnboardingActionLabel(step));
        var manualQbittorrentConfirmation = step.Id == OnboardingStepIds.Qbittorrent && !complete;
        _overviewOnboardingDone.Content = L("Mark as configured");
        _overviewOnboardingDone.IsVisible = manualQbittorrentConfirmation;
        _overviewOnboardingDone.IsEnabled = manualQbittorrentConfirmation;
        _overviewOnboardingPrevious.IsEnabled = true;
        _overviewOnboardingPrevious.Opacity = _onboardingStepIndex > 0 ? 1 : 0.45;
        _overviewOnboardingNext.Content = "→";
        ToolTip.SetTip(_overviewOnboardingClose, L("Close onboarding"));
        ToolTip.SetTip(_overviewOnboardingPrevious, L("←  Back"));
        ToolTip.SetTip(_overviewOnboardingNext, L("Next step  →"));
        ToolTip.SetTip(_overviewOnboardingDone, L(complete ? "Completed" : "Mark as done"));
        ToolTip.SetTip(
            _overviewOnboardingAction,
            $"{L(step.Title)}: {L(description)}");

        foreach (var dot in _overviewOnboardingDots.Children.OfType<Border>())
        {
            if (dot.Tag is not int dotIndex)
                continue;
            var dotComplete = IsOnboardingStepComplete(OnboardingSteps[dotIndex]);
            var active = dotIndex == _onboardingStepIndex;
            dot.Width = active ? 18 : 5;
            dot.Background = dotComplete
                ? XRatioPalette.Positive
                : active
                    ? XRatioPalette.Accent
                    : XRatioPalette.Border;
        }
    }

    private string QbittorrentOnboardingDescription() => string.Join(
        "\n",
        L("1. Open Tools > Options > Connection."),
        L("2. In Proxy Server, select Type: HTTP."),
        string.Format(
            CultureInfo.InvariantCulture,
            L("3. Enter Host: 127.0.0.1 and Port: {0}."),
            _settings.ListenPort),
        L("4. Enable “Use proxy for BitTorrent purposes”, leave peer connections disabled, then click Apply."));

    private string QbittorrentOnboardingDetail()
    {
        var detail = string.Format(
            CultureInfo.InvariantCulture,
            L("TYPE  HTTP     HOST  127.0.0.1     PORT  {0}"),
            _settings.ListenPort);
        if (!_torrentClientDetectionComplete)
            return detail;

        return detail + L(_detectedTorrentClient is not null
            ? " · qBittorrent detected"
            : " · qBittorrent not detected; use the guide");
    }

    private string SidebarOnboardingDescription(OnboardingStep step) =>
        step.Id switch
        {
            OnboardingStepIds.Qbittorrent => QbittorrentOnboardingDescription(),
            OnboardingStepIds.Https => "Optional: trust the local CA for HTTPS trackers.",
            OnboardingStepIds.Interception => "Open the live tracker view and learn what appears there.",
            OnboardingStepIds.Simulation => "Create and run an independent tracker session from a .torrent file.",
            _ => step.Description
        };

    private string SidebarOnboardingDetail(OnboardingStep step) =>
        step.Id switch
        {
            OnboardingStepIds.Qbittorrent => QbittorrentOnboardingDetail(),
            OnboardingStepIds.Https => "Platform > HTTPS interception",
            OnboardingStepIds.Interception => "Torrent · tracker · peers · counters · last announce",
            OnboardingStepIds.Simulation => "Choose file · add session · start · monitor",
            _ => step.Detail
        };

    private string SidebarOnboardingActionLabel(OnboardingStep step) =>
        step.Id switch
        {
            OnboardingStepIds.Qbittorrent =>
                _torrentClientDetectionComplete && _detectedTorrentClient is not null
                    ? "Open qBittorrent →"
                    : "Setup guide →",
            OnboardingStepIds.Https => "Platform →",
            OnboardingStepIds.Interception => "Show me how →",
            OnboardingStepIds.Simulation => "Show me how →",
            _ => step.ActionLabel
        };

    private bool IsOnboardingStepComplete(OnboardingStep step)
    {
        if (_settings.OnboardingCompletedSteps?.Contains(step.Id, StringComparer.Ordinal) == true)
            return true;

        return step.Id switch
        {
            OnboardingStepIds.Https => !_certificates.Capability.IsSupported || _certificateTrusted,
            _ => false
        };
    }

    private int FirstIncompleteOnboardingStep()
    {
        var index = OnboardingSteps
            .Select((step, itemIndex) => (step, itemIndex))
            .FirstOrDefault(item => !IsOnboardingStepComplete(item.step));
        return index.step is null ? OnboardingSteps.Count - 1 : index.itemIndex;
    }

    private void ShowOnboarding()
    {
        if (!_onboardingBuilt || !_settingsLoaded || _settings.OnboardingDismissed)
            return;

        _onboardingStepIndex = FirstIncompleteOnboardingStep();
        _onboardingOverlay.IsVisible = false;
        _onboardingNavigationGroup.IsVisible = true;
        _overviewOnboardingCard.IsVisible = true;
        RefreshOnboarding();
    }

    private Task OpenOnboardingFromNavigationAsync()
    {
        if (!_settingsLoaded)
            return Task.CompletedTask;

        // The sidebar entry is an intentional replay action. If the user closed
        // the tour earlier, clear that dismissal before showing it again.
        if (_settings.OnboardingDismissed)
            return RestoreOnboardingAsync();

        _tabs.SelectedIndex = 0;
        ShowOnboarding();
        return Task.CompletedTask;
    }

    private async Task OpenOnboardingSidebarStepAsync(int stepIndex)
    {
        if (!_settingsLoaded || OnboardingSteps.Count == 0)
            return;

        _onboardingStepIndex = Math.Clamp(stepIndex, 0, OnboardingSteps.Count - 1);
        if (_settings.OnboardingDismissed)
            await RestoreOnboardingAsync();
        if (_settings.OnboardingDismissed)
            return;

        _onboardingStepIndex = Math.Clamp(stepIndex, 0, OnboardingSteps.Count - 1);
        RefreshOnboarding();
        if (OnboardingSteps[_onboardingStepIndex].Id == OnboardingStepIds.Qbittorrent)
        {
            Dispatcher.UIThread.Post(
                _overviewTorrentClientScreenshot.BringIntoView,
                DispatcherPriority.Loaded);
        }
        await RunOnboardingActionAsync();
    }

    private async Task DismissOnboardingAsync()
    {
        if (!_settingsLoaded)
        {
            _onboardingOverlay.IsVisible = false;
            return;
        }

        var wasDirty = _settingsDirty;
        try
        {
            _settings = _settings with { OnboardingDismissed = true };
            _onboardingReplayActive = false;
            await SaveSettingsAsync();
            _onboardingOverlay.IsVisible = false;
            _onboardingNavigationGroup.IsVisible = false;
            _overviewOnboardingCard.IsVisible = false;
            if (wasDirty)
                MarkSettingsDirty();
            else
                MarkSettingsSaved();
            AddActivity("Onboarding closed.");
            RefreshOnboarding();
            UpdateOnboardingSettingsStatus();
        }
        catch (Exception exception)
        {
            _settingsSaveStatus.Text = L($"Onboarding could not be closed: {exception.Message}");
            _settingsSaveStatus.Foreground = XRatioPalette.Danger;
            AddActivity($"Onboarding close error: {exception.Message}", ActivityLevel.Error, "Settings");
        }
    }

    private async Task RestoreOnboardingAsync()
    {
        if (!_settingsLoaded)
            return;

        var wasDirty = _settingsDirty;
        try
        {
            _settings = _settings with { OnboardingDismissed = false };
            _onboardingReplayActive = true;
            await SaveSettingsAsync();
            _onboardingNavigationGroup.IsVisible = true;
            _overviewOnboardingCard.IsVisible = true;
            if (wasDirty)
                MarkSettingsDirty();
            else
                MarkSettingsSaved();
            _onboardingStepIndex = FirstIncompleteOnboardingStep();
            _tabs.SelectedIndex = 0;
            ShowOnboarding();
            AddActivity("Onboarding reopened.", ActivityLevel.Success, "Settings");
        }
        catch (Exception exception)
        {
            _onboardingSettingsStatus.Text = L($"Onboarding could not be restored: {exception.Message}");
            _onboardingSettingsStatus.Foreground = XRatioPalette.Danger;
            AddActivity($"Onboarding restore error: {exception.Message}", ActivityLevel.Error, "Settings");
        }
    }

    private async Task MarkCurrentOnboardingStepAsync()
    {
        if (!_settingsLoaded || OnboardingSteps.Count == 0)
            return;

        await MarkOnboardingStepCompleteAsync(OnboardingSteps[_onboardingStepIndex].Id);
    }

    private async Task MarkOnboardingStepCompleteAsync(string stepId)
    {
        var completed = (_settings.OnboardingCompletedSteps ?? Array.Empty<string>())
            .ToHashSet(StringComparer.Ordinal);
        if (!completed.Add(stepId))
        {
            RefreshOnboarding();
            return;
        }

        var wasDirty = _settingsDirty;
        try
        {
            _settings = _settings with { OnboardingCompletedSteps = completed.ToArray() };
            await SaveSettingsAsync();
            if (wasDirty)
                MarkSettingsDirty();
            else
                MarkSettingsSaved();
            RefreshOnboarding();
        }
        catch (Exception exception)
        {
            _settingsSaveStatus.Text = L($"Onboarding progress could not be saved: {exception.Message}");
            _settingsSaveStatus.Foreground = XRatioPalette.Danger;
            AddActivity($"Onboarding progress error: {exception.Message}", ActivityLevel.Error, "Settings");
        }
    }

    private async Task MoveToNextOnboardingStepAsync()
    {
        if (OnboardingSteps.Count == 0)
            return;

        if (_onboardingStepIndex == OnboardingSteps.Count - 1)
        {
            if (IsOnboardingStepComplete(OnboardingSteps[_onboardingStepIndex]))
                await DismissOnboardingAsync();
            else
            {
                _onboardingStepStatus.Text = L("Complete this step or use × to close.");
                _onboardingStepStatus.Foreground = XRatioPalette.Warning;
            }
            return;
        }

        _onboardingStepIndex++;
        RefreshOnboarding();
    }

    private async Task RunOnboardingActionAsync()
    {
        if (OnboardingSteps.Count == 0)
            return;

        switch (OnboardingSteps[_onboardingStepIndex].Action)
        {
            case OnboardingAction.OpenGuide:
                if (OnboardingSteps[_onboardingStepIndex].Id == OnboardingStepIds.Qbittorrent &&
                    TryOpenDetectedTorrentClient())
                    break;
                _onboardingOverlay.IsVisible = false;
                _tabs.SelectedIndex = 0;
                await ShowGuideAsync(_tabs);
                if (!_settings.OnboardingDismissed)
                    ShowOnboarding();
                break;
            case OnboardingAction.OpenPlatform:
                SelectTabAndReveal(5, _certificateStatus);
                break;
            case OnboardingAction.OpenInterception:
                SelectTabAndReveal(1, _torrents);
                ShowInterceptionOnboardingCoachmark();
                RefreshOnboarding();
                break;
            case OnboardingAction.OpenSimulation:
                SelectTabAndReveal(2, _torrentPath);
                ShowSimulationOnboardingCoachmark();
                RefreshOnboarding();
                break;
        }
    }

    private void SelectTabAndReveal(int tabIndex, Control target)
    {
        _tabs.SelectedIndex = tabIndex;
        target.BringIntoView();

        // Selection changes the content presenter synchronously, but the
        // scrollable layout may only know its final bounds on the next pass.
        // Bring the target into view again after that pass so long sections
        // (notably HTTPS in Platform) land on the relevant control reliably.
        Dispatcher.UIThread.Post(
            () =>
            {
                target.BringIntoView();
                var scroller = tabIndex switch
                {
                    4 => _settingsScroller,
                    5 => _platformScroller,
                    _ => null
                };
                if (scroller?.Content is Visual scrollContent &&
                    target.TranslatePoint(new Point(0, 0), scrollContent) is { } targetPosition)
                {
                    scroller.Offset = new Vector(
                        scroller.Offset.X,
                        Math.Max(0, targetPosition.Y - 120));
                }
                target.Focus();
            },
            DispatcherPriority.Loaded);
        RefreshOnboarding();
    }

    private void RefreshTorrentClientDetection()
    {
        _detectedTorrentClient = TorrentClientDetector.Find();
        _torrentClientDetectionComplete = true;
        RefreshOnboarding();
    }

    private bool TryOpenDetectedTorrentClient()
    {
        if (!_torrentClientDetectionComplete)
            RefreshTorrentClientDetection();

        if (_detectedTorrentClient is null)
            return false;

        try
        {
            if (TorrentClientDetector.TryOpen(_detectedTorrentClient))
            {
                AddActivity("qBittorrent opened from onboarding.", ActivityLevel.Success, "Onboarding");
                return true;
            }
        }
        catch (Exception exception) when (!_exiting)
        {
            AddActivity($"Could not open qBittorrent: {exception.Message}", ActivityLevel.Warning, "Onboarding");
        }

        AddActivity("Could not open qBittorrent. Use the qBittorrent guide instead.", ActivityLevel.Warning, "Onboarding");
        return false;
    }

    private void UpdateOnboardingSettingsStatus()
    {
        if (!_onboardingBuilt)
            return;

        if (!_settingsLoaded)
        {
            _onboardingSettingsStatus.Text = L("Loading onboarding…");
            _onboardingSettingsStatus.Foreground = XRatioPalette.Muted;
            _restoreOnboarding.IsEnabled = false;
            return;
        }

        _onboardingSettingsStatus.Text = L(_settings.OnboardingDismissed
            ? "Onboarding is hidden. You can show it again whenever you need it."
            : "Onboarding is available from the first run and stays here until you close it.");
        _onboardingSettingsStatus.Foreground = XRatioPalette.Muted;
        _restoreOnboarding.IsEnabled = true;
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
        "Configure qBittorrent or another torrent client",
        "Route tracker announces through the local XRatio HTTP proxy before checking the ratio.",
        [
            "Start XRatio and verify that the header shows HTTP/HTTPS active on 127.0.0.1:3773.",
            "In qBittorrent, open Tools > Options > Connection.",
            "Under Proxy Server, choose HTTP, set Host to 127.0.0.1 and Port to 3773.",
            "Enable Perform hostname lookup via proxy and Use proxy for BitTorrent purposes. Leave Use proxy for peer connections disabled because XRatio handles tracker announces only.",
            "For Deluge, Transmission, Tixati, BiglyBT, Vuze or another client, open Settings/Preferences and find Connection, Network or Proxy. Use HTTP, server 127.0.0.1 and port 3773. Enable tracker/BitTorrent proxying and leave peer connections disabled when that option is separate.",
            "In XRatio Settings > Announce behavior, download reporting is kept at zero. Use Pause or Stop when you need to suspend announce rewriting; Pretend to seed remains optional.",
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
            // Clip nested surfaces to the card radius so the upper corners
            // keep the same restrained geometry as the outer border.
            ClipToBounds = true,
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
            ClipToBounds = true,
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
        var onboarding = BuildOverviewOnboardingCard();
        var content = new Grid
        {
            MaxWidth = 980,
            HorizontalAlignment = HorizontalAlignment.Left,
            RowDefinitions = new RowDefinitions("Auto,Auto,Auto,Auto"),
            ColumnDefinitions = new ColumnDefinitions("1.45*,1*"),
            ColumnSpacing = 16,
            RowSpacing = 14,
            Children =
            {
                Place(_startupFailureBanner, column: 0),
                Place(runtime, row: 1, column: 0),
                Place(modes, row: 1, column: 1),
                Place(trust, row: 2, column: 0),
                Place(onboarding, row: 3, column: 0)
            }
        };
        Grid.SetColumnSpan(_startupFailureBanner, 2);
        Grid.SetColumnSpan(trust, 2);
        Grid.SetColumnSpan(onboarding, 2);
        UpdateOverviewMetrics();
        _overviewScroller.Tag = "OverviewScroll";
        _overviewScroller.Content = content;
        _overviewScroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        _overviewScroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        return BuildTabLayout("Overview", "Current runtime status.", _overviewScroller);
    }

    private Control BuildOverviewOnboardingCard()
    {
        _overviewOnboardingCounter.FontSize = 10.5;
        _overviewOnboardingCounter.FontWeight = FontWeight.SemiBold;
        _overviewOnboardingCounter.Foreground = XRatioPalette.Subtle;
        // Keep the step counter and completion check on the same visual
        // baseline instead of letting the larger check glyph float upward.
        _overviewOnboardingCounter.VerticalAlignment = VerticalAlignment.Bottom;
        _overviewOnboardingProgress.FontSize = 10.5;
        _overviewOnboardingProgress.FontWeight = FontWeight.SemiBold;
        _overviewOnboardingProgress.Foreground = XRatioPalette.Accent;
        _overviewOnboardingProgress.VerticalAlignment = VerticalAlignment.Center;

        _overviewOnboardingStatusIcon.FontSize = 14;
        _overviewOnboardingStatusIcon.FontWeight = FontWeight.Bold;
        _overviewOnboardingStatusIcon.VerticalAlignment = VerticalAlignment.Bottom;

        _overviewOnboardingTitle.FontSize = 18;
        _overviewOnboardingTitle.FontWeight = FontWeight.SemiBold;
        _overviewOnboardingTitle.Foreground = XRatioPalette.Ink;
        _overviewOnboardingTitle.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _overviewOnboardingDescription.FontSize = 13.5;
        _overviewOnboardingDescription.LineHeight = 20;
        _overviewOnboardingDescription.Foreground = XRatioPalette.Muted;
        _overviewOnboardingDescription.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _overviewOnboardingDetail.FontSize = 13;
        _overviewOnboardingDetail.FontWeight = FontWeight.SemiBold;
        _overviewOnboardingDetail.Foreground = XRatioPalette.Ink;
        _overviewOnboardingDetail.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _overviewOnboardingDots.Orientation = Orientation.Horizontal;
        _overviewOnboardingDots.Spacing = 5;
        _overviewOnboardingDots.HorizontalAlignment = HorizontalAlignment.Center;
        _overviewOnboardingDots.VerticalAlignment = VerticalAlignment.Center;
        _overviewOnboardingDots.IsVisible = false;
        for (var index = 0; index < OnboardingSteps.Count; index++)
        {
            _overviewOnboardingDots.Children.Add(new Border
            {
                Tag = index,
                Width = 5,
                Height = 5,
                CornerRadius = new CornerRadius(3),
                Background = XRatioPalette.Border,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        _overviewOnboardingAction.Tag = "OverviewOnboardingAction";
        StyleButton(_overviewOnboardingAction, ButtonTone.Primary, 0);
        _overviewOnboardingAction.MinHeight = 36;
        _overviewOnboardingAction.HorizontalAlignment = HorizontalAlignment.Right;
        _overviewOnboardingAction.HorizontalContentAlignment = HorizontalAlignment.Center;
        _overviewOnboardingAction.Padding = new Thickness(14, 4);
        _overviewOnboardingAction.CornerRadius = new CornerRadius(18);
        _overviewOnboardingAction.IsVisible = false;
        _overviewOnboardingAction.Click += async (_, _) => await RunOnboardingActionAsync();

        _overviewOnboardingDone.Tag = "OverviewOnboardingDone";
        _overviewOnboardingDone.Content = "Mark as configured";
        // This is the primary acknowledgement in the qBittorrent step. Keep
        // it visually distinct from the quiet pager controls so it is obvious
        // what the user should press after applying the proxy settings.
        StyleButton(_overviewOnboardingDone, ButtonTone.Primary, 164);
        _overviewOnboardingDone.MinWidth = 164;
        _overviewOnboardingDone.MinHeight = 40;
        _overviewOnboardingDone.Padding = new Thickness(16, 8);
        _overviewOnboardingDone.CornerRadius = new CornerRadius(9);
        _overviewOnboardingDone.Click += async (_, _) => await MarkCurrentOnboardingStepAsync();

        _overviewOnboardingPrevious.Tag = "OverviewOnboardingPrevious";
        _overviewOnboardingPrevious.Content = "←";
        StyleButton(_overviewOnboardingPrevious, ButtonTone.Quiet, 30);
        _overviewOnboardingPrevious.Width = 30;
        _overviewOnboardingPrevious.MinWidth = 30;
        _overviewOnboardingPrevious.Height = 36;
        _overviewOnboardingPrevious.MinHeight = 36;
        _overviewOnboardingPrevious.Padding = new Thickness(0);
        _overviewOnboardingPrevious.CornerRadius = new CornerRadius(18);
        _overviewOnboardingPrevious.Click += (_, _) =>
        {
            if (_onboardingStepIndex <= 0)
                return;
            _onboardingStepIndex--;
            RefreshOnboarding();
        };

        _overviewOnboardingNext.Tag = "OverviewOnboardingNext";
        _overviewOnboardingNext.Content = "→";
        StyleButton(_overviewOnboardingNext, ButtonTone.Quiet, 30);
        _overviewOnboardingNext.Width = 30;
        _overviewOnboardingNext.MinWidth = 30;
        _overviewOnboardingNext.Height = 36;
        _overviewOnboardingNext.MinHeight = 36;
        _overviewOnboardingNext.Padding = new Thickness(0);
        _overviewOnboardingNext.CornerRadius = new CornerRadius(18);
        _overviewOnboardingNext.Click += async (_, _) => await MoveToNextOnboardingStepAsync();

        _overviewOnboardingClose.Tag = "OverviewOnboardingClose";
        _overviewOnboardingClose.Content = BuildCloseGlyph(CloseGlyphSize);
        StyleButton(_overviewOnboardingClose, ButtonTone.Quiet, 24);
        // Keep a full 36px target so the close action is easy to hit without
        // changing the title/progress alignment around it.
        _overviewOnboardingClose.Width = CloseButtonSize;
        _overviewOnboardingClose.MinWidth = CloseButtonSize;
        _overviewOnboardingClose.Height = CloseButtonSize;
        _overviewOnboardingClose.MinHeight = CloseButtonSize;
        _overviewOnboardingClose.Padding = new Thickness(0);
        _overviewOnboardingClose.FontSize = 15;
        _overviewOnboardingClose.FontWeight = FontWeight.Normal;
        _overviewOnboardingClose.CornerRadius = new CornerRadius(CloseButtonSize / 2);
        _overviewOnboardingClose.Background = Brushes.Transparent;
        _overviewOnboardingClose.BorderBrush = Brushes.Transparent;
        _overviewOnboardingClose.BorderThickness = new Thickness(0);
        _overviewOnboardingClose.Foreground = XRatioPalette.Subtle;
        _overviewOnboardingClose.Opacity = 0.78;
        _overviewOnboardingClose.HorizontalContentAlignment = HorizontalAlignment.Center;
        _overviewOnboardingClose.VerticalContentAlignment = VerticalAlignment.Center;
        _overviewOnboardingClose.HorizontalAlignment = HorizontalAlignment.Center;
        _overviewOnboardingClose.VerticalAlignment = VerticalAlignment.Center;
        _overviewOnboardingClose.PointerEntered += (_, _) =>
            SetCloseButtonHoverState(_overviewOnboardingClose, hovered: true);
        _overviewOnboardingClose.PointerExited += (_, _) =>
            SetCloseButtonHoverState(_overviewOnboardingClose, hovered: false);
        _overviewOnboardingClose.GotFocus += (_, _) =>
            SetCloseButtonHoverState(_overviewOnboardingClose, hovered: true);
        _overviewOnboardingClose.LostFocus += (_, _) =>
            SetCloseButtonHoverState(_overviewOnboardingClose, hovered: false);
        _overviewOnboardingClose.Template = new FuncControlTemplate<Button>((button, _) => new Border
        {
            [!Border.BackgroundProperty] = button[!Button.BackgroundProperty],
            [!Border.BorderBrushProperty] = button[!Button.BorderBrushProperty],
            [!Border.BorderThicknessProperty] = button[!Button.BorderThicknessProperty],
            [!Border.CornerRadiusProperty] = button[!Button.CornerRadiusProperty],
            Child = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = button[!Button.ContentProperty],
                [!ContentPresenter.HorizontalContentAlignmentProperty] =
                    button[!Button.HorizontalContentAlignmentProperty],
                [!ContentPresenter.VerticalContentAlignmentProperty] =
                    button[!Button.VerticalContentAlignmentProperty]
            }
        });
        _overviewOnboardingClose.Classes.Add("onboarding-overview-close");
        _overviewOnboardingClose.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("onboarding-overview-close").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.NeutralSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.NavBorder),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Ink),
                new Setter(Button.OpacityProperty, 1d),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(12))
            }
        });
        _overviewOnboardingClose.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("onboarding-overview-close").Class(":pressed"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.NeutralSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.NavBorder),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Ink),
                new Setter(Button.OpacityProperty, 1d),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(12))
            }
        });
        _overviewOnboardingClose.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("onboarding-overview-close").Class(":focus-visible"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.NeutralSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.NavBorder),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Ink),
                new Setter(Button.OpacityProperty, 1d),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(12))
            }
        });
        ToolTip.SetTip(_overviewOnboardingClose, L("Close onboarding"));
        _overviewOnboardingClose.Click += async (_, _) => await DismissOnboardingAsync();

        var detail = new Border
        {
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0, 2, 0, 4),
            Child = _overviewOnboardingDetail
        };

        using (var imageStream = AssetLoader.Open(
                   new Uri("avares://XRatio/Assets/qbittorrent-proxy-settings.png")))
        {
            var screenshot = new Bitmap(imageStream);
            _overviewTorrentClientScreenshot.Tag = "OnboardingQbittorrentScreenshot";
            _overviewTorrentClientScreenshot.Background = XRatioPalette.MetricSurface;
            _overviewTorrentClientScreenshot.BorderBrush = XRatioPalette.Border;
            _overviewTorrentClientScreenshot.BorderThickness = new Thickness(1);
            _overviewTorrentClientScreenshot.CornerRadius = new CornerRadius(10);
            _overviewTorrentClientScreenshot.Padding = new Thickness(10);
            _overviewTorrentClientScreenshot.ClipToBounds = true;
            _overviewTorrentClientScreenshot.HorizontalAlignment = HorizontalAlignment.Center;
            _overviewTorrentClientScreenshot.MaxWidth = 560;
            _overviewTorrentClientScreenshot.Child = new StackPanel
            {
                Spacing = 8,
                Children =
                {
                    _overviewTorrentClientScreenshotTitle,
                    new Image
                    {
                        // Keep the complete Proxy Server group in frame. The
                        // previous 600px crop stopped before its right edge,
                        // which made the port row look truncated in the guide.
                        Source = new CroppedBitmap(screenshot, new PixelRect(176, 328, 809, 319)),
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        MaxWidth = 540,
                        MaxHeight = 230
                    }
                }
            };
        }

        _overviewTorrentClientScreenshotTitle.FontSize = 9.5;
        _overviewTorrentClientScreenshotTitle.FontWeight = FontWeight.Bold;
        _overviewTorrentClientScreenshotTitle.Foreground = XRatioPalette.Accent;
        _overviewTorrentClientScreenshotTitle.LetterSpacing = 0.8;

        _overviewOtherTorrentClientsDescription.FontSize = 12.5;
        _overviewOtherTorrentClientsDescription.LineHeight = 18;
        _overviewOtherTorrentClientsDescription.Foreground = XRatioPalette.Muted;
        _overviewOtherTorrentClientsDescription.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _overviewOtherTorrentClientsHint.Tag = "OnboardingOtherTorrentClients";
        _overviewOtherTorrentClientsHint.Background = Brushes.Transparent;
        _overviewOtherTorrentClientsHint.BorderBrush = XRatioPalette.Border;
        _overviewOtherTorrentClientsHint.BorderThickness = new Thickness(0, 1, 0, 0);
        _overviewOtherTorrentClientsHint.Padding = new Thickness(0, 9, 0, 0);
        _overviewOtherTorrentClientsHint.Child = new StackPanel
        {
            Spacing = 3,
            Children =
                {
                _overviewOtherTorrentClientsTitle,
                _overviewOtherTorrentClientsDescription
            }
        };
        _overviewOtherTorrentClientsTitle.FontSize = 9.5;
        _overviewOtherTorrentClientsTitle.FontWeight = FontWeight.Bold;
        _overviewOtherTorrentClientsTitle.Foreground = XRatioPalette.Subtle;
        _overviewOtherTorrentClientsTitle.LetterSpacing = 0.8;

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
            ColumnSpacing = 8,
            Children =
            {
                _overviewOnboardingDots,
                Place(_overviewOnboardingDone, column: 2),
                Place(_overviewOnboardingAction, column: 3)
            }
        };

        var footer = new Grid
        {
            IsVisible = false,
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 6,
            Children =
            {
                _overviewOnboardingPrevious,
                Place(_overviewOnboardingNext, column: 2)
            }
        };

        var stepCard = new Border
        {
            Tag = "OverviewOnboardingStepPanel",
            MaxWidth = 600,
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(18),
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
                        ColumnSpacing = 8,
                        Children =
                        {
                            _overviewOnboardingCounter,
                            Place(_overviewOnboardingStatusIcon, column: 2)
                        }
                    },
                    new StackPanel
                    {
                        Spacing = 4,
                        Children = { _overviewOnboardingTitle, _overviewOnboardingDescription }
                    },
                    detail,
                    _overviewTorrentClientScreenshot,
                    _overviewOtherTorrentClientsHint,
                    actions,
                    footer
                }
            }
        };

        var taskRows = BuildOnboardingSidebarCapsules();
        _overviewOnboardingCard.Tag = "OverviewOnboardingCard";
        _overviewOnboardingCard.Background = Brushes.Transparent;
        _overviewOnboardingCard.BorderBrush = XRatioPalette.Border;
        _overviewOnboardingCard.BorderThickness = new Thickness(0, 1, 0, 1);
        _overviewOnboardingCard.CornerRadius = new CornerRadius(0);
        _overviewOnboardingCard.Padding = new Thickness(0, 12, 0, 14);
        _overviewOnboardingCard.HorizontalAlignment = HorizontalAlignment.Stretch;
        _overviewOnboardingCard.Child = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            RowSpacing = 10,
            Children =
            {
                new Grid
                {
                    // Keep the fixed close column the same width as the
                    // resting button so the X stays aligned with the
                    // header's right edge before and during hover.
                    ColumnDefinitions = new ColumnDefinitions($"*,Auto,{CloseButtonSize}"),
                    ColumnSpacing = 6,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "GET STARTED",
                            FontSize = 9,
                            FontWeight = FontWeight.Bold,
                            Foreground = XRatioPalette.Accent,
                            LetterSpacing = 1.15,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        Place(_overviewOnboardingProgress, column: 1),
                        Place(_overviewOnboardingClose, column: 2)
                    }
                },
                Place(
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("0.72*,1.28*"),
                        ColumnSpacing = 14,
                        Children =
                        {
                            taskRows,
                            Place(stepCard, column: 1)
                        }
                    },
                    row: 1)
            }
        };
        return _overviewOnboardingCard;
    }

    // Overview treatment follows the assistant-ui composition: a small focused
    // card inside an intentionally quiet stage. The three slim layers behind it
    // borrow the banner-stacking rhythm without turning the onboarding into a
    // second dashboard.
    private Control BuildOverviewOnboardingCardReference()
    {
        _overviewOnboardingCounter.FontSize = 10.5;
        _overviewOnboardingCounter.FontWeight = FontWeight.SemiBold;
        _overviewOnboardingCounter.Foreground = XRatioPalette.ReferenceMuted;
        _overviewOnboardingCounter.VerticalAlignment = VerticalAlignment.Bottom;
        _overviewOnboardingStatusIcon.FontSize = 13;
        _overviewOnboardingStatusIcon.FontWeight = FontWeight.Bold;
        _overviewOnboardingStatusIcon.VerticalAlignment = VerticalAlignment.Bottom;

        _overviewOnboardingTitle.FontSize = 15.5;
        _overviewOnboardingTitle.FontWeight = FontWeight.SemiBold;
        _overviewOnboardingTitle.Foreground = XRatioPalette.ReferenceText;
        _overviewOnboardingTitle.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _overviewOnboardingDescription.FontSize = 12;
        _overviewOnboardingDescription.Foreground = XRatioPalette.ReferenceMuted;
        _overviewOnboardingDescription.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _overviewOnboardingDetail.FontSize = 11.5;
        _overviewOnboardingDetail.Foreground = XRatioPalette.ReferenceMuted;
        _overviewOnboardingDetail.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _overviewOnboardingDots.Orientation = Orientation.Horizontal;
        _overviewOnboardingDots.Spacing = 5;
        _overviewOnboardingDots.HorizontalAlignment = HorizontalAlignment.Center;
        _overviewOnboardingDots.VerticalAlignment = VerticalAlignment.Center;
        _overviewOnboardingDots.Children.Clear();
        for (var index = 0; index < OnboardingSteps.Count; index++)
        {
            _overviewOnboardingDots.Children.Add(new Border
            {
                Tag = index,
                Width = 5,
                Height = 5,
                CornerRadius = new CornerRadius(3),
                Background = XRatioPalette.ReferenceBorder,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        _overviewOnboardingAction.Tag = "OverviewOnboardingAction";
        StyleButton(_overviewOnboardingAction, ButtonTone.Secondary, 0);
        _overviewOnboardingAction.MinHeight = 36;
        _overviewOnboardingAction.HorizontalAlignment = HorizontalAlignment.Stretch;
        _overviewOnboardingAction.HorizontalContentAlignment = HorizontalAlignment.Left;
        _overviewOnboardingAction.Padding = new Thickness(11, 6);
        _overviewOnboardingAction.CornerRadius = new CornerRadius(10);
        _overviewOnboardingAction.Background = XRatioPalette.ReferenceField;
        _overviewOnboardingAction.BorderBrush = XRatioPalette.ReferenceBorder;
        _overviewOnboardingAction.BorderThickness = new Thickness(1);
        _overviewOnboardingAction.Foreground = XRatioPalette.ReferenceText;
        _overviewOnboardingAction.Click += async (_, _) => await RunOnboardingActionAsync();

        _overviewOnboardingDone.Tag = "OverviewOnboardingDone";
        _overviewOnboardingDone.Content = "✓";
        StyleButton(_overviewOnboardingDone, ButtonTone.Quiet, 34);
        _overviewOnboardingDone.Width = 34;
        _overviewOnboardingDone.MinWidth = 34;
        _overviewOnboardingDone.Height = 34;
        _overviewOnboardingDone.MinHeight = 36;
        _overviewOnboardingDone.Padding = new Thickness(0);
        _overviewOnboardingDone.CornerRadius = new CornerRadius(17);
        _overviewOnboardingDone.Background = XRatioPalette.ReferenceField;
        _overviewOnboardingDone.BorderBrush = XRatioPalette.ReferenceBorder;
        _overviewOnboardingDone.BorderThickness = new Thickness(1);
        _overviewOnboardingDone.Foreground = XRatioPalette.ReferenceMuted;
        _overviewOnboardingDone.IsVisible = false;
        _overviewOnboardingDone.Click += async (_, _) => await MarkCurrentOnboardingStepAsync();

        _overviewOnboardingPrevious.Tag = "OverviewOnboardingPrevious";
        _overviewOnboardingPrevious.Content = "←";
        StyleButton(_overviewOnboardingPrevious, ButtonTone.Quiet, 30);
        _overviewOnboardingPrevious.Width = 30;
        _overviewOnboardingPrevious.MinWidth = 30;
        _overviewOnboardingPrevious.Height = 30;
        _overviewOnboardingPrevious.MinHeight = 36;
        _overviewOnboardingPrevious.Padding = new Thickness(0);
        _overviewOnboardingPrevious.CornerRadius = new CornerRadius(15);
        _overviewOnboardingPrevious.Background = XRatioPalette.ReferenceField;
        _overviewOnboardingPrevious.BorderBrush = XRatioPalette.ReferenceBorder;
        _overviewOnboardingPrevious.BorderThickness = new Thickness(1);
        _overviewOnboardingPrevious.Foreground = XRatioPalette.ReferenceText;
        _overviewOnboardingPrevious.Classes.Add("reference-pager");
        _overviewOnboardingPrevious.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("reference-pager").Class(":disabled"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.ReferenceField),
                new Setter(Button.BorderBrushProperty, XRatioPalette.ReferenceBorder),
                new Setter(Button.ForegroundProperty, XRatioPalette.ReferenceSubtle),
                new Setter(Button.OpacityProperty, 0.58)
            }
        });
        _overviewOnboardingPrevious.Click += (_, _) =>
        {
            if (_onboardingStepIndex <= 0)
                return;
            _onboardingStepIndex--;
            RefreshOnboarding();
        };

        _overviewOnboardingNext.Tag = "OverviewOnboardingNext";
        _overviewOnboardingNext.Content = "→";
        StyleButton(_overviewOnboardingNext, ButtonTone.Quiet, 30);
        _overviewOnboardingNext.Width = 30;
        _overviewOnboardingNext.MinWidth = 30;
        _overviewOnboardingNext.Height = 30;
        _overviewOnboardingNext.MinHeight = 36;
        _overviewOnboardingNext.Padding = new Thickness(0);
        _overviewOnboardingNext.CornerRadius = new CornerRadius(15);
        _overviewOnboardingNext.Background = XRatioPalette.ReferenceField;
        _overviewOnboardingNext.BorderBrush = XRatioPalette.ReferenceBorder;
        _overviewOnboardingNext.BorderThickness = new Thickness(1);
        _overviewOnboardingNext.Foreground = XRatioPalette.ReferenceText;
        _overviewOnboardingNext.Click += async (_, _) => await MoveToNextOnboardingStepAsync();

        var status = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { _overviewOnboardingStatusIcon }
        };

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 7,
            Children =
            {
                _overviewOnboardingAction,
                Place(_overviewOnboardingDone, column: 1)
            }
        };

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 8,
            Margin = new Thickness(0, 1, 0, 0),
            Children =
            {
                _overviewOnboardingPrevious,
                Place(_overviewOnboardingDots, column: 1),
                Place(_overviewOnboardingNext, column: 2)
            }
        };

        var card = new Border
        {
            Width = 384,
            MaxWidth = 384,
            Background = XRatioPalette.ReferenceSurface,
            BorderBrush = XRatioPalette.ReferenceBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(18),
            Padding = new Thickness(18),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new StackPanel
            {
                Spacing = 10,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                        ColumnSpacing = 8,
                        Children =
                        {
                            _overviewOnboardingCounter,
                            Place(status, column: 2)
                        }
                    },
                    _overviewOnboardingTitle,
                    _overviewOnboardingDescription,
                    actions,
                    footer
                }
            }
        };

        var stageContent = new Grid
        {
            ClipToBounds = true,
            Children =
            {
                BuildOnboardingBannerGhost(382, 0.16, -50),
                BuildOnboardingBannerGhost(398, 0.26, -26),
                BuildOnboardingBannerGhost(414, 0.40, -6),
                card
            }
        };

        return new Border
        {
            Tag = "OverviewOnboardingCard",
            Background = XRatioPalette.ReferenceCanvas,
            BorderBrush = XRatioPalette.ReferenceBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(16, 14),
            MinHeight = 252,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,*"),
                RowSpacing = 3,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "GET STARTED",
                                FontSize = 9,
                                FontWeight = FontWeight.Bold,
                                Foreground = XRatioPalette.ReferenceMuted,
                                LetterSpacing = 1.1,
                                VerticalAlignment = VerticalAlignment.Center
                            },
                            Place(
                                new TextBlock
                                {
                                    Text = "Onboarding",
                                    FontSize = 11,
                                    FontWeight = FontWeight.SemiBold,
                                    Foreground = XRatioPalette.ReferenceText,
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    VerticalAlignment = VerticalAlignment.Center
                                },
                                column: 1)
                        }
                    },
                    Place(stageContent, row: 1)
                }
            }
        };
    }

    private static Control BuildOnboardingBannerGhost(double width, double opacity, double verticalOffset)
    {
        var line = new Border
        {
            Width = Math.Max(120, width * 0.42),
            Height = 7,
            CornerRadius = new CornerRadius(4),
            Background = XRatioPalette.ReferenceBorder,
            Opacity = 0.72
        };
        var dot = new Border
        {
            Width = 13,
            Height = 13,
            CornerRadius = new CornerRadius(7),
            Background = XRatioPalette.ReferenceBorder,
            Opacity = 0.72,
            VerticalAlignment = VerticalAlignment.Center
        };
        return new Border
        {
            Width = width,
            Height = 34,
            Margin = new Thickness(0, verticalOffset, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brushes.Transparent,
            BorderBrush = XRatioPalette.ReferenceBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(17),
            Opacity = opacity,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 9,
                Margin = new Thickness(11, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Children = { dot, line }
            }
        };
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
            // The hero owns the card's first background layer. Round its top
            // corners too so that layer cannot square off the outer border.
            CornerRadius = new CornerRadius(5, 5, 0, 0),
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
        ConfigureTextBox(_simulationStopValue, "Duration");
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
        BuildSimulationTimerUnitSelector();
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
            _simulationStopHint,
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
            "Timer",
            "Uploaded MiB",
            "Downloaded MiB",
            "Ratio"
        };
        _simulationStopMode.SelectedIndex = 0;
        _simulationStopMode.SelectionChanged += (_, _) => UpdateSimulationStopEditor();
        UpdateSimulationStopEditor();

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
            Margin = new Thickness(0, 6, 0, 10),
            Children = { torrentFile, torrentInfo, speeds, options, safetyNote }
        };
        var advanced = new StackPanel
        {
            Spacing = 6,
            Margin = new Thickness(0, 6, 0, 10),
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
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Items =
            {
                new TabItem
                {
                    Header = "Main",
                    Content = BuildVerticalSimulationScroller(main),
                    Padding = new Thickness(12, 6, 12, 0)
                },
                new TabItem
                {
                    Header = "Advanced",
                    Content = BuildVerticalSimulationScroller(advanced),
                    Padding = new Thickness(12, 6, 12, 0)
                }
            }
        };

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
        var resizeSplitter = new Border
        {
            Tag = "SimulationSessionsResizeSplitter",
            Height = 6,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = Brushes.Transparent,
            CornerRadius = new CornerRadius(0),
            Cursor = new Cursor(StandardCursorType.SizeNorthSouth),
            ZIndex = 10
        };
        var simulationSessionsHeader = new Border
        {
            Tag = "SimulationSessionsHeader",
            Background = XRatioPalette.AccentSoft,
            BorderBrush = XRatioPalette.SectionBorder,
            BorderThickness = new Thickness(0, 0, 0, 1),
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(16, 0, 16, 0),
            Height = SimulationSessionsHeaderHeight,
            MinHeight = SimulationSessionsHeaderHeight,
            MaxHeight = SimulationSessionsHeaderHeight,
            ClipToBounds = true,
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8,
                VerticalAlignment = VerticalAlignment.Center,
                Children =
                {
                    new Border
                    {
                        Width = 4,
                        Height = 18,
                        CornerRadius = new CornerRadius(2),
                        Background = XRatioPalette.Accent,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    new TextBlock
                    {
                        Tag = "SimulationSessionsLabel",
                        Text = "Simulation sessions",
                        FontSize = 12.5,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = XRatioPalette.Ink,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                }
            }
        };
        var sessions = new Border
        {
            Tag = "SimulationSessionsListSurface",
            Background = XRatioPalette.Surface,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(0),
            Height = _simulationSessionsHeight,
            MinHeight = _simulationSessionsHeight,
            MaxHeight = _simulationSessionsHeight,
            VerticalAlignment = VerticalAlignment.Stretch,
            ClipToBounds = true,
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions($"{SimulationSessionsHeaderHeight},*"),
                RowSpacing = 0,
                ClipToBounds = true,
                Children =
                {
                    simulationSessionsHeader,
                    Place(new Border
                    {
                        Tag = "SimulationSessionsBody",
                        Background = XRatioPalette.Surface,
                        Padding = new Thickness(16, 12),
                        ClipToBounds = true,
                        Child = new Grid
                        {
                            ClipToBounds = true,
                            Children = { _simulations, _simulationsEmptyState }
                        }
                    }, row: 1)
                }
            }
        };
        var commandBar = new Grid
        {
            Tag = "SimulationCommandBar",
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 12,
            Margin = new Thickness(0),
            Children =
            {
                _simulationAddAction,
                Place(_simulationAddFeedback, column: 1),
                Place(_simulationActions, column: 2)
            }
        };
        var commandFooter = new Border
        {
            Tag = "SimulationSessionsCommandFooter",
            Background = XRatioPalette.MetricSurface,
            BorderBrush = XRatioPalette.SectionBorder,
            BorderThickness = new Thickness(0, 1, 0, 0),
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(14, 10, 14, 12),
            ClipToBounds = true,
            Child = commandBar
        };
        var sessionsRowDefinition = new RowDefinition(_simulationSessionsHeight, GridUnitType.Pixel);
        var simulationSessionsPanel = new Border
        {
            Tag = "SimulationSessionsSurface",
            Background = XRatioPalette.SurfaceRaised,
            BorderBrush = XRatioPalette.SectionBorder,
            BorderThickness = new Thickness(1, 1, 1, 0),
            CornerRadius = new CornerRadius(0),
            Margin = new Thickness(0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(0),
            ClipToBounds = true,
            Child = new Grid
            {
                RowDefinitions = [sessionsRowDefinition, new RowDefinition(GridLength.Auto)],
                RowSpacing = 0,
                Children =
                {
                    sessions,
                    Place(commandFooter, row: 1),
                    resizeSplitter
                }
            }
        };

        Point dragStartPoint = default;
        double dragStartHeight = 0;

        resizeSplitter.PointerPressed += (_, args) =>
        {
            if (args.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDraggingSimulationSessions = true;
                dragStartPoint = args.GetPosition(this);
                dragStartHeight = _simulationSessionsHeight;
                args.Pointer.Capture(resizeSplitter);
                args.Handled = true;
            }
        };

        resizeSplitter.PointerMoved += (_, args) =>
        {
            if (_isDraggingSimulationSessions)
            {
                var currentPoint = args.GetPosition(this);
                var deltaY = dragStartPoint.Y - currentPoint.Y;
                var minHeight = 90.0;
                var currentWindowHeight = Bounds.Height > 0 ? Bounds.Height : Height;
                var maxHeight = Math.Max(minHeight, currentWindowHeight - 240.0);
                var newHeight = Math.Clamp(dragStartHeight + deltaY, minHeight, maxHeight);

                _simulationSessionsHeight = newHeight;
                sessions.Height = newHeight;
                sessions.MinHeight = newHeight;
                sessions.MaxHeight = newHeight;
                sessionsRowDefinition.Height = new GridLength(newHeight, GridUnitType.Pixel);
                args.Handled = true;
            }
        };

        resizeSplitter.PointerReleased += (_, args) =>
        {
            if (_isDraggingSimulationSessions)
            {
                _isDraggingSimulationSessions = false;
                args.Pointer.Capture(null);
                args.Handled = true;
            }
        };

        resizeSplitter.PointerCaptureLost += (_, _) =>
        {
            _isDraggingSimulationSessions = false;
        };

        SizeChanged += (_, args) =>
        {
            var maxSessionsHeight = Math.Max(90.0, args.NewSize.Height - 240.0);
            if (_simulationSessionsHeight > maxSessionsHeight)
            {
                _simulationSessionsHeight = maxSessionsHeight;
                sessions.Height = maxSessionsHeight;
                sessions.MinHeight = maxSessionsHeight;
                sessions.MaxHeight = maxSessionsHeight;
                sessionsRowDefinition.Height = new GridLength(maxSessionsHeight, GridUnitType.Pixel);
            }
        };
        HookSimulationFormPersistence();
        return new Border
        {
            Background = Brushes.Transparent,
            Padding = new Thickness(16, 10, 16, 0),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("*,Auto"),
                RowSpacing = 0,
                Children =
                {
                    modeTabs,
                    Place(simulationSessionsPanel, row: 1)
                }
            }
        };
    }

    private async Task ChooseTorrentAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = L("Choose a torrent"),
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType(L("BitTorrent metadata")) { Patterns = ["*.torrent"] }
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
            input.TextChanged += (_, _) =>
            {
                if (input == _simulationCompleted && _settingsLoaded && !_restoringSimulationForm)
                    _simulationCompletedCustomized = true;
                QueueSimulationFormPersistence();
            };

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
            CompletedPercentCustomized = _simulationCompletedCustomized,
            ListeningPort = _simulationPort.Text ?? string.Empty,
            PeersRequested = _simulationNumWant.Text ?? string.Empty,
            AnnounceIntervalSeconds = _simulationAnnounceInterval.Text ?? string.Empty,
            StopMode = Math.Clamp(_simulationStopMode.SelectedIndex, 0, 4),
            StopValue = _simulationStopValue.Text ?? string.Empty,
            StopTimerUnit = _simulationTimerUnit,
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
            _simulationCompletedCustomized = ResolveSimulationCompletedPercentCustomized(settings);
            _simulationCompleted.Text = ResolveSimulationCompletedPercent(settings);
            _simulationPort.Text = settings.ListeningPort;
            _simulationNumWant.Text = settings.PeersRequested;
            _simulationAnnounceInterval.Text = settings.AnnounceIntervalSeconds;
            _simulationStopMode.SelectedIndex = Math.Clamp(settings.StopMode, 0, 4);
            _simulationStopValue.Text = settings.StopValue;
            SetSimulationTimerUnit(settings.StopTimerUnit, persist: false);
            UpdateSimulationStopEditor();
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

    internal static string ResolveSimulationCompletedPercent(SimulationFormSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var defaultValue = SimulationDefaults.InitialCompletedPercent.ToString(CultureInfo.InvariantCulture);
        return ResolveSimulationCompletedPercentCustomized(settings) &&
               !string.IsNullOrWhiteSpace(settings.CompletedPercent)
            ? settings.CompletedPercent
            : defaultValue;
    }

    private static bool ResolveSimulationCompletedPercentCustomized(SimulationFormSettings settings)
    {
        if (settings.CompletedPercentCustomized)
            return true;

        // Settings written before the default changed to 0 have no marker. Treat
        // their old 100% value (or an invalid/empty value) as the untouched default,
        // while preserving other valid values a user may have chosen.
        return !IsLegacySimulationCompletedPercent(settings.CompletedPercent);
    }

    private static bool IsLegacySimulationCompletedPercent(string? value)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ||
            !double.IsFinite(parsed) ||
            parsed is < 0 or > 100)
            return true;

        return parsed == 100;
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
                MaximumRuntime = stopMode == SimulationTimerStopMode
                    ? ResolveSimulationTimerDuration(stopValue!.Value, _simulationTimerUnit)
                    : null,
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
        RequestSimulationRowsRefresh(snapshot.Id);

    private void RequestSimulationRowsRefresh(Guid selectedId)
    {
        var schedule = false;
        lock (_simulationRefreshGate)
        {
            if (_exiting)
                return;

            // Several sessions publish on the same tick. Keep only the latest
            // selected id and rebuild the list once for the whole UI turn.
            _pendingSimulationRefreshId = selectedId;
            if (!_simulationRefreshScheduled)
            {
                _simulationRefreshScheduled = true;
                schedule = true;
            }
        }

        if (schedule)
            Dispatcher.UIThread.Post(DrainSimulationRowsRefresh, DispatcherPriority.Background);
    }

    private void DrainSimulationRowsRefresh()
    {
        Guid? selectedId;
        lock (_simulationRefreshGate)
        {
            selectedId = _pendingSimulationRefreshId;
            _pendingSimulationRefreshId = null;
            _simulationRefreshScheduled = false;
        }

        if (!_exiting && selectedId is { } id)
        {
            if (_tabs.SelectedIndex == 2)
                RefreshSimulationRows(id);
            else
                _simulationRowsRefreshPending = true;
        }

        var schedule = false;
        lock (_simulationRefreshGate)
        {
            if (!_exiting && _pendingSimulationRefreshId is not null && !_simulationRefreshScheduled)
            {
                _simulationRefreshScheduled = true;
                schedule = true;
            }
        }

        if (schedule)
            Dispatcher.UIThread.Post(DrainSimulationRowsRefresh, DispatcherPriority.Background);
    }

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

    private ListBoxItem BuildSimulationListItem(SimulationRow row)
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
        var (statusKey, statusForeground, statusBackground) = snapshot.State switch
        {
            SimulationState.Running => ("●  Running", XRatioPalette.Positive, XRatioPalette.PositiveSoft),
            SimulationState.Starting => ("▶  Starting", XRatioPalette.Accent, XRatioPalette.AccentSoft),
            SimulationState.Stopping => ("■  Stopping", XRatioPalette.Danger, XRatioPalette.DangerSoft),
            SimulationState.Faulted => ("!  Error", XRatioPalette.Danger, XRatioPalette.DangerSoft),
            _ => ("■  Stopped", XRatioPalette.Muted, XRatioPalette.NeutralSoft)
        };
        var statusText = L(statusKey);
        var seeders = string.Format(
            CultureInfo.InvariantCulture,
            L("{0} seeders"),
            snapshot.Seeders);
        var leechers = string.Format(
            CultureInfo.InvariantCulture,
            L("{0} leechers"),
            snapshot.Leechers);
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
                                 seeders,
                                 leechers,
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
                                     Text = string.Format(
                                         CultureInfo.InvariantCulture,
                                         L("Downloaded {0} of {1}"),
                                         FormatBytes(snapshot.Downloaded),
                                         FormatBytes(total)),
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
                        Text = "Torrent name · tracker · peers · status · transfer counters · last announce",
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

    private Control BuildInterceptionOnboardingCoachmark()
    {
        _interceptionCoachmarkTitle.Text = "How to use Interception";
        _interceptionCoachmarkTitle.FontSize = 16;
        _interceptionCoachmarkTitle.FontWeight = FontWeight.SemiBold;
        _interceptionCoachmarkTitle.Foreground = XRatioPalette.Ink;

        _interceptionCoachmarkSteps.Text =
            "1  Start or refresh a torrent in your client.\n" +
            "2  Its tracker announce appears in this list automatically.\n" +
            "3  Select a row to read the tracker, peers and transfer counters.\n" +
            "4  Right-click a row to copy its info hash or reset its statistics.";
        _interceptionCoachmarkSteps.FontSize = 12.5;
        _interceptionCoachmarkSteps.LineHeight = 21;
        _interceptionCoachmarkSteps.Foreground = XRatioPalette.Ink;
        _interceptionCoachmarkSteps.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _interceptionCoachmarkTroubleshooting.Text =
            "Nothing appears? Check that your torrent client uses XRatio’s HTTP proxy and that the header says Active.";
        _interceptionCoachmarkTroubleshooting.FontSize = 11.5;
        _interceptionCoachmarkTroubleshooting.Foreground = XRatioPalette.Muted;
        _interceptionCoachmarkTroubleshooting.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _interceptionCoachmarkClose.Tag = "InterceptionCoachmarkClose";
        _interceptionCoachmarkClose.Content = BuildCloseGlyph(CloseGlyphSize);
        _interceptionCoachmarkClose.Template = new FuncControlTemplate<Button>((button, _) => new ContentPresenter
        {
            Content = button.Content,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        });
        _interceptionCoachmarkClose.Width = CloseButtonSize;
        _interceptionCoachmarkClose.MinWidth = CloseButtonSize;
        _interceptionCoachmarkClose.Height = CloseButtonSize;
        _interceptionCoachmarkClose.MinHeight = CloseButtonSize;
        _interceptionCoachmarkClose.Padding = new Thickness(0);
        _interceptionCoachmarkClose.Background = Brushes.Transparent;
        _interceptionCoachmarkClose.BorderThickness = new Thickness(0);
        _interceptionCoachmarkClose.HorizontalAlignment = HorizontalAlignment.Right;
        _interceptionCoachmarkClose.VerticalAlignment = VerticalAlignment.Top;
        _interceptionCoachmarkClose.Click += (_, _) =>
            _interceptionOnboardingCoachmark.IsVisible = false;

        _interceptionCoachmarkDone.Tag = "InterceptionCoachmarkDone";
        _interceptionCoachmarkDone.Content = "Got it";
        StyleButton(_interceptionCoachmarkDone, ButtonTone.Primary, minWidth: 106);
        _interceptionCoachmarkDone.CornerRadius = new CornerRadius(18);
        _interceptionCoachmarkDone.HorizontalAlignment = HorizontalAlignment.Right;
        _interceptionCoachmarkDone.Click += async (_, _) =>
        {
            _interceptionOnboardingCoachmark.IsVisible = false;
            await MarkOnboardingStepCompleteAsync(OnboardingStepIds.Interception);
            _onboardingStepIndex = OnboardingSteps
                .Select((step, index) => (step, index))
                .First(item => item.step.Id == OnboardingStepIds.Simulation)
                .index;
            SelectTabAndReveal(2, _torrentPath);
            ShowSimulationOnboardingCoachmark();
            RefreshOnboarding();
        };

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 12,
            Children =
            {
                _interceptionCoachmarkTitle,
                Place(_interceptionCoachmarkClose, column: 1)
            }
        };

        _interceptionOnboardingCoachmark.Tag = "InterceptionOnboardingCoachmark";
        _interceptionOnboardingCoachmark.Width = 392;
        _interceptionOnboardingCoachmark.HorizontalAlignment = HorizontalAlignment.Right;
        _interceptionOnboardingCoachmark.VerticalAlignment = VerticalAlignment.Top;
        _interceptionOnboardingCoachmark.Margin = new Thickness(0, 86, 24, 0);
        _interceptionOnboardingCoachmark.Background = XRatioPalette.Surface;
        _interceptionOnboardingCoachmark.BorderBrush = XRatioPalette.Border;
        _interceptionOnboardingCoachmark.BorderThickness = new Thickness(1);
        _interceptionOnboardingCoachmark.CornerRadius = new CornerRadius(18);
        _interceptionOnboardingCoachmark.IsVisible = false;
        _interceptionOnboardingCoachmark.Padding = new Thickness(20, 18);
        _interceptionOnboardingCoachmark.Child = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                header,
                new Border
                {
                    BorderBrush = XRatioPalette.Border,
                    BorderThickness = new Thickness(0, 1, 0, 0)
                },
                _interceptionCoachmarkSteps,
                _interceptionCoachmarkTroubleshooting,
                _interceptionCoachmarkDone
            }
        };

        return _interceptionOnboardingCoachmark;
    }

    private void ShowInterceptionOnboardingCoachmark()
    {
        if (_settings.OnboardingDismissed)
            return;

        _interceptionCoachmarkTitle.Text = L("How to use Interception");
        _interceptionCoachmarkSteps.Text = string.Join(
            "\n",
            L("1  Start or refresh a torrent in your client."),
            L("2  Its tracker announce appears in this list automatically."),
            L("3  Select a row to read the tracker, peers and transfer counters."),
            L("4  Right-click a row to copy its info hash or reset its statistics."));
        _interceptionCoachmarkTroubleshooting.Text = string.Format(
            CultureInfo.InvariantCulture,
            L("Nothing appears? Check HTTP proxy 127.0.0.1:{0} in your client and that the header says Active."),
            _settings.ListenPort);
        _interceptionCoachmarkDone.Content = L("Got it");

        _interceptionOnboardingCoachmark.IsVisible = true;
    }

    private Control BuildSimulationOnboardingCoachmark()
    {
        _simulationCoachmarkTitle.Text = "How to use Simulation";
        _simulationCoachmarkTitle.FontSize = 16;
        _simulationCoachmarkTitle.FontWeight = FontWeight.SemiBold;
        _simulationCoachmarkTitle.Foreground = XRatioPalette.Ink;

        _simulationCoachmarkSteps.Text =
            "1  Choose a .torrent file and check the detected tracker.\n" +
            "2  Set the client profile, ratios and transfer speeds.\n" +
            "3  Click Add session, select it in the list, then press Start.\n" +
            "4  Use Manual update while it runs; press Stop when finished.";
        _simulationCoachmarkSteps.FontSize = 12.5;
        _simulationCoachmarkSteps.LineHeight = 21;
        _simulationCoachmarkSteps.Foreground = XRatioPalette.Ink;
        _simulationCoachmarkSteps.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _simulationCoachmarkTroubleshooting.Text =
            "Adding only saves the session. The tracker is contacted when you press Start.";
        _simulationCoachmarkTroubleshooting.FontSize = 11.5;
        _simulationCoachmarkTroubleshooting.Foreground = XRatioPalette.Muted;
        _simulationCoachmarkTroubleshooting.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _simulationCoachmarkClose.Tag = "SimulationCoachmarkClose";
        _simulationCoachmarkClose.Content = BuildCloseGlyph(CloseGlyphSize);
        _simulationCoachmarkClose.Template = new FuncControlTemplate<Button>((button, _) => new ContentPresenter
        {
            Content = button.Content,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        });
        _simulationCoachmarkClose.Width = CloseButtonSize;
        _simulationCoachmarkClose.MinWidth = CloseButtonSize;
        _simulationCoachmarkClose.Height = CloseButtonSize;
        _simulationCoachmarkClose.MinHeight = CloseButtonSize;
        _simulationCoachmarkClose.Padding = new Thickness(0);
        _simulationCoachmarkClose.Background = Brushes.Transparent;
        _simulationCoachmarkClose.BorderThickness = new Thickness(0);
        _simulationCoachmarkClose.HorizontalAlignment = HorizontalAlignment.Right;
        _simulationCoachmarkClose.VerticalAlignment = VerticalAlignment.Top;
        _simulationCoachmarkClose.Click += (_, _) =>
            _simulationOnboardingCoachmark.IsVisible = false;

        _simulationCoachmarkDone.Tag = "SimulationCoachmarkDone";
        _simulationCoachmarkDone.Content = "Got it";
        StyleButton(_simulationCoachmarkDone, ButtonTone.Primary, minWidth: 106);
        _simulationCoachmarkDone.CornerRadius = new CornerRadius(18);
        _simulationCoachmarkDone.HorizontalAlignment = HorizontalAlignment.Right;
        _simulationCoachmarkDone.Click += async (_, _) =>
        {
            _simulationOnboardingCoachmark.IsVisible = false;
            await MarkOnboardingStepCompleteAsync(OnboardingStepIds.Simulation);
        };

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 12,
            Children =
            {
                _simulationCoachmarkTitle,
                Place(_simulationCoachmarkClose, column: 1)
            }
        };

        _simulationOnboardingCoachmark.Tag = "SimulationOnboardingCoachmark";
        _simulationOnboardingCoachmark.Width = 392;
        _simulationOnboardingCoachmark.HorizontalAlignment = HorizontalAlignment.Right;
        _simulationOnboardingCoachmark.VerticalAlignment = VerticalAlignment.Top;
        _simulationOnboardingCoachmark.Margin = new Thickness(0, 86, 24, 0);
        _simulationOnboardingCoachmark.Padding = new Thickness(20, 18);
        _simulationOnboardingCoachmark.Background = XRatioPalette.Surface;
        _simulationOnboardingCoachmark.BorderBrush = XRatioPalette.Border;
        _simulationOnboardingCoachmark.BorderThickness = new Thickness(1);
        _simulationOnboardingCoachmark.CornerRadius = new CornerRadius(18);
        _simulationOnboardingCoachmark.IsVisible = false;
        _simulationOnboardingCoachmark.Child = new StackPanel
        {
            Spacing = 14,
            Children =
            {
                header,
                new Border
                {
                    BorderBrush = XRatioPalette.Border,
                    BorderThickness = new Thickness(0, 1, 0, 0)
                },
                _simulationCoachmarkSteps,
                _simulationCoachmarkTroubleshooting,
                _simulationCoachmarkDone
            }
        };

        return _simulationOnboardingCoachmark;
    }

    private void ShowSimulationOnboardingCoachmark()
    {
        if (_settings.OnboardingDismissed)
            return;

        _simulationCoachmarkTitle.Text = L("How to use Simulation");
        _simulationCoachmarkSteps.Text = string.Join(
            "\n",
            L("1  Choose a .torrent file and check the detected tracker."),
            L("2  Set the client profile, ratios and transfer speeds."),
            L("3  Click Add session, select it in the list, then press Start."),
            L("4  Use Manual update while it runs; press Stop when finished."));
        _simulationCoachmarkTroubleshooting.Text = L(
            "Adding only saves the session. The tracker is contacted when you press Start.");
        _simulationCoachmarkDone.Content = L("Got it");

        _simulationOnboardingCoachmark.IsVisible = true;
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
        _pretendSeed.Content = "Pretend to seed (completed torrents only)";
        ConfigureCheckBox(_onlyTrackers);
        ConfigureCheckBox(_onlyLocal);
        _onlyLocal.IsEnabled = false;
        ConfigureCheckBox(_proxyDebugLogging);
        ConfigureCheckBox(_noDownload);
        ConfigureCheckBox(_pretendSeed);
        // Download reporting is intentionally always enabled. Use Pause or
        // Stop to suspend announce rewriting instead of toggling this mode.
        _noDownload.IsChecked = true;
        _noDownload.IsEnabled = false;
        // Keep the initial surface aligned with the model defaults while the
        // persisted settings load asynchronously on window open.
        _pretendSeed.IsChecked = true;
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
            UpdateSimulationStopEditor();
            RefreshTorrents();
            RefreshSimulationRows();
            RefreshActivityLocalization();
            LanguageChanged?.Invoke(_language);
            MarkSettingsDirty();
        };
        HookSettingsDirtyState();
        HookRatioShapingWarning();

        StyleButton(_checkUpdates, ButtonTone.Secondary, minWidth: 190);
        _checkUpdates.Content = "Check for updates";
        _checkUpdates.Click += async (_, _) => await CheckForUpdatesAsync(startup: false);
        StyleButton(_downloadUpdate, ButtonTone.Primary, minWidth: 170);
        _downloadUpdate.Content = "Download update";
        _downloadUpdate.IsVisible = false;
        _downloadUpdate.Click += async (_, _) => await InstallLatestUpdateAsync();
        ConfigureUpdateIndicator(_updateIndicator);
        _updateIndicatorIcon.Data = StreamGeometry.Parse(
            "M11 2h2v9.17l3.59-3.58L18 9l-6 6-6-6 1.41-1.41L11 11.17V2zM4 19h16v2H4z");
        _updateIndicatorIcon.Width = 17;
        _updateIndicatorIcon.Height = 17;
        _updateIndicatorIcon.HorizontalAlignment = HorizontalAlignment.Center;
        _updateIndicatorIcon.VerticalAlignment = VerticalAlignment.Center;
        // The download glyph has a heavier baseline (the tray line), so lift
        // it by one pixel for optical centering in the compact action slot.
        _updateIndicatorIcon.Margin = new Thickness(0, -1, 0, 1);
        _updateIndicatorIcon.Foreground = XRatioPalette.Accent;
        _updateIndicatorIcon.Cursor = new Cursor(StandardCursorType.Arrow);
        _updateIndicatorLabel.Text = UiText.UpdateIndicatorLabel(UiText.English);
        _updateIndicatorLabel.FontSize = 11;
        _updateIndicatorLabel.FontWeight = FontWeight.SemiBold;
        _updateIndicatorLabel.Foreground = XRatioPalette.OnAccent;
        _updateIndicatorLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _updateIndicatorLabel.VerticalAlignment = VerticalAlignment.Center;
        _updateIndicatorLabel.IsVisible = false;
        _updateIndicatorLabel.TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
        _updateIndicatorLabel.TextTrimming = TextTrimming.CharacterEllipsis;
        _updateIndicatorLabel.Cursor = new Cursor(StandardCursorType.Arrow);
        var updateIndicatorContent = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Cursor = new Cursor(StandardCursorType.Arrow),
            Children =
            {
                _updateIndicatorIcon,
                _updateIndicatorLabel
            }
        };
        _updateIndicator.Content = updateIndicatorContent;
        _updateIndicator.PointerEntered += (_, _) =>
        {
            _updateIndicatorPointerOver = true;
            RefreshUpdateIndicatorState();
        };
        _updateIndicator.PointerExited += (_, _) =>
        {
            _updateIndicatorPointerOver = false;
            RefreshUpdateIndicatorState();
        };
        _updateIndicator.GotFocus += (_, _) =>
        {
            _updateIndicatorFocused = true;
            RefreshUpdateIndicatorState();
        };
        _updateIndicator.LostFocus += (_, _) =>
        {
            _updateIndicatorFocused = false;
            RefreshUpdateIndicatorState();
        };
        _updateIndicator.Tag = "UpdateAction";
        _updateIndicator.IsVisible = false;
        _updateIndicator.Click += async (_, _) => await InstallLatestUpdateAsync();
        _updateStatus.Text = "Not checked yet";
        _updateStatus.Foreground = XRatioPalette.Muted;
        _updateStatus.FontSize = 12;
        _updateStatus.VerticalAlignment = VerticalAlignment.Center;
        _updateStatus.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _checkUpdatesOnStartup.Tag = "CheckUpdatesOnStartup";
        _checkUpdatesOnStartup.Content = "Check for updates at startup";
        ConfigureCheckBox(_checkUpdatesOnStartup);
        _checkUpdatesOnStartup.IsChecked = true;

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
            "Check GitHub and install a verified Windows update automatically when one is available.",
            BuildSettingsBody(
                new Grid
                {
                    Width = 190,
                    ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                    ColumnSpacing = 18,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Current version",
                            FontSize = 13,
                            Foreground = XRatioPalette.Ink,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        Place(versionDisplay, column: 1)
                    }
                },
                new Grid
                {
                    Tag = "UpdateActions",
                    ColumnDefinitions = new ColumnDefinitions("Auto,Auto,*"),
                    ColumnSpacing = 12,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Children =
                    {
                        Place(_checkUpdates),
                        Place(_downloadUpdate, column: 1),
                        Place(_updateStatus, column: 2)
                    }
                },
                _checkUpdatesOnStartup),
            bottomPadding: 10);

        StyleButton(_restoreOnboarding, ButtonTone.Secondary, minWidth: 178);
        _restoreOnboarding.Tag = "RestoreOnboarding";
        _restoreOnboarding.Content = "Show onboarding again";
        _restoreOnboarding.Click += async (_, _) => await RestoreOnboardingAsync();
        _onboardingSettingsStatus.FontSize = 11.5;
        _onboardingSettingsStatus.Foreground = XRatioPalette.Muted;
        _onboardingSettingsStatus.VerticalAlignment = VerticalAlignment.Center;
        _onboardingSettingsStatus.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        var onboarding = BuildSettingsSection(
            "Onboarding",
            "Replay the guided setup at any time. Your completed steps stay checked.",
            BuildSettingsBody(
                new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*"),
                    ColumnSpacing = 14,
                    Children =
                    {
                        _restoreOnboarding,
                        Place(_onboardingSettingsStatus, column: 1)
                    }
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
            "Minimum values must not exceed maximum values. Changing these values affects tracker reporting; use Pause or Stop for temporary control.",
            BuildFieldGrid(
                ("Upload/download multiplier min", (Control)_downloadRatioMin),
                ("Upload/download multiplier max", (Control)_downloadRatioMax),
                ("Upload/upload multiplier min", (Control)_uploadRatioMin),
                ("Upload/upload multiplier max", (Control)_uploadRatioMax),
                ("Boost maximum (KiB/s)", (Control)_boost),
                ("Boost chance (%)", (Control)_boostChance)));

        var announce = BuildSettingsSection(
            "Announce behavior",
            "Download reporting stays at zero; use Pause or Stop to suspend announcements.",
            BuildToggleGroup(_noDownload, _pretendSeed));

        var resetSettings = BuildSettingsSection(
            "Reset to defaults",
            "Restores configurable settings to their defaults. Tracked torrents, statistics, onboarding progress and simulation sessions are preserved.",
            BuildSettingsBody(_settingsResetAction),
            bottomPadding: 10);
        resetSettings.Tag = "SettingsResetSection";

        var content = new StackPanel
        {
            Spacing = 14,
            Margin = new Thickness(28, 24, 28, 0),
            MaxWidth = 820,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children =
            {
                BuildTabHeading("Settings", "Tune the proxy while keeping safe defaults."),
                onboarding,
                appearance,
                connection,
                ratio,
                announce,
                resetSettings,
                updates
            }
        };
        _settingsSaveStatus.Text = "Loading settings…";
        _settingsSaveStatus.FontSize = 12;
        _settingsSaveStatus.Foreground = XRatioPalette.Muted;
        _settingsSaveStatus.VerticalAlignment = VerticalAlignment.Center;
        var actionBar = new Border
        {
            Tag = "SettingsActionBar",
            Background = XRatioPalette.Topbar,
            BorderBrush = XRatioPalette.NavBorder,
            BorderThickness = new Thickness(0, 1, 0, 0),
            CornerRadius = new CornerRadius(14, 0, 0, 0),
            ClipToBounds = true,
            Padding = new Thickness(28, 10),
            Child = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 12,
                Children = { _settingsSaveAction, _settingsSaveStatus }
            }
        };
        ApplySettingsTooltips();
        return new Grid
        {
            RowDefinitions = new RowDefinitions("*,Auto"),
            Children =
            {
                ConfigureSettingsScroller(content),
                Place(actionBar, row: 1)
            }
        };
    }

    private ScrollViewer ConfigureSettingsScroller(Control content)
    {
        _settingsScroller.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _settingsScroller.Content = content;
        return _settingsScroller;
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
                     _onlyTrackers, _proxyDebugLogging, _noDownload, _pretendSeed,
                     _checkUpdatesOnStartup
                 })
            checkBox.PropertyChanged += (_, args) =>
            {
                if (args.Property == ToggleButton.IsCheckedProperty)
                    MarkSettingsDirty();
            };
    }

    private void HookRatioShapingWarning()
    {
        foreach (var input in new[]
                 {
                     _downloadRatioMin, _downloadRatioMax, _uploadRatioMin,
                     _uploadRatioMax, _boost, _boostChance
                 })
            input.GotFocus += (_, _) => _ = ConfirmRatioShapingEditAsync();
    }

    private async Task ConfirmRatioShapingEditAsync()
    {
        if (!_settingsLoaded || _ratioShapingWarningAcknowledged || _ratioShapingWarningShowing)
            return;

        _ratioShapingWarningShowing = true;
        try
        {
            if (await ConfirmDangerousActionAsync(
                    "Change ratio shaping",
                    "These values change the upload/download data XRatio announces to trackers. Change them only for an authorized, understood purpose; use Pause or Stop for temporary control.",
                    "I understand"))
            {
                _ratioShapingWarningAcknowledged = true;
                return;
            }

            FocusManager?.Focus(null, NavigationMethod.Pointer, KeyModifiers.None);
        }
        catch (Exception exception)
        {
            FocusManager?.Focus(null, NavigationMethod.Pointer, KeyModifiers.None);
            AddActivity($"Ratio shaping warning could not be shown: {exception.Message}", ActivityLevel.Warning);
        }
        finally
        {
            _ratioShapingWarningShowing = false;
        }
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
            _updateIndicator.IsVisible = false;
            _latestReleaseUri = null;
            _latestDownloadUri = null;
            _latestUpdate = null;
            NotifyUpdateAvailabilityChanged();

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
                _latestUpdate = result;
                NotifyUpdateAvailabilityChanged();
                _latestReleaseUri = result.ReleaseUri;
                // Prefer the single-file Windows executable so the compact
                // action downloads exactly what users need to launch XRatio.
                _latestDownloadUri = result.ExecutableDownloadUri ?? result.DownloadUri;
                _updateStatus.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    L("Update available: {0}"),
                    $"v{result.LatestVersion}");
                _updateStatus.Foreground = XRatioPalette.Accent;
                _downloadUpdate.Content = L("Download update");
                _downloadUpdate.IsVisible = _latestDownloadUri is not null || _latestReleaseUri is not null;
                _downloadUpdate.IsEnabled = true;
                _updateIndicator.IsVisible = _downloadUpdate.IsVisible;
                _updateIndicator.IsEnabled = true;
                _updateIndicatorPointerOver = false;
                _updateIndicatorFocused = false;
                RefreshUpdateIndicatorState();
                if (_latestReleaseUri is not null)
                    ToolTip.SetTip(_updateStatus, _latestReleaseUri.ToString());

                var executablePath = UpdateInstaller.GetCurrentExecutablePath();
                if (result.ExecutableDownloadUri is not null &&
                    result.ExecutableChecksumUri is not null &&
                    UpdateInstaller.CanAutoUpdate(executablePath))
                {
                    await InstallLatestUpdateAsync();
                }
            }
            else
            {
                _latestUpdate = null;
                NotifyUpdateAvailabilityChanged();
                _updateStatus.Text = L("You are up to date");
                _updateStatus.Foreground = XRatioPalette.Positive;
                _downloadUpdate.IsVisible = false;
                _updateIndicator.IsVisible = false;
                ToolTip.SetTip(_updateStatus, null);
            }
        }
        catch (Exception) when (!_exiting)
        {
            _latestUpdate = null;
            NotifyUpdateAvailabilityChanged();
            _updateStatus.Text = L("Unable to check for updates");
            _updateStatus.Foreground = XRatioPalette.Warning;
            _downloadUpdate.IsVisible = false;
            _updateIndicator.IsVisible = false;
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

    private async Task InstallLatestUpdateAsync()
    {
        if (_updateInstallInProgress || _exiting ||
            _latestUpdate is not { IsUpdateAvailable: true } update)
            return;

        var executablePath = UpdateInstaller.GetCurrentExecutablePath();
        if (!UpdateInstaller.CanAutoUpdate(executablePath))
        {
            await OpenLatestReleaseAsync();
            return;
        }

        if (update.ExecutableDownloadUri is null || update.ExecutableChecksumUri is null)
        {
            _updateStatus.Text = L("Automatic update is unavailable for this release");
            _updateStatus.Foreground = XRatioPalette.Warning;
            return;
        }

        if (!await ConfirmDangerousActionAsync(
                "Install update",
                "This will download and install the verified Windows update, then restart XRatio.",
                "Install update"))
            return;

        _updateInstallInProgress = true;
        _checkUpdates.IsEnabled = false;
        _downloadUpdate.IsEnabled = false;
        _updateIndicator.IsEnabled = false;
        _updateStatus.Text = L("Downloading update…");
        _updateStatus.Foreground = XRatioPalette.Muted;
        ToolTip.SetTip(_updateStatus, null);
        try
        {
            await UpdateInstaller.DownloadAndLaunchUpdaterAsync(
                update,
                executablePath!,
                CancellationToken.None);
            _updateStatus.Text = L("Installing update…");
            _updateStatus.Foreground = XRatioPalette.Accent;
            await PrepareForExitAsync();
        }
        catch (Exception exception) when (!_exiting)
        {
            _updateStatus.Text = L("Automatic update failed");
            _updateStatus.Foreground = XRatioPalette.Warning;
            ToolTip.SetTip(_updateStatus, UiText.TranslateMessage(exception.Message, _language));
            _downloadUpdate.IsEnabled = true;
            _updateIndicator.IsEnabled = true;
        }
        finally
        {
            _updateInstallInProgress = false;
        }
    }

    private async Task OpenLatestReleaseAsync()
    {
        var uri = _latestDownloadUri ?? _latestReleaseUri;
        if (uri is null)
            return;

        if (!await ConfirmDangerousActionAsync(
                "Open update in browser",
                "This will open the verified update download in your default browser.",
                "Open browser",
                browserAction: true))
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

    private void RefreshUpdateIndicatorState()
    {
        var expanded = _updateIndicatorPointerOver || _updateIndicatorFocused;
        var expandedWidth = ResolveUpdateIndicatorExpandedWidth(_updateIndicatorLabel.Text);
        _updateIndicatorLabel.MaxWidth = expandedWidth - 45;
        _updateIndicator.Width = expanded ? expandedWidth : UpdateIndicatorCollapsedWidth;
        _updateIndicator.MinWidth = UpdateIndicatorCollapsedWidth;
        _updateIndicator.Padding = expanded
            ? new Thickness(10, 0, 12, 0)
            : new Thickness(0);
        _updateIndicator.Background = expanded
            ? XRatioPalette.Accent
            : Brushes.Transparent;
        _updateIndicator.BorderBrush = XRatioPalette.Accent;
        _updateIndicator.BorderThickness = expanded
            ? new Thickness(1)
            : new Thickness(2);
        _updateIndicator.CornerRadius = new CornerRadius(18);
        _updateIndicator.Foreground = expanded
            ? XRatioPalette.OnAccent
            : XRatioPalette.Accent;
        _updateIndicatorIcon.Foreground = _updateIndicator.Foreground;
        _updateIndicatorLabel.Foreground = _updateIndicator.Foreground;
        _updateIndicatorLabel.IsVisible = expanded;
    }

    internal static double ResolveUpdateIndicatorExpandedWidth(string? label)
    {
        var text = label?.Trim() ?? string.Empty;
        // Keep the compact action predictable before Avalonia has measured the
        // visual tree. The estimate is deliberately conservative for Segoe UI
        // and caps the pill so a long translation cannot squeeze the guide row.
        var estimatedTextWidth = Math.Max(32, text.Length * 6.6);
        var contentWidth = 17 + 6 + estimatedTextWidth + 22;
        return Math.Clamp(
            Math.Ceiling(contentWidth),
            UpdateIndicatorMinExpandedWidth,
            UpdateIndicatorMaxExpandedWidth);
    }

    private static void ConfigureUpdateIndicator(Button button)
    {
        button.Classes.Add("update-action");
        // Fluent's default Button template paints its own pointer-over layer,
        // which can replace the accent brush with the platform gray. Keep
        // this compact action on the same brush-driven template as the other
        // custom action controls so the hover color is deterministic.
        button.Template = new FuncControlTemplate<Button>((updateButton, _) => new Border
        {
            [!Border.BackgroundProperty] = updateButton[!Button.BackgroundProperty],
            [!Border.BorderBrushProperty] = updateButton[!Button.BorderBrushProperty],
            [!Border.BorderThicknessProperty] = updateButton[!Button.BorderThicknessProperty],
            [!Border.CornerRadiusProperty] = updateButton[!Button.CornerRadiusProperty],
            [!Border.PaddingProperty] = updateButton[!Button.PaddingProperty],
            Child = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = updateButton[!Button.ContentProperty],
                [!ContentPresenter.HorizontalContentAlignmentProperty] =
                    updateButton[!Button.HorizontalContentAlignmentProperty],
                [!ContentPresenter.VerticalContentAlignmentProperty] =
                    updateButton[!Button.VerticalContentAlignmentProperty]
            }
        });
        button.Background = Brushes.Transparent;
        button.BorderBrush = XRatioPalette.Accent;
        button.BorderThickness = new Thickness(2);
        button.CornerRadius = new CornerRadius(18);
        button.Foreground = XRatioPalette.Accent;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        button.VerticalContentAlignment = VerticalAlignment.Center;
        button.Width = UpdateIndicatorCollapsedWidth;
        button.MinWidth = UpdateIndicatorCollapsedWidth;
        button.Height = 44;
        button.MinHeight = 44;
        button.Margin = new Thickness(0, 2, 0, 2);
        button.Padding = new Thickness(0);
        button.Cursor = new Cursor(StandardCursorType.Arrow);
        button.HorizontalAlignment = HorizontalAlignment.Right;
        button.ClipToBounds = true;
        button.Transitions = new Transitions
        {
            new DoubleTransition
            {
                Property = Layoutable.WidthProperty,
                Duration = TimeSpan.FromMilliseconds(UpdateIndicatorTransitionMilliseconds),
                Easing = new CubicEaseOut()
            },
            new ThicknessTransition
            {
                Property = Button.PaddingProperty,
                Duration = TimeSpan.FromMilliseconds(UpdateIndicatorTransitionMilliseconds),
                Easing = new CubicEaseOut()
            },
            new BrushTransition
            {
                Property = Button.BackgroundProperty,
                Duration = TimeSpan.FromMilliseconds(UpdateIndicatorTransitionMilliseconds),
                Easing = new CubicEaseOut()
            },
            new BrushTransition
            {
                Property = Button.BorderBrushProperty,
                Duration = TimeSpan.FromMilliseconds(UpdateIndicatorTransitionMilliseconds),
                Easing = new CubicEaseOut()
            }
        };
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("update-action").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.Accent),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.OnAccent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("update-action").Class(":pressed"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.Accent),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.OnAccent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("update-action").Class(":focus-visible"))
        {
            Setters =
            {
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.BackgroundProperty, XRatioPalette.Accent),
                new Setter(Button.ForegroundProperty, XRatioPalette.OnAccent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
    }

    private Button BuildBugReportNavigationButton()
    {
        _bugReportButton.Tag = "BugReportAction";
        _bugReportButton.Content = new PathIcon
        {
            Tag = "BugReportIcon",
            Data = StreamGeometry.Parse(
                // Keep the inner contour wound in the opposite direction so
                // the circle remains hollow with Avalonia's non-zero fill.
                "M11 17h2v-2h-2v2zM12 0C18.627 0 24 5.373 24 12S18.627 24 12 24 0 18.627 0 12 5.373 0 12 0zM12 2C7.029 2 2 7.029 2 12s5.029 10 10 10 10-5.029 10-10S16.971 2 12 2zM11 7h2v6h-2V7z"),
            Width = 16,
            Height = 16,
            Foreground = XRatioPalette.Muted,
            VerticalAlignment = VerticalAlignment.Center
        };
        _bugReportButton.Background = Brushes.Transparent;
        _bugReportButton.BorderBrush = Brushes.Transparent;
        _bugReportButton.BorderThickness = new Thickness(0);
        _bugReportButton.CornerRadius = new CornerRadius(18);
        _bugReportButton.Width = 36;
        _bugReportButton.MinWidth = 36;
        _bugReportButton.Height = 44;
        _bugReportButton.MinHeight = 44;
        _bugReportButton.Padding = new Thickness(0);
        _bugReportButton.Margin = new Thickness(0, 2, 0, 2);
        _bugReportButton.VerticalAlignment = VerticalAlignment.Center;
        _bugReportButton.HorizontalContentAlignment = HorizontalAlignment.Center;
        _bugReportButton.VerticalContentAlignment = VerticalAlignment.Center;
        _bugReportButton.Foreground = XRatioPalette.Muted;
        _bugReportButton.Classes.Add("bug-report-nav-action");
        _bugReportButton.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("bug-report-nav-action").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.AccentSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Accent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        _bugReportButton.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("bug-report-nav-action").Class(":pressed"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.Accent),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.OnAccent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        _bugReportButton.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("bug-report-nav-action").Class(":focus-visible"))
        {
            Setters =
            {
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        AutomationProperties.SetName(_bugReportButton, L("Report a bug"));
        ToolTip.SetTip(_bugReportButton, L("Report a bug on GitHub"));
        _bugReportButton.Click += async (_, _) => await OpenBugReportAsync();
        return _bugReportButton;
    }

    private static Grid BuildGuideIcon() =>
        new()
        {
            Tag = "GuideIcon",
            Width = 16,
            Height = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                new PathIcon
                {
                    Tag = "GuideIconRing",
                    Data = StreamGeometry.Parse(
                        // Keep the inner contour wound in the opposite direction so
                        // the circle remains hollow with Avalonia's non-zero fill.
                        "M12 0C18.627 0 24 5.373 24 12S18.627 24 12 24 0 18.627 0 12 5.373 0 12 0zM12 2C7.029 2 2 7.029 2 12s5.029 10 10 10 10-5.029 10-10S16.971 2 12 2z"),
                    Width = 16,
                    Height = 16,
                    Foreground = XRatioPalette.Muted,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                new TextBlock
                {
                    Tag = "NavIcon",
                    Text = "?",
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 11,
                    FontWeight = FontWeight.SemiBold,
                    Width = 16,
                    Height = 16,
                    TextAlignment = TextAlignment.Center,
                    Foreground = XRatioPalette.Muted,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, -1, 0, 0)
                }
            }
        };

    private Button BuildGitHubNavigationButton()
    {
        var button = new Button
        {
            Tag = "GitHubAction",
            Content = new PathIcon
            {
                Data = StreamGeometry.Parse(
                    "M12 .297c-6.63 0-12 5.373-12 12 0 5.303 3.438 9.8 8.205 11.385.6.113.82-.258.82-.577 0-.285-.01-1.04-.015-2.04-3.338.724-4.042-1.61-4.042-1.61-.546-1.387-1.333-1.757-1.333-1.757-1.089-.745.084-.729.084-.729 1.205.084 1.84 1.236 1.84 1.236 1.07 1.835 2.809 1.305 3.495.998.108-.776.417-1.305.76-1.605-2.665-.3-5.466-1.332-5.466-5.93 0-1.31.465-2.38 1.235-3.22-.135-.303-.54-1.523.105-3.176 0 0 1.005-.322 3.3 1.23.96-.267 1.98-.399 3-.405 1.02.006 2.04.138 3 .405 2.28-1.552 3.285-1.23 3.285-1.23.645 1.653.24 2.873.12 3.176.765.84 1.23 1.91 1.23 3.22 0 4.61-2.805 5.625-5.475 5.92.42.36.81 1.096.81 2.21 0 1.595-.015 2.875-.015 3.265 0 .315.21.69.825.57C20.565 22.092 24 17.592 24 12.297c0-6.627-5.373-12-12-12z"),
                Width = 16,
                Height = 16,
                Foreground = XRatioPalette.Muted
            },
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(18),
            Width = 36,
            MinWidth = 36,
            Height = 44,
            MinHeight = 44,
            Padding = new Thickness(0),
            Margin = new Thickness(0, 2, 0, 2),
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            Foreground = XRatioPalette.Muted
        };
        button.Classes.Add("github-nav-action");
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("github-nav-action").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.AccentSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Accent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("github-nav-action").Class(":pressed"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.Accent),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.OnAccent),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        button.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("github-nav-action").Class(":focus-visible"))
        {
            Setters =
            {
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(18))
            }
        });
        ToolTip.SetTip(button, L("Open XRatio on GitHub"));
        button.Click += async (_, _) => await OpenRepositoryAsync();
        return button;
    }

    private async Task OpenRepositoryAsync()
    {
        if (!await ConfirmDangerousActionAsync(
                "Open GitHub in browser",
                "This will open the XRatio GitHub page in your default browser.",
                "Open browser",
                browserAction: true))
            return;

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

    private async Task OpenBugReportAsync()
    {
        if (!await ConfirmDangerousActionAsync(
                "Open bug report in browser",
                "This will open the GitHub issue form in your default browser.",
                "Open browser",
                browserAction: true))
            return;

        try
        {
            var launcher = TopLevel.GetTopLevel(this)?.Launcher;
            if (launcher is null || !await launcher.LaunchUriAsync(new Uri(BugReportUrl)))
                _updateStatus.Text = L("Could not open bug report");
        }
        catch (Exception) when (!_exiting)
        {
            _updateStatus.Text = L("Could not open bug report");
        }
    }

    private void MarkSettingsSaved()
    {
        _settingsDirty = false;
        _settingsSaveStatus.Text = L("All changes saved");
        _settingsSaveStatus.Foreground = XRatioPalette.Positive;
        _settingsSaveAction.IsEnabled = false;
        _settingsResetAction.IsEnabled = _settingsLoaded && !_settingsResetInProgress;
    }

    internal static XRatioSettings ResetConfigurableSettings(XRatioSettings current)
    {
        ArgumentNullException.ThrowIfNull(current);

        var defaults = new XRatioSettings();
        return defaults with
        {
            // Configuration reset must not erase data collected by the proxy
            // or the user's simulation/onboarding state. Only the editable
            // settings represented by the Settings and Platform tabs return
            // to their model defaults.
            OnboardingDismissed = current.OnboardingDismissed,
            OnboardingCompletedSteps = current.OnboardingCompletedSteps,
            LifetimeRuntimeSeconds = current.LifetimeRuntimeSeconds,
            LifetimeActualDownloaded = current.LifetimeActualDownloaded,
            LifetimeActualUploaded = current.LifetimeActualUploaded,
            LifetimeReportedDownloaded = current.LifetimeReportedDownloaded,
            LifetimeReportedUploaded = current.LifetimeReportedUploaded,
            Sessions = current.Sessions,
            SimulationForm = current.SimulationForm,
            PersistedTorrents = current.PersistedTorrents
        };
    }

    private async Task ResetSettingsAsync()
    {
        if (!_settingsLoaded || _settingsResetInProgress)
            return;

        _settingsResetInProgress = true;
        try
        {
            if (!await ConfirmDangerousActionAsync(
                    "Reset settings",
                    "Reset all configurable settings to their defaults? Tracked torrent statistics, onboarding progress and simulation sessions will be preserved.",
                    "Reset settings"))
                return;

            _settings = ResetConfigurableSettings(_settings);
            _language = UiText.Normalize(_settings.Language);
            ApplyTheme(_settings.ThemeMode, _settings.AccentColor);
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
            RefreshTorrents();
            RefreshSimulationRows();
            RefreshActivityLocalization();
            LanguageChanged?.Invoke(_language);
            UpdateSimulationStopEditor();
            MarkSettingsDirty();
            await SaveAndApplyAsync();
            if (!_settingsDirty)
                AddActivity("Configuration reset to defaults.");
        }
        catch (Exception exception)
        {
            AddActivity($"Configuration error: {exception.Message}");
        }
        finally
        {
            _settingsResetInProgress = false;
            _settingsResetAction.IsEnabled = _settingsLoaded;
        }
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
        foreach (var capabilityText in new[] { _autostartCapability, _certificateCapability })
        {
            capabilityText.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
            capabilityText.Foreground = XRatioPalette.Muted;
            capabilityText.FontSize = 12;
        }
        _autostartCapability.Text = $"Autostart: {_autostart.Capability.Description}";
        _certificateCapability.Text = $"Certificates: {_certificates.Capability.Description}";
        var startup = BuildSettingsSection(
            "Startup",
            "Choose how XRatio should behave when your session begins.",
            BuildSettingsBody(
                _autostartCapability,
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
                _certificateCapability,
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
        _platformScroller.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        _platformScroller.Content = content;
        return _platformScroller;
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
            // Keep the setting enabled even when an older profile persisted it
            // as false before this option became mandatory.
            _settings = SessionStatistics.StartSession(
                loadedSettings with { ReportDownloadAsZero = true });
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
            LanguageChanged?.Invoke(_language);
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
            {
                // The persisted preference is the default source of truth.
                // Keep a first-run checked state visible even before Windows
                // has a matching Run entry; an existing Run entry can still
                // surface as enabled when the preference was saved as false.
                _autoStart.IsChecked = _settings.AutoStart ||
                                       await _autostart.IsEnabledAsync();
            }
            await RefreshCertificateStatusAsync();
            await StartProxyAsync();
            RefreshTorrentClientDetection();
            _torrentNames.Refresh(force: true);
            RefreshTorrents();
            // Initialization can update bound controls asynchronously. Reassert
            // the clean baseline once the first runtime state is ready.
            MarkSettingsSaved();
            ShowOnboarding();
            if (_settings.CheckUpdatesOnStartup)
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
        ShowOnboarding();
    }

    private string GetRunningProxyStatusText() =>
        _certificateTrusted
            ? $"HTTP/HTTPS active on 127.0.0.1:{_proxy?.BoundPort ?? _settings.ListenPort}"
            : $"HTTP active on 127.0.0.1:{_proxy?.BoundPort ?? _settings.ListenPort}";

    public void TogglePause()
    {
        if (_proxy?.IsRunning != true)
            return;

        _paused = !_paused;
        _pause.Content = L(_paused ? "Resume" : "Pause");
        SetStatus(_paused
            ? $"Paused on 127.0.0.1:{_proxy?.BoundPort ?? _settings.ListenPort}"
            : _proxy?.IsRunning == true
                ? GetRunningProxyStatusText()
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
            _certificateTrusted = false;
            _certificateStatus.Text = L("Unavailable on this platform");
            _certificateStatus.Foreground = XRatioPalette.Muted;
            _certificateStatusDetail.Text = L("XRatio cannot install or inspect a user-scoped CA here.");
            _certificateConsent.IsVisible = false;
            _trustCertificate.IsVisible = false;
            _removeCertificate.IsVisible = false;
            if (!_settings.OnboardingDismissed)
                ShowOnboarding();
            return;
        }

        var trusted = await _certificates.IsTrustedAsync();
        _certificateTrusted = trusted;
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
        if (_proxy?.IsRunning == true && !_paused)
            SetStatus(GetRunningProxyStatusText());
        if (!_settings.OnboardingDismissed && !_onboardingOverlay.IsVisible)
            ShowOnboarding();
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
            ShowOnboarding();
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
            SetStatus(GetRunningProxyStatusText());
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
        SetStatus("Proxy stopped");
        UpdateOverviewMetrics();
        NotifyRuntimeStateChanged();
    }

    private void ShowStartupFailure(Exception exception)
    {
        var detail = DescribeStartupFailure(exception, _settings.ListenPort);
        _startupFailureDetail.Text = L(detail);
        _startupFailureBanner.IsVisible = true;
        SetStatus("Interception needs attention");
        _status.Foreground = XRatioPalette.Danger;
        _statusIndicator.Background = XRatioPalette.Danger;
        _toggle.Content = L("Retry");
        AddActivity($"Startup error: {detail}", ActivityLevel.Error, "Startup");
        UpdateOverviewMetrics();
        NotifyRuntimeStateChanged();
    }

    private void NotifyRuntimeStateChanged() =>
        RuntimeStateChanged?.Invoke(IsProxyRunning, IsProxyPaused);

    private void NotifyUpdateAvailabilityChanged() =>
        UpdateAvailabilityChanged?.Invoke(IsUpdateAvailable);

    private void ClearStartupFailure()
    {
        _startupFailureBanner.IsVisible = false;
        _status.Foreground = XRatioPalette.Muted;
        _statusIndicator.Background = XRatioPalette.Positive;
    }

    private void OnProxyActivity(object? sender, ProxyEvent activity)
    {
        if (_exiting)
            return;

        _pendingProxyActivities.Enqueue(activity);
        if (Interlocked.Exchange(ref _proxyActivityDrainScheduled, 1) == 0)
            Dispatcher.UIThread.Post(DrainProxyActivities, DispatcherPriority.Background);
    }

    private void DrainProxyActivities()
    {
        try
        {
            var processed = 0;
            while (processed < ProxyActivityBatchSize && _pendingProxyActivities.TryDequeue(out var activity))
            {
                var disposition = activity.Disposition.ToString();
                AddActivity(
                    $"{disposition}: {activity.Message}",
                    disposition.Contains("fail", StringComparison.OrdinalIgnoreCase)
                        ? ActivityLevel.Error
                        : ActivityLevel.Info,
                    "Proxy",
                    activity.Timestamp,
                    scrollIntoView: false);
                processed++;
            }

            if (processed > 0)
            {
                _activity.ScrollIntoView(_activity.ItemCount - 1);
                _torrentsRefreshPending = true;
                if (_tabs.SelectedIndex == 1)
                    RefreshTorrents();
                else if (_tabs.SelectedIndex == 0)
                    UpdateOverviewMetrics();
                else if (!_settings.OnboardingDismissed)
                    RefreshOnboarding();
                RequestTorrentStatePersistence();
            }
        }
        finally
        {
            Volatile.Write(ref _proxyActivityDrainScheduled, 0);
            if (!_exiting && !_pendingProxyActivities.IsEmpty &&
                Interlocked.Exchange(ref _proxyActivityDrainScheduled, 1) == 0)
                Dispatcher.UIThread.Post(DrainProxyActivities, DispatcherPriority.Background);
        }
    }

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
        _torrentsRefreshPending = false;
        var selectedHash = GetSelectedTorrentRow()?.Snapshot.InfoHash;
        _torrentNames.Refresh();
        _torrents.Items.Clear();
        var torrents = _transformer.GetSnapshots();
        foreach (var torrent in torrents)
        {
            var name = _torrentNames.Resolve(torrent.InfoHash) ?? "Torrent";
            var row = new TorrentRow(torrent, L(GetTorrentStatus(torrent)), name);
            var item = BuildTorrentListItem(row);
            _torrents.Items.Add(item);
            if (torrent.InfoHash == selectedHash)
                _torrents.SelectedItem = item;
        }
        _torrentsEmptyState.IsVisible = _torrents.ItemCount == 0;
        UpdateOverviewMetrics(torrents);
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
        if (GetSelectedTorrentRow() is not { } row)
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
        if (GetSelectedTorrentRow() is not { } row)
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

    private TorrentRow? GetSelectedTorrentRow() => _torrents.SelectedItem switch
    {
        TorrentRow row => row,
        ListBoxItem { Tag: TorrentRow row } => row,
        _ => null
    };

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
                        Text = string.Format(
                            CultureInfo.InvariantCulture,
                            L("Reset all tracked statistics for {0}?"),
                            AbbreviateHash(infoHash)),
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

    private async Task<bool> ConfirmDangerousActionAsync(
        string title,
        string message,
        string confirmLabel,
        bool browserAction = false)
    {
        var confirmed = false;
        var dialog = new Window
        {
            Title = title,
            Width = browserAction ? 392 : 430,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            ShowInTaskbar = false,
            WindowDecorations = WindowDecorations.None,
            Background = Brushes.Transparent,
            TransparencyLevelHint = [WindowTransparencyLevel.Transparent],
            TransparencyBackgroundFallback = XRatioPalette.Canvas,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var confirmWidth = browserAction ? 148d : 136d;
        var cancelWidth = browserAction ? 96d : 108d;
        var confirm = CreateButton(
            confirmLabel,
            browserAction ? ButtonTone.Primary : ButtonTone.DangerStrong,
            confirmWidth);
        var cancel = CreateButton("Cancel", ButtonTone.Secondary, cancelWidth);
        confirm.Width = confirmWidth;
        cancel.Width = cancelWidth;
        confirm.Height = 36;
        cancel.Height = 36;
        confirm.HorizontalAlignment = HorizontalAlignment.Stretch;
        cancel.HorizontalAlignment = HorizontalAlignment.Stretch;

        var close = new Button
        {
            Content = "×",
            Width = 32,
            MinWidth = 32,
            Height = 32,
            MinHeight = 32,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(16),
            Foreground = XRatioPalette.Muted,
            FontFamily = new FontFamily("Segoe UI Variable Text, Segoe UI"),
            FontSize = 18,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        close.Classes.Add("dialog-close");
        close.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("dialog-close").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.SurfaceRaised),
                new Setter(Button.BorderBrushProperty, XRatioPalette.Border),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Ink)
            }
        });
        close.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("dialog-close").Class(":focus-visible"))
        {
            Setters =
            {
                new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                new Setter(Button.BorderThicknessProperty, new Thickness(1))
            }
        });

        var titleMarker = browserAction
            ? (Control)new Border
            {
                Width = 30,
                Height = 30,
                CornerRadius = new CornerRadius(15),
                Background = XRatioPalette.AccentSoft,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new PathIcon
                {
                    Data = StreamGeometry.Parse(
                        "M14 3h7v7h-2V6.41l-9.29 9.3-1.42-1.42L17.59 5H14V3zM5 5h5V3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-5h-2v5H5V5z"),
                    Width = 15,
                    Height = 15,
                    Foreground = XRatioPalette.Accent,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
            : new Border
            {
                Width = 7,
                Height = 7,
                CornerRadius = new CornerRadius(4),
                Background = XRatioPalette.Danger,
                VerticalAlignment = VerticalAlignment.Center
            };

        var titleBar = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            MinHeight = browserAction ? 32 : 30,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Place(new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = browserAction ? 10 : 9,
                    VerticalAlignment = VerticalAlignment.Center,
                    Children =
                    {
                        titleMarker,
                        new TextBlock
                        {
                            Text = title,
                            FontSize = browserAction ? 14 : 13,
                            FontWeight = FontWeight.SemiBold,
                            Foreground = XRatioPalette.Ink,
                            VerticalAlignment = VerticalAlignment.Center
                        }
                    }
                }),
                Place(close, column: 1)
            }
        };

        titleBar.Children[0].PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(titleBar).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
                dialog.BeginMoveDrag(e);
        };

        confirm.Click += (_, _) =>
        {
            confirmed = true;
            dialog.Close();
        };
        close.Click += (_, _) => dialog.Close();
        cancel.Click += (_, _) => dialog.Close();

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
            ColumnSpacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Children =
            {
                Place(cancel, column: 1),
                Place(confirm, column: 2)
            }
        };

        var messageSurface = new Border
        {
            Background = browserAction ? XRatioPalette.MetricSurface : Brushes.Transparent,
            BorderBrush = browserAction ? XRatioPalette.Border : Brushes.Transparent,
            BorderThickness = browserAction ? new Thickness(1) : new Thickness(0),
            CornerRadius = browserAction ? new CornerRadius(8) : new CornerRadius(0),
            Padding = browserAction ? new Thickness(12, 10) : new Thickness(0),
            Child = new TextBlock
            {
                Text = message,
                Foreground = XRatioPalette.Ink,
                FontSize = browserAction ? 13 : 13.5,
                LineHeight = browserAction ? 19 : 20,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = browserAction ? 350 : 382
            }
        };

        dialog.Content = new Border
        {
            Background = XRatioPalette.Surface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(browserAction ? 14 : 13),
            Margin = new Thickness(6),
            Padding = new Thickness(18, 16, 18, 16),
            Child = new StackPanel
            {
                Spacing = browserAction ? 12 : 14,
                Children =
                {
                    new Border
                    {
                        BorderBrush = browserAction ? Brushes.Transparent : XRatioPalette.Border,
                        BorderThickness = browserAction ? new Thickness(0) : new Thickness(0, 0, 0, 1),
                        Padding = browserAction ? new Thickness(0) : new Thickness(0, 0, 0, 12),
                        Child = titleBar
                    },
                    messageSurface,
                    new Border
                    {
                        BorderBrush = browserAction ? Brushes.Transparent : XRatioPalette.Border,
                        BorderThickness = browserAction ? new Thickness(0) : new Thickness(0, 1, 0, 0),
                        Padding = browserAction ? new Thickness(0) : new Thickness(0, 12, 0, 0),
                        Child = actions
                    }
                }
            }
        };
        dialog.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
                dialog.Close();
        };
        ApplyLocalization(dialog);
        await dialog.ShowDialog(this);
        return confirmed;
    }

    private static string AbbreviateHash(string infoHash) =>
        infoHash.Length <= 8 ? infoHash : $"{infoHash[..8]}…";

    private static string FormatBytes(long bytes)
    {
        double value = bytes;
        var index = 0;
        while (Math.Abs(value) >= 1024 && index < ByteSuffixes.Length - 1)
        {
            value /= 1024;
            index++;
        }
        return index == 0
            ? $"{value:0} {ByteSuffixes[index]}"
            : $"{value:0.0} {ByteSuffixes[index]}";
    }

    private void AddActivity(
        string message,
        ActivityLevel? level = null,
        string? source = null,
        DateTimeOffset? timestamp = null,
        bool scrollIntoView = true)
    {
        var entry = ActivityEntry.Create(
            message,
            level ?? InferActivityLevel(message),
            source ?? InferActivitySource(message),
            timestamp ?? DateTimeOffset.Now);
        _activity.Items.Add(BuildActivityItem(entry));
        if (_activity.ItemCount > 500)
            _activity.Items.RemoveAt(0);
        if (scrollIntoView)
            _activity.ScrollIntoView(_activity.ItemCount - 1);
    }

    private void RefreshActivityLocalization()
    {
        var entries = _activity.Items
            .OfType<ListBoxItem>()
            .Select(item => item.Tag)
            .OfType<ActivityEntry>()
            .ToArray();
        if (entries.Length == 0)
            return;

        _activity.Items.Clear();
        foreach (var entry in entries)
            _activity.Items.Add(BuildActivityItem(entry));
        _activity.ScrollIntoView(_activity.ItemCount - 1);
    }

    private ListBoxItem BuildActivityItem(ActivityEntry entry)
    {
        var localizedEntry = ActivityEntry.Create(
            UiText.TranslateMessage(entry.CanonicalMessage, _language),
            entry.Level,
            entry.Source,
            entry.Timestamp);
        var color = entry.Level switch
        {
            ActivityLevel.Error => XRatioPalette.Danger,
            ActivityLevel.Warning => XRatioPalette.Warning,
            ActivityLevel.Success => XRatioPalette.Positive,
            _ => XRatioPalette.Muted
        };
        var needsSettings = entry.Level == ActivityLevel.Error &&
                            (entry.Detail.Contains("port", StringComparison.OrdinalIgnoreCase) ||
                             entry.Detail.Contains("socket", StringComparison.OrdinalIgnoreCase));
        Button? openSettings = null;
        if (needsSettings)
        {
            openSettings = CreateButton(L("Open Settings"), ButtonTone.Quiet, 112);
            openSettings.Click += (_, _) =>
            {
                _tabs.SelectedIndex = 4;
                _port.Focus();
            };
        }
        var details = new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock
                {
                    Text = localizedEntry.Summary,
                    FontSize = 12.5,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = XRatioPalette.Ink,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                },
                new TextBlock
                {
                    Text = localizedEntry.Detail,
                    IsVisible = !string.IsNullOrWhiteSpace(localizedEntry.Detail),
                    FontSize = 11.5,
                    Foreground = XRatioPalette.Muted,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }
        };
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions(needsSettings ? "72,86,*,Auto" : "72,86,*"),
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
                    Text = $"{L(entry.Level.ToString()).ToUpperInvariant()} · {L(entry.Source)}",
                    FontSize = 10.5,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = color,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Top
                }, column: 1),
                Place(details, column: 2)
            }
        };
        if (openSettings is not null)
            grid.Children.Add(Place(openSettings, column: 3));

        return new ListBoxItem
        {
            Tag = entry,
            Padding = new Thickness(10, 8),
            Content = grid
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
        _noDownload.IsChecked = true;
        _noDownload.IsEnabled = false;
        _pretendSeed.IsChecked = settings.PretendToSeed;
        _checkUpdatesOnStartup.IsChecked = settings.CheckUpdatesOnStartup;
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
        ReportDownloadAsZero = true,
        PretendToSeed = _pretendSeed.IsChecked == true,
        CheckUpdatesOnStartup = _checkUpdatesOnStartup.IsChecked == true,
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

    internal static TimeSpan ResolveSimulationTimerDuration(double value, string? unit)
    {
        if (!double.IsFinite(value) || value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Timer duration must be greater than zero.");

        var duration = NormalizeSimulationTimerUnit(unit) == SimulationTimerHours
            ? TimeSpan.FromHours(value)
            : TimeSpan.FromMinutes(value);
        return duration > TimeSpan.Zero
            ? duration
            : throw new ArgumentOutOfRangeException(nameof(value), "Timer duration is too small.");
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

    private static Border BuildCompactGroup(
        string title,
        Control body,
        bool showTitle = true) =>
        new()
        {
            Background = XRatioPalette.SurfaceRaised,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 9),
            Child = showTitle
                ? new Grid
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
                : body
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
        _simulationStopValue.Width = 96;
        _simulationStopValue.MinWidth = 96;
        _simulationStopHint.FontSize = 10.5;
        _simulationStopHint.Foreground = XRatioPalette.Muted;
        _simulationStopHint.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
        _simulationStopHint.Margin = new Thickness(0, 1, 0, 0);
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
        _simulationStopValueEditor.ColumnDefinitions = new ColumnDefinitions("Auto,Auto");
        _simulationStopValueEditor.ColumnSpacing = 6;
        _simulationStopValueEditor.HorizontalAlignment = HorizontalAlignment.Left;
        _simulationStopValueEditor.VerticalAlignment = VerticalAlignment.Center;
        _simulationStopValueEditor.Children.Clear();
        _simulationStopValueEditor.Children.Add(_simulationStopValue);
        _simulationStopValueEditor.Children.Add(Place(_simulationTimerUnitSelector, column: 1));
        var stop = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,180,Auto,*"),
            ColumnSpacing = 8,
            Children =
            {
                CreateCompactLabel("Stop"),
                Place(_simulationStopMode, column: 1),
                Place(_simulationStopValueEditor, column: 2)
            }
        };
        return new StackPanel { Spacing = 6, Children = { primary, stop, _simulationStopHint } };
    }

    private void UpdateSimulationStopEditor()
    {
        var mode = Math.Clamp(_simulationStopMode.SelectedIndex, 0, 4);
        var (placeholder, hint) = mode switch
        {
            SimulationTimerStopMode => ("Duration", "Timer starts when Start is pressed and stops this session after the selected duration."),
            2 => ("MiB", "Stop automatically after this session uploads the selected amount."),
            3 => ("MiB", "Stop automatically after this session downloads the selected amount."),
            4 => ("Ratio", "Stop automatically when the selected upload/download ratio is reached."),
            _ => ("Not used", "Leave Never selected for manual stopping, or choose a rule above to stop automatically.")
        };

        _simulationStopValueEditor.IsVisible = mode > 0;
        _simulationStopValue.IsEnabled = mode > 0;
        _simulationTimerUnitSelector.IsVisible = mode == SimulationTimerStopMode;
        _simulationTimerUnitSelector.IsEnabled = mode == SimulationTimerStopMode;
        _simulationStopValue.PlaceholderText = L(placeholder);
        _simulationStopHint.Text = L(hint);
    }

    private Border BuildSimulationTimerUnitSelector()
    {
        ConfigureSimulationTimerUnitToggle(_simulationTimerMinutes, SimulationTimerMinutes, "SimulationTimerMinutes");
        ConfigureSimulationTimerUnitToggle(_simulationTimerHours, SimulationTimerHours, "SimulationTimerHours");
        _simulationTimerUnitSelector.Tag = "SimulationTimerUnitSelector";
        _simulationTimerUnitSelector.Width = SimulationTimerUnitSelectorWidth;
        _simulationTimerUnitSelector.MinWidth = SimulationTimerUnitSelectorWidth;
        _simulationTimerUnitSelector.MinHeight = 32;
        _simulationTimerUnitSelector.Height = 32;
        _simulationTimerUnitSelector.Padding = new Thickness(2);
        _simulationTimerUnitSelector.Background = XRatioPalette.SurfaceRaised;
        _simulationTimerUnitSelector.BorderBrush = XRatioPalette.Border;
        _simulationTimerUnitSelector.BorderThickness = new Thickness(1);
        _simulationTimerUnitSelector.CornerRadius = new CornerRadius(7);
        _simulationTimerUnitSelector.VerticalAlignment = VerticalAlignment.Center;
        _simulationTimerUnitSelector.Child = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 2,
            Children =
            {
                _simulationTimerMinutes,
                Place(_simulationTimerHours, column: 1)
            }
        };
        _simulationTimerMinutes.Click += (_, _) => SetSimulationTimerUnit(SimulationTimerMinutes);
        _simulationTimerHours.Click += (_, _) => SetSimulationTimerUnit(SimulationTimerHours);
        SetSimulationTimerUnit(SimulationTimerMinutes, persist: false);
        return _simulationTimerUnitSelector;
    }

    private static void ConfigureSimulationTimerUnitToggle(
        ToggleButton button,
        string label,
        string tag)
    {
        button.Tag = tag;
        button.Content = label;
        button.Width = 50;
        button.MinWidth = 50;
        button.MinHeight = 26;
        button.Height = 26;
        button.Padding = new Thickness(5, 2);
        button.Background = Brushes.Transparent;
        button.BorderBrush = Brushes.Transparent;
        button.BorderThickness = new Thickness(0);
        button.CornerRadius = new CornerRadius(5);
        button.Foreground = XRatioPalette.Muted;
        button.FontSize = 10.5;
        button.FontWeight = FontWeight.SemiBold;
        button.HorizontalContentAlignment = HorizontalAlignment.Center;
        button.VerticalContentAlignment = VerticalAlignment.Center;
        button.Cursor = new Cursor(StandardCursorType.Hand);
        AutomationProperties.SetName(button, label);
    }

    private void SetSimulationTimerUnit(string? unit, bool persist = true)
    {
        _simulationTimerUnit = NormalizeSimulationTimerUnit(unit);
        var minutesSelected = _simulationTimerUnit == SimulationTimerMinutes;
        _simulationTimerMinutes.IsChecked = minutesSelected;
        _simulationTimerHours.IsChecked = !minutesSelected;
        ApplySimulationTimerUnitVisual(_simulationTimerMinutes, minutesSelected);
        ApplySimulationTimerUnitVisual(_simulationTimerHours, !minutesSelected);
        if (persist)
            QueueSimulationFormPersistence();
    }

    private static void ApplySimulationTimerUnitVisual(ToggleButton button, bool selected)
    {
        button.Background = selected ? XRatioPalette.AccentSoft : Brushes.Transparent;
        button.BorderBrush = selected ? XRatioPalette.Accent : Brushes.Transparent;
        button.BorderThickness = selected ? new Thickness(1) : new Thickness(0);
        button.Foreground = selected ? XRatioPalette.Accent : XRatioPalette.Muted;
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

    private void UpdateOverviewMetrics(IReadOnlyList<TorrentSnapshot>? torrents = null)
    {
        _overviewProxyKpi.Text = L(_proxy?.IsRunning == true ? _paused ? "Paused" : "Active" : "Stopped");
        _pause.IsEnabled = _proxy?.IsRunning == true;
        _statusIndicator.Background = _startupFailureBanner.IsVisible
            ? XRatioPalette.Danger
            : _proxy?.IsRunning == true
                ? _paused ? XRatioPalette.Warning : XRatioPalette.Positive
                : XRatioPalette.Subtle;
        var snapshots = torrents ?? _transformer.GetSnapshots();
        _overviewTorrentKpi.Text = snapshots.Count.ToString(CultureInfo.InvariantCulture);
        var active = _simulationEntries.Values.Count(entry => entry.Session.State == SimulationState.Running);
        _overviewSimulationKpi.Text = $"{active} / {_simulationEntries.Count}";
        _overviewReportedKpi.Text = FormatBytes(snapshots.Sum(torrent => torrent.ReportedUploadedTotal));
        RefreshOnboarding();
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
            Cursor = new Cursor(StandardCursorType.Arrow),
            Children =
            {
                BuildFlagIcon(UiText.FlagCodeAt(index)),
                new TextBlock
                {
                    Text = UiText.DisplayNameAt(index),
                    Foreground = XRatioPalette.Ink,
                    VerticalAlignment = VerticalAlignment.Center,
                    Cursor = new Cursor(StandardCursorType.Arrow)
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

    private static Grid BuildCloseGlyph(double size = CloseGlyphSize)
    {
        var glyph = new Grid
        {
            Tag = "CloseGlyph",
            Width = size,
            Height = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            ClipToBounds = false
        };

        foreach (var angle in new[] { 45d, -45d })
        {
            glyph.Children.Add(new Border
            {
                Tag = "CloseGlyphStroke",
                Width = size,
                Height = 1.5,
                CornerRadius = new CornerRadius(0.75),
                Background = XRatioPalette.Subtle,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new RotateTransform(angle)
            });
        }

        return glyph;
    }

    private static Button CreateButton(string content, ButtonTone tone, double minWidth)
    {
        var button = new Button { Content = content };
        StyleButton(button, tone, minWidth);
        return button;
    }

    private static void SetCloseButtonHoverState(Button button, bool hovered)
    {
        button.Background = hovered ? XRatioPalette.NeutralSoft : Brushes.Transparent;
        button.BorderBrush = hovered ? XRatioPalette.NavBorder : Brushes.Transparent;
        button.BorderThickness = hovered ? new Thickness(1) : new Thickness(0);
        button.Foreground = hovered ? XRatioPalette.Ink : XRatioPalette.Subtle;
        button.Opacity = hovered ? 1 : 0.78;
        var radius = Math.Min(button.Width, button.Height) / 2;
        button.CornerRadius = double.IsNaN(radius) || double.IsInfinity(radius)
            ? new CornerRadius(12)
            : new CornerRadius(radius);
        if (button.Content is PathIcon icon)
            icon.Foreground = hovered ? XRatioPalette.Ink : XRatioPalette.Subtle;
        if (button.Content is TextBlock glyph)
            glyph.Foreground = hovered ? XRatioPalette.Ink : XRatioPalette.Subtle;
        if (button.Content is Grid closeGlyph)
        {
            foreach (var stroke in closeGlyph.Children.OfType<Border>()
                         .Where(child => Equals(child.Tag, "CloseGlyphStroke")))
            {
                stroke.Background = hovered ? XRatioPalette.Ink : XRatioPalette.Subtle;
            }
        }
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
        // The reference components use their own semantic aliases, but the
        // aliases follow the selected XRatio theme. A dark island inside the
        // light console breaks hierarchy and was the source of the previous
        // onboarding's visual mismatch.
        public static readonly SolidColorBrush ReferenceCanvas = Brush("#111111");
        public static readonly SolidColorBrush ReferenceSurface = Brush("#171717");
        public static readonly SolidColorBrush ReferenceRaised = Brush("#1D1D1D");
        public static readonly SolidColorBrush ReferenceField = Brush("#262626");
        public static readonly SolidColorBrush ReferenceSelected = Brush("#242424");
        public static readonly SolidColorBrush ReferenceBorder = Brush("#2B2B2B");
        public static readonly SolidColorBrush ReferenceText = Brush("#F5F5F5");
        public static readonly SolidColorBrush ReferenceMuted = Brush("#A5A5A5");
        public static readonly SolidColorBrush ReferenceSubtle = Brush("#777777");
        public static readonly SolidColorBrush ReferenceGreen = Brush("#42D39A");
        public static readonly SolidColorBrush ReferenceGreenSoft = Brush("#19382F");
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
        public static readonly SolidColorBrush SectionBorder = Brush("#8297AE");
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
            Set(SectionBorder, dark ? "#4A6685" : softDark ? "#596574" : dim ? "#6E829C" : "#8297AE");
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

            ReferenceCanvas.Color = Canvas.Color;
            ReferenceSurface.Color = Surface.Color;
            ReferenceRaised.Color = SurfaceRaised.Color;
            ReferenceField.Color = MetricSurface.Color;
            ReferenceSelected.Color = NeutralSoft.Color;
            ReferenceBorder.Color = Border.Color;
            ReferenceText.Color = Ink.Color;
            ReferenceMuted.Color = Muted.Color;
            ReferenceSubtle.Color = Subtle.Color;
            ReferenceGreen.Color = Positive.Color;
            ReferenceGreenSoft.Color = PositiveSoft.Color;
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
            BuildOnboardingNavigationGroup(),
            BuildSupportNavigationGroup());

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
        // Keep the tour in its own rail row. If it lives in the same scroll
        // surface as the six regular destinations, only the first two task
        // capsules remain visible above the fixed Support footer.
        var hasFixedOnboarding = groups.Length >= 2;
        var regularGroupCount = hasFixedOnboarding
            ? Math.Max(0, groups.Length - 2)
            : Math.Max(0, groups.Length - 1);
        foreach (var group in groups.Take(regularGroupCount))
            upperGroups.Children.Add(group);

        var upperScroll = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = upperGroups,
            ClipToBounds = true,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Top,
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var content = new Grid
        {
            RowDefinitions = hasFixedOnboarding
                ? new RowDefinitions("*,Auto,Auto")
                : new RowDefinitions("*,Auto"),
            ClipToBounds = true
        };
        content.Children.Add(upperScroll);
        if (hasFixedOnboarding)
        {
            var onboardingScroll = new ScrollViewer
            {
                Tag = "OnboardingRailScroll",
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = groups[^2],
                ClipToBounds = true,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MaxHeight = 280
            };
            content.Children.Add(Place(onboardingScroll, row: 1));
            content.Children.Add(Place(groups[^1], row: 2));
        }
        else
        {
            content.Children.Add(Place(groups[^1], row: 1));
        }

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

    private StackPanel BuildSupportNavigationGroup()
    {
        var group = new StackPanel { Spacing = 0 };
        // Keep the Support heading on the same content rail as every other
        // navigation section; the action row can still use the full width.
        group.Children.Add(BuildNavigationSection("Support", divider: true, horizontalMargin: 4));

        var guide = BuildNavigationButton(
            null,
            string.Empty,
            "Guide",
            icon: BuildGuideIcon());
        guide.Height = 44;
        guide.VerticalAlignment = VerticalAlignment.Center;
        guide.VerticalContentAlignment = VerticalAlignment.Center;
        var bugReport = BuildBugReportNavigationButton();
        var github = BuildGitHubNavigationButton();
        group.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto"),
            RowDefinitions = new RowDefinitions("48"),
            ColumnSpacing = 4,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                Place(guide),
                Place(bugReport, column: 1),
                Place(github, column: 2),
                Place(_updateIndicator, column: 3)
            }
        });
        return group;
    }

    private StackPanel BuildOnboardingNavigationGroup()
    {
        var group = _onboardingNavigationGroup;
        group.Spacing = 0;
        group.Children.Clear();
        group.Children.Add(BuildNavigationSection("Get started", divider: true));

        var onboarding = BuildNavigationButton(
            null,
            "\uE768",
            "Onboarding",
            OpenOnboardingFromNavigationAsync);
        onboarding.Tag = "OnboardingNavAction";

        if (onboarding.Content is Border headerRow)
        {
            headerRow.Padding = new Thickness(12, 6, 8, 6);
            headerRow.Child = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
                ColumnSpacing = 10,
                Children =
                {
                    new TextBlock
                    {
                        Tag = "NavIcon",
                        Text = "\uE768",
                        FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                        FontSize = 14,
                        Width = 16,
                        TextAlignment = TextAlignment.Center,
                        Foreground = XRatioPalette.Muted,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Place(
                        new TextBlock
                        {
                            Tag = "OnboardingSidebarLabel",
                            Text = "Onboarding",
                            FontSize = 12.5,
                            FontWeight = FontWeight.SemiBold,
                            Foreground = XRatioPalette.Ink,
                            VerticalAlignment = VerticalAlignment.Center
                        },
                        column: 1),
                    Place(
                        _onboardingChecklistProgress,
                        column: 2)
                }
            };
        }

        _onboardingChecklistProgress.Tag = "OnboardingChecklistProgress";
        _onboardingChecklistProgress.Text = $"0/{OnboardingSteps.Count}";
        _onboardingChecklistProgress.FontSize = 10;
        _onboardingChecklistProgress.FontWeight = FontWeight.SemiBold;
        _onboardingChecklistProgress.Foreground = XRatioPalette.Accent;
        _onboardingChecklistProgress.VerticalAlignment = VerticalAlignment.Center;

        var dismiss = _onboardingSidebarClose;
        dismiss.Tag = "OnboardingSidebarClose";
        dismiss.Content = BuildCloseGlyph(CloseGlyphSize);
        // Keep the close affordance quiet at rest and give it a compact,
        // centered hover target. The custom template forwards the Button
        // brushes so the bubble is rendered consistently by every theme.
        dismiss.Template = new FuncControlTemplate<Button>((button, _) => new Border
        {
            [!Border.BackgroundProperty] = button[!Button.BackgroundProperty],
            [!Border.BorderBrushProperty] = button[!Button.BorderBrushProperty],
            [!Border.BorderThicknessProperty] = button[!Button.BorderThicknessProperty],
            [!Border.CornerRadiusProperty] = button[!Button.CornerRadiusProperty],
            Child = new ContentPresenter
            {
                [!ContentPresenter.ContentProperty] = button[!Button.ContentProperty],
                [!ContentPresenter.HorizontalContentAlignmentProperty] =
                    button[!Button.HorizontalContentAlignmentProperty],
                [!ContentPresenter.VerticalContentAlignmentProperty] =
                    button[!Button.VerticalContentAlignmentProperty]
            }
        });
        dismiss.Classes.Add("onboarding-sidebar-close");
        dismiss.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("onboarding-sidebar-close").Class(":pointerover"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.NeutralSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.NavBorder),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Ink),
                new Setter(Button.OpacityProperty, 1d),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(12))
            }
        });
        dismiss.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("onboarding-sidebar-close").Class(":pressed"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.NeutralSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.NavBorder),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Ink),
                new Setter(Button.OpacityProperty, 1d),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(12))
            }
        });
        dismiss.Styles.Add(new Style(selector =>
            selector.OfType<Button>().Class("onboarding-sidebar-close").Class(":focus-visible"))
        {
            Setters =
            {
                new Setter(Button.BackgroundProperty, XRatioPalette.NeutralSoft),
                new Setter(Button.BorderBrushProperty, XRatioPalette.NavBorder),
                new Setter(Button.BorderThicknessProperty, new Thickness(1)),
                new Setter(Button.ForegroundProperty, XRatioPalette.Ink),
                new Setter(Button.OpacityProperty, 1d),
                new Setter(Button.CornerRadiusProperty, new CornerRadius(12))
            }
        });
        dismiss.Width = CloseButtonSize;
        dismiss.MinWidth = CloseButtonSize;
        dismiss.Height = CloseButtonSize;
        dismiss.MinHeight = CloseButtonSize;
        dismiss.Padding = new Thickness(0);
        dismiss.Margin = new Thickness(0);
        dismiss.Background = Brushes.Transparent;
        dismiss.BorderBrush = Brushes.Transparent;
        dismiss.BorderThickness = new Thickness(0);
        dismiss.CornerRadius = new CornerRadius(CloseButtonSize / 2);
        dismiss.Foreground = XRatioPalette.Subtle;
        dismiss.Opacity = 0.78;
        dismiss.HorizontalContentAlignment = HorizontalAlignment.Center;
        dismiss.VerticalContentAlignment = VerticalAlignment.Center;
        dismiss.HorizontalAlignment = HorizontalAlignment.Center;
        dismiss.VerticalAlignment = VerticalAlignment.Center;
        dismiss.PointerEntered += (_, _) => SetCloseButtonHoverState(dismiss, hovered: true);
        dismiss.PointerExited += (_, _) => SetCloseButtonHoverState(dismiss, hovered: false);
        dismiss.GotFocus += (_, _) => SetCloseButtonHoverState(dismiss, hovered: true);
        dismiss.LostFocus += (_, _) => SetCloseButtonHoverState(dismiss, hovered: false);
        ToolTip.SetTip(dismiss, L("Close onboarding"));
        dismiss.Click += async (_, eventArgs) =>
        {
            eventArgs.Handled = true;
            await DismissOnboardingAsync();
        };

        group.Children.Add(new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 4,
            Children =
            {
                onboarding,
                Place(dismiss, column: 1)
            }
        });

        _onboardingChecklistContainer.Tag = "OnboardingChecklist";
        _onboardingChecklistContainer.Spacing = 0;
        _onboardingChecklistContainer.Margin = new Thickness(4, 0, 4, 2);
        _onboardingChecklistContainer.MinHeight = 0;
        _onboardingChecklistContainer.ClipToBounds = true;
        _onboardingChecklistContainer.IsVisible = false;
        group.Children.Add(_onboardingChecklistContainer);

        return group;
    }

    private Control BuildOnboardingSidebarCard()
    {
        _onboardingSidebarCounter.FontSize = 10;
        _onboardingSidebarCounter.FontWeight = FontWeight.SemiBold;
        _onboardingSidebarCounter.Foreground = XRatioPalette.Subtle;
        _onboardingSidebarCounter.VerticalAlignment = VerticalAlignment.Center;

        _onboardingSidebarStatusIcon.FontSize = 14;
        _onboardingSidebarStatusIcon.FontWeight = FontWeight.Bold;
        _onboardingSidebarStatusIcon.VerticalAlignment = VerticalAlignment.Center;

        _onboardingSidebarTitle.FontSize = 13;
        _onboardingSidebarTitle.FontWeight = FontWeight.SemiBold;
        _onboardingSidebarTitle.Foreground = XRatioPalette.Ink;
        _onboardingSidebarTitle.TextWrapping = Avalonia.Media.TextWrapping.Wrap;

        _onboardingSidebarDots.Orientation = Orientation.Horizontal;
        _onboardingSidebarDots.Spacing = 4;
        _onboardingSidebarDots.HorizontalAlignment = HorizontalAlignment.Center;
        for (var index = 0; index < OnboardingSteps.Count; index++)
        {
            _onboardingSidebarDots.Children.Add(new Border
            {
                Tag = index,
                Width = 5,
                Height = 5,
                CornerRadius = new CornerRadius(3),
                Background = XRatioPalette.Border,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        _onboardingSidebarAction.Tag = "OnboardingSidebarAction";
        StyleButton(_onboardingSidebarAction, ButtonTone.Primary, 0);
        _onboardingSidebarAction.MinHeight = 36;
        _onboardingSidebarAction.HorizontalAlignment = HorizontalAlignment.Stretch;
        _onboardingSidebarAction.Padding = new Thickness(9, 5);
        _onboardingSidebarAction.CornerRadius = new CornerRadius(17);
        _onboardingSidebarAction.Click += async (_, _) =>
            await OpenOnboardingSidebarStepAsync(_onboardingStepIndex);

        _onboardingSidebarDone.Tag = "OnboardingSidebarDone";
        StyleButton(_onboardingSidebarDone, ButtonTone.Quiet, 0);
        _onboardingSidebarDone.MinHeight = 36;
        _onboardingSidebarDone.MinWidth = 36;
        _onboardingSidebarDone.Width = 36;
        _onboardingSidebarDone.Padding = new Thickness(0);
        _onboardingSidebarDone.CornerRadius = new CornerRadius(18);
        _onboardingSidebarDone.Click += async (_, _) => await MarkCurrentOnboardingStepAsync();

        _onboardingSidebarPrevious.Tag = "OnboardingSidebarPrevious";
        _onboardingSidebarPrevious.Content = "←";
        StyleButton(_onboardingSidebarPrevious, ButtonTone.Quiet, 30);
        _onboardingSidebarPrevious.Width = 30;
        _onboardingSidebarPrevious.MinWidth = 30;
        _onboardingSidebarPrevious.Height = 36;
        _onboardingSidebarPrevious.MinHeight = 36;
        _onboardingSidebarPrevious.Padding = new Thickness(0);
        _onboardingSidebarPrevious.CornerRadius = new CornerRadius(18);
        _onboardingSidebarPrevious.Click += (_, _) =>
        {
            if (_onboardingStepIndex <= 0)
                return;
            _onboardingStepIndex--;
            RefreshOnboarding();
        };

        _onboardingSidebarNext.Tag = "OnboardingSidebarNext";
        _onboardingSidebarNext.Content = "→";
        StyleButton(_onboardingSidebarNext, ButtonTone.Quiet, 30);
        _onboardingSidebarNext.Width = 30;
        _onboardingSidebarNext.MinWidth = 30;
        _onboardingSidebarNext.Height = 36;
        _onboardingSidebarNext.MinHeight = 36;
        _onboardingSidebarNext.Padding = new Thickness(0);
        _onboardingSidebarNext.CornerRadius = new CornerRadius(18);
        _onboardingSidebarNext.Click += async (_, _) => await MoveToNextOnboardingStepAsync();

        var actions = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            ColumnSpacing = 6,
            Children =
            {
                _onboardingSidebarAction,
                Place(_onboardingSidebarDone, column: 1)
            }
        };

        var footer = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            ColumnSpacing = 6,
            Children =
            {
                _onboardingSidebarPrevious,
                Place(_onboardingSidebarDots, column: 1),
                Place(_onboardingSidebarNext, column: 2)
            }
        };

        return new Border
        {
            Tag = "OnboardingSidebarCard",
            Background = XRatioPalette.MetricSurface,
            BorderBrush = XRatioPalette.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(9),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Spacing = 7,
                Children =
                {
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto"),
                        ColumnSpacing = 7,
                        Children =
                        {
                            _onboardingSidebarTitle,
                            Place(_onboardingSidebarStatusIcon, column: 2),
                            Place(_onboardingSidebarCounter, column: 1)
                        }
                    },
                    actions,
                    footer
                }
            }
        };
    }

    // The sidebar is the persistent checklist. It uses the compact List variant
    // of Task Rows: one shared surface, fine separators, and a single selected
    // row instead of five competing capsule cards.
    private Control BuildOnboardingSidebarCapsules()
    {
        _onboardingSidebarRows.Clear();

        _onboardingSidebarCounter.FontSize = 10;
        _onboardingSidebarCounter.FontWeight = FontWeight.SemiBold;
        _onboardingSidebarCounter.Foreground = XRatioPalette.Subtle;
        _onboardingSidebarTitle.FontSize = 11;
        _onboardingSidebarTitle.Foreground = XRatioPalette.Ink;
        _onboardingSidebarDescription.FontSize = 10;
        _onboardingSidebarDetail.FontSize = 10;

        _onboardingSidebarDots.Orientation = Orientation.Horizontal;
        _onboardingSidebarDots.Spacing = 4;
        _onboardingSidebarDots.HorizontalAlignment = HorizontalAlignment.Center;
        _onboardingSidebarDots.VerticalAlignment = VerticalAlignment.Center;
        _onboardingSidebarDots.Children.Clear();
        for (var index = 0; index < OnboardingSteps.Count; index++)
        {
            _onboardingSidebarDots.Children.Add(new Border
            {
                Tag = index,
                Width = 5,
                Height = 5,
                CornerRadius = new CornerRadius(3),
                Background = XRatioPalette.Border,
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        var rows = new StackPanel
        {
            Tag = "OnboardingSidebarCapsules",
            Spacing = 8,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        for (var index = 0; index < OnboardingSteps.Count; index++)
        {
            var stepIndex = index;
            var step = OnboardingSteps[index];
            var status = new TextBlock
            {
                Text = (index + 1).ToString(CultureInfo.InvariantCulture),
                FontSize = 10,
                FontWeight = FontWeight.SemiBold,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = XRatioPalette.Muted
            };
            var statusBadge = new Border
            {
                Width = 24,
                Height = 24,
                CornerRadius = new CornerRadius(12),
                Background = XRatioPalette.Surface,
                BorderBrush = XRatioPalette.Border,
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Child = status
            };
            var title = new TextBlock
            {
                Text = L(step.Title),
                FontSize = 11.5,
                FontWeight = FontWeight.SemiBold,
                Foreground = XRatioPalette.Ink,
                TextTrimming = TextTrimming.CharacterEllipsis,
                VerticalAlignment = VerticalAlignment.Center
            };
            var meta = new TextBlock
            {
                Text = L("To do"),
                FontSize = 9.5,
                FontWeight = FontWeight.SemiBold,
                Foreground = XRatioPalette.Subtle,
                TextTrimming = TextTrimming.CharacterEllipsis,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 76
            };
            var metaPill = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                Padding = new Thickness(2, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = meta
            };
            var chevron = new TextBlock
            {
                Text = "›",
                FontSize = 17,
                Foreground = XRatioPalette.Subtle,
                Width = 14,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var rowButton = new Button
            {
                Tag = $"OnboardingSidebarStep{index + 1}",
                MinHeight = 44,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                Background = XRatioPalette.Surface,
                BorderBrush = XRatioPalette.Border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(22),
                Padding = new Thickness(10, 4),
                Content = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"),
                    ColumnSpacing = 6,
                    Children =
                    {
                        statusBadge,
                        Place(title, column: 1),
                        Place(metaPill, column: 2),
                        Place(chevron, column: 3)
                    }
                }
            };
            rowButton.Classes.Add("onboarding-capsule");
            rowButton.Styles.Add(new Style(selector =>
                selector.OfType<Button>().Class("onboarding-capsule").Class(":pointerover"))
            {
                Setters =
                {
                    new Setter(Button.BackgroundProperty, XRatioPalette.NeutralSoft),
                    new Setter(Button.BorderBrushProperty, XRatioPalette.Border),
                    new Setter(Button.BorderThicknessProperty, new Thickness(1))
                }
            });
            rowButton.Styles.Add(new Style(selector =>
                selector.OfType<Button>().Class("onboarding-capsule").Class(":pressed"))
            {
                Setters =
                {
                    new Setter(Button.BackgroundProperty, XRatioPalette.AccentSoft),
                    new Setter(Button.BorderBrushProperty, XRatioPalette.Accent),
                    new Setter(Button.BorderThicknessProperty, new Thickness(1))
                }
            });
            ToolTip.SetTip(
                rowButton,
                $"{L(step.Title)}: {L(SidebarOnboardingDescription(step))}");
            rowButton.Click += async (_, _) => await OpenOnboardingSidebarStepAsync(stepIndex);
            rows.Children.Add(rowButton);
            _onboardingSidebarRows.Add(
                new OnboardingSidebarRowView(
                    stepIndex,
                    rowButton,
                    statusBadge,
                    status,
                    title,
                    meta,
                    metaPill,
                    chevron));
        }

        _onboardingSidebarPrevious.Tag = "OnboardingSidebarPrevious";
        _onboardingSidebarPrevious.Content = "←";
        StyleButton(_onboardingSidebarPrevious, ButtonTone.Quiet, 26);
        _onboardingSidebarPrevious.Width = 26;
        _onboardingSidebarPrevious.MinWidth = 26;
        _onboardingSidebarPrevious.Height = 26;
        _onboardingSidebarPrevious.MinHeight = 36;
        _onboardingSidebarPrevious.Padding = new Thickness(0);
        _onboardingSidebarPrevious.CornerRadius = new CornerRadius(13);
        _onboardingSidebarPrevious.Click += (_, _) =>
        {
            if (_onboardingStepIndex <= 0)
                return;
            _onboardingStepIndex--;
            RefreshOnboarding();
        };

        _onboardingSidebarNext.Tag = "OnboardingSidebarNext";
        _onboardingSidebarNext.Content = "→";
        StyleButton(_onboardingSidebarNext, ButtonTone.Quiet, 26);
        _onboardingSidebarNext.Width = 26;
        _onboardingSidebarNext.MinWidth = 26;
        _onboardingSidebarNext.Height = 26;
        _onboardingSidebarNext.MinHeight = 36;
        _onboardingSidebarNext.Padding = new Thickness(0);
        _onboardingSidebarNext.CornerRadius = new CornerRadius(13);
        _onboardingSidebarNext.Click += async (_, _) => await MoveToNextOnboardingStepAsync();

        // Navigation arrows belong to the focused overlay and overview card.
        // Keeping another footer in the rail made the compact Task Rows feel
        // like a second pager and pushed the last rows below Support.
        _onboardingSidebarPrevious.IsVisible = false;
        _onboardingSidebarNext.IsVisible = false;
        _onboardingSidebarDots.IsVisible = false;

        // These controls keep the existing automation/accessibility contract;
        // the capsule rows are the visible direct actions now.
        _onboardingSidebarAction.Tag = "OnboardingSidebarAction";
        _onboardingSidebarAction.Content = "Settings →";
        _onboardingSidebarAction.MinHeight = 36;
        _onboardingSidebarAction.IsVisible = false;
        _onboardingSidebarAction.Click += async (_, _) =>
            await OpenOnboardingSidebarStepAsync(_onboardingStepIndex);
        _onboardingSidebarDone.Tag = "OnboardingSidebarDone";
        _onboardingSidebarDone.Content = "✓";
        _onboardingSidebarDone.MinHeight = 36;
        _onboardingSidebarDone.IsVisible = false;
        _onboardingSidebarDone.Click += async (_, _) => await MarkCurrentOnboardingStepAsync();
        var legacyContract = new StackPanel
        {
            IsVisible = false,
            Children =
            {
                _onboardingSidebarCounter,
                _onboardingSidebarStatusIcon,
                _onboardingSidebarTitle,
                _onboardingSidebarDescription,
                _onboardingSidebarDetail,
                _onboardingSidebarAction,
                _onboardingSidebarDone,
                _onboardingSidebarPrevious,
                _onboardingSidebarDots,
                _onboardingSidebarNext
            }
        };

        return new Border
        {
            Tag = "OnboardingSidebarCard",
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(0),
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ClipToBounds = true,
            Child = new StackPanel
            {
                Spacing = 8,
                Children = { rows, legacyContract }
            }
        };
    }

    private static StackPanel BuildNavigationSection(
        string section,
        bool divider,
        double horizontalMargin = 4)
    {
        var header = new StackPanel
        {
            Spacing = 5,
            Margin = new Thickness(horizontalMargin, divider ? 16 : 5, horizontalMargin, 4)
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
            // Keep group labels grounded in the familiar desktop UI face.
            // A slightly larger medium cut reads like a native section heading,
            // rather than a tiny over-designed metadata label.
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = XRatioPalette.Muted,
            LetterSpacing = 0,
            VerticalAlignment = VerticalAlignment.Center
        });
        return header;
    }

    private Button BuildNavigationButton(
        int? index,
        string glyph,
        string label,
        Func<Task>? action = null,
        Control? icon = null)
    {
        var row = BuildNavRow(glyph, label, icon);
        var button = new Button
        {
            Tag = index is int tabIndex
                ? tabIndex
                : action is null ? "GuideAction" : "OnboardingAction",
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
            else if (action is not null)
                await action();
            else
                await ShowGuideAsync(_tabs);
        };
        _navigationItems.Add((button, row));
        return button;
    }

    private static Border BuildNavRow(string glyph, string label, Control? icon = null) =>
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
                    icon ?? new TextBlock
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
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11,
            FontWeight = FontWeight.Medium,
            Foreground = XRatioPalette.Muted,
            LetterSpacing = 0,
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

    private ListBoxItem BuildTorrentListItem(TorrentRow row)
    {
        var snapshot = row.Snapshot;
        var lastAnnounce = snapshot.LastAnnounce?.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                           ?? "—";
        var tracker = MaskTrackerUrl(snapshot.Tracker);
        var identity = string.Format(
            CultureInfo.InvariantCulture,
            L("{0} · {1}/{2} peers · {3} · last {4}"),
            tracker,
            snapshot.CompletePeers,
            snapshot.IncompletePeers,
            row.Status,
            lastAnnounce);
        var counters = string.Format(
            CultureInfo.InvariantCulture,
            L("Actual ↓ {0} ↑ {1} left {2}   ·   Reported ↓ {3} ↑ {4} left {5}"),
            FormatBytes(snapshot.ActualDownloaded),
            FormatBytes(snapshot.ActualUploaded),
            FormatBytes(snapshot.ActualLeft),
            FormatBytes(snapshot.ReportedDownloaded),
            FormatBytes(snapshot.ReportedUploaded),
            FormatBytes(snapshot.ReportedLeft));
        var content = new StackPanel
        {
            Spacing = 2,
            Margin = new Thickness(4, 4),
            Children =
            {
                new TextBlock
                {
                    Text = row.Name,
                    Foreground = XRatioPalette.Ink,
                    FontSize = 12.5,
                    FontWeight = FontWeight.SemiBold,
                    TextTrimming = TextTrimming.CharacterEllipsis
                },
                new TextBlock
                {
                    Text = identity,
                    Foreground = XRatioPalette.Muted,
                    FontSize = 10.5,
                    FontFeatures = XRatioPalette.TabularNumbers,
                    TextTrimming = TextTrimming.CharacterEllipsis
                },
                new TextBlock
                {
                    Text = counters,
                    Foreground = XRatioPalette.Subtle,
                    FontSize = 10,
                    FontFeatures = XRatioPalette.TabularNumbers,
                    TextTrimming = TextTrimming.CharacterEllipsis
                }
            }
        };
        var surface = new Border
        {
            Padding = new Thickness(4, 2),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Child = content
        };
        ToolTip.SetTip(surface, $"{L("Info hash")}: {snapshot.InfoHash}");
        return new ListBoxItem
        {
            Tag = row,
            Content = surface,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(0),
            MinHeight = 62
        };
    }

    private sealed record TorrentRow(TorrentSnapshot Snapshot, string Status, string Name)
    {
        public override string ToString()
        {
            var lastAnnounce = Snapshot.LastAnnounce?.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture)
                               ?? "—";
            return $"{Name} | {MaskTrackerUrl(Snapshot.Tracker)} | " +
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
        string Detail,
        string CanonicalMessage)
    {
        public static ActivityEntry Create(
            string message,
            ActivityLevel level,
            string source,
            DateTimeOffset timestamp)
        {
            var canonicalMessage = message.Trim();
            var separator = canonicalMessage.IndexOf(':');
            return separator is > 0 and < 48
                ? new ActivityEntry(
                    timestamp,
                    level,
                    source,
                    canonicalMessage[..separator].Trim(),
                    canonicalMessage[(separator + 1)..].Trim(),
                    canonicalMessage)
                : new ActivityEntry(timestamp, level, source, canonicalMessage, string.Empty, canonicalMessage);
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
