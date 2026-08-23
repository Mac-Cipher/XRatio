using System.Buffers.Text;
using System.Text;

namespace XRatio.Core.Torrents;

internal sealed class BencodeReader
{
    private const int MaxDepth = 64;
    private const int MaxNodes = 100_000;
    private const int MaxCollectionEntries = 50_000;
    private readonly ReadOnlyMemory<byte> _data;
    private int _position;
    private int _nodes;

    public BencodeReader(ReadOnlyMemory<byte> data)
    {
        _data = data;
    }

    public BencodeNode ReadRoot()
    {
        var result = ReadNode(0);
        if (_position != _data.Length)
            throw Error("Unexpected trailing data.");
        return result;
    }

    private BencodeNode ReadNode(int depth)
    {
        if (depth > MaxDepth)
            throw Error("Bencode nesting is too deep.");
        if (_position >= _data.Length)
            throw Error("Unexpected end of bencoded data.");
        if (++_nodes > MaxNodes)
            throw Error("Bencode node budget exceeded.");

        return _data.Span[_position] switch
        {
            (byte)'i' => ReadInteger(),
            (byte)'l' => ReadList(depth + 1),
            (byte)'d' => ReadDictionary(depth + 1),
            >= (byte)'0' and <= (byte)'9' => ReadString(),
            _ => throw Error("Invalid bencode token.")
        };
    }

    private BencodeInteger ReadInteger()
    {
        var start = _position++;
        var terminator = _data.Span[_position..].IndexOf((byte)'e');
        if (terminator < 0)
            throw Error("Unterminated bencode integer.");

        var valueBytes = _data.Span.Slice(_position, terminator);
        if (valueBytes.IsEmpty || (valueBytes.Length > 1 && valueBytes[0] == (byte)'0') ||
            (valueBytes.Length > 2 && valueBytes[0] == (byte)'-' && valueBytes[1] == (byte)'0') ||
            !Utf8Parser.TryParse(valueBytes, out long value, out var consumed) || consumed != valueBytes.Length)
            throw Error("Invalid bencode integer.");

        _position += terminator + 1;
        return new BencodeInteger(value, start, _position);
    }

    private BencodeString ReadString()
    {
        var start = _position;
        var separator = _data.Span[_position..].IndexOf((byte)':');
        if (separator <= 0)
            throw Error("Invalid bencode string length.");

        var lengthBytes = _data.Span.Slice(_position, separator);
        if (lengthBytes.Length > 1 && lengthBytes[0] == (byte)'0')
            throw Error("Invalid bencode string length.");
        if (!Utf8Parser.TryParse(lengthBytes, out int length, out var consumed) ||
            consumed != lengthBytes.Length || length < 0)
            throw Error("Invalid bencode string length.");

        _position += separator + 1;
        if (length > _data.Length - _position)
            throw Error("Bencode string exceeds the input length.");

        var value = _data.Slice(_position, length).ToArray();
        _position += length;
        return new BencodeString(value, start, _position);
    }

    private BencodeList ReadList(int depth)
    {
        var start = _position++;
        var values = new List<BencodeNode>();
        while (!ConsumeEnd())
        {
            if (values.Count >= MaxCollectionEntries)
                throw Error("Bencode collection entry budget exceeded.");
            values.Add(ReadNode(depth));
        }
        return new BencodeList(values, start, _position);
    }

    private BencodeDictionary ReadDictionary(int depth)
    {
        var start = _position++;
        var values = new Dictionary<string, BencodeNode>(StringComparer.Ordinal);
        while (!ConsumeEnd())
        {
            if (values.Count >= MaxCollectionEntries)
                throw Error("Bencode collection entry budget exceeded.");
            if (ReadNode(depth) is not BencodeString key)
                throw Error("Bencode dictionary keys must be strings.");
            var keyText = Encoding.UTF8.GetString(key.Value);
            if (!values.TryAdd(keyText, ReadNode(depth)))
                throw Error($"Duplicate bencode dictionary key: {keyText}.");
        }
        return new BencodeDictionary(values, start, _position);
    }

    private bool ConsumeEnd()
    {
        if (_position >= _data.Length)
            throw Error("Unterminated bencode collection.");
        if (_data.Span[_position] != (byte)'e')
            return false;
        _position++;
        return true;
    }

    private TorrentParseException Error(string message) =>
        new($"{message} Offset: {_position}.");
}
