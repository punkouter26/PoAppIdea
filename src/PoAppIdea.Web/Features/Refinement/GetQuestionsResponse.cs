using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Response DTO containing refinement questions.
/// </summary>
public sealed record GetQuestionsResponse
{
    /// <summary>
    /// The refinement phase (PM or Architect).
    /// </summary>
    public required RefinementPhase Phase { get; init; }

    /// <summary>
    /// Session phase string representation.
    /// </summary>
    public required string PhaseDisplayName { get; init; }

    /// <summary>
    /// List of 10 questions for the current phase.
    /// </summary>
    public required IReadOnlyList<RefinementQuestionDto> Questions { get; init; }

    /// <summary>
    /// Count of questions already answered in this phase.
    /// </summary>
    public required int AnsweredCount { get; init; }

    /// <summary>
    /// Total questions in this phase.
    /// </summary>
    public required int TotalQuestions { get; init; }
}

/// <summary>
/// Individual refinement question.
/// </summary>
public sealed record RefinementQuestionDto
{
    /// <summary>
    /// Question index (1-10).
    /// </summary>
    public required int QuestionNumber { get; init; }

    /// <summary>
    /// The question text.
    /// </summary>
    public required string QuestionText { get; init; }

    /// <summary>
    /// Question category (e.g., "UserPersonas", "Deployment").
    /// </summary>
    public required string Category { get; init; }

    /// <summary>
    /// Example answer to guide the user.
    /// </summary>
    public required string ExampleAnswer { get; init; }

    /// <summary>
    /// Whether this question has been answered.
    /// </summary>
    public bool IsAnswered { get; init; }

    /// <summary>
    /// The user's answer if already provided.
    /// </summary>
    public string? ExistingAnswer { get; init; }
}
