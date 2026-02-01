namespace PoAppIdea.Web.Features.Synthesis;

/// <summary>
/// Request DTO for submitting selected ideas for synthesis.
/// POST /api/sessions/{sessionId}/submit
/// </summary>
public sealed record SubmitSelectionsRequest
{
    /// <summary>
    /// List of selected idea IDs (feature variations or mutations).
    /// Min: 1, Max: 10 per spec.
    /// </summary>
    public required List<Guid> SelectedIdeaIds { get; init; }
}
