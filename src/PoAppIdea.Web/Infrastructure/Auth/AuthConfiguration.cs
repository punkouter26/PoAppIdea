using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PoAppIdea.Web.Infrastructure.Auth;

/// <summary>
/// Configures OAuth authentication providers.
/// Pattern: Strategy Pattern - Each OAuth provider is a pluggable authentication strategy.
/// </summary>
public static class AuthConfiguration
{
    /// <summary>
    /// Adds OAuth authentication with Google and Microsoft providers.
    /// </summary>
    public static IServiceCollection AddOAuthAuthentication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
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
            // Use Lax for same-site navigation compatibility
            // Use SameAsRequest in dev (allows localhost), Always in production (requires HTTPS)
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = environment.IsDevelopment() 
                ? CookieSecurePolicy.SameAsRequest 
                : CookieSecurePolicy.Always;
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = ".PoAppIdea.Auth";
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
                // Use Lax to ensure cookie flows through OAuth redirect chain
                // This fixes "The oauth state was missing or invalid" error
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = environment.IsDevelopment() 
                    ? CookieSecurePolicy.SameAsRequest 
                    : CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
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
                // Use Lax to ensure cookie flows through OAuth redirect chain
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = environment.IsDevelopment() 
                    ? CookieSecurePolicy.SameAsRequest 
                    : CookieSecurePolicy.Always;
                options.CorrelationCookie.HttpOnly = true;
                options.CorrelationCookie.IsEssential = true;
            });
        }

        return services;
    }
}
