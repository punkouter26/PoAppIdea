using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Request DTO for browsing the public gallery.
/// GET /api/gallery
/// </summary>
public sealed record BrowseGalleryRequest
{
    /// <summary>
    /// Optional search query.
    /// </summary>
    public string? Query { get; init; }

    /// <summary>
    /// Filter by app type.
    /// </summary>
    public AppType? AppType { get; init; }

    /// <summary>
    /// Number of items to return (max 100).
    /// </summary>
    public int Limit { get; init; } = 20;

    /// <summary>
    /// Cursor for pagination (opaque string from previous response).
    /// </summary>
    public string? Cursor { get; init; }
}
