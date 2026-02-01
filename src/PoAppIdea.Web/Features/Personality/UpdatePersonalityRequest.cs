namespace PoAppIdea.Web.Features.Personality;

/// <summary>
/// Request DTO for updating a user's personality profile.
/// POST /api/users/{userId}/personality
/// </summary>
public sealed record UpdatePersonalityRequest
{
    /// <summary>
    /// Optional: Override product biases.
    /// </summary>
    public Dictionary<string, float>? ProductBiases { get; init; }

    /// <summary>
    /// Optional: Override technical biases.
    /// </summary>
    public Dictionary<string, float>? TechnicalBiases { get; init; }

    /// <summary>
    /// Optional: Add patterns to dislike list.
    /// </summary>
    public IReadOnlyList<string>? AddDislikedPatterns { get; init; }

    /// <summary>
    /// Optional: Remove patterns from dislike list.
    /// </summary>
    public IReadOnlyList<string>? RemoveDislikedPatterns { get; init; }

    /// <summary>
    /// Whether to reset the profile entirely.
    /// </summary>
    public bool ResetProfile { get; init; } = false;
}
