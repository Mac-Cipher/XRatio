using System.Net.Http.Headers;
using System.Text.Json;

namespace XRatio.Desktop;

internal sealed record UpdateCheckResult(
    string CurrentVersion,
    string? LatestVersion,
    string? ReleaseName,
    Uri? ReleaseUri,
    Uri? DownloadUri,
    bool IsUpdateAvailable,
    string? Error);

internal static class UpdateChecker
{
    internal const string LatestReleaseApi =
        "https://api.github.com/repos/Mac-Cipher/XRatio/releases/latest";

    private static readonly HttpClient Client = CreateClient();

    public static async Task<UpdateCheckResult> CheckAsync(
        string currentVersion,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await Client.GetAsync(
                LatestReleaseApi,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return Failure(currentVersion, $"GitHub returned HTTP {(int)response.StatusCode}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return ParseRelease(currentVersion, document.RootElement);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Failure(currentVersion, "Update check cancelled.");
        }
        catch (HttpRequestException exception)
        {
            return Failure(currentVersion, exception.Message);
        }
        catch (JsonException exception)
        {
            return Failure(currentVersion, exception.Message);
        }
    }

    internal static UpdateCheckResult ParseRelease(string currentVersion, JsonElement release)
    {
        var tag = release.TryGetProperty("tag_name", out var tagElement)
            ? tagElement.GetString()
            : null;
        var latestVersion = NormalizeTag(tag);
        if (latestVersion is null)
            return Failure(currentVersion, "The GitHub release did not contain a valid version tag.");

        var releaseName = release.TryGetProperty("name", out var nameElement)
            ? nameElement.GetString()
            : null;
        var releaseUrl = release.TryGetProperty("html_url", out var urlElement)
            ? urlElement.GetString()
            : null;
        Uri.TryCreate(releaseUrl, UriKind.Absolute, out var releaseUri);
        var downloadUri = ParseDownloadUri(release);

        return new UpdateCheckResult(
            currentVersion,
            latestVersion,
            releaseName,
            releaseUri,
            downloadUri,
            IsNewerVersion(currentVersion, latestVersion),
            null);
    }

    private static Uri? ParseDownloadUri(JsonElement release)
    {
        if (!release.TryGetProperty("assets", out var assets) ||
            assets.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var asset in assets.EnumerateArray()
                     .Where(item =>
                         item.TryGetProperty("name", out var nameElement) &&
                         nameElement.GetString()?.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) == true)
                     .Concat(assets.EnumerateArray().Where(item =>
                         item.TryGetProperty("name", out var nameElement) &&
                         nameElement.GetString()?.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) == true)))
        {
            if (asset.TryGetProperty("browser_download_url", out var downloadElement) &&
                Uri.TryCreate(downloadElement.GetString(), UriKind.Absolute, out var downloadUri))
                return downloadUri;
        }

        return null;
    }

    internal static string? NormalizeTag(string? tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return null;

        var normalized = tag.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
            normalized = normalized[1..];
        return Version.TryParse(normalized, out _) ? normalized : null;
    }

    internal static bool IsNewerVersion(string currentVersion, string latestVersion)
    {
        return Version.TryParse(NormalizeTag(currentVersion), out var current) &&
               Version.TryParse(NormalizeTag(latestVersion), out var latest) &&
               latest > current;
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("XRatio", AppVersion.Current));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }

    private static UpdateCheckResult Failure(string currentVersion, string error) =>
        new(currentVersion, null, null, null, null, false, error);
}
