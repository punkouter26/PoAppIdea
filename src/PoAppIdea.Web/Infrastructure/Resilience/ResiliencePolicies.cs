using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace PoAppIdea.Web.Infrastructure.Resilience;

/// <summary>
/// Configures resilience policies using Polly.
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Default retry policy with exponential backoff.
    /// </summary>
    public static ResiliencePipeline<HttpResponseMessage> CreateHttpPipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new RetryStrategyOptions<HttpResponseMessage>
            {
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutRejectedException>()
                    .HandleResult(r => !r.IsSuccessStatusCode && (int)r.StatusCode >= 500),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<HttpResponseMessage>
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .AddTimeout(TimeSpan.FromSeconds(30))
            .Build();
    }

    /// <summary>
    /// Resilience policy for Azure Storage operations.
    /// </summary>
    public static ResiliencePipeline CreateStoragePipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<Azure.RequestFailedException>(ex => ex.Status >= 500 || ex.Status == 429),
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddTimeout(TimeSpan.FromSeconds(60))
            .Build();
    }

    /// <summary>
    /// Resilience policy for Azure OpenAI operations.
    /// </summary>
    public static ResiliencePipeline CreateAIPipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
                    .Handle<Azure.RequestFailedException>(ex => ex.Status == 429),
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(2),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            })
            .AddTimeout(TimeSpan.FromMinutes(2))
            .Build();
    }

    /// <summary>
    /// Configures standard resilience for HTTP clients.
    /// </summary>
    public static void ConfigureStandardResilience(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("standard", pipeline =>
        {
            pipeline
                .AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                })
                .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(30)
                })
                .AddTimeout(TimeSpan.FromSeconds(30));
        });
    }
}
