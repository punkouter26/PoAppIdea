using Microsoft.Extensions.Logging;
using PoAppIdea.Core.Entities;
using PoAppIdea.Core.Enums;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Mock implementation of IIdeaGenerator for testing without AI costs.
/// Pattern: Null Object Pattern - Provides deterministic mock data for testing.
/// 
/// Enable by setting environment variable: MOCK_AI=true
/// Or in appsettings: "MockAI": true
/// </summary>
public sealed class MockIdeaGenerator : IIdeaGenerator
{
    private readonly ILogger<MockIdeaGenerator> _logger;
    private static readonly Random _random = new(42); // Seeded for reproducibility

    // Diverse mock app ideas for testing
    private static readonly string[][] MockIdeaData =
    [
        ["TaskFlow Pro", "A productivity powerhouse that uses AI to automatically prioritize tasks based on deadlines, dependencies, and your work patterns. Features smart scheduling, team collaboration, and progress analytics.", "productivity,tasks,ai,scheduling,collaboration"],
        ["FitBuddy AI", "Your personal AI fitness coach that creates adaptive workout plans based on your goals, available equipment, and recovery status. Includes form analysis via camera and social challenges.", "fitness,ai,workouts,health,tracking"],
        ["SmartNote Hub", "An intelligent note-taking app that automatically organizes, tags, and connects your notes using AI. Features voice transcription, diagram recognition, and knowledge graph visualization.", "notes,ai,organization,knowledge,productivity"],
        ["MealMaster", "AI-powered meal planning that considers your dietary preferences, budget, and what's already in your fridge. Generates shopping lists and suggests recipes with step-by-step video guides.", "meals,recipes,ai,planning,nutrition"],
        ["BudgetWise", "A financial wellness app that analyzes spending patterns, predicts upcoming expenses, and suggests personalized saving strategies. Includes investment tracking and bill reminders.", "finance,budget,savings,investment,tracking"],
        ["CodeHelper", "An AI coding assistant that integrates with your IDE to provide context-aware suggestions, automated testing, and code review. Supports multiple languages and frameworks.", "coding,ai,developer,tools,productivity"],
        ["TravelMate AI", "Your intelligent travel companion that creates personalized itineraries based on your interests, budget, and travel style. Features real-time translation and local recommendations.", "travel,planning,ai,itinerary,recommendations"],
        ["HomeHub IoT", "A unified smart home controller that connects all your devices, learns your routines, and automates your home for comfort and energy efficiency.", "home,iot,automation,smart,devices"],
        ["PetCare Plus", "Comprehensive pet health management with vet appointment scheduling, medication reminders, diet tracking, and AI-powered health insights from photos.", "pets,health,care,tracking,vet"],
        ["StudyPal", "An adaptive learning platform that creates personalized study plans, uses spaced repetition for retention, and gamifies the learning experience with achievements.", "education,learning,study,gamification,ai"]
    ];

    public MockIdeaGenerator(ILogger<MockIdeaGenerator> logger)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<Idea>> GenerateInitialIdeasAsync(
        Guid sessionId,
        AppType appType,
        int complexityLevel,
        ProductPersonality? personality,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK] Generating initial ideas for session {SessionId}, AppType={AppType}", sessionId, appType);
        
        // Simulate AI latency (but much faster)
        return Task.Delay(500, cancellationToken)
            .ContinueWith(_ => GenerateMockIdeas(sessionId, 1), cancellationToken);
    }

    public Task<IReadOnlyList<Idea>> GenerateMutatedIdeasAsync(
        Guid sessionId,
        IReadOnlyList<Idea> likedIdeas,
        IReadOnlyList<Idea> dislikedIdeas,
        int batchNumber,
        MutationType mutationType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MOCK] Generating mutated ideas for session {SessionId}, Batch={BatchNumber}, Type={MutationType}", 
            sessionId, batchNumber, mutationType);
        
        return Task.Delay(300, cancellationToken)
            .ContinueWith(_ => GenerateMutatedMockIdeas(sessionId, likedIdeas, batchNumber, mutationType), cancellationToken);
    }

    // Optimization 9: Synthesis is now handled exclusively by SynthesisEngine

    private static IReadOnlyList<Idea> GenerateMockIdeas(Guid sessionId, int batchNumber)
    {
        return MockIdeaData.Select((data, index) => new Idea
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Title = data[0],
            Description = data[1],
            BatchNumber = batchNumber,
            GenerationPrompt = "[MOCK] Initial batch generation",
            DnaKeywords = data[2].Split(',').ToList(),
            Score = 0.0f,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();
    }

    private static IReadOnlyList<Idea> GenerateMutatedMockIdeas(
        Guid sessionId, 
        IReadOnlyList<Idea> likedIdeas, 
        int batchNumber, 
        MutationType mutationType)
    {
        // Create hybrid ideas based on mutation type
        var mutatedIdeas = new List<Idea>();
        
        for (int i = 0; i < 10; i++)
        {
            var parentIndex = i % Math.Max(1, likedIdeas.Count);
            var parent = likedIdeas.Count > 0 ? likedIdeas[parentIndex] : null;
            
            var idea = new Idea
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                Title = mutationType == MutationType.Crossover 
                    ? $"Hybrid Concept {i + 1}: {(parent?.Title ?? "Innovation")} Plus"
                    : $"Evolved {i + 1}: {(parent?.Title ?? "Next-Gen App")}",
                Description = mutationType == MutationType.Crossover
                    ? $"A crossover mutation combining the best features of {parent?.Title ?? "top ideas"} with complementary capabilities for enhanced user value."
                    : $"A repurposed evolution of {parent?.Title ?? "the base concept"} applied to a new domain with expanded functionality.",
                BatchNumber = batchNumber,
                GenerationPrompt = $"[MOCK] {mutationType} mutation",
                DnaKeywords = parent?.DnaKeywords?.Take(3).Concat(["hybrid", "evolved", "enhanced"]).ToList() 
                    ?? ["innovation", "hybrid", "enhanced", "evolved", "ai"],
                Score = 0.0f,
                CreatedAt = DateTimeOffset.UtcNow
            };
            mutatedIdeas.Add(idea);
        }
        
        return mutatedIdeas;
    }

    // Optimization 9: GenerateMockSynthesis removed — synthesis handled by SynthesisEngine
}
