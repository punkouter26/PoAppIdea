using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Synthesis;

/// <summary>
/// Endpoint for triggering re-synthesis of selected ideas.
/// POST /api/sessions/{sessionId}/synthesize
/// </summary>
public static class SynthesizeEndpoint
{
    public static void MapSynthesizeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/synthesize", HandleAsync)
            .WithName("Synthesize")
            .WithTags("Synthesis")
            .RequireAuthorization()
            .WithSummary("Trigger synthesis of selected ideas")
            .WithDescription("Re-synthesizes the currently selected ideas into a cohesive concept. Requires at least 2 selected ideas.")
            .Produces<SynthesisDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromServices] SynthesisService synthesisService,
        CancellationToken cancellationToken)
    {
        try
        {
            var synthesis = await synthesisService.SynthesizeAsync(sessionId, cancellationToken);
            return Results.Ok(synthesis);
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
