using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository for mutation persistence operations.
/// </summary>
public interface IMutationRepository
{
    /// <summary>
    /// Creates a new mutation.
    /// </summary>
    Task<Mutation> CreateAsync(Mutation mutation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple mutations in a batch.
    /// </summary>
    Task<IReadOnlyList<Mutation>> CreateBatchAsync(IEnumerable<Mutation> mutations, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a mutation by ID.
    /// </summary>
    Task<Mutation?> GetByIdAsync(Guid mutationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all mutations for a session.
    /// </summary>
    Task<IReadOnlyList<Mutation>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top N mutations by score for a session.
    /// </summary>
    Task<IReadOnlyList<Mutation>> GetTopByScoreAsync(Guid sessionId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mutations by parent idea ID.
    /// </summary>
    Task<IReadOnlyList<Mutation>> GetByParentIdeaIdAsync(Guid parentIdeaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing mutation.
    /// </summary>
    Task<Mutation> UpdateAsync(Mutation mutation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a mutation.
    /// </summary>
    Task DeleteAsync(Guid mutationId, CancellationToken cancellationToken = default);
}
