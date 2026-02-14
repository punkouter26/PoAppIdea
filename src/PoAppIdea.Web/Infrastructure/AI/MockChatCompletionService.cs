using System.Runtime.CompilerServices;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace PoAppIdea.Web.Infrastructure.AI;

/// <summary>
/// Mock IChatCompletionService for testing without Azure OpenAI API calls.
/// Pattern: Test Double (Stub) - Returns predictable responses without calling real AI.
/// 
/// This prevents "token leakage" costs during testing and provides deterministic results.
/// Enable by setting environment variable: MOCK_AI=true
/// Or in appsettings: "MockAI": true
/// </summary>
public sealed class MockChatCompletionService : IChatCompletionService
{
    private readonly ILogger<MockChatCompletionService>? _logger;

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public MockChatCompletionService(ILogger<MockChatCompletionService>? logger = null)
    {
        _logger = logger;
    }

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
        _logger?.LogInformation("[MOCK ChatCompletion] Generating response for prompt: {Prompt}", 
            lastMessage.Length > 100 ? lastMessage[..100] + "..." : lastMessage);
        
        var response = GenerateMockResponse(lastMessage);

        var result = new List<ChatMessageContent>
        {
            new(AuthorRole.Assistant, response)
        };

        return Task.FromResult<IReadOnlyList<ChatMessageContent>>(result);
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
        _logger?.LogInformation("[MOCK ChatCompletion Streaming] Generating response for prompt: {Prompt}", 
            lastMessage.Length > 100 ? lastMessage[..100] + "..." : lastMessage);
        
        var response = GenerateMockResponse(lastMessage);

        // Stream the response word by word
        foreach (var word in response.Split(' '))
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, word + " ");
            await Task.Delay(5, cancellationToken); // Simulate streaming
        }
    }

    private static string GenerateMockResponse(string prompt)
    {
        var promptLower = prompt.ToLowerInvariant();
        
        // Feature expansion (must be checked BEFORE idea generation since prompts contain "generate" + "app")
        if (promptLower.Contains("feature") && promptLower.Contains("variation"))
        {
            return """
            {"variations": [
              {"variationTheme": "Minimalist MVP", "features": [{"name": "Core Task Board", "description": "Simple drag-and-drop task management", "priority": "Must"}, {"name": "Quick Notes", "description": "Lightweight text-only note capture", "priority": "Must"}, {"name": "Basic Reminders", "description": "Simple date-based notifications", "priority": "Should"}, {"name": "Offline Mode", "description": "Works without internet connection", "priority": "Should"}, {"name": "Dark Theme", "description": "Eye-friendly dark mode", "priority": "Could"}], "serviceIntegrations": ["Local Storage", "Push Notifications"]},
              {"variationTheme": "Enterprise-Ready", "features": [{"name": "SSO Login", "description": "SAML/OIDC single sign-on", "priority": "Must"}, {"name": "Admin Dashboard", "description": "User management and analytics", "priority": "Must"}, {"name": "Audit Trail", "description": "Complete activity logging", "priority": "Must"}, {"name": "Role Permissions", "description": "Granular RBAC controls", "priority": "Should"}, {"name": "SLA Monitoring", "description": "Uptime and performance tracking", "priority": "Should"}, {"name": "Data Export", "description": "CSV/PDF report generation", "priority": "Could"}], "serviceIntegrations": ["Azure AD", "Microsoft Teams", "Datadog"]},
              {"variationTheme": "Privacy-First", "features": [{"name": "E2E Encryption", "description": "End-to-end encrypted data", "priority": "Must"}, {"name": "Consent Manager", "description": "GDPR-compliant consent flows", "priority": "Must"}, {"name": "Data Residency", "description": "Choose where data is stored", "priority": "Should"}, {"name": "Anonymous Mode", "description": "Use without creating an account", "priority": "Should"}, {"name": "Auto-Delete", "description": "Scheduled data purging", "priority": "Could"}], "serviceIntegrations": ["Azure Key Vault", "Signal Protocol"]},
              {"variationTheme": "Social-Heavy", "features": [{"name": "Activity Feed", "description": "See what friends are doing", "priority": "Must"}, {"name": "Group Challenges", "description": "Compete with friends on goals", "priority": "Must"}, {"name": "Share Cards", "description": "Beautiful shareable achievement cards", "priority": "Should"}, {"name": "Comments & Reactions", "description": "Social interactions on items", "priority": "Should"}, {"name": "Leaderboards", "description": "Weekly and monthly rankings", "priority": "Could"}], "serviceIntegrations": ["Firebase", "Share API", "WebSockets"]},
              {"variationTheme": "AI-Powered", "features": [{"name": "Smart Suggestions", "description": "AI recommends next actions", "priority": "Must"}, {"name": "Auto-Categorize", "description": "ML-based content classification", "priority": "Must"}, {"name": "Predictive Analytics", "description": "Forecast trends from usage", "priority": "Should"}, {"name": "Natural Language Input", "description": "Create items via conversational text", "priority": "Should"}, {"name": "Anomaly Detection", "description": "Alert on unusual patterns", "priority": "Could"}], "serviceIntegrations": ["Azure OpenAI", "Azure Cognitive Services", "Application Insights"]}
            ]}
            """;
        }

        // Idea generation
        if (promptLower.Contains("generate") && (promptLower.Contains("idea") || promptLower.Contains("app")))
        {
            return """
            [
              {"title": "TaskFlow Pro", "description": "A productivity powerhouse that uses AI to automatically prioritize tasks based on deadlines, dependencies, and your work patterns. Features smart scheduling, team collaboration, and progress analytics.", "dnaKeywords": ["productivity", "ai", "tasks", "scheduling", "collaboration"]},
              {"title": "FitBuddy AI", "description": "Your personal AI fitness coach that creates adaptive workout plans based on your goals, available equipment, and recovery status. Includes form analysis and social challenges.", "dnaKeywords": ["fitness", "ai", "workouts", "health", "tracking"]},
              {"title": "SmartNote Hub", "description": "An intelligent note-taking app that automatically organizes, tags, and connects your notes using AI. Features voice transcription and knowledge graph visualization.", "dnaKeywords": ["notes", "ai", "organization", "knowledge", "productivity"]},
              {"title": "MealMaster", "description": "AI-powered meal planning that considers your dietary preferences, budget, and what's in your fridge. Generates shopping lists and suggests recipes.", "dnaKeywords": ["meals", "recipes", "ai", "planning", "nutrition"]},
              {"title": "BudgetWise", "description": "A financial wellness app that analyzes spending patterns, predicts expenses, and suggests personalized saving strategies.", "dnaKeywords": ["finance", "budget", "savings", "tracking", "analytics"]},
              {"title": "CodeHelper", "description": "An AI coding assistant that integrates with your IDE to provide context-aware suggestions, automated testing, and code review.", "dnaKeywords": ["coding", "ai", "developer", "tools", "productivity"]},
              {"title": "TravelMate AI", "description": "Your intelligent travel companion that creates personalized itineraries based on your interests, budget, and travel style.", "dnaKeywords": ["travel", "planning", "ai", "itinerary", "recommendations"]},
              {"title": "HomeHub IoT", "description": "A unified smart home controller that connects all your devices, learns your routines, and automates your home.", "dnaKeywords": ["home", "iot", "automation", "smart", "devices"]},
              {"title": "PetCare Plus", "description": "Comprehensive pet health management with vet scheduling, medication reminders, diet tracking, and AI health insights.", "dnaKeywords": ["pets", "health", "care", "tracking", "vet"]},
              {"title": "StudyPal", "description": "An adaptive learning platform that creates personalized study plans and uses spaced repetition for retention.", "dnaKeywords": ["education", "learning", "study", "gamification", "ai"]}
            ]
            """;
        }

        // Mutations/Variations
        if (promptLower.Contains("mutation") || promptLower.Contains("crossover") || promptLower.Contains("repurpos"))
        {
            return """
            [
              {"title": "Hybrid Concept 1: TaskFit Pro", "description": "A crossover that combines productivity tracking with fitness goals, helping users stay healthy while being productive.", "dnaKeywords": ["productivity", "fitness", "hybrid", "goals", "wellness"]},
              {"title": "Hybrid Concept 2: SmartMeal Budget", "description": "Combines meal planning with budget tracking for cost-effective healthy eating.", "dnaKeywords": ["meals", "budget", "planning", "savings", "nutrition"]},
              {"title": "Evolved Concept 1: AI Study Coach", "description": "An evolved learning platform with AI-powered tutoring and adaptive quizzes.", "dnaKeywords": ["education", "ai", "tutoring", "adaptive", "learning"]},
              {"title": "Hybrid Concept 3: TravelHome", "description": "Smart home integration for travelers - control your home while away.", "dnaKeywords": ["travel", "home", "iot", "remote", "automation"]},
              {"title": "Evolved Concept 2: PetFit Tracker", "description": "Pet health meets fitness tracking with activity monitoring and health insights.", "dnaKeywords": ["pets", "fitness", "health", "tracking", "ai"]}
            ]
            """;
        }

        // Feature expansion fallback (if the specific check above didn't match)
        if (promptLower.Contains("feature") || promptLower.Contains("expand"))
        {
            return """
            {"variations": [
              {"variationTheme": "General", "features": [{"name": "Core Feature", "description": "Primary app functionality", "priority": "Must"}, {"name": "User Profile", "description": "Basic user management", "priority": "Should"}, {"name": "Settings", "description": "App configuration", "priority": "Could"}], "serviceIntegrations": ["Azure Storage", "Azure AD"]}
            ]}
            """;
        }

        // Synthesis
        if (promptLower.Contains("synthesi") || promptLower.Contains("combine") || promptLower.Contains("merge"))
        {
            return """
            {"mergedTitle": "ProductivityAI Pro", "thematicBridge": "Unified productivity through intelligent automation and AI-powered personalization.", "retainedElements": ["smart scheduling", "ai insights", "team collaboration", "progress tracking", "personalized recommendations"]}
            """;
        }

        // Refinement questions
        if (promptLower.Contains("question") || promptLower.Contains("refinement") || promptLower.Contains("clarif"))
        {
            return """
            [
              {"question": "Who is the primary target audience for this product?", "phase": "ProductManager", "order": 1},
              {"question": "What is the core value proposition that differentiates this product?", "phase": "ProductManager", "order": 2},
              {"question": "What are the key success metrics you would track?", "phase": "ProductManager", "order": 3}
            ]
            """;
        }

        // PRD generation
        if (promptLower.Contains("prd") || promptLower.Contains("product requirements"))
        {
            return """
            ## Executive Summary
            This product is a comprehensive productivity platform that combines AI-powered task management with intelligent automation. The solution addresses the growing need for efficient work management in distributed teams.

            ## Target Users
            - Knowledge workers in tech companies
            - Remote team managers
            - Freelance professionals

            ## Key Features
            1. AI-powered task prioritization
            2. Smart scheduling with calendar integration
            3. Team collaboration spaces
            4. Progress analytics and reporting

            ## Success Metrics
            - Daily active users: 10,000+
            - Task completion rate: 80%+
            - User retention (30-day): 60%+
            """;
        }

        // Technical documentation
        if (promptLower.Contains("technical") || promptLower.Contains("architect"))
        {
            return """
            ## Architecture Overview
            The system follows a modern cloud-native architecture with microservices deployed on Azure.

            ## Technology Stack
            - Frontend: Blazor WebAssembly
            - Backend: ASP.NET Core 10
            - Database: Azure Cosmos DB
            - Cache: Redis
            - AI: Azure OpenAI

            ## API Design
            RESTful APIs with OpenAPI documentation.

            ## Security
            OAuth 2.0 with Azure AD B2C integration.
            """;
        }

        // Default response
        return """
        {
          "response": "This is a mock AI response for testing purposes.",
          "status": "success"
        }
        """;
    }
}
