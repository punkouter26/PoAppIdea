using Azure.Data.Tables;
using Azure.Identity;
using Azure.Storage.Blobs;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Web.Infrastructure.Storage;

namespace PoAppIdea.Web.Infrastructure.Configuration;

/// <summary>
/// Extension methods for configuring Azure Storage and repositories.
/// </summary>
public static class StorageExtensions
{
    /// <summary>
    /// Adds Azure Storage clients and repository implementations.
    /// </summary>
    public static IServiceCollection AddAzureStorageAndRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var storageConnectionString = configuration["AzureStorage:ConnectionString"];
        var tableServiceUri = configuration["AzureStorage:TableServiceUri"];
        var blobServiceUri = configuration["AzureStorage:BlobServiceUri"];
        var accountName = configuration["AzureStorage:AccountName"];

        // Auto-construct URIs from account name if not provided
        if (string.IsNullOrWhiteSpace(tableServiceUri) && !string.IsNullOrWhiteSpace(accountName))
        {
            tableServiceUri = $"https://{accountName}.table.core.windows.net";
        }

        if (string.IsNullOrWhiteSpace(blobServiceUri) && !string.IsNullOrWhiteSpace(accountName))
        {
            blobServiceUri = $"https://{accountName}.blob.core.windows.net";
        }

        // Register clients using Managed Identity (preferred) or connection string (fallback)
        if (!string.IsNullOrWhiteSpace(tableServiceUri) && !string.IsNullOrWhiteSpace(blobServiceUri))
        {
            var credential = new DefaultAzureCredential();

            services.AddSingleton(_ => new TableServiceClient(new Uri(tableServiceUri), credential));
            services.AddSingleton(_ => new BlobServiceClient(new Uri(blobServiceUri), credential));
        }
        else if (!string.IsNullOrEmpty(storageConnectionString))
        {
            services.AddSingleton(_ => new TableServiceClient(storageConnectionString));
            services.AddSingleton(_ => new BlobServiceClient(storageConnectionString));
        }

        // Register storage wrapper clients
        services.AddSingleton<TableStorageClient>();
        services.AddSingleton<BlobStorageClient>();

        // Register all repository implementations
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IPersonalityRepository, PersonalityRepository>();
        services.AddScoped<IIdeaRepository, IdeaRepository>();
        services.AddScoped<ISwipeRepository, SwipeRepository>();
        services.AddScoped<IArtifactRepository, ArtifactRepository>();
        services.AddScoped<IMutationRepository, MutationRepository>();
        services.AddScoped<IFeatureVariationRepository, FeatureVariationRepository>();
        services.AddScoped<ISynthesisRepository, SynthesisRepository>();
        services.AddScoped<IRefinementAnswerRepository, RefinementAnswerRepository>();
        services.AddScoped<IVisualAssetRepository, VisualAssetRepository>();

        return services;
    }
}
