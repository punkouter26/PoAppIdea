using PoAppIdea.Core.Entities;

namespace PoAppIdea.Core.Interfaces;

/// <summary>
/// Repository for product personality persistence operations.
/// </summary>
public interface IPersonalityRepository
{
    /// <summary>
    /// Gets the personality profile for a user.
    /// </summary>
    Task<ProductPersonality?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates or updates a personality profile.
    /// </summary>
    Task<ProductPersonality> UpsertAsync(ProductPersonality personality, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a personality profile.
    /// </summary>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
