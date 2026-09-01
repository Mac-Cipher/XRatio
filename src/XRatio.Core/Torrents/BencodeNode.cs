namespace XRatio.Core.Torrents;

internal abstract record BencodeNode(int StartOffset, int EndOffset);

internal sealed record BencodeInteger(long Value, int StartOffset, int EndOffset)
    : BencodeNode(StartOffset, EndOffset);

internal sealed record BencodeString(byte[] Value, int StartOffset, int EndOffset)
    : BencodeNode(StartOffset, EndOffset);

internal sealed record BencodeList(
    IReadOnlyList<BencodeNode> Values,
    int StartOffset,
    int EndOffset)
    : BencodeNode(StartOffset, EndOffset);

internal sealed record BencodeDictionary(
    IReadOnlyDictionary<string, BencodeNode> Values,
    int StartOffset,
    int EndOffset)
    : BencodeNode(StartOffset, EndOffset);
