using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using PoAppIdea.Web.Infrastructure.AI;
using Testcontainers.Azurite;

namespace PoAppIdea.IntegrationTests.Infrastructure;

/// <summary>
/// Custom WebApplicationFactory with Testcontainers support.
/// Spins up ephemeral Azurite (Azure Storage Emulator) for integration tests.
/// Pattern: Factory Method (GoF) - Creates test server with isolated infrastructure.
/// </summary>
public sealed class PoAppIdeaWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SemaphoreSlim _startupLock = new(1, 1);
    private bool _isAzuriteStarted;
    private string _azuriteConnectionString = "UseDevelopmentStorage=true";

    private readonly AzuriteContainer _azuriteContainer = new AzuriteBuilder()
        .WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
        .WithPortBinding(10000, true) // Blob
        .WithPortBinding(10001, true) // Queue
        .WithPortBinding(10002, true) // Table
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(10002))
        .Build();

    public string AzuriteConnectionString => _azuriteConnectionString;

    public async Task InitializeAsync()
    {
        await EnsureAzuriteStartedAsync();
    }

    private async Task EnsureAzuriteStartedAsync()
    {
        if (_isAzuriteStarted)
        {
            return;
        }

        await _startupLock.WaitAsync();

        try
        {
            if (_isAzuriteStarted)
            {
                return;
            }

            await _azuriteContainer.StartAsync();
            _azuriteConnectionString = _azuriteContainer.GetConnectionString();
            _isAzuriteStarted = true;
            Console.WriteLine($"[Integration Tests] Azurite started on connection: {_azuriteConnectionString}");
        }
        finally
        {
            _startupLock.Release();
        }
    }

    public new async Task DisposeAsync()
    {
        await _azuriteContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        EnsureAzuriteStartedAsync().GetAwaiter().GetResult();

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Override Azure Storage connection string with Azurite
            // This allows tests to use real Azure Table/Blob storage APIs
            // against an ephemeral container

            // Add test authentication scheme for protected endpoints
            // Pattern: Test Double (Stub) - Replaces real OAuth with predictable auth
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, options => { });

            // Replace Azure OpenAI service with mock
            // Pattern: Test Double (Fake) - Avoids API costs and network dependencies
            services.AddSingleton<IChatCompletionService, MockChatCompletionService>();
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Override configuration with test-specific values
            var testConfig = new Dictionary<string, string?>
            {
                ["AzureStorage:ConnectionString"] = _azuriteConnectionString,
                ["AzureStorage:TableServiceUri"] = string.Empty,
                ["AzureStorage:BlobServiceUri"] = string.Empty,
                ["AzureStorage:AccountName"] = string.Empty,
                ["AzureOpenAI:Endpoint"] = "https://test-endpoint.openai.azure.com/",
                ["AzureOpenAI:ApiKey"] = "test-api-key-for-integration-tests"
            };

            config.AddInMemoryCollection(testConfig);
        });
    }
}
