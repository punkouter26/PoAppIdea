using FluentValidation;
using PoAppIdea.Web.Features.FeatureExpansion;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for ExpandFeaturesRequest.
/// Ensures variation count is valid.
/// </summary>
public sealed class ExpandFeaturesRequestValidator : AbstractValidator<ExpandFeaturesRequest>
{
    public ExpandFeaturesRequestValidator()
    {
        RuleFor(x => x.VariationsPerMutation)
            .InclusiveBetween(1, 20)
            .WithMessage("VariationsPerMutation must be between 1 and 20.");

        RuleFor(x => x.TopMutationIds)
            .Must(ids => ids is null || ids.Count <= 10)
            .WithMessage("Maximum 10 mutation IDs can be provided.");
    }
}

/// <summary>
/// Validator for RateFeatureVariationRequest.
/// Ensures score is within valid range (1-5).
/// </summary>
public sealed class RateFeatureVariationRequestValidator : AbstractValidator<RateFeatureVariationRequest>
{
    public RateFeatureVariationRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1f, 5f)
            .WithMessage("Score must be between 1 and 5.");
    }
}
