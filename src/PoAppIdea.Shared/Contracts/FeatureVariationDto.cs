using PoAppIdea.Core.Enums;

namespace PoAppIdea.Shared.Contracts;

/// <summary>
/// DTO for a single feature.
/// </summary>
public sealed record FeatureDto
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required FeaturePriority Priority { get; init; }
}

/// <summary>
/// DTO for feature variation data transfer.
/// </summary>
public sealed record FeatureVariationDto
{
    public required Guid Id { get; init; }
    public required Guid SessionId { get; init; }
    public required Guid MutationId { get; init; }
    public required IReadOnlyList<FeatureDto> Features { get; init; }
    public required IReadOnlyList<string> ServiceIntegrations { get; init; }
    public required string VariationTheme { get; init; }
    public required float Score { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}
