using Fhi.HelseIdSelvbetjening.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fhi.HelseIdSelvbetjening
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
            services.AddTransient<IHelseIdSelvbetjeningService, HelseIdSelvbetjeningService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddHttpClient();

            return services;
        }
    }
}
