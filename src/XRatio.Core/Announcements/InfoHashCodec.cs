namespace XRatio.Core.Announcements;

/// <summary>
/// Converts the binary URL form used by BitTorrent announces into the stable
/// hexadecimal form used by torrent metadata and client-side catalogues.
/// Human-readable test hashes (for example "abc") are intentionally left as
/// they are so older profiles and synthetic tracker tests remain compatible.
/// </summary>
internal static class InfoHashCodec
{
    public static string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Contains('%', StringComparison.Ordinal))
        {
            Span<byte> bytes = stackalloc byte[20];
            if (TryDecodeBytes(value, bytes, out var length) && length == bytes.Length)
                return Convert.ToHexString(bytes);
        }

        if (value.Length != 40)
            return value;

        var hasLowercase = false;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (!IsHexDigit(character))
                return value;
            hasLowercase |= character is >= 'a' and <= 'f';
        }

        // Most announces already use uppercase hexadecimal. Returning the
        // original string avoids a new 40-character allocation in that case.
        return hasLowercase ? value.ToUpperInvariant() : value;
    }

    private static bool TryDecodeBytes(string value, Span<byte> destination, out int length)
    {
        length = 0;
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (character == '%')
            {
                if (index + 2 >= value.Length ||
                    !TryHex(value[index + 1], out var high) ||
                    !TryHex(value[index + 2], out var low) ||
                    length >= destination.Length)
                {
                    length = 0;
                    return false;
                }

                destination[length++] = (byte)((high << 4) | low);
                index += 2;
                continue;
            }

            // A correctly encoded info-hash is normally all %HH sequences,
            // but accepting visible ASCII keeps this helper harmless for
            // synthetic announce requests used by clients and tests.
            if (character > 0x7F || length >= destination.Length)
            {
                length = 0;
                return false;
            }
            destination[length++] = (byte)character;
        }

        return true;
    }

    private static bool IsHexDigit(char value) =>
        value is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';

    private static bool TryHex(char value, out int result)
    {
        result = value switch
        {
            >= '0' and <= '9' => value - '0',
            >= 'a' and <= 'f' => value - 'a' + 10,
            >= 'A' and <= 'F' => value - 'A' + 10,
            _ => -1
        };
        return result >= 0;
    }
}
