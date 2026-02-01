using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Blobs;

namespace PoAppIdea.Web.Infrastructure.Health;

/// <summary>
/// Health check endpoint configuration.
/// </summary>
public static class HealthEndpoint
{
    /// <summary>
    /// Adds health check services.
    /// </summary>
    public static IServiceCollection AddHealthCheckServices(this IServiceCollection services, IConfiguration configuration)
    {
        var storageConnectionString = configuration["AzureStorage:ConnectionString"];
        
        var healthChecksBuilder = services.AddHealthChecks();

        // Azure Storage health checks - use simple connection check via service clients
        if (!string.IsNullOrEmpty(storageConnectionString))
        {
            healthChecksBuilder.AddCheck(
                "azure-table-storage",
                new AzureTableStorageHealthCheck(storageConnectionString),
                HealthStatus.Degraded,
                ["storage", "azure"]);

            healthChecksBuilder.AddCheck(
                "azure-blob-storage",
                new AzureBlobStorageHealthCheck(storageConnectionString),
                HealthStatus.Degraded,
                ["storage", "azure"]);
        }

        return services;
    }

    /// <summary>
    /// Maps health check endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Simple liveness check
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // No checks, just returns healthy if app is running
            ResponseWriter = WriteSimpleResponse
        }).AllowAnonymous();

        // Full readiness check with all dependencies
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = WriteDetailedResponse
        }).AllowAnonymous();

        // Detailed health check for monitoring
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = WriteDetailedResponse
        }).AllowAnonymous();

        return endpoints;
    }

    private static Task WriteSimpleResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var response = new { status = report.Status.ToString() };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task WriteDetailedResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                tags = e.Value.Tags,
                exception = e.Value.Exception?.Message
            })
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// Health check for Azure Table Storage.
/// </summary>
public sealed class AzureTableStorageHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public AzureTableStorageHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceClient = new TableServiceClient(_connectionString);
            await serviceClient.GetPropertiesAsync(cancellationToken);
            return HealthCheckResult.Healthy("Azure Table Storage is accessible.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure Table Storage is not accessible.", ex);
        }
    }
}

/// <summary>
/// Health check for Azure Blob Storage.
/// </summary>
public sealed class AzureBlobStorageHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public AzureBlobStorageHealthCheck(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceClient = new BlobServiceClient(_connectionString);
            await serviceClient.GetPropertiesAsync(cancellationToken);
            return HealthCheckResult.Healthy("Azure Blob Storage is accessible.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure Blob Storage is not accessible.", ex);
        }
    }
}
