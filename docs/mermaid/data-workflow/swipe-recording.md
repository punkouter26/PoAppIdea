# Data Workflow - Swipe Recording

Sequence diagram showing how user swipes are recorded and processed.

```mermaid
sequenceDiagram
    autonumber
    participant User
    participant SparkPage as SparkPage.razor
    participant SparkSvc as SparkService
    participant Storage as TableStorage
    participant Analytics as Telemetry

    User->>SparkPage: Swipe Right (Like)
    Note over User,SparkPage: Captures: direction, duration, timestamp
    
    SparkPage->>SparkPage: Calculate swipe speed
    SparkPage->>SparkSvc: RecordSwipeAsync(request)
    
    Note over SparkSvc: SwipeRequest contains:<br/>SessionId, IdeaId, Direction,<br/>SwipeSpeed, DurationMs
    
    SparkSvc->>SparkSvc: Create Swipe entity
    SparkSvc->>Storage: UpsertEntityAsync<Swipe>
    Storage-->>SparkSvc: Success
    
    SparkSvc->>SparkSvc: Calculate weighted score
    Note over SparkSvc: Fast swipes = higher confidence<br/>Score = direction * speed_weight
    
    SparkSvc->>Storage: UpdateIdeaScore(ideaId, score)
    Storage-->>SparkSvc: Updated Idea
    
    SparkSvc->>Analytics: TrackEvent("SwipeRecorded")
    Analytics-->>SparkSvc: Logged
    
    SparkSvc-->>SparkPage: SwipeResult
    SparkPage-->>User: Show next idea card
    
    alt Batch Complete (10 swipes)
        SparkPage->>SparkSvc: GetTopIdeasAsync(sessionId)
        SparkSvc->>Storage: QueryAsync(orderBy: Score DESC)
        Storage-->>SparkSvc: Top ideas
        SparkSvc-->>SparkPage: Results summary
        SparkPage-->>User: Show batch results
    end
```

## Swipe Speed Weighting

| Speed | Duration | Weight |
|-------|----------|--------|
| **Fast** | < 1s | 1.5x |
| **Normal** | 1-3s | 1.0x |
| **Slow** | 3-5s | 0.8x |
| **Hesitant** | > 5s | 0.5x |

## Data Model

```
Swipe {
    Id: Guid
    SessionId: Guid
    IdeaId: Guid
    Direction: Right/Left
    SwipeSpeed: Fast/Normal/Slow/Hesitant
    DurationMs: int
    CreatedAt: DateTimeOffset
}
```
