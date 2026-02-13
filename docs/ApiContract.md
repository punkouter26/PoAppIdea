# PoAppIdea API Contract

> **Version:** 1.0  
> **Last Updated:** 2026-02-12  
> **Base URL:** `https://poappidea.azurewebsites.net/api`

---

## 📋 API Overview

| Category | Endpoints | Authentication |
|----------|-----------|----------------|
| Session | 4 | Required |
| Spark | 3 | Required |
| Mutation | 4 | Required |
| Feature | 4 | Required |
| Synthesis | 4 | Required |
| Refinement | 3 | Required |
| Visual | 3 | Required |
| Artifact | 4 | Required |
| Gallery | 3 | Optional |
| Health | 1 | None |

---

## 🔐 Authentication

All endpoints (except `/health`) require a valid JWT Bearer token.

```http
Authorization: Bearer <token>
```

### OAuth Providers
- Google (`google`)
- GitHub (`github`)
- Microsoft (`microsoft`)

---

## 📄 Session Endpoints

### POST /api/sessions
Create a new ideation session.

**Request:**
```json
{
  "appType": "Mobile",
  "complexityLevel": 3
}
```

**Response (201):**
```json
{
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "appType": "Mobile",
  "complexityLevel": 3,
  "currentPhase": "Phase0_Scope",
  "status": "InProgress",
  "createdAt": "2026-02-12T20:00:00Z"
}
```

### GET /api/sessions/{id}
Get session details.

**Response (200):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "appType": "Mobile",
  "currentPhase": "Phase1_Spark",
  "status": "InProgress",
  "topIdeaIds": [],
  "selectedIdeaIds": []
}
```

### GET /api/sessions
List all sessions for current user.

**Response (200):**
```json
{
  "sessions": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "appType": "Mobile",
      "currentPhase": "Completed",
      "status": "Completed",
      "createdAt": "2026-02-12T20:00:00Z"
    }
  ]
}
```

---

## ⚡ Spark Endpoints

### POST /api/sessions/{id}/ideas
Generate 20 app ideas.

**Response (201):**
```json
{
  "ideas": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "title": "Fitness Tracking App",
      "description": "AI-powered workout planner",
      "category": "Health & Wellness",
      "ranking": 1
    }
  ]
}
```

### POST /api/sessions/{id}/swipes
Record a swipe action.

**Request:**
```json
{
  "ideaId": "550e8400-e29b-41d4-a716-446655440010",
  "direction": "Right",
  "speed": 2
}
```

### GET /api/sessions/{id}/top-ideas
Get top 3 selected ideas.

**Response (200):**
```json
{
  "topIdeas": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440010",
      "title": "Fitness Tracking App"
    }
  ]
}
```

---

## 🧬 Mutation Endpoints

### POST /api/sessions/{id}/mutations
Generate 9 mutations from top ideas.

**Response (201):**
```json
{
  "mutations": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440020",
      "title": "AI Fitness Coach",
      "description": "Personal AI trainer with real-time form correction",
      "mutationType": "Enhancement",
      "rating": 0
    }
  ]
}
```

### POST /api/sessions/{id}/mutations/{mutationId}/rate
Rate a mutation.

**Request:**
```json
{
  "rating": 5
}
```

### GET /api/sessions/{id}/top-mutations
Get top-rated mutations.

---

## 🎯 Feature Expansion Endpoints

### POST /api/sessions/{id}/features
Generate 50 feature variations.

**Response (201):**
```json
{
  "features": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440030",
      "featureName": "Workout Tracking",
      "description": "Track workouts with exercise details",
      "priority": "Must",
      "category": "Core"
    }
  ]
}
```

### POST /api/sessions/{id}/features/{featureId}/priority
Set feature priority.

**Request:**
```json
{
  "priority": "Should"
}
```

---

## 🔗 Synthesis Endpoints

### POST /api/sessions/{id}/synthesis
Synthesize selected ideas.

**Request:**
```json
{
  "selectedIdeaIds": [
    "550e8400-e29b-41d4-a716-446655440010",
    "550e8400-e29b-41d4-a716-446655440011"
  ]
}
```

**Response (201):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440040",
  "mergedConcept": "AI-Powered Fitness Platform",
  "vision": "Democratize personalized fitness",
  "targetAudience": "Fitness enthusiasts"
}
```

---

## 💬 Refinement Endpoints

### GET /api/sessions/{id}/questions?phase=pm
Get PM refinement questions.

**Response (200):**
```json
{
  "questions": [
    {
      "id": "q1",
      "text": "Who is your target user?",
      "category": "Audience"
    }
  ]
}
```

### GET /api/sessions/{id}/questions?phase=tech
Get technical refinement questions.

### POST /api/sessions/{id}/refinement
Submit answers.

**Request:**
```json
{
  "questionId": "q1",
  "answer": "Fitness enthusiasts aged 25-45"
}
```

---

## 🎨 Visual Endpoints

### POST /api/sessions/{id}/visuals
Generate DALL-E 3 visuals.

**Request:**
```json
{
  "style": "Modern",
  "count": 3
}
```

**Response (201):**
```json
{
  "visuals": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440050",
      "imageUrl": "https://storage.blob.core.windows.net/visuals/...",
      "description": "Mobile app mockup"
    }
  ]
}
```

### POST /api/sessions/{id}/visuals/{id}/select
Select a visual.

---

## 📄 Artifact Endpoints

### POST /api/sessions/{id}/artifacts
Generate artifacts (PRD, Tech Doc).

**Request:**
```json
{
  "types": ["PRD", "TechnicalDocument"]
}
```

**Response (201):**
```json
{
  "artifacts": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440060",
      "title": "Product Requirements Document",
      "artifactType": "PRD",
      "blobPath": "artifacts/prd-001.md"
    }
  ]
}
```

### GET /api/sessions/{id}/artifacts/{id}/download
Download artifact file.

---

## 🏥 Health Endpoints

### GET /health
Health check (no auth required).

**Response (200):**
```json
{
  "status": "Healthy",
  "timestamp": "2026-02-12T20:00:00Z",
  "version": "1.0.0"
}
```

---

## ⚠️ Error Responses

### 400 - Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "errors": {
    "ComplexityLevel": ["Must be between 1 and 5"]
  }
}
```

### 401 - Unauthorized
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required"
}
```

### 403 - Forbidden
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Access denied"
}
```

### 404 - Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Session not found"
}
```

### 429 - Too Many Requests
```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Try again in 60 seconds."
}
```

### 500 - Internal Server Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred"
}
```

---

## 🔒 Rate Limiting

| Tier | Requests | Window |
|------|----------|--------|
| Default | 100 | 60 seconds |
| AI Generation | 10 | 60 seconds |

---

## 📋 Simplified API Summary

### Core Endpoints
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/sessions | Create session |
| GET | /api/sessions/{id} | Get session |
| POST | /api/sessions/{id}/ideas | Generate ideas |
| POST | /api/sessions/{id}/swipes | Record swipe |
| POST | /api/sessions/{id}/mutations | Generate mutations |
| POST | /api/sessions/{id}/features | Generate features |
| POST | /api/sessions/{id}/synthesis | Create synthesis |
| POST | /api/sessions/{id}/refinement | Submit answers |
| POST | /api/sessions/{id}/visuals | Generate visuals |
| POST | /api/sessions/{id}/artifacts | Generate artifacts |
| GET | /health | Health check |

### Error Codes
- 400: Validation error
- 401: Unauthorized
- 403: Forbidden
- 404: Not found
- 429: Rate limited
- 500: Server error
