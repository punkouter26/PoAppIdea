namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Request to rate a feature variation.
/// </summary>
public sealed record RateFeatureVariationRequest
{
    /// <summary>
    /// Rating score from 1-5.
    /// </summary>
    public required float Score { get; init; }
}
