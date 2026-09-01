using System.Text;

namespace XRatio.Core.Announcements;

public sealed record TrackerResponse(int? Complete, int? Incomplete, int? Interval, string? FailureReason);

public static class TrackerResponseParser
{
    private static ReadOnlySpan<byte> CompleteMarker => "8:completei"u8;
    private static ReadOnlySpan<byte> IncompleteMarker => "10:incompletei"u8;
    private static ReadOnlySpan<byte> IntervalMarker => "8:intervali"u8;
    private static ReadOnlySpan<byte> FailureReasonMarker => "14:failure reason"u8;

    public static TrackerResponse Parse(ReadOnlySpan<byte> payload) => new(
        ReadInteger(payload, CompleteMarker),
        ReadInteger(payload, IncompleteMarker),
        ReadInteger(payload, IntervalMarker),
        ReadFailureReason(payload));

    private static int? ReadInteger(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> marker)
    {
        var searchStart = 0;
        while (searchStart <= payload.Length - marker.Length)
        {
            var offset = payload[searchStart..].IndexOf(marker);
            if (offset < 0)
                return null;

            var markerStart = searchStart + offset;
            var numberStart = markerStart + marker.Length;
            var numberEnd = numberStart;
            while (numberEnd < payload.Length && IsDigit(payload[numberEnd]))
                numberEnd++;

            if (numberEnd > numberStart && numberEnd < payload.Length && payload[numberEnd] == (byte)'e')
                return ParseInteger(payload[numberStart..numberEnd]);

            // Keep searching when an occurrence is not a complete bencode
            // integer, matching Regex.Match's behavior for malformed data.
            searchStart = markerStart + 1;
        }

        return null;
    }

    private static string? ReadFailureReason(ReadOnlySpan<byte> payload)
    {
        var searchStart = 0;
        while (searchStart <= payload.Length - FailureReasonMarker.Length)
        {
            var offset = payload[searchStart..].IndexOf(FailureReasonMarker);
            if (offset < 0)
                return null;

            var markerStart = searchStart + offset;
            var lengthStart = markerStart + FailureReasonMarker.Length;
            var lengthEnd = lengthStart;
            while (lengthEnd < payload.Length && IsDigit(payload[lengthEnd]))
                lengthEnd++;

            var digitCount = lengthEnd - lengthStart;
            if (digitCount is >= 1 and <= 5 && lengthEnd < payload.Length && payload[lengthEnd] == (byte)':')
            {
                var length = ParseSmallInteger(payload[lengthStart..lengthEnd]);
                if (length > 4096)
                    return null;

                // The original parser normalizes the decimal length when it
                // constructs the marker used by IndexOf. Thus a length such
                // as "09" is only accepted if a later canonical "9" marker
                // exists, which is preserved here without materializing the
                // complete response as a string.
                return FindFailureReason(payload, length);
            }

            searchStart = markerStart + 1;
        }

        return null;
    }

    private static string? FindFailureReason(ReadOnlySpan<byte> payload, int length)
    {
        var searchStart = 0;
        while (searchStart <= payload.Length - FailureReasonMarker.Length)
        {
            var offset = payload[searchStart..].IndexOf(FailureReasonMarker);
            if (offset < 0)
                return null;

            var markerStart = searchStart + offset;
            var lengthStart = markerStart + FailureReasonMarker.Length;
            var lengthEnd = lengthStart;
            while (lengthEnd < payload.Length && IsDigit(payload[lengthEnd]))
                lengthEnd++;
            var digitCount = lengthEnd - lengthStart;
            if (digitCount is >= 1 and <= 5 && lengthEnd < payload.Length &&
                payload[lengthEnd] == (byte)':' &&
                ParseSmallInteger(payload[lengthStart..lengthEnd]) == length &&
                IsCanonicalDecimal(payload[lengthStart..lengthEnd], length))
            {
                var valueStart = lengthEnd + 1;
                if (valueStart + length > payload.Length)
                    return null;
                return Encoding.Latin1.GetString(payload.Slice(valueStart, length));
            }

            searchStart = markerStart + 1;
        }

        return null;
    }

    private static int? ParseInteger(ReadOnlySpan<byte> digits)
    {
        var result = 0;
        foreach (var digit in digits)
        {
            var value = digit - (byte)'0';
            if (result > (int.MaxValue - value) / 10)
                return null;
            result = result * 10 + value;
        }

        return result;
    }

    private static int ParseSmallInteger(ReadOnlySpan<byte> digits)
    {
        var result = 0;
        foreach (var digit in digits)
            result = result * 10 + digit - (byte)'0';
        return result;
    }

    private static bool IsCanonicalDecimal(ReadOnlySpan<byte> digits, int value)
    {
        if (value == 0)
            return digits.Length == 1 && digits[0] == (byte)'0';

        var digitCount = 0;
        for (var remaining = value; remaining > 0; remaining /= 10)
            digitCount++;
        if (digits.Length != digitCount)
            return false;

        for (var index = digits.Length - 1; index >= 0; index--)
        {
            if (digits[index] != (byte)('0' + value % 10))
                return false;
            value /= 10;
        }

        return true;
    }

    private static bool IsDigit(byte value) => value is >= (byte)'0' and <= (byte)'9';
}
