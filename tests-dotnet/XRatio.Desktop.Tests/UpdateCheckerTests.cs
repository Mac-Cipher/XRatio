using System.Text.Json;
using XRatio.Desktop;

namespace XRatio.Desktop.Tests;

public sealed class UpdateCheckerTests
{
    [Fact]
    public void VersionMetadata_UsesTheProductVersion()
    {
        Assert.Equal("1.0.0", AppVersion.Current);
        Assert.Equal("v1.0.0", AppVersion.Display);
    }

    [Fact]
    public void ParseRelease_IdentifiesNewerRelease()
    {
        using var document = JsonDocument.Parse("""
            {
              "tag_name": "v0.2.0",
              "name": "XRatio 0.2.0",
              "html_url": "https://github.com/Mac-Cipher/XRatio/releases/tag/v0.2.0",
              "assets": [
                {
                  "name": "XRatio-dotnet-win-x64-v0.2.0.zip",
                  "browser_download_url": "https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio-dotnet-win-x64-v0.2.0.zip"
                },
                {
                  "name": "XRatio.exe",
                  "browser_download_url": "https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe"
                },
                {
                  "name": "XRatio.exe.sha256",
                  "browser_download_url": "https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe.sha256"
                }
              ]
            }
            """);

        var result = UpdateChecker.ParseRelease("0.1.1", document.RootElement);

        Assert.True(result.IsUpdateAvailable);
        Assert.Equal("0.2.0", result.LatestVersion);
        Assert.Equal("XRatio 0.2.0", result.ReleaseName);
        Assert.Equal(
            "https://github.com/Mac-Cipher/XRatio/releases/tag/v0.2.0",
            result.ReleaseUri?.ToString());
        Assert.Equal(
            "https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio-dotnet-win-x64-v0.2.0.zip",
            result.DownloadUri?.ToString());
        Assert.Equal(
            "https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe",
            result.ExecutableDownloadUri?.ToString());
        Assert.Equal(
            "https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe.sha256",
            result.ExecutableChecksumUri?.ToString());
        Assert.Null(result.Error);
    }

    [Fact]
    public void VersionComparison_HandlesPrefixesAndCurrentRelease()
    {
        Assert.Equal("1.2.3", UpdateChecker.NormalizeTag("v1.2.3"));
        Assert.True(UpdateChecker.IsNewerVersion("v0.1.1", "0.1.2"));
        Assert.False(UpdateChecker.IsNewerVersion("0.1.1", "v0.1.1"));
        Assert.Null(UpdateChecker.NormalizeTag("preview"));
    }

    [Fact]
    public void TestUpdateOverrideCreatesVisibleNonInstallingRelease()
    {
        var result = UpdateChecker.CreateTestUpdateResult("0.1.4.2", "v9.9.9");

        Assert.NotNull(result);
        Assert.True(result.IsUpdateAvailable);
        Assert.Equal("9.9.9", result.LatestVersion);
        Assert.Equal("XRatio local update test", result.ReleaseName);
        Assert.NotNull(result.ReleaseUri);
        Assert.NotNull(result.DownloadUri);
        Assert.Null(result.ExecutableDownloadUri);
        Assert.Null(result.ExecutableChecksumUri);
    }

    [Fact]
    public async Task TestUpdateEnvironmentOverrideSkipsNetworkCheck()
    {
        var previous = Environment.GetEnvironmentVariable(
            UpdateChecker.TestUpdateVersionEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(
                UpdateChecker.TestUpdateVersionEnvironmentVariable,
                "9.9.9");

            var result = await UpdateChecker.CheckAsync("0.1.4.2");

            Assert.True(result.IsUpdateAvailable);
            Assert.Equal("9.9.9", result.LatestVersion);
            Assert.Null(result.Error);
            Assert.Null(result.ExecutableDownloadUri);
            Assert.Null(result.ExecutableChecksumUri);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                UpdateChecker.TestUpdateVersionEnvironmentVariable,
                previous);
        }
    }
}
