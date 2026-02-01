using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Endpoint for getting top feature variations by score.
/// GET /api/sessions/{sessionId}/features/top
/// </summary>
public static class GetTopFeaturesEndpoint
{
    public static void MapGetTopFeaturesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/sessions/{sessionId:guid}/features/top", HandleAsync)
            .WithName("GetTopFeatures")
            .WithTags("FeatureExpansion")
            .AllowAnonymous()
            .WithSummary("Get top rated feature variations")
            .WithDescription("Returns the top 10 feature variations by user rating, identifying proto-apps for submission phase.")
            .Produces<IReadOnlyList<FeatureVariationDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromQuery] int? count,
        [FromServices] FeatureExpansionService featureExpansionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var variations = await featureExpansionService.GetTopVariationsAsync(
                sessionId, count, cancellationToken);
            return Results.Ok(variations);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
    }
}
