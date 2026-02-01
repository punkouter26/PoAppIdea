using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Personality;

/// <summary>
/// Endpoint for retrieving a user's personality profile.
/// GET /api/users/{userId}/personality
/// </summary>
public static class GetPersonalityEndpoint
{
    public static void MapGetPersonalityEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/users/{userId:guid}/personality", HandleAsync)
            .WithName("GetPersonality")
            .WithTags("Personality")
            .RequireAuthorization()
            .WithSummary("Get user's Product Personality profile")
            .WithDescription("Returns the user's learned preferences based on swipe behavior across sessions.")
            .Produces<GetPersonalityResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid userId,
        [FromServices] PersonalityService personalityService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken,
        [FromQuery] bool includeDetails = true,
        [FromQuery] bool includeSessionHistory = false)
    {
        // Validate user can access this profile
        var currentUserId = GetCurrentUserId(user);
        if (currentUserId != userId)
        {
            return Results.Forbid();
        }

        var request = new GetPersonalityRequest
        {
            IncludeDetails = includeDetails,
            IncludeSessionHistory = includeSessionHistory
        };

        var response = await personalityService.GetPersonalityAsync(userId, request, cancellationToken);

        return response is null
            ? Results.NotFound(new { error = $"Personality profile for user {userId} not found" })
            : Results.Ok(response);
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
