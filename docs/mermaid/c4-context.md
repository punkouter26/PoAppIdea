# C4 Context Diagram - PoAppIdea

System context diagram showing PoAppIdea and its external dependencies.

```mermaid
C4Context
    title System Context Diagram - PoAppIdea

    Person(user, "Creator", "User who wants to evolve app ideas into professional specifications")
    
    System(poappidea, "PoAppIdea", "AI-powered ideation platform that transforms concepts into product visions through swiping, synthesis, and artifact generation")
    
    System_Ext(azureopenai, "Azure OpenAI", "GPT-4o for idea generation, mutations, synthesis, and artifact creation. DALL-E 3 for visual mockups")
    System_Ext(azurestorage, "Azure Storage", "Table Storage for entities, Blob Storage for artifacts and images")
    System_Ext(appinsights, "Application Insights", "Telemetry, logging, and monitoring via OpenTelemetry")
    System_Ext(oauth, "OAuth Providers", "Google, GitHub, Microsoft identity providers for authentication")
    System_Ext(keyvault, "Azure Key Vault", "Secure secrets management for production")

    Rel(user, poappidea, "Uses", "HTTPS")
    Rel(poappidea, azureopenai, "Generates ideas, mutations, artifacts", "HTTPS/REST")
    Rel(poappidea, azurestorage, "Stores sessions, ideas, artifacts", "HTTPS")
    Rel(poappidea, appinsights, "Sends telemetry", "HTTPS")
    Rel(poappidea, oauth, "Authenticates users", "OAuth 2.0")
    Rel(poappidea, keyvault, "Retrieves secrets", "HTTPS")
```

## Actors

| Actor | Description |
|-------|-------------|
| **Creator** | End user who brainstorms and evolves app ideas |

## External Systems

| System | Purpose |
|--------|---------|
| **Azure OpenAI** | AI backbone for idea generation (GPT-4o) and visual mockups (DALL-E 3) |
| **Azure Storage** | Persistence layer for all entities and generated artifacts |
| **Application Insights** | Observability and monitoring |
| **OAuth Providers** | User authentication via Google, GitHub, Microsoft |
| **Azure Key Vault** | Production secrets management |
