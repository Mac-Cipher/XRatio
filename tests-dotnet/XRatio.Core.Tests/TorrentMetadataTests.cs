using System.Security.Cryptography;
using System.Text;
using XRatio.Core.Torrents;

namespace XRatio.Core.Tests;

public sealed class TorrentMetadataTests
{
    [Fact]
    public void Load_ParsesMetadataAndHashesExactInfoDictionary()
    {
        var info = "d6:lengthi1000e4:name4:demo12:piece lengthi16384e6:pieces20:xxxxxxxxxxxxxxxxxxxx7:privatei1ee";
        var torrent = $"d8:announce28:http://tracker.test/announce4:info{info}e";
        var path = Path.Combine(Path.GetTempPath(), $"xratio-{Guid.NewGuid():N}.torrent");
        try
        {
            File.WriteAllBytes(path, Encoding.ASCII.GetBytes(torrent));

            var metadata = TorrentMetadata.Load(path);

            Assert.Equal("demo", metadata.Name);
            Assert.Equal(1000, metadata.TotalSize);
            Assert.Equal(1, metadata.PieceCount);
            Assert.True(metadata.IsPrivate);
            Assert.Equal("http://tracker.test/announce", Assert.Single(metadata.Trackers).ToString().TrimEnd('/'));
            Assert.Equal(Convert.ToHexString(SHA1.HashData(Encoding.ASCII.GetBytes(info))), metadata.InfoHashHex);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_RejectsTrackerlessTorrent()
    {
        var path = Path.Combine(Path.GetTempPath(), $"xratio-{Guid.NewGuid():N}.torrent");
        try
        {
            File.WriteAllText(path, "d4:infod6:lengthi1e4:name1:x6:pieces20:xxxxxxxxxxxxxxxxxxxxee");
            Assert.Throws<TorrentParseException>(() => TorrentMetadata.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_RejectsAFlatNodeFloodWithinTheFileSizeLimit()
    {
        var path = Path.Combine(Path.GetTempPath(), $"xratio-{Guid.NewGuid():N}.torrent");
        try
        {
            File.WriteAllText(path, "l" + string.Concat(Enumerable.Repeat("i0e", 100_001)) + "e");

            var exception = Assert.Throws<TorrentParseException>(() => TorrentMetadata.Load(path));

            Assert.Contains("budget exceeded", exception.Message, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Load_IgnoresTrackerUrisContainingUserInfo()
    {
        var info = "d6:lengthi1e4:name1:x6:pieces20:xxxxxxxxxxxxxxxxxxxxe";
        var torrent = $"d8:announce36:http://user:secret@tracker.test/path4:info{info}e";
        var path = Path.Combine(Path.GetTempPath(), $"xratio-{Guid.NewGuid():N}.torrent");
        try
        {
            File.WriteAllText(path, torrent);

            Assert.Throws<TorrentParseException>(() => TorrentMetadata.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
