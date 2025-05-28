# Demo Usage Example for ReadClientSecretExpiration Command

This document demonstrates how to use the new `readclientsecretexpiration` command.

## Command Syntax

```bash
dotnet run -- readclientsecretexpiration --ClientId <client-id> [--ExistingPrivateJwkPath <path>] [--ExistingPrivateJwk <jwk>]
```

## Example Usage

### Using a private key file

```bash
dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwkPath "./my-private-key.jwk"
```

### Using a private key value directly:

```bash
dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk '{"kty":"RSA","d":"...","n":"...","e":"AQAB"}'
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

### PowerShell Example

```powershell
$result = & dotnet run -- readclientsecretexpiration --ClientId $ClientId --ExistingPrivateJwkPath $KeyPath
if ($LASTEXITCODE -eq 0) {
    Write-Host "Secret expiration retrieved successfully: $result"
    
    # Extract epoch timestamp from output (the numeric value)
    if ($result -match "(\d+)$") {
        $epochTime = [long]$matches[1]
        $expirationDate = [DateTimeOffset]::FromUnixTimeSeconds($epochTime).DateTime
        $daysUntilExpiry = ($expirationDate - (Get-Date)).Days
        
        Write-Host "Expiration epoch: $epochTime"
        Write-Host "Expiration date: $expirationDate"
        Write-Host "Days until expiry: $daysUntilExpiry"
        
        # Schedule renewal logic here
        if ($daysUntilExpiry -lt 30) {
            Write-Warning "Secret expires in less than 30 days!"
        }
    }
} else {
    Write-Error "Failed to read secret expiration: $result"
}
```

## Notes

- The command uses the same authentication mechanism as other HelseID commands
- Requires appropriate permissions (`nhn:selvbetjening/client` scope)
- Returns exit code 0 on success, non-zero on error for automation purposes
- Output is a simple epoch timestamp (Unix timestamp) for easy parsing in automation scripts
- Epoch timestamp enables precise date calculations and automation logic
