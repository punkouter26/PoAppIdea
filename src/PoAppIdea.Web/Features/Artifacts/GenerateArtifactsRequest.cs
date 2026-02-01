namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// Request DTO for generating final artifacts.
/// POST /api/sessions/{sessionId}/artifacts
/// </summary>
public sealed record GenerateArtifactsRequest
{
    /// <summary>
    /// Optional override for artifact title prefix.
    /// If not provided, uses the synthesized/selected idea title.
    /// </summary>
    public string? TitleOverride { get; init; }

    /// <summary>
    /// Whether to include detailed technical specifications.
    /// Default is true.
    /// </summary>
    public bool IncludeTechnicalDetails { get; init; } = true;

    /// <summary>
    /// Whether to generate the visual asset pack.
    /// Default is true.
    /// </summary>
    public bool IncludeVisualPack { get; init; } = true;
}
