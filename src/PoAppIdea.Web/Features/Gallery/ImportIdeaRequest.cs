namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Request DTO for importing an idea from the gallery.
/// POST /api/gallery/{artifactId}/import
/// </summary>
public sealed record ImportIdeaRequest
{
    /// <summary>
    /// Whether to start a new session based on the imported idea.
    /// </summary>
    public bool CreateSession { get; init; } = true;

    /// <summary>
    /// Optional notes about why the idea was imported.
    /// </summary>
    public string? ImportNotes { get; init; }
}
