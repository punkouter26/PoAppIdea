using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Endpoint for browsing the public gallery.
/// GET /api/gallery
/// </summary>
public static class BrowseGalleryEndpoint
{
    public static void MapBrowseGalleryEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/gallery", HandleAsync)
            .WithName("BrowseGallery")
            .WithTags("Gallery")
            .AllowAnonymous()
            .WithSummary("Browse public gallery")
            .WithDescription("Returns published artifacts with optional search and filtering.")
            .Produces<BrowseGalleryResponse>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromServices] GalleryService galleryService,
        CancellationToken cancellationToken,
        [FromQuery] string? query = null,
        [FromQuery] AppType? appType = null,
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null)
    {
        var request = new BrowseGalleryRequest
        {
            Query = query,
            AppType = appType,
            Limit = limit,
            Cursor = cursor
        };

        var response = await galleryService.BrowseGalleryAsync(request, cancellationToken);
        return Results.Ok(response);
    }
}
