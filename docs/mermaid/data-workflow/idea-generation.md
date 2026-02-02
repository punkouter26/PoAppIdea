# Data Workflow - Idea Generation

Sequence diagram showing how ideas are generated in the Spark phase.

```mermaid
sequenceDiagram
    autonumber
    participant User
    participant SparkPage as SparkPage.razor
    participant SparkSvc as SparkService
    participant IdeaGen as IdeaGenerator
    participant OpenAI as Azure OpenAI
    participant Storage as TableStorage
    participant Session as SessionRepository

    User->>SparkPage: Click "Generate Ideas"
    SparkPage->>SparkSvc: GenerateIdeasAsync(sessionId, count)
    
    SparkSvc->>Session: GetSessionAsync(sessionId)
    Session->>Storage: GetEntityAsync<Session>
    Storage-->>Session: Session entity
    Session-->>SparkSvc: Session with AppType, Complexity
    
    SparkSvc->>IdeaGen: GenerateIdeasAsync(session, 10)
    
    Note over IdeaGen,OpenAI: Build prompt with AppType, Complexity, existing likes
    IdeaGen->>OpenAI: ChatCompletionAsync(prompt)
    OpenAI-->>IdeaGen: JSON array of ideas
    
    IdeaGen->>IdeaGen: Parse and validate ideas
    IdeaGen-->>SparkSvc: List<Idea>
    
    loop For each idea
        SparkSvc->>Storage: UpsertEntityAsync<Idea>
        Storage-->>SparkSvc: Success
    end
    
    SparkSvc-->>SparkPage: List<IdeaDto>
    SparkPage-->>User: Display idea cards
```

## Key Components

| Component | Responsibility |
|-----------|----------------|
| **SparkPage** | UI for swipe interface |
| **SparkService** | Orchestrates idea generation |
| **IdeaGenerator** | Builds prompts, calls OpenAI |
| **Azure OpenAI** | GPT-4o text generation |
| **TableStorage** | Persists ideas |
