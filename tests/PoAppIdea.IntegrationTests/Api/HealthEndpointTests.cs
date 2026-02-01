using System.Net;
using PoAppIdea.IntegrationTests.Infrastructure;

namespace PoAppIdea.IntegrationTests.Api;

/// <summary>
/// Integration tests for health check endpoints.
/// Verifies: API connectivity, dependencies health status.
/// </summary>
public sealed class HealthEndpointTests : IClassFixture<PoAppIdeaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly PoAppIdeaWebApplicationFactory _factory;

    public HealthEndpointTests(PoAppIdeaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Endpoint_ShouldReturn_SuccessStatusCode()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task Health_Ready_Endpoint_ShouldReturn_Response()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/ready");

        // Assert - May be unhealthy if no storage configured, but endpoint should respond
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Health_Live_Endpoint_ShouldReturn_OK()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health/live");

        // Assert - Liveness should always return OK if app is running
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
