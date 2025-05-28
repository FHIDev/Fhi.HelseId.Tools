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
✓ Client secret expires: 2024-12-31T23:59:59Z
```

### Error Case (Invalid Client)

``` text
Reading client secret expiration for client: invalid-client
✗ Error: Client not found or access denied
```

### No Expiration Date Available

``` text
Reading client secret expiration for client: my-client-id
ℹ Client secret expiration date not available
```

## Integration with Automation Systems

### Octopus Deploy Variable

You can capture the output for use in automation:

```bash
# Capture exit code and output
expiration_date=$(dotnet run -- readclientsecretexpiration --ClientId "$CLIENT_ID" --ExistingPrivateJwkPath "$KEY_PATH" 2>&1)
exit_code=$?

if [ $exit_code -eq 0 ]; then
    echo "Secret expires: $expiration_date"
    # Schedule renewal based on expiration date
else
    echo "Failed to read expiration: $expiration_date"
    exit 1
fi
```

### PowerShell Example

```powershell
$result = & dotnet run -- readclientsecretexpiration --ClientId $ClientId --ExistingPrivateJwkPath $KeyPath
if ($LASTEXITCODE -eq 0) {
    Write-Host "Secret expiration retrieved successfully: $result"
    # Add logic to schedule renewal
} else {
    Write-Error "Failed to read secret expiration: $result"
}
```

## Notes

- The command uses the same authentication mechanism as other HelseID commands
- Requires appropriate permissions (`nhn:selvbetjening/client` scope)
- Returns exit code 0 on success, non-zero on error for automation purposes
- Handles both ISO 8601 date strings and Unix timestamps from the API
