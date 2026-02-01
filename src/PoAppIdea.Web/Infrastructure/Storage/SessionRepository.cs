using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Shared.Constants;

namespace PoAppIdea.Web.Infrastructure.Storage;

/// <summary>
/// Azure Table Storage implementation of ISessionRepository.
/// </summary>
public sealed class SessionRepository : ISessionRepository
{
    private readonly TableStorageClient _tableClient;

    public SessionRepository(TableStorageClient tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task<Session> CreateAsync(Session session, CancellationToken cancellationToken = default)
    {
        return await _tableClient.InsertAsync(
            AppConstants.TableNames.Sessions,
            session.UserId.ToString(),
            session.Id.ToString(),
            session,
            cancellationToken);
    }

    public async Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        // Query all partitions to find by session ID (row key)
        var filter = $"RowKey eq '{sessionId}'";
        var results = await _tableClient.QueryAsync<Session>(AppConstants.TableNames.Sessions, filter, cancellationToken);
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _tableClient.QueryByPartitionKeyAsync<Session>(
            AppConstants.TableNames.Sessions,
            userId.ToString(),
            cancellationToken);
    }

    public async Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default)
    {
        return await _tableClient.UpsertAsync(
            AppConstants.TableNames.Sessions,
            session.UserId.ToString(),
            session.Id.ToString(),
            session,
            cancellationToken);
    }

    /// <summary>
    /// Gets a session with resumption state for abandoned sessions (FR-024).
    /// Validates ownership and returns session if it exists and belongs to user.
    /// </summary>
    public async Task<Session?> GetForResumptionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _tableClient.GetAsync<Session>(
            AppConstants.TableNames.Sessions,
            userId.ToString(),
            sessionId.ToString(),
            cancellationToken);

        // Only return if session exists, belongs to user, and is not completed/abandoned
        if (session is not null && session.UserId == userId && session.Status == SessionStatus.InProgress)
        {
            return session;
        }

        return null;
    }

    public async Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        // Need to find the session first to get the partition key
        var session = await GetByIdAsync(sessionId, cancellationToken);
        if (session is not null)
        {
            await _tableClient.DeleteAsync(
                AppConstants.TableNames.Sessions,
                session.UserId.ToString(),
                sessionId.ToString(),
                cancellationToken);
        }
    }
}
