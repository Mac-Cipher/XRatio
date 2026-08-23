using System.Text.Json;
using System.Text.Json.Serialization;
using XRatio.Core.Torrents;

namespace XRatio.Core.Simulation;

public sealed record SavedSimulationSession
{
    public required string TorrentPath { get; init; }
    public required string Tracker { get; init; }
    public string ClientProfileId { get; init; } = SimulationDefaults.ClientProfileId;
    public long UploadBytesPerSecond { get; init; } = SimulationDefaults.UploadBytesPerSecond;
    public long DownloadBytesPerSecond { get; init; } = SimulationDefaults.DownloadBytesPerSecond;
    public int? SpeedVariationPercent { get; init; }
    public bool? RandomUploadEnabled { get; init; }
    public long? RandomUploadMinimumBytesPerSecond { get; init; }
    public long? RandomUploadMaximumBytesPerSecond { get; init; }
    public bool? RandomDownloadEnabled { get; init; }
    public long? RandomDownloadMinimumBytesPerSecond { get; init; }
    public long? RandomDownloadMaximumBytesPerSecond { get; init; }
    public double InitialCompletedPercent { get; init; } = SimulationDefaults.InitialCompletedPercent;
    public int Port { get; init; } = 6881;
    public int NumWant { get; init; } = 200;
    public int AnnounceIntervalSeconds { get; init; } = 1800;
    public long? MaximumRuntimeSeconds { get; init; }
    public long? MaximumUploadedBytes { get; init; }
    public long? MaximumDownloadedBytes { get; init; }
    public double? MaximumRatio { get; init; }
    public string? ProxyAddress { get; init; }
    public string? ProxyUsername { get; init; }

    public SimulationOptions ToOptions()
    {
        var torrent = TorrentMetadata.Load(TorrentPath);
        if (!Uri.TryCreate(Tracker, UriKind.Absolute, out var tracker))
            throw new InvalidDataException($"Invalid saved tracker URI: {Tracker}.");
        Uri? proxy = null;
        if (!string.IsNullOrWhiteSpace(ProxyAddress) && !Uri.TryCreate(ProxyAddress, UriKind.Absolute, out proxy))
            throw new InvalidDataException($"Invalid saved proxy URI: {ProxyAddress}.");
        var legacyVariation = Math.Clamp(SpeedVariationPercent ?? 0, 0, 100);
        var legacyUploadMaximum = LegacyRandomMaximum(UploadBytesPerSecond, legacyVariation);
        var legacyDownloadMaximum = LegacyRandomMaximum(DownloadBytesPerSecond, legacyVariation);
        return new SimulationOptions
        {
            Torrent = torrent,
            Tracker = tracker,
            ClientProfileId = ClientProfileId,
            UploadBytesPerSecond = UploadBytesPerSecond,
            DownloadBytesPerSecond = DownloadBytesPerSecond,
            RandomUploadEnabled = RandomUploadEnabled ?? legacyVariation > 0,
            RandomUploadMinimumBytesPerSecond = RandomUploadMinimumBytesPerSecond ?? 0,
            RandomUploadMaximumBytesPerSecond = RandomUploadMaximumBytesPerSecond ?? legacyUploadMaximum,
            RandomDownloadEnabled = RandomDownloadEnabled ?? legacyVariation > 0,
            RandomDownloadMinimumBytesPerSecond = RandomDownloadMinimumBytesPerSecond ?? 0,
            RandomDownloadMaximumBytesPerSecond = RandomDownloadMaximumBytesPerSecond ?? legacyDownloadMaximum,
            InitialCompletedPercent = InitialCompletedPercent,
            Port = Port,
            NumWant = NumWant,
            AnnounceIntervalSeconds = AnnounceIntervalSeconds,
            MaximumRuntime = MaximumRuntimeSeconds is { } seconds ? TimeSpan.FromSeconds(seconds) : null,
            MaximumUploadedBytes = MaximumUploadedBytes,
            MaximumDownloadedBytes = MaximumDownloadedBytes,
            MaximumRatio = MaximumRatio,
            Proxy = new SimulationProxyOptions { Address = proxy, Username = ProxyUsername }
        }.Validate();
    }

    public static SavedSimulationSession FromOptions(SimulationOptions options) => new()
    {
        TorrentPath = options.Torrent.SourcePath,
        Tracker = options.Tracker.ToString(),
        ClientProfileId = options.ClientProfileId,
        UploadBytesPerSecond = options.UploadBytesPerSecond,
        DownloadBytesPerSecond = options.DownloadBytesPerSecond,
        RandomUploadEnabled = options.RandomUploadEnabled,
        RandomUploadMinimumBytesPerSecond = options.RandomUploadMinimumBytesPerSecond,
        RandomUploadMaximumBytesPerSecond = options.RandomUploadMaximumBytesPerSecond,
        RandomDownloadEnabled = options.RandomDownloadEnabled,
        RandomDownloadMinimumBytesPerSecond = options.RandomDownloadMinimumBytesPerSecond,
        RandomDownloadMaximumBytesPerSecond = options.RandomDownloadMaximumBytesPerSecond,
        InitialCompletedPercent = options.InitialCompletedPercent,
        Port = options.Port,
        NumWant = options.NumWant,
        AnnounceIntervalSeconds = options.AnnounceIntervalSeconds,
        MaximumRuntimeSeconds = options.MaximumRuntime is { } runtime ? (long)runtime.TotalSeconds : null,
        MaximumUploadedBytes = options.MaximumUploadedBytes,
        MaximumDownloadedBytes = options.MaximumDownloadedBytes,
        MaximumRatio = options.MaximumRatio,
        ProxyAddress = options.Proxy.Address?.ToString(),
        ProxyUsername = options.Proxy.Username
    };

    private static long LegacyRandomMaximum(long baseline, int variationPercent) =>
        Math.Clamp(baseline, 0, SimulationOptions.MaximumTransferRateBytesPerSecond) * variationPercent / 100;
}

public sealed class SimulationSessionStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly string _path;

    public SimulationSessionStore(string profileDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileDirectory);
        Directory.CreateDirectory(profileDirectory);
        _path = Path.Combine(profileDirectory, "simulations.json");
    }

    public async Task<IReadOnlyList<SavedSimulationSession>> LoadAsync(CancellationToken cancellationToken = default)
    {
        foreach (var candidate in new[] { _path, _path + ".bak" })
        {
            if (!File.Exists(candidate))
                continue;
            try
            {
                List<SavedSimulationSession> sessions;
                await using (var stream = File.OpenRead(candidate))
                {
                    sessions = await JsonSerializer.DeserializeAsync<List<SavedSimulationSession>>(
                        stream, JsonOptions, cancellationToken).ConfigureAwait(false) ?? [];
                }
                var distinct = sessions.Distinct().ToArray();
                if (candidate.Equals(_path, StringComparison.OrdinalIgnoreCase) && distinct.Length != sessions.Count)
                    await SaveAsync(distinct, cancellationToken).ConfigureAwait(false);
                return distinct;
            }
            catch (JsonException) when (candidate.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
            }
        }
        return [];
    }

    public async Task SaveAsync(
        IReadOnlyCollection<SavedSimulationSession> sessions,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        var temporary = _path + ".tmp";
        await using (var stream = new FileStream(
            temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
        {
            await JsonSerializer.SerializeAsync(
                stream,
                sessions.Distinct().ToArray(),
                JsonOptions,
                cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        if (File.Exists(_path))
            File.Replace(temporary, _path, _path + ".bak", true);
        else
            File.Move(temporary, _path);
    }
}
