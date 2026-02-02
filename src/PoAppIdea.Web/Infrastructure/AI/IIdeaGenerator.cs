using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Interface for idea generation services.
/// Pattern: Strategy Pattern - Allows swapping between real AI and mock implementations.
/// </summary>
public interface IIdeaGenerator
{
    /// <summary>
    /// Generates initial batch of ideas based on app type and complexity.
    /// </summary>
    Task<IReadOnlyList<Idea>> GenerateInitialIdeasAsync(
        Guid sessionId,
        AppType appType,
        int complexityLevel,
        ProductPersonality? personality,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates mutated ideas based on swiped ideas using crossover or repurposing.
    /// </summary>
    Task<IReadOnlyList<Idea>> GenerateMutatedIdeasAsync(
        Guid sessionId,
        IReadOnlyList<Idea> likedIdeas,
        IReadOnlyList<Idea> dislikedIdeas,
        int batchNumber,
        MutationType mutationType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates synthesized idea from top candidates.
    /// </summary>
    Task<Synthesis> GenerateSynthesisAsync(
        Guid sessionId,
        IReadOnlyList<Idea> topIdeas,
        CancellationToken cancellationToken = default);
}
