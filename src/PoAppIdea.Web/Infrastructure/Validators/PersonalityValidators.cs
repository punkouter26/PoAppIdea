using FluentValidation;
using PoAppIdea.Web.Features.Personality;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for UpdatePersonalityRequest.
/// Ensures bias values are within valid bounds.
/// </summary>
public sealed class UpdatePersonalityRequestValidator : AbstractValidator<UpdatePersonalityRequest>
{
    public UpdatePersonalityRequestValidator()
    {
        RuleFor(x => x.ProductBiases)
            .Must(HaveValidBiasValues)
            .When(x => x.ProductBiases != null && x.ProductBiases.Count > 0)
            .WithMessage("Product bias values must be between -1.0 and 1.0.");

        RuleFor(x => x.TechnicalBiases)
            .Must(HaveValidBiasValues)
            .When(x => x.TechnicalBiases != null && x.TechnicalBiases.Count > 0)
            .WithMessage("Technical bias values must be between -1.0 and 1.0.");

        RuleFor(x => x.AddDislikedPatterns)
            .Must(patterns => patterns is null || patterns.All(p => !string.IsNullOrWhiteSpace(p) && p.Length <= 200))
            .WithMessage("Disliked patterns must not be empty and must not exceed 200 characters.");

        RuleFor(x => x.RemoveDislikedPatterns)
            .Must(patterns => patterns is null || patterns.All(p => !string.IsNullOrWhiteSpace(p) && p.Length <= 200))
            .WithMessage("Patterns to remove must not be empty and must not exceed 200 characters.");
    }

    private static bool HaveValidBiasValues(Dictionary<string, float>? biases)
    {
        if (biases == null) return true;
        return biases.Values.All(v => v >= -1.0f && v <= 1.0f);
    }
}
