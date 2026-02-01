using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// Endpoint for retrieving a single artifact by ID.
/// GET /api/artifacts/{artifactId}
/// </summary>
public static class GetArtifactEndpoint
{
    public static void MapGetArtifactEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/artifacts/{artifactId:guid}", HandleAsync)
            .WithName("GetArtifact")
            .WithTags("Artifacts")
            .RequireAuthorization()
            .WithSummary("Get a single artifact")
            .WithDescription("Returns a single artifact by its unique identifier.")
            .Produces<ArtifactResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid artifactId,
        [FromServices] ArtifactService artifactService,
        CancellationToken cancellationToken)
    {
        var artifact = await artifactService.GetArtifactByIdAsync(artifactId, cancellationToken);
        return artifact is null
            ? Results.NotFound(new { error = $"Artifact {artifactId} not found" })
            : Results.Ok(artifact);
    }
}
