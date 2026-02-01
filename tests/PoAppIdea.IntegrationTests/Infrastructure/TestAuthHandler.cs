using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PoAppIdea.IntegrationTests.Infrastructure;

/// <summary>
/// Test authentication handler that bypasses OAuth for integration tests.
/// Pattern: Strategy (GoF) - Provides mock auth strategy for test environment.
/// 
/// This handler automatically authenticates all requests with a test user,
/// allowing integration tests to verify protected endpoints without real OAuth.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    // Use a valid GUID since endpoints parse NameIdentifier as Guid
    public static readonly Guid TestUserGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string TestUserId = "11111111-1111-1111-1111-111111111111";
    public const string TestUserEmail = "test@poappidea.com";
    public const string TestUserName = "Test User";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Create test claims with valid GUID for NameIdentifier
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId),
            new Claim(ClaimTypes.Email, TestUserEmail),
            new Claim(ClaimTypes.Name, TestUserName),
            new Claim("sub", TestUserId)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
