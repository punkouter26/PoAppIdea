using FluentValidation;
using PoAppIdea.Web.Features.Artifacts;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for GenerateArtifactsRequest.
/// Ensures optional parameters are valid when provided.
/// </summary>
public sealed class GenerateArtifactsRequestValidator : AbstractValidator<GenerateArtifactsRequest>
{
    public GenerateArtifactsRequestValidator()
    {
        RuleFor(x => x.TitleOverride)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.TitleOverride))
            .WithMessage("Title override must not exceed 200 characters.");
    }
}
