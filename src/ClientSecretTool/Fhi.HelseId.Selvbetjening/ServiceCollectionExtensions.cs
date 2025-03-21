using Fhi.HelseId.Selvbetjening.Services;
using Fhi.HelseId.Selvbetjening.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fhi.HelseId.Selvbetjening
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSelvbetjeningServices(this IServiceCollection services)
        {
            services.AddSingleton<IOptions<SelvbetjeningConfiguration>, OptionsManager<SelvbetjeningConfiguration>>();
            services.AddTransient<IHelseIdSelvbetjeningService, HelseIdSelvbetjeningService>();
            services.AddHttpClient();

            return services;
        }
    }
}
