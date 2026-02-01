using FluentValidation;
using PoAppIdea.Web.Features.Mutation;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for MutateIdeasRequest.
/// Ensures mutation limits are valid.
/// </summary>
public sealed class MutateIdeasRequestValidator : AbstractValidator<MutateIdeasRequest>
{
    public MutateIdeasRequestValidator()
    {
        RuleFor(x => x.MutationsPerIdea)
            .InclusiveBetween(1, 50)
            .WithMessage("MutationsPerIdea must be between 1 and 50.");

        RuleFor(x => x.TopIdeaIds)
            .Must(ids => ids is null || ids.Count <= 10)
            .WithMessage("Maximum 10 idea IDs can be provided.");
    }
}

/// <summary>
/// Validator for RateMutationRequest.
/// Ensures rating score is within valid range (1-5).
/// </summary>
public sealed class RateMutationRequestValidator : AbstractValidator<RateMutationRequest>
{
    public RateMutationRequestValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1f, 5f)
            .WithMessage("Score must be between 1 and 5.");

        RuleFor(x => x.Feedback)
            .MaximumLength(500)
            .When(x => x.Feedback is not null)
            .WithMessage("Feedback must not exceed 500 characters.");
    }
}
