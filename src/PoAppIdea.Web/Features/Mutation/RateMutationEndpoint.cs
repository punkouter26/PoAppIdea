using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Endpoint for rating a mutation.
/// POST /api/sessions/{sessionId}/mutations/{mutationId}/rate
/// </summary>
public static class RateMutationEndpoint
{
    public static void MapRateMutationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/mutations/{mutationId:guid}/rate", HandleAsync)
            .WithName("RateMutation")
            .WithTags("Mutation")
            .RequireAuthorization()
            .WithSummary("Rate a mutation")
            .WithDescription("Allows users to rate a mutation on a 1-5 scale.")
            .Produces<MutationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromRoute] Guid mutationId,
        [FromBody] RateMutationRequest request,
        [FromServices] MutationService mutationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var mutation = await mutationService.RateMutationAsync(sessionId, mutationId, request, cancellationToken);
            return Results.Ok(mutation);
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
