using XRatio.Core.Torrents;

namespace XRatio.Core.Simulation;

public static class SimulationDefaults
{
    public const string ClientProfileId = "qbittorrent-5.2";
    public const int UploadKiBPerSecond = 10_000;
    public const int DownloadKiBPerSecond = 2_500;
    public const int RandomUploadMinimumKiBPerSecond = 5_000;
    public const int RandomUploadMaximumKiBPerSecond = 15_000;
    public const int RandomDownloadMinimumKiBPerSecond = 1_000;
    public const int RandomDownloadMaximumKiBPerSecond = 5_000;
    public const long UploadBytesPerSecond = UploadKiBPerSecond * 1024L;
    public const long DownloadBytesPerSecond = DownloadKiBPerSecond * 1024L;
    public const long RandomUploadMinimumBytesPerSecond = RandomUploadMinimumKiBPerSecond * 1024L;
    public const long RandomUploadMaximumBytesPerSecond = RandomUploadMaximumKiBPerSecond * 1024L;
    public const long RandomDownloadMinimumBytesPerSecond = RandomDownloadMinimumKiBPerSecond * 1024L;
    public const long RandomDownloadMaximumBytesPerSecond = RandomDownloadMaximumKiBPerSecond * 1024L;
    public const double InitialCompletedPercent = 0;
}

public enum SimulationState
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Faulted
}

public enum TrackerEvent
{
    None,
    Started,
    Completed,
    Stopped
}

public sealed record SimulationOptions
{
    public const long MaximumTransferRateBytesPerSecond = 1024L * 1024 * 1024 * 1024;

    public required TorrentMetadata Torrent { get; init; }
    public required Uri Tracker { get; init; }
    // Optional local label for the account used on the tracker. It is never
    // included in tracker announces or sent over the network.
    public string? AccountName { get; init; }
    public string ClientProfileId { get; init; } = SimulationDefaults.ClientProfileId;
    public long UploadBytesPerSecond { get; init; } = SimulationDefaults.UploadBytesPerSecond;
    public long DownloadBytesPerSecond { get; init; } = SimulationDefaults.DownloadBytesPerSecond;
    public bool RandomUploadEnabled { get; init; } = true;
    public long RandomUploadMinimumBytesPerSecond { get; init; } = SimulationDefaults.RandomUploadMinimumBytesPerSecond;
    public long RandomUploadMaximumBytesPerSecond { get; init; } = SimulationDefaults.RandomUploadMaximumBytesPerSecond;
    public bool RandomDownloadEnabled { get; init; } = true;
    public long RandomDownloadMinimumBytesPerSecond { get; init; } = SimulationDefaults.RandomDownloadMinimumBytesPerSecond;
    public long RandomDownloadMaximumBytesPerSecond { get; init; } = SimulationDefaults.RandomDownloadMaximumBytesPerSecond;
    public double InitialCompletedPercent { get; init; } = SimulationDefaults.InitialCompletedPercent;
    public int Port { get; init; } = 6881;
    public int NumWant { get; init; } = 200;
    public int AnnounceIntervalSeconds { get; init; } = 1800;
    public TimeSpan? MaximumRuntime { get; init; }
    public long? MaximumUploadedBytes { get; init; }
    public long? MaximumDownloadedBytes { get; init; }
    public double? MaximumRatio { get; init; }
    public SimulationProxyOptions Proxy { get; init; } = new();

    public SimulationOptions Validate()
    {
        ArgumentNullException.ThrowIfNull(Torrent);
        ArgumentNullException.ThrowIfNull(Tracker);
        if (AccountName is { Length: > 128 })
            throw new ArgumentOutOfRangeException(nameof(AccountName));
        ClientProfileCatalog.Get(ClientProfileId);
        if (!Torrent.Trackers.Contains(Tracker))
            throw new ArgumentException("The selected tracker does not belong to the torrent.", nameof(Tracker));
        ValidateRate(UploadBytesPerSecond, nameof(UploadBytesPerSecond));
        ValidateRate(DownloadBytesPerSecond, nameof(DownloadBytesPerSecond));
        ValidateRandomRange(
            RandomUploadMinimumBytesPerSecond,
            RandomUploadMaximumBytesPerSecond,
            nameof(RandomUploadMinimumBytesPerSecond));
        ValidateRandomRange(
            RandomDownloadMinimumBytesPerSecond,
            RandomDownloadMaximumBytesPerSecond,
            nameof(RandomDownloadMinimumBytesPerSecond));
        if (!double.IsFinite(InitialCompletedPercent) || InitialCompletedPercent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(InitialCompletedPercent));
        if (Port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(Port));
        if (NumWant is < 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(NumWant));
        if (AnnounceIntervalSeconds is < 30 or > 86400)
            throw new ArgumentOutOfRangeException(nameof(AnnounceIntervalSeconds));
        if (MaximumRuntime is { } runtime && runtime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(MaximumRuntime));
        if (MaximumUploadedBytes is < 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumUploadedBytes));
        if (MaximumDownloadedBytes is < 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumDownloadedBytes));
        if (MaximumRatio is { } ratio && (!double.IsFinite(ratio) || ratio < 0))
            throw new ArgumentOutOfRangeException(nameof(MaximumRatio));
        Proxy.Validate();
        return this;
    }

    private static void ValidateRandomRange(long minimum, long maximum, string parameterName)
    {
        if (minimum < 0 || maximum < minimum || maximum > MaximumTransferRateBytesPerSecond)
            throw new ArgumentOutOfRangeException(parameterName, "Random speed bounds must satisfy 0 <= minimum <= maximum.");
    }

    private static void ValidateRate(long rate, string parameterName)
    {
        if (rate is < 0 or > MaximumTransferRateBytesPerSecond)
            throw new ArgumentOutOfRangeException(parameterName);
    }
}

public sealed record SimulationProxyOptions
{
    public Uri? Address { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }

    public void Validate()
    {
        if (Address is null)
            return;
        if (!Address.IsAbsoluteUri || Address.Scheme is not ("http" or "https" or "socks4" or "socks4a" or "socks5"))
            throw new ArgumentException("Proxy scheme must be HTTP, HTTPS, SOCKS4, SOCKS4A or SOCKS5.", nameof(Address));
        if (!string.IsNullOrEmpty(Address.UserInfo))
            throw new ArgumentException("Proxy credentials must not be embedded in the proxy address.", nameof(Address));
    }
}

public sealed record SimulationSnapshot(
    Guid Id,
    string Name,
    string InfoHash,
    string Tracker,
    string Client,
    SimulationState State,
    long Uploaded,
    long Downloaded,
    long Left,
    long UploadRate,
    long DownloadRate,
    int Seeders,
    int Leechers,
    TimeSpan Runtime,
    DateTimeOffset? NextAnnounce,
    string? LastError)
{
    public string? AccountName { get; init; }

    public string TrackerName => FormatTrackerName(Tracker);

    public double Ratio => Downloaded == 0 ? Uploaded > 0 ? double.PositiveInfinity : 0 : (double)Uploaded / Downloaded;

    private static string FormatTrackerName(string tracker)
    {
        if (!Uri.TryCreate(tracker, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host))
            return tracker;
        return uri.IsDefaultPort || uri.Port < 0
            ? uri.Host
            : $"{uri.Host}:{uri.Port}";
    }

    public double CompletionPercent
    {
        get
        {
            var total = Downloaded + Left;
            return total <= 0 ? 0 : Math.Clamp(Downloaded * 100d / total, 0, 100);
        }
    }
}

public sealed record TrackerAnnounce(
    Uri Tracker,
    string InfoHashHex,
    string PeerId,
    int Port,
    long Uploaded,
    long Downloaded,
    long Left,
    int NumWant,
    string Key,
    TrackerEvent Event,
    ClientProfile Client,
    SimulationProxyOptions Proxy);

public sealed record TrackerAnnounceResult(int IntervalSeconds, int Seeders, int Leechers, string? Warning = null);
