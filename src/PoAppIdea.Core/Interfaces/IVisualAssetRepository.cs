using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository interface for VisualAsset operations.
/// Follows Repository Pattern for data access abstraction.
/// </summary>
public interface IVisualAssetRepository
{
    /// <summary>
    /// Creates a new visual asset.
    /// </summary>
    Task<VisualAsset> CreateAsync(VisualAsset asset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple visual assets in batch.
    /// </summary>
    Task CreateBatchAsync(IEnumerable<VisualAsset> assets, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a visual asset by ID.
    /// </summary>
    Task<VisualAsset?> GetByIdAsync(Guid sessionId, Guid assetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all visual assets for a session.
    /// </summary>
    Task<IReadOnlyList<VisualAsset>> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the selected visual asset for a session.
    /// </summary>
    Task<VisualAsset?> GetSelectedAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a visual asset.
    /// </summary>
    Task<VisualAsset> UpdateAsync(VisualAsset asset, CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects a visual asset and deselects all others for the session.
    /// </summary>
    Task SelectAsync(Guid sessionId, Guid assetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts visual assets for a session.
    /// </summary>
    Task<int> CountBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a visual asset.
    /// </summary>
    Task DeleteAsync(Guid sessionId, Guid assetId, CancellationToken cancellationToken = default);
}
