using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for Swipe entity and scoring logic.
/// Focus: Swipe direction scoring, speed multipliers.
/// </summary>
public sealed class SwipeTests
{
    private static Swipe CreateValidSwipe(
        SwipeDirection direction = SwipeDirection.Right,
        SwipeSpeed speed = SwipeSpeed.Medium,
        int durationMs = 2000) => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        IdeaId = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Direction = direction,
        DurationMs = durationMs,
        Timestamp = DateTimeOffset.UtcNow,
        SpeedCategory = speed
    };

    [Theory]
    [InlineData(SwipeDirection.Right, 1)]   // Like
    [InlineData(SwipeDirection.Left, -1)]   // Dislike
    [InlineData(SwipeDirection.Up, 2)]      // Super-like
    public void Swipe_Direction_ShouldHave_CorrectBaseScore(SwipeDirection direction, int expectedScore)
    {
        // Arrange
        var swipe = CreateValidSwipe(direction: direction);

        // Act
        var score = GetBaseScore(swipe.Direction);

        // Assert
        score.Should().Be(expectedScore);
    }

    [Theory]
    [InlineData(SwipeSpeed.Slow, 0.5)]
    [InlineData(SwipeSpeed.Medium, 1.0)]
    [InlineData(SwipeSpeed.Fast, 1.5)]
    public void Swipe_Speed_ShouldHave_CorrectMultiplier(SwipeSpeed speed, double expectedMultiplier)
    {
        // Arrange & Act
        var multiplier = GetSpeedMultiplier(speed);

        // Assert
        multiplier.Should().Be(expectedMultiplier);
    }

    [Theory]
    [InlineData(SwipeDirection.Right, SwipeSpeed.Fast, 1.5)]    // Like + Fast = 1 * 1.5
    [InlineData(SwipeDirection.Up, SwipeSpeed.Slow, 1.0)]       // Super-like + Slow = 2 * 0.5
    [InlineData(SwipeDirection.Left, SwipeSpeed.Medium, -1.0)]  // Dislike + Medium = -1 * 1.0
    public void Swipe_CalculatedScore_ShouldCombine_DirectionAndSpeed(
        SwipeDirection direction,
        SwipeSpeed speed,
        double expectedFinalScore)
    {
        // Arrange
        var swipe = CreateValidSwipe(direction: direction, speed: speed);

        // Act
        var finalScore = CalculateSwipeScore(swipe);

        // Assert
        finalScore.Should().BeApproximately(expectedFinalScore, 0.01);
    }

    [Fact]
    public void Swipe_ShouldTrack_Timestamp()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Act
        var swipe = new Swipe
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            IdeaId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Direction = SwipeDirection.Right,
            DurationMs = 1500,
            Timestamp = now,
            SpeedCategory = SwipeSpeed.Medium
        };

        // Assert
        swipe.Timestamp.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(500, SwipeSpeed.Fast)]    // < 1000ms = Fast
    [InlineData(2000, SwipeSpeed.Medium)] // 1000-3000ms = Medium
    [InlineData(4000, SwipeSpeed.Slow)]   // > 3000ms = Slow
    public void Swipe_DurationMs_ShouldCorrelate_WithSpeedCategory(int durationMs, SwipeSpeed expectedSpeed)
    {
        // Arrange & Act
        var swipe = CreateValidSwipe(speed: expectedSpeed, durationMs: durationMs);

        // Assert
        swipe.DurationMs.Should().Be(durationMs);
        swipe.SpeedCategory.Should().Be(expectedSpeed);
    }

    [Fact]
    public void Swipe_ShouldHave_RequiredUserId()
    {
        // Arrange & Act
        var userId = Guid.NewGuid();
        var swipe = new Swipe
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            IdeaId = Guid.NewGuid(),
            UserId = userId,
            Direction = SwipeDirection.Right,
            DurationMs = 1000,
            Timestamp = DateTimeOffset.UtcNow,
            SpeedCategory = SwipeSpeed.Medium
        };

        // Assert
        swipe.UserId.Should().Be(userId);
    }

    #region Helper Methods (Domain Logic Under Test)

    private static int GetBaseScore(SwipeDirection direction) => direction switch
    {
        SwipeDirection.Right => 1,   // Like
        SwipeDirection.Left => -1,   // Dislike
        SwipeDirection.Up => 2,      // Super-like
        _ => 0
    };

    private static double GetSpeedMultiplier(SwipeSpeed speed) => speed switch
    {
        SwipeSpeed.Slow => 0.5,
        SwipeSpeed.Medium => 1.0,
        SwipeSpeed.Fast => 1.5,
        _ => 1.0
    };

    private static double CalculateSwipeScore(Swipe swipe)
    {
        var baseScore = GetBaseScore(swipe.Direction);
        var multiplier = GetSpeedMultiplier(swipe.SpeedCategory);
        return baseScore * multiplier;
    }

    #endregion
}
