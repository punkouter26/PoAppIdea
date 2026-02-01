using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for RefinementAnswer entity domain logic.
/// Focus: Pure logic validation, no external dependencies.
/// Phase 7 tests per User Story 5 - Deep Refinement via Interactive Inquiry.
/// </summary>
public sealed class RefinementAnswerTests
{
    private static RefinementAnswer CreateValidAnswer(
        RefinementPhase phase = RefinementPhase.Phase4_PM,
        int questionNumber = 1) => new()
    {
        Id = Guid.NewGuid(),
        SessionId = Guid.NewGuid(),
        Phase = phase,
        QuestionNumber = questionNumber,
        QuestionText = "Who is your primary target user?",
        QuestionCategory = "UserPersonas",
        AnswerText = "Tech-savvy millennials aged 25-35 who need productivity tools.",
        Timestamp = DateTimeOffset.UtcNow
    };

    #region Entity Initialization Tests

    [Fact]
    public void RefinementAnswer_ShouldInitialize_WithValidDefaults()
    {
        // Arrange & Act
        var answer = CreateValidAnswer();

        // Assert
        answer.Id.Should().NotBeEmpty();
        answer.SessionId.Should().NotBeEmpty();
        answer.Phase.Should().Be(RefinementPhase.Phase4_PM);
        answer.QuestionNumber.Should().Be(1);
        answer.QuestionText.Should().NotBeNullOrWhiteSpace();
        answer.QuestionCategory.Should().NotBeNullOrWhiteSpace();
        answer.AnswerText.Should().NotBeNullOrWhiteSpace();
        answer.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void RefinementAnswer_ShouldSupport_PMPhase()
    {
        // Arrange & Act
        var answer = CreateValidAnswer(RefinementPhase.Phase4_PM);

        // Assert
        answer.Phase.Should().Be(RefinementPhase.Phase4_PM);
    }

    [Fact]
    public void RefinementAnswer_ShouldSupport_ArchitectPhase()
    {
        // Arrange & Act
        var answer = CreateValidAnswer(RefinementPhase.Phase5_Architect);

        // Assert
        answer.Phase.Should().Be(RefinementPhase.Phase5_Architect);
    }

    #endregion

    #region QuestionNumber Tests

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void RefinementAnswer_QuestionNumber_ShouldBeInValidRange(int questionNumber)
    {
        // Arrange & Act
        var answer = CreateValidAnswer(questionNumber: questionNumber);

        // Assert - Valid range per spec: 1-10
        answer.QuestionNumber.Should().BeGreaterThanOrEqualTo(1);
        answer.QuestionNumber.Should().BeLessThanOrEqualTo(10);
    }

    #endregion

    #region AnswerText Tests

    [Fact]
    public void RefinementAnswer_AnswerText_ShouldSupportLongAnswers()
    {
        // Arrange
        var longAnswer = new string('A', 2000); // Max per spec

        // Act
        var answer = CreateValidAnswer();
        answer.AnswerText = longAnswer;

        // Assert
        answer.AnswerText.Length.Should().Be(2000);
    }

    [Theory]
    [InlineData("Short answer")]
    [InlineData("A moderately detailed answer explaining the user base and their needs.")]
    public void RefinementAnswer_AnswerText_ShouldAcceptVariousLengths(string text)
    {
        // Arrange & Act
        var answer = CreateValidAnswer();
        answer.AnswerText = text;

        // Assert
        answer.AnswerText.Should().Be(text);
    }

    #endregion

    #region QuestionCategory Tests

    [Theory]
    [InlineData("UserPersonas")]
    [InlineData("MarketPosition")]
    [InlineData("BusinessModel")]
    [InlineData("Architecture")]
    [InlineData("Security")]
    [InlineData("Scalability")]
    public void RefinementAnswer_QuestionCategory_ShouldSupportAllCategories(string category)
    {
        // Arrange & Act
        var answer = new RefinementAnswer
        {
            Id = Guid.NewGuid(),
            SessionId = Guid.NewGuid(),
            Phase = RefinementPhase.Phase4_PM,
            QuestionNumber = 1,
            QuestionText = "Test question",
            QuestionCategory = category,
            AnswerText = "Test answer",
            Timestamp = DateTimeOffset.UtcNow
        };

        // Assert
        answer.QuestionCategory.Should().Be(category);
    }

    #endregion

    #region Phase Transition Tests

    [Fact]
    public void RefinementPhase_ShouldHave_TwoPhases()
    {
        // Assert - Per spec: Phase4_PM (10 questions) + Phase5_Architect (10 questions)
        var phases = Enum.GetValues<RefinementPhase>();
        phases.Should().HaveCount(2);
        phases.Should().Contain(RefinementPhase.Phase4_PM);
        phases.Should().Contain(RefinementPhase.Phase5_Architect);
    }

    [Fact]
    public void RefinementAnswer_ShouldTrack_Timestamp()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow.AddSeconds(-1);
        
        // Act
        var answer = CreateValidAnswer();
        var after = DateTimeOffset.UtcNow.AddSeconds(1);

        // Assert
        answer.Timestamp.Should().BeAfter(before);
        answer.Timestamp.Should().BeBefore(after);
    }

    #endregion

    #region Session Association Tests

    [Fact]
    public void RefinementAnswer_MultipleAnswers_ShouldShareSessionId()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        
        // Act
        var answers = Enumerable.Range(1, 10)
            .Select(i => new RefinementAnswer
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Phase = RefinementPhase.Phase4_PM,
                QuestionNumber = i,
                QuestionText = $"Question {i}",
                QuestionCategory = "General",
                AnswerText = $"Answer {i}",
                Timestamp = DateTimeOffset.UtcNow
            })
            .ToList();

        // Assert - All answers should belong to the same session
        answers.Should().AllSatisfy(a => a.SessionId.Should().Be(sessionId));
        answers.Should().HaveCount(10);
    }

    [Fact]
    public void RefinementAnswer_ShouldHave_UniqueIds()
    {
        // Arrange & Act
        var answer1 = CreateValidAnswer();
        var answer2 = CreateValidAnswer();

        // Assert
        answer1.Id.Should().NotBe(answer2.Id);
    }

    #endregion
}
