using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of IIdeaRepository.
/// </summary>
public sealed class IdeaRepository : IIdeaRepository
{
    private readonly TableStorageClient _tableClient;

    public IdeaRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<Idea> CreateAsync(Idea idea, CancellationToken cancellationToken = default)
    {
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.Ideas,
            idea.SessionId.ToString(),
            idea.Id.ToString(),
            idea,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Idea>> CreateBatchAsync(IEnumerable<Idea> ideas, CancellationToken cancellationToken = default)
    {
        var results = new List<Idea>();
        foreach (var idea in ideas)
        {
            var created = await CreateAsync(idea, cancellationToken);
            results.Add(created);
        }
        return results;
    }

    public async Task<Idea?> GetByIdAsync(Guid ideaId, CancellationToken cancellationToken = default)
    {
        var filter = $"RowKey eq '{ideaId}'";
        var results = await _tableClient.QueryAsync<Idea>(AppConstants.TableNames.Ideas, filter, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Idea>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.QueryByPartitionKeyAsync<Idea>(
            AppConstants.TableNames.Ideas,
            sessionId.ToString(),
            cancellationToken);
    }

    public async Task<IReadOnlyList<Idea>> GetByBatchAsync(Guid sessionId, int batchNumber, CancellationToken cancellationToken = default)
    {
        var allIdeas = await GetBySessionIdAsync(sessionId, cancellationToken);
        return allIdeas.Where(i => i.BatchNumber == batchNumber).ToList();
    }

    public async Task<IReadOnlyList<Idea>> GetTopByScoreAsync(Guid sessionId, int count, CancellationToken cancellationToken = default)
    {
        var allIdeas = await GetBySessionIdAsync(sessionId, cancellationToken);
        return allIdeas.OrderByDescending(i => i.Score).Take(count).ToList();
    }

    public async Task<Idea> UpdateAsync(Idea idea, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.Ideas,
            idea.SessionId.ToString(),
            idea.Id.ToString(),
            idea,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid ideaId, CancellationToken cancellationToken = default)
    {
        var idea = await GetByIdAsync(ideaId, cancellationToken);
        if (idea is not null)
        {
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.Ideas,
                idea.SessionId.ToString(),
                ideaId.ToString(),
                cancellationToken);
        }
    }
}
