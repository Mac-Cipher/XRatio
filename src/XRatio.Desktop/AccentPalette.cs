namespace XRatio.Desktop;

internal static class AccentPalette
{
    public const string Blue = "Blue";
    public const string Teal = "Teal";

    public static readonly IReadOnlyList<string> Options = new[] { Blue, Teal };

    public static string Normalize(string? value) =>
        string.Equals(value, Teal, StringComparison.OrdinalIgnoreCase)
            ? Teal
            : Blue;

    public static string Primary(string? value, bool dark, bool dim = false) =>
        Normalize(value) == Teal
            ? dark ? "#59D8E6" : dim ? "#36A4AF" : "#087E8B"
            : dark ? "#60A5FA" : dim ? "#3B82F6" : "#1D4ED8";

    public static string Soft(string? value, bool dark, bool dim = false) =>
        Normalize(value) == Teal
            ? dark ? "#153A44" : dim ? "#21474E" : "#D8F3F5"
            : dark ? "#172B4D" : dim ? "#273A5B" : "#E8F0FF";

    public static string Light1(string? value, bool dark, bool dim = false) =>
        Normalize(value) == Teal
            ? dark ? "#86E9F1" : dim ? "#63C6CF" : "#36A4AF"
            : dark ? "#93C5FD" : dim ? "#60A5FA" : "#3B82F6";

    public static string Dark1(string? value, bool dark, bool dim = false) =>
        Normalize(value) == Teal
            ? dark ? "#2DA8B8" : dim ? "#227B87" : "#05616C"
            : dark ? "#2563EB" : dim ? "#2563EB" : "#1E40AF";
}
