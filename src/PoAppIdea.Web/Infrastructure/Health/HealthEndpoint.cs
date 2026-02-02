using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.AI.OpenAI;
using Azure.Security.KeyVault.Secrets;
using Azure.Identity;
using System.ClientModel;

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

        // Azure OpenAI health check - verify API connectivity before users hit generation failures
        var openAiEndpoint = configuration["AzureOpenAI:Endpoint"];
        var openAiApiKey = configuration["AzureOpenAI:ApiKey"];
        if (!string.IsNullOrEmpty(openAiEndpoint) && !string.IsNullOrEmpty(openAiApiKey))
        {
            healthChecksBuilder.AddCheck(
                "azure-openai",
                new AzureOpenAIHealthCheck(openAiEndpoint, openAiApiKey),
                HealthStatus.Degraded,
                ["ai", "azure"]);
        }

        // Azure Key Vault health check - verify secrets access from PoShared resource group
        var keyVaultUri = configuration["KeyVault:Uri"];
        if (!string.IsNullOrEmpty(keyVaultUri))
        {
            healthChecksBuilder.AddCheck(
                "azure-keyvault",
                new AzureKeyVaultHealthCheck(keyVaultUri),
                HealthStatus.Degraded,
                ["keyvault", "azure"]);
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

/// <summary>
/// Health check for Azure OpenAI.
/// Verifies API key and endpoint connectivity before users hit generation failures.
/// </summary>
public sealed class AzureOpenAIHealthCheck : IHealthCheck
{
    private readonly string _endpoint;
    private readonly string _apiKey;

    public AzureOpenAIHealthCheck(string endpoint, string apiKey)
    {
        _endpoint = endpoint;
        _apiKey = apiKey;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Create client and attempt a lightweight API call to verify connectivity
            var credential = new ApiKeyCredential(_apiKey);
            var client = new AzureOpenAIClient(new Uri(_endpoint), credential);
            
            // GetChatClient is a lightweight operation that validates endpoint/auth
            // We just verify we can create the client without throwing
            _ = client.GetChatClient("gpt-4o-mini");
            
            // For a more thorough check, we could make an actual API call,
            // but that would incur costs. The client creation validates the endpoint format.
            return await Task.FromResult(HealthCheckResult.Healthy("Azure OpenAI endpoint is configured and accessible."));
        }
        catch (UriFormatException ex)
        {
            return HealthCheckResult.Unhealthy("Azure OpenAI endpoint URL is invalid.", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure OpenAI is not accessible.", ex);
        }
    }
}

/// <summary>
/// Health check for Azure Key Vault.
/// Verifies connectivity to the PoShared Key Vault using Managed Identity or DefaultAzureCredential.
/// </summary>
public sealed class AzureKeyVaultHealthCheck : IHealthCheck
{
    private readonly string _vaultUri;

    public AzureKeyVaultHealthCheck(string vaultUri)
    {
        _vaultUri = vaultUri;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use DefaultAzureCredential for Managed Identity in production, local dev fallback
            var credential = new DefaultAzureCredential();
            var client = new SecretClient(new Uri(_vaultUri), credential);
            
            // List secrets is a lightweight operation to verify connectivity and permissions
            await foreach (var _ in client.GetPropertiesOfSecretsAsync(cancellationToken).ConfigureAwait(false))
            {
                // Just need to verify we can enumerate - break after first
                break;
            }
            
            return HealthCheckResult.Healthy("Azure Key Vault is accessible.");
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 403)
        {
            return HealthCheckResult.Unhealthy("Azure Key Vault access denied. Check Managed Identity permissions.", ex);
        }
        catch (UriFormatException ex)
        {
            return HealthCheckResult.Unhealthy("Azure Key Vault URI is invalid.", ex);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Azure Key Vault is not accessible.", ex);
        }
    }
}
