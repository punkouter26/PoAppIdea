using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Endpoint for getting all mutations for a session.
/// GET /api/sessions/{sessionId}/mutations
/// </summary>
public static class GetMutationsEndpoint
{
    public static void MapGetMutationsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/mutations", HandleAsync)
            .WithName("GetMutations")
            .WithTags("Mutation")
            .RequireAuthorization()
            .WithSummary("Get all mutations")
            .WithDescription("Returns all mutations generated for the specified session.")
            .Produces<IReadOnlyList<MutationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromServices] MutationService mutationService,
        CancellationToken cancellationToken)
    {
        try
        {
            var mutations = await mutationService.GetMutationsAsync(sessionId, cancellationToken);
            return Results.Ok(mutations);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
