namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Request DTO for publishing an artifact to the gallery.
/// POST /api/artifacts/{artifactId}/publish
/// </summary>
public sealed record PublishArtifactRequest
{
    /// <summary>
    /// Optional custom description for the gallery listing.
    /// </summary>
    public string? GalleryDescription { get; init; }

    /// <summary>
    /// Whether to make the artifact searchable in the gallery.
    /// </summary>
    public bool IsSearchable { get; init; } = true;
}
