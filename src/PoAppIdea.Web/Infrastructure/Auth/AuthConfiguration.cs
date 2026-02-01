using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.GitHub;

namespace PoAppIdea.Web.Infrastructure.Auth;

/// <summary>
/// Configures OAuth authentication providers.
/// </summary>
public static class AuthConfiguration
{
    /// <summary>
    /// Adds OAuth authentication with Google, GitHub, and Microsoft providers.
    /// </summary>
    public static IServiceCollection AddOAuthAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;

            // Configure cookie policy for OAuth state correlation
            // This fixes "The oauth state was missing or invalid" error
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        // Google OAuth (app-prefixed for multi-app key vault)
        var googleConfig = configuration.GetSection("PoAppIdea:Authentication:Google");
        if (!string.IsNullOrEmpty(googleConfig["ClientId"]))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleConfig["ClientId"]!;
                options.ClientSecret = googleConfig["ClientSecret"]!;
                options.CallbackPath = "/signin-google";
                options.SaveTokens = true;
                options.Scope.Add("email");
                options.Scope.Add("profile");

                // Configure correlation cookie for OAuth state
                // Use None + Always for cross-site OAuth redirects
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
            });
        }

        // GitHub OAuth (app-prefixed for multi-app key vault)
        var githubConfig = configuration.GetSection("PoAppIdea:Authentication:GitHub");
        if (!string.IsNullOrEmpty(githubConfig["ClientId"]))
        {
            authBuilder.AddGitHub(options =>
            {
                options.ClientId = githubConfig["ClientId"]!;
                options.ClientSecret = githubConfig["ClientSecret"]!;
                options.CallbackPath = "/signin-github";
                options.SaveTokens = true;
                options.Scope.Add("user:email");

                // Configure correlation cookie for OAuth state
                // Use None + Always for cross-site OAuth redirects
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
            });
        }

        // Microsoft OAuth (app-prefixed for multi-app key vault)
        var microsoftConfig = configuration.GetSection("PoAppIdea:Authentication:Microsoft");
        if (!string.IsNullOrEmpty(microsoftConfig["ClientId"]))
        {
            authBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = microsoftConfig["ClientId"]!;
                options.ClientSecret = microsoftConfig["ClientSecret"]!;
                options.CallbackPath = "/signin-microsoft";
                options.SaveTokens = true;

                // Configure correlation cookie for OAuth state
                // Use None + Always for cross-site OAuth redirects
                options.CorrelationCookie.SameSite = SameSiteMode.None;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
            });
        }

        return services;
    }
}
