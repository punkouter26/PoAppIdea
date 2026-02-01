using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Request DTO for getting refinement questions.
/// </summary>
public sealed record GetQuestionsRequest
{
    /// <summary>
    /// Optional phase parameter to get questions for a specific phase.
    /// If not provided, returns questions for the current session phase.
    /// </summary>
    public RefinementPhase? Phase { get; init; }
}
