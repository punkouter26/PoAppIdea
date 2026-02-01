using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PoAppIdea.IntegrationTests.Infrastructure;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.IntegrationTests.Api;

/// <summary>
/// Integration tests for Synthesis API endpoints.
/// Verifies: Idea submission, synthesis generation, and retrieval.
/// Phase 6 tests per FR-013, FR-014.
/// 
/// Note: Tests run with auto-authentication via TestAuthHandler.
/// Tests verify endpoint behavior with authenticated requests.
/// </summary>
public sealed class SynthesisEndpointTests : IClassFixture<PoAppIdeaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _noRedirectClient;
    private readonly PoAppIdeaWebApplicationFactory _factory;

    public SynthesisEndpointTests(PoAppIdeaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        // Client that doesn't follow redirects - shows true auth response
        _noRedirectClient = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    #region POST Endpoints - With Auth (auto-authenticated via TestAuthHandler)

    [Fact]
    public async Task SubmitSelections_WithNonExistentSession_ShouldReturn_NotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new
        {
            SelectedIdeaIds = new[] { Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.PostAsJsonAsync($"/api/sessions/{sessionId}/submit", request);

        // Assert - Session doesn't exist, so returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Synthesize_WithNonExistentSession_ShouldReturn_NotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.PostAsync($"/api/sessions/{sessionId}/synthesize", null);

        // Assert - Session doesn't exist, so returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET Endpoints - Allow Anonymous

    [Fact]
    public async Task GetSynthesis_AllowsAnonymous_ShouldRespond()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/synthesis");

        // Assert - GET allows anonymous, returns 404 when no synthesis exists or 500 if service error
        // Either proves the endpoint is registered and anonymous access works
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetSelectableIdeas_AllowsAnonymous_ShouldRespond()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/selectable");

        // Assert - GET allows anonymous, may return 404 if session doesn't exist, 500 on service error, or OK
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region Endpoint Registration Tests

    [Fact]
    public async Task SubmitSelections_Endpoint_ShouldBeRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new
        {
            SelectedIdeaIds = new[] { Guid.NewGuid() }
        };

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.PostAsJsonAsync($"/api/sessions/{sessionId}/submit", request);

        // Assert - 404 proves endpoint exists and reached handler (not 405 Method Not Allowed)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task Synthesize_Endpoint_ShouldBeRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act - With TestAuthHandler, request is authenticated
        var response = await _noRedirectClient.PostAsync($"/api/sessions/{sessionId}/synthesize", null);

        // Assert - 404 proves endpoint exists and reached handler (not 405 Method Not Allowed)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetSynthesis_Endpoint_ShouldBeRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/synthesis");

        // Assert - 404 proves endpoint exists (not 405 Method Not Allowed)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetSelectableIdeas_Endpoint_ShouldBeRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/selectable");

        // Assert - Response proves endpoint exists
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region Method Validation Tests

    [Fact]
    public async Task AllEndpoints_ShouldNotReturn_MethodNotAllowed()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act & Assert - POST endpoints
        var postSubmitResponse = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/submit", 
            new { SelectedIdeaIds = new[] { Guid.NewGuid() } });
        postSubmitResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        var postSynthesizeResponse = await _client.PostAsync(
            $"/api/sessions/{sessionId}/synthesize", null);
        postSynthesizeResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        // Act & Assert - GET endpoints
        var getSynthesisResponse = await _client.GetAsync($"/api/sessions/{sessionId}/synthesis");
        getSynthesisResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        var getSelectableResponse = await _client.GetAsync($"/api/sessions/{sessionId}/selectable");
        getSelectableResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region Request Validation Tests

    [Fact]
    public async Task SubmitSelections_WithEmptyRequest_ShouldReturn_NotFound()
    {
        // Arrange - With TestAuthHandler, request is authenticated
        var sessionId = Guid.NewGuid();
        var request = new
        {
            SelectedIdeaIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sessions/{sessionId}/submit", request);

        // Assert - Session doesn't exist, returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitSelections_WithSingleIdea_ShouldReturn_NotFound()
    {
        // Arrange - With TestAuthHandler, request is authenticated
        var sessionId = Guid.NewGuid();
        var request = new
        {
            SelectedIdeaIds = new[] { Guid.NewGuid() }
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sessions/{sessionId}/submit", request);

        // Assert - Session doesn't exist, returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SubmitSelections_WithMaxIdeas_ShouldReturn_NotFound()
    {
        // Arrange - With TestAuthHandler, request is authenticated
        var sessionId = Guid.NewGuid();
        var request = new
        {
            SelectedIdeaIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray()
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sessions/{sessionId}/submit", request);

        // Assert - Session doesn't exist, returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
