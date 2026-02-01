using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Azure;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Configuration for Semantic Kernel with Azure OpenAI.
/// </summary>
public static class SemanticKernelConfig
{
    /// <summary>
    /// Configures Semantic Kernel with Azure OpenAI services.
    /// </summary>
    public static Kernel CreateKernel(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured");
        var apiKey = configuration["AzureOpenAI:ApiKey"]
            ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is not configured");
        var chatDeployment = configuration["AzureOpenAI:ChatDeployment"] ?? "gpt-4o";
        var imageDeployment = configuration["AzureOpenAI:ImageDeployment"] ?? "dall-e-3";

        var builder = Kernel.CreateBuilder();

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: chatDeployment,
            endpoint: endpoint,
            apiKey: apiKey);

        return builder.Build();
    }

    /// <summary>
    /// Creates an Azure OpenAI client for DALL-E image generation.
    /// </summary>
    public static AzureOpenAIClient CreateImageClient(IConfiguration configuration)
    {
        var endpoint = configuration["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("AzureOpenAI:Endpoint is not configured");
        var apiKey = configuration["AzureOpenAI:ApiKey"]
            ?? throw new InvalidOperationException("AzureOpenAI:ApiKey is not configured");

        return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }

    /// <summary>
    /// Gets the image deployment name from configuration.
    /// </summary>
    public static string GetImageDeployment(IConfiguration configuration)
    {
        return configuration["AzureOpenAI:ImageDeployment"] ?? "dall-e-3";
    }
}
