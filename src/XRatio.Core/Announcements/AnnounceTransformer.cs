using System.Globalization;
using XRatio.Core.Configuration;

namespace XRatio.Core.Announcements;

public sealed class AnnounceTransformer
{
    private const int MaximumInfoHashCharacters = 256;
    private static readonly IReadOnlyDictionary<string, string> NoUpdates =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlySet<string> CompletedEventRemoval =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "event" };
    private readonly object _gate = new();
    private readonly IRandomSource _random;
    private readonly Dictionary<string, TorrentState> _torrents = new(StringComparer.Ordinal);

    public AnnounceTransformer(IRandomSource? random = null) =>
        _random = random ?? new SystemRandomSource();

    public AnnounceTransformResult Transform(
        Uri target,
        XRatioSettings settings,
        bool paused = false,
        DateTimeOffset? now = null)
    {
        settings.Validate();
        if (target.Scheme is not ("http" or "https") || target.UserInfo.Length > 0)
            return new(AnnounceDisposition.RejectedInvalid, null, "Unsupported target scheme.");

        var resource = target.PathAndQuery + target.Fragment;
        var query = QueryStringEditor.Parse(resource);
        if (!query.Contains("info_hash"))
        {
            return settings.OnlyTrackerTraffic
                ? new(AnnounceDisposition.BlockedNonTracker, null, "Blocked non-tracker traffic.")
                : new(AnnounceDisposition.Forwarded, target, "Forwarding non-tracker traffic.");
        }

        var hash = InfoHashCodec.Normalize(query.GetLast("info_hash") ?? string.Empty);
        if (hash.Length is 0 or > MaximumInfoHashCharacters)
            return new(AnnounceDisposition.RejectedInvalid, null, "Invalid info_hash.");
        var eventName = query.GetLast("event") ?? string.Empty;
        if (!TryCounter(query.GetLast("downloaded"), out var downloaded) ||
            !TryCounter(query.GetLast("uploaded"), out var uploaded) ||
            !TryCounter(query.GetLast("left"), out var left))
            return new(AnnounceDisposition.Forwarded, target, "Forwarding non-announce tracker traffic.", hash);

        lock (_gate)
        {
            var timestamp = now ?? DateTimeOffset.UtcNow;
            var state = GetState(hash);
            var restoredState = state.Restored;
            state.Tracker = target.IsDefaultPort ? target.Host : $"{target.Host}:{target.Port}";
            var actualDownDifference = state.ActualLast is null
                ? downloaded
                : Math.Max(0, downloaded - state.ActualLast.Value.Downloaded);
            var actualUpDifference = state.ActualLast is null
                ? uploaded
                : Math.Max(0, uploaded - state.ActualLast.Value.Uploaded);
            if (eventName.Equals("started", StringComparison.Ordinal) && !restoredState)
            {
                actualDownDifference = downloaded;
                actualUpDifference = uploaded;
            }

            state.ActualFirst ??= new Counters(downloaded, uploaded, left);
            if (state.ActualLast is not null && !eventName.Equals("started", StringComparison.Ordinal))
            {
                state.ActualDownloadedTotal = SaturatingAdd(
                    state.ActualDownloadedTotal,
                    actualDownDifference);
                state.ActualUploadedTotal = SaturatingAdd(
                    state.ActualUploadedTotal,
                    actualUpDifference);
            }
            state.ActualLast = new Counters(downloaded, uploaded, left);

            var reportedPrevious = eventName.Equals("started", StringComparison.Ordinal) && !restoredState
                ? null
                : state.ReportedLast;
            var previousUpload = reportedPrevious?.Uploaded ?? 0;
            var previousDownload = reportedPrevious?.Downloaded ?? 0;
            var elapsedSeconds = state.ReportedAt is null ||
                                 eventName.Equals("started", StringComparison.Ordinal) ||
                                 restoredState
                ? 0
                : Math.Max(0, (timestamp - state.ReportedAt.Value).TotalSeconds);

            var rewrittenResource = resource;
            if (paused)
            {
                uploaded = Math.Max(uploaded, previousUpload);
                downloaded = Math.Max(downloaded, previousDownload);
            }
            else
            {
                if (settings.ReportDownloadAsZero)
                {
                    downloaded = 0;
                    left = state.ActualFirst!.Value.Left;
                    if (eventName.Equals("completed", StringComparison.Ordinal))
                    {
                        rewrittenResource = QueryStringEditor.Parse(rewrittenResource).Rewrite(
                            NoUpdates,
                            CompletedEventRemoval);
                    }
                }

                // Only advertise a torrent as seeded when qBittorrent's last
                // real announce says it has no bytes left. Applying left=0 to
                // an active download hides the remaining work from the
                // tracker, which can make it return no peers and stall the
                // download.
                if (settings.PretendToSeed && state.ActualLast is { Left: 0 })
                    left = 0;

                if (state.IncompletePeers >= settings.MinimumPeers)
                {
                    var downRatio = BetweenLegacyBounds(
                        settings.UploadPerDownloadMaximum,
                        settings.UploadPerDownloadMinimum);
                    var upRatio = BetweenLegacyBounds(
                        settings.UploadPerUploadMaximum,
                        settings.UploadPerUploadMinimum);
                    var calculated = (double)previousUpload + actualUpDifference +
                                     downRatio * actualDownDifference +
                                     upRatio * actualUpDifference;
                    if (_random.NextDouble() * 100 < settings.BoostChancePercent)
                    {
                        calculated += settings.BoostKiBPerSecond * 1024 *
                                      elapsedSeconds * _random.NextDouble();
                    }
                    uploaded = RoundCounter(calculated);
                }
                else
                {
                    uploaded = SaturatingAdd(previousUpload, actualUpDifference);
                }
            }

            if (!eventName.Equals("started", StringComparison.Ordinal) && uploaded < previousUpload)
            {
                return new(
                    AnnounceDisposition.RejectedInvalid,
                    null,
                    "Upload regression rejected to preserve tracker consistency.",
                    hash);
            }

            rewrittenResource = QueryStringEditor.Parse(rewrittenResource).RewriteCounters(
                downloaded.ToString(CultureInfo.InvariantCulture),
                uploaded.ToString(CultureInfo.InvariantCulture),
                left.ToString(CultureInfo.InvariantCulture));
            var builder = new UriBuilder(target)
            {
                Path = ExtractPath(rewrittenResource),
                Query = ExtractQuery(rewrittenResource),
                Fragment = ExtractFragment(rewrittenResource)
            };

            if (state.ReportedLast is not null && !eventName.Equals("started", StringComparison.Ordinal))
            {
                state.ReportedDownloadedTotal = SaturatingAdd(
                    state.ReportedDownloadedTotal,
                    Math.Max(0, downloaded - state.ReportedLast.Value.Downloaded));
                state.ReportedUploadedTotal = SaturatingAdd(
                    state.ReportedUploadedTotal,
                    Math.Max(0, uploaded - state.ReportedLast.Value.Uploaded));
            }
            state.ReportedLast = new Counters(downloaded, uploaded, left);
            state.ReportedAt = timestamp;
            state.Restored = false;
            return new(AnnounceDisposition.Rewritten, builder.Uri, "Announce statistics rewritten.", hash);
        }
    }

    public void ObserveTrackerResponse(string infoHash, TrackerResponse response)
    {
        infoHash = InfoHashCodec.Normalize(infoHash);
        lock (_gate)
        {
            var state = GetState(infoHash);
            if (response.Complete is not null)
                state.CompletePeers = response.Complete.Value;
            if (response.Incomplete is not null)
                state.IncompletePeers = response.Incomplete.Value;
        }
    }

    public bool ResetTorrent(string infoHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(infoHash);
        infoHash = InfoHashCodec.Normalize(infoHash);
        lock (_gate)
            return _torrents.Remove(infoHash);
    }

    public bool HasActiveDownloads
    {
        get
        {
            lock (_gate)
            {
                foreach (var state in _torrents.Values)
                {
                    if (state.ActualLast is { Left: > 0 })
                        return true;
                }

                return false;
            }
        }
    }

    public bool IsSeedOnlyStandby
    {
        get
        {
            lock (_gate)
            {
                if (_torrents.Count == 0)
                    return false;
                foreach (var state in _torrents.Values)
                {
                    if (state.ActualLast is { Left: > 0 })
                        return false;
                }

                return true;
            }
        }
    }

    public void Restore(IEnumerable<PersistedTorrentState> states)
    {
        ArgumentNullException.ThrowIfNull(states);
        lock (_gate)
        {
            _torrents.Clear();
            var count = 0;
            foreach (var persisted in states)
            {
                if (count++ >= XRatioSettings.MaxPersistedTorrents)
                    break;
                if (persisted is null || string.IsNullOrWhiteSpace(persisted.InfoHash) ||
                    persisted.InfoHash.Length > MaximumInfoHashCharacters)
                    continue;

                var infoHash = InfoHashCodec.Normalize(persisted.InfoHash);
                _torrents[infoHash] = new TorrentState
                {
                    Tracker = persisted.Tracker,
                    ActualFirst = new Counters(0, 0, persisted.ActualFirstLeft),
                    ActualLast = new Counters(
                        persisted.ActualDownloaded,
                        persisted.ActualUploaded,
                        persisted.ActualLeft),
                    ReportedLast = new Counters(
                        persisted.ReportedDownloaded,
                        persisted.ReportedUploaded,
                        persisted.ReportedLeft),
                    ReportedAt = persisted.LastAnnounce,
                    CompletePeers = persisted.CompletePeers,
                    IncompletePeers = persisted.IncompletePeers,
                    Restored = true
                };
            }
        }
    }

    public IReadOnlyList<PersistedTorrentState> GetPersistedStates()
    {
        lock (_gate)
        {
            var result = new List<PersistedTorrentState>(_torrents.Count);
            foreach (var item in _torrents)
            {
                var state = item.Value;
                if (state.ActualLast is not { } actual || state.ReportedLast is not { } reported)
                    continue;
                var actualFirstLeft = state.ActualFirst is { } first ? first.Left : actual.Left;
                result.Add(new PersistedTorrentState(
                    item.Key,
                    state.Tracker,
                    actualFirstLeft,
                    actual.Downloaded,
                    actual.Uploaded,
                    actual.Left,
                    reported.Downloaded,
                    reported.Uploaded,
                    reported.Left,
                    state.CompletePeers,
                    state.IncompletePeers,
                    state.ReportedAt));
            }

            return result.ToArray();
        }
    }

    public IReadOnlyList<TorrentSnapshot> GetSnapshots()
    {
        lock (_gate)
        {
            var result = new List<TorrentSnapshot>(_torrents.Count);
            foreach (var item in _torrents)
            {
                var state = item.Value;
                var actual = state.ActualLast is { } actualValue ? actualValue : default;
                var reported = state.ReportedLast is { } reportedValue ? reportedValue : default;
                result.Add(new TorrentSnapshot(
                    item.Key,
                    state.Tracker,
                    actual.Downloaded,
                    actual.Uploaded,
                    actual.Left,
                    reported.Downloaded,
                    reported.Uploaded,
                    reported.Left,
                    state.ActualDownloadedTotal,
                    state.ActualUploadedTotal,
                    state.ReportedDownloadedTotal,
                    state.ReportedUploadedTotal,
                    state.CompletePeers,
                    state.IncompletePeers,
                    state.ReportedAt));
            }

            return result
                .OrderByDescending(snapshot => snapshot.LastAnnounce)
                .ToArray();
        }
    }

    private static bool TryCounter(string? value, out long result) =>
        long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) && result >= 0;

    private static long SaturatingAdd(long current, long increment)
    {
        if (increment <= 0)
            return current;
        if (current >= long.MaxValue - increment)
            return long.MaxValue;
        return current + increment;
    }

    private static long RoundCounter(double value)
    {
        if (double.IsNaN(value) || value <= 0)
            return 0;
        if (value >= long.MaxValue)
            return long.MaxValue;

        var rounded = Math.Round(value, MidpointRounding.ToEven);
        return rounded >= long.MaxValue ? long.MaxValue : (long)rounded;
    }

    private double BetweenLegacyBounds(double lowerExpression, double upperExpression) =>
        lowerExpression + _random.NextDouble() * (upperExpression - lowerExpression);

    private TorrentState GetState(string hash)
    {
        if (!_torrents.TryGetValue(hash, out var state))
        {
            if (_torrents.Count >= XRatioSettings.MaxPersistedTorrents)
            {
                string? oldestKey = null;
                var oldestTimestamp = DateTimeOffset.MaxValue;
                foreach (var item in _torrents)
                {
                    var timestamp = item.Value.ReportedAt ?? DateTimeOffset.MinValue;
                    if (oldestKey is null || timestamp < oldestTimestamp)
                    {
                        oldestKey = item.Key;
                        oldestTimestamp = timestamp;
                    }
                }

                if (oldestKey is not null)
                    _torrents.Remove(oldestKey);
            }
            _torrents.Add(hash, state = new TorrentState());
        }
        return state;
    }

    private static string ExtractQuery(string resource)
    {
        var query = resource.IndexOf('?');
        if (query < 0)
            return string.Empty;
        var fragment = resource.IndexOf('#', query);
        return fragment < 0 ? resource[(query + 1)..] : resource[(query + 1)..fragment];
    }

    private static string ExtractPath(string resource)
    {
        var query = resource.IndexOf('?');
        var fragment = resource.IndexOf('#');
        var end = query < 0 ? fragment : fragment < 0 ? query : Math.Min(query, fragment);
        return end < 0 ? resource : resource[..end];
    }

    private static string ExtractFragment(string resource)
    {
        var fragment = resource.IndexOf('#');
        return fragment < 0 ? string.Empty : resource[(fragment + 1)..];
    }

    private sealed class TorrentState
    {
        public string Tracker { get; set; } = string.Empty;
        public Counters? ActualFirst { get; set; }
        public Counters? ActualLast { get; set; }
        public Counters? ReportedLast { get; set; }
        public DateTimeOffset? ReportedAt { get; set; }
        public int IncompletePeers { get; set; }
        public long ActualDownloadedTotal { get; set; }
        public long ActualUploadedTotal { get; set; }
        public long ReportedDownloadedTotal { get; set; }
        public long ReportedUploadedTotal { get; set; }
        public int CompletePeers { get; set; }
        public bool Restored { get; set; }
    }

    private readonly record struct Counters(long Downloaded, long Uploaded, long Left);
}
