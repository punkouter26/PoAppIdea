using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace PoAppIdea.Web.Infrastructure;

/// <summary>
/// Helper for consistent user ID handling across OAuth providers.
/// Converts external provider IDs (Google, GitHub, Microsoft) to deterministic GUIDs.
/// Pattern: Utility class per GoF - centralizes user ID conversion logic.
/// </summary>
public static class UserIdHelper
{
    /// <summary>
    /// Gets or creates a deterministic GUID from a user's claims.
    /// </summary>
    /// <param name="user">The claims principal from authentication.</param>
    /// <returns>A GUID representing the user, or null if not authenticated.</returns>
    public static Guid? GetUserId(ClaimsPrincipal? user)
    {
        var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }

        return ParseOrCreateGuid(userIdClaim);
    }

    /// <summary>
    /// Gets or creates a deterministic GUID from a user ID string.
    /// If the string is already a valid GUID, it's returned as-is.
    /// Otherwise, a deterministic GUID is created using MD5 hash.
    /// </summary>
    /// <param name="userIdClaim">The user ID claim value (GUID or external provider ID).</param>
    /// <returns>A deterministic GUID for the user.</returns>
    public static Guid ParseOrCreateGuid(string userIdClaim)
    {
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return CreateDeterministicGuid(userIdClaim);
    }

    /// <summary>
    /// Creates a deterministic GUID from any string input using MD5 hash.
    /// Used for external OAuth provider IDs (Google, GitHub, Microsoft).
    /// </summary>
    /// <param name="input">The input string to convert.</param>
    /// <returns>A deterministic GUID based on the input.</returns>
    public static Guid CreateDeterministicGuid(string input)
    {
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
