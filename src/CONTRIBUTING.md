# Contributing to PoAppIdea

Thank you for your interest in contributing to PoAppIdea! This document provides guidelines and instructions for contributing.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Code Standards](#code-standards)
- [Architecture Guidelines](#architecture-guidelines)
- [Testing Requirements](#testing-requirements)
- [Pull Request Process](#pull-request-process)

## 📜 Code of Conduct

Be respectful, inclusive, and professional. We're all here to build something great together.

## 🚀 Getting Started

### Prerequisites

1. **.NET 10 SDK** - [Download](https://dotnet.microsoft.com/download)
2. **Visual Studio Code** or **Visual Studio 2022** (17.10+)
3. **Azure Storage Emulator** (Azurite)
4. **Node.js 18+** (for E2E tests)
5. **Git**

### Initial Setup

```bash
# Clone the repository
git clone <repository-url>
cd PoAppIdea

# Restore dependencies
dotnet restore

# Set up user secrets
cd src/PoAppIdea.Web
dotnet user-secrets init
# Add your secrets (see README.md)

# Run the app
dotnet run
```

### Recommended VS Code Extensions

- C# Dev Kit
- GitHub Copilot
- REST Client (for .http files)
- Playwright Test for VSCode

## 🔄 Development Workflow

### Branch Naming

```
feature/US<N>-short-description    # New user story
bugfix/issue-<N>-short-description # Bug fixes
hotfix/critical-fix                # Production fixes
chore/maintenance-task             # Non-feature work
```

### Commit Messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
feat(session): add resume session endpoint
fix(spark): handle empty idea batch gracefully
docs(readme): update deployment instructions
test(mutation): add integration tests for combine
refactor(synthesis): extract scoring logic
```

### Development Cycle

1. Create a feature branch from `main`
2. Make changes following our code standards
3. Write/update tests
4. Run all tests locally
5. Create a pull request
6. Address review feedback
7. Merge after approval

## 💻 Code Standards

### General Principles

- **SOLID Principles**: Single responsibility, dependency injection
- **Clean Code**: Self-documenting names, small functions
- **GoF Patterns**: Document pattern usage with comments
- **Zero Warnings**: All warnings are treated as errors

### C# Conventions

```csharp
// ✅ Good: Descriptive names, nullable annotations
public async Task<Session?> GetSessionByIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(sessionId);
    // Implementation
}

// ✅ Good: Pattern usage documented
/// <summary>
/// Repository Pattern: Abstracts data access for Session entities.
/// This allows swapping Azure Table Storage for another provider.
/// </summary>
public class SessionRepository : ISessionRepository
```

### File Organization

```
Features/
├── {FeatureName}/
│   ├── {Action}Request.cs      # Request DTO
│   ├── {Action}Response.cs     # Response DTO
│   ├── {FeatureName}Service.cs # Business logic
│   └── {Action}Endpoint.cs     # Minimal API endpoint
```

### Blazor Components

```razor
@* Prefer component parameters over cascading values *@
@* Use EventCallback for parent communication *@
@* Keep code-behind minimal, delegate to services *@

<RadzenCard>
    <ChildContent>
        <!-- Semantic HTML structure -->
    </ChildContent>
</RadzenCard>

@code {
    [Parameter, EditorRequired]
    public required Guid SessionId { get; set; }
    
    [Parameter]
    public EventCallback<SwipeResult> OnSwipe { get; set; }
    
    [Inject]
    private SparkService SparkService { get; set; } = default!;
}
```

## 🏗️ Architecture Guidelines

### Vertical Slice Architecture

Each feature is self-contained:
- DTOs, Service, and Endpoints together
- Minimize cross-feature dependencies
- Shared infrastructure in `Infrastructure/`

### Dependency Injection

- Register services in `Program.cs`
- Use `Scoped` for request-level services
- Use `Singleton` for stateless utilities

### Storage Patterns

```csharp
// All storage access through repository interfaces
public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Session> CreateAsync(Session session, CancellationToken cancellationToken = default);
    Task<Session> UpdateAsync(Session session, CancellationToken cancellationToken = default);
}
```

### Error Handling

Use Problem Details for API errors:

```csharp
return Results.Problem(
    title: "Session not found",
    detail: $"Session with ID {sessionId} does not exist",
    statusCode: StatusCodes.Status404NotFound
);
```

## 🧪 Testing Requirements

### Test Pyramid

1. **Unit Tests** (Fast, isolated)
   - Pure logic and domain rules
   - Mock all dependencies
   - Target: 80%+ code coverage

2. **Integration Tests** (Database, API)
   - Use Testcontainers for real storage
   - Test complete request flows
   - Focus on happy paths + critical errors

3. **E2E Tests** (Browser, user flows)
   - Critical user journeys only
   - Chromium + Mobile viewports
   - Run headed during development

### Test Naming

```csharp
// Unit tests
public class SessionServiceTests
{
    [Fact]
    public async Task CreateSession_WithValidInput_ReturnsNewSession()
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateSession_WithInvalidComplexity_ThrowsArgumentException(int complexity)
}

// Integration tests
public class SessionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task POST_Sessions_Returns201WithLocation()
}
```

### Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/PoAppIdea.UnitTests

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# E2E tests
cd tests/PoAppIdea.E2E
npm test
```

## 🔍 Pull Request Process

### Before Submitting

- [ ] All tests pass locally
- [ ] No new warnings (build with `TreatWarningsAsErrors`)
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated if needed

### PR Description Template

```markdown
## Summary
Brief description of changes

## Type
- [ ] Feature
- [ ] Bug fix
- [ ] Documentation
- [ ] Refactoring

## Related Issues
Closes #123

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Screenshots (if UI changes)
```

### Review Checklist

Reviewers will check:
- Correctness and completeness
- Test coverage
- Performance implications
- Security considerations
- Code quality and readability

### Merge Requirements

- At least 1 approval
- All CI checks passing
- No unresolved conversations
- Up-to-date with `main`

## 📞 Getting Help

- Check existing issues and discussions
- Create a new issue with reproduction steps
- Ask in team channels

Thank you for contributing! 🎉
