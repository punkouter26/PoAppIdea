using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Artifacts;

/// <summary>
/// Endpoint for generating final artifacts.
/// POST /api/sessions/{sessionId}/artifacts
/// </summary>
public static class GenerateArtifactsEndpoint
{
    public static void MapGenerateArtifactsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/artifacts", HandleAsync)
            .WithName("GenerateArtifacts")
            .WithTags("Artifacts")
            .RequireAuthorization()
            .WithSummary("Generate final artifacts")
            .WithDescription("Generates PRD, Technical Deep-Dive, and Visual Asset Pack based on all session data. Returns 202 Accepted with generation status.")
            .Produces<GenerateArtifactsResponse>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] GenerateArtifactsRequest? request,
        [FromServices] ArtifactService artifactService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await artifactService.GenerateArtifactsAsync(
                sessionId,
                request ?? new GenerateArtifactsRequest(),
                cancellationToken);

            return Results.Accepted(value: response);
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
}
