# Acceptance Tests for ReadClientSecretExpiration Command

This document explains how to run the acceptance tests for the new `readclientsecretexpiration` command.

## Overview

The acceptance tests verify that the CLI command works end-to-end against a real test environment. These tests are marked as `[Explicit]` and must be run manually.

## Available Tests

### 1. ReadClientSecretExpiration_FromPath

Tests reading client secret expiration using a private key file.

### 2. ReadClientSecretExpiration_FromDirectKey  

Tests reading client secret expiration using a private key value directly.

## Prerequisites

Before running these tests, you need:

1. **Test Environment**: Access to HelseID test environment
2. **Valid Test Client**: A client configured with:
   - The `nhn:selvbetjening/client` scope
   - Valid client secret (private key)
3. **Test Data**: Private key file in `TestData/oldkey.json`

## Setup Instructions

### 1. Configure Test Client ID

Edit the test methods in `AcceptanceTests.cs` and replace:

```csharp
var clientId = "88d474a8-07df-4dc4-abb0-6b759c2b99ec"; // Replace with your test client ID
```

### 2. Prepare Test Data Directory

Create a `TestData` directory in the test project root (not in bin folder) and add your private key file:

```
tests/Fhi.HelseIdSelvbetjening.CLI.Tests/TestData/
└── oldkey.json  // Your test client's private key
```

**Important**: The test will automatically locate the correct TestData directory relative to the test project, not the bin output directory.

### 3. Set Environment

The tests automatically set `DOTNET_ENVIRONMENT=Test` to use test configuration.

## Running the Tests

### Via Visual Studio

1. Open Test Explorer
2. Find the acceptance tests under `Fhi.HelseIdSelvbetjening.CLI.AcceptanceTests`
3. Right-click on the specific test you want to run
4. Select "Run Selected Tests"

### Via Command Line

```powershell
# Run specific acceptance test
cd "c:\git\Fhi.HelseId.Tools"
dotnet test tests\Fhi.HelseIdSelvbetjening.CLI.Tests\Fhi.HelseIdSelvbetjening.CLI.Tests.csproj --filter "ReadClientSecretExpiration_FromPath"

# Run all acceptance tests
dotnet test tests\Fhi.HelseIdSelvbetjening.CLI.Tests\Fhi.HelseIdSelvbetjening.CLI.Tests.csproj --filter "AcceptanceTests"
```

## Expected Results

### Success Case

- Exit code: 0
- Output contains: "Reading client secret expiration for client"
- May contain expiration date or "expiration date not available"

### Failure Case

- Exit code: non-zero
- Output contains error message explaining the issue

## Sample Private Key Format

Your `oldkey.json` should contain a valid RSA private key in JWK format:

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

## Troubleshooting

### Common Issues

1. **"Client not found or access denied"**
   - Verify client ID is correct
   - Ensure client has `nhn:selvbetjening/client` scope
   - Check private key matches the client

2. **"No private key provided"**
   - Verify `TestData/oldkey.json` exists and is valid JSON
   - Check file path is correct

3. **Network/Environment Issues**
   - Verify test environment is accessible
   - Check if authentication service is responding

### Debug Tips

- The tests capture console output for inspection
- Exit codes and output are printed for debugging
- Use the explicit nature of tests to run them individually

## Integration with CI/CD

While these are manual acceptance tests, you could potentially:

1. Set up dedicated test environment with known test clients
2. Store test keys securely in CI/CD secrets
3. Run as part of release validation (not regular CI)

Note: Be careful with real credentials in automated environments.
