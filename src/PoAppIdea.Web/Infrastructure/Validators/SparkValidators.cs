using FluentValidation;
using PoAppIdea.Web.Features.Spark;

namespace PoAppIdea.Web.Infrastructure.Validators;

/// <summary>
/// Validator for GenerateIdeasRequest.
/// Ensures batch size is within allowed bounds.
/// </summary>
public sealed class GenerateIdeasRequestValidator : AbstractValidator<GenerateIdeasRequest>
{
    public GenerateIdeasRequestValidator()
    {
        RuleFor(x => x.BatchSize)
            .InclusiveBetween(1, 50)
            .WithMessage("BatchSize must be between 1 and 50.");

        RuleFor(x => x.SeedPhrase)
            .MaximumLength(500)
            .When(x => x.SeedPhrase is not null)
            .WithMessage("SeedPhrase must not exceed 500 characters.");
    }
}

/// <summary>
/// Validator for RecordSwipeRequest.
/// Ensures swipe data is valid and complete.
/// </summary>
public sealed class RecordSwipeRequestValidator : AbstractValidator<RecordSwipeRequest>
{
    public RecordSwipeRequestValidator()
    {
        RuleFor(x => x.IdeaId)
            .NotEmpty()
            .WithMessage("Idea ID is required.");

        RuleFor(x => x.Direction)
            .IsInEnum()
            .WithMessage("Invalid swipe direction. Must be Left (discard), Right (keep), or Up (super-like).");

        RuleFor(x => x.DurationMs)
            .GreaterThan(0)
            .WithMessage("DurationMs must be greater than 0 milliseconds.");
    }
}
