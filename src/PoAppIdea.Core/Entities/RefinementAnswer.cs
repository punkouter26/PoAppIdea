using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Entities;

/// <summary>
/// User response to a clarifying question in refinement phases.
/// </summary>
public sealed class RefinementAnswer
{
    /// <summary>
    /// Unique answer identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Session context.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Phase4_PM or Phase5_Architect.
    /// </summary>
    public required RefinementPhase Phase { get; init; }

    /// <summary>
    /// Question index (1-10).
    /// </summary>
    public required int QuestionNumber { get; init; }

    /// <summary>
    /// The question asked.
    /// </summary>
    public required string QuestionText { get; init; }

    /// <summary>
    /// Question category (e.g., "UserPersonas", "Deployment").
    /// </summary>
    public required string QuestionCategory { get; init; }

    /// <summary>
    /// User's answer (1-2000 chars).
    /// </summary>
    public required string AnswerText { get; set; }

    /// <summary>
    /// When answered.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }
}
