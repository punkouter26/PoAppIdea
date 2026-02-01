using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Endpoint for getting the top-rated ideas for a session.
/// GET /api/sessions/{sessionId}/ideas/top
/// </summary>
public static class GetTopIdeasEndpoint
{
    public static void MapGetTopIdeasEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/ideas/top", HandleAsync)
            .WithName("GetTopIdeas")
            .WithTags("Spark")
            .RequireAuthorization()
            .WithSummary("Get top-rated ideas")
            .WithDescription("Returns the top 5 ideas based on user swipes and AI scoring per FR-006.")
            .Produces<IReadOnlyList<IdeaDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromServices] SparkService sparkService,
        CancellationToken cancellationToken)
    {
        try
        {
            var topIdeas = await sparkService.GetTopIdeasAsync(sessionId, cancellationToken);
            return Results.Ok(topIdeas);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
