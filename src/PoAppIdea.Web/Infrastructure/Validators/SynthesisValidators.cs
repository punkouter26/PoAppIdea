using FluentValidation;
using PoAppIdea.Web.Features.Synthesis;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for SubmitSelectionsRequest.
/// Ensures at least one item is selected (1-10 per spec).
/// </summary>
public sealed class SubmitSelectionsRequestValidator : AbstractValidator<SubmitSelectionsRequest>
{
    public SubmitSelectionsRequestValidator()
    {
        RuleFor(x => x.SelectedIdeaIds)
            .NotEmpty()
            .WithMessage("At least one idea must be selected.");

        RuleFor(x => x.SelectedIdeaIds)
            .Must(ids => ids.Count <= 10)
            .WithMessage("Maximum 10 ideas can be selected.");
    }
}
