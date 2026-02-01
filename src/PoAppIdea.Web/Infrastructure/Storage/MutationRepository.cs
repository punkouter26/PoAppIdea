using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of IMutationRepository.
/// </summary>
public sealed class MutationRepository : IMutationRepository
{
    private readonly TableStorageClient _tableClient;

    public MutationRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<Mutation> CreateAsync(Mutation mutation, CancellationToken cancellationToken = default)
    {
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.Mutations,
            mutation.SessionId.ToString(),
            mutation.Id.ToString(),
            mutation,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Mutation>> CreateBatchAsync(IEnumerable<Mutation> mutations, CancellationToken cancellationToken = default)
    {
        var results = new List<Mutation>();
        foreach (var mutation in mutations)
        {
            var created = await CreateAsync(mutation, cancellationToken);
            results.Add(created);
        }
        return results;
    }

    public async Task<Mutation?> GetByIdAsync(Guid mutationId, CancellationToken cancellationToken = default)
    {
        var filter = $"RowKey eq '{mutationId}'";
        var results = await _tableClient.QueryAsync<Mutation>(AppConstants.TableNames.Mutations, filter, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Mutation>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.QueryByPartitionKeyAsync<Mutation>(
            AppConstants.TableNames.Mutations,
            sessionId.ToString(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Mutation>> GetTopByScoreAsync(Guid sessionId, int count, CancellationToken cancellationToken = default)
    {
        var allMutations = await GetBySessionIdAsync(sessionId, cancellationToken);
        return allMutations.OrderByDescending(m => m.Score).Take(count).ToList();
    }

    public async Task<IReadOnlyList<Mutation>> GetByParentIdeaIdAsync(Guid parentIdeaId, CancellationToken cancellationToken = default)
    {
        // Query all mutations and filter by parent idea ID
        // Note: This is a client-side filter since ParentIdeaIds is a list
        var filter = $"PartitionKey ne ''"; // Get all mutations
        var allMutations = await _tableClient.QueryAsync<Mutation>(AppConstants.TableNames.Mutations, filter, cancellationToken);
        return allMutations.Where(m => m.ParentIdeaIds.Contains(parentIdeaId)).ToList();
    }

    public async Task<Mutation> UpdateAsync(Mutation mutation, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.Mutations,
            mutation.SessionId.ToString(),
            mutation.Id.ToString(),
            mutation,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid mutationId, CancellationToken cancellationToken = default)
    {
        var mutation = await GetByIdAsync(mutationId, cancellationToken);
        if (mutation is not null)
        {
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.Mutations,
                mutation.SessionId.ToString(),
                mutationId.ToString(),
                cancellationToken);
        }
    }
}
