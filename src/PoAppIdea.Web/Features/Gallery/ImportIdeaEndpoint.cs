using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Web.Features.Session;

namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Endpoint for importing an idea from the gallery.
/// POST /api/gallery/{artifactId}/import
/// </summary>
public static class ImportIdeaEndpoint
{
    public static void MapImportIdeaEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/gallery/{artifactId:guid}/import", HandleAsync)
            .WithName("ImportFromGallery")
            .WithTags("Gallery")
            .RequireAuthorization()
            .WithSummary("Import idea from gallery")
            .WithDescription("Creates a new session based on a published artifact from the gallery.")
            .Produces<StartSessionResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid artifactId,
        [FromBody] ImportIdeaRequest? request,
        [FromServices] GalleryService galleryService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId(user);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        try
        {
            var session = await galleryService.ImportFromGalleryAsync(
                artifactId,
                userId,
                request ?? new ImportIdeaRequest(),
                cancellationToken);

            var response = new StartSessionResponse
            {
                SessionId = session.Id,
                AppType = session.AppType,
                ComplexityLevel = session.ComplexityLevel,
                CurrentPhase = session.CurrentPhase,
                CreatedAt = session.CreatedAt
            };

            return Results.Created($"/api/sessions/{session.Id}", response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
