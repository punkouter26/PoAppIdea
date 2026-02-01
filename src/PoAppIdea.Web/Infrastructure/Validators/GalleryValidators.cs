using FluentValidation;
using PoAppIdea.Web.Features.Gallery;

namespace PoAppIdea.Web.Infrastructure.Validators;

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
