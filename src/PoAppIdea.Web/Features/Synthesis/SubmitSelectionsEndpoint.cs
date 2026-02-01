using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Synthesis;

/// <summary>
/// Endpoint for submitting selected ideas for synthesis.
/// POST /api/sessions/{sessionId}/submit
/// </summary>
public static class SubmitSelectionsEndpoint
{
    public static void MapSubmitSelectionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/submit", HandleAsync)
            .WithName("SubmitSelections")
            .WithTags("Synthesis")
            .RequireAuthorization()
            .WithSummary("Submit selected ideas for synthesis")
            .WithDescription("Submits 1-10 selected ideas. If multiple ideas selected, synthesizes them into a cohesive concept with thematic bridging per FR-014.")
            .Produces<SubmitSelectionsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] SubmitSelectionsRequest request,
        [FromServices] SynthesisService synthesisService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await synthesisService.SubmitSelectionsAsync(sessionId, request, cancellationToken);
            return Results.Ok(response);
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
