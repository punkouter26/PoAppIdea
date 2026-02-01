using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Endpoint for generating mutations from top ideas.
/// POST /api/sessions/{sessionId}/mutations
/// </summary>
public static class MutateIdeasEndpoint
{
    public static void MapMutateIdeasEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/mutations", HandleAsync)
            .WithName("MutateIdeas")
            .WithTags("Mutation")
            .RequireAuthorization()
            .WithSummary("Generate mutations from top ideas")
            .WithDescription("Generates crossover and repurposing mutations from top 5 ideas per FR-007. Creates 50 mutations total (10 per top idea).")
            .Produces<MutateIdeasResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] MutateIdeasRequest request,
        [FromServices] MutationService mutationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await mutationService.GenerateMutationsAsync(sessionId, request, cancellationToken);
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
