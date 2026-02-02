# Features Reference

Complete feature documentation for PoAppIdea.

---

## Feature Index

| Feature | Phase | Endpoints | Description |
|---------|-------|-----------|-------------|
| [Authentication](#authentication) | 0 | 3 | OAuth login/logout |
| [Session Management](#session-management) | 0 | 4 | Session lifecycle |
| [Spark (Idea Generation)](#spark-idea-generation) | 1 | 3 | AI-powered idea discovery |
| [Mutations](#mutations) | 2 | 4 | Idea evolution |
| [Feature Expansion](#feature-expansion) | 3 | 4 | Feature variations |
| [Synthesis](#synthesis) | 4 | 4 | Idea merging |
| [Refinement](#refinement) | 5 | 3 | Q&A phases |
| [Visuals](#visuals) | 6 | 3 | Mockup generation |
| [Artifacts](#artifacts) | 6 | 3 | Document generation |
| [Gallery](#gallery) | - | 2 | Public browsing |

---

## Authentication

OAuth-based authentication with multiple providers.

### Supported Providers

| Provider | Status | Scopes |
|----------|--------|--------|
| Google | ✅ Active | email, profile |
| GitHub | ✅ Active | user:email |
| Microsoft | ✅ Active | User.Read |

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/auth/login/{provider}` | Initiate OAuth flow |
| GET | `/auth/callback/{provider}` | OAuth callback |
| POST | `/auth/logout` | Sign out user |

### Configuration

```json
{
  "Authentication": {
    "Google": { "ClientId": "...", "ClientSecret": "..." },
    "GitHub": { "ClientId": "...", "ClientSecret": "..." },
    "Microsoft": { "ClientId": "...", "ClientSecret": "..." }
  }
}
```

---

## Session Management

Session creation and lifecycle management.

### Session States

```
Created → InProgress → Completed
              ↓
          Abandoned (after 24h inactivity)
```

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sessions` | Create new session |
| GET | `/api/sessions` | List user sessions |
| GET | `/api/sessions/{id}` | Get session details |
| POST | `/api/sessions/{id}/resume` | Resume session |

### Request/Response

**Create Session**
```json
POST /api/sessions
{
  "appType": "Mobile",
  "complexity": 3
}

Response:
{
  "sessionId": "guid",
  "phase": "Phase1_Spark",
  "status": "InProgress"
}
```

---

## Spark (Idea Generation)

AI-powered idea discovery through swiping interface.

### How It Works

1. User selects App Type and Complexity (1-5)
2. AI generates 10 ideas (Batch 1)
3. User swipes right (like) or left (dislike)
4. System records swipe direction and speed
5. After 10 swipes, AI generates Batch 2 based on likes
6. Top 3 ideas calculated from weighted scores

### Swipe Speed Weighting

| Speed | Duration | Weight |
|-------|----------|--------|
| Fast | < 1s | 1.5x (high confidence) |
| Normal | 1-3s | 1.0x |
| Slow | 3-5s | 0.8x |
| Hesitant | > 5s | 0.5x (low confidence) |

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sessions/{id}/ideas` | Generate ideas |
| POST | `/api/sessions/{id}/swipes` | Record swipe |
| GET | `/api/sessions/{id}/ideas/top` | Get top 3 ideas |

---

## Mutations

Directed evolution of top ideas.

### Mutation Types

| Type | Description |
|------|-------------|
| **Crossover** | Blend elements from multiple liked ideas |
| **Repurposing** | Shift target audience/use case |
| **Inversion** | Flip core assumption |

### Flow

1. Top 3 ideas from Spark phase
2. Each idea gets 3 mutations (9 total)
3. User rates mutations 1-5 stars
4. Top 10 mutations selected

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sessions/{id}/mutations` | Generate mutations |
| GET | `/api/sessions/{id}/mutations` | List mutations |
| POST | `/api/sessions/{id}/mutations/{mid}/rate` | Rate mutation |
| GET | `/api/sessions/{id}/mutations/top` | Get top 10 |

---

## Feature Expansion

Expand mutations into detailed feature variations.

### Feature Model

```typescript
Feature {
  name: string;           // e.g., "User Authentication"
  description: string;    // e.g., "OAuth 2.0 with SSO"
  priority: MoSCoW;       // Must | Should | Could | Wont
}

FeatureVariation {
  features: Feature[];         // 3-10 features
  serviceIntegrations: string[]; // e.g., ["Stripe", "Auth0"]
  variationTheme: string;      // e.g., "Enterprise-Ready"
}
```

### MoSCoW Prioritization

| Priority | Description | Target % |
|----------|-------------|----------|
| Must | Critical for launch | 30-40% |
| Should | High value, not blocking | 30-40% |
| Could | Nice to have | 15-20% |
| Won't | Out of scope | 5-10% |

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sessions/{id}/features` | Expand features |
| GET | `/api/sessions/{id}/features` | List variations |
| POST | `/api/sessions/{id}/features/{vid}/rate` | Rate variation |
| GET | `/api/sessions/{id}/features/top` | Get top 10 |

---

## Synthesis

Merge multiple ideas into cohesive concepts.

### Selection Rules

- Minimum: 1 idea
- Maximum: 10 ideas
- Single selection: Proceeds directly
- Multiple: AI finds thematic bridge

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sessions/{id}/selectable` | Get selectable ideas |
| POST | `/api/sessions/{id}/selections` | Submit selections |
| POST | `/api/sessions/{id}/synthesis` | Run synthesis |
| GET | `/api/sessions/{id}/synthesis` | Get result |

---

## Refinement

Product Manager and Architect Q&A phases.

### Question Phases

| Phase | Role | Focus Areas |
|-------|------|-------------|
| Phase 1 | PM | User personas, core loops, success metrics |
| Phase 2 | Architect | Deployment, scaling, security, data |

### Question Categories

**PM Phase:**
- Target users and personas
- Core value proposition
- Feature prioritization
- Success metrics and KPIs
- Competitive differentiation

**Architect Phase:**
- Deployment strategy
- Scaling requirements
- Data storage and privacy
- Security considerations
- Maintenance and monitoring

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sessions/{id}/refinement` | Get questions |
| POST | `/api/sessions/{id}/refinement` | Submit answers |
| GET | `/api/sessions/{id}/refinement/answers` | Get all answers |

---

## Visuals

AI-generated mockups and visual direction.

### Visual Types

| Type | Description |
|------|-------------|
| Mood Board | Color palette, typography, visual style |
| Mockup | Low-fidelity wireframe |
| Style Guide | Design system elements |

### Generation

- 3 unique visual directions generated
- Based on all previous context
- User selects preferred direction
- Selected style included in artifacts

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sessions/{id}/visuals` | Generate visuals |
| GET | `/api/sessions/{id}/visuals` | List visuals |
| POST | `/api/sessions/{id}/visuals/{vid}/select` | Select visual |

---

## Artifacts

Final document generation.

### Artifact Types

| Type | Format | Contents |
|------|--------|----------|
| **PRD** | Markdown | Executive summary, features, user stories |
| **Tech Deep-Dive** | Markdown | Architecture, stack, deployment |
| **Visual Pack** | Images + JSON | Mockups, style guide, assets |
| **Download** | ZIP | All artifacts bundled |

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/sessions/{id}/artifacts` | Generate artifacts |
| GET | `/api/sessions/{id}/artifacts` | List artifacts |
| GET | `/api/sessions/{id}/artifacts/{type}` | Download artifact |

---

## Gallery

Public discovery of completed ideas.

### Features

- Browse anonymized completed sessions
- Filter by app type, complexity
- Fork ideas to start new session
- Community upvotes (future)

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/gallery` | Browse public ideas |
| POST | `/api/gallery/{id}/fork` | Fork to new session |
