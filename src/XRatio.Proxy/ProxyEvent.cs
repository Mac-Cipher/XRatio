using XRatio.Core.Announcements;

namespace XRatio.Proxy;

public sealed record ProxyEvent(
    DateTimeOffset Timestamp,
    AnnounceDisposition Disposition,
    string Message,
    Uri? Target = null,
    string? InfoHash = null);

