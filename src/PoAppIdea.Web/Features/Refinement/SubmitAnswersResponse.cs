using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Response DTO after submitting refinement answers.
/// </summary>
public sealed record SubmitAnswersResponse
{
    /// <summary>
    /// Session ID.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Current session phase after submission.
    /// </summary>
    public required SessionPhase CurrentPhase { get; init; }

    /// <summary>
    /// Number of answers recorded.
    /// </summary>
    public required int AnswersRecorded { get; init; }

    /// <summary>
    /// Next phase the session will transition to.
    /// </summary>
    public required SessionPhase? NextPhase { get; init; }

    /// <summary>
    /// Whether all refinement phases are complete.
    /// </summary>
    public required bool RefinementComplete { get; init; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public required string Message { get; init; }
}
