using PoAppIdea.Shared.Contracts;

namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Response DTO containing a batch of generated ideas.
/// </summary>
public sealed class GenerateIdeasResponse
{
    /// <summary>
    /// The session these ideas belong to.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// The batch number (1-indexed).
    /// </summary>
    public required int BatchNumber { get; init; }

    /// <summary>
    /// The generated ideas for this batch.
    /// </summary>
    public required IReadOnlyList<IdeaDto> Ideas { get; init; }

    /// <summary>
    /// Indicates if more ideas can be generated.
    /// </summary>
    public required bool HasMore { get; init; }
}
