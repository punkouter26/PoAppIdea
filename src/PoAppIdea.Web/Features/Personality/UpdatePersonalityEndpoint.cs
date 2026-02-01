using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Features.Personality;

/// <summary>
/// Endpoint for updating a user's personality profile.
/// POST /api/users/{userId}/personality
/// </summary>
public static class UpdatePersonalityEndpoint
{
    public static void MapUpdatePersonalityEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/users/{userId:guid}/personality", HandleAsync)
            .WithName("UpdatePersonality")
            .WithTags("Personality")
            .RequireAuthorization()
            .WithSummary("Update user's Product Personality profile")
            .WithDescription("Updates the user's preference biases and disliked patterns.")
            .Produces<GetPersonalityResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid userId,
        [FromBody] UpdatePersonalityRequest request,
        [FromServices] PersonalityService personalityService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // Validate user can update this profile
        var currentUserId = GetCurrentUserId(user);
        if (currentUserId != userId)
        {
            return Results.Forbid();
        }

        try
        {
            var response = await personalityService.UpdatePersonalityAsync(userId, request, cancellationToken);
            return Results.Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
