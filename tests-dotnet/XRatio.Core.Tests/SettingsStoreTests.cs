using XRatio.Core.Announcements;
using XRatio.Core.Configuration;

namespace XRatio.Core.Tests;

public sealed class SettingsStoreTests
{
    [Fact]
    public void NewSettings_EnableAnnounceSpoofingByDefault()
    {
        var settings = new XRatioSettings();

        Assert.True(settings.ReportDownloadAsZero);
        Assert.True(settings.PretendToSeed);
        Assert.Equal("Blue", settings.AccentColor);
        Assert.Equal("Color", settings.TrayIconStyle);
        Assert.True(settings.ShowTrayIcon);
    }

    [Fact]
    public void Validate_AcceptsAllSupportedAccentColors()
    {
        foreach (var accent in new[] { "Blue", "Teal", "Violet", "Amber", "Rose", "Green" })
        {
            var settings = new XRatioSettings { AccentColor = accent };
            settings.Validate();
        }
    }

    [Fact]
    public void Validate_RejectsUnknownTrayIconStyle()
    {
        var settings = new XRatioSettings { TrayIconStyle = "Unexpected" };

        Assert.Throws<ArgumentOutOfRangeException>(() => settings.Validate());
    }

    [Fact]
    public async Task SaveAndLoad_RoundTripsConfiguration()
    {
        var directory = Path.Combine(Path.GetTempPath(), "XRatio.Tests", Guid.NewGuid().ToString("N"));
        var store = new JsonSettingsStore(directory);
        var persisted = new PersistedTorrentState(
            "abc",
            "tracker.test",
            900,
            100,
            200,
            700,
            0,
            400,
            0,
            2,
            7,
            DateTimeOffset.Parse("2026-08-06T10:00:00Z"));
        var expected = new XRatioSettings
        {
            ListenPort = 48123,
            ThemeMode = "Dim",
            AccentColor = "Teal",
            TrayIconStyle = "Monochrome",
            ShowTrayIcon = false,
            PretendToSeed = true,
            SimulationForm = new SimulationFormSettings
            {
                AccountName = "BBouche75",
                ClientProfileId = "qbittorrent-5.2",
                UploadKiBPerSecond = "43210",
                DownloadKiBPerSecond = "3210",
                RandomDownloadEnabled = false,
                CompletedPercent = "37.5",
                StopMode = 4,
                StopValue = "2.75",
                ProxyAddress = "http://127.0.0.1:8080"
            },
            PersistedTorrents = [persisted]
        };
        await store.SaveAsync(expected);

        var actual = await store.LoadAsync();

        Assert.Equal(expected.ListenPort, actual.ListenPort);
        Assert.Equal(expected.ThemeMode, actual.ThemeMode);
        Assert.Equal(expected.AccentColor, actual.AccentColor);
        Assert.Equal(expected.TrayIconStyle, actual.TrayIconStyle);
        Assert.Equal(expected.ShowTrayIcon, actual.ShowTrayIcon);
        Assert.Equal(expected.PretendToSeed, actual.PretendToSeed);
        Assert.Equal(expected.SimulationForm, actual.SimulationForm);
        Assert.Equal(persisted, Assert.Single(actual.PersistedTorrents));
        Assert.True(File.Exists(store.SettingsPath));
    }

    [Fact]
    public async Task Load_WhenPrimaryAndBackupAreInvalid_FallsBackToDefaults()
    {
        var directory = Path.Combine(Path.GetTempPath(), "XRatio.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        await File.WriteAllTextAsync(Path.Combine(directory, "settings.json"), "{not-json");
        await File.WriteAllTextAsync(
            Path.Combine(directory, "settings.json.bak"),
            """{"ListenPort":70000}""");
        var store = new JsonSettingsStore(directory);

        var actual = await store.LoadAsync();

        Assert.Equal(SettingsLoadSource.Defaults, store.LastLoadSource);
        Assert.Equal(3773, actual.ListenPort);
    }

    [Fact]
    public async Task CanceledSave_DoesNotLeaveTemporaryFileOrChangePrimary()
    {
        var directory = Path.Combine(Path.GetTempPath(), "XRatio.Tests", Guid.NewGuid().ToString("N"));
        var store = new JsonSettingsStore(directory);
        var original = new XRatioSettings { ListenPort = 48123 };
        await store.SaveAsync(original);
        var before = await File.ReadAllTextAsync(store.SettingsPath);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => store.SaveAsync(original with { ListenPort = 48124 }, cancellation.Token));

        Assert.Equal(before, await File.ReadAllTextAsync(store.SettingsPath));
        Assert.False(File.Exists(store.SettingsPath + ".tmp"));
    }

    [Fact]
    public void ProfileDirectory_UsesAbsoluteExplicitOverride()
    {
        var previous = Environment.GetEnvironmentVariable("XRATIO_PROFILE_DIR");
        var relative = Path.Combine(".", "XRatio.ProfileOverride", Guid.NewGuid().ToString("N"));
        try
        {
            Environment.SetEnvironmentVariable("XRATIO_PROFILE_DIR", relative);

            Assert.Equal(Path.GetFullPath(relative), ProfileDirectory.GetDefault());
        }
        finally
        {
            Environment.SetEnvironmentVariable("XRATIO_PROFILE_DIR", previous);
        }
    }
}

