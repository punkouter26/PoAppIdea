using System.ComponentModel.DataAnnotations;

namespace PoAppIdea.Web.Features.Mutation;

/// <summary>
/// Request to rate a mutation.
/// </summary>
public sealed record RateMutationRequest
{
    /// <summary>
    /// User rating score (1-5 scale).
    /// </summary>
    [Required]
    [Range(1, 5)]
    public required float Score { get; init; }

    /// <summary>
    /// Optional feedback text about the mutation.
    /// </summary>
    [MaxLength(500)]
    public string? Feedback { get; init; }
}
