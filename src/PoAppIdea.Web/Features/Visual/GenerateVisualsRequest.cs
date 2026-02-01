namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Request DTO for generating visual mockups.
/// The session context provides all needed information from previous phases.
/// </summary>
public sealed record GenerateVisualsRequest
{
    /// <summary>
    /// Optional style hints for visual generation.
    /// If not provided, system will infer from refinement answers.
    /// </summary>
    public string? StyleHint { get; init; }

    /// <summary>
    /// Number of visuals to generate (default 10).
    /// </summary>
    public int Count { get; init; } = 10;
}
