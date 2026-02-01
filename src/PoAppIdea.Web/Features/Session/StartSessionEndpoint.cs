using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PoAppIdea.Web.Infrastructure;

namespace PoAppIdea.Web.Features.Session;

/// <summary>
/// Endpoint for starting a new session.
/// POST /api/sessions
/// </summary>
public static class StartSessionEndpoint
{
    public static void MapStartSessionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/sessions", HandleAsync)
            .WithName("StartSession")
            .WithTags("Session")
            .WithSummary("Start a new ideation session")
            .WithDescription("Creates a new session with the specified app type and complexity level")
            .RequireAuthorization()
            .Produces<StartSessionResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] StartSessionRequest request,
        [FromServices] SessionService sessionService,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        // Validate request
        if (request.ComplexityLevel < 1 || request.ComplexityLevel > 5)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["ComplexityLevel"] = ["Complexity level must be between 1 and 5"]
            });
        }

        try
        {
            var response = await sessionService.StartSessionAsync(userId, request, cancellationToken);
            return Results.Created($"/api/sessions/{response.SessionId}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Request"] = [ex.Message]
            });
        }
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        return UserIdHelper.GetUserId(user) ?? Guid.Empty;
    }
}
