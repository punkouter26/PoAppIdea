# C4 Container Diagram - PoAppIdea

Container diagram showing the internal structure of PoAppIdea.

```mermaid
C4Container
    title Container Diagram - PoAppIdea

    Person(user, "Creator", "User evolving app ideas")

    Container_Boundary(poappidea, "PoAppIdea Platform") {
        Container(web, "PoAppIdea.Web", "Blazor Server + .NET 10", "Main web application with SSR and interactive components")
        Container(core, "PoAppIdea.Core", ".NET Class Library", "Domain entities, enums, and repository interfaces")
        Container(shared, "PoAppIdea.Shared", ".NET Class Library", "DTOs and contracts shared across projects")
    }

    Container_Boundary(infra, "Infrastructure Layer") {
        Container(storage, "Storage Client", "Azure.Data.Tables", "Table Storage operations for all entities")
        Container(blob, "Blob Client", "Azure.Storage.Blobs", "Blob Storage for artifacts and images")
        Container(ai, "AI Services", "Semantic Kernel", "Azure OpenAI integration for GPT-4o and DALL-E")
        Container(auth, "Auth Handler", "ASP.NET Core", "OAuth cookie authentication")
        Container(telemetry, "Telemetry", "OpenTelemetry", "Distributed tracing and metrics")
    }

    System_Ext(azureopenai, "Azure OpenAI", "GPT-4o, DALL-E 3")
    System_Ext(azurestorage, "Azure Storage", "Tables + Blobs")
    System_Ext(appinsights, "Application Insights", "Monitoring")
    System_Ext(oauth, "OAuth Providers", "Google, GitHub, Microsoft")

    Rel(user, web, "Uses", "HTTPS")
    Rel(web, core, "Uses")
    Rel(web, shared, "Uses")
    Rel(core, shared, "Uses")
    
    Rel(web, storage, "Persists entities")
    Rel(web, blob, "Stores artifacts")
    Rel(web, ai, "Generates content")
    Rel(web, auth, "Authenticates")
    Rel(web, telemetry, "Reports")
    
    Rel(storage, azurestorage, "HTTPS")
    Rel(blob, azurestorage, "HTTPS")
    Rel(ai, azureopenai, "HTTPS")
    Rel(auth, oauth, "OAuth 2.0")
    Rel(telemetry, appinsights, "OTLP")
```

## Containers

| Container | Technology | Purpose |
|-----------|------------|---------|
| **PoAppIdea.Web** | Blazor Server + Radzen | Main web application with 14 pages |
| **PoAppIdea.Core** | .NET 10 Library | 11 domain entities, enums, interfaces |
| **PoAppIdea.Shared** | .NET 10 Library | DTOs and contracts |

## Infrastructure Components

| Component | Technology | Purpose |
|-----------|------------|---------|
| **Storage Client** | Azure.Data.Tables | CRUD for 11 entity types |
| **Blob Client** | Azure.Storage.Blobs | Artifact and image storage |
| **AI Services** | Semantic Kernel | GPT-4o chat, DALL-E image generation |
| **Auth Handler** | ASP.NET Core Identity | OAuth authentication flow |
| **Telemetry** | OpenTelemetry | Traces, metrics, logs |
