using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// Response DTO for artifact generation status.
/// </summary>
public sealed record GenerateArtifactsResponse
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Current generation status.
    /// </summary>
    public required ArtifactGenerationStatus Status { get; init; }

    /// <summary>
    /// List of artifact types being generated.
    /// </summary>
    public required IReadOnlyList<ArtifactType> ArtifactTypes { get; init; }

    /// <summary>
    /// Number of artifacts generated so far.
    /// </summary>
    public required int GeneratedCount { get; init; }

    /// <summary>
    /// Estimated time to completion in seconds.
    /// Null if completed or failed.
    /// </summary>
    public int? EstimatedCompletionSeconds { get; init; }
}

/// <summary>
/// Status of artifact generation process.
/// </summary>
public enum ArtifactGenerationStatus
{
    /// <summary>
    /// Queued for generation.
    /// </summary>
    Queued,

    /// <summary>
    /// Currently generating.
    /// </summary>
    InProgress,

    /// <summary>
    /// All artifacts generated successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Generation failed.
    /// </summary>
    Failed
}
