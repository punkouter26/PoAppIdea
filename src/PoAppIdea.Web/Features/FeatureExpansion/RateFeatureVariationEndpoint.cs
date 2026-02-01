using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Endpoint for rating a feature variation.
/// POST /api/sessions/{sessionId}/features/{variationId}/rate
/// </summary>
public static class RateFeatureVariationEndpoint
{
    public static void MapRateFeatureVariationEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/features/{variationId:guid}/rate", HandleAsync)
            .WithName("RateFeatureVariation")
            .WithTags("FeatureExpansion")
            .RequireAuthorization()
            .WithSummary("Rate a feature variation")
            .WithDescription("Records a user rating (1-5) for a feature variation to help identify top candidates for submission.")
            .Produces<FeatureVariationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromRoute] Guid variationId,
        [FromBody] RateFeatureVariationRequest request,
        [FromServices] FeatureExpansionService featureExpansionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await featureExpansionService.RateVariationAsync(
                sessionId, variationId, request, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
