using System.Security.Cryptography;
using System.Text;

namespace XRatio.Core.Torrents;

public sealed record TorrentMetadata(
    string SourcePath,
    string Name,
    string InfoHashHex,
    long TotalSize,
    int PieceCount,
    bool IsPrivate,
    IReadOnlyList<Uri> Trackers)
{
    public const int MaxTorrentFileBytes = 16 * 1024 * 1024;
    public const int MaxTrackers = 256;

    /// <summary>
    /// Reads only the display identity from a torrent file. qBittorrent can
    /// retain metadata without an announce URL (for example magnet-derived
    /// torrents), so this deliberately does not require a usable tracker or
    /// a complete simulation profile.
    /// </summary>
    public static bool TryLoadIdentity(string path, out TorrentIdentity? identity)
    {
        identity = null;
        try
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            var fullPath = Path.GetFullPath(path);
            var file = new FileInfo(fullPath);
            if (!file.Exists || file.Length <= 0 || file.Length > MaxTorrentFileBytes)
                return false;

            var bytes = File.ReadAllBytes(fullPath);
            if (new BencodeReader(bytes).ReadRoot() is not BencodeDictionary root ||
                !root.Values.TryGetValue("info", out var infoNode) ||
                infoNode is not BencodeDictionary info)
                return false;

            var name = ReadText(info, "name.utf-8") ?? ReadText(info, "name");
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var infoBytes = bytes.AsSpan(info.StartOffset, info.EndOffset - info.StartOffset);
            identity = new TorrentIdentity(
                fullPath,
                name,
                Convert.ToHexString(SHA1.HashData(infoBytes)));
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or
                                          ArgumentException or OverflowException or TorrentParseException)
        {
            return false;
        }
    }

    public static TorrentMetadata Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        var file = new FileInfo(fullPath);
        if (!file.Exists)
            throw new FileNotFoundException("Torrent file was not found.", fullPath);
        if (file.Length <= 0 || file.Length > MaxTorrentFileBytes)
            throw new TorrentParseException($"Torrent files must be between 1 byte and {MaxTorrentFileBytes} bytes.");

        var bytes = File.ReadAllBytes(fullPath);
        if (new BencodeReader(bytes).ReadRoot() is not BencodeDictionary root)
            throw new TorrentParseException("The torrent root must be a bencoded dictionary.");
        if (!root.Values.TryGetValue("info", out var infoNode) || infoNode is not BencodeDictionary info)
            throw new TorrentParseException("The torrent has no info dictionary.");

        var trackers = ReadTrackers(root);
        if (trackers.Count == 0)
            throw new TorrentParseException("The torrent has no supported HTTP or HTTPS tracker.");

        var name = ReadText(info, "name.utf-8") ?? ReadText(info, "name");
        if (string.IsNullOrWhiteSpace(name))
            throw new TorrentParseException("The torrent has no name.");

        var totalSize = ReadTotalSize(info);
        if (totalSize <= 0)
            throw new TorrentParseException("The torrent size must be positive.");

        var pieces = ReadBytes(info, "pieces") ?? throw new TorrentParseException("The torrent has no pieces field.");
        if (pieces.Length == 0 || pieces.Length % 20 != 0)
            throw new TorrentParseException("The torrent pieces field is invalid.");

        var infoBytes = bytes.AsSpan(info.StartOffset, info.EndOffset - info.StartOffset);
        var hash = Convert.ToHexString(SHA1.HashData(infoBytes));
        var isPrivate = ReadInteger(info, "private") == 1;
        return new TorrentMetadata(fullPath, name, hash, totalSize, pieces.Length / 20, isPrivate, trackers);
    }

    private static IReadOnlyList<Uri> ReadTrackers(BencodeDictionary root)
    {
        var result = new List<Uri>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (root.Values.TryGetValue("announce-list", out var announceList) && announceList is BencodeList tiers)
        {
            foreach (var tier in tiers.Values.OfType<BencodeList>())
                foreach (var value in tier.Values.OfType<BencodeString>())
                    AddTracker(result, seen, Decode(value.Value));
        }
        if (root.Values.TryGetValue("announce", out var announce) && announce is BencodeString primary)
            AddTracker(result, seen, Decode(primary.Value));
        return result;
    }

    private static void AddTracker(List<Uri> trackers, HashSet<string> seen, string candidate)
    {
        if (trackers.Count >= MaxTrackers)
            throw new TorrentParseException($"A torrent cannot contain more than {MaxTrackers} trackers.");
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            uri.UserInfo.Length > 0)
            return;
        if (seen.Add(uri.AbsoluteUri))
            trackers.Add(uri);
    }

    private static long ReadTotalSize(BencodeDictionary info)
    {
        var single = ReadInteger(info, "length");
        if (single is not null)
            return single.Value;
        if (!info.Values.TryGetValue("files", out var filesNode) || filesNode is not BencodeList files)
            throw new TorrentParseException("The torrent has neither length nor files.");

        long total = 0;
        foreach (var file in files.Values.OfType<BencodeDictionary>())
        {
            var length = ReadInteger(file, "length") ?? throw new TorrentParseException("A torrent file entry has no length.");
            if (length < 0)
                throw new TorrentParseException("A torrent file length cannot be negative.");
            total = checked(total + length);
        }
        return total;
    }

    private static string? ReadText(BencodeDictionary dictionary, string key) =>
        ReadBytes(dictionary, key) is { } bytes ? Decode(bytes) : null;

    private static byte[]? ReadBytes(BencodeDictionary dictionary, string key) =>
        dictionary.Values.TryGetValue(key, out var node) && node is BencodeString value ? value.Value : null;

    private static long? ReadInteger(BencodeDictionary dictionary, string key) =>
        dictionary.Values.TryGetValue(key, out var node) && node is BencodeInteger value ? value.Value : null;

    private static string Decode(byte[] value)
    {
        try
        {
            return new UTF8Encoding(false, true).GetString(value);
        }
        catch (DecoderFallbackException)
        {
            return Encoding.Latin1.GetString(value);
        }
    }
}

public sealed record TorrentIdentity(string SourcePath, string Name, string InfoHashHex);

public sealed class TorrentParseException : Exception
{
    public TorrentParseException(string message) : base(message)
    {
    }
}
