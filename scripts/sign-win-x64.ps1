[CmdletBinding(DefaultParameterSetName = 'Pfx')]
param(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ExecutablePath,

    [Parameter(Mandatory = $true, ParameterSetName = 'Pfx')]
    [ValidateNotNullOrEmpty()]
    [string]$PfxPath,

    [Parameter(ParameterSetName = 'Pfx')]
    [SecureString]$PfxPassword,

    [Parameter(Mandatory = $true, ParameterSetName = 'Store')]
    [ValidateNotNullOrEmpty()]
    [string]$CertificateThumbprint,

    [string]$TimestampServer = 'https://timestamp.digicert.com',

    [string]$ChecksumPath
)

$ErrorActionPreference = 'Stop'

$resolvedExecutable = (Resolve-Path -LiteralPath $ExecutablePath).Path
if (![string]::Equals([IO.Path]::GetExtension($resolvedExecutable), '.exe', [StringComparison]::OrdinalIgnoreCase)) {
    throw "Authenticode signing expects a Windows executable: $resolvedExecutable"
}

if ([string]::IsNullOrWhiteSpace($ChecksumPath)) {
    $ChecksumPath = "$resolvedExecutable.sha256"
}

$certificate = $null
$loadedFromPfx = $false
try {
    if ($PSCmdlet.ParameterSetName -eq 'Pfx') {
        $resolvedPfx = (Resolve-Path -LiteralPath $PfxPath).Path
        if ($null -eq $PfxPassword) {
            $PfxPassword = [Security.SecureString]::new()
        }

        $certificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new(
            $resolvedPfx,
            $PfxPassword,
            [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::EphemeralKeySet)
        $loadedFromPfx = $true
    }
    else {
        $normalizedThumbprint = ($CertificateThumbprint -replace '\s', '').ToUpperInvariant()
        $certificate = Get-ChildItem -LiteralPath "Cert:\CurrentUser\My\$normalizedThumbprint" -ErrorAction SilentlyContinue
        if ($null -eq $certificate) {
            throw "Code-signing certificate '$normalizedThumbprint' was not found in Cert:\CurrentUser\My."
        }
    }

    if ($null -eq $certificate -or !$certificate.HasPrivateKey) {
        throw 'The selected certificate does not contain a private key.'
    }

    $hasCodeSigningEku = @($certificate.EnhancedKeyUsageList | Where-Object {
            $_.ObjectId -eq '1.3.6.1.5.5.7.3.3'
        }).Count -gt 0
    if (!$hasCodeSigningEku) {
        throw "The selected certificate is not intended for code signing: $($certificate.Subject)"
    }

    $signParameters = @{
        FilePath    = $resolvedExecutable
        Certificate = $certificate
    }
    if (![string]::IsNullOrWhiteSpace($TimestampServer)) {
        $signParameters.TimestampServer = $TimestampServer
    }

    $result = Set-AuthenticodeSignature @signParameters
    if ($result.Status -ne [System.Management.Automation.SignatureStatus]::Valid) {
        throw "Authenticode signing failed with status '$($result.Status)': $($result.StatusMessage)"
    }

    $verified = Get-AuthenticodeSignature -FilePath $resolvedExecutable
    if ($verified.Status -ne [System.Management.Automation.SignatureStatus]::Valid) {
        throw "Signed executable verification failed with status '$($verified.Status)': $($verified.StatusMessage)"
    }
    if ($verified.SignerCertificate.Thumbprint -ne $certificate.Thumbprint) {
        throw "Signed executable was produced by an unexpected certificate: $($verified.SignerCertificate.Thumbprint)"
    }

    $hash = (Get-FileHash -LiteralPath $resolvedExecutable -Algorithm SHA256).Hash.ToLowerInvariant()
    $hashLine = "$hash  $([IO.Path]::GetFileName($resolvedExecutable))"
    Set-Content -LiteralPath $ChecksumPath -Value $hashLine -Encoding ascii

    Write-Output "Signed executable: $resolvedExecutable"
    Write-Output "Signer: $($certificate.Subject)"
    Write-Output "Certificate thumbprint: $($certificate.Thumbprint)"
    Write-Output "Timestamp server: $(if ([string]::IsNullOrWhiteSpace($TimestampServer)) { 'not used' } else { $TimestampServer })"
    Write-Output "SHA256: $hash"
    Write-Output "Checksum: $ChecksumPath"
}
finally {
    if ($loadedFromPfx -and $null -ne $certificate) {
        $certificate.Dispose()
    }
}
