using System.ComponentModel.DataAnnotations;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Request DTO for submitting refinement answers.
/// </summary>
public sealed record SubmitAnswersRequest
{
    /// <summary>
    /// List of answers to refinement questions.
    /// Can be empty if user skips all questions.
    /// </summary>
    [MaxLength(10)]
    public required IReadOnlyList<AnswerInput> Answers { get; init; }

    /// <summary>
    /// When true, advances to the next phase even if not all questions are answered.
    /// Allows users to skip questions that aren't relevant to their app.
    /// </summary>
    public bool SkipRemaining { get; init; }
}

/// <summary>
/// Individual answer input.
/// </summary>
public sealed record AnswerInput
{
    /// <summary>
    /// Question index (1-10).
    /// </summary>
    [Range(1, 10)]
    public required int QuestionNumber { get; init; }

    /// <summary>
    /// User's answer text (1-2000 chars).
    /// </summary>
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string AnswerText { get; init; }
}
