using System.Reflection;

namespace XRatio.Desktop;

internal static class AppVersion
{
    public const string Fallback = "1.0.1";

    public static string Current =>
        typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
            ?.Split('+', 2)[0]
        ?? Fallback;

    public static string Display => $"v{Current}";
}
