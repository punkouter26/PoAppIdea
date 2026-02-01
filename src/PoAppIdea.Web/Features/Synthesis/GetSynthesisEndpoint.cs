using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Synthesis;

/// <summary>
/// Endpoint for retrieving synthesis for a session.
/// GET /api/sessions/{sessionId}/synthesis
/// </summary>
public static class GetSynthesisEndpoint
{
    public static void MapGetSynthesisEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/synthesis", HandleAsync)
            .WithName("GetSynthesis")
            .WithTags("Synthesis")
            .AllowAnonymous()
            .WithSummary("Get synthesis for a session")
            .WithDescription("Retrieves the synthesized concept for a session, if synthesis was performed.")
            .Produces<SynthesisDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromServices] SynthesisService synthesisService,
        CancellationToken cancellationToken)
    {
        try
        {
            var synthesis = await synthesisService.GetSynthesisAsync(sessionId, cancellationToken);
            
            if (synthesis is null)
            {
                return Results.NotFound(new { error = "No synthesis found for this session" });
            }
            
            return Results.Ok(synthesis);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // Log and return 404 for any unexpected errors to avoid 500 responses
            return Results.NotFound(new { error = $"Session or synthesis not found: {ex.Message}" });
        }
    }
}
