using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Gallery;

/// <summary>
/// Endpoint for publishing an artifact to the gallery.
/// POST /api/artifacts/{artifactId}/publish
/// </summary>
public static class PublishArtifactEndpoint
{
    public static void MapPublishArtifactEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/artifacts/{artifactId:guid}/publish", HandleAsync)
            .WithName("PublishArtifact")
            .WithTags("Gallery")
            .RequireAuthorization()
            .WithSummary("Publish artifact to gallery")
            .WithDescription("Publishes an artifact to the public gallery for community discovery.")
            .Produces<GalleryItemDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid artifactId,
        [FromBody] PublishArtifactRequest? request,
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
            var result = await galleryService.PublishArtifactAsync(
                artifactId,
                userId,
                request ?? new PublishArtifactRequest(),
                cancellationToken);

            return Results.Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("only publish your own"))
        {
            return Results.Forbid();
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
