using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Synthesis;

/// <summary>
/// Endpoint for retrieving selectable ideas for submission.
/// GET /api/sessions/{sessionId}/selectable
/// </summary>
public static class GetSelectableIdeasEndpoint
{
    public static void MapGetSelectableIdeasEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/selectable", HandleAsync)
            .WithName("GetSelectableIdeas")
            .WithTags("Synthesis")
            .AllowAnonymous()
            .WithSummary("Get selectable ideas for submission")
            .WithDescription("Returns the top feature variations that can be selected for submission.")
            .Produces<List<SelectableIdeaDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromServices] SynthesisService synthesisService,
        CancellationToken cancellationToken)
    {
        try
        {
            var selectableIdeas = await synthesisService.GetSelectableIdeasAsync(sessionId, cancellationToken);
            return Results.Ok(selectableIdeas);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // Log and return 404 for any unexpected errors to avoid 500 responses
            return Results.NotFound(new { error = $"Session not found: {ex.Message}" });
        }
    }
}
