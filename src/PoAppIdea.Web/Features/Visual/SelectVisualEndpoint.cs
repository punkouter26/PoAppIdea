using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Visual;

/// <summary>
/// Endpoint for selecting a visual direction.
/// POST /api/sessions/{sessionId}/visuals/{assetId}/select
/// </summary>
public static class SelectVisualEndpoint
{
    public static void MapSelectVisualEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/visuals/{assetId:guid}/select", HandleAsync)
            .WithName("SelectVisual")
            .WithTags("Visual")
            .RequireAuthorization()
            .WithSummary("Select a visual direction")
            .WithDescription("Marks the specified visual as the selected direction for the session. Deselects any previously selected visual.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromRoute] Guid assetId,
        [FromBody] SelectVisualRequest? request,
        [FromServices] VisualService visualService,
        CancellationToken cancellationToken)
    {
        try
        {
            await visualService.SelectVisualAsync(sessionId, assetId, cancellationToken);
            return Results.Ok(new { message = "Visual selected successfully", assetId });
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
