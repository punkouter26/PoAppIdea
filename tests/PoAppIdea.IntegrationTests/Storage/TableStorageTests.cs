using Azure.Data.Tables;
using PoAppIdea.IntegrationTests.Infrastructure;
using Testcontainers.Azurite;

namespace PoAppIdea.IntegrationTests.Storage;

/// <summary>
/// Integration tests for Azure Table Storage operations.
/// Uses Testcontainers Azurite for ephemeral storage.
/// Verifies: CRUD operations, query patterns.
/// </summary>
public sealed class TableStorageTests : IAsyncLifetime
{
    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:3.35.0")
        .WithCommand("--skipApiVersionCheck")
        .Build();

    private TableServiceClient _tableServiceClient = null!;

    public async Task InitializeAsync()
    {
        await _azuriteContainer.StartAsync();
        _tableServiceClient = new TableServiceClient(_azuriteContainer.GetConnectionString());
        Console.WriteLine("[Test] Azurite Table Storage container started");
    }

    public async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
    }

    [Fact]
    public async Task TableClient_ShouldCreate_Table()
    {
        // Arrange
        var tableName = $"TestTable{Guid.NewGuid():N}"; // Table names max 63 chars

        // Act
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        // Assert
        var exists = await TableExistsAsync(tableName);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task TableClient_ShouldAdd_Entity()
    {
        // Arrange
        var tableName = $"Sessions{Guid.NewGuid():N}";
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("partition1", Guid.NewGuid().ToString())
        {
            { "Prompt", "Build a fitness tracking app" },
            { "Status", "Active" },
            { "CreatedAt", DateTimeOffset.UtcNow }
        };

        // Act
        await tableClient.AddEntityAsync(entity);

        // Assert
        var retrieved = await tableClient.GetEntityAsync<TableEntity>(entity.PartitionKey, entity.RowKey);
        retrieved.Value.Should().NotBeNull();
        retrieved.Value.GetString("Prompt").Should().Be("Build a fitness tracking app");
    }

    [Fact]
    public async Task TableClient_ShouldQuery_Entities()
    {
        // Arrange
        var tableName = $"Ideas{Guid.NewGuid():N}";
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        var partitionKey = "session1";
        for (int i = 0; i < 5; i++)
        {
            var entity = new TableEntity(partitionKey, $"idea-{i}")
            {
                { "Title", $"Idea {i}" },
                { "Score", i * 10 }
            };
            await tableClient.AddEntityAsync(entity);
        }

        // Act
        var query = tableClient.QueryAsync<TableEntity>(e => e.PartitionKey == partitionKey);
        var results = new List<TableEntity>();
        await foreach (var entity in query)
        {
            results.Add(entity);
        }

        // Assert
        results.Should().HaveCount(5);
    }

    [Fact]
    public async Task TableClient_ShouldUpdate_Entity()
    {
        // Arrange
        var tableName = $"Updates{Guid.NewGuid():N}";
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("partition1", "row1")
        {
            { "Score", 50 }
        };
        await tableClient.AddEntityAsync(entity);

        // Act - Fetch fresh entity to get current ETag
        var fetchedEntity = await tableClient.GetEntityAsync<TableEntity>("partition1", "row1");
        fetchedEntity.Value["Score"] = 100;
        await tableClient.UpdateEntityAsync(fetchedEntity.Value, fetchedEntity.Value.ETag, TableUpdateMode.Replace);

        // Assert
        var updated = await tableClient.GetEntityAsync<TableEntity>("partition1", "row1");
        updated.Value.GetInt32("Score").Should().Be(100);
    }

    [Fact]
    public async Task TableClient_ShouldDelete_Entity()
    {
        // Arrange
        var tableName = $"Deletes{Guid.NewGuid():N}";
        var tableClient = _tableServiceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity("partition1", "row1") { { "Data", "ToDelete" } };
        await tableClient.AddEntityAsync(entity);

        // Act
        await tableClient.DeleteEntityAsync("partition1", "row1");

        // Assert
        var queryResult = tableClient.QueryAsync<TableEntity>(e => e.RowKey == "row1");
        var count = 0;
        await foreach (var _ in queryResult) count++;
        count.Should().Be(0);
    }

    private async Task<bool> TableExistsAsync(string tableName)
    {
        await foreach (var table in _tableServiceClient.QueryAsync(t => t.Name == tableName))
        {
            return true;
        }
        return false;
    }
}
