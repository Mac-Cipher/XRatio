namespace XRatio.Desktop;

internal static class ThemePalette
{
    public const string Light = "Light";
    public const string Dim = "Dim";
    public const string Dark = "Dark";

    public static readonly IReadOnlyList<string> Options = new[] { Light, Dim, Dark };

    public static string Normalize(string? value) =>
        string.Equals(value, Dark, StringComparison.OrdinalIgnoreCase)
            ? Dark
            : string.Equals(value, Dim, StringComparison.OrdinalIgnoreCase)
                ? Dim
                : Light;

    public static bool UsesDarkControls(string? value) => Normalize(value) != Light;
}
