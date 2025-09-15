<!-- filepath: c:\git\Fhi.HelseId.Tools\docs\ClientSecret\read-client-secret-expiration-command.md -->
# Read Client Secret Expiration Command

The `readclientsecretexpiration` command allows you to query the expiration date of a client secret from HelseID. This is useful for automated monitoring and scheduling of client secret renewals in systems like Octopus Deploy.

## Authentication

The command uses the client's existing private JWK (JSON Web Key) to authenticate with HelseID, similar to the update commands.

## Parameters

| Parameter name |Short Name| Description | Required | Sample |
|----------------|----------|---|----------|--------|
| --ClientId |-c| The target Client's unique identifier. <br> Found in HelseId Selvbetjening Klient konfigurasjon. | <b>Yes</b> | `37a08838-db82-4de0-bfe1-bed876e7086e` |
| --ExistingPrivateJwkPath |-ep| Path to the private key file. | <b>No*</b> | `C:\keys\37a08838-db82-4de0-bfe1-bed876e7086e_private.json` |
| --ExistingPrivateJwk |-e| Private key string. | <b>No*</b> | `{"alg":"PS512","d":"xxx .....}` |
|--AuthorityUrl |-a|Authority url to target.                                                                         |<b>Yes</b>|`https://helseid-sts.test.nhn.no`|
|--BaseAddress |-b|Base address url to target.                                                                         |<b>Yes</b>|`https://api.selvbetjening.test.nhn.no`|

<i>*Either ExistingPrivateJwkPath or ExistingPrivateJwk value must be provided.</i>

## Commands

### Read expiration using private key file

```bash
helseid-cli readclientsecretexpiration --ClientId <CLIENT_ID> --ExistingPrivateJwkPath <PATH_TO_PRIVATE_KEY> --AuthorityUrl <AUTHORITY_URL> --BaseAddress <BASE_URL>
```

### Read expiration using private key string

```bash
helseid-cli readclientsecretexpiration --ClientId <CLIENT_ID> --ExistingPrivateJwk <PRIVATE_KEY_JSON> --AuthorityUrl <AUTHORITY_URL> --BaseAddress <BASE_URL>
```

### Using short parameter names

```bash
helseid-cli readclientsecretexpiration -c <CLIENT_ID> -ep <PATH_TO_PRIVATE_KEY> --a <AUTHORITY_URL> --b <BASE_URL>
```

## Examples

### Example 1: Read expiration from file

```bash
helseid-cli readclientsecretexpiration \
  --ClientId "37a08838-db82-4de0-bfe1-bed876e7086e" \
  --ExistingPrivateJwkPath "C:\keys\client_private.json" \
  --AuthorityUrl "https://helseid-sts.test.nhn.no" \
  --BaseAddress "https://api.selvbetjening.test.nhn.no"
```

### Example 2: Read expiration with inline key

```bash
helseid-cli readclientsecretexpiration \
  --ClientId "37a08838-db82-4de0-bfe1-bed876e7086e" \
  --ExistingPrivateJwk '{"alg":"PS512","d":"...private key data..."}'
  --AuthorityUrl "https://helseid-sts.test.nhn.no" \
  --BaseAddress "https://api.selvbetjening.test.nhn.no"
```

## Working with Escaped JSON from HelseID API

### Using HelseID API responses with PowerShell

When HelseID APIs return JWK data, it often comes with escaped quotes like: `{\"kty\":\"RSA\",\"kid\":\"...\"}`

#### Best practice: PowerShell variable (preserves API response exactly)

```powershell
# Get JWK from API response - use as-is without modification
$apiJwkResponse = '{\"kty\":\"RSA\",\"kid\":\"my-key-2024\",\"d\":\"MIIEowIBAAKCAQEA...\",\"n\":\"xGHNF7qI...\",\"e\":\"AQAB\"}'

dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk $apiJwkResponse --AuthorityUrl "https://helseid-sts.test.nhn.no" --BaseAddress "https://api.selvbetjening.test.nhn.no"
```

#### Alternative: PowerShell here-string

```powershell
# Wrap API response in here-string without modification
$json = @"
{\"kty\":\"RSA\",\"kid\":\"my-key-2024\",\"d\":\"MIIEowIBAAKCAQEA...\",\"n\":\"xGHNF7qI...\",\"e\":\"AQAB\"}
"@

dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk $json --AuthorityUrl "https://helseid-sts.test.nhn.no" --BaseAddress "https://api.selvbetjening.test.nhn.no"
```

### Important: Avoid Direct Command Line Usage with Escaped JSON

```powershell
# This will fail due to shell parsing issues:
dotnet run -- readclientsecretexpiration --ClientId "my-client-id" --ExistingPrivateJwk "{\"kty\":\"RSA\"}" --AuthorityUrl "https://helseid-sts.test.nhn.no" --BaseAddress "https://api.selvbetjening.test.nhn.no"
```

## Output

The command outputs the expiration date in a human-readable format:

### Successful response

```text
Environment: Production
Client secret expiration date: 2025-06-27 14:30:00
```

### Error response

```text
Environment: Production
Failed to read client secret expiration: Unauthorized
```

### No expiration date available

```text
Environment: Production
Client secret expiration date not available in response
```

## Exit Codes

- `0`: Success - expiration date retrieved successfully
- Non-zero: Error occurred (authentication failure, network error, etc.)

## Integration with Automation Systems

### Octopus Deploy Integration

This command can be integrated into Octopus Deploy runbooks to monitor client secret expiration:

```powershell
# PowerShell script step in Octopus Deploy
$clientId = "#{ClientId}"
$privateKeyPath = "#{PrivateKeyPath}"
$authority = "#{AuthorityUrl}"
$baseAddress = "#{BaseAddress}"

$result = & helseid-cli readclientsecretexpiration -c $clientId -ep $privateKeyPath -a $authority -b $baseAddress

if ($LASTEXITCODE -eq 0) {
    Write-Host "Successfully retrieved expiration date: $result"
    
    # Extract date and calculate days until expiry
    if ($result -match "(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})") {
        $expirationDate = [DateTime]::Parse($matches[1])
        $daysUntilExpiry = ($expirationDate - (Get-Date)).Days
        
        Write-Host "Days until expiry: $daysUntilExpiry"
        
        if ($daysUntilExpiry -lt 30) {
            Write-Warning "Secret expires soon - scheduling renewal"
            # Add renewal logic here
        }
    }
} else {
    Write-Error "Failed to retrieve expiration date: $result"
    Exit 1
}
```

### Bash Script Example

```bash
#!/bin/bash
# Capture exit code and output
output=$(helseid-cli readclientsecretexpiration --ClientId "$CLIENT_ID" --ExistingPrivateJwkPath "$KEY_PATH" 2>&1 --AuthorityUrl "$AUTHORITY_URL" -BaseAddress "$BASE_ADDRESS")
exit_code=$?

if [ $exit_code -eq 0 ]; then
    echo "Output: $output"
    # Extract date from output (assuming format: "Client secret expiration date: 2025-06-27 14:30:00")
    expiration_date=$(echo "$output" | grep -o "[0-9]\{4\}-[0-9]\{2\}-[0-9]\{2\} [0-9]\{2\}:[0-9]\{2\}:[0-9]\{2\}")
    
    if [ ! -z "$expiration_date" ]; then
        echo "Secret expires at: $expiration_date"
        # Calculate days until expiration (requires date command)
        expiry_seconds=$(date -d "$expiration_date" +%s)
        current_seconds=$(date +%s)
        days_until_expiry=$(( ($expiry_seconds - $current_seconds) / 86400 ))
        echo "Days until expiry: $days_until_expiry"
        
        if [ $days_until_expiry -lt 30 ]; then
            echo "WARNING: Secret expires soon - schedule renewal!"
        fi
    fi
else
    echo "Failed to read expiration: $output"
    exit 1
fi
```

### PowerShell Real-World Example with API Response

```powershell
# Real-world example: Get JWK from HelseID API and check expiration
$clientId = "my-client-id"

# API response comes with escaped quotes - use as-is
$jwkFromApi = '{\"kty\":\"RSA\",\"kid\":\"my-key-2024\",\"d\":\"MIIEowIBAAKCAQEA...\"}'

$authority = "https://helseid-sts.test.nhn.no",
$baseAddress = "https://api.selvbetjening.test.nhn.no"

# Pass API response directly without modification
$result = & helseid-cli readclientsecretexpiration --ClientId $clientId --ExistingPrivateJwk $jwkFromApi --AuthorityUrl $authority -BaseAddress $baseAddress

if ($LASTEXITCODE -eq 0) {
    Write-Host "Secret expiration retrieved: $result"
    
    # Extract date and calculate days until expiry
    if ($result -match "(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})") {
        $expirationDate = [DateTime]::Parse($matches[1])
        $daysUntilExpiry = ($expirationDate - (Get-Date)).Days
        
        Write-Host "Expires: $expirationDate ($daysUntilExpiry days)"
        
        if ($daysUntilExpiry -lt 30) {
            Write-Warning "Secret expires soon - schedule renewal!"
            # Add renewal logic or notification here
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
- Output format is designed to be easily parsed by automation scripts
- Expiration date enables precise date calculations and automation logic
