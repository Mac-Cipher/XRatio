using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace XRatio.Desktop.Platform;

/// <summary>
/// Platform-neutral certificate issuance. Storage and OS trust remain owned by
/// the platform service that calls this factory.
/// </summary>
internal static class CertificateAuthorityCertificates
{
    private const string ServerAuthenticationOid = "1.3.6.1.5.5.7.3.1";

    private static X509KeyStorageFlags PrivateKeyStorageFlags =>
        X509KeyStorageFlags.Exportable |
        (OperatingSystem.IsMacOS()
            ? X509KeyStorageFlags.DefaultKeySet
            : X509KeyStorageFlags.EphemeralKeySet);

    public static X509Certificate2 CreateRoot(string installationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installationId);
        using var key = RSA.Create(3072);
        var request = new CertificateRequest(
            $"CN=XRatio Local CA {installationId}",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: true,
                hasPathLengthConstraint: true,
                pathLengthConstraint: 0,
                critical: true));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign,
                critical: true));
        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, critical: false));

        using var generated = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddYears(10));
        var pkcs12 = generated.Export(X509ContentType.Pfx);
        try
        {
            return X509CertificateLoader.LoadPkcs12(
                pkcs12,
                password: null,
                PrivateKeyStorageFlags);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pkcs12);
        }
    }

    public static bool IsXRatioRoot(X509Certificate2 certificate, string installationId)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        if (string.IsNullOrWhiteSpace(installationId))
            return false;

        var basicConstraints = certificate.Extensions
            .OfType<X509BasicConstraintsExtension>()
            .FirstOrDefault();
        var keyUsage = certificate.Extensions
            .OfType<X509KeyUsageExtension>()
            .FirstOrDefault();
        return string.Equals(
                   certificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false),
                   $"XRatio Local CA {installationId}",
                   StringComparison.Ordinal) &&
               string.Equals(certificate.Subject, certificate.Issuer, StringComparison.Ordinal) &&
               basicConstraints is not null &&
               basicConstraints.CertificateAuthority &&
               basicConstraints.HasPathLengthConstraint &&
               basicConstraints.PathLengthConstraint == 0 &&
               keyUsage is not null &&
               keyUsage.KeyUsages.HasFlag(X509KeyUsageFlags.KeyCertSign);
    }

    public static X509Certificate2 CreateServerCertificate(
        X509Certificate2 root,
        string host)
    {
        ArgumentNullException.ThrowIfNull(root);
        ValidateHost(host);

        using var key = RSA.Create(2048);
        var request = new CertificateRequest(
            $"CN={host}",
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: false,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: true));
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new(ServerAuthenticationOid) },
                critical: true));
        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, critical: false));

        var san = new SubjectAlternativeNameBuilder();
        if (IPAddress.TryParse(host, out var address))
            san.AddIpAddress(address);
        else
            san.AddDnsName(host);
        request.CertificateExtensions.Add(san.Build(critical: true));

        var serial = RandomNumberGenerator.GetBytes(16);
        serial[0] &= 0x7F;
        using var signed = request.Create(
            root,
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddDays(30),
            serial);
        using var withPrivateKey = signed.CopyWithPrivateKey(key);
        var pkcs12 = withPrivateKey.Export(X509ContentType.Pfx);
        try
        {
            return X509CertificateLoader.LoadPkcs12(
                pkcs12,
                password: null,
                PrivateKeyStorageFlags);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pkcs12);
        }
    }

    public static void ValidateHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Length > 253 ||
            Uri.CheckHostName(host) == UriHostNameType.Unknown)
            throw new ArgumentException(
                "A valid DNS name or IP address is required.",
                nameof(host));
    }
}

