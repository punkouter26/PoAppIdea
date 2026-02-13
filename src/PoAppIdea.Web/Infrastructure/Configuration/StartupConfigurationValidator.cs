namespace PoAppIdea.Web.Infrastructure.Configuration;

/// <summary>
/// Validates required runtime configuration and fails fast for invalid production settings.
/// </summary>
public static class StartupConfigurationValidator
{
    private static readonly string[] PlaceholderTokens =
    {
        "your-",
        "<your",
        "changeme",
        "todo",
        "example"
    };

    public static void ValidateOrThrow(IConfiguration configuration, IHostEnvironment environment)
    {
        var useMockAI = configuration.GetValue<bool>("MockAI") ||
                        Environment.GetEnvironmentVariable("MOCK_AI")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

        if (!useMockAI)
        {
            var requiredAiKeys = new[]
            {
                "AzureOpenAI:Endpoint",
                "AzureOpenAI:ApiKey"
            };

            var invalidAiKeys = requiredAiKeys
                .Where(key => IsMissingOrPlaceholder(configuration[key]))
                .ToList();

            if (invalidAiKeys.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Invalid AI configuration. Missing/placeholder values: {string.Join(", ", invalidAiKeys)}");
            }
        }

        if (environment.IsDevelopment())
        {
            return;
        }

        var requiredKeys = new[]
        {
            "AzureOpenAI:Endpoint",
            "AzureOpenAI:ApiKey",
            "PoAppIdea:Authentication:Google:ClientId",
            "PoAppIdea:Authentication:Google:ClientSecret",
            "PoAppIdea:Authentication:Microsoft:ClientId",
            "PoAppIdea:Authentication:Microsoft:ClientSecret"
        };

        var invalidKeys = requiredKeys
            .Where(key => IsMissingOrPlaceholder(configuration[key]))
            .ToList();

        if (invalidKeys.Count > 0)
        {
            throw new InvalidOperationException(
                $"Invalid runtime configuration. Missing/placeholder values: {string.Join(", ", invalidKeys)}");
        }
    }

    private static bool IsMissingOrPlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return PlaceholderTokens.Any(token =>
            value.Contains(token, StringComparison.OrdinalIgnoreCase));
    }
}
