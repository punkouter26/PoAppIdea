using System.Collections.Concurrent;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace PoAppIdea.Web.Infrastructure;

/// <summary>
/// Rate limiting middleware using sliding window algorithm.
/// Pattern: Token Bucket - limits requests per time window per client.
/// Uses periodic timer-based cleanup to prevent memory bloat from expired buckets.
/// </summary>
public sealed class RateLimitingMiddleware : IDisposable
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, RateLimitBucket> _buckets = new();
    private readonly Timer _cleanupTimer;
    private bool _disposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;

        // Initialize periodic cleanup timer - runs every CleanupIntervalSeconds
        // This prevents unbounded memory growth from accumulated rate limit buckets
        var cleanupInterval = TimeSpan.FromSeconds(options.CleanupIntervalSeconds);
        _cleanupTimer = new Timer(
            callback: _ => CleanupExpiredBuckets(),
            state: null,
            dueTime: cleanupInterval,
            period: cleanupInterval);

        _logger.LogDebug(
            "Rate limiting initialized with {RequestsPerWindow} requests per {WindowSeconds}s window, cleanup every {CleanupInterval}s",
            options.RequestsPerWindow,
            options.WindowSeconds,
            options.CleanupIntervalSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for health checks and static files
        var path = context.Request.Path.Value ?? "";
        if (ShouldSkipRateLimiting(path))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var endpoint = GetEndpointKey(context);
        var bucketKey = $"{clientId}:{endpoint}";

        var bucket = _buckets.GetOrAdd(bucketKey, _ => new RateLimitBucket(_options));
        var result = bucket.TryConsume();

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit"] = _options.RequestsPerWindow.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = result.RemainingRequests.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = result.ResetTime.ToUnixTimeSeconds().ToString();

        if (!result.IsAllowed)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId} on endpoint {Endpoint}. Reset at {ResetTime}",
                clientId,
                endpoint,
                result.ResetTime);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/problem+json";
            context.Response.Headers["Retry-After"] = result.RetryAfterSeconds.ToString();

            var problem = new ProblemDetails
            {
                Status = (int)HttpStatusCode.TooManyRequests,
                Title = "Too Many Requests",
                Detail = $"Rate limit exceeded. Please try again in {result.RetryAfterSeconds} seconds.",
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc6585#section-4"
            };
            problem.Extensions["retryAfter"] = result.RetryAfterSeconds;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
            return;
        }

        await _next(context);
    }

    private static bool ShouldSkipRateLimiting(string path)
    {
        return path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/js", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
               path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Prefer authenticated user ID
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        var ipAddress = forwarded?.Split(',').FirstOrDefault()?.Trim() 
            ?? context.Connection.RemoteIpAddress?.ToString() 
            ?? "unknown";

        return $"ip:{ipAddress}";
    }

    private static string GetEndpointKey(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";

        // Normalize path by removing IDs (GUIDs)
        var normalizedPath = System.Text.RegularExpressions.Regex.Replace(
            path,
            @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
            "{id}");

        return $"{method}:{normalizedPath}";
    }

    private void CleanupExpiredBuckets()
    {
        if (_disposed) return;

        var now = DateTimeOffset.UtcNow;
        var initialCount = _buckets.Count;
        var removedCount = 0;

        var keysToRemove = _buckets
            .Where(kvp => kvp.Value.IsExpired(now))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            if (_buckets.TryRemove(key, out _))
            {
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            _logger.LogDebug(
                "Rate limit bucket cleanup: removed {RemovedCount} expired buckets, {RemainingCount} active",
                removedCount,
                _buckets.Count);
        }
    }

    /// <summary>
    /// Disposes the cleanup timer.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _cleanupTimer.Dispose();
    }
}

/// <summary>
/// Rate limit options for configuration.
/// </summary>
public sealed class RateLimitOptions
{
    /// <summary>
    /// Maximum requests allowed per time window.
    /// </summary>
    public int RequestsPerWindow { get; set; } = 100;

    /// <summary>
    /// Time window duration in seconds.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// Additional window for bucket cleanup (multiplier of WindowSeconds).
    /// </summary>
    public int CleanupWindowMultiplier { get; set; } = 2;

    /// <summary>
    /// Interval in seconds for periodic cleanup of expired rate limit buckets.
    /// Prevents memory bloat under heavy traffic. Default: 300 seconds (5 minutes).
    /// </summary>
    public int CleanupIntervalSeconds { get; set; } = 300;
}

/// <summary>
/// Token bucket for rate limiting using sliding window.
/// </summary>
internal sealed class RateLimitBucket
{
    private readonly object _lock = new();
    private readonly int _capacity;
    private readonly TimeSpan _window;
    private readonly TimeSpan _cleanupWindow;
    private int _tokens;
    private DateTimeOffset _windowStart;

    public RateLimitBucket(RateLimitOptions options)
    {
        _capacity = options.RequestsPerWindow;
        _window = TimeSpan.FromSeconds(options.WindowSeconds);
        _cleanupWindow = TimeSpan.FromSeconds(options.WindowSeconds * options.CleanupWindowMultiplier);
        _tokens = _capacity;
        _windowStart = DateTimeOffset.UtcNow;
    }

    public RateLimitResult TryConsume()
    {
        lock (_lock)
        {
            var now = DateTimeOffset.UtcNow;

            // Check if window has expired
            if (now - _windowStart >= _window)
            {
                _tokens = _capacity;
                _windowStart = now;
            }

            var resetTime = _windowStart + _window;

            if (_tokens > 0)
            {
                _tokens--;
                return new RateLimitResult(true, _tokens, resetTime, 0);
            }

            var retryAfter = (int)Math.Ceiling((resetTime - now).TotalSeconds);
            return new RateLimitResult(false, 0, resetTime, Math.Max(1, retryAfter));
        }
    }

    public bool IsExpired(DateTimeOffset now)
    {
        lock (_lock)
        {
            return now - _windowStart >= _cleanupWindow;
        }
    }
}

/// <summary>
/// Result of rate limit check.
/// </summary>
internal readonly record struct RateLimitResult(
    bool IsAllowed,
    int RemainingRequests,
    DateTimeOffset ResetTime,
    int RetryAfterSeconds);

/// <summary>
/// Extension methods for adding rate limiting middleware.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    /// <summary>
    /// Adds rate limiting middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, Action<RateLimitOptions>? configure = null)
    {
        var options = new RateLimitOptions();
        configure?.Invoke(options);

        var logger = app.ApplicationServices.GetRequiredService<ILogger<RateLimitingMiddleware>>();
        return app.UseMiddleware<RateLimitingMiddleware>(logger, options);
    }
}
