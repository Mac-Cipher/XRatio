using System.Security;
using XRatio.Core.Torrents;

namespace XRatio.Desktop;

/// <summary>
/// Resolves the human-facing name for intercepted announces from the local
/// torrent metadata retained by qBittorrent. Tracker announces carry only the
/// binary info-hash, so the client metadata is the reliable source of names.
/// </summary>
internal sealed class TorrentNameCatalog
{
    private const int MaximumTorrentFiles = 4096;
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromSeconds(2);
    private readonly object _gate = new();
    private readonly IReadOnlyList<string> _directories;
    private readonly Dictionary<string, CachedIdentity> _files = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _names = new(StringComparer.OrdinalIgnoreCase);
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

    public TorrentNameCatalog(IEnumerable<string>? directories = null)
    {
        _directories = (directories ?? GetDefaultDirectories())
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public string? Resolve(string infoHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(infoHash);
        lock (_gate)
            return _names.TryGetValue(infoHash, out var name) ? name : null;
    }

    public void Refresh(bool force = false)
    {
        var now = DateTimeOffset.UtcNow;
        lock (_gate)
        {
            if (!force && now - _lastRefresh < RefreshInterval)
                return;
            _lastRefresh = now;
        }

        var currentFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nextNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in EnumerateTorrentFiles())
        {
            currentFiles.Add(path);
            var stamp = ReadStamp(path);
            CachedIdentity identity;
            lock (_gate)
            {
                if (_files.TryGetValue(path, out var cached) && cached.Matches(stamp))
                {
                    identity = cached;
                }
                else
                {
                    identity = LoadIdentity(path, stamp);
                    _files[path] = identity;
                }
            }

            if (identity.Name is not null && identity.InfoHashHex is not null)
                nextNames[identity.InfoHashHex] = identity.Name;
        }

        lock (_gate)
        {
            foreach (var stale in _files.Keys.Where(path => !currentFiles.Contains(path)).ToArray())
                _files.Remove(stale);
            _names = nextNames;
        }
    }

    private IEnumerable<string> EnumerateTorrentFiles()
    {
        var count = 0;
        foreach (var directory in _directories)
        {
            string[] files;
            try
            {
                files = Directory.EnumerateFiles(directory, "*.torrent", SearchOption.TopDirectoryOnly)
                    .Take(MaximumTorrentFiles - count)
                    .ToArray();
            }
            catch (Exception exception) when (IsFilesystemEnumerationFailure(exception))
            {
                continue;
            }

            foreach (var path in files)
            {
                if (++count > MaximumTorrentFiles)
                    yield break;
                yield return path;
            }
        }
    }

    private static CachedIdentity LoadIdentity(string path, FileStamp stamp)
    {
        try
        {
            return TorrentMetadata.TryLoadIdentity(path, out var identity) && identity is not null
                ? new CachedIdentity(stamp, identity.Name.Trim(), identity.InfoHashHex)
                : new CachedIdentity(stamp, null, null);
        }
        catch (Exception exception) when (IsFilesystemEnumerationFailure(exception) ||
                                          exception is InvalidDataException)
        {
            return new CachedIdentity(stamp, null, null);
        }
    }

    private static FileStamp ReadStamp(string path)
    {
        try
        {
            var file = new FileInfo(path);
            return new FileStamp(file.Exists, file.Length, file.LastWriteTimeUtc);
        }
        catch (Exception exception) when (IsFilesystemEnumerationFailure(exception))
        {
            return new FileStamp(false, 0, DateTime.MinValue);
        }
    }

    private static bool IsFilesystemEnumerationFailure(Exception exception) =>
        exception is IOException or UnauthorizedAccessException or SecurityException or
        ArgumentException or NotSupportedException;

    private static IEnumerable<string> GetDefaultDirectories()
    {
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(local))
            yield return Path.Combine(local, "qBittorrent", "BT_backup");

        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (!string.IsNullOrWhiteSpace(roaming))
            yield return Path.Combine(roaming, "qBittorrent", "BT_backup");

        // Keep the catalogue useful for qBittorrent's standard Linux profile
        // when the same desktop surface is run outside Windows.
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrWhiteSpace(userProfile))
            yield return Path.Combine(userProfile, ".local", "share", "qBittorrent", "BT_backup");
    }

    private sealed record CachedIdentity(FileStamp Stamp, string? Name, string? InfoHashHex)
    {
        public bool Matches(FileStamp stamp) => Stamp == stamp;
    }

    private readonly record struct FileStamp(bool Exists, long Length, DateTime LastWriteTimeUtc);
}
