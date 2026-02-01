using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Endpoint for getting visual assets for a session.
/// GET /api/sessions/{sessionId}/visuals
/// </summary>
public static class GetVisualsEndpoint
{
    public static void MapGetVisualsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/visuals", HandleAsync)
            .WithName("GetVisuals")
            .WithTags("Visual")
            .RequireAuthorization()
            .WithSummary("Get generated visuals")
            .WithDescription("Returns all visual assets generated for the session, including selection status.")
            .Produces<GetVisualsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromServices] VisualService visualService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await visualService.GetVisualsAsync(sessionId, cancellationToken);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
