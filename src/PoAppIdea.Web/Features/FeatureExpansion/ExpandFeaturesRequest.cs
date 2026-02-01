namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Request to generate feature variations from top mutations.
/// </summary>
public sealed record ExpandFeaturesRequest
{
    /// <summary>
    /// Optional list of specific mutation IDs to expand.
    /// If not provided, top 10 mutations by score will be used.
    /// </summary>
    public IReadOnlyList<Guid>? TopMutationIds { get; init; }

    /// <summary>
    /// Number of feature variations to generate per mutation.
    /// Defaults to 5 per FR-011.
    /// </summary>
    public int VariationsPerMutation { get; init; } = 5;
}
