namespace XRatio.Desktop;

internal static class ThemePalette
{
    public const string Light = "Light";
    public const string Dim = "Dim";
    public const string SoftDark = "Soft Dark";
    public const string Dark = "Dark";

    public static readonly IReadOnlyList<string> Options = new[] { Light, Dim, SoftDark, Dark };

    public static string Normalize(string? value) =>
        string.Equals(value, Dark, StringComparison.OrdinalIgnoreCase)
            ? Dark
            : string.Equals(value, SoftDark, StringComparison.OrdinalIgnoreCase)
                ? SoftDark
            : string.Equals(value, Dim, StringComparison.OrdinalIgnoreCase)
                ? Dim
                : Light;

    public static int IndexOf(string? value) =>
        Array.FindIndex(Options.ToArray(), option =>
            string.Equals(option, value, StringComparison.OrdinalIgnoreCase));

    public static bool UsesDarkControls(string? value) => Normalize(value) != Light;
}
