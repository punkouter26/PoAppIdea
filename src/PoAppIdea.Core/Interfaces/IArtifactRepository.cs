using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository for artifact persistence operations.
/// </summary>
public interface IArtifactRepository
{
    /// <summary>
    /// Creates a new artifact.
    /// </summary>
    Task<Artifact> CreateAsync(Artifact artifact, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an artifact by ID.
    /// </summary>
    Task<Artifact?> GetByIdAsync(Guid artifactId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an artifact by human-readable slug.
    /// </summary>
    Task<Artifact?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all artifacts for a session.
    /// </summary>
    Task<IReadOnlyList<Artifact>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all artifacts for a user.
    /// </summary>
    Task<IReadOnlyList<Artifact>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets published artifacts for the public gallery with pagination.
    /// </summary>
    Task<IReadOnlyList<Artifact>> GetPublishedAsync(int skip, int take, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing artifact.
    /// </summary>
    Task<Artifact> UpdateAsync(Artifact artifact, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an artifact.
    /// </summary>
    Task DeleteAsync(Guid artifactId, CancellationToken cancellationToken = default);
}
