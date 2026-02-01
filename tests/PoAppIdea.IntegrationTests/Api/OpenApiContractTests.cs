using System.Net;
using System.Text.Json;
using PoAppIdea.IntegrationTests.Infrastructure;

namespace PoAppIdea.IntegrationTests.Api;

/// <summary>
/// API Contract Tests - OpenAPI schema validation.
/// Pattern: Contract Testing - Validates that the live API matches the OpenAPI specification.
/// 
/// Purpose: Catches breaking API changes before E2E tests run, providing earlier feedback.
/// These tests compare the generated OpenAPI document from the running server against
/// expected endpoints and schemas defined in the specification.
/// </summary>
public sealed class OpenApiContractTests : IClassFixture<PoAppIdeaWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly PoAppIdeaWebApplicationFactory _factory;
    private JsonDocument? _openApiDocument;

    public OpenApiContractTests(PoAppIdeaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<JsonDocument> GetOpenApiDocumentAsync()
    {
        if (_openApiDocument is not null)
        {
            return _openApiDocument;
        }

        var response = await _client.GetAsync("/openapi/v1.json");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        _openApiDocument = JsonDocument.Parse(content);

        return _openApiDocument;
    }

    private IEnumerable<string> GetPaths(JsonDocument doc)
    {
        if (doc.RootElement.TryGetProperty("paths", out var paths))
        {
            foreach (var path in paths.EnumerateObject())
            {
                yield return path.Name;
            }
        }
    }

    private bool HasOperation(JsonDocument doc, string path, string operation)
    {
        if (doc.RootElement.TryGetProperty("paths", out var paths) &&
            paths.TryGetProperty(path, out var pathItem))
        {
            return pathItem.TryGetProperty(operation.ToLowerInvariant(), out _);
        }
        return false;
    }

    [Fact]
    public async Task OpenApi_Document_ShouldBeValid()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/openapi/v1.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
        content.Should().Contain("openapi");
    }

    [Fact]
    public async Task OpenApi_ShouldContain_SessionEndpoints()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();
        var paths = GetPaths(doc).ToList();

        // Assert - Session CRUD operations
        paths.Should().Contain("/api/sessions");
        HasOperation(doc, "/api/sessions", "post").Should().BeTrue("Should have POST for session creation");
        HasOperation(doc, "/api/sessions", "get").Should().BeTrue("Should have GET for listing sessions");
    }

    [Fact]
    public async Task OpenApi_ShouldContain_SparkEndpoints()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();
        var paths = GetPaths(doc).ToList();

        // Assert - Spark generation endpoints (ideas endpoint)
        paths.Should().Contain(p => p.Contains("/ideas"),
            "Should have ideas generation endpoints");
    }

    [Fact]
    public async Task OpenApi_ShouldContain_RefinementEndpoints()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();
        var paths = GetPaths(doc).ToList();

        // Assert - Refinement (PM/Architect questions)
        paths.Should().Contain(p => p.Contains("/refinement"),
            "Should have refinement endpoints for PM/Architect questions");
    }

    [Fact]
    public async Task OpenApi_ShouldContain_SynthesisEndpoints()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();
        var paths = GetPaths(doc).ToList();

        // Assert - Synthesis (idea merging)
        paths.Should().Contain(p => p.Contains("/synthesis") || p.Contains("/submit"),
            "Should have synthesis/submit endpoints");
    }

    [Fact]
    public async Task OpenApi_ShouldContain_FeatureExpansionEndpoints()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();
        var paths = GetPaths(doc).ToList();

        // Assert - Feature expansion
        paths.Should().Contain(p => p.Contains("/features"),
            "Should have feature expansion endpoints");
    }

    [Fact]
    public async Task OpenApi_ShouldContain_AuthEndpoints()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();
        var paths = GetPaths(doc).ToList();

        // Assert - Authentication endpoints
        paths.Should().Contain(p => p.Contains("/auth"),
            "Should have authentication endpoints");
    }

    [Fact]
    public async Task OpenApi_ShouldDefine_RequiredSchemas()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();

        // Assert - Core schema definitions exist
        doc.RootElement.TryGetProperty("components", out var components).Should().BeTrue();
        components.TryGetProperty("schemas", out var schemas).Should().BeTrue();

        var schemaNames = new List<string>();
        foreach (var schema in schemas.EnumerateObject())
        {
            schemaNames.Add(schema.Name);
        }

        schemaNames.Should().NotBeEmpty("OpenAPI should define component schemas");

        // Check for common patterns in schema names
        schemaNames.Should().Contain(s =>
            s.Contains("Session", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Idea", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Request", StringComparison.OrdinalIgnoreCase) ||
            s.Contains("Response", StringComparison.OrdinalIgnoreCase),
            "Should define DTOs for API contracts");
    }

    [Theory]
    [InlineData("/api/sessions", "GET")]
    [InlineData("/api/sessions", "POST")]
    public async Task OpenApi_SessionEndpoints_ShouldBeDocumented(string endpoint, string method)
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();

        // Assert
        HasOperation(doc, endpoint, method).Should().BeTrue(
            $"OpenAPI should document {method} {endpoint}");
    }

    [Fact]
    public async Task OpenApi_Version_ShouldBe_V3()
    {
        // Arrange
        var doc = await GetOpenApiDocumentAsync();

        // Assert - Should be OpenAPI 3.x
        doc.RootElement.TryGetProperty("openapi", out var version).Should().BeTrue();
        version.GetString()!.Should().StartWith("3.", "Should use OpenAPI version 3.x");
    }

    [Fact]
    public async Task HealthEndpoints_ShouldBeAccessible_EvenIfNotDocumented()
    {
        // Note: Health endpoints are not included in OpenAPI by default in ASP.NET Core,
        // but they should still be accessible. This validates the actual endpoints work.

        // Act & Assert - Health endpoints respond
        var healthResponse = await _client.GetAsync("/health");
        healthResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);

        var liveResponse = await _client.GetAsync("/health/live");
        liveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var readyResponse = await _client.GetAsync("/health/ready");
        readyResponse.Should().NotBeNull();
    }
}
