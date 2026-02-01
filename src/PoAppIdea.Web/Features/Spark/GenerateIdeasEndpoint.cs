using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Endpoint for generating a batch of ideas for a session.
/// POST /api/sessions/{sessionId}/ideas
/// </summary>
public static class GenerateIdeasEndpoint
{
    public static void MapGenerateIdeasEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/ideas", HandleAsync)
            .WithName("GenerateIdeas")
            .WithTags("Spark")
            .RequireAuthorization()
            .WithSummary("Generate a batch of ideas")
            .WithDescription("Generates a batch of AI-powered app ideas for the specified session, incorporating learning from previous swipes.")
            .Produces<GenerateIdeasResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] GenerateIdeasRequest request,
        [FromServices] SparkService sparkService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await sparkService.GenerateIdeasAsync(sessionId, request, cancellationToken);
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
