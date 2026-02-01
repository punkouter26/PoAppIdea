using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Entities;

/// <summary>
/// A single feature with MoSCoW priority.
/// </summary>
public sealed class Feature
{
    /// <summary>
    /// Feature name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Feature description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// MoSCoW priority.
    /// </summary>
    public required FeaturePriority Priority { get; set; }
}

/// <summary>
/// A set of capabilities and service integrations for an evolved idea.
/// </summary>
public sealed class FeatureVariation
{
    /// <summary>
    /// Unique variation identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session context.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Parent mutation.
    /// </summary>
    public required Guid MutationId { get; init; }

    /// <summary>
    /// Feature list (3-10 features).
    /// </summary>
    public required List<Feature> Features { get; set; }

    /// <summary>
    /// Service integrations (e.g., "Geolocation API", "OAuth").
    /// </summary>
    public required List<string> ServiceIntegrations { get; set; }

    /// <summary>
    /// Variation theme (e.g., "Privacy-first", "Social-heavy").
    /// </summary>
    public required string VariationTheme { get; set; }

    /// <summary>
    /// User rating score.
    /// </summary>
    public required float Score { get; set; }

    /// <summary>
    /// Generation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
