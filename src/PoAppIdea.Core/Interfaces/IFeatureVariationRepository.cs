using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository for feature variation persistence operations.
/// </summary>
public interface IFeatureVariationRepository
{
    /// <summary>
    /// Creates a new feature variation.
    /// </summary>
    Task<FeatureVariation> CreateAsync(FeatureVariation variation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple feature variations in a batch.
    /// </summary>
    Task<IReadOnlyList<FeatureVariation>> CreateBatchAsync(IEnumerable<FeatureVariation> variations, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a feature variation by ID.
    /// </summary>
    Task<FeatureVariation?> GetByIdAsync(Guid variationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all feature variations for a session.
    /// </summary>
    Task<IReadOnlyList<FeatureVariation>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets feature variations for a specific mutation.
    /// </summary>
    Task<IReadOnlyList<FeatureVariation>> GetByMutationIdAsync(Guid mutationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top N feature variations by score for a session.
    /// </summary>
    Task<IReadOnlyList<FeatureVariation>> GetTopByScoreAsync(Guid sessionId, int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing feature variation.
    /// </summary>
    Task<FeatureVariation> UpdateAsync(FeatureVariation variation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a feature variation.
    /// </summary>
    Task DeleteAsync(Guid variationId, CancellationToken cancellationToken = default);
}
