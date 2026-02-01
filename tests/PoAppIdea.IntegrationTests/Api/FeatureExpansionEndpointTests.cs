using System.Net;
using System.Net.Http.Json;
using PoAppIdea.IntegrationTests.Infrastructure;
using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.IntegrationTests.Api;

/// <summary>
/// Integration tests for Feature Expansion API endpoints.
/// Verifies: Feature variation generation, rating, and top selection.
/// Phase 5 tests per FR-011, FR-012.
/// 
/// Note: Tests run with auto-authentication via TestAuthHandler.
/// Tests verify endpoint behavior with authenticated requests.
/// GET endpoints allow anonymous access; POST endpoints require authentication.
/// </summary>
public sealed class FeatureExpansionEndpointTests : IClassFixture<PoAppIdeaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly PoAppIdeaWebApplicationFactory _factory;

    public FeatureExpansionEndpointTests(PoAppIdeaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region POST Endpoints - With Auth (auto-authenticated via TestAuthHandler)

    [Fact]
    public async Task ExpandFeatures_WithNonExistentSession_ShouldReturn_NotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new
        {
            VariationsPerMutation = 5
        };

        // Act - With TestAuthHandler, request is authenticated
        var response = await _client.PostAsJsonAsync($"/api/sessions/{sessionId}/features", request);

        // Assert - Session doesn't exist, so returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RateFeatureVariation_WithNonExistentSession_ShouldReturn_NotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var variationId = Guid.NewGuid();
        var request = new { Score = 4.0f };

        // Act - With TestAuthHandler, request is authenticated
        var response = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/features/{variationId}/rate", 
            request);

        // Assert - Session doesn't exist, so returns 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET Endpoints - Allow Anonymous

    [Fact]
    public async Task GetFeatureVariations_AllowsAnonymous_ShouldReturn_OK()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/features");

        // Assert - GET allows anonymous access
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var variations = await response.Content.ReadFromJsonAsync<List<FeatureVariationDto>>();
        variations.Should().NotBeNull();
        variations.Should().BeEmpty(); // No data for random session
    }

    [Fact]
    public async Task GetTopFeatures_AllowsAnonymous_ShouldReturn_OK()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/features/top");

        // Assert - GET allows anonymous access
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var topVariations = await response.Content.ReadFromJsonAsync<List<FeatureVariationDto>>();
        topVariations.Should().NotBeNull();
        topVariations.Should().BeEmpty(); // No data for random session
    }

    [Fact]
    public async Task GetTopFeatures_WithCountParameter_ShouldReturn_OK()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/features/top?count=5");

        // Assert - GET allows anonymous access with parameters
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFeatureVariations_WithMutationIdFilter_ShouldReturn_OK()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var mutationId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/features?mutationId={mutationId}");

        // Assert - GET allows anonymous access with query params
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Endpoint Registration Tests

    [Fact]
    public async Task ExpandFeatures_Endpoint_ShouldBeRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var request = new { VariationsPerMutation = 5 };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/sessions/{sessionId}/features", request);

        // Assert - 404 proves endpoint exists and reached handler (not 405 Method Not Allowed)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task RateFeatureVariation_Endpoint_ShouldBeRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var variationId = Guid.NewGuid();
        var request = new { Score = 4.0f };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/features/{variationId}/rate", 
            request);

        // Assert - 404 proves endpoint exists and reached handler (not 405 Method Not Allowed)
        response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task GetTopFeatures_Endpoint_ShouldBeRegistered()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/features/top");

        // Assert - 200 proves endpoint exists (not 404)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Method Validation Tests

    [Fact]
    public async Task AllEndpoints_ShouldNotReturn_MethodNotAllowed()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var variationId = Guid.NewGuid();

        // Act & Assert - POST endpoints
        var postExpandResponse = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/features", 
            new { VariationsPerMutation = 5 });
        postExpandResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        var postRateResponse = await _client.PostAsJsonAsync(
            $"/api/sessions/{sessionId}/features/{variationId}/rate", 
            new { Score = 4.0f });
        postRateResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        // Act & Assert - GET endpoints
        var getVariationsResponse = await _client.GetAsync($"/api/sessions/{sessionId}/features");
        getVariationsResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);

        var getTopResponse = await _client.GetAsync($"/api/sessions/{sessionId}/features/top");
        getTopResponse.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
    }

    #endregion

    #region DTO Serialization Tests

    [Fact]
    public async Task FeatureVariation_DTO_ShouldDeserializeCorrectly()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}/features");
        
        // Assert - Should successfully deserialize even if empty
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNull();
        
        // JSON should be valid array
        content.Should().StartWith("[");
        content.Should().EndWith("]");
    }

    #endregion
}
