using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fhi.HelseId.ClientSecret.App.Tests
{
    public class AppTests
    {
        [Test]
        public async Task MainProgram()
        {
            var args = new[] { "generatekey" };

            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            var host = new HostBuilder()
               .ConfigureAppConfiguration(config =>
               {
                   config.AddCommandLine(args);
               })
                .ConfigureServices((context, services) =>
                {
                    Program.ConfigureServices(args, context, services);
                })
                .Build();

            await host.StartAsync();

            var output = stringWriter.ToString().Trim();

            await host.StopAsync(TimeSpan.FromSeconds(10));
        }

        [Test]
        public async Task MainProgram_ShouldGenerateKeyOutput()
        {
            // Arrange
            var args = new[] { "generatekey" };

            // Redirect console output
            var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Act
            await Task.Run(() => Program.Main(args)); // If Main is asynchronous, ensure it completes.

            // Assert
            var output = stringWriter.ToString().Trim();  // Get the output
        }
    }
}
