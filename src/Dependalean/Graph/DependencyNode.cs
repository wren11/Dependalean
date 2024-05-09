namespace Dependalean.Graph;

/// <summary>
/// Represents a node in a dependency graph.
/// </summary>
/// <remarks>
/// Initializes a new instance of the DependencyNode class with the specified name.
/// </remarks>
/// <param name="name">The name of the dependency node.</param>
public class DependencyNode(string name)
{
    /// <summary>
    /// Gets or sets the name of the dependency node.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Gets or sets the dependencies of the current node.
    /// </summary>
    public HashSet<DependencyNode> Dependencies { get; } = [];

    /// <summary>
    /// Adds a dependency to the current node.
    /// </summary>
    /// <param name="node">The dependency node to add.</param>
    public void AddDependency(DependencyNode node)
    {
        Dependencies.Add(node);
    }

    /// <summary>
    /// Traverses the dependency graph starting from the current node and performs an action on each visited node.
    /// </summary>
    /// <param name="visitAction">The action to perform on each visited node.</param>
    /// <param name="visited">A set to track visited nodes and avoid circular dependencies.</param>
    public void Traverse(Action<string> visitAction, HashSet<string> visited)
    {
        if (!visited.Add(Name))
        {
            visitAction($"Already visited {Name}, skipping to avoid circular dependency.");
            return;
        }

        foreach (var dependency in Dependencies)
        {
            dependency.Traverse(visitAction, visited);
        }

        visitAction(Name);
    }
}