# Demo Usage Example for ReadClientSecretExpiration Command

This document demonstrates how to use the new `readclientsecretexpiration` command.

## Command Syntax

```bash
dotnet run -- readclientsecretexpiration --ClientId <client-id> [--ExistingPrivateJwkPath <path>] [--ExistingPrivateJwk <jwk>]
```

## Example Usage

### Using a private key file

```powershell
dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwkPath "./my-private-key.jwk"
```

### Using JSON from HelseID API (with escaped quotes)

When HelseID APIs return JWK data, it comes with escaped quotes like: `{\"kty\":\"RSA\",\"kid\":\"...\"}`

#### **🥇 Best: PowerShell variable (preserves API response exactly):**

```powershell
# Get JWK from API response - use as-is without modification
$apiJwkResponse = '{\"kty\":\"RSA\",\"kid\":\"my-key-2024\",\"d\":\"MIIEowIBAAKCAQEA...\",\"n\":\"xGHNF7qI...\",\"e\":\"AQAB\"}'

dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk $apiJwkResponse
```

#### **🥈 Alternative: PowerShell here-string (preserves API response):**

```powershell
# Wrap API response in here-string without modification
$json = @"
{\"kty\":\"RSA\",\"kid\":\"my-key-2024\",\"d\":\"MIIEowIBAAKCAQEA...\",\"n\":\"xGHNF7qI...\",\"e\":\"AQAB\"}
"@

dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk $json
```

### ❌ Don't do this - Command line parsing will break

```powershell
# This fails because shell parsing breaks the escaped quotes
dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk "{\"kty\":\"RSA\"}"
```

## PowerShell Tips for Command Line JSON

### ✅ Simple approaches that preserve API responses

**Use PowerShell variables to wrap escaped JSON:**

```powershell
# API returns: {\"kty\":\"RSA\",\"kid\":\"...\"}
# Just assign to variable and use as-is:
$apiResponse = '{\"kty\":\"RSA\",\"kid\":\"my-key\"}'
dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk $apiResponse
```

**Use here-strings for multi-line API responses:**

```powershell
$apiResponse = @"
{\"kty\":\"RSA\",\"kid\":\"my-key\",\"d\":\"...\",\"n\":\"...\",\"e\":\"AQAB\"}
"@
dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk $apiResponse
```

### ❌ Avoid direct command line usage with escaped JSON

```powershell
# This will fail due to shell parsing issues:
dotnet run -- readclientsecretexpiration --ExistingPrivateJwk "{\"kty\":\"RSA\"}"
```

## Expected Output

### Success Case

``` text
Reading client secret expiration for client: my-client-id
1735689599
```

### Error Case (Invalid Client)

``` text
Reading client secret expiration for client: invalid-client
✗ Error: Client not found or access denied
```

### No Expiration Date Available

``` text
Reading client secret expiration for client: my-client-id
Client secret expiration date not available in response
```

## Integration with Automation Systems

### Octopus Deploy Variable

You can capture the output for use in automation:

```bash
# Capture exit code and output
output=$(dotnet run -- readclientsecretexpiration --ClientId "$CLIENT_ID" --ExistingPrivateJwkPath "$KEY_PATH" 2>&1)
exit_code=$?

if [ $exit_code -eq 0 ]; then
    echo "Output: $output"
    # Extract epoch timestamp from output (the numeric value)
    epoch_time=$(echo "$output" | grep -o "[0-9]\+$")
    if [ ! -z "$epoch_time" ]; then
        echo "Secret expires at epoch: $epoch_time"
        # Calculate days until expiration
        current_epoch=$(date +%s)
        days_until_expiry=$(( ($epoch_time - $current_epoch) / 86400 ))
        echo "Days until expiry: $days_until_expiry"
    fi
else
    echo "Failed to read expiration: $output"
    exit 1
fi
```

### PowerShell Automation Example

```powershell
# Real-world example: Get JWK from HelseID API and check expiration
$clientId = "my-client-id"

# API response comes with escaped quotes - use as-is
$jwkFromApi = '{\"kty\":\"RSA\",\"kid\":\"my-key-2024\",\"d\":\"MIIEowIBAAKCAQEA...\"}'

# Pass API response directly without modification
$result = & dotnet run -- readclientsecretexpiration --ClientId $clientId --ExistingPrivateJwk $jwkFromApi

if ($LASTEXITCODE -eq 0) {
    Write-Host "Secret expiration retrieved: $result"
    
    # Extract epoch timestamp and calculate days until expiry
    if ($result -match "(\d+)$") {
        $epochTime = [long]$matches[1]
        $expirationDate = [DateTimeOffset]::FromUnixTimeSeconds($epochTime).DateTime
        $daysUntilExpiry = ($expirationDate - (Get-Date)).Days
        
        Write-Host "Expires: $expirationDate ($daysUntilExpiry days)"
        
        if ($daysUntilExpiry -lt 30) {
            Write-Warning "Secret expires soon - schedule renewal!"
        }
    }
} else {
    Write-Error "Failed: $result"
}
```

## Notes

- The command uses the same authentication mechanism as other HelseID commands
- Requires appropriate permissions (`nhn:selvbetjening/client` scope)
- Returns exit code 0 on success, non-zero on error for automation purposes
- Output is a simple epoch timestamp (Unix timestamp) for easy parsing in automation scripts
- Epoch timestamp enables precise date calculations and automation logic
