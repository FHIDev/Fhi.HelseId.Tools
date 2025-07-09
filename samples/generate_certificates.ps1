# This script generates a self-signed certificate, exports it in various formats,
# and saves the Base64 encoded versions of the private and public keys.
# Ensure the script is run with administrative privileges

# Prompt user for the Common Name (CN)
$cnInput = Read-Host "Enter the Common Name (CN) for the certificate"
$certPath = Read-Host "Enter path to store the certificate"

# Prompt for password securely (won't show the password as it's typed)
$securePassword = Read-Host "Enter password for the certificate" -AsSecureString

# Generate the self-signed certificate with algorithm parameters
$certificate = New-SelfSignedCertificate `
    -Subject "CN=$cnInput" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyExportPolicy Exportable `
    -KeySpec Signature `
    -NotAfter (Get-Date).AddYears(5)

# Get the thumbprint of the generated certificate
$thumbprint = $certificate.Thumbprint
Write-Host "Certificate Thumbprint: $thumbprint"

# Write thumbprint to a text file
$thumbprintFile = "$($certPath)\$($cnInput)_thumbprint.txt"
$thumbprint | Out-File -FilePath $thumbprintFile
Write-Host "Certificate thumbprint saved to: $thumbprintFile"

# Export the private key to a PFX file
$pfxFilePath = "$($certPath)\$($cnInput)_private.pfx"
Export-PfxCertificate -Cert "Cert:\CurrentUser\My\$thumbprint" -FilePath $pfxFilePath -Password $securePassword
Write-Host "Private key exported to: $pfxFilePath"

# Export the public key to a CER file
$cerFilePath = "$($certPath)\$($cnInput)_public.cer"
Export-Certificate -Cert "Cert:\CurrentUser\My\$thumbprint" -FilePath $cerFilePath
Write-Host "Public key exported to: $cerFilePath"

# Read and encode the PFX file as Base64
$pfxContent = [System.IO.File]::ReadAllBytes($pfxFilePath)
$pfxBase64 = [System.Convert]::ToBase64String($pfxContent)

# Read and encode the CER file as Base64
$cerContent = [System.IO.File]::ReadAllBytes($cerFilePath)
$cerBase64 = [System.Convert]::ToBase64String($cerContent)

# Output the Base64 encoded keys to files
$pfxBase64File = "$($certPath)\$($cnInput)_private_base64.txt"
$cerBase64File = "$($certPath)\$($cnInput)_public_base64.txt"

$pfxBase64 | Out-File -FilePath $pfxBase64File
$cerBase64 | Out-File -FilePath $cerBase64File

Write-Host "Base64 encoded private key saved to: $pfxBase64File"
Write-Host "Base64 encoded public key saved to: $cerBase64File"

# Convert binary CER to PEM format
$base64Cer = [Convert]::ToBase64String($cerContent, 'InsertLineBreaks')
$pemCer = @(
    "-----BEGIN CERTIFICATE-----"
    $base64Cer
    "-----END CERTIFICATE-----"
)
$pemFile = "$($certPath)\$($cnInput)_public.pem"
$pemCer | Out-File -FilePath $pemFile -Encoding ascii

Write-Host "PEM-formatted public key saved to: $pemFile"

# Cleanup
Write-Host "Certificate generation and export complete."