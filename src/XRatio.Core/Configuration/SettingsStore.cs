using System.Text.Json;

namespace XRatio.Core.Configuration;

public interface ISettingsStore
{
    SettingsLoadSource LastLoadSource { get; }
    Task<XRatioSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(XRatioSettings settings, CancellationToken cancellationToken = default);
}

public enum SettingsLoadSource
{
    Defaults,
    Json,
    JsonBackup,
    LegacyTcl,
    LegacyTclBackup
}

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _profileDirectory;

    public JsonSettingsStore(string profileDirectory)
    {
        _profileDirectory = profileDirectory;
        SettingsPath = Path.Combine(profileDirectory, "settings.json");
    }

    public string SettingsPath { get; }
    public SettingsLoadSource LastLoadSource { get; private set; } = SettingsLoadSource.Defaults;

    public async Task<XRatioSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(SettingsPath))
        {
            var imported = await TryImportLegacyAsync(cancellationToken);
            if (imported is not null)
            {
                await SaveAsync(imported, cancellationToken);
                return imported;
            }
            LastLoadSource = SettingsLoadSource.Defaults;
            return CreateDefaults();
        }

        var primary = await TryLoadJsonAsync(SettingsPath, cancellationToken);
        if (primary is not null)
        {
            LastLoadSource = SettingsLoadSource.Json;
            return primary;
        }

        var backup = await TryLoadJsonAsync(SettingsPath + ".bak", cancellationToken);
        if (backup is not null)
        {
            LastLoadSource = SettingsLoadSource.JsonBackup;
            return backup;
        }

        LastLoadSource = SettingsLoadSource.Defaults;
        return CreateDefaults();
    }

    private async Task<XRatioSettings?> TryImportLegacyAsync(CancellationToken cancellationToken)
    {
        var primary = Path.Combine(_profileDirectory, "settings.dat");
        var backup = primary + ".bak";
        foreach (var candidate in new[]
                 {
                     (Path: primary, Source: SettingsLoadSource.LegacyTcl),
                     (Path: backup, Source: SettingsLoadSource.LegacyTclBackup)
                 })
        {
            if (!File.Exists(candidate.Path))
                continue;
            try
            {
                var info = new FileInfo(candidate.Path);
                if (info.Length is <= 0 or > 64 * 1024)
                    continue;
                var content = await File.ReadAllTextAsync(candidate.Path, cancellationToken);
                var values = TclSettingsImporter.ParseArrayList(content);
                var settings = TclSettingsImporter.Map(values, CreateDefaults()).Validate();
                LastLoadSource = candidate.Source;
                return settings;
            }
            catch (Exception exception) when (exception is IOException or FormatException or ArgumentException)
            {
            }
        }
        return null;
    }

    private static async Task<XRatioSettings?> TryLoadJsonAsync(
        string path,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
            return null;
        try
        {
            await using var stream = File.OpenRead(path);
            return (await JsonSerializer.DeserializeAsync<XRatioSettings>(
                stream,
                JsonOptions,
                cancellationToken))?.Validate();
        }
        catch (Exception exception) when (exception is IOException or JsonException or ArgumentException)
        {
            return null;
        }
    }

    private static XRatioSettings CreateDefaults()
    {
        var configuredPort = Environment.GetEnvironmentVariable("XRATIO_LISTEN_PORT");
        return int.TryParse(configuredPort, out var port) && port is >= 1 and <= 65534
            ? new XRatioSettings { ListenPort = port }
            : new XRatioSettings();
    }

    public async Task SaveAsync(XRatioSettings settings, CancellationToken cancellationToken = default)
    {
        settings.Validate();
        var directory = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(directory);
        var temporary = SettingsPath + ".tmp";
        try
        {
            await using (var stream = new FileStream(
                             temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (File.Exists(SettingsPath))
                File.Copy(SettingsPath, SettingsPath + ".bak", overwrite: true);
            File.Move(temporary, SettingsPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }
}

public static class ProfileDirectory
{
    public static string GetDefault()
    {
        var overridePath = Environment.GetEnvironmentVariable("XRATIO_PROFILE_DIR");
        if (!string.IsNullOrWhiteSpace(overridePath))
            return Path.GetFullPath(overridePath);
        if (OperatingSystem.IsWindows())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XRatio");
        if (OperatingSystem.IsMacOS())
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Application Support", "XRatio");

        var xdg = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
        return !string.IsNullOrWhiteSpace(xdg)
            ? Path.Combine(xdg, "XRatio")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "XRatio");
    }
}

