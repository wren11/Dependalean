namespace Dependalean.Configuration;

/// <summary>
/// Configuration settings for cleanup operations.
/// </summary>
public class CleanupConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether soft delete is enabled.
    /// </summary>
    public bool SoftDelete { get; set; } = false;

    public CleanupConfiguration WithSoftDelete(bool enableSoftDelete)
    {
        SoftDelete = enableSoftDelete;
        return this;
    }
}
