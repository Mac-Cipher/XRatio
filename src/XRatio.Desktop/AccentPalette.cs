namespace XRatio.Desktop;

internal static class AccentPalette
{
    public const string Blue = "Blue";
    public const string Teal = "Teal";
    public const string Violet = "Violet";
    public const string Amber = "Amber";
    public const string Rose = "Rose";
    public const string Green = "Green";

    public static readonly IReadOnlyList<string> Options = new[] { Blue, Teal, Violet, Amber, Rose, Green };

    public static string Normalize(string? value) =>
        Options.FirstOrDefault(option => string.Equals(value, option, StringComparison.OrdinalIgnoreCase)) ?? Blue;

    public static int IndexOf(string? value) =>
        Array.FindIndex(Options.ToArray(), option =>
            string.Equals(option, value, StringComparison.OrdinalIgnoreCase));

    public static string Primary(string? value, bool dark, bool dim = false, bool softDark = false) =>
        Normalize(value) switch
        {
            Teal => dark ? "#59D8E6" : softDark ? "#66C4CB" : dim ? "#36A4AF" : "#087E8B",
            Violet => dark ? "#B79BFF" : softDark ? "#9D86DF" : dim ? "#8B5CF6" : "#6D28D9",
            Amber => dark ? "#F6D27A" : softDark ? "#E6BB6C" : dim ? "#F59E0B" : "#B45309",
            Rose => dark ? "#FF9CB2" : softDark ? "#E98EA4" : dim ? "#F43F5E" : "#BE123C",
            Green => dark ? "#7BE3AA" : softDark ? "#6ACB98" : dim ? "#22C55E" : "#15803D",
            _ => dark ? "#60A5FA" : softDark ? "#82A9D9" : dim ? "#3B82F6" : "#1D4ED8"
        };

    public static string Soft(string? value, bool dark, bool dim = false, bool softDark = false) =>
        Normalize(value) switch
        {
            Teal => dark ? "#153A44" : softDark ? "#243A3E" : dim ? "#21474E" : "#D8F3F5",
            Violet => dark ? "#30244D" : softDark ? "#332B4A" : dim ? "#46366D" : "#EEE8FF",
            Amber => dark ? "#4A3920" : softDark ? "#453823" : dim ? "#654A1F" : "#FFF2D4",
            Rose => dark ? "#4D2633" : softDark ? "#482B35" : dim ? "#6A3040" : "#FFE7EC",
            Green => dark ? "#183D2B" : softDark ? "#21402F" : dim ? "#245536" : "#E2F8EA",
            _ => dark ? "#172B4D" : softDark ? "#29374A" : dim ? "#273A5B" : "#E8F0FF"
        };

    public static string Light1(string? value, bool dark, bool dim = false, bool softDark = false) =>
        Normalize(value) switch
        {
            Teal => dark ? "#86E9F1" : softDark ? "#8AD8DD" : dim ? "#63C6CF" : "#36A4AF",
            Violet => dark ? "#D2C2FF" : softDark ? "#C1B0F1" : dim ? "#B39AFB" : "#8B5CF6",
            Amber => dark ? "#FFE09A" : softDark ? "#F2D18A" : dim ? "#F9C75A" : "#D97706",
            Rose => dark ? "#FFC1CD" : softDark ? "#F6B0BE" : dim ? "#FB7185" : "#E11D48",
            Green => dark ? "#B5F4CF" : softDark ? "#A0E7BE" : dim ? "#66D391" : "#22C55E",
            _ => dark ? "#93C5FD" : softDark ? "#B4CDEF" : dim ? "#60A5FA" : "#3B82F6"
        };

    public static string Dark1(string? value, bool dark, bool dim = false, bool softDark = false) =>
        Normalize(value) switch
        {
            Teal => dark ? "#2DA8B8" : softDark ? "#348C93" : dim ? "#227B87" : "#05616C",
            Violet => dark ? "#8B6AD9" : softDark ? "#7254B4" : dim ? "#6D42D7" : "#4C1D95",
            Amber => dark ? "#C99D3F" : softDark ? "#B5893E" : dim ? "#C47C05" : "#92400E",
            Rose => dark ? "#D96F88" : softDark ? "#BD6177" : dim ? "#C92E4C" : "#9F1239",
            Green => dark ? "#43B87B" : softDark ? "#3E9568" : dim ? "#169447" : "#166534",
            _ => dark ? "#2563EB" : dim || softDark ? "#2563EB" : "#1E40AF"
        };
}
