using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository for session persistence operations.
/// </summary>
public interface ISessionRepository
{
    /// <summary>
    /// Creates a new session.
    /// </summary>
    Task<Session> CreateAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a session by ID.
    /// </summary>
    Task<Session?> GetByIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sessions for a user.
    /// </summary>
    Task<IReadOnlyList<Session>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing session.
    /// </summary>
    Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a session with resumption state for abandoned sessions (FR-024).
    /// </summary>
    Task<Session?> GetForResumptionAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a session.
    /// </summary>
    Task DeleteAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
