using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Endpoint for getting refinement questions.
/// GET /api/sessions/{sessionId}/refinement/questions
/// </summary>
public static class GetQuestionsEndpoint
{
    public static void MapGetQuestionsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/refinement/questions", HandleAsync)
            .WithName("GetRefinementQuestions")
            .WithTags("Refinement")
            .WithSummary("Get current phase questions")
            .WithDescription("Returns 10 questions for the current refinement phase (PM or Architect)")
            .Produces<GetQuestionsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromQuery] RefinementPhase? phase,
        [FromServices] RefinementService refinementService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await refinementService.GetQuestionsAsync(sessionId, phase, cancellationToken);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = "Session not found", sessionId });
        }
        catch (Exception)
        {
            return Results.NotFound(new { error = "Session or refinement not found", sessionId });
        }
    }
}
