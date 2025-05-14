using Fhi.HelseIdSelvbetjening.Services;
using Fhi.HelseIdSelvbetjening.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Fhi.HelseId.Selvbetjening
{
    /// <summary>
    /// Extensions for adding Selvbetjening services to an application
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add SelvbetjeningConfiguration and required services
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <returns></returns>
        public static IServiceCollection AddSelvbetjeningServices(this IServiceCollection services)
        {
            services.AddSingleton<IOptions<SelvbetjeningConfiguration>, OptionsManager<SelvbetjeningConfiguration>>();
            services.AddTransient<IHelseIdSelvbetjeningService, HelseIdSelvbetjeningService>();
            return services;
        }
    }
}
