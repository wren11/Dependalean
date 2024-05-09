using Dependalean.Abstractions;
using Dependalean.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dependalean.DependencyInjection.Extensions
{
    /// <summary>
    /// Extension methods for registering and configuring Dependaclean services.
    /// </summary>
    public static class DependacleanExtensions
    {
        /// <summary>
        /// Adds Dependaclean services to the specified service collection.
        /// </summary>
        /// <param name="services">The service collection to add the services to.</param>
        /// <param name="cleanupConfiguration"></param>
        public static IServiceCollection AddCleanupManager(this IServiceCollection services, CleanupConfiguration cleanupConfiguration)
        {
            services.AddScoped<IEntityCleanupManager, EntityCleanupManager>();
            services.AddSingleton(cleanupConfiguration);
            return services;
        }
    }
}