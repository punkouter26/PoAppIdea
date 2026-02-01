using Azure.Monitor.OpenTelemetry.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;

namespace PoAppIdea.Web.Infrastructure.Telemetry;

/// <summary>
/// Configures OpenTelemetry with Azure Application Insights.
/// </summary>
public static class TelemetryConfiguration
{
    /// <summary>
    /// Adds OpenTelemetry telemetry with Azure Monitor exporter.
    /// </summary>
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["ApplicationInsights:ConnectionString"];

        if (string.IsNullOrEmpty(connectionString))
        {
            // Skip telemetry if not configured
            return services;
        }

        services.AddOpenTelemetry()
            .UseAzureMonitor(options =>
            {
                options.ConnectionString = connectionString;
            })
            .WithTracing(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSource("PoAppIdea.*");
            })
            .WithMetrics(builder =>
            {
                builder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("PoAppIdea.*");
            });

        return services;
    }

    /// <summary>
    /// Adds custom logging with OpenTelemetry integration.
    /// </summary>
    public static ILoggingBuilder AddTelemetryLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        var connectionString = configuration["ApplicationInsights:ConnectionString"];

        if (!string.IsNullOrEmpty(connectionString))
        {
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
            });
        }

        return builder;
    }
}
