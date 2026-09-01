using System.Collections.Concurrent;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using XRatio.Core.Platform;

namespace XRatio.Desktop.Platform;

[SupportedOSPlatform("windows")]
internal sealed class WindowsCertificateAuthorityService : ICertificateAuthorityService, IDisposable
{
    private const string MetadataFileName = "ca.json";
    private const long MaximumMetadataBytes = 16 * 1024;
    private static readonly JsonSerializerOptions MetadataJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        MaxDepth = 8
    };
    private readonly string _metadataPath;
    private readonly ICertificateTrustStore _certificateStore;
    private readonly SemaphoreSlim _rootGate = new(1, 1);
    private readonly ConcurrentDictionary<string, Lazy<Task<X509Certificate2>>> _hostCertificates =
        new(StringComparer.OrdinalIgnoreCase);
    private X509Certificate2? _root;

    public WindowsCertificateAuthorityService(string profileDirectory)
        : this(profileDirectory, new WindowsCertificateStore())
    {
    }

    internal WindowsCertificateAuthorityService(
        string profileDirectory,
        ICertificateTrustStore certificateStore)
    {
        ArgumentNullException.ThrowIfNull(certificateStore);
        var tlsDirectory = Path.Combine(profileDirectory, "tls");
        Directory.CreateDirectory(tlsDirectory);
        _metadataPath = Path.Combine(tlsDirectory, MetadataFileName);
        _certificateStore = certificateStore;
    }

    public PlatformCapability Capability { get; } = new(
        true,
        "Windows CurrentUser certificate stores; trust is installed only after explicit confirmation.");

    public async Task<bool> IsTrustedAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CertificateMetadata? metadata;
        try
        {
            metadata = await ReadMetadataAsync(cancellationToken);
        }
        catch (InvalidDataException)
        {
            // Corrupt metadata must never be treated as an invitation to
            // generate a replacement CA. Keep HTTPS disabled until the
            // installation can be repaired explicitly.
            return false;
        }

        if (metadata is null)
            return false;

        using var trusted = _certificateStore.FindTrusted(metadata.Thumbprint);
        return trusted is not null &&
               CertificateAuthorityCertificates.IsXRatioRoot(trusted, metadata.InstallationId);
    }

    public async Task RequestTrustAsync(CancellationToken cancellationToken = default)
    {
        var root = await GetOrCreateRootAsync(cancellationToken);
        if (await IsTrustedAsync(cancellationToken))
            return;

        try
        {
            using var publicRoot = X509CertificateLoader.LoadCertificate(root.Export(X509ContentType.Cert));
            _certificateStore.AddTrusted(publicRoot);
            if (!await IsTrustedAsync(cancellationToken))
                throw new InvalidOperationException(
                    "Windows did not add the XRatio CA to the current user's trusted root store.");
        }
        catch
        {
            // A denied prompt or a store error must not leave a private CA that
            // can never be matched to a trusted root. Cleanup uses a fresh token
            // so cancellation cannot strand the installation secret.
            await RemoveTrustAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task RemoveTrustAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var metadata = await ReadMetadataAsync(cancellationToken);
        var thumbprint = _root?.Thumbprint ?? metadata?.Thumbprint;
        if (thumbprint is null)
            return;

        if (metadata is not null)
            ValidateStoredRoots(metadata);

        _certificateStore.RemoveTrusted(thumbprint);
        _certificateStore.RemovePrivate(thumbprint);

        DisposeHostCertificates();
        _root?.Dispose();
        _root = null;
        if (File.Exists(_metadataPath))
            File.Delete(_metadataPath);
    }

    public async Task<X509Certificate2> GetServerCertificateAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CertificateAuthorityCertificates.ValidateHost(host);
        if (_hostCertificates.Count >= 256 && !_hostCertificates.ContainsKey(host))
            throw new InvalidOperationException("The per-host certificate cache limit has been reached.");

        var lazy = _hostCertificates.GetOrAdd(
            host,
            key => new Lazy<Task<X509Certificate2>>(
                () => CreateServerCertificateAsync(key, CancellationToken.None),
                LazyThreadSafetyMode.ExecutionAndPublication));
        X509Certificate2 cached;
        try
        {
            cached = await lazy.Value.WaitAsync(cancellationToken);
        }
        catch
        {
            _hostCertificates.TryRemove(new KeyValuePair<string, Lazy<Task<X509Certificate2>>>(host, lazy));
            throw;
        }
        var pkcs12 = cached.Export(X509ContentType.Pfx);
        try
        {
            return X509CertificateLoader.LoadPkcs12(
                pkcs12,
                password: null,
                X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pkcs12);
        }
    }

    private async Task<X509Certificate2> GetOrCreateRootAsync(CancellationToken cancellationToken)
    {
        if (_root is not null)
            return _root;

        await _rootGate.WaitAsync(cancellationToken);
        try
        {
            if (_root is not null)
                return _root;

            var metadata = await ReadMetadataAsync(cancellationToken);
            if (metadata is not null)
            {
                var existing = _certificateStore.FindPrivate(metadata.Thumbprint);
                if (existing is not null)
                {
                    if (!existing.HasPrivateKey ||
                        !CertificateAuthorityCertificates.IsXRatioRoot(existing, metadata.InstallationId))
                    {
                        existing.Dispose();
                        throw new InvalidDataException(
                            "The certificate matching XRatio CA metadata is not an XRatio installation root.");
                    }

                    return _root = existing;
                }

                using (var trusted = _certificateStore.FindTrusted(metadata.Thumbprint))
                {
                    if (trusted is not null &&
                        !CertificateAuthorityCertificates.IsXRatioRoot(trusted, metadata.InstallationId))
                    {
                        throw new InvalidDataException(
                            "The trusted certificate matching XRatio CA metadata is not an XRatio installation root.");
                    }
                }

                // The private key may have been removed outside XRatio while
                // the installation metadata and public Root entry remained. Remove
                // only that exact installation thumbprint before replacing it so
                // re-enabling cannot accumulate an orphaned trusted root.
                _certificateStore.RemoveTrusted(metadata.Thumbprint);
                _certificateStore.RemovePrivate(metadata.Thumbprint);
            }

            var installationId = metadata?.InstallationId ?? Guid.NewGuid().ToString("N");
            var generatedRoot = CreateAndPersistRoot(installationId);
            try
            {
                await WriteMetadataAsync(
                    new CertificateMetadata(installationId, generatedRoot.Thumbprint),
                    cancellationToken);
                _root = generatedRoot;
                return generatedRoot;
            }
            catch
            {
                // Metadata persistence failed after the private key was stored.
                // Remove that exact key before propagating the I/O/cancellation
                // error so an untracked installation secret cannot accumulate.
                _certificateStore.RemovePrivate(generatedRoot.Thumbprint);
                generatedRoot.Dispose();
                throw;
            }
        }
        finally
        {
            _rootGate.Release();
        }
    }

    private X509Certificate2 CreateAndPersistRoot(string installationId)
    {
        using var generated = CertificateAuthorityCertificates.CreateRoot(installationId);
        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var pkcs12 = generated.Export(X509ContentType.Pfx, password);
        try
        {
            return _certificateStore.ImportAndStorePrivate(
                pkcs12,
                password,
                "XRatio installation CA");
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pkcs12);
        }
    }

    private async Task<X509Certificate2> CreateServerCertificateAsync(
        string host,
        CancellationToken cancellationToken)
    {
        var root = await GetOrCreateRootAsync(cancellationToken);
        return CertificateAuthorityCertificates.CreateServerCertificate(root, host);
    }

    private async Task<CertificateMetadata?> ReadMetadataAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_metadataPath))
            return null;

        var info = new FileInfo(_metadataPath);
        if (info.Length is <= 0 or > MaximumMetadataBytes)
            throw new InvalidDataException(
                "The XRatio CA metadata is too large; refusing to generate a replacement CA.");

        await using var stream = File.OpenRead(_metadataPath);
        CertificateMetadata? metadata;
        try
        {
            metadata = await JsonSerializer.DeserializeAsync<CertificateMetadata>(
                stream,
                MetadataJsonOptions,
                cancellationToken);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                "The XRatio CA metadata is invalid; refusing to generate a replacement CA.",
                exception);
        }

        if (metadata is null ||
            string.IsNullOrWhiteSpace(metadata.InstallationId) ||
            metadata.InstallationId.Length > 128 ||
            metadata.InstallationId.Any(char.IsControl) ||
            metadata.Thumbprint is not { Length: 40 } ||
            !metadata.Thumbprint.All(Uri.IsHexDigit))
        {
            throw new InvalidDataException(
                "The XRatio CA metadata is incomplete or invalid; refusing to generate a replacement CA.");
        }

        return metadata;
    }

    private async Task WriteMetadataAsync(
        CertificateMetadata metadata,
        CancellationToken cancellationToken)
    {
        var temporary = _metadataPath + ".tmp";
        try
        {
            await using (var stream = new FileStream(
                             temporary, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough))
            {
                await JsonSerializer.SerializeAsync(
                    stream,
                    metadata,
                    MetadataJsonOptions,
                    cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            File.Move(temporary, _metadataPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }

    public void Dispose()
    {
        DisposeHostCertificates();
        _root?.Dispose();
        _rootGate.Dispose();
        _certificateStore.Dispose();
    }

    private void DisposeHostCertificates()
    {
        foreach (var item in _hostCertificates.Values)
            if (item.IsValueCreated && item.Value.IsCompletedSuccessfully)
                item.Value.Result.Dispose();
        _hostCertificates.Clear();
    }

    private void ValidateStoredRoots(CertificateMetadata metadata)
    {
        using var privateRoot = _certificateStore.FindPrivate(metadata.Thumbprint);
        if (privateRoot is not null &&
            (!privateRoot.HasPrivateKey ||
             !CertificateAuthorityCertificates.IsXRatioRoot(privateRoot, metadata.InstallationId)))
        {
            throw new InvalidDataException(
                "The private certificate matching XRatio CA metadata is not an XRatio installation root.");
        }

        using var trustedRoot = _certificateStore.FindTrusted(metadata.Thumbprint);
        if (trustedRoot is not null &&
            !CertificateAuthorityCertificates.IsXRatioRoot(trustedRoot, metadata.InstallationId))
        {
            throw new InvalidDataException(
                "The trusted certificate matching XRatio CA metadata is not an XRatio installation root.");
        }
    }

    private sealed record CertificateMetadata(string InstallationId, string Thumbprint);
}

