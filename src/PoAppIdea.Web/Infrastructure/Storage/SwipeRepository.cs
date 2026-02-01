using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of ISwipeRepository.
/// </summary>
public sealed class SwipeRepository : ISwipeRepository
{
    private readonly TableStorageClient _tableClient;

    public SwipeRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<Swipe> CreateAsync(Swipe swipe, CancellationToken cancellationToken = default)
    {
        var rowKey = $"{swipe.SessionId}_{swipe.IdeaId}";
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.Swipes,
            swipe.UserId.ToString(),
            rowKey,
            swipe,
            cancellationToken);
    }

    public async Task<Swipe?> GetByIdAsync(Guid swipeId, CancellationToken cancellationToken = default)
    {
        // Query all partitions to find by swipe ID
        var allSwipes = await _tableClient.QueryAsync<Swipe>(
            AppConstants.TableNames.Swipes,
            $"",
            cancellationToken);
        return allSwipes.FirstOrDefault(s => s.Id == swipeId);
    }

    public async Task<IReadOnlyList<Swipe>> GetByUserSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var allSwipes = await _tableClient.QueryByPartitionKeyAsync<Swipe>(
            AppConstants.TableNames.Swipes,
            userId.ToString(),
            cancellationToken);
        return allSwipes.Where(s => s.SessionId == sessionId).ToList();
    }

    public async Task<IReadOnlyList<Swipe>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        // Query all swipes and filter by session ID
        var allSwipes = await _tableClient.QueryAsync<Swipe>(
            AppConstants.TableNames.Swipes,
            $"",
            cancellationToken);
        return allSwipes.Where(s => s.SessionId == sessionId).ToList();
    }

    public async Task<int> GetCountBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var swipes = await GetBySessionIdAsync(sessionId, cancellationToken);
        return swipes.Count;
    }

    public async Task DeleteAsync(Guid swipeId, CancellationToken cancellationToken = default)
    {
        var swipe = await GetByIdAsync(swipeId, cancellationToken);
        if (swipe is not null)
        {
            var rowKey = $"{swipe.SessionId}_{swipe.IdeaId}";
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.Swipes,
                swipe.UserId.ToString(),
                rowKey,
                cancellationToken);
        }
    }
}
