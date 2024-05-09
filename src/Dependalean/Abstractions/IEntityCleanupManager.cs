namespace Dependalean.Abstractions;

/// <summary>
/// Interface for managing the cleanup of entity records.
/// </summary>
public interface IEntityCleanupManager
{
    /// <summary>
    /// Asynchronously purges entity records based on the specified table name and filter.
    /// </summary>
    /// <param name="tableName">The name of the table to purge.</param>
    /// <param name="filterQuery">The query used to filter records for deletion.</param>
    /// <param name="cancellationToken">Token for operation cancellation (optional).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PurgeEntityRecordsAsync(string tableName, string filterQuery, CancellationToken cancellationToken = default!);

    /// <summary>
    /// Asynchronously deletes entity records based on the specified entity name and filter.
    /// </summary>
    /// <param name="entityName">The name of the entity from which records will be deleted.</param>
    /// <param name="filterQuery">The query used to filter records for deletion (optional).</param>
    /// <param name="cancellationToken">Token for operation cancellation (optional).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteEntityRecordsAsync(string entityName, string filterQuery = "", CancellationToken cancellationToken = default!);
}