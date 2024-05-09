using Dependalean.Graph;
using Microsoft.EntityFrameworkCore;

namespace Dependalean.Abstractions;

/// <summary>
/// Represents a dependency graph of entities.
/// </summary>
public interface IEntityDependencyGraph
{
    /// <summary>
    /// Gets the DbContext associated with the dependency graph.
    /// </summary>
    DbContext Context { get; }

    /// <summary>
    /// Gets or sets the root node of the dependency graph.
    /// </summary>
    DependencyNode Root { get; set; }

    /// <summary>
    /// Traverses the dependency graph and performs an action on each visited node.
    /// </summary>
    /// <param name="onNodeVisited">The action to perform on each visited node.</param>
    void TraverseDependencyGraph(Action<string>? onNodeVisited);
}