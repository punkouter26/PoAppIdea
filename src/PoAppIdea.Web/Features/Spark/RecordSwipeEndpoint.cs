using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PoAppIdea.Web.Infrastructure;

namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Endpoint for recording a swipe action.
/// POST /api/sessions/{sessionId}/swipes
/// </summary>
public static class RecordSwipeEndpoint
{
    public static void MapRecordSwipeEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions/{sessionId:guid}/swipes", HandleAsync)
            .WithName("RecordSwipe")
            .WithTags("Spark")
            .RequireAuthorization()
            .WithSummary("Record a swipe action")
            .WithDescription("Records a user's swipe (left/right/up) on an idea with timing data for hesitation detection per FR-004.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        [FromRoute] Guid sessionId,
        [FromBody] RecordSwipeRequest request,
        [FromServices] SparkService sparkService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        // Validate request
        if (request.DurationMs < 0)
        {
            return Results.BadRequest(new { error = "Duration cannot be negative" });
        }

        try
        {
            await sparkService.RecordSwipeAsync(sessionId, userId, request, cancellationToken);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return Results.NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        return UserIdHelper.GetUserId(user) ?? Guid.Empty;
    }
}
