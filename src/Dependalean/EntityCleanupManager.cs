using System.Linq.Dynamic.Core;
using System.Reflection;
using Dependalean.Abstractions;
using Dependalean.Configuration;
using Dependalean.Graph;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Dependalean;

/// <summary>
/// Manages cleanup processes for entities based on configured dependencies and filters.
/// </summary>
public class EntityCleanupManager : IEntityCleanupManager
{
    private readonly ILogger<IEntityCleanupManager> _logger;
    private readonly IEntityDependencyGraph _graph;
    private readonly CleanupConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the EntityCleanupManager class.
    /// </summary>
    /// <param name="logger">The logger for capturing runtime information.</param>
    /// <param name="graph">The graph depicting entity dependencies.</param>
    /// <param name="configure">A configuration action to set up cleanup settings.</param>
    public EntityCleanupManager(ILogger<EntityCleanupManager> logger, IEntityDependencyGraph graph, CleanupConfiguration configure)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        _configuration = configure ?? throw new ArgumentNullException(nameof(configure));
        _logger.LogInformation("EntityCleanupManager initialized.");
    }

    /// <summary>
    /// Purges entity records asynchronously based on the specified table name and filter.
    /// </summary>
    /// <param name="tableName">The name of the table to purge.</param>
    /// <param name="filterQuery">The query used to filter records for deletion.</param>
    /// <param name="cancellationToken">Token for operation cancellation.</param>
    public async Task PurgeEntityRecordsAsync(string tableName, string filterQuery, CancellationToken cancellationToken = default)
    {
        // Log the start of cleanup process for the specified table with filter query
        _logger.LogInformation("Starting cleanup of {TableName} with filter {FilterQuery}.", tableName, filterQuery);

        // Find the dependency node corresponding to the specified table name
        var node = FindNodeByName(tableName);

        // Check if the node is found in the dependency graph
        if (node == null!)
        {
            // Log a warning if the table is not found in the dependency graph
            _logger.LogWarning("Table {TableName} not found in the dependency graph.", tableName);
        }

        // Initialize a hash set to accumulate all unique dependency nodes
        var toDelete = new HashSet<DependencyNode>();

        // Collect all dependencies recursively starting from the found node
        if (node != null)
            CollectDependencies(node, toDelete);

        // Begin a database transaction to ensure data integrity during cleanup
        await using var transaction = await _graph.Context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Delete records from the specified table with the provided filter query
            await DeleteEntityRecordsAsync(tableName, filterQuery, cancellationToken);

            // Iterate over dependency nodes in reverse order and delete records for each node
            foreach (var entity in toDelete.Reverse())
            {
                await DeleteEntityRecordsAsync(entity.Name, "", cancellationToken);
            }

            // Commit the transaction if cleanup is successful
            await transaction.CommitAsync(cancellationToken);

            // Log successful completion of cleanup for the specified table
            _logger.LogInformation("Cleanup completed successfully for {TableName}.", tableName);
        }
        catch (Exception ex)
        {
            // Rollback the transaction and log any exceptions that occur during cleanup
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error during cleanup: {ErrorMessage}", ex.Message);
        }
    }

    /// <summary>
    /// Finds a dependency node by name within the graph's root dependencies.
    /// </summary>
    /// <param name="name">The name of the node to find.</param>
    /// <returns>The first matching <see cref="DependencyNode"/> if found; otherwise, throws an exception.</returns>
    /// <remarks>
    /// This method performs a case-insensitive comparison to find the node.
    /// </remarks>
    private DependencyNode FindNodeByName(string name)
    {
        // Log the attempt to find a node by name
        _logger.LogDebug("Attempting to find a dependency node with name: {NodeName}.", name);

        try
        {
            // Attempt to find the first node in the root's dependencies that matches the specified name, ignoring case.
            var node = _graph.Root.Dependencies.First(node => node.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            _logger.LogDebug("Dependency node with name {NodeName} found successfully.", name);
            return node;
        }
        catch (InvalidOperationException)
        {
            // Log an error if no matching node is found
            _logger.LogError("Dependency node with name {NodeName} not found.", name);
            throw new Exception($"Dependency node with name '{name}' not found.");
        }
    }

    /// <summary>
    /// Recursively collects all dependencies of a given node into a provided hash set.
    /// </summary>
    /// <param name="node">The root node to collect dependencies from.</param>
    /// <param name="toDelete">A hash set that accumulates all the unique dependency nodes.</param>
    /// <remarks>
    /// This method avoids circular dependencies by checking if a node has already been processed.
    /// </remarks>
    private void CollectDependencies(DependencyNode node, HashSet<DependencyNode> toDelete)
    {
        // Log the start of dependency collection for the given node
        _logger.LogDebug("Collecting dependencies for node: {NodeName}.", node.Name);

        // Check if the node has already been added to prevent processing it again, which also prevents cycles.
        if (!toDelete.Add(node))
        {
            // Log a debug message if the node has already been processed
            _logger.LogDebug("Node {NodeName} already processed, skipping to prevent circular dependency.", node.Name);
            return; // Node already processed, exit to avoid circular dependencies.
        }

        // Recurse into each dependency of the node, collecting all unique nodes.
        foreach (var dependency in node.Dependencies)
        {
            // Recursively collect dependencies for each child node
            CollectDependencies(dependency, toDelete);
        }

        // Log the completion of dependency collection for the given node
        _logger.LogDebug("Dependency collection for node {NodeName} completed.", node.Name);
    }

    /// <summary>
    /// Asynchronously deletes records from the specified entity in the database.
    /// </summary>
    /// <param name="entityName">The name of the entity from which records will be deleted.</param>
    /// <param name="filterQuery">The filter query to apply when selecting records for deletion.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation (optional).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteEntityRecordsAsync(string entityName, string filterQuery = "", CancellationToken cancellationToken = default)
    {
        // Log the deletion process with entity name and filter query
        _logger.LogDebug("Deleting records from {EntityName} with filter {FilterQuery}.", entityName, filterQuery);

        // Get the DbSet for the specified entity name from the DbContext
        var dbSet = GetDbSet(entityName);

        // Check if DbSet is null, log a warning, and return if DbSet is not found
        if (dbSet == null!)
        {
            _logger.LogWarning("{EntityName} DbSet not found in context.", entityName);
            return;
        }

        // Initialize query with the DbSet
        var query = dbSet;

        // Apply filterQuery if provided
        if (!string.IsNullOrWhiteSpace(filterQuery))
        {
            query = query.Where(filterQuery);
        }

        // Retrieve items matching the query and convert them to a list asynchronously
        var items = await query.Cast<object>().ToListAsync(cancellationToken);

        // Check if any items were retrieved
        if (items.Count != 0)
        {
            // Remove the retrieved items from the DbContext
            _graph.Context.RemoveRange(items);

            // Save changes to the database if soft delete is not enabled
            if (!_configuration.SoftDelete)
            {
                _graph.Context.SaveChanges();
            }
            else
            {
                // Otherwise, save changes asynchronously
                await _graph.Context.SaveChangesAsync(cancellationToken);
            }

            // Log success message
            _logger.LogInformation("Records deleted from {EntityName}.", entityName);
        }
        else
        {
            // Log message if no records were found to delete
            _logger.LogInformation("No records found to delete in {EntityName} with the specified filter.", entityName);
        }
    }

    /// <summary>
    /// Retrieves the <see cref="DbSet{TEntity}"/> for the specified entity name from the DbContext.
    /// </summary>
    /// <param name="entityName">The name of the entity whose DbSet will be retrieved.</param>
    /// <returns>The DbSet for the specified entity name, or null if not found.</returns>
    private IQueryable GetDbSet(string entityName)
    {
        // Get the property representing the DbSet for the specified entity name
        var dbSetProperty = _graph.Context.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>) &&
                        p.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase));

        // Log verbose information about DbSet retrieval attempt
        _logger.LogDebug("Retrieving DbSet for entity: {EntityName}.", entityName);

        // Check if DbSet property is found
        if (dbSetProperty != null)
        {
            // Retrieve and return the DbSet if found
            var dbSet = (IQueryable)dbSetProperty.GetValue(_graph.Context)!;
            _logger.LogDebug("DbSet for entity {EntityName} retrieved successfully.", entityName);
            return dbSet;
        }
        else
        {
            // Log a warning if DbSet property is not found
            _logger.LogWarning("DbSet for entity {EntityName} not found in context.", entityName);
            return null!;
        }
    }
}