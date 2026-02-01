namespace PoAppIdea.Core.Entities;

/// <summary>
/// Speed profile for swipe behavior analysis.
/// </summary>
public sealed class SpeedProfile
{
    public required double AverageFastMs { get; set; }
    public required double AverageMediumMs { get; set; }
    public required double AverageSlowMs { get; set; }
}

/// <summary>
/// Accumulated preferences learned from user's swipe behavior across sessions.
/// </summary>
public sealed class ProductPersonality
{
    /// <summary>
    /// Owner of this personality.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Bias scores for product traits (e.g., "social": 0.8). Range: -1.0 to 1.0.
    /// </summary>
    public required Dictionary<string, float> ProductBiases { get; set; }

    /// <summary>
    /// Bias scores for tech traits (e.g., "serverless": 0.6). Range: -1.0 to 1.0.
    /// </summary>
    public required Dictionary<string, float> TechnicalBiases { get; set; }

    /// <summary>
    /// Patterns to rank lower (e.g., "subscription-based"). Max 50 entries.
    /// </summary>
    public required List<string> DislikedPatterns { get; set; }

    /// <summary>
    /// Fast/Medium/Slow average speeds.
    /// </summary>
    public required SpeedProfile SwipeSpeedProfile { get; set; }

    /// <summary>
    /// Number of completed sessions.
    /// </summary>
    public required int TotalSessions { get; set; }

    /// <summary>
    /// Last profile update timestamp.
    /// </summary>
    public required DateTimeOffset LastUpdatedAt { get; set; }
}
