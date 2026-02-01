using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Response containing generated feature variations.
/// </summary>
public sealed record ExpandFeaturesResponse
{
    /// <summary>
    /// Session identifier.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Total number of feature variations generated.
    /// </summary>
    public required int TotalVariations { get; init; }

    /// <summary>
    /// Generated feature variations grouped by parent mutation.
    /// </summary>
    public required IReadOnlyList<FeatureVariationDto> Variations { get; init; }

    /// <summary>
    /// Number of mutations that received variations.
    /// </summary>
    public required int MutationsProcessed { get; init; }
}
