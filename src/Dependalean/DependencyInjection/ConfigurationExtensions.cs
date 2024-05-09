using Dependalean.Configuration;
using Dependalean.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dependalean.DependencyInjection
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Adds Dependaclean services to the specified service collection, including cleanup manager and entity dependency graph.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="configure"></param>
        /// <returns>The modified service collection.</returns>
        public static IServiceCollection AddDependaclean<T>(this IServiceCollection services, Action<CleanupConfiguration> configure) where T: DbContext
        {
            var cleanupConfiguration = new CleanupConfiguration();
            configure?.Invoke(cleanupConfiguration);

            return services
                    .AddEntityDependencyGraph<T>()
                    .AddCleanupManager(cleanupConfiguration);
        }
    }
}