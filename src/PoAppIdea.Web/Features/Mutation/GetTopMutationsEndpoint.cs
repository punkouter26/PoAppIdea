using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Endpoint for getting the top-rated mutations for a session.
/// GET /api/sessions/{sessionId}/mutations/top
/// </summary>
public static class GetTopMutationsEndpoint
{
    public static void MapGetTopMutationsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/mutations/top", HandleAsync)
            .WithName("GetTopMutations")
            .WithTags("Mutation")
            .RequireAuthorization()
            .WithSummary("Get top-rated mutations")
            .WithDescription("Returns the top 10 mutations by score per FR-008.")
            .Produces<IReadOnlyList<MutationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromQuery] int count = 10,
        [FromServices] MutationService? mutationService = null,
        CancellationToken cancellationToken = default)
    {
        if (mutationService is null)
        {
            return Results.Problem("Mutation service not available.");
        }

        try
        {
            var mutations = await mutationService.GetTopMutationsAsync(sessionId, count, cancellationToken);
            return Results.Ok(mutations);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
