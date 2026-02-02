using Azure.Data.Tables;
using Azure.Storage.Blobs;
using FluentValidation;
using Microsoft.SemanticKernel;
using PoAppIdea.Core.Interfaces;
using PoAppIdea.Web.Components;
using PoAppIdea.Web.Features.Artifacts;
using PoAppIdea.Web.Features.Auth;
using PoAppIdea.Web.Features.FeatureExpansion;
using PoAppIdea.Web.Features.Gallery;
using PoAppIdea.Web.Features.Personality;
using PoAppIdea.Web.Features.Mutation;
using PoAppIdea.Web.Features.Refinement;
using PoAppIdea.Web.Features.Session;
using PoAppIdea.Web.Features.Spark;
using PoAppIdea.Web.Features.Synthesis;
using PoAppIdea.Web.Features.Visual;
using PoAppIdea.Web.Infrastructure;
using PoAppIdea.Web.Infrastructure.AI;
using PoAppIdea.Web.Infrastructure.Auth;
using PoAppIdea.Web.Infrastructure.Health;
using PoAppIdea.Web.Infrastructure.Storage;
using PoAppIdea.Web.Infrastructure.Telemetry;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add telemetry
builder.Services.AddTelemetry(builder.Configuration);
builder.Logging.AddTelemetryLogging(builder.Configuration);

// Add Blazor components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR with optimized configuration (T175)
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    // T175: Optimize for large artifact outputs (PRDs, Tech Deep Dives)
    options.MaximumReceiveMessageSize = 256 * 1024; // 256KB for receiving
    options.StreamBufferCapacity = 20; // Buffer for streaming scenarios
    options.MaximumParallelInvocationsPerClient = 2; // Limit concurrent invocations
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2); // Longer timeout for AI generation
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});

// Add OpenAPI
builder.Services.AddOpenApi();

// Add authentication
builder.Services.AddOAuthAuthentication(builder.Configuration);

// Add authorization with fallback policy requiring authenticated users
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// Add health checks
builder.Services.AddHealthCheckServices(builder.Configuration);

// Configure Azure Storage
var storageConnectionString = builder.Configuration["AzureStorage:ConnectionString"];
if (!string.IsNullOrEmpty(storageConnectionString))
{
    builder.Services.AddSingleton(_ => new TableServiceClient(storageConnectionString));
    builder.Services.AddSingleton(_ => new BlobServiceClient(storageConnectionString));
    builder.Services.AddSingleton<TableStorageClient>();
    builder.Services.AddSingleton<BlobStorageClient>();

    // Register repositories
    builder.Services.AddScoped<ISessionRepository, SessionRepository>();
    builder.Services.AddScoped<IPersonalityRepository, PersonalityRepository>();
    builder.Services.AddScoped<IIdeaRepository, IdeaRepository>();
    builder.Services.AddScoped<ISwipeRepository, SwipeRepository>();
    builder.Services.AddScoped<IArtifactRepository, ArtifactRepository>();
    builder.Services.AddScoped<IMutationRepository, MutationRepository>();
    builder.Services.AddScoped<IFeatureVariationRepository, FeatureVariationRepository>();
    builder.Services.AddScoped<ISynthesisRepository, SynthesisRepository>();
    builder.Services.AddScoped<IRefinementAnswerRepository, RefinementAnswerRepository>();
    builder.Services.AddScoped<IVisualAssetRepository, VisualAssetRepository>();
}

// Configure Semantic Kernel (only when not in mock mode)
var useMockAI = builder.Configuration.GetValue<bool>("MockAI") || 
    Environment.GetEnvironmentVariable("MOCK_AI")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

if (!useMockAI)
{
    builder.Services.AddSingleton(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        return SemanticKernelConfig.CreateKernel(config);
    });

    // Register IChatCompletionService from Kernel
    builder.Services.AddScoped(sp =>
    {
        var kernel = sp.GetRequiredService<Microsoft.SemanticKernel.Kernel>();
        return kernel.GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();
    });

    // Register real AI implementations
    builder.Services.AddScoped<IIdeaGenerator, IdeaGenerator>();
    builder.Services.AddScoped<IVisualGenerator, VisualGenerator>();
    builder.Services.AddScoped<IArtifactGenerator, ArtifactGenerator>();
    Console.WriteLine("[Config] Using REAL AI services (Azure OpenAI)");
}
else
{
    // Register mock AI implementations (no API calls, no costs)
    builder.Services.AddScoped<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService, MockChatCompletionService>();
    builder.Services.AddScoped<IIdeaGenerator, MockIdeaGenerator>();
    builder.Services.AddScoped<IVisualGenerator, MockVisualGenerator>();
    builder.Services.AddScoped<IArtifactGenerator, MockArtifactGenerator>();
    Console.WriteLine("[Config] Using MOCK AI services (no API calls)");
}

// Add feature services
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<SparkService>();
builder.Services.AddScoped<MutationService>();
builder.Services.AddScoped<FeatureExpansionService>();
builder.Services.AddScoped<SynthesisService>();
builder.Services.AddScoped<SynthesisEngine>();
builder.Services.AddScoped<RefinementService>();
builder.Services.AddScoped<VisualService>();
builder.Services.AddScoped<ArtifactService>();
builder.Services.AddScoped<PersonalityService>();
builder.Services.AddScoped<GalleryService>();

// Add FluentValidation (T168)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add HTTP client factory with resilience
builder.Services.AddHttpClient();

// Add Radzen dialog, notification and tooltip services
builder.Services.AddScoped<Radzen.DialogService>();
builder.Services.AddScoped<Radzen.NotificationService>();
builder.Services.AddScoped<Radzen.TooltipService>();
builder.Services.AddScoped<Radzen.ContextMenuService>();

var app = builder.Build();

// Configure the HTTP request pipeline

// T167: Add global exception handler first (catches all unhandled exceptions)
app.UseGlobalExceptionHandler();

// T169: Add rate limiting for API endpoints
app.UseRateLimiting(options =>
{
    options.RequestsPerWindow = 100;
    options.WindowSeconds = 60;
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files including from Razor Class Libraries like Radzen
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Map health endpoints
app.MapHealthEndpoints();

// Map authentication endpoints
app.MapAuthEndpoints();

// Map Session endpoints
app.MapStartSessionEndpoint();
app.MapGetSessionEndpoint();
app.MapResumeSessionEndpoint();
app.MapListSessionsEndpoint();

// Map Spark endpoints
app.MapGenerateIdeasEndpoint();
app.MapRecordSwipeEndpoint();
app.MapGetTopIdeasEndpoint();

// Map Mutation endpoints
app.MapMutateIdeasEndpoint();
app.MapGetMutationsEndpoint();
app.MapRateMutationEndpoint();
app.MapGetTopMutationsEndpoint();

// Map FeatureExpansion endpoints
app.MapExpandFeaturesEndpoint();
app.MapGetFeatureVariationsEndpoint();
app.MapRateFeatureVariationEndpoint();
app.MapGetTopFeaturesEndpoint();

// Map Synthesis endpoints
app.MapSubmitSelectionsEndpoint();
app.MapSynthesizeEndpoint();
app.MapGetSynthesisEndpoint();
app.MapGetSelectableIdeasEndpoint();

// Map Refinement endpoints
app.MapGetQuestionsEndpoint();
app.MapSubmitAnswersEndpoint();
app.MapGetAnswersEndpoint();

// Map Visual endpoints
app.MapGenerateVisualsEndpoint();
app.MapGetVisualsEndpoint();
app.MapSelectVisualEndpoint();

// Map Artifacts endpoints
app.MapGenerateArtifactsEndpoint();
app.MapGetArtifactsEndpoint();
app.MapGetArtifactEndpoint();
app.MapDownloadArtifactEndpoint();

// Map Personality endpoints
app.MapGetPersonalityEndpoint();
app.MapUpdatePersonalityEndpoint();

// Map Gallery endpoints
app.MapBrowseGalleryEndpoint();
app.MapPublishArtifactEndpoint();
app.MapImportIdeaEndpoint();

// Map SignalR hubs
app.MapHub<SwipeHub>("/hubs/swipe");

// Map Blazor components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
