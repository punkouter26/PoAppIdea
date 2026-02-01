namespace PoAppIdea.Web.Features.Personality;

/// <summary>
/// Request DTO for getting a user's personality profile.
/// GET /api/users/{userId}/personality
/// </summary>
public sealed record GetPersonalityRequest
{
    /// <summary>
    /// Whether to include detailed preference breakdown.
    /// </summary>
    public bool IncludeDetails { get; init; } = true;

    /// <summary>
    /// Whether to include session history summary.
    /// </summary>
    public bool IncludeSessionHistory { get; init; } = false;
}
