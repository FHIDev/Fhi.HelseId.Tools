using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fhi.HelseIdSelvbetjening.CLI.IntegrationTests.Setup
{
    internal class CommandLineBuilder
    {
        public static async Task<int> CommandLineBuilderInvokerAsync(ParseResult parseResult)
        {
            var invocationConfig = new InvocationConfiguration
            {
                EnableDefaultExceptionHandler = false
            };

            int exitCode;
            try
            {
                exitCode = await parseResult.InvokeAsync(invocationConfig);
                return exitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Test caught exception: {ex.Message}");
                exitCode = 1;
                return exitCode;
            }
        }
    }
}
