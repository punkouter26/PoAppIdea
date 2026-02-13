using FluentValidation;
using PoAppIdea.Core.Enums;
using PoAppIdea.Web.Features.Session;
using PoAppIdea.Web.Features.Spark;
using PoAppIdea.Web.Features.Mutation;
using PoAppIdea.Web.Features.FeatureExpansion;
using PoAppIdea.Web.Features.Synthesis;
using PoAppIdea.Web.Features.Refinement;
using PoAppIdea.Web.Features.Visual;
using PoAppIdea.Web.Features.Artifacts;
using PoAppIdea.Web.Features.Personality;
using PoAppIdea.Web.Features.Gallery;

namespace PoAppIdea.Web.Infrastructure.Validators;

#region Session Validators

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

#endregion

#region Spark Validators

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

#endregion

#region Mutation Validators

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

#endregion

#region Feature Expansion Validators

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

#endregion

#region Synthesis Validators

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

#endregion

#region Refinement Validators

/// <summary>
/// Validator for SubmitAnswersRequest.
/// Ensures answers are provided within allowed bounds.
/// </summary>
public sealed class SubmitAnswersRequestValidator : AbstractValidator<SubmitAnswersRequest>
{
    public SubmitAnswersRequestValidator()
    {
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

#endregion

#region Visual Validators

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

#endregion

#region Artifact Validators

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

#endregion

#region Personality Validators

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

#endregion

#region Gallery Validators

/// <summary>
/// Validator for BrowseGalleryRequest.
/// Ensures pagination parameters are valid.
/// </summary>
public sealed class BrowseGalleryRequestValidator : AbstractValidator<BrowseGalleryRequest>
{
    public BrowseGalleryRequestValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100)
            .WithMessage("Limit must be between 1 and 100.");

        RuleFor(x => x.Query)
            .MaximumLength(200)
            .When(x => !string.IsNullOrEmpty(x.Query))
            .WithMessage("Query must not exceed 200 characters.");

        RuleFor(x => x.AppType)
            .IsInEnum()
            .When(x => x.AppType.HasValue)
            .WithMessage("Invalid app type filter.");
    }
}

/// <summary>
/// Validator for PublishArtifactRequest.
/// Ensures gallery description is within limits.
/// </summary>
public sealed class PublishArtifactRequestValidator : AbstractValidator<PublishArtifactRequest>
{
    public PublishArtifactRequestValidator()
    {
        RuleFor(x => x.GalleryDescription)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.GalleryDescription))
            .WithMessage("Gallery description must not exceed 1000 characters.");
    }
}

/// <summary>
/// Validator for ImportIdeaRequest.
/// Ensures import notes are within limits.
/// </summary>
public sealed class ImportIdeaRequestValidator : AbstractValidator<ImportIdeaRequest>
{
    public ImportIdeaRequestValidator()
    {
        RuleFor(x => x.ImportNotes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.ImportNotes))
            .WithMessage("Import notes must not exceed 1000 characters.");
    }
}

#endregion
