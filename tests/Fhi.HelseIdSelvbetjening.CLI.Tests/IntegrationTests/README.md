# Configuration Integration Tests

## Overview

This directory contains comprehensive tests for configuration binding scenarios that were previously broken in the HelseID CLI tool.

## Background

The original issue was that command builders were creating temporary service providers to access configuration during command building, which resulted in `NullReferenceException` when configuration-dependent services tried to access configuration values that were never properly bound.

## Fixed Issues

1. **Command builders no longer create temporary service providers** - Configuration binding is now handled exclusively by the HostBuilder
2. **Proper configuration registration** - IOptions<SelvbetjeningConfiguration> is registered through the standard ASP.NET Core options pattern
3. **Service resolution with configuration** - Services that depend on configuration can now be resolved without null reference exceptions

## Test Coverage

### ConfigurationBindingTests.cs

Tests for general configuration binding scenarios:

- HostBuilder configuration loading for Test and Production environments
- Command builder configuration binding verification  
- Service resolution with proper configuration
- Options pattern conflict prevention
- Mocked service scenarios
- Invalid environment handling

### ConfigurationBugRegressionTests.cs

Tests specifically targeting the original bug scenarios:

- Verification that command builders no longer create temporary service providers
- HostBuilder proper configuration registration
- Simulation of the original bug scenario (demonstrating what was broken)
- Proper configuration binding demonstration
- ServiceCollectionExtensions conflict prevention

## Test Design Principles

### Resilient Assertions

Tests use pattern-based assertions rather than exact value matches to avoid brittleness:

- `Does.StartWith("https://")` instead of exact URL matches
- `Does.Contain("test")` for test environment verification
- `Does.Not.Contain("test")` for production environment verification

This approach ensures tests remain valid even if:

- Configuration URLs change
- Class names are refactored
- Environment-specific values are updated

### Environment Isolation

Each test properly sets up and tears down environment variables to ensure isolation and prevent test interference.

## Running the Tests

```bash
# Run all configuration tests
dotnet test tests/Fhi.HelseIdSelvbetjening.CLI.Tests/IntegrationTests/

# Run specific test file
dotnet test tests/Fhi.HelseIdSelvbetjening.CLI.Tests/IntegrationTests/ConfigurationBindingTests.cs
```

## Future Considerations

- Additional edge cases for configuration validation scenarios
- Tests for configuration reload scenarios
- Performance tests for configuration binding overhead
