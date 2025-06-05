# Global Exception Handling in CLI

## Overview

This CLI application now implements a centralized global exception handler similar to what you would find in an MVC application. This allows us to handle all exceptions in one place and remove individual try-catch blocks from command implementations.

## How It Works

### 1. GlobalExceptionHandler Class

The `GlobalExceptionHandler` class is located in `Infrastructure/GlobalExceptionHandler.cs` and provides:

- **Centralized Exception Handling**: All exceptions are caught and processed in one location
- **User-Friendly Error Messages**: Different exception types are mapped to meaningful user messages
- **Comprehensive Logging**: Full exception details are logged for debugging purposes
- **Exit Code Management**: Proper exit codes are set to indicate failure to calling processes

### 2. Exception Handler Wrapping

The global exception handler provides `WrapHandler` methods that can wrap any command handler:

```csharp
// For handlers with no parameters
public Func<Task> WrapHandler(Func<Task> handler)

// For handlers with 1 parameter
public Func<T, Task> WrapHandler<T>(Func<T, Task> handler)

// For handlers with 2 parameters
public Func<T1, T2, Task> WrapHandler<T1, T2>(Func<T1, T2, Task> handler)

// And additional overloads for more parameters...
```

### 3. Integration with Command Builders

Each command builder now:

1. **Registers the GlobalExceptionHandler** in the DI container:
   ```csharp
   public Action<IServiceCollection>? Services => services =>
   {
       // ... other services
       services.AddTransient<GlobalExceptionHandler>();
   };
   ```

2. **Uses the exception handler to wrap command handlers**:
   ```csharp
   var exceptionHandler = host.Services.GetRequiredService<GlobalExceptionHandler>();
   
   command.SetHandler(exceptionHandler.WrapHandler(async (param1, param2) =>
   {
       // Command implementation without try-catch
       var service = new SomeService();
       await service.ExecuteAsync();
   }), option1, option2);
   ```

## Example Implementation

Here's how the `GenerateKeyCommandBuilder` was updated:

### Before (with try-catch):
```csharp
generateKeyCommand.SetHandler(async (string? keyFileNamePrefix, string? keyDirectory) =>
{
    try
    {
        var parameters = new GenerateKeyParameters
        {
            KeyFileNamePrefix = keyFileNamePrefix,
            KeyDirectory = keyDirectory
        };

        var logger = host.Services.GetRequiredService<ILogger<KeyGeneratorService>>();
        var fileWriter = host.Services.GetRequiredService<IFileHandler>();
        var service = new KeyGeneratorService(parameters, fileWriter, logger);

        await service.ExecuteAsync();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        Environment.ExitCode = 1;
    }
}, keyNameOption, keyDirOption);
```

### After (with global exception handler):
```csharp
var exceptionHandler = host.Services.GetRequiredService<GlobalExceptionHandler>();

generateKeyCommand.SetHandler(exceptionHandler.WrapHandler(async (string? keyFileNamePrefix, string? keyDirectory) =>
{
    var parameters = new GenerateKeyParameters
    {
        KeyFileNamePrefix = keyFileNamePrefix,
        KeyDirectory = keyDirectory
    };

    var logger = host.Services.GetRequiredService<ILogger<KeyGeneratorService>>();
    var fileWriter = host.Services.GetRequiredService<IFileHandler>();
    var service = new KeyGeneratorService(parameters, fileWriter, logger);

    await service.ExecuteAsync();
}), keyNameOption, keyDirOption);
```

## Exception Type Mapping

The global exception handler provides user-friendly messages for common exception types:

- **FileNotFoundException**: "Error: File not found - {filename}"
- **DirectoryNotFoundException**: "Error: The specified directory was not found."
- **UnauthorizedAccessException**: "Error: Access denied. Check your permissions."
- **HttpRequestException**: "Error: Network request failed - {message}"
- **ArgumentException**: "Error: Invalid argument - {message}"
- **InvalidOperationException**: "Error: Invalid operation - {message}"
- **TaskCanceledException**: "Error: Operation was cancelled or timed out."
- **NotSupportedException**: "Error: Operation not supported - {message}"
- **Default**: "Error: An unexpected error occurred - {message}. Check logs for details."

## Benefits

1. **Consistency**: All exceptions are handled in a consistent manner across the application
2. **Maintainability**: Exception handling logic is centralized and easy to modify
3. **Clean Code**: Command implementations focus on business logic without boilerplate exception handling
4. **Better User Experience**: Users get meaningful error messages instead of raw exception details
5. **Debugging**: Full exception details are still logged for development and debugging purposes

## Testing

The global exception handler can be tested by:

1. **Invalid Parameters**: Using commands with missing required parameters
2. **File System Errors**: Accessing non-existent files or directories with insufficient permissions
3. **Network Errors**: Making HTTP requests to invalid endpoints
4. **Custom Exceptions**: Throwing specific exceptions in service implementations

## Future Enhancements

- Add support for custom exception types specific to the application domain
- Implement retry logic for transient failures
- Add structured logging with correlation IDs
- Include telemetry and metrics collection for exception tracking

## Recent Enhancements (v2.0)

### Correlation IDs for Request Tracking

- Every command execution now gets a unique 8-character correlation ID
- Correlation IDs are logged at the start and end of command execution
- Exception logs include correlation IDs for easier debugging
- Debug builds show correlation IDs in error messages

### Enhanced Exception Type Support

- Added support for `ArgumentNullException` with parameter name
- Added support for `SecurityException`, `JsonException`, `TimeoutException`, `FormatException`
- Improved error message clarity and specificity
- Added specific exit codes for different error categories

### Exit Code Mapping

- **1**: Generic/Unknown errors
- **2**: File/Directory not found errors  
- **3**: Permission/Access denied errors
- **4**: Invalid arguments or missing parameters
- **5**: Network/HTTP request errors
- **6**: Timeout/Cancellation errors
- **7**: Security violations
- **8**: Format/JSON parsing errors
- **9**: Timeout errors
- **10**: Unsupported operation errors

### Synchronous Handler Support

- Added support for wrapping synchronous (non-async) command handlers
- Consistent exception handling for both sync and async operations

### Complete Parameter Coverage

- Added wrapper methods for 4 and 5 parameter handlers
- Now supports 0-6 parameter handlers comprehensively

### Enhanced Logging

- Detailed parameter logging (can be configured)
- Inner exception details are now logged separately
- Stack trace information included in logs
- Debug-specific error information

### Configuration Support

- Added `GlobalExceptionHandlerOptions` for customization
- Configurable parameter logging behavior
- Custom exit code mappings
- Custom error message templates
