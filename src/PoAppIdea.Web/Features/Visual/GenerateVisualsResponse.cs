namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Status of visual generation operation.
/// </summary>
public enum VisualGenerationStatus
{
    Queued,
    InProgress,
    Completed,
    Failed
}

/// <summary>
/// Response DTO for visual generation operation.
/// Follows async pattern since DALL-E generation can take time.
/// </summary>
public sealed record GenerateVisualsResponse
{
    /// <summary>
    /// Session ID for tracking.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Current status of the generation operation.
    /// </summary>
    public required VisualGenerationStatus Status { get; init; }

    /// <summary>
    /// Number of visuals generated so far.
    /// </summary>
    public required int GeneratedCount { get; init; }

    /// <summary>
    /// Number of visuals queued for retry (Edge Case T173).
    /// These are requests that failed due to service unavailability.
    /// </summary>
    public int QueuedCount { get; init; }

    /// <summary>
    /// Estimated seconds until completion.
    /// </summary>
    public int? EstimatedCompletionSeconds { get; init; }

    /// <summary>
    /// Error message if generation failed.
    /// </summary>
    public string? Error { get; init; }
}
