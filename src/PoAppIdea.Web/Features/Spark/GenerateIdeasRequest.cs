namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Request DTO for generating a batch of ideas.
/// </summary>
public sealed class GenerateIdeasRequest
{
    /// <summary>
    /// Number of ideas to generate in this batch.
    /// Default is 10 per FR-002.
    /// </summary>
    public int BatchSize { get; init; } = 10;

    /// <summary>
    /// Optional seed phrase to influence idea generation.
    /// </summary>
    public string? SeedPhrase { get; init; }
}
