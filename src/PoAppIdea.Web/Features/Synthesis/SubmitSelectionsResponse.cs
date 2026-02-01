using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Synthesis;

/// <summary>
/// Response DTO for idea submission and synthesis.
/// Returns synthesis result if multiple ideas selected, single idea details if one selected.
/// </summary>
public sealed record SubmitSelectionsResponse
{
    /// <summary>
    /// The session ID.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Number of ideas submitted.
    /// </summary>
    public required int SubmittedCount { get; init; }

    /// <summary>
    /// Whether synthesis was performed (multiple ideas selected).
    /// </summary>
    public required bool SynthesisPerformed { get; init; }

    /// <summary>
    /// The synthesized result (null if only one idea selected).
    /// </summary>
    public SynthesisDto? Synthesis { get; init; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public required string Message { get; init; }
}
