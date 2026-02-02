# C4 Component Diagram - PoAppIdea.Web

Component diagram showing the internal structure of the Web application.

```mermaid
C4Component
    title Component Diagram - PoAppIdea.Web Features

    Container_Boundary(features, "Feature Modules (VSA)") {
        Component(session, "Session", "Feature Module", "Session lifecycle: create, resume, list")
        Component(spark, "Spark", "Feature Module", "Idea generation and swipe recording")
        Component(mutation, "Mutation", "Feature Module", "Idea evolution and rating")
        Component(featureexp, "FeatureExpansion", "Feature Module", "Feature variations and MoSCoW")
        Component(synthesis, "Synthesis", "Feature Module", "Idea merging and cohesive synthesis")
        Component(refinement, "Refinement", "Feature Module", "PM and Architect Q&A phases")
        Component(visual, "Visual", "Feature Module", "Mockup and mood board generation")
        Component(artifacts, "Artifacts", "Feature Module", "PRD, Tech Deep-Dive generation")
        Component(gallery, "Gallery", "Feature Module", "Public idea discovery")
        Component(personality, "Personality", "Feature Module", "Product personality definition")
        Component(auth, "Auth", "Feature Module", "OAuth login/logout endpoints")
    }

    Container_Boundary(infra, "Infrastructure") {
        Component(tablestorage, "TableStorageClient", "Repository", "Generic CRUD for Azure Tables")
        Component(blobstorage, "BlobStorageClient", "Repository", "Artifact/image storage")
        Component(ideagen, "IdeaGenerator", "AI Service", "GPT-4o idea generation")
        Component(visualgen, "VisualGenerator", "AI Service", "DALL-E mockup generation")
        Component(artifactgen, "ArtifactGenerator", "AI Service", "PRD/document generation")
        Component(health, "HealthEndpoint", "Health Check", "Dependency health monitoring")
        Component(ratelimit, "RateLimitingMiddleware", "Middleware", "API rate limiting")
        Component(errorhandler, "ExceptionHandler", "Middleware", "Global error handling")
    }

    Rel(session, tablestorage, "Persists sessions")
    Rel(spark, ideagen, "Generates ideas")
    Rel(spark, tablestorage, "Stores swipes")
    Rel(mutation, ideagen, "Generates mutations")
    Rel(featureexp, ideagen, "Expands features")
    Rel(synthesis, ideagen, "Synthesizes ideas")
    Rel(refinement, tablestorage, "Stores answers")
    Rel(visual, visualgen, "Generates mockups")
    Rel(artifacts, artifactgen, "Generates documents")
    Rel(artifacts, blobstorage, "Stores artifacts")
```

## Feature Modules

| Module | Endpoints | Purpose |
|--------|-----------|---------|
| **Session** | 4 | Session CRUD and lifecycle |
| **Spark** | 3 | Idea generation, swipe recording, top ideas |
| **Mutation** | 4 | Idea mutation, rating, top mutations |
| **FeatureExpansion** | 4 | Feature variations, rating, top features |
| **Synthesis** | 4 | Idea selection, merging, synthesis |
| **Refinement** | 3 | PM/Architect questions and answers |
| **Visual** | 3 | Mockup generation and selection |
| **Artifacts** | 3 | PRD, Tech Deep-Dive, Visual Pack |
| **Gallery** | 2 | Public gallery browsing |
| **Personality** | 2 | Product personality management |
| **Auth** | 3 | OAuth login, logout, callback |

## Infrastructure Components

| Component | Type | Purpose |
|-----------|------|---------|
| **TableStorageClient** | Repository | Generic Azure Table operations |
| **BlobStorageClient** | Repository | Blob storage operations |
| **IdeaGenerator** | AI Service | Text generation via GPT-4o |
| **VisualGenerator** | AI Service | Image generation via DALL-E 3 |
| **ArtifactGenerator** | AI Service | Document generation |
| **HealthEndpoint** | Health Check | `/health` endpoint |
| **RateLimitingMiddleware** | Middleware | 100 req/60s rate limiting |
| **ExceptionHandler** | Middleware | Global error handling |
