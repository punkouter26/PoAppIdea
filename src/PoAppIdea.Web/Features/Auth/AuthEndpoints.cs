using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace PoAppIdea.Web.Features.Auth;

/// <summary>
/// Authentication endpoints for OAuth login/logout flows.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Maps authentication endpoints for login, logout, and callback handling.
    /// </summary>
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var auth = app.MapGroup("/auth")
            .WithTags("Authentication");

        // Test login endpoint - DEVELOPMENT ONLY
        // Allows E2E tests to authenticate without real OAuth
        if (app.Environment.IsDevelopment())
        {
            auth.MapGet("/test-login", async (HttpContext context, string? returnUrl, string? userId, string? email, string? name) =>
            {
                Console.WriteLine("[Auth] Test login endpoint called (Development only)");

                var testUserId = userId ?? "test-user-e2e";
                var testEmail = email ?? "test@example.com";
                var testName = name ?? "E2E Test User";

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, testUserId),
                    new(ClaimTypes.Name, testName),
                    new(ClaimTypes.Email, testEmail),
                    new("http://schemas.microsoft.com/identity/claims/identityprovider", "TestProvider")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await context.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties { IsPersistent = true });

                Console.WriteLine($"[Auth] Test user signed in: {testName} ({testEmail})");

                return Results.Redirect(returnUrl ?? "/");
            })
            .WithName("TestLogin")
            .WithSummary("Development-only test login for E2E tests")
            .AllowAnonymous();
        }

        // Login endpoint - initiates OAuth flow with specified provider
        auth.MapGet("/login/{provider}", async (string provider, HttpContext context, string? returnUrl) =>
        {
            Console.WriteLine($"[Auth] Login initiated for provider: {provider}");

            var validProviders = new[] { "Google", "GitHub", "Microsoft" };
            if (!validProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"[Auth] Invalid provider requested: {provider}");
                return Results.BadRequest($"Invalid authentication provider: {provider}");
            }

            var redirectUri = returnUrl ?? "/";
            var properties = new AuthenticationProperties
            {
                RedirectUri = $"/auth/callback?returnUrl={Uri.EscapeDataString(redirectUri)}",
                IsPersistent = true
            };

            Console.WriteLine($"[Auth] Challenging with provider: {provider}, redirect: {properties.RedirectUri}");
            return Results.Challenge(properties, [provider]);
        })
        .WithName("Login")
        .WithSummary("Initiate OAuth login with specified provider")
        .AllowAnonymous();

        // Callback endpoint - handles OAuth callback after successful authentication
        auth.MapGet("/callback", async (HttpContext context, string? returnUrl) =>
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine($"[Auth] Callback: User authenticated - {user.Identity.Name}");
                var redirect = returnUrl ?? "/";
                return Results.Redirect(redirect);
            }

            Console.WriteLine("[Auth] Callback: Authentication failed, redirecting to login");
            return Results.Redirect("/login");
        })
        .WithName("AuthCallback")
        .WithSummary("Handle OAuth callback after authentication")
        .AllowAnonymous();

        // Logout endpoint - signs out user and clears session
        auth.MapGet("/logout", async (HttpContext context, string? returnUrl) =>
        {
            Console.WriteLine($"[Auth] Logout initiated for user: {context.User.Identity?.Name}");

            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            Console.WriteLine("[Auth] User signed out successfully");
            return Results.Redirect(returnUrl ?? "/");
        })
        .WithName("Logout")
        .WithSummary("Sign out current user")
        .AllowAnonymous();

        // User info endpoint - returns current user information
        auth.MapGet("/me", (HttpContext context) =>
        {
            var user = context.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
            Console.WriteLine($"[Auth] User info requested for: {user.Identity.Name}");

            return Results.Ok(new
            {
                IsAuthenticated = true,
                Name = user.Identity.Name,
                Email = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                Provider = user.FindFirst("http://schemas.microsoft.com/identity/claims/identityprovider")?.Value
                    ?? user.Identity.AuthenticationType,
                Claims = claims
            });
        })
        .WithName("GetCurrentUser")
        .WithSummary("Get current authenticated user information")
        .RequireAuthorization();
    }
}
