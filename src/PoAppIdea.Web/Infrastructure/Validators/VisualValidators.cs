using FluentValidation;
using PoAppIdea.Web.Features.Visual;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for GenerateVisualsRequest.
/// Ensures visual count is valid.
/// </summary>
public sealed class GenerateVisualsRequestValidator : AbstractValidator<GenerateVisualsRequest>
{
    public GenerateVisualsRequestValidator()
    {
        RuleFor(x => x.Count)
            .InclusiveBetween(1, 20)
            .WithMessage("Count must be between 1 and 20.");

        RuleFor(x => x.StyleHint)
            .MaximumLength(500)
            .When(x => x.StyleHint is not null)
            .WithMessage("StyleHint must not exceed 500 characters.");
    }
}
