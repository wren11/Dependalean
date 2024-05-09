# Dependalean: Entity Cleanup Management System

## Overview
Dependalean is a robust .NET library designed to manage the cleanup of entity records in a database, respecting entity dependencies. It provides a structured way to purge records from a database while considering foreign key constraints and custom cleanup configurations.

## Features
- **Entity Dependency Graph**: Automatically builds a graph of entity dependencies from a DbContext.
- **Cleanup Configuration**: Allows configuration of cleanup behaviors such as soft deletes.
- **Asynchronous Operations**: Supports asynchronous cleanup operations to enhance performance and responsiveness.
- **Logging and Error Handling**: Integrated logging for tracking operations and handling errors gracefully.

## Getting Started

### Prerequisites
- .NET 8.0 or higher
- Entity Framework Core 8.0.4 or higher

### Installation
Dependalean is not yet available as a NuGet package. To use it, you need to clone the repository and include it in your .NET solution.

### Configuration
1. **Add Dependalean Services**: Register Dependalean services in the startup configuration of your application.


``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDependaclean<MyDbContext>(config => {
        config.WithSoftDelete(true); // Enable soft delete
    });
}
```

2. **Configure Entity Dependency Graph**: Automatically handled when you register Dependalean services.

### Basic Usage

#### Purging Entity Records
To purge entity records, you can use the `EntityCleanupManager` which is injected through dependency injection where needed.


``` csharp
public class MyService
{
    private readonly IEntityCleanupManager cleanupManager;

    public MyService(IEntityCleanupManager cleanupManager)
    {
        cleanupManager = cleanupManager;
    }

    public async Task PurgeRecordsAsync()
    {
        await cleanupManager.PurgeEntityRecordsAsync("MyTable", "IsActive = 0");
    }
}
```

## Examples

### Configuring Soft Delete
You can configure the cleanup process to use soft deletes, which means records are flagged as deleted rather than being removed from the database.

``` csharp
services.AddDependaclean<MyDbContext>(config => {
    config.WithSoftDelete(true);
});
```


### Custom Cleanup Logic
You can extend the `EntityCleanupManager` to implement custom cleanup logic, such as cascading deletes or specialized filtering.

``` csharp
public class CustomCleanupManager : EntityCleanupManager
{
    public CustomCleanupManager(
        ILogger<CustomCleanupManager> logger,
        IEntityDependencyGraph graph,
        CleanupConfiguration configure) : base(logger, graph, configure)
    {

    }

    protected override async Task DeleteEntityRecordsAsync(
        string entityName, string filterQuery, CancellationToken cancellationToken)
    {
        // Custom deletion logic here
    }
}
```

## Documentation
For more detailed documentation, refer to the inline comments and summaries in the codebase. Each class and method is documented to explain its purpose and usage.

## Contributing
Contributions are welcome! Please feel free to submit pull requests or create issues for bugs and feature requests.

## License
Dependalean is licensed under the MIT License. See the LICENSE file in the repository for more information.