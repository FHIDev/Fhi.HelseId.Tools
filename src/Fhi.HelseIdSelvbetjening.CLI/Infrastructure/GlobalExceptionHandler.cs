using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Security;
using System.Text.Json;

namespace Fhi.HelseIdSelvbetjening.CLI.Infrastructure
{    /// <summary>
    /// Global exception handler for CLI commands
    /// </summary>
    public class GlobalExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private static readonly AsyncLocal<string?> _correlationId = new();

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets or sets the correlation ID for the current execution context
        /// </summary>
        public static string CorrelationId
        {
            get => _correlationId.Value ?? Guid.NewGuid().ToString("N")[..8];
            set => _correlationId.Value = value;
        }

        /// <summary>
        /// Wraps a command handler with global exception handling
        /// </summary>
        /// <param name="handler">The original command handler</param>
        /// <returns>A wrapped handler with exception handling</returns>
        public Func<Task> WrapHandler(Func<Task> handler)
        {
            return async () =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing command handler. Correlation ID: {CorrelationId}", correlationId);
                    handler = HandlerWithCorrelationId(handler, correlationId);
                    await handler();
                    _logger.LogDebug("Command handler completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(ex, correlationId);
                }
            };
        }

        /// <summary>
        /// Wraps a command handler with global exception handling
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <returns>A wrapped handler with exception handling</returns>
        public Func<T, Task> WrapHandler<T>(Func<T, Task> handler)
        {
            return async (T param) =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing command handler with parameter. Correlation ID: {CorrelationId}, Parameter: {Parameter}", correlationId, param);
                    handler = HandlerWithCorrelationId(handler, correlationId);
                    await handler(param);
                    _logger.LogDebug("Command handler with parameter completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(ex, correlationId);
                }
            };
        }

        /// <summary>
        /// Wraps a command handler with global exception handling
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <returns>A wrapped handler with exception handling</returns>
        public Func<T1, T2, Task> WrapHandler<T1, T2>(Func<T1, T2, Task> handler)
        {
            return async (T1 param1, T2 param2) =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing command handler with parameters. Correlation ID: {CorrelationId}, Parameters: {Parameter1}, {Parameter2}", correlationId, param1, param2);
                    handler = HandlerWithCorrelationId(handler, correlationId);
                    await handler(param1, param2);
                    _logger.LogDebug("Command handler with parameters completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(ex, correlationId);
                }
            };
        }

        /// <summary>
        /// Wraps a command handler with global exception handling for handlers with 3 parameters
        /// </summary>
        public Func<T1, T2, T3, Task> WrapHandler<T1, T2, T3>(Func<T1, T2, T3, Task> handler)
        {
            return async (T1 param1, T2 param2, T3 param3) =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing command handler with parameters. Correlation ID: {CorrelationId}, Parameters: {Parameter1}, {Parameter2}, {Parameter3}", correlationId, param1, param2, param3);
                    handler = HandlerWithCorrelationId(handler, correlationId);
                    await handler(param1, param2, param3);
                    _logger.LogDebug("Command handler with parameters completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(ex, correlationId);
                }
            };
        }

        /// <summary>
        /// Wraps a command handler with global exception handling for handlers with 4 parameters
        /// </summary>
        public Func<T1, T2, T3, T4, Task> WrapHandler<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> handler)
        {
            return async (T1 param1, T2 param2, T3 param3, T4 param4) =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing command handler with parameters. Correlation ID: {CorrelationId}, Parameters: {Parameter1}, {Parameter2}, {Parameter3}, {Parameter4}", correlationId, param1, param2, param3, param4);
                    handler = HandlerWithCorrelationId(handler, correlationId);
                    await handler(param1, param2, param3, param4);
                    _logger.LogDebug("Command handler with parameters completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(ex, correlationId);
                }
            };
        }

        /// <summary>
        /// Wraps a command handler with global exception handling for handlers with 5 parameters
        /// </summary>
        public Func<T1, T2, T3, T4, T5, Task> WrapHandler<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> handler)
        {
            return async (T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing command handler with parameters. Correlation ID: {CorrelationId}, Parameters: {Parameter1}, {Parameter2}, {Parameter3}, {Parameter4}, {Parameter5}", correlationId, param1, param2, param3, param4, param5);
                    handler = HandlerWithCorrelationId(handler, correlationId);
                    await handler(param1, param2, param3, param4, param5);
                    _logger.LogDebug("Command handler with parameters completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(ex, correlationId);
                }
            };
        }

        /// <summary>
        /// Wraps a command handler with global exception handling for handlers with 6 parameters
        /// </summary>
        public Func<T1, T2, T3, T4, T5, T6, Task> WrapHandler<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> handler)
        {
            return async (T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing command handler with parameters. Correlation ID: {CorrelationId}, Parameters: {Parameter1}, {Parameter2}, {Parameter3}, {Parameter4}, {Parameter5}, {Parameter6}", correlationId, param1, param2, param3, param4, param5, param6);
                    handler = HandlerWithCorrelationId(handler, correlationId);
                    await handler(param1, param2, param3, param4, param5, param6);
                    _logger.LogDebug("Command handler with parameters completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(ex, correlationId);
                }
            };
        }

        /// <summary>
        /// Wraps a synchronous command handler with global exception handling
        /// </summary>
        /// <param name="handler">The original synchronous command handler</param>
        /// <returns>A wrapped handler with exception handling</returns>
        public Action WrapHandler(Action handler)
        {
            return () =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing synchronous command handler. Correlation ID: {CorrelationId}", correlationId);
                    handler();
                    _logger.LogDebug("Synchronous command handler completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    HandleExceptionAsync(ex, correlationId).GetAwaiter().GetResult();
                }
            };
        }

        /// <summary>
        /// Wraps a synchronous command handler with global exception handling
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="handler">The original synchronous command handler</param>
        /// <returns>A wrapped handler with exception handling</returns>
        public Action<T> WrapHandler<T>(Action<T> handler)
        {
            return (T param) =>
            {
                var correlationId = Guid.NewGuid().ToString("N")[..8];
                try
                {
                    _logger.LogDebug("Executing synchronous command handler with parameter. Correlation ID: {CorrelationId}, Parameter: {Parameter}", correlationId, param);
                    handler(param);
                    _logger.LogDebug("Synchronous command handler with parameter completed successfully. Correlation ID: {CorrelationId}", correlationId);
                }
                catch (Exception ex)
                {
                    HandleExceptionAsync(ex, correlationId).GetAwaiter().GetResult();
                }
            };
        }        /// <summary>
        /// Handles exceptions in a centralized way
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        /// <param name="correlationId">The correlation ID for tracking this request</param>
        private async Task HandleExceptionAsync(Exception exception, string correlationId)
        {
            // Log the full exception details with context
            _logger.LogError(exception, 
                "An unhandled exception occurred while executing the command. " +
                "Exception type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}, Correlation ID: {CorrelationId}", 
                exception.GetType().Name, 
                exception.Message,
                exception.StackTrace,
                correlationId);

            // Log inner exceptions if present
            if (exception.InnerException != null)
            {
                _logger.LogError(exception.InnerException,
                    "Inner exception details - Type: {InnerExceptionType}, Message: {InnerMessage}, Correlation ID: {CorrelationId}",
                    exception.InnerException.GetType().Name,
                    exception.InnerException.Message,
                    correlationId);
            }

            // Determine the type of exception and provide appropriate user feedback
            var userMessage = GetUserFriendlyMessage(exception);
            await Console.Error.WriteLineAsync(userMessage);

            // Add a hint for developers in debug builds
#if DEBUG
            await Console.Error.WriteLineAsync($"Debug Info: Check logs for detailed error information. Exception ID: {exception.GetHashCode()}");
#endif

            // Set exit code to indicate failure

            Environment.ExitCode = GetExitCodeForException(exception);
        }
        /// <summary>
        /// Gets a user-friendly error message based on the exception type
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>A user-friendly error message</returns>
        private static string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException argNullEx => $"Error: Required parameter is missing - {argNullEx.ParamName}",
                ArgumentException argEx => $"Error: Invalid argument - {argEx.Message}",
                FileNotFoundException fileEx => $"Error: File not found - {fileEx.FileName}",
                DirectoryNotFoundException => "Error: The specified directory was not found.",
                UnauthorizedAccessException => "Error: Access denied. Check your permissions.",
                SecurityException secEx => $"Error: Security violation - {secEx.Message}",
                HttpRequestException httpEx => $"Error: Network request failed - {httpEx.Message}",
                TaskCanceledException => "Error: Operation was cancelled or timed out.",
                TimeoutException => "Error: Operation timed out. Please try again.",
                InvalidOperationException invOpEx => $"Error: Invalid operation - {invOpEx.Message}",
                NotSupportedException notSuppEx => $"Error: Operation not supported - {notSuppEx.Message}",
                FormatException formatEx => $"Error: Invalid format - {formatEx.Message}",
                JsonException jsonEx => $"Error: Invalid JSON format - {jsonEx.Message}",
                _ => $"Error: An unexpected error occurred - {exception.Message}. Check logs for details."
            };
        }

        /// <summary>
        /// Gets the appropriate exit code based on the exception type
        /// </summary>
        /// <param name="exception">The exception</param>
        /// <returns>The exit code to use</returns>
        private static int GetExitCodeForException(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => 4,
                ArgumentException => 4,
                FileNotFoundException => 2,
                DirectoryNotFoundException => 2,
                UnauthorizedAccessException => 3,
                SecurityException => 7,
                HttpRequestException => 5,
                TaskCanceledException => 6,
                TimeoutException => 9,
                FormatException => 8,
                JsonException => 8,
                NotSupportedException => 10,
                _ => 1 // Generic error
            };
        }

        /// <summary>
        /// Sets the correlation ID for the current execution context
        /// </summary>
        /// <param name="correlationId">The correlation ID to set</param>
        public static void SetCorrelationId(string correlationId)
        {
            CorrelationId = correlationId;
        }        /// <summary>
        /// Clears the correlation ID for the current execution context
        /// </summary>
        public static void ClearCorrelationId()
        {
            _correlationId.Value = null;
        }

        /// <summary>
        /// Handler that wraps the original handler and sets the correlation ID
        /// </summary>
        /// <param name="handler">The original command handler</param>
        /// <param name="correlationId">The correlation ID to set</param>
        /// <returns>A wrapped handler with the correlation ID set</returns>
        private static Func<Task> HandlerWithCorrelationId(Func<Task> handler, string correlationId)
        {
            return async () =>
            {
                var previousCorrelationId = CorrelationId;
                try
                {
                    CorrelationId = correlationId;
                    await handler();
                }
                finally
                {
                    CorrelationId = previousCorrelationId;
                }
            };
        }

        /// <summary>
        /// Handler that wraps the original handler and sets the correlation ID
        /// </summary>
        /// <typeparam name="T">Type of the parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <param name="correlationId">The correlation ID to set</param>
        /// <returns>A wrapped handler with the correlation ID set</returns>
        private static Func<T, Task> HandlerWithCorrelationId<T>(Func<T, Task> handler, string correlationId)
        {
            return async (T param) =>
            {
                var previousCorrelationId = CorrelationId;
                try
                {
                    CorrelationId = correlationId;
                    await handler(param);
                }
                finally
                {
                    CorrelationId = previousCorrelationId;
                }
            };
        }

        /// <summary>
        /// Handler that wraps the original handler and sets the correlation ID
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <param name="correlationId">The correlation ID to set</param>
        /// <returns>A wrapped handler with the correlation ID set</returns>
        private static Func<T1, T2, Task> HandlerWithCorrelationId<T1, T2>(Func<T1, T2, Task> handler, string correlationId)
        {
            return async (T1 param1, T2 param2) =>
            {
                var previousCorrelationId = CorrelationId;
                try
                {
                    CorrelationId = correlationId;
                    await handler(param1, param2);
                }
                finally
                {
                    CorrelationId = previousCorrelationId;
                }
            };
        }

        /// <summary>
        /// Handler that wraps the original handler and sets the correlation ID
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <typeparam name="T3">Type of the third parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <param name="correlationId">The correlation ID to set</param>
        /// <returns>A wrapped handler with the correlation ID set</returns>
        private static Func<T1, T2, T3, Task> HandlerWithCorrelationId<T1, T2, T3>(Func<T1, T2, T3, Task> handler, string correlationId)
        {
            return async (T1 param1, T2 param2, T3 param3) =>
            {
                var previousCorrelationId = CorrelationId;
                try
                {
                    CorrelationId = correlationId;
                    await handler(param1, param2, param3);
                }
                finally
                {
                    CorrelationId = previousCorrelationId;
                }
            };
        }

        /// <summary>
        /// Handler that wraps the original handler and sets the correlation ID
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <typeparam name="T3">Type of the third parameter</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <param name="correlationId">The correlation ID to set</param>
        /// <returns>A wrapped handler with the correlation ID set</returns>
        private static Func<T1, T2, T3, T4, Task> HandlerWithCorrelationId<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> handler, string correlationId)
        {
            return async (T1 param1, T2 param2, T3 param3, T4 param4) =>
            {
                var previousCorrelationId = CorrelationId;
                try
                {
                    CorrelationId = correlationId;
                    await handler(param1, param2, param3, param4);
                }
                finally
                {
                    CorrelationId = previousCorrelationId;
                }
            };
        }

        /// <summary>
        /// Handler that wraps the original handler and sets the correlation ID
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <typeparam name="T3">Type of the third parameter</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <param name="correlationId">The correlation ID to set</param>
        /// <returns>A wrapped handler with the correlation ID set</returns>
        private static Func<T1, T2, T3, T4, T5, Task> HandlerWithCorrelationId<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> handler, string correlationId)
        {
            return async (T1 param1, T2 param2, T3 param3, T4 param4, T5 param5) =>
            {
                var previousCorrelationId = CorrelationId;
                try
                {
                    CorrelationId = correlationId;
                    await handler(param1, param2, param3, param4, param5);
                }
                finally
                {
                    CorrelationId = previousCorrelationId;
                }
            };
        }

        /// <summary>
        /// Handler that wraps the original handler and sets the correlation ID
        /// </summary>
        /// <typeparam name="T1">Type of the first parameter</typeparam>
        /// <typeparam name="T2">Type of the second parameter</typeparam>
        /// <typeparam name="T3">Type of the third parameter</typeparam>
        /// <typeparam name="T4">Type of the fourth parameter</typeparam>
        /// <typeparam name="T5">Type of the fifth parameter</typeparam>
        /// <typeparam name="T6">Type of the sixth parameter</typeparam>
        /// <param name="handler">The original command handler</param>
        /// <param name="correlationId">The correlation ID to set</param>
        /// <returns>A wrapped handler with the correlation ID set</returns>
        private static Func<T1, T2, T3, T4, T5, T6, Task> HandlerWithCorrelationId<T1, T2, T3, T4, T5, T6>(
            Func<T1, T2, T3, T4, T5, T6, Task> handler, string correlationId)
        {
            return async (T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6) =>
            {
                var previousCorrelationId = CorrelationId;
                try
                {
                    CorrelationId = correlationId;
                    await handler(param1, param2, param3, param4, param5, param6);
                }
                finally
                {
                    CorrelationId = previousCorrelationId;
                }
            };
        }
    }
}
