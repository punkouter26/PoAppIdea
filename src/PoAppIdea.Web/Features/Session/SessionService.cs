using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;

namespace PoAppIdea.Web.Features.Session;

/// <summary>
/// Service for session lifecycle management.
/// </summary>
public sealed class SessionService(
    ISessionRepository sessionRepository,
    ILogger<SessionService> logger)
{
    /// <summary>
    /// Starts a new ideation session.
    /// </summary>
    public async Task<StartSessionResponse> StartSessionAsync(
        Guid userId,
        StartSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validate complexity level
        if (request.ComplexityLevel < 1 || request.ComplexityLevel > 5)
        {
            throw new ArgumentException("Complexity level must be between 1 and 5", nameof(request));
        }

        var session = new Core.Entities.Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AppType = request.AppType,
            ComplexityLevel = request.ComplexityLevel,
            CurrentPhase = SessionPhase.Phase1_Spark,
            Status = SessionStatus.InProgress,
            TopIdeaIds = [],
            SelectedIdeaIds = [],
            CreatedAt = DateTimeOffset.UtcNow
        };

        await sessionRepository.CreateAsync(session, cancellationToken);

        logger.LogInformation(
            "Started session {SessionId} for user {UserId} with AppType={AppType}, Complexity={Complexity}",
            session.Id, userId, request.AppType, request.ComplexityLevel);

        return new StartSessionResponse
        {
            SessionId = session.Id,
            AppType = session.AppType,
            ComplexityLevel = session.ComplexityLevel,
            CurrentPhase = session.CurrentPhase,
            CreatedAt = session.CreatedAt
        };
    }

    /// <summary>
    /// Gets a session by ID.
    /// </summary>
    public async Task<Core.Entities.Session?> GetSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await sessionRepository.GetByIdAsync(sessionId, cancellationToken);
    }

    /// <summary>
    /// Gets a session for resumption (FR-024).
    /// </summary>
    public async Task<Core.Entities.Session?> GetSessionForResumptionAsync(
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await sessionRepository.GetForResumptionAsync(sessionId, userId, cancellationToken);
    }

    /// <summary>
    /// Gets all sessions for a user.
    /// </summary>
    public async Task<IReadOnlyList<Core.Entities.Session>> GetUserSessionsAsync(
        Guid userId,
        SessionStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var sessions = await sessionRepository.GetByUserIdAsync(userId, cancellationToken);

        if (status.HasValue)
        {
            sessions = sessions.Where(s => s.Status == status.Value).ToList();
        }

        return sessions.OrderByDescending(s => s.CreatedAt).ToList();
    }

    /// <summary>
    /// Updates the session phase.
    /// </summary>
    public async Task<Core.Entities.Session> UpdatePhaseAsync(
        Guid sessionId,
        SessionPhase newPhase,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        session.CurrentPhase = newPhase;

        await sessionRepository.UpdateAsync(session, cancellationToken);

        logger.LogInformation("Session {SessionId} transitioned to phase {Phase}", sessionId, newPhase);

        return session;
    }

    /// <summary>
    /// Updates the top idea IDs for a session.
    /// </summary>
    public async Task UpdateTopIdeasAsync(
        Guid sessionId,
        List<Guid> topIdeaIds,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        session.TopIdeaIds = topIdeaIds;

        await sessionRepository.UpdateAsync(session, cancellationToken);
    }

    /// <summary>
    /// Completes a session.
    /// </summary>
    public async Task CompleteSessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        session.Status = SessionStatus.Completed;
        session.CurrentPhase = SessionPhase.Completed;
        session.CompletedAt = DateTimeOffset.UtcNow;

        await sessionRepository.UpdateAsync(session, cancellationToken);

        logger.LogInformation("Session {SessionId} completed", sessionId);
    }
}
