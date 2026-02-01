using System.ComponentModel.DataAnnotations;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Request DTO for submitting refinement answers.
/// </summary>
public sealed record SubmitAnswersRequest
{
    /// <summary>
    /// List of answers to refinement questions.
    /// Must contain exactly 10 answers.
    /// </summary>
    [Required]
    [MinLength(1)]
    [MaxLength(10)]
    public required IReadOnlyList<AnswerInput> Answers { get; init; }
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
