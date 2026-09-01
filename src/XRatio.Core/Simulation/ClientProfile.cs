using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace XRatio.Core.Simulation;

public sealed record ClientProfile(
    string Id,
    string DisplayName,
    string UserAgent,
    string PeerIdPrefix)
{
    public string CreatePeerId()
    {
        var prefix = Encoding.ASCII.GetBytes(PeerIdPrefix);
        if (prefix.Length > 20)
            throw new InvalidOperationException("Peer ID prefixes cannot exceed 20 bytes.");

        Span<byte> random = stackalloc byte[20 - prefix.Length];
        RandomNumberGenerator.Fill(random);
        var suffix = Convert.ToHexString(random).ToLowerInvariant();
        var peerId = PeerIdPrefix + suffix;
        return peerId[..20];
    }

    public static string CreateKey()
    {
        Span<byte> bytes = stackalloc byte[4];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLower(CultureInfo.InvariantCulture);
    }
}

public static class ClientProfileCatalog
{
    public static IReadOnlyList<ClientProfile> All { get; } =
    [
        new("qbittorrent-5.2", "qBittorrent 5.2.3", "qBittorrent/5.2.3", "-qB5230-"),
        new("qbittorrent-5.1", "qBittorrent 5.1", "qBittorrent/5.1.0", "-qB5100-"),
        new("qbittorrent-4.6", "qBittorrent 4.6.0", "qBittorrent/4.6.0", "-qB4600-"),
        new("utorrent-3.6", "µTorrent 3.6", "uTorrent/3600", "-UT3600-"),
        new("transmission-4", "Transmission 4", "Transmission/4.0.6", "-TR4060-"),
        new("deluge-2.1", "Deluge 2.1", "Deluge/2.1.1", "-DE2110-"),
        new("vuze-5.7", "Vuze 5.7", "Azureus 5.7.6.0", "-AZ5760-"),
        new("biglybt-3", "BiglyBT 3", "BiglyBT/3.7.0.0", "-BI3700-"),
        new("bittorrent-8", "BitTorrent 8", "BitTorrent/8.2.0", "-BT8200-"),
        new("libtorrent-2", "libtorrent 2", "libtorrent/2.0", "-LT2000-"),
        new("rtorrent-0.9", "rTorrent 0.9", "rTorrent/0.9.8", "-lt0D80-"),
        new("ktorrent-6", "KTorrent 6", "KTorrent/6.1", "-KT6100-"),
        new("bitcomet-2", "BitComet 2", "BitComet/2.13", "-BC0213-"),
        new("bitlord-2", "BitLord 2", "BitLord/2.4", "-BL2400-"),
        new("bittornado-0.3", "BitTornado 0.3", "BitTornado/0.3.18", "-T03I00-"),
        new("tixati-3", "Tixati 3", "Tixati/3.31", "-TX3310-"),
        new("aria2-1.37", "aria2 1.37", "aria2/1.37.0", "-A2-1-37-"),
        new("generic", "Generic BitTorrent", "BitTorrent/1.0", "-XR1000-")
    ];

    private static readonly IReadOnlyDictionary<string, ClientProfile> ProfilesById =
        All.ToDictionary(profile => profile.Id, StringComparer.OrdinalIgnoreCase);

    public static ClientProfile Get(string id) =>
        id is not null && ProfilesById.TryGetValue(id, out var profile)
            ? profile
            : throw new KeyNotFoundException($"Unknown client profile: {id}.");
}
