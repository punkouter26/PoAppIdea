using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Endpoint for generating visual mockups via DALL-E.
/// POST /api/sessions/{sessionId}/visuals
/// </summary>
public static class GenerateVisualsEndpoint
{
    public static void MapGenerateVisualsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/visuals", HandleAsync)
            .WithName("GenerateVisuals")
            .WithTags("Visual")
            .RequireAuthorization()
            .WithSummary("Generate visual mockups")
            .WithDescription("Triggers DALL-E to generate 3 mood boards/mockups for the session. Returns 202 Accepted with generation status.")
            .Produces<GenerateVisualsResponse>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] GenerateVisualsRequest? request,
        [FromServices] VisualService visualService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await visualService.GenerateVisualsAsync(
                sessionId,
                request ?? new GenerateVisualsRequest(),
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
