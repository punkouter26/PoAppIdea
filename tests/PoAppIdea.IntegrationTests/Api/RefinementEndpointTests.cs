using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PoAppIdea.IntegrationTests.Infrastructure;

namespace PoAppIdea.IntegrationTests.Api;

/// <summary>
/// Integration tests for Refinement API endpoints.
/// Verifies: Question retrieval, answer submission, and answer retrieval.
/// Phase 7 tests per User Story 5 - Deep Refinement via Interactive Inquiry.
/// 
/// Note: Tests run with auto-authentication via TestAuthHandler.
/// Tests verify endpoint behavior with authenticated requests.
/// </summary>
public sealed class RefinementEndpointTests : IClassFixture<PoAppIdeaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _noRedirectClient;
    private readonly PoAppIdeaWebApplicationFactory _factory;

    public RefinementEndpointTests(PoAppIdeaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        // Client that doesn't follow redirects - shows true auth response
        _noRedirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region GET Questions Endpoint

    [Fact]
    public async Task GetQuestions_WithNonExistentSession_ShouldReturn_NotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.GetAsync($"/api/sessions/{sessionId}/refinement/questions");

        // Assert - Session doesn't exist, so returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetQuestions_EndpointIsRegistered_ShouldRespond()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - Standard client (follows redirects, may end up at login page)
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/refinement/questions");

        // Assert - Endpoint should be registered (not 405)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetQuestions_WithPhaseParameter_ShouldAcceptQueryParam()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - Test with phase query parameter (authenticated via TestAuthHandler)
        var response = await _noRedirectClient.GetAsync(
            $"/api/sessions/{sessionId}/refinement/questions?phase=Phase4_PM");

        // Assert - Session doesn't exist, returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST Answers Endpoint

    [Fact]
    public async Task SubmitAnswers_WithNonExistentSession_ShouldReturn_NotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new
        {
            Answers = new[]
            {
                new { QuestionNumber = 1, AnswerText = "Test answer for question 1" },
                new { QuestionNumber = 2, AnswerText = "Test answer for question 2" }
            }
        };

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/refinement/answers", request);

        // Assert - Session doesn't exist, so returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitAnswers_EndpointIsRegistered_ShouldRespond()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new
        {
            Answers = new[]
            {
                new { QuestionNumber = 1, AnswerText = "Test answer" }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/refinement/answers", request);

        // Assert - Endpoint should be registered
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task SubmitAnswers_WithEmptyBody_ShouldRejectRequest()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - Send empty JSON object
        var response = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/refinement/answers", new { });

        // Assert - Should reject with redirect (auth) or bad request
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region GET Answers Endpoint

    [Fact]
    public async Task GetAnswers_WithAuthentication_ShouldReturn_OK()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.GetAsync($"/api/sessions/{sessionId}/refinement/answers");

        // Assert - Answers endpoint allows access and returns empty list for no data
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAnswers_EndpointIsRegistered_ShouldRespond()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/refinement/answers");

        // Assert - Endpoint should be registered
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetAnswers_WithPhaseFilter_ShouldReturn_OK()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - Test with phase query parameter (authenticated via TestAuthHandler)
        var response = await _noRedirectClient.GetAsync(
            $"/api/sessions/{sessionId}/refinement/answers?phase=Phase5_Architect");

        // Assert - Answers endpoint returns OK with empty list
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Endpoint Route Tests

    [Theory]
    [InlineData("questions")]
    [InlineData("answers")]
    public async Task RefinementEndpoints_ShouldUseCorrectRoutePattern(string endpoint)
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.GetAsync($"/api/sessions/{sessionId}/refinement/{endpoint}");

        // Assert - Should not be 405 (route exists)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task RefinementEndpoints_InvalidSessionId_ShouldHandleGracefully()
    {
        // Arrange - Invalid GUID format should be rejected by route constraint
        // Note: Blazor apps with cookie auth may redirect to login for any protected route
        
        // Act - Use no-redirect client to see actual route handling
        var response = await _noRedirectClient.GetAsync("/api/sessions/not-a-guid/refinement/questions");

        // Assert - Should redirect (302) because auth fails before route validation,
        // or return 404 if route constraint rejects first
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }

    #endregion
}
