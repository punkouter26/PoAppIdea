using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository interface for RefinementAnswer persistence.
/// Follows Repository pattern for data access abstraction.
/// </summary>
public interface IRefinementAnswerRepository
{
    /// <summary>
    /// Creates a new refinement answer.
    /// </summary>
    Task<RefinementAnswer> CreateAsync(RefinementAnswer answer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple answers in batch.
    /// </summary>
    Task CreateBatchAsync(IEnumerable<RefinementAnswer> answers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an answer by ID.
    /// </summary>
    Task<RefinementAnswer?> GetByIdAsync(Guid sessionId, Guid answerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all answers for a session.
    /// </summary>
    Task<IReadOnlyList<RefinementAnswer>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all answers for a session and specific phase.
    /// </summary>
    Task<IReadOnlyList<RefinementAnswer>> GetBySessionAndPhaseAsync(
        Guid sessionId,
        RefinementPhase phase,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing answer.
    /// </summary>
    Task<RefinementAnswer> UpdateAsync(RefinementAnswer answer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all answers for a session.
    /// </summary>
    Task DeleteBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts answers for a session and phase.
    /// </summary>
    Task<int> CountBySessionAndPhaseAsync(
        Guid sessionId,
        RefinementPhase phase,
        CancellationToken cancellationToken = default);
}
