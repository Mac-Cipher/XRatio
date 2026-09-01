using XRatio.Desktop;

namespace XRatio.Desktop.Tests;

public sealed class UpdateInstallerTests
{
    private const string Hash = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    [Fact]
    public void ParseChecksum_AcceptsSha256sumFilenameFormat()
    {
        var parsed = UpdateInstaller.ParseChecksum($"{Hash.ToUpperInvariant()}  *XRatio.exe\n");

        Assert.Equal(Hash, parsed);
    }

    [Fact]
    public void ParseChecksum_RejectsChecksumForAnotherFile()
    {
        Assert.Throws<InvalidOperationException>(() =>
            UpdateInstaller.ParseChecksum($"{Hash}  *other.exe\n"));
    }

    [Theory]
    [InlineData("https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe", true)]
    [InlineData("https://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe.sha256", true)]
    [InlineData("https://github.com/another/repo/releases/download/v0.2.0/XRatio.exe", false)]
    [InlineData("http://github.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe", false)]
    [InlineData("https://example.com/Mac-Cipher/XRatio/releases/download/v0.2.0/XRatio.exe", false)]
    public void IsTrustedReleaseAsset_RestrictsUpdateToOfficialHttpsAssets(string value, bool expected)
    {
        Assert.Equal(expected, UpdateInstaller.IsTrustedReleaseAsset(new Uri(value)));
    }
}
