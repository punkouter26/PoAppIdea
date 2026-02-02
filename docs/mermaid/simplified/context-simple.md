# Simplified C4 Context Diagram

Quick-reference architecture diagram for onboarding.

```mermaid
flowchart TB
    subgraph Users["👤 Users"]
        user[Creator]
    end

    subgraph PoAppIdea["🚀 PoAppIdea Platform"]
        web[Web App<br/>Blazor Server]
    end

    subgraph Azure["☁️ Azure Services"]
        openai[Azure OpenAI<br/>GPT-4o + DALL-E]
        storage[Azure Storage<br/>Tables + Blobs]
        insights[App Insights<br/>Monitoring]
        keyvault[Key Vault<br/>Secrets]
    end

    subgraph Auth["🔐 Identity"]
        google[Google]
        github[GitHub]
        microsoft[Microsoft]
    end

    user -->|HTTPS| web
    web -->|AI Generation| openai
    web -->|Data Storage| storage
    web -->|Telemetry| insights
    web -->|Secrets| keyvault
    web -->|OAuth| google
    web -->|OAuth| github
    web -->|OAuth| microsoft

    style web fill:#6366f1,color:#fff
    style openai fill:#10b981,color:#fff
    style storage fill:#3b82f6,color:#fff
    style insights fill:#f59e0b,color:#fff
    style keyvault fill:#ef4444,color:#fff
```

## Quick Reference

| Layer | What | Why |
|-------|------|-----|
| **Frontend** | Blazor Server + Radzen | Rich interactive UI with SSR |
| **AI** | Azure OpenAI (GPT-4o, DALL-E) | Idea generation, synthesis, visuals |
| **Storage** | Azure Tables + Blobs | Fast NoSQL + artifact storage |
| **Auth** | OAuth (Google, GitHub, MS) | Social login, no passwords |
| **Observability** | OpenTelemetry → App Insights | Full distributed tracing |
