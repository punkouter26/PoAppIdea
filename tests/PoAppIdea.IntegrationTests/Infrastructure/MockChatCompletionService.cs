using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace PoAppIdea.IntegrationTests.Infrastructure;

/// <summary>
/// Mock IChatCompletionService for integration tests.
/// Pattern: Test Double (Stub) - Returns predictable responses without calling Azure OpenAI.
/// 
/// This prevents "token leakage" costs during testing and provides deterministic results.
/// </summary>
public sealed class MockChatCompletionService : IChatCompletionService
{
    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        // Return predictable mock response based on the prompt
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
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
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = chatHistory.LastOrDefault()?.Content ?? "";
        var response = GenerateMockResponse(lastMessage);

        // Stream the response word by word
        foreach (var word in response.Split(' '))
        {
            yield return new StreamingChatMessageContent(AuthorRole.Assistant, word + " ");
            await Task.Delay(10, cancellationToken); // Simulate streaming
        }
    }

    private static string GenerateMockResponse(string prompt)
    {
        // Return appropriate mock responses based on common prompts
        if (prompt.Contains("idea", StringComparison.OrdinalIgnoreCase) ||
            prompt.Contains("app", StringComparison.OrdinalIgnoreCase))
        {
            return """
            [
              {"title": "Smart Task Manager", "description": "AI-powered task management app with intelligent prioritization", "score": 85},
              {"title": "Team Sync Hub", "description": "Real-time collaboration platform for remote teams", "score": 78},
              {"title": "Focus Flow", "description": "Productivity app with focus timers and analytics", "score": 72}
            ]
            """;
        }

        if (prompt.Contains("mutation", StringComparison.OrdinalIgnoreCase) ||
            prompt.Contains("variation", StringComparison.OrdinalIgnoreCase))
        {
            return """
            [
              {"name": "Gamification Layer", "description": "Add points and achievements to increase engagement"},
              {"name": "Voice Commands", "description": "Enable hands-free operation through voice control"},
              {"name": "AI Assistant", "description": "Integrate smart assistant for natural language queries"}
            ]
            """;
        }

        if (prompt.Contains("feature", StringComparison.OrdinalIgnoreCase))
        {
            return """
            [
              {"name": "Dark Mode", "priority": "High", "description": "Eye-friendly dark theme option"},
              {"name": "Offline Sync", "priority": "Medium", "description": "Work without internet connection"},
              {"name": "Widget Support", "priority": "Low", "description": "Home screen widgets for quick access"}
            ]
            """;
        }

        if (prompt.Contains("question", StringComparison.OrdinalIgnoreCase) ||
            prompt.Contains("refinement", StringComparison.OrdinalIgnoreCase))
        {
            return """
            [
              {"question": "Who is the primary target audience?", "phase": "Scope"},
              {"question": "What is the core problem being solved?", "phase": "Scope"},
              {"question": "What platforms should be supported?", "phase": "Technical"}
            ]
            """;
        }

        // Default response
        return """
        {
          "response": "This is a mock response for testing purposes.",
          "status": "success"
        }
        """;
    }
}
