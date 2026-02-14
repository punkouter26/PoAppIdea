using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Cache for AI responses keyed by prompt hash. Prevents redundant API calls
/// for identical or near-identical prompts within a configurable TTL.
/// Saves significant tokens when users retry or sessions have similar parameters.
/// Pattern: Decorator/Proxy — wraps AI calls with prompt-hash → response caching.
/// </summary>
public sealed class AiResponseCache
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<AiResponseCache> _logger;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(15);

    public AiResponseCache(IMemoryCache cache, ILogger<AiResponseCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets a cached AI response or executes the factory to generate one.
    /// Cache key is the SHA256 hash of the combined prompt text.
    /// </summary>
    public async Task<string> GetOrGenerateAsync(
        string cacheCategory,
        string promptText,
        Func<Task<string>> aiCallFactory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(cacheCategory, promptText);

        if (_cache.TryGetValue(cacheKey, out string? cachedResponse) && cachedResponse is not null)
        {
            _logger.LogDebug("AI response cache HIT for category '{Category}' (key: {Key})",
                cacheCategory, cacheKey[..16]);
            return cachedResponse;
        }

        _logger.LogDebug("AI response cache MISS for category '{Category}' (key: {Key})",
            cacheCategory, cacheKey[..16]);

        cancellationToken.ThrowIfCancellationRequested();
        var response = await aiCallFactory();

        if (!string.IsNullOrWhiteSpace(response))
        {
            var options = new MemoryCacheEntryOptions
            {
                SlidingExpiration = ttl ?? DefaultTtl,
                AbsoluteExpirationRelativeToNow = (ttl ?? DefaultTtl) * 3,
                Size = response.Length // For memory pressure tracking
            };

            _cache.Set(cacheKey, response, options);
            _logger.LogDebug("Cached AI response for category '{Category}' ({Length} chars, TTL: {Ttl})",
                cacheCategory, response.Length, ttl ?? DefaultTtl);
        }

        return response;
    }

    /// <summary>
    /// Builds a deterministic cache key from the prompt category and text.
    /// Uses SHA256 to keep keys short while avoiding collisions.
    /// </summary>
    private static string BuildCacheKey(string category, string promptText)
    {
        var input = $"{category}:{promptText}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var hash = Convert.ToHexStringLower(hashBytes);
        return $"ai:{category}:{hash}";
    }
}
