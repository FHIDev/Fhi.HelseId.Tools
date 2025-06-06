# Read Client Secret Expiration Command

This document explains how to use the `readclientsecretexpiration` command, which checks the expiration date for a HelseID client secret.

## Command Usage

The command allows you to check when a client secret expires in two ways:

### 1. Using a Private Key File

```bash
helseid-cli readclientsecretexpiration --clientId "your-client-id" --existingPrivateJwkPath "path/to/private.json"
```

Or with short options:

```bash
helseid-cli readclientsecretexpiration -c "your-client-id" -p "path/to/private.json"
```

### 2. Using a Direct Private Key Value

```bash
helseid-cli readclientsecretexpiration --clientId "your-client-id" --existingPrivateJwk "{...jwk json...}"
```

Or with short options:

```bash
helseid-cli readclientsecretexpiration -c "your-client-id" -j "{...jwk json...}"
```

## Prerequisites

To use this command, you need:

1. Access to HelseID environment
2. A client with the `nhn:selvbetjening/client` scope
3. Valid private key for the client

## Expected Output

When successful, the command will output:

- Information about the client ID being checked
- The expiration date of the client secret (if available)
- Or a message indicating that the expiration date is not available

## Sample Private Key Format

Your private key file should contain a valid RSA private key in JWK format:

```json
{
  "kty": "RSA",
  "d": "...",
  "n": "...",
  "e": "AQAB",
  "use": "sig",
  "kid": "..."
}
```
