using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// Endpoint for retrieving all artifacts for a session.
/// GET /api/sessions/{sessionId}/artifacts
/// </summary>
public static class GetArtifactsEndpoint
{
    public static void MapGetArtifactsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/artifacts", HandleAsync)
            .WithName("GetArtifacts")
            .WithTags("Artifacts")
            .RequireAuthorization()
            .WithSummary("Get generated artifacts")
            .WithDescription("Returns all generated artifacts (PRD, Technical Deep-Dive, Visual Asset Pack) for a session.")
            .Produces<IReadOnlyList<ArtifactResponseDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromServices] ArtifactService artifactService,
        CancellationToken cancellationToken)
    {
        try
        {
            var artifacts = await artifactService.GetArtifactsAsync(sessionId, cancellationToken);
            return Results.Ok(artifacts);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
