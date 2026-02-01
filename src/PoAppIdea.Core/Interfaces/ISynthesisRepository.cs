using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository interface for Synthesis CRUD operations.
/// </summary>
public interface ISynthesisRepository
{
    /// <summary>
    /// Creates a new synthesis record.
    /// </summary>
    Task<Synthesis> CreateAsync(Synthesis synthesis, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a synthesis by its ID.
    /// </summary>
    Task<Synthesis?> GetByIdAsync(Guid synthesisId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the synthesis for a session (one synthesis per session max).
    /// </summary>
    Task<Synthesis?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing synthesis.
    /// </summary>
    Task<Synthesis> UpdateAsync(Synthesis synthesis, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a synthesis.
    /// </summary>
    Task DeleteAsync(Guid synthesisId, CancellationToken cancellationToken = default);
}
