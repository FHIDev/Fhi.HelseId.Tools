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


    }
}
