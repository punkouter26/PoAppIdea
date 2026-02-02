using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.UnitTests.Entities;

/// <summary>
/// Unit tests for User entity domain logic.
/// Focus: Identity, authentication provider, profile properties.
/// </summary>
public sealed class UserTests
{
    private static User CreateValidUser() => new()
    {
        Id = Guid.NewGuid(),
        ExternalId = "google-oauth2|123456789",
        Provider = AuthProvider.Google,
        Email = "test@example.com",
        DisplayName = "Test User",
        CreatedAt = DateTimeOffset.UtcNow,
        LastLoginAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public void User_ShouldInitialize_WithRequiredProperties()
    {
        // Arrange & Act
        var user = CreateValidUser();

        // Assert
        user.Id.Should().NotBeEmpty();
        user.ExternalId.Should().NotBeNullOrEmpty();
        user.Provider.Should().Be(AuthProvider.Google);
        user.Email.Should().NotBeNullOrEmpty();
        user.DisplayName.Should().NotBeNullOrEmpty();
        user.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        user.LastLoginAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(AuthProvider.Google)]
    [InlineData(AuthProvider.GitHub)]
    [InlineData(AuthProvider.Microsoft)]
    public void User_ShouldSupport_AllAuthProviders(AuthProvider provider)
    {
        // Arrange & Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            ExternalId = $"{provider.ToString().ToLowerInvariant()}|12345",
            Provider = provider,
            Email = $"user@{provider.ToString().ToLowerInvariant()}.com",
            DisplayName = $"{provider} User",
            CreatedAt = DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        // Assert
        user.Provider.Should().Be(provider);
    }

    [Fact]
    public void User_DisplayName_ShouldBeUpdateable()
    {
        // Arrange
        var user = CreateValidUser();
        var newDisplayName = "Updated Display Name";

        // Act
        user.DisplayName = newDisplayName;

        // Assert
        user.DisplayName.Should().Be(newDisplayName);
    }

    [Fact]
    public void User_LastLoginAt_ShouldBeUpdateable()
    {
        // Arrange
        var user = CreateValidUser();
        var originalLogin = user.LastLoginAt;

        // Act
        user.LastLoginAt = DateTimeOffset.UtcNow.AddHours(1);

        // Assert
        user.LastLoginAt.Should().BeAfter(originalLogin);
    }

    [Fact]
    public void User_Email_ShouldBeUpdateable()
    {
        // Arrange
        var user = CreateValidUser();
        var newEmail = "newemail@example.com";

        // Act
        user.Email = newEmail;

        // Assert
        user.Email.Should().Be(newEmail);
    }
}
