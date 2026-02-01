using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository for swipe persistence operations.
/// </summary>
public interface ISwipeRepository
{
    /// <summary>
    /// Creates a new swipe.
    /// </summary>
    Task<Swipe> CreateAsync(Swipe swipe, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a swipe by ID.
    /// </summary>
    Task<Swipe?> GetByIdAsync(Guid swipeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all swipes for a user in a session.
    /// </summary>
    Task<IReadOnlyList<Swipe>> GetByUserSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all swipes for a session.
    /// </summary>
    Task<IReadOnlyList<Swipe>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets swipe count for a session.
    /// </summary>
    Task<int> GetCountBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a swipe.
    /// </summary>
    Task DeleteAsync(Guid swipeId, CancellationToken cancellationToken = default);
}
