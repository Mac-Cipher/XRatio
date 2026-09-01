using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;

namespace XRatio.Desktop;

internal sealed record UpdatePreparation(
    string TargetPath,
    string StagedPath,
    string UpdaterPath,
    string ExpectedSha256,
    bool RestartMinimized);

/// <summary>
/// Downloads and applies a verified Windows release without replacing the
/// executable while it is still running. The updater is a copy of the current
/// executable, launched with a private command-line contract, so it can wait
/// for the parent process and then replace the original file.
/// </summary>
internal static class UpdateInstaller
{
    internal const string ApplyUpdateArgument = "--apply-update";

    private const string RestartMinimizedArgument = "--restart-minimized";
    private const string UpdateDirectoryName = ".xratio-update";
    private const string StagedPrefix = "xratio-staged-";
    private const string UpdaterPrefix = "xratio-updater-";
    private const string BackupPrefix = "xratio-old-";
    private const long MaximumDownloadBytes = 512L * 1024 * 1024;
    private static readonly TimeSpan ParentExitTimeout = TimeSpan.FromSeconds(45);
    private static readonly HttpClient Client = CreateClient();

    internal static bool CanAutoUpdate(string? executablePath)
    {
        if (!OperatingSystem.IsWindows() || string.IsNullOrWhiteSpace(executablePath))
            return false;

        try
        {
            var fullPath = Path.GetFullPath(executablePath);
            return File.Exists(fullPath) &&
                   string.Equals(Path.GetFileName(fullPath), "XRatio.exe", StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    internal static string? GetCurrentExecutablePath()
    {
        try
        {
            var path = Environment.ProcessPath;
            return string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    internal static async Task<UpdatePreparation> DownloadAndLaunchUpdaterAsync(
        UpdateCheckResult update,
        string executablePath,
        CancellationToken cancellationToken = default)
    {
        if (update.ExecutableDownloadUri is null || update.ExecutableChecksumUri is null)
            throw new InvalidOperationException("The release does not contain a verified Windows executable asset.");
        if (!CanAutoUpdate(executablePath))
            throw new InvalidOperationException("This launch is not a packaged Windows XRatio executable.");

        ValidateReleaseAssetUri(update.ExecutableDownloadUri);
        ValidateReleaseAssetUri(update.ExecutableChecksumUri);

        var targetPath = Path.GetFullPath(executablePath);
        var targetDirectory = Path.GetDirectoryName(targetPath) ??
                              throw new InvalidOperationException("The XRatio executable directory could not be resolved.");
        var updateDirectory = GetUpdateDirectory(targetPath);
        Directory.CreateDirectory(updateDirectory);

        var token = Guid.NewGuid().ToString("N");
        var stagedPath = Path.Combine(updateDirectory, $"{StagedPrefix}{token}.exe");
        var updaterPath = Path.Combine(updateDirectory, $"{UpdaterPrefix}{token}.exe");
        var keepArtifacts = false;

        try
        {
            var expectedSha256 = await DownloadChecksumAsync(
                update.ExecutableChecksumUri,
                cancellationToken);
            await DownloadFileAsync(
                update.ExecutableDownloadUri,
                stagedPath,
                cancellationToken);

            var actualSha256 = await ComputeSha256Async(stagedPath, cancellationToken);
            if (!string.Equals(actualSha256, expectedSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The downloaded XRatio executable failed its SHA-256 verification.");
            if (!IsPortableExecutable(stagedPath))
                throw new InvalidOperationException("The downloaded XRatio asset is not a Windows executable.");

            File.Copy(targetPath, updaterPath, overwrite: false);
            var restartMinimized = Environment.GetCommandLineArgs()
                .Any(argument => string.Equals(argument, "--minimized", StringComparison.OrdinalIgnoreCase));
            var startInfo = new ProcessStartInfo
            {
                FileName = updaterPath,
                WorkingDirectory = targetDirectory,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            startInfo.ArgumentList.Add(ApplyUpdateArgument);
            startInfo.ArgumentList.Add(Environment.ProcessId.ToString(CultureInfo.InvariantCulture));
            startInfo.ArgumentList.Add(targetPath);
            startInfo.ArgumentList.Add(stagedPath);
            startInfo.ArgumentList.Add(expectedSha256);
            if (restartMinimized)
                startInfo.ArgumentList.Add(RestartMinimizedArgument);

            using var updater = Process.Start(startInfo);
            if (updater is null)
                throw new InvalidOperationException("The XRatio update helper could not be started.");

            keepArtifacts = true;
            return new UpdatePreparation(
                targetPath,
                stagedPath,
                updaterPath,
                expectedSha256,
                restartMinimized);
        }
        finally
        {
            if (!keepArtifacts)
            {
                TryDeleteFile(stagedPath);
                TryDeleteFile(updaterPath);
            }
        }
    }

    internal static string ParseChecksum(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("The XRatio checksum asset was empty.");

        foreach (var line in content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var fields = line.Trim().Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length == 0 || !IsSha256(fields[0]))
                continue;

            if (fields.Length >= 2 &&
                string.Equals(fields[1].TrimStart('*'), "XRatio.exe", StringComparison.OrdinalIgnoreCase))
                return fields[0].ToLowerInvariant();
        }

        throw new InvalidOperationException("The XRatio checksum asset did not contain a valid XRatio.exe SHA-256.");
    }

    internal static bool IsTrustedReleaseAsset(Uri? uri)
    {
        if (uri is null || !uri.IsAbsoluteUri ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
            return false;

        return uri.AbsolutePath.StartsWith(
            "/Mac-Cipher/XRatio/releases/download/",
            StringComparison.OrdinalIgnoreCase);
    }

    internal static void ScheduleStaleArtifactCleanup(string? executablePath)
    {
        if (!CanAutoUpdate(executablePath))
            return;

        var updateDirectory = GetUpdateDirectory(Path.GetFullPath(executablePath!));
        _ = Task.Run(async () =>
        {
            // The previous helper cannot delete its own image. Retry after the
            // replacement process has had time to start and exit.
            for (var attempt = 0; attempt < 12; attempt++)
            {
                CleanupStaleArtifacts(updateDirectory);
                if (!Directory.Exists(updateDirectory))
                    return;
                try
                {
                    if (!Directory.EnumerateFileSystemEntries(updateDirectory).Any())
                    {
                        Directory.Delete(updateDirectory);
                        return;
                    }
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500 + (attempt * 500)));
            }
        });
    }

    internal static bool TryRunApplyCommand(string[] args, out int exitCode)
    {
        if (args.Length == 0 ||
            !string.Equals(args[0], ApplyUpdateArgument, StringComparison.Ordinal))
        {
            exitCode = 0;
            return false;
        }

        exitCode = ApplyUpdate(args) ? 0 : 1;
        return true;
    }

    private static async Task<string> DownloadChecksumAsync(Uri uri, CancellationToken cancellationToken)
    {
        using var response = await GetAssetAsync(uri, cancellationToken);
        if (response.Content.Headers.ContentLength is > 64 * 1024)
            throw new InvalidOperationException("The XRatio checksum asset was unexpectedly large.");

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseChecksum(content);
    }

    private static async Task DownloadFileAsync(
        Uri uri,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        using var response = await GetAssetAsync(uri, cancellationToken);
        if (response.Content.Headers.ContentLength is > MaximumDownloadBytes)
            throw new InvalidOperationException("The XRatio update exceeds the safe download limit.");

        await using var source = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var destination = new FileStream(
            destinationPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 128 * 1024,
            options: FileOptions.SequentialScan | FileOptions.WriteThrough);

        var buffer = new byte[128 * 1024];
        long total = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            total += read;
            if (total > MaximumDownloadBytes)
                throw new InvalidOperationException("The XRatio update exceeds the safe download limit.");
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        await destination.FlushAsync(cancellationToken);
    }

    private static async Task<HttpResponseMessage> GetAssetAsync(
        Uri uri,
        CancellationToken cancellationToken)
    {
        ValidateReleaseAssetUri(uri);
        var response = await Client.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            throw new InvalidOperationException($"GitHub returned HTTP {(int)response.StatusCode} for the XRatio update asset.");
        }

        return response;
    }

    private static async Task<string> ComputeSha256Async(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 128 * 1024,
            options: FileOptions.SequentialScan);
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool ApplyUpdate(string[] args)
    {
        if (!OperatingSystem.IsWindows() || args.Length < 5 ||
            !int.TryParse(args[1], NumberStyles.None, CultureInfo.InvariantCulture, out var parentPid) ||
            parentPid <= 0 || !IsSha256(args[4]))
            return false;

        string targetPath;
        string stagedPath;
        try
        {
            targetPath = Path.GetFullPath(args[2]);
            stagedPath = Path.GetFullPath(args[3]);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var updaterPath = GetCurrentExecutablePath();
        if (!IsSafeApplyPath(targetPath, stagedPath, updaterPath))
            return false;

        var restartMinimized = args.Skip(5)
            .Any(argument => string.Equals(argument, RestartMinimizedArgument, StringComparison.Ordinal));
        if (!WaitForParent(parentPid))
            return TryLaunch(targetPath, restartMinimized);

        var launchedNewVersion = false;
        string? backupPath = null;
        try
        {
            if (!File.Exists(targetPath) || !File.Exists(stagedPath) ||
                !string.Equals(
                    ComputeSha256(stagedPath),
                    args[4],
                    StringComparison.OrdinalIgnoreCase) ||
                !IsPortableExecutable(stagedPath))
                return TryLaunch(targetPath, restartMinimized);

            backupPath = Path.Combine(
                Path.GetDirectoryName(targetPath)!,
                $"{BackupPrefix}{Guid.NewGuid():N}.exe");
            ReplaceFile(targetPath, stagedPath, backupPath);

            if (!File.Exists(targetPath) ||
                !string.Equals(ComputeSha256(targetPath), args[4], StringComparison.OrdinalIgnoreCase) ||
                !IsPortableExecutable(targetPath))
                throw new InvalidOperationException("The replaced XRatio executable failed its final verification.");

            launchedNewVersion = TryLaunch(targetPath, restartMinimized);
            if (!launchedNewVersion)
                throw new InvalidOperationException("The new XRatio executable could not be started.");

            TryDeleteFile(backupPath);
            return true;
        }
        catch
        {
            if (!launchedNewVersion && backupPath is not null)
                RestoreBackup(targetPath, backupPath);
            return TryLaunch(targetPath, restartMinimized);
        }
        finally
        {
            if (launchedNewVersion && backupPath is not null)
                TryDeleteFile(backupPath);
        }
    }

    private static void ReplaceFile(string targetPath, string stagedPath, string backupPath)
    {
        try
        {
            File.Replace(stagedPath, targetPath, backupPath, ignoreMetadataErrors: true);
            return;
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (IOException)
        {
            // Some filesystems do not expose ReplaceFileW. Fall back to a
            // same-directory move with a rollback path.
        }

        File.Move(targetPath, backupPath);
        try
        {
            File.Move(stagedPath, targetPath);
        }
        catch
        {
            RestoreBackup(targetPath, backupPath);
            throw;
        }
    }

    private static void RestoreBackup(string targetPath, string backupPath)
    {
        try
        {
            if (File.Exists(targetPath))
                File.Delete(targetPath);
            if (File.Exists(backupPath))
                File.Move(backupPath, targetPath, overwrite: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static bool WaitForParent(int parentPid)
    {
        if (parentPid == Environment.ProcessId)
            return false;

        try
        {
            using var parent = Process.GetProcessById(parentPid);
            return parent.WaitForExit((int)ParentExitTimeout.TotalMilliseconds);
        }
        catch (ArgumentException)
        {
            // The parent exited between Process.Start and GetProcessById.
            return true;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
        catch (Win32Exception)
        {
            // Failing closed is important: never replace a running image.
            return false;
        }
    }

    private static bool IsSafeApplyPath(
        string targetPath,
        string stagedPath,
        string? updaterPath)
    {
        if (!string.Equals(Path.GetFileName(targetPath), "XRatio.exe", StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(targetPath) ||
            string.IsNullOrWhiteSpace(updaterPath))
            return false;

        var updateDirectory = GetUpdateDirectory(targetPath);
        return IsPathInside(updateDirectory, stagedPath) &&
               Path.GetFileName(stagedPath).StartsWith(StagedPrefix, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Path.GetExtension(stagedPath), ".exe", StringComparison.OrdinalIgnoreCase) &&
               IsPathInside(updateDirectory, updaterPath) &&
               Path.GetFileName(updaterPath).StartsWith(UpdaterPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPathInside(string directory, string path)
    {
        try
        {
            var fullDirectory = Path.GetFullPath(directory)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
                Path.DirectorySeparatorChar;
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool TryLaunch(string targetPath, bool restartMinimized)
    {
        if (!File.Exists(targetPath))
            return false;

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = targetPath,
                WorkingDirectory = Path.GetDirectoryName(targetPath)!,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            if (restartMinimized)
                startInfo.ArgumentList.Add("--minimized");
            using var process = Process.Start(startInfo);
            return process is not null;
        }
        catch (Win32Exception)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static string ComputeSha256(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(stream)).ToLowerInvariant();
    }

    private static bool IsPortableExecutable(string path)
    {
        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            return stream.ReadByte() == 'M' && stream.ReadByte() == 'Z';
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void ValidateReleaseAssetUri(Uri uri)
    {
        if (!IsTrustedReleaseAsset(uri))
            throw new InvalidOperationException("The XRatio update URL is not an official GitHub release asset.");
    }

    private static bool IsSha256(string value) =>
        value.Length == 64 && value.All(IsHexDigit);

    private static bool IsHexDigit(char value) =>
        value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

    private static string GetUpdateDirectory(string executablePath) =>
        Path.Combine(Path.GetDirectoryName(executablePath)!, UpdateDirectoryName);

    private static void CleanupStaleArtifacts(string updateDirectory)
    {
        if (!Directory.Exists(updateDirectory))
            return;

        try
        {
            foreach (var path in Directory.EnumerateFiles(updateDirectory))
            {
                var name = Path.GetFileName(path);
                if (name.StartsWith(StagedPrefix, StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith(UpdaterPrefix, StringComparison.OrdinalIgnoreCase) ||
                    name.StartsWith(BackupPrefix, StringComparison.OrdinalIgnoreCase))
                    TryDeleteFile(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static void TryDeleteFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(3)
        };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("XRatio", AppVersion.Current));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        return client;
    }
}
