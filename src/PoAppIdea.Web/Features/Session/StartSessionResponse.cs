using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Session;

/// <summary>
/// Response after starting a new session.
/// </summary>
public sealed record StartSessionResponse
{
    /// <summary>
    /// The created session ID.
    /// </summary>
    public required Guid SessionId { get; init; }

    /// <summary>
    /// Selected app type.
    /// </summary>
    public required AppType AppType { get; init; }

    /// <summary>
    /// Selected complexity level.
    /// </summary>
    public required int ComplexityLevel { get; init; }

    /// <summary>
    /// Current session phase.
    /// </summary>
    public required SessionPhase CurrentPhase { get; init; }

    /// <summary>
    /// Session creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }
}
