using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository for idea persistence operations.
/// </summary>
public interface IIdeaRepository
{
    /// <summary>
    /// Creates a new idea.
    /// </summary>
    Task<Idea> CreateAsync(Idea idea, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple ideas in a batch.
    /// </summary>
    Task<IReadOnlyList<Idea>> CreateBatchAsync(IEnumerable<Idea> ideas, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an idea by ID.
    /// </summary>
    Task<Idea?> GetByIdAsync(Guid ideaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all ideas for a session.
    /// </summary>
    Task<IReadOnlyList<Idea>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets ideas for a specific batch in a session.
    /// </summary>
    Task<IReadOnlyList<Idea>> GetByBatchAsync(Guid sessionId, int batchNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top N ideas by score for a session.
    /// </summary>
    Task<IReadOnlyList<Idea>> GetTopByScoreAsync(Guid sessionId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing idea.
    /// </summary>
    Task<Idea> UpdateAsync(Idea idea, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an idea.
    /// </summary>
    Task DeleteAsync(Guid ideaId, CancellationToken cancellationToken = default);
}
