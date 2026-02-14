namespace PoAppIdea.Web.Infrastructure.Configuration;

/// <summary>
/// Strongly-typed configuration for SparkService idea generation settings.
/// Bound from appsettings.json "IdeaGeneration" section.
/// </summary>
public sealed class SparkServiceOptions
{
    /// <summary>
    /// Number of ideas to generate per batch (default: 5, reduced from 10 to save tokens).
    /// </summary>
    public int IdeasPerBatch { get; set; } = 5;

    /// <summary>
    /// Maximum number of batches allowed per session (default: 2).
    /// </summary>
    public int MaxBatches { get; set; } = 2;

    /// <summary>
    /// Minimum allowed batch size (validation constraint).
    /// </summary>
    public const int MinBatchSize = 1;

    /// <summary>
    /// Maximum allowed batch size (validation constraint).
    /// </summary>
    public const int MaxBatchSize = 50;

    /// <summary>
    /// Fast swipe threshold in milliseconds (default: 1000ms).
    /// Swipes faster than this indicate low confidence.
    /// </summary>
    public int FastSwipeThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Hesitation threshold in milliseconds (default: 3000ms).
    /// Swipes slower than this indicate deliberation/high confidence.
    /// </summary>
    public int HesitationThresholdMs { get; set; } = 3000;

    /// <summary>
    /// Weight applied to fast swipes in scoring algorithm.
    /// </summary>
    public float FastSwipeWeight { get; set; } = 0.5f;

    /// <summary>
    /// Weight applied to normal-speed swipes in scoring algorithm.
    /// </summary>
    public float NormalSwipeWeight { get; set; } = 1.0f;

    /// <summary>
    /// Weight applied to hesitation (slow) swipes in scoring algorithm.
    /// </summary>
    public float HesitationWeight { get; set; } = 1.5f;

    /// <summary>
    /// Number of top ideas to select for mutation phase (default: 3).
    /// </summary>
    public int TopIdeasCount { get; set; } = 3;
}
