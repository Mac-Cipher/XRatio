using System.Text.RegularExpressions;

namespace XRatio.Proxy;

public interface IProxyDebugLogger
{
    void Write(string message);
}

/// <summary>
/// Redacts tracker credentials and token-like path segments before a message
/// can reach a diagnostic sink.
/// </summary>
public static partial class ProxyDebugRedactor
{
    [GeneratedRegex(
        "(?<key>info_hash|passkey|authkey|token|access_token|api_key|apikey|secret|password|passwd|peer_id|key|ip|ipv4|ipv6)=(?<value>[^&\\s]*)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex SensitiveQueryPattern();

    [GeneratedRegex(
        "(?<prefix>/)[A-Za-z0-9._~-]{20,}(?=(/|[?\\s]|$))",
        RegexOptions.CultureInvariant)]
    private static partial Regex SensitivePathPattern();

    public static string RedactSensitive(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (text.IndexOf('=') < 0 && text.IndexOf('/') < 0)
            return text;
        var redacted = SensitiveQueryPattern().Replace(text, "${key}=<redacted>");
        return SensitivePathPattern().Replace(redacted, "/<redacted>");
    }
}

/// <summary>
/// Best-effort rotating file logger with bounded retention. Diagnostics must
/// never interrupt proxy traffic, so all filesystem failures are intentionally
/// swallowed.
/// </summary>
public sealed class FileProxyDebugLogger : IProxyDebugLogger
{
    private const long MaximumBytes = 1024 * 1024;
    private static readonly TimeSpan MaximumAge = TimeSpan.FromDays(7);
    private static readonly TimeSpan PurgeInterval = TimeSpan.FromHours(1);
    private readonly string _path;
    private readonly string _backupPath;
    private readonly string? _directory;
    private readonly object _gate = new();
    private DateTimeOffset _nextPurgeUtc;

    public FileProxyDebugLogger(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = Path.GetFullPath(path);
        _backupPath = _path + ".1";
        _directory = Path.GetDirectoryName(_path);
        var now = DateTimeOffset.UtcNow;
        PurgeExpiredFiles(now);
        _nextPurgeUtc = now + PurgeInterval;
    }

    public void Write(string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        try
        {
            lock (_gate)
            {
                if (string.IsNullOrWhiteSpace(_directory))
                    return;
                Directory.CreateDirectory(_directory);
                var now = DateTimeOffset.UtcNow;
                if (now >= _nextPurgeUtc)
                {
                    PurgeExpiredFiles(now);
                    _nextPurgeUtc = now + PurgeInterval;
                }
                if (File.Exists(_path) && new FileInfo(_path).Length >= MaximumBytes)
                    File.Move(_path, _backupPath, overwrite: true);

                var line = $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}: " +
                           ProxyDebugRedactor.RedactSensitive(message) +
                           Environment.NewLine;
                File.AppendAllText(_path, line);
            }
        }
        catch (Exception)
        {
            // Debug logging is strictly optional and must never disrupt proxy
            // traffic because a profile directory can be read-only or locked.
        }
    }

    private void PurgeExpiredFiles(DateTimeOffset now)
    {
        DeleteIfExpired(_path, now);
        DeleteIfExpired(_backupPath, now);
    }

    private static void DeleteIfExpired(string path, DateTimeOffset now)
    {
        try
        {
            if (!File.Exists(path))
                return;

            var lastWriteUtc = File.GetLastWriteTimeUtc(path);
            if (now.UtcDateTime - lastWriteUtc >= MaximumAge)
                File.Delete(path);
        }
        catch (Exception)
        {
            // A stale diagnostic file is best-effort cleanup only.
        }
    }
}

