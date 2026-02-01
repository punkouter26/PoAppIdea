using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;
using PoAppIdea.Core.Interfaces;

using SessionEntity = PoAppIdea.Core.Entities.Session;
using SynthesisEntity = PoAppIdea.Core.Entities.Synthesis;

namespace PoAppIdea.Web.Features.Refinement;

/// <summary>
/// Service for managing refinement questions and answers.
/// Implements Chain of Responsibility pattern for phase progression.
/// </summary>
public sealed class RefinementService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefinementAnswerRepository _answerRepository;
    private readonly ISynthesisRepository _synthesisRepository;
    private readonly ILogger<RefinementService> _logger;

    /// <summary>
    /// PM phase questions - Product Manager focused questions about user needs,
    /// market fit, business model, and product strategy.
    /// </summary>
    private static readonly IReadOnlyList<(string Category, string Question, string Example)> PmQuestions =
    [
        ("UserPersonas", "Who is your primary target user? Describe their demographics, needs, and pain points.",
            "Tech-savvy millennials aged 25-35 who struggle with productivity and need a simple, mobile-first task manager."),
        ("UserPersonas", "What specific problem are you solving for these users?",
            "Users waste 30+ minutes daily switching between multiple tools. We unify their workflow in one app."),
        ("MarketPosition", "Who are your main competitors and how will you differentiate?",
            "Competitors like Todoist focus on features. We focus on AI-assisted task prioritization and minimal UX."),
        ("MarketPosition", "What is your unique value proposition in one sentence?",
            "The only task manager that learns your work style and automatically schedules your day."),
        ("BusinessModel", "How will this product generate revenue?",
            "Freemium model: free basic tier, $9.99/month premium with AI features and unlimited integrations."),
        ("BusinessModel", "What are the key metrics you'll track to measure success?",
            "DAU, task completion rate, time-to-value (first task created), and monthly churn rate."),
        ("UserExperience", "Describe the ideal first-time user experience (first 5 minutes).",
            "Sign up with Google, import existing tasks from email, see AI-suggested priorities within 2 minutes."),
        ("UserExperience", "What are the top 3 features users will spend the most time with?",
            "1) Smart daily planner view, 2) Quick-add task capture, 3) Team collaboration inbox."),
        ("GoToMarket", "What is your launch strategy?",
            "Beta with 500 ProductHunt early adopters, iterate for 3 months, then public launch with influencer partnerships."),
        ("RiskMitigation", "What is the biggest risk to this product's success and how will you mitigate it?",
            "Risk: Users already have task apps. Mitigation: Focus on migration tools and immediate value demonstration.")
    ];

    /// <summary>
    /// Architect phase questions - Technical architecture, scalability,
    /// security, and implementation strategy.
    /// </summary>
    private static readonly IReadOnlyList<(string Category, string Question, string Example)> ArchitectQuestions =
    [
        ("Architecture", "What is the preferred cloud platform and why?",
            "Azure for seamless .NET integration, managed services (App Service, Cosmos DB), and enterprise compliance."),
        ("Architecture", "Describe the high-level system architecture (frontend, backend, data layer).",
            "Blazor WASM frontend, .NET 8 API backend on Azure App Service, Cosmos DB for data, Azure CDN for assets."),
        ("DataStrategy", "How will you handle data persistence and what database technology fits best?",
            "Azure Cosmos DB for global distribution and flexible schema, Azure Table Storage for session telemetry."),
        ("DataStrategy", "What is your data backup and disaster recovery strategy?",
            "Cosmos DB automatic backups with 30-day retention, geo-redundant storage, RPO < 1 hour."),
        ("Security", "How will you handle authentication and authorization?",
            "Azure Entra ID with OAuth 2.0, role-based access control, JWT tokens with 1-hour expiry and refresh tokens."),
        ("Security", "What sensitive data will you store and how will you protect it?",
            "User emails and task content. Encryption at rest (AES-256), in transit (TLS 1.3), and tokenized PII."),
        ("Integration", "What third-party services or APIs will you integrate with?",
            "Google Calendar, Microsoft 365, Slack, Zapier webhooks, and Stripe for payments."),
        ("Integration", "How will you handle API versioning and breaking changes?",
            "URL-based versioning (v1, v2), 6-month deprecation notices, and backward compatibility contracts."),
        ("Scalability", "What is your expected scale (users, requests per second, data volume)?",
            "Launch: 10K users, 100 RPS. Year 1: 500K users, 5K RPS, 10TB data. Horizontal scaling via Azure Autoscale."),
        ("Deployment", "What is your CI/CD and deployment strategy?",
            "GitHub Actions for CI, Azure DevOps for CD, blue-green deployments, feature flags for gradual rollout.")
    ];

    public RefinementService(
        ISessionRepository sessionRepository,
        IRefinementAnswerRepository answerRepository,
        ISynthesisRepository synthesisRepository,
        ILogger<RefinementService> logger)
    {
        _sessionRepository = sessionRepository;
        _answerRepository = answerRepository;
        _synthesisRepository = synthesisRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets refinement questions for the current or specified phase.
    /// </summary>
    public async Task<GetQuestionsResponse> GetQuestionsAsync(
        Guid sessionId,
        RefinementPhase? requestedPhase = null,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        // Determine which phase to return questions for
        var phase = DeterminePhase(session, requestedPhase);

        // Get existing answers for this phase
        var existingAnswers = await _answerRepository.GetBySessionAndPhaseAsync(
            sessionId, phase, cancellationToken);
        var answeredNumbers = existingAnswers.ToDictionary(a => a.QuestionNumber, a => a.AnswerText);

        // Get question templates for the phase
        var questionTemplates = phase == RefinementPhase.Phase4_PM ? PmQuestions : ArchitectQuestions;

        var questions = questionTemplates.Select((q, index) =>
        {
            var questionNumber = index + 1;
            var isAnswered = answeredNumbers.ContainsKey(questionNumber);
            return new RefinementQuestionDto
            {
                QuestionNumber = questionNumber,
                QuestionText = q.Question,
                Category = q.Category,
                ExampleAnswer = q.Example,
                IsAnswered = isAnswered,
                ExistingAnswer = isAnswered ? answeredNumbers[questionNumber] : null
            };
        }).ToList();

        _logger.LogInformation(
            "Session {SessionId}: Returning {Count} questions for {Phase} phase ({Answered} already answered)",
            sessionId, questions.Count, phase, existingAnswers.Count);

        return new GetQuestionsResponse
        {
            Phase = phase,
            PhaseDisplayName = phase == RefinementPhase.Phase4_PM ? "Product Manager" : "Technical Architect",
            Questions = questions,
            AnsweredCount = existingAnswers.Count,
            TotalQuestions = 5
        };
    }

    /// <summary>
    /// Submits answers to refinement questions.
    /// </summary>
    public async Task<SubmitAnswersResponse> SubmitAnswersAsync(
        Guid sessionId,
        SubmitAnswersRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new InvalidOperationException($"Session {sessionId} not found");

        if (session.Status == SessionStatus.Completed)
        {
            throw new InvalidOperationException("Cannot submit answers for a completed session");
        }

        // Validate session is in a refinement phase
        if (session.CurrentPhase != SessionPhase.Phase4_ProductRefinement &&
            session.CurrentPhase != SessionPhase.Phase5_TechnicalRefinement)
        {
            throw new InvalidOperationException(
                $"Session is in {session.CurrentPhase}, not a refinement phase");
        }

        // Map session phase to refinement phase
        var refinementPhase = session.CurrentPhase == SessionPhase.Phase4_ProductRefinement
            ? RefinementPhase.Phase4_PM
            : RefinementPhase.Phase5_Architect;

        // Get question templates for context
        var questionTemplates = refinementPhase == RefinementPhase.Phase4_PM ? PmQuestions : ArchitectQuestions;

        // Create answer entities
        var answers = request.Answers.Select(a =>
        {
            var template = questionTemplates.ElementAtOrDefault(a.QuestionNumber - 1);
            return new RefinementAnswer
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Phase = refinementPhase,
                QuestionNumber = a.QuestionNumber,
                QuestionText = template.Question ?? $"Question {a.QuestionNumber}",
                QuestionCategory = template.Category ?? "General",
                AnswerText = a.AnswerText,
                Timestamp = DateTimeOffset.UtcNow
            };
        }).ToList();

        // Save answers
        await _answerRepository.CreateBatchAsync(answers, cancellationToken);

        _logger.LogInformation(
            "Session {SessionId}: Saved {Count} answers for {Phase}",
            sessionId, answers.Count, refinementPhase);

        // Check if all questions for this phase are answered
        var totalAnswers = await _answerRepository.CountBySessionAndPhaseAsync(
            sessionId, refinementPhase, cancellationToken);

        var phaseComplete = totalAnswers >= 10;
        SessionPhase? nextPhase = null;
        var refinementComplete = false;

        if (phaseComplete)
        {
            // Advance to next phase
            if (refinementPhase == RefinementPhase.Phase4_PM)
            {
                session.CurrentPhase = SessionPhase.Phase5_TechnicalRefinement;
                nextPhase = SessionPhase.Phase5_TechnicalRefinement;
            }
            else
            {
                session.CurrentPhase = SessionPhase.Phase6_Visual;
                nextPhase = SessionPhase.Phase6_Visual;
                refinementComplete = true;
            }

            await _sessionRepository.UpdateAsync(session, cancellationToken);

            _logger.LogInformation(
                "Session {SessionId}: {Phase} complete, advancing to {NextPhase}",
                sessionId, refinementPhase, session.CurrentPhase);
        }

        return new SubmitAnswersResponse
        {
            SessionId = sessionId,
            CurrentPhase = session.CurrentPhase,
            AnswersRecorded = answers.Count,
            NextPhase = nextPhase,
            RefinementComplete = refinementComplete,
            Message = phaseComplete
                ? $"All {refinementPhase} questions answered. Proceeding to {session.CurrentPhase}."
                : $"Recorded {answers.Count} answers. {10 - totalAnswers} remaining."
        };
    }

    /// <summary>
    /// Gets all answers for a session, optionally filtered by phase.
    /// </summary>
    public async Task<IReadOnlyList<RefinementAnswer>> GetAnswersAsync(
        Guid sessionId,
        RefinementPhase? phase = null,
        CancellationToken cancellationToken = default)
    {
        if (phase.HasValue)
        {
            return await _answerRepository.GetBySessionAndPhaseAsync(sessionId, phase.Value, cancellationToken);
        }

        return await _answerRepository.GetBySessionIdAsync(sessionId, cancellationToken);
    }

    /// <summary>
    /// Gets the synthesis context for generating questions.
    /// Used to personalize questions based on the synthesized concept.
    /// </summary>
    public async Task<SynthesisEntity?> GetSynthesisContextAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session?.SynthesisId == null)
        {
            return null;
        }

        return await _synthesisRepository.GetByIdAsync(session.SynthesisId.Value, cancellationToken);
    }

    private static RefinementPhase DeterminePhase(SessionEntity session, RefinementPhase? requested)
    {
        // If a specific phase is requested, validate and return it
        if (requested.HasValue)
        {
            return requested.Value;
        }

        // Otherwise, determine from session state
        return session.CurrentPhase switch
        {
            SessionPhase.Phase4_ProductRefinement => RefinementPhase.Phase4_PM,
            SessionPhase.Phase5_TechnicalRefinement => RefinementPhase.Phase5_Architect,
            // For completed sessions, allow viewing architect questions
            SessionPhase.Phase6_Visual or SessionPhase.Completed => RefinementPhase.Phase5_Architect,
            // For sessions not yet at refinement, default to PM
            _ => RefinementPhase.Phase4_PM
        };
    }
}
