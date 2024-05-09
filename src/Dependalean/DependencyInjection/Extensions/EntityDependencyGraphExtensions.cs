using Dependalean.Abstractions;
using Dependalean.Graph;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace Dependalean.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for registering EntityDependencyGraph.
/// </summary>
public static class EntityDependencyGraphExtensions
{
    /// <summary>
    /// Adds EntityDependencyGraph as a scoped service and dynamically acquires the DbContext.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddEntityDependencyGraph<T>(this IServiceCollection services) where T: DbContext
    {
        return services.AddScoped<IEntityDependencyGraph>(provider =>
        {
            // Resolve the DbContext dynamically
            var dbContextFactory = provider.GetRequiredService<IDbContextFactory<T>>();

            // Create an instance of EntityDependencyGraph using the resolved DbContext
            return new EntityDependencyGraph(dbContextFactory.CreateDbContext());
        });
    }
}

public interface IDbContext
{
    IModel Model { get; set; }
}