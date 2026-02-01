namespace PoAppIdea.Core.Enums;

/// <summary>
/// Direction of a swipe action.
/// </summary>
public enum SwipeDirection
{
    /// <summary>
    /// Swipe left - discard/dislike the idea.
    /// </summary>
    Left,

    /// <summary>
    /// Swipe right - like/keep the idea.
    /// </summary>
    Right,

    /// <summary>
    /// Swipe up - super-like the idea (extra weighting).
    /// </summary>
    Up
}
