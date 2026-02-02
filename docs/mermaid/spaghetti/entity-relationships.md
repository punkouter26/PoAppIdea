# Spaghetti Analysis - Entity Relationships

Visualization of entity dependencies and data flow.

```mermaid
erDiagram
    User ||--o{ Session : owns
    Session ||--o{ Idea : contains
    Session ||--o{ Swipe : tracks
    Session ||--o{ Mutation : evolves
    Session ||--o{ FeatureVariation : expands
    Session ||--o| Synthesis : merges
    Session ||--o{ RefinementAnswer : refines
    Session ||--o{ VisualAsset : visualizes
    Session ||--o{ Artifact : generates
    Session ||--o| ProductPersonality : defines
    
    Idea ||--o{ Swipe : receives
    Idea ||--o{ Mutation : evolves_into
    Mutation ||--o{ FeatureVariation : expands_into
    FeatureVariation ||--o{ Synthesis : selected_for
    
    User {
        Guid Id PK
        string Email
        string DisplayName
        AuthProvider Provider
    }
    
    Session {
        Guid Id PK
        Guid UserId FK
        AppType AppType
        int Complexity
        SessionPhase Phase
        SessionStatus Status
    }
    
    Idea {
        Guid Id PK
        Guid SessionId FK
        string Title
        string Description
        float Score
        int BatchNumber
    }
    
    Swipe {
        Guid Id PK
        Guid SessionId FK
        Guid IdeaId FK
        SwipeDirection Direction
        SwipeSpeed Speed
        int DurationMs
    }
    
    Mutation {
        Guid Id PK
        Guid SessionId FK
        Guid SourceIdeaId FK
        MutationType Type
        float Score
    }
    
    FeatureVariation {
        Guid Id PK
        Guid SessionId FK
        Guid MutationId FK
        string VariationTheme
        float Score
    }
    
    Synthesis {
        Guid Id PK
        Guid SessionId FK
        string MergedConcept
        string ThematicBridge
    }
    
    RefinementAnswer {
        Guid Id PK
        Guid SessionId FK
        RefinementPhase Phase
        int QuestionIndex
        string Answer
    }
    
    VisualAsset {
        Guid Id PK
        Guid SessionId FK
        string ImageUrl
        bool IsSelected
    }
    
    Artifact {
        Guid Id PK
        Guid SessionId FK
        ArtifactType Type
        string BlobUrl
    }
```

## Entity Coupling Analysis

### High Fan-Out (Session)

Session is the central hub with 10 dependent entity types:

| Entity | Cardinality | Purpose |
|--------|-------------|---------|
| Idea | 1:N (20) | Generated ideas |
| Swipe | 1:N (20) | User interactions |
| Mutation | 1:N (9) | Evolved concepts |
| FeatureVariation | 1:N (50) | Feature variants |
| Synthesis | 1:1 | Merged concept |
| RefinementAnswer | 1:N (20) | Q&A responses |
| VisualAsset | 1:N (3) | Generated mockups |
| Artifact | 1:N (3) | Final documents |
| ProductPersonality | 1:1 | Product identity |

### Data Flow Pattern

```
User → Session → Ideas → Swipes
                    ↓
              Mutations → FeatureVariations
                    ↓
              Synthesis → RefinementAnswers
                    ↓
              VisualAssets → Artifacts
```

## Recommendations

1. **Partition Strategy**: Session-based partitioning in Table Storage (already implemented)
2. **Cascade Deletes**: Session deletion should cascade to all child entities
3. **Archive Pattern**: Move completed sessions to cold storage after 90 days
