using PoAppIdea.Core.Enums;

namespace PoAppIdea.Core.Entities;

/// <summary>
/// Represents an authenticated user of the platform.
/// </summary>
public sealed class User
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// OAuth provider user ID (e.g., Google sub).
    /// </summary>
    public required string ExternalId { get; init; }

    /// <summary>
    /// Authentication provider.
    /// </summary>
    public required AuthProvider Provider { get; init; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Display name for gallery (1-100 chars).
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Account creation timestamp.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Last authentication timestamp.
    /// </summary>
    public required DateTimeOffset LastLoginAt { get; set; }
}
