# PoAppIdea Data Model

> **Version:** 2.0 (Enhanced)  
> **Last Updated:** 2026-02-12  
> **Audience:** Backend developers, data architects

---

## 📊 Entity Relationship Diagram (ERD)

### Complete Data Model

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
erDiagram
    USER ||--o{ SESSION : "owns"
    SESSION ||--o{ IDEA : "generates"
    SESSION ||--o{ SWIPE : "records"
    SESSION ||--o{ MUTATION : "creates"
    SESSION ||--o{ FEATURE_VARIATION : "expands"
    SESSION ||--o{ SYNTHESIS : "produces"
    SESSION ||--o{ REFINEMENT_ANSWER : "contains"
    SESSION ||--o{ VISUAL_ASSET : "outputs"
    SESSION ||--o{ ARTIFACT : "generates"
    IDEA ||--o{ SWIPE : "receives"
    MUTATION ||--o{ SWIPE : "rates"
    FEATURE_VARIATION ||--o{ SWIPE : "ratings"
    SYNTHESIS ||--o{ REFINEMENT_ANSWER : "answers"
    SYNTHESIS ||--o{ VISUAL_ASSET : "visualizes"
    SYNTHESIS ||--o{ ARTIFACT : "documents"
    SESSION ||--o| PRODUCT_PERSONALITY : "defines"

    USER {
        string id PK
        string authProvider
        string email
        string displayName
        string avatarUrl
        datetime createdAt
        datetime lastLoginAt
        int totalSessionsCompleted
    }

    SESSION {
        guid id PK
        guid userId FK
        string appType
        int complexityLevel
        string currentPhase
        string status
        datetime createdAt
        datetime completedAt
        datetime updatedAt
        list guid topIdeaIds
        list guid selectedIdeaIds
        guid synthesisId FK
        string metadata
    }

    IDEA {
        guid id PK
        guid sessionId FK
        string title
        string description
        string category
        int ranking
        int swipeScore
        datetime createdAt
    }

    SWIPE {
        guid id PK
        guid sessionId FK
        guid targetId FK
        string targetType
        string direction
        int speed
        datetime createdAt
    }

    MUTATION {
        guid id PK
        guid sessionId FK
        guid sourceIdeaId FK
        string title
        string description
        string mutationType
        int rating
        datetime createdAt
    }

    FEATURE_VARIATION {
        guid id PK
        guid sessionId FK
        string featureName
        string description
        string priority
        string category
        int rating
        datetime createdAt
    }

    SYNTHESIS {
        guid id PK
        guid sessionId FK
        string mergedConcept
        string vision
        string targetAudience
        string uniqueValueProp
        datetime createdAt
    }

    REFINEMENT_ANSWER {
        guid id PK
        guid sessionId FK
        guid synthesisId FK
        string phase
        string question
        string answer
        int sequenceNumber
        datetime createdAt
    }

    VISUAL_ASSET {
        guid id PK
        guid sessionId FK
        string imageUrl
        string style
        string prompt
        string dalleVersion
        datetime createdAt
        string blobStoragePath
    }

    ARTIFACT {
        guid id PK
        guid sessionId FK
        string type
        string format
        string content
        string blobStoragePath
        datetime createdAt
        long fileSizeBytes
    }

    PRODUCT_PERSONALITY {
        guid id PK
        guid sessionId FK
        string tone
        string style
        string targetAudienceSegment
        string uniqueValueProp
        datetime createdAt
    }
```

---

## 🔄 Session State Machine

### Session Lifecycle & Status Transitions

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
stateDiagram-v2
    [*] --> Created: POST /sessions
    Created --> InProgress: User enters scope
    InProgress --> InProgress: User advances phases
    InProgress --> Paused: User logs out
    Paused --> InProgress: User resumes session
    InProgress --> Abandoned: 30 days inactive
    InProgress --> Completed: Phase 8 artifacts generated
    Completed --> Archived: User action
    
    note right of Created
        Session created but not started
        Session ID generated
    end note
    
    note right of InProgress
        User actively working
        May pause anytime
    end note
    
    note right of Paused
        Session saved mid-operation
        Can resume later
    end note
    
    note right of Completed
        All artifacts generated
        Ready for download
    end note
    
    note right of Abandoned
        Auto-cleanup for stale data
    end note
```

---

## 📝 Data Storage Schema

### How Data Maps to Azure Table Storage

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
graph TB
    subgraph Tables["🗂️ Azure Table Storage"]
        direction TB
        UsersTable["📋 Users Table<br/>PartitionKey: authProvider<br/>RowKey: userId"]
        SessionsTable["📋 Sessions Table<br/>PartitionKey: userId<br/>RowKey: sessionId"]
        IdeasTable["📋 Ideas Table<br/>PartitionKey: sessionId<br/>RowKey: ideaId"]
        SwipesTable["📋 Swipes Table<br/>PartitionKey: sessionId<br/>RowKey: swipeId"]
        MutationsTable["📋 Mutations Table<br/>PartitionKey: sessionId<br/>RowKey: mutationId"]
        FeaturesTable["📋 Features Table<br/>PartitionKey: sessionId<br/>RowKey: featureId"]
        SynthesisTable["📋 Synthesis Table<br/>PartitionKey: sessionId<br/>RowKey: synthesisId"]
        RefinementTable["📋 Refinement Q&A Table<br/>PartitionKey: sessionId<br/>RowKey: answerId"]
        ArtifactsTable["📋 Artifacts Table<br/>PartitionKey: sessionId<br/>RowKey: artifactId"]
    end

    subgraph Blobs["🗂️ Azure Blob Storage"]
        direction TB
        VisualContainer["📁 visuals/<br/>sessionId/imageId.png"]
        ArtifactContainer["📁 artifacts/<br/>sessionId/prd.pdf"]
        TechDocContainer["📁 artifacts/<br/>sessionId/tech-spec.md"]
    end

    Query["🔍 Query Pattern<br/>Session → All Data<br/>for Download"]
    
    UsersTable --> Query
    SessionsTable --> Query
    IdeasTable --> Query
    SwipesTable --> Query
    MutationsTable --> Query
    FeaturesTable --> Query
    SynthesisTable --> Query
    RefinementTable --> Query
    ArtifactsTable --> Query

    style Tables fill:#ffffcc
    style Blobs fill:#d4e8ff
    style Query fill:#512BD4,stroke:#fff,color:#fff
```

---

## 💾 Data Types & Properties

### Core Entity Definitions

#### USER
| Property | Type | Notes |
|----------|------|-------|
| `id` | string | UUID from OAuth provider |
| `authProvider` | string | 'google', 'github', 'microsoft' |
| `email` | string | Unique identifier |
| `displayName` | string | User-friendly name |
| `avatarUrl` | string | OAuth provider avatar |
| `createdAt` | datetime | Account creation |
| `lastLoginAt` | datetime | Latest session login |
| `totalSessionsCompleted` | int | Metric: completed count |

#### SESSION
| Property | Type | Notes |
|----------|------|-------|
| `id` | GUID | Unique session identifier |
| `userId` | GUID | Reference to USER |
| `appType` | string | 'Web', 'Mobile', 'Desktop', 'API', 'SaaS' |
| `complexityLevel` | int | 1-5 scale |
| `currentPhase` | string | See phase list below |
| `status` | string | 'Created', 'InProgress', 'Paused', 'Completed', 'Abandoned' |
| `createdAt` | datetime | Session start |
| `completedAt` | datetime? | Null if incomplete |
| `topIdeaIds` | list[GUID] | Phase 1: Top 3 ideas |
| `selectedIdeaIds` | list[GUID] | Phase 4: Selected for synthesis |
| `synthesisId` | GUID | Reference to SYNTHESIS |
| `metadata` | JSON | Extra session-specific data |

#### IDEA
| Property | Type | Notes |
|----------|------|-------|
| `id` | GUID | Unique idea ID |
| `sessionId` | GUID | Reference to SESSION |
| `title` | string | One-line concept |
| `description` | string | 50-200 char description |
| `category` | string | Auto-categorized by AI |
| `ranking` | int | Swipe-score ranking (1-20) |
| `swipeScore` | int | Calculated: left=-1 * speed, right=+1 * speed |
| `createdAt` | datetime | Generated at Phase 1 |

#### SYNTHESIS
| Property | Type | Notes |
|----------|------|-------|
| `id` | GUID | Unique synthesis ID |
| `sessionId` | GUID | Reference to SESSION |
| `mergedConcept` | string | Consolidated concept title |
| `vision` | string | 1-2 paragraph vision |
| `targetAudience` | string | Who uses this app? |
| `uniqueValueProp` | string | What makes it unique? |
| `createdAt` | datetime | Generated at Phase 4 |

---

## 🔑 Indexing Strategy

### Query Optimization

```
Table: Sessions
├── PartitionKey: userId (frequent query: "get all sessions for user")
├── RowKey: sessionId
└── Index: (status, completedAt DESC) — for "completed sessions by date"

Table: Ideas
├── PartitionKey: sessionId (frequent query: "get all ideas in session")
├── RowKey: ideaId
└── Index: (ranking DESC) — for "top-ranked ideas"

Table: Swipes
├── PartitionKey: sessionId (frequent query: "get all swipes in session")
├── RowKey: swipeId
└── Index: (createdAt ASC) — for "swipe history"
```

---

## 📁 Blob Storage Structure

### File Organization Hierarchy

```
visuals/
  {sessionId}/
    {imageId}.png           # DALL-E 3 output
    {imageId}.metadata.json # Prompt, style, dimensions

artifacts/
  {sessionId}/
    prd.pdf                 # Product Specification
    prd.md                  # Markdown version
    tech-spec.pdf           # Technical Deep Dive
    tech-spec.md            # Markdown version
    manifest.json           # Asset manifest & metadata

gallery/
  {userId}/
    {sessionId}/
      snapshot.json         # Public-facing session summary
      image.png             # Featured image
```

---

## 🔐 Data Privacy & Retention

### Compliance & Cleanup

| Entity | Retention | Action |
|--------|-----------|--------|
| **User Account** | Indefinite | Soft-delete option; PII anonymized on request |
| **Completed Session** | 1 year | Auto-archive after 1 year |
| **Abandoned Session** | 30 days | Auto-delete if inactive |
| **Visual Assets** | With Session | Delete when session deleted |
| **Artifacts** | With Session | Delete when session deleted |
| **Swipes/Ratings** | With Session | Delete when session deleted |

---

## 📊 Data Growth Projections

### Estimated Storage Requirements

| Metric | Size | Monthly Growth | Notes |
|--------|------|---|-------|
| **1 Complete Session** | ~500 KB | — | Average session with artifacts |
| **1 Visual Asset (PNG)** | ~2-3 MB | — | DALL-E 3, high-res |
| **100 Sessions/Month** | ~50 MB | ~50 MB | Low volume early |
| **1,000 Sessions/Month** | ~500 MB | ~500 MB | Growing volume |
| **Annual Data (1k sessions/mo)** | ~6 GB | — | Manageable scale |

