using FluentValidation;
using PoAppIdea.Core.Enums;
using PoAppIdea.Web.Features.Session;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for StartSessionRequest.
/// Ensures app type is valid and complexity is within bounds.
/// </summary>
public sealed class StartSessionRequestValidator : AbstractValidator<StartSessionRequest>
{
    public StartSessionRequestValidator()
    {
        RuleFor(x => x.AppType)
            .IsInEnum()
            .WithMessage("Invalid app type. Must be one of: Mobile, Web, Desktop, CLI, API.");

        RuleFor(x => x.ComplexityLevel)
            .InclusiveBetween(1, 5)
            .WithMessage("Complexity level must be between 1 and 5.");
    }
}
