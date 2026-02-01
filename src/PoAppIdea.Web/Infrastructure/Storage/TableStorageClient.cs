using System.Text.Json;
using Azure;
using Azure.Data.Tables;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Client for Azure Table Storage operations.
/// </summary>
public sealed class TableStorageClient
{
    private readonly TableServiceClient _serviceClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TableStorageClient(TableServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Gets or creates a table client.
    /// </summary>
    public async Task<TableClient> GetTableClientAsync(string tableName, CancellationToken cancellationToken = default)
    {
        var tableClient = _serviceClient.GetTableClient(tableName);
        await tableClient.CreateIfNotExistsAsync(cancellationToken);
        return tableClient;
    }

    /// <summary>
    /// Inserts an entity into a table.
    /// </summary>
    public async Task<T> InsertAsync<T>(string tableName, string partitionKey, string rowKey, T entity, CancellationToken cancellationToken = default)
        where T : class
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);
        var tableEntity = ToTableEntity(partitionKey, rowKey, entity);
        await tableClient.AddEntityAsync(tableEntity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Upserts an entity into a table.
    /// </summary>
    public async Task<T> UpsertAsync<T>(string tableName, string partitionKey, string rowKey, T entity, CancellationToken cancellationToken = default)
        where T : class
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);
        var tableEntity = ToTableEntity(partitionKey, rowKey, entity);
        await tableClient.UpsertEntityAsync(tableEntity, TableUpdateMode.Replace, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Gets an entity from a table.
    /// </summary>
    public async Task<T?> GetAsync<T>(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken = default)
        where T : class
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);
        try
        {
            var response = await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey, cancellationToken: cancellationToken);
            return FromTableEntity<T>(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    /// <summary>
    /// Queries entities from a table.
    /// </summary>
    public async Task<IReadOnlyList<T>> QueryAsync<T>(string tableName, string filter, CancellationToken cancellationToken = default)
        where T : class
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);
        var results = new List<T>();

        await foreach (var entity in tableClient.QueryAsync<TableEntity>(filter, cancellationToken: cancellationToken))
        {
            var item = FromTableEntity<T>(entity);
            if (item is not null)
            {
                results.Add(item);
            }
        }

        return results;
    }

    /// <summary>
    /// Queries entities by partition key.
    /// </summary>
    public async Task<IReadOnlyList<T>> QueryByPartitionKeyAsync<T>(string tableName, string partitionKey, CancellationToken cancellationToken = default)
        where T : class
    {
        var filter = $"PartitionKey eq '{partitionKey}'";
        return await QueryAsync<T>(tableName, filter, cancellationToken);
    }

    /// <summary>
    /// Deletes an entity from a table.
    /// </summary>
    public async Task DeleteAsync(string tableName, string partitionKey, string rowKey, CancellationToken cancellationToken = default)
    {
        var tableClient = await GetTableClientAsync(tableName, cancellationToken);
        try
        {
            await tableClient.DeleteEntityAsync(partitionKey, rowKey, ETag.All, cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            // Entity doesn't exist, ignore
        }
    }

    private TableEntity ToTableEntity<T>(string partitionKey, string rowKey, T entity) where T : class
    {
        var json = JsonSerializer.Serialize(entity, _jsonOptions);
        return new TableEntity(partitionKey, rowKey)
        {
            ["Data"] = json
        };
    }

    private T? FromTableEntity<T>(TableEntity tableEntity) where T : class
    {
        if (!tableEntity.TryGetValue("Data", out var data) || data is not string json)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(json, _jsonOptions);
    }
}
