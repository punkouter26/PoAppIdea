using FluentValidation;
using PoAppIdea.Web.Features.Refinement;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for SubmitAnswersRequest.
/// Ensures answers are provided within allowed bounds.
/// </summary>
public sealed class SubmitAnswersRequestValidator : AbstractValidator<SubmitAnswersRequest>
{
    public SubmitAnswersRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty()
            .WithMessage("At least one answer is required.");

        RuleFor(x => x.Answers)
            .Must(a => a.Count <= 10)
            .WithMessage("Maximum 10 answers allowed.");

        RuleForEach(x => x.Answers)
            .SetValidator(new AnswerInputValidator());
    }
}

/// <summary>
/// Validator for individual AnswerInput.
/// </summary>
public sealed class AnswerInputValidator : AbstractValidator<AnswerInput>
{
    public AnswerInputValidator()
    {
        RuleFor(x => x.QuestionNumber)
            .InclusiveBetween(1, 10)
            .WithMessage("Question number must be between 1 and 10.");

        RuleFor(x => x.AnswerText)
            .NotEmpty()
            .WithMessage("Answer text is required.")
            .MaximumLength(2000)
            .WithMessage("Answer text must not exceed 2000 characters.");
    }
}
