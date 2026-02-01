using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.FeatureExpansion;

/// <summary>
/// Endpoint for generating feature variations from top mutations.
/// POST /api/sessions/{sessionId}/features
/// </summary>
public static class ExpandFeaturesEndpoint
{
    public static void MapExpandFeaturesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/features", HandleAsync)
            .WithName("ExpandFeatures")
            .WithTags("FeatureExpansion")
            .RequireAuthorization()
            .WithSummary("Generate feature variations from top mutations")
            .WithDescription("Generates feature variations with different themes from top 10 mutations per FR-011. Creates 50 variations total (5 per mutation).")
            .Produces<ExpandFeaturesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] ExpandFeaturesRequest request,
        [FromServices] FeatureExpansionService featureExpansionService,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await featureExpansionService.GenerateFeatureVariationsAsync(sessionId, request, cancellationToken);
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
