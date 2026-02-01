using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for Session entity domain logic.
/// Focus: State transitions, validation rules.
/// </summary>
public sealed class SessionTests
{
    private static Session CreateValidSession() => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        AppType = AppType.Productivity,
        ComplexityLevel = 3,
        CurrentPhase = SessionPhase.Phase0_Scope,
        Status = SessionStatus.InProgress,
        CreatedAt = DateTimeOffset.UtcNow,
        TopIdeaIds = [],
        SelectedIdeaIds = []
    };

    [Fact]
    public void Session_ShouldInitialize_WithCorrectPhase()
    {
        // Arrange & Act
        var session = CreateValidSession();

        // Assert
        session.CurrentPhase.Should().Be(SessionPhase.Phase0_Scope);
        session.Status.Should().Be(SessionStatus.InProgress);
    }

    [Theory]
    [InlineData(SessionPhase.Phase0_Scope)]
    [InlineData(SessionPhase.Phase1_Spark)]
    [InlineData(SessionPhase.Phase2_Mutation)]
    [InlineData(SessionPhase.Phase3_FeatureExpansion)]
    [InlineData(SessionPhase.Phase4_ProductRefinement)]
    [InlineData(SessionPhase.Phase5_TechnicalRefinement)]
    [InlineData(SessionPhase.Phase6_Visual)]
    [InlineData(SessionPhase.Completed)]
    public void Session_ShouldSupport_AllPhases(SessionPhase phase)
    {
        // Arrange
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AppType = AppType.Mobile,
            ComplexityLevel = 2,
            CurrentPhase = phase,
            Status = SessionStatus.InProgress,
            CreatedAt = DateTimeOffset.UtcNow,
            TopIdeaIds = [],
            SelectedIdeaIds = []
        };

        // Assert
        session.CurrentPhase.Should().Be(phase);
    }

    [Fact]
    public void Session_PhaseTransition_Scope_To_Spark()
    {
        // Arrange
        var session = CreateValidSession();

        // Act - Simulate phase transition
        session.CurrentPhase = SessionPhase.Phase1_Spark;

        // Assert
        session.CurrentPhase.Should().Be(SessionPhase.Phase1_Spark);
    }

    [Fact]
    public void Session_StatusTransition_InProgress_To_Completed()
    {
        // Arrange
        var session = CreateValidSession();
        session.CurrentPhase = SessionPhase.Phase6_Visual;

        // Act
        session.Status = SessionStatus.Completed;
        session.CompletedAt = DateTimeOffset.UtcNow;

        // Assert
        session.Status.Should().Be(SessionStatus.Completed);
        session.CompletedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Session_ComplexityLevel_ShouldBeValid(int complexity)
    {
        // Arrange & Act
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AppType = AppType.Game,
            ComplexityLevel = complexity,
            CurrentPhase = SessionPhase.Phase0_Scope,
            Status = SessionStatus.InProgress,
            CreatedAt = DateTimeOffset.UtcNow,
            TopIdeaIds = [],
            SelectedIdeaIds = []
        };

        // Assert
        session.ComplexityLevel.Should().Be(complexity);
    }

    [Theory]
    [InlineData(AppType.Game)]
    [InlineData(AppType.Productivity)]
    [InlineData(AppType.Mobile)]
    [InlineData(AppType.Automation)]
    public void Session_ShouldSupport_AllAppTypes(AppType appType)
    {
        // Arrange & Act
        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AppType = appType,
            ComplexityLevel = 3,
            CurrentPhase = SessionPhase.Phase0_Scope,
            Status = SessionStatus.InProgress,
            CreatedAt = DateTimeOffset.UtcNow,
            TopIdeaIds = [],
            SelectedIdeaIds = []
        };

        // Assert
        session.AppType.Should().Be(appType);
    }

    [Fact]
    public void Session_TopIdeaIds_ShouldBeUpdateable()
    {
        // Arrange
        var session = CreateValidSession();
        var ideaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act
        session.TopIdeaIds = ideaIds;

        // Assert
        session.TopIdeaIds.Should().HaveCount(3);
    }

    [Fact]
    public void Session_SelectedIdeaIds_ShouldBeUpdateable()
    {
        // Arrange
        var session = CreateValidSession();
        var selectedIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        session.SelectedIdeaIds = selectedIds;

        // Assert
        session.SelectedIdeaIds.Should().HaveCount(2);
    }
}
