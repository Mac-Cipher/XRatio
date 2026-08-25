using XRatio.Core.Announcements;
using XRatio.Core.Simulation;

namespace XRatio.Core.Configuration;

public sealed record SimulationFormSettings
{
    public string? TorrentPath { get; init; }
    public string? Tracker { get; init; }
    public string ClientProfileId { get; init; } = SimulationDefaults.ClientProfileId;
    public string UploadKiBPerSecond { get; init; } = SimulationDefaults.UploadKiBPerSecond.ToString();
    public string DownloadKiBPerSecond { get; init; } = SimulationDefaults.DownloadKiBPerSecond.ToString();
    public bool RandomUploadEnabled { get; init; } = true;
    public string RandomUploadMinimumKiBPerSecond { get; init; } = SimulationDefaults.RandomUploadMinimumKiBPerSecond.ToString();
    public string RandomUploadMaximumKiBPerSecond { get; init; } = SimulationDefaults.RandomUploadMaximumKiBPerSecond.ToString();
    public bool RandomDownloadEnabled { get; init; } = true;
    public string RandomDownloadMinimumKiBPerSecond { get; init; } = SimulationDefaults.RandomDownloadMinimumKiBPerSecond.ToString();
    public string RandomDownloadMaximumKiBPerSecond { get; init; } = SimulationDefaults.RandomDownloadMaximumKiBPerSecond.ToString();
    public string CompletedPercent { get; init; } = SimulationDefaults.InitialCompletedPercent.ToString();
    public string ListeningPort { get; init; } = "6881";
    public string PeersRequested { get; init; } = "200";
    public string AnnounceIntervalSeconds { get; init; } = "1800";
    public int StopMode { get; init; }
    public string StopValue { get; init; } = string.Empty;
    public string ProxyAddress { get; init; } = string.Empty;
    public string ProxyUsername { get; init; } = string.Empty;
}

public sealed record XRatioSettings
{
    public const int MaxPersistedTorrents = 2048;
    public string ThemeMode { get; init; } = "Light";
    public string AccentColor { get; init; } = "Blue";
    public string Language { get; init; } = "French";
    public int ListenPort { get; init; } = 3773;
    public bool OnlyTrackerTraffic { get; init; } = true;
    public bool OnlyLocalConnections { get; init; } = true;
    public bool ProxyDebugLogging { get; init; }
    public bool StartMinimized { get; init; }
    public bool AutoStart { get; init; }
    public int MinimumPeers { get; init; } = 5;
    public double UploadPerDownloadMinimum { get; init; }
    public double UploadPerDownloadMaximum { get; init; } = 0.05;
    public double UploadPerUploadMinimum { get; init; } = 4.0;
    public double UploadPerUploadMaximum { get; init; } = 8.0;
    public double BoostKiBPerSecond { get; init; } = 15;
    public int BoostChancePercent { get; init; } = 5;
    public bool ReportDownloadAsZero { get; init; } = true;
    public bool PretendToSeed { get; init; } = true;
    public long LifetimeRuntimeSeconds { get; init; }
    public long LifetimeActualDownloaded { get; init; }
    public long LifetimeActualUploaded { get; init; }
    public long LifetimeReportedDownloaded { get; init; }
    public long LifetimeReportedUploaded { get; init; }
    public int Sessions { get; init; }
    public SimulationFormSettings SimulationForm { get; init; } = new();
    public IReadOnlyList<PersistedTorrentState> PersistedTorrents { get; init; } =
        Array.Empty<PersistedTorrentState>();

    public XRatioSettings Validate()
    {
        if (AccentColor is not ("Blue" or "Teal" or "Violet" or "Amber" or "Rose" or "Green"))
            throw new ArgumentOutOfRangeException(nameof(AccentColor));
        if (!OnlyLocalConnections)
            throw new InvalidOperationException("Remote proxy listening is disabled because it has no authentication boundary.");
        if (ListenPort is < 1 or > 65534)
            throw new ArgumentOutOfRangeException(nameof(ListenPort));
        if (MinimumPeers is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(MinimumPeers));
        if (BoostChancePercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(BoostChancePercent));
        if (UploadPerDownloadMinimum < 0 || UploadPerDownloadMaximum < 0 ||
            UploadPerUploadMinimum < 0 || UploadPerUploadMaximum < 0 ||
            BoostKiBPerSecond < 0)
            throw new ArgumentOutOfRangeException(nameof(XRatioSettings));
        if (LifetimeRuntimeSeconds < 0 || LifetimeActualDownloaded < 0 ||
            LifetimeActualUploaded < 0 || LifetimeReportedDownloaded < 0 ||
            LifetimeReportedUploaded < 0 || Sessions < 0)
            throw new ArgumentOutOfRangeException(nameof(XRatioSettings));
        if (PersistedTorrents is null)
            throw new ArgumentNullException(nameof(PersistedTorrents));
        if (PersistedTorrents.Count > MaxPersistedTorrents)
            throw new ArgumentOutOfRangeException(nameof(PersistedTorrents));
        if (SimulationForm is null)
            throw new ArgumentNullException(nameof(SimulationForm));
        if (SimulationForm.StopMode is < 0 or > 4)
            throw new ArgumentOutOfRangeException(nameof(SimulationForm.StopMode));
        if (PersistedTorrents.Any(state =>
                state is null ||
                state.ActualFirstLeft < 0 ||
                state.ActualDownloaded < 0 ||
                state.ActualUploaded < 0 ||
                state.ActualLeft < 0 ||
                state.ReportedDownloaded < 0 ||
                state.ReportedUploaded < 0 ||
                state.ReportedLeft < 0 ||
                state.CompletePeers < 0 ||
                state.IncompletePeers < 0))
            throw new ArgumentOutOfRangeException(nameof(PersistedTorrents));
        return this;
    }
}
