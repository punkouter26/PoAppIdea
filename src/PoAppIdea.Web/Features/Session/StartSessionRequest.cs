using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Features.Session;

/// <summary>
/// Request to start a new ideation session.
/// </summary>
public sealed record StartSessionRequest
{
    /// <summary>
    /// Type of app to generate ideas for.
    /// </summary>
    public required AppType AppType { get; init; }

    /// <summary>
    /// Complexity level (1-5).
    /// 1 = simple weekend project, 5 = enterprise-grade.
    /// </summary>
    public required int ComplexityLevel { get; init; }
}
