using System.Diagnostics;

namespace XRatio.Desktop;

internal sealed record DetectedTorrentClient(string DisplayName, string ExecutablePath);

internal static class TorrentClientDetector
{
    private const string QbittorrentExecutable = "qbittorrent.exe";

    public static DetectedTorrentClient? Find()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        // Prefer a running instance so portable installs are detected even
        // when they are outside the usual Program Files locations.
        foreach (var process in Process.GetProcessesByName("qbittorrent"))
        {
            try
            {
                var executablePath = process.MainModule?.FileName;
                if (IsQbittorrentExecutable(executablePath))
                    return new DetectedTorrentClient("qBittorrent", executablePath!);
            }
            catch (Exception)
            {
                // The process may exit or deny MainModule access between the
                // enumeration and inspection. Continue with known locations.
            }
            finally
            {
                process.Dispose();
            }
        }

        foreach (var executablePath in CandidatePaths())
        {
            if (IsQbittorrentExecutable(executablePath))
                return new DetectedTorrentClient("qBittorrent", executablePath);
        }

        return null;
    }

    public static bool TryOpen(DetectedTorrentClient client)
    {
        if (!OperatingSystem.IsWindows() || !IsQbittorrentExecutable(client.ExecutablePath))
            return false;

        var workingDirectory = Path.GetDirectoryName(client.ExecutablePath);
        var startInfo = new ProcessStartInfo
        {
            FileName = client.ExecutablePath,
            UseShellExecute = true,
            WorkingDirectory = string.IsNullOrWhiteSpace(workingDirectory)
                ? Environment.CurrentDirectory
                : workingDirectory
        };
        return Process.Start(startInfo) is not null;
    }

    private static IEnumerable<string> CandidatePaths()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        };

        foreach (var root in roots.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            yield return Path.Combine(root, "qBittorrent", QbittorrentExecutable);
            yield return Path.Combine(root, "Programs", "qBittorrent", QbittorrentExecutable);
        }

        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(pathVariable))
            yield break;

        foreach (var directory in pathVariable.Split(Path.PathSeparator,
                     StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            yield return Path.Combine(directory, QbittorrentExecutable);
    }

    private static bool IsQbittorrentExecutable(string? path) =>
        !string.IsNullOrWhiteSpace(path) &&
        string.Equals(Path.GetFileName(path), QbittorrentExecutable, StringComparison.OrdinalIgnoreCase) &&
        File.Exists(path);
}
