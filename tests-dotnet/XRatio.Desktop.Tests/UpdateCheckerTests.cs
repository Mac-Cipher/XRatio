using System.Text.Json;
using XRatio.Desktop;

namespace XRatio.Desktop.Tests;

public sealed class UpdateCheckerTests
{
    [Fact]
    public void VersionMetadata_UsesTheProductVersion()
    {
        Assert.Equal("0.1.4.2", AppVersion.Current);
        Assert.Equal("v0.1.4.2", AppVersion.Display);
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
}
