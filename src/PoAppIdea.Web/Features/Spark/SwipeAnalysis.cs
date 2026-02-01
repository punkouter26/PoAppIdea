namespace PoAppIdea.Web.Features.Spark;

/// <summary>
/// Analysis of swipe patterns in a session.
/// Used to detect edge cases (all likes, all dislikes) and offer appropriate UI guidance.
/// </summary>
public sealed class SwipeAnalysis
{
    /// <summary>
    /// Total number of swipes recorded.
    /// </summary>
    public int TotalSwipes { get; init; }

    /// <summary>
    /// Count of right swipes (likes).
    /// </summary>
    public int LikeCount { get; init; }

    /// <summary>
    /// Count of left swipes (dislikes).
    /// </summary>
    public int DislikeCount { get; init; }

    /// <summary>
    /// Count of up swipes (super-likes).
    /// </summary>
    public int SuperLikeCount { get; init; }

    /// <summary>
    /// True if all swiped ideas were liked (no dislikes).
    /// In this case, swipe speed is used for ranking differentiation.
    /// </summary>
    public bool AllLikes { get; init; }

    /// <summary>
    /// True if all swiped ideas were disliked (no likes or super-likes).
    /// In this case, the user may want to restart with different parameters.
    /// </summary>
    public bool AllDislikes { get; init; }

    /// <summary>
    /// True if the UI should offer a restart option.
    /// Triggered when all ideas are disliked and enough swipes have been recorded.
    /// </summary>
    public bool ShouldOfferRestart { get; init; }

    /// <summary>
    /// Suggested message to display to the user based on swipe patterns.
    /// </summary>
    public string? SuggestedMessage => (AllLikes, AllDislikes) switch
    {
        (_, true) => "None of these ideas resonated with you. Would you like to restart with different parameters or try a different app type?",
        (true, _) => "You liked all the ideas! We'll use your swipe timing to help rank which ones excited you most.",
        _ => null
    };
}
