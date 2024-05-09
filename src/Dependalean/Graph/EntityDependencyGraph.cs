using Dependalean.Abstractions;
using Microsoft.EntityFrameworkCore;

// ReSharper disable MethodHasAsyncOverloadWithCancellation

namespace Dependalean.Graph;

/// <summary>
/// Represents a graph of entity dependencies.
/// </summary>
public class EntityDependencyGraph : IEntityDependencyGraph
{
    /// <summary>
    /// The DbContext associated with the entity dependency graph.
    /// </summary>
    public DbContext Context { get; }

    /// <summary>
    /// The root node of the entity dependency graph.
    /// </summary>
    public DependencyNode Root { get; set; }

    /// <summary>
    /// Initializes a new instance of the EntityDependencyGraph class with the specified DbContext.
    /// </summary>
    /// <param name="context">The DbContext representing the database context.</param>
    public EntityDependencyGraph(DbContext context)
    {
        Context = context;

        // Create dependency nodes for each entity type and establish dependencies between them
        var nodes = context.Model.GetEntityTypes()
            .DistinctBy(entityType => entityType.GetTableName())
            .ToDictionary(
                e => e.GetTableName()!,
                e => new DependencyNode(e.GetTableName()!));

        foreach (var entityType in context.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName()!;
            var node = nodes[tableName];

            foreach (var fk in entityType.GetForeignKeys())
            {
                var principalTableName = fk.PrincipalEntityType.GetTableName() ?? fk.PrincipalEntityType.ClrType.Name;
                var dependentNode = nodes[principalTableName];
                dependentNode.AddDependency(node);
            }
        }

        // Set up the root node and its dependencies
        Root = new DependencyNode("Root");
        var allDependencies = nodes.Values.SelectMany(n => n.Dependencies).ToHashSet();
        var rootNodes = nodes.Values.Where(n => !allDependencies.Contains(n)).ToList();

        foreach (var rootNode in rootNodes)
        {
            Root.AddDependency(rootNode);
        }
    }

    /// <summary>
    /// Traverses the dependency graph and performs an action on each visited node.
    /// </summary>
    /// <param name="onNodeVisited">The action to perform on each visited node.</param>
    public void TraverseDependencyGraph(Action<string>? onNodeVisited)
    {
        ArgumentNullException.ThrowIfNull(onNodeVisited);
        var visited = new HashSet<string>();
        Root.Traverse(onNodeVisited, visited);
    }
}