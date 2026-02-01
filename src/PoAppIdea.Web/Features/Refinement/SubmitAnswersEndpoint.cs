using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Endpoint for submitting refinement answers.
/// POST /api/sessions/{sessionId}/refinement/answers
/// </summary>
public static class SubmitAnswersEndpoint
{
    public static void MapSubmitAnswersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/refinement/answers", HandleAsync)
            .WithName("SubmitRefinementAnswers")
            .WithTags("Refinement")
            .WithSummary("Submit answers to refinement questions")
            .WithDescription("Records answers and advances phase when 10 questions answered")
            .Produces<SubmitAnswersResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] SubmitAnswersRequest request,
        [FromServices] RefinementService refinementService,
        CancellationToken cancellationToken)
    {
        // Validate request
        if (request.Answers.Count == 0)
        {
            return Results.BadRequest(new { error = "At least one answer is required" });
        }

        if (request.Answers.Any(a => string.IsNullOrWhiteSpace(a.AnswerText)))
        {
            return Results.BadRequest(new { error = "Answer text cannot be empty" });
        }

        if (request.Answers.Any(a => a.QuestionNumber < 1 || a.QuestionNumber > 10))
        {
            return Results.BadRequest(new { error = "Question number must be between 1 and 10" });
        }

        try
        {
            var response = await refinementService.SubmitAnswersAsync(sessionId, request, cancellationToken);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = "Session not found", sessionId });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return Results.NotFound(new { error = "Session or refinement not found", sessionId });
        }
    }
}
