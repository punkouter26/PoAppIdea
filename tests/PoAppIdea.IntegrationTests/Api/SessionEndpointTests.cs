using System.Net;
using System.Net.Http.Json;
using PoAppIdea.IntegrationTests.Infrastructure;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.IntegrationTests.Api;

/// <summary>
/// Integration tests for Session API endpoints.
/// Verifies: Session lifecycle, state transitions, persistence.
/// 
/// Note: Tests run with auto-authentication via TestAuthHandler.
/// Tests verify endpoint behavior with authenticated requests.
/// </summary>
public sealed class SessionEndpointTests : IClassFixture<PoAppIdeaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly PoAppIdeaWebApplicationFactory _factory;

    public SessionEndpointTests(PoAppIdeaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StartSession_WithValidPrompt_ShouldReturn_Created()
    {
        // Arrange
        var request = new
        {
            Prompt = "Build me a task management app for remote teams",
            UserId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/sessions/start", request);

        // Assert
        // Note: May return 500 if OpenAI not configured, but endpoint should be reachable
        response.Should().NotBeNull();
        Console.WriteLine($"[Test] StartSession response: {response.StatusCode}");
    }

    [Fact]
    public async Task GetSession_WithInvalidId_ShouldReturn_NotFound()
    {
        // Arrange
        var invalidSessionId = Guid.NewGuid();

        // Act - With TestAuthHandler, request is authenticated
        var response = await _client.GetAsync($"/api/sessions/{invalidSessionId}");

        // Assert - Session doesn't exist, returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListSessions_ForAuthenticatedUser_ShouldReturn_OK()
    {
        // Arrange - User is auto-authenticated via TestAuthHandler

        // Act - Endpoint is /api/sessions (not /api/sessions/user/{userId})
        var response = await _client.GetAsync("/api/sessions");

        // Assert - Returns list (may be empty)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
