namespace PoAppIdea.Web.Features.Personality;

/// <summary>
/// Response DTO for a user's personality profile.
/// </summary>
public sealed record GetPersonalityResponse
{
    /// <summary>
    /// User identifier.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Product-related preference biases (e.g., "social": 0.8, "productivity": -0.3).
    /// Values range from -1.0 (strongly dislike) to 1.0 (strongly prefer).
    /// </summary>
    public required Dictionary<string, float> ProductBiases { get; init; }

    /// <summary>
    /// Technical preference biases (e.g., "serverless": 0.6, "monolith": -0.5).
    /// Values range from -1.0 (strongly dislike) to 1.0 (strongly prefer).
    /// </summary>
    public required Dictionary<string, float> TechnicalBiases { get; init; }

    /// <summary>
    /// Patterns the user consistently dislikes.
    /// </summary>
    public required IReadOnlyList<string> DislikedPatterns { get; init; }

    /// <summary>
    /// User's swipe speed profile.
    /// </summary>
    public required SwipeSpeedProfileDto SwipeSpeedProfile { get; init; }

    /// <summary>
    /// Number of completed sessions.
    /// </summary>
    public required int TotalSessions { get; init; }

    /// <summary>
    /// When the profile was last updated.
    /// </summary>
    public required DateTimeOffset LastUpdatedAt { get; init; }

    /// <summary>
    /// Top preferred themes/categories derived from biases.
    /// </summary>
    public IReadOnlyList<PreferenceThemeDto>? TopPreferences { get; init; }

    /// <summary>
    /// Recent session summaries (if requested).
    /// </summary>
    public IReadOnlyList<SessionSummaryDto>? RecentSessions { get; init; }
}

/// <summary>
/// Swipe speed profile data.
/// </summary>
public sealed record SwipeSpeedProfileDto
{
    public required double AverageFastMs { get; init; }
    public required double AverageMediumMs { get; init; }
    public required double AverageSlowMs { get; init; }
}

/// <summary>
/// A preference theme with strength.
/// </summary>
public sealed record PreferenceThemeDto
{
    public required string Theme { get; init; }
    public required float Strength { get; init; }
    public required string Category { get; init; }
}

/// <summary>
/// Summary of a session for history display.
/// </summary>
public sealed record SessionSummaryDto
{
    public required Guid SessionId { get; init; }
    public required string AppType { get; init; }
    public required int ComplexityLevel { get; init; }
    public required string CurrentPhase { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
