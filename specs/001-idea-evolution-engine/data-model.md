# Data Model: PoAppIdea - The Self-Evolving Ideation Engine

**Branch**: `001-idea-evolution-engine` | **Date**: 2026-01-29

## Overview

This document defines the domain entities, their relationships, validation rules, and state transitions for the PoAppIdea platform. The model supports Azure Table Storage partitioning strategy with UserId as the primary partition key.

---

## Entity Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                   User                                       │
│  (OAuth: Google/GitHub/Microsoft or Email/Password)                         │
└─────────────────────────────────────────────────────────────────────────────┘
         │                                    │
         │ 1:1                                │ 1:N
         ▼                                    ▼
┌─────────────────────┐            ┌─────────────────────────────────────────┐
│  ProductPersonality │            │                 Session                  │
│  (learned prefs)    │            │  (one evolution journey: phases 0-6)    │
└─────────────────────┘            └─────────────────────────────────────────┘
                                              │
                     ┌────────────────────────┼────────────────────────┐
                     │                        │                        │
                     ▼                        ▼                        ▼
              ┌─────────────┐          ┌─────────────┐          ┌─────────────┐
              │    Idea     │          │   Swipe     │          │  Artifact   │
              │ (generated) │◄─────────│ (user vote) │          │   (output)  │
              └─────────────┘          └─────────────┘          └─────────────┘
                     │
         ┌───────────┴───────────┐
         ▼                       ▼
┌─────────────────┐    ┌─────────────────────┐
│    Mutation     │    │  FeatureVariation   │
│ (evolved idea)  │    │  (service options)  │
└─────────────────┘    └─────────────────────┘
         │
         ▼
┌─────────────────────┐
│     Synthesis       │
│ (merged concept)    │
└─────────────────────┘
         │
         ▼
┌─────────────────────┐
│  RefinementAnswer   │
│ (PM/Arch questions) │
└─────────────────────┘
         │
         ▼
┌─────────────────────┐
│    VisualAsset      │
│ (DALL-E mockup)     │
└─────────────────────┘
```

---

## Entities

### User

Represents an authenticated user of the platform.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique identifier |
| ExternalId | string | Unique, Required | OAuth provider user ID (e.g., Google sub) |
| Provider | AuthProvider | Required | Google, GitHub, Microsoft, or Email |
| Email | string | Required, Email format | User's email address |
| DisplayName | string | Required, 1-100 chars | Display name for gallery |
| CreatedAt | DateTimeOffset | Required | Account creation timestamp |
| LastLoginAt | DateTimeOffset | Required | Last authentication timestamp |

**Validation Rules**:
- Email must be valid email format
- DisplayName must be 1-100 characters, no special characters except space/hyphen

---

### ProductPersonality

Accumulated preferences learned from user's swipe behavior across sessions.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| UserId | Guid | PK, FK→User | Owner of this personality |
| ProductBiases | Dictionary<string, float> | Required | Bias scores for product traits (e.g., "social": 0.8) |
| TechnicalBiases | Dictionary<string, float> | Required | Bias scores for tech traits (e.g., "serverless": 0.6) |
| DislikedPatterns | List<string> | Required | Patterns to rank lower (e.g., "subscription-based") |
| SwipeSpeedProfile | SpeedProfile | Required | Fast/Medium/Slow average speeds |
| TotalSessions | int | Required, ≥0 | Number of completed sessions |
| LastUpdatedAt | DateTimeOffset | Required | Last profile update |

**Validation Rules**:
- Bias scores must be between -1.0 and 1.0
- DislikedPatterns max 50 entries

**State Transitions**:
- Updated after each completed session
- Bias scores decay 10% per 30 days of inactivity

---

### Session

A single idea evolution journey through phases 0-6.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique session identifier |
| UserId | Guid | FK→User, Required | Owner of this session |
| AppType | AppType | Required | Game, Productivity, Mobile, Automation |
| ComplexityLevel | int | Required, 1-5 | Complexity slider value |
| CurrentPhase | SessionPhase | Required | Current phase (0-6) |
| Status | SessionStatus | Required | InProgress, Completed, Abandoned |
| CreatedAt | DateTimeOffset | Required | Session start timestamp |
| CompletedAt | DateTimeOffset? | Nullable | Session completion timestamp |
| TopIdeaIds | List<Guid> | Required | Top 5 from Phase 1, Top 10 from Phase 2 |
| SelectedIdeaIds | List<Guid> | Required | User's final selection (1-10 ideas) |
| SynthesisId | Guid? | Nullable | Merged concept if multiple selected |

**Validation Rules**:
- ComplexityLevel must be 1-5
- TopIdeaIds max 10 entries
- SelectedIdeaIds max 10 entries

**State Transitions**:
```
Created → Phase0 (scope config)
Phase0 → Phase1 (spark discovery) when AppType + Complexity set
Phase1 → Phase2 (mutation) when 50 swipes complete
Phase2 → Phase3 (feature expansion) when top 10 rated
Phase3 → Submission when all variations rated
Submission → Phase4 (PM questions) when selection made
Phase4 → Phase5 (Architect questions) when 10 answers
Phase5 → Phase6 (visuals) when 10 answers
Phase6 → Completed when visual selected + artifacts generated
Any → Abandoned after 30 days inactive
```

---

### Idea

A generated app concept.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique idea identifier |
| SessionId | Guid | FK→Session, Required | Session this idea belongs to |
| Title | string | Required, 1-100 chars | Short idea title |
| Description | string | Required, 1-500 chars | 1-2 sentence description |
| BatchNumber | int | Required, 1-5 | Which batch (for Phase 1 learning) |
| GenerationPrompt | string | Required | LLM prompt used to generate |
| DnaKeywords | List<string> | Required | Extracted trait keywords |
| Score | float | Required | Weighted rating score |
| CreatedAt | DateTimeOffset | Required | Generation timestamp |

**Validation Rules**:
- Title: 1-100 characters
- Description: 1-500 characters
- BatchNumber: 1-5
- DnaKeywords: max 20 entries

---

### Swipe

User interaction with an idea.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique swipe identifier |
| SessionId | Guid | FK→Session, Required | Session context |
| IdeaId | Guid | FK→Idea, Required | Idea being swiped |
| UserId | Guid | FK→User, Required | User who swiped |
| Direction | SwipeDirection | Required | Like or Dislike |
| DurationMs | int | Required, >0 | Time from display to swipe (ms) |
| Timestamp | DateTimeOffset | Required | When swipe occurred |
| SpeedCategory | SwipeSpeed | Required | Fast (<1s), Medium (1-3s), Slow (>3s) |

**Validation Rules**:
- DurationMs must be positive
- SpeedCategory derived: <1000ms = Fast, 1000-3000ms = Medium, >3000ms = Slow

**Business Rules**:
- Fast + Like = 2x weight
- Medium + Like = 1x weight
- Slow + Like = 0.5x weight

---

### Mutation

A derived idea from Crossover or Repurposing.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique mutation identifier |
| SessionId | Guid | FK→Session, Required | Session context |
| ParentIdeaIds | List<Guid> | Required, 1-2 | Source ideas (1 for Repurpose, 2 for Crossover) |
| MutationType | MutationType | Required | Crossover or Repurposing |
| Title | string | Required, 1-100 chars | Mutated idea title |
| Description | string | Required, 1-500 chars | Mutated description |
| MutationRationale | string | Required | How/why mutation was applied |
| Score | float | Required | User rating score |
| CreatedAt | DateTimeOffset | Required | Generation timestamp |

**Validation Rules**:
- Crossover requires exactly 2 ParentIdeaIds
- Repurposing requires exactly 1 ParentIdeaId

---

### FeatureVariation

A set of capabilities and service integrations.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique variation identifier |
| SessionId | Guid | FK→Session, Required | Session context |
| MutationId | Guid | FK→Mutation, Required | Parent mutation |
| Features | List<Feature> | Required, 3-10 | Feature list |
| ServiceIntegrations | List<string> | Required | e.g., "Geolocation API", "OAuth" |
| VariationTheme | string | Required | e.g., "Privacy-first", "Social-heavy" |
| Score | float | Required | User rating score |
| CreatedAt | DateTimeOffset | Required | Generation timestamp |

**Nested Type - Feature**:
| Field | Type | Description |
|-------|------|-------------|
| Name | string | Feature name |
| Description | string | Feature description |
| Priority | FeaturePriority | Must/Should/Could/Won't |

---

### Synthesis

A merged concept from multiple selected ideas.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique synthesis identifier |
| SessionId | Guid | FK→Session, Required | Session context |
| SourceIdeaIds | List<Guid> | Required, 2-10 | Ideas being merged |
| MergedTitle | string | Required, 1-100 chars | Unified concept title |
| MergedDescription | string | Required, 1-1000 chars | Unified description |
| ThematicBridge | string | Required | Explanation of how ideas connect |
| RetainedElements | Dictionary<Guid, List<string>> | Required | Which elements kept per source |
| CreatedAt | DateTimeOffset | Required | Synthesis timestamp |

**Validation Rules**:
- SourceIdeaIds: 2-10 entries
- If only 1 idea selected, no Synthesis created (bypass)

---

### RefinementAnswer

User response to a clarifying question.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique answer identifier |
| SessionId | Guid | FK→Session, Required | Session context |
| Phase | RefinementPhase | Required | Phase4_PM or Phase5_Architect |
| QuestionNumber | int | Required, 1-10 | Question index |
| QuestionText | string | Required | The question asked |
| QuestionCategory | string | Required | e.g., "UserPersonas", "Deployment" |
| AnswerText | string | Required, 1-2000 chars | User's answer |
| Timestamp | DateTimeOffset | Required | When answered |

**Validation Rules**:
- QuestionNumber: 1-10
- AnswerText: 1-2000 characters

---

### VisualAsset

A DALL-E generated mockup or mood board.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique asset identifier |
| SessionId | Guid | FK→Session, Required | Session context |
| BlobUrl | string | Required, URL | Azure Blob Storage URL |
| ThumbnailUrl | string | Required, URL | Thumbnail URL |
| Prompt | string | Required | DALL-E prompt used |
| StyleAttributes | StyleInfo | Required | Color palette, layout, vibe |
| IsSelected | bool | Required | User's chosen visual |
| CreatedAt | DateTimeOffset | Required | Generation timestamp |

**Nested Type - StyleInfo**:
| Field | Type | Description |
|-------|------|-------------|
| ColorPalette | List<string> | Hex colors |
| LayoutStyle | string | e.g., "Dashboard", "Card-based" |
| Vibe | string | e.g., "Professional", "Playful" |

---

### Artifact

Final deliverable document.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | Guid | PK, Required | Unique artifact identifier |
| SessionId | Guid | FK→Session, Required | Session context |
| UserId | Guid | FK→User, Required | Owner |
| Type | ArtifactType | Required | PRD, TechnicalDeepDive, VisualAssetPack |
| Title | string | Required | Human-readable title |
| Content | string | Required | Markdown content (PRD, Tech) or manifest (Visual) |
| BlobUrl | string? | Nullable | For Visual pack zip file |
| IsPublished | bool | Required | Published to gallery |
| HumanReadableSlug | string | Required, Unique | URL-friendly identifier |
| CreatedAt | DateTimeOffset | Required | Generation timestamp |
| PublishedAt | DateTimeOffset? | Nullable | When published to gallery |

**Validation Rules**:
- HumanReadableSlug: lowercase, alphanumeric + hyphens, 5-100 chars

---

## Enumerations

### AuthProvider
```csharp
public enum AuthProvider { Google, GitHub, Microsoft, Email }
```

### AppType
```csharp
public enum AppType { Game, Productivity, Mobile, Automation }
```

### SessionPhase
```csharp
public enum SessionPhase { 
    Phase0_Scope, 
    Phase1_Spark, 
    Phase2_Mutation, 
    Phase3_FeatureExpansion,
    Submission,
    Phase4_ProductRefinement,
    Phase5_TechnicalRefinement,
    Phase6_Visual,
    Completed
}
```

### SessionStatus
```csharp
public enum SessionStatus { InProgress, Completed, Abandoned }
```

### SwipeDirection
```csharp
public enum SwipeDirection { Like, Dislike }
```

### SwipeSpeed
```csharp
public enum SwipeSpeed { Fast, Medium, Slow }
```

### MutationType
```csharp
public enum MutationType { Crossover, Repurposing }
```

### FeaturePriority
```csharp
public enum FeaturePriority { Must, Should, Could, Wont }
```

### RefinementPhase
```csharp
public enum RefinementPhase { Phase4_PM, Phase5_Architect }
```

### ArtifactType
```csharp
public enum ArtifactType { PRD, TechnicalDeepDive, VisualAssetPack }
```

---

## Azure Table Storage Mapping

| Entity | Table Name | PartitionKey | RowKey |
|--------|------------|--------------|--------|
| User | Users | Provider | ExternalId |
| ProductPersonality | Personalities | UserId | "profile" |
| Session | Sessions | UserId | SessionId |
| Idea | Ideas | SessionId | IdeaId |
| Swipe | Swipes | UserId | `{SessionId}_{IdeaId}` |
| Mutation | Mutations | SessionId | MutationId |
| FeatureVariation | FeatureVariations | SessionId | VariationId |
| Synthesis | Syntheses | SessionId | SynthesisId |
| RefinementAnswer | RefinementAnswers | SessionId | `{Phase}_{QuestionNumber}` |
| VisualAsset | VisualAssets | SessionId | AssetId |
| Artifact | Artifacts | UserId | `{SessionId}_{ArtifactType}` |

---

## Index Requirements

| Query Pattern | Supported By |
|--------------|--------------|
| Get user by OAuth ID | Users table: PK=Provider, RK=ExternalId |
| Get user's sessions | Sessions table: PK=UserId |
| Get session's ideas | Ideas table: PK=SessionId |
| Get user's swipes in session | Swipes table: PK=UserId, filter RK prefix |
| Get user's published artifacts | Artifacts table: PK=UserId, filter IsPublished |
| Browse public gallery | Artifacts table: Secondary index on IsPublished (or separate GalleryArtifacts table) |

---

## Validation Summary

All entities enforce:
1. Required fields cannot be null
2. String lengths within specified bounds
3. Enum values from defined set
4. Foreign key references to existing entities
5. Business rule constraints (e.g., Crossover requires 2 parents)
