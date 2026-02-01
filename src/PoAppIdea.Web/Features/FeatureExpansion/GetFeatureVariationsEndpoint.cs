using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Endpoint for getting feature variations for a session.
/// GET /api/sessions/{sessionId}/features
/// </summary>
public static class GetFeatureVariationsEndpoint
{
    public static void MapGetFeatureVariationsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/features", HandleAsync)
            .WithName("GetFeatureVariations")
            .WithTags("FeatureExpansion")
            .AllowAnonymous()
            .WithSummary("Get all feature variations for a session")
            .WithDescription("Returns all feature variations generated in Phase 3 for the specified session.")
            .Produces<IReadOnlyList<FeatureVariationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromQuery] Guid? mutationId,
        [FromServices] FeatureExpansionService featureExpansionService,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<FeatureVariationDto> variations;

            if (mutationId.HasValue)
            {
                variations = await featureExpansionService.GetVariationsByMutationAsync(
                    mutationId.Value, cancellationToken);
            }
            else
            {
                variations = await featureExpansionService.GetFeatureVariationsAsync(
                    sessionId, cancellationToken);
            }

            return Results.Ok(variations);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
