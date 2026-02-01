using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Endpoint for getting refinement answers.
/// GET /api/sessions/{sessionId}/refinement/answers
/// </summary>
public static class GetAnswersEndpoint
{
    public static void MapGetAnswersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/refinement/answers", HandleAsync)
            .WithName("GetRefinementAnswers")
            .WithTags("Refinement")
            .WithSummary("Get submitted refinement answers")
            .WithDescription("Returns all answers for a session, optionally filtered by phase")
            .Produces<GetAnswersResponse>(StatusCodes.Status200OK)
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
            var answers = await refinementService.GetAnswersAsync(sessionId, phase, cancellationToken);

            // Map to response DTOs
            var answerDtos = answers.Select(a => new RefinementAnswerDto
            {
                Id = a.Id,
                Phase = a.Phase,
                PhaseDisplayName = a.Phase == RefinementPhase.Phase4_PM
                    ? "Product Manager"
                    : "Technical Architect",
                QuestionNumber = a.QuestionNumber,
                QuestionText = a.QuestionText,
                Category = a.QuestionCategory,
                AnswerText = a.AnswerText,
                Timestamp = a.Timestamp
            }).OrderBy(a => a.Phase).ThenBy(a => a.QuestionNumber).ToList();

            return Results.Ok(new GetAnswersResponse
            {
                SessionId = sessionId,
                TotalAnswers = answerDtos.Count,
                PmAnswersCount = answerDtos.Count(a => a.Phase == RefinementPhase.Phase4_PM),
                ArchitectAnswersCount = answerDtos.Count(a => a.Phase == RefinementPhase.Phase5_Architect),
                Answers = answerDtos
            });
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

/// <summary>
/// Response DTO for getting answers.
/// </summary>
public sealed record GetAnswersResponse
{
    public required Guid SessionId { get; init; }
    public required int TotalAnswers { get; init; }
    public required int PmAnswersCount { get; init; }
    public required int ArchitectAnswersCount { get; init; }
    public required IReadOnlyList<RefinementAnswerDto> Answers { get; init; }
}

/// <summary>
/// Individual answer DTO.
/// </summary>
public sealed record RefinementAnswerDto
{
    public required Guid Id { get; init; }
    public required RefinementPhase Phase { get; init; }
    public required string PhaseDisplayName { get; init; }
    public required int QuestionNumber { get; init; }
    public required string QuestionText { get; init; }
    public required string Category { get; init; }
    public required string AnswerText { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
}
