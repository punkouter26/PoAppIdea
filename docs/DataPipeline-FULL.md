# PoAppIdea Data Pipeline

> **Version:** 2.0 (Enhanced)  
> **Last Updated:** 2026-02-12  
> **Audience:** Backend developers, DevOps, data architects

---

## 📊 Complete Data Pipeline Flow

### From User Input to Downloadable Artifacts

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
graph TB
    subgraph Input["📥 Input Layer"]
        direction TB
        Browser["🖥️ User Browser"]
        Form["📋 Form Data"]
    end

    subgraph Processing["⚙️ Processing Layer"]
        direction TB
        Validate["✓ Validation<br/>(FluentValidation)"]
        Enrich["🔸 Enrich Metadata"]
        Transform["🔀 Transform to Domain"]
    end

    subgraph AIGeneration["🤖 AI Generation Layer"]
        direction TB
        SecKernel["Semantic Kernel"]
        GPT4["GPT-4o Chat<br/>Completions"]
        DALLE["DALL-E 3<br/>Image Gen"]
    end

    subgraph Storage["💾 Storage Layer"]
        direction TB
        Tables["Azure Table Storage<br/>(Relational Data)"]
        Blobs["Azure Blob Storage<br/>(Files & Assets)"]
        Cache["Redis Cache<br/>(Optional)"]
    end

    subgraph Aggregation["🧬 Aggregation Layer"]
        direction TB
        Synthesis["Synthesis Engine<br/>(Merge Ideas)"]
        Refine["Refinement Engine<br/>(Answer Q's)"]
        Package["Package Generator<br/>(Create ZIP)"]
    end

    subgraph Output["📤 Output Layer"]
        direction TB
        Format["📄 Format Outputs<br/>(PDF, Markdown)"]
        Sign["🔐 Sign & Verify"]
        Download["📥 Download Ready"]
    end

    subgraph Monitoring["📊 Monitoring"]
        direction TB
        AppInsights["Application Insights<br/>(Logs & Metrics)"]
        HealthCheck["Health Check<br/>(/health)"]
    end

    Browser --> Form
    Form --> Input
    
    Input --> Validate
    Validate --> Enrich
    Enrich --> Transform
    
    Transform --> AIGeneration
    Transform --> Storage
    
    AIGeneration --> GPT4
    AIGeneration --> DALLE
    
    GPT4 --> Storage
    DALLE --> Storage
    
    Storage --> Aggregation
    Aggregation --> Synthesis
    Aggregation --> Refine
    Aggregation --> Package
    
    Package --> Output
    Output --> Format
    Format --> Sign
    Sign --> Download
    
    Input -.->|"Telemetry"| Monitoring
    Storage -.->|"Telemetry"| Monitoring
    AIGeneration -.->|"API Calls"| Monitoring
    Output -.->|"Telemetry"| Monitoring

    style Input fill:#d4e8ff
    style Processing fill:#cce5ff
    style AIGeneration fill:#ffe5cc
    style Storage fill:#ffffcc
    style Aggregation fill:#e8f4e8
    style Output fill:#107c10,stroke:#fff,color:#fff
    style Monitoring fill:#e5ffe5
```

---

## 🔄 Phase-by-Phase Data Transformations

### How Data Evolves Through Each Phase

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
graph TD
    P0["⚙️ Phase 0: Scope<br/>Input: appType, complexity<br/>Output: Session created"]
    
    P1["⚡ Phase 1: Spark<br/>Input: scope data<br/>Process: Prompt GPT-4o x20<br/>Output: 20 ideas"]
    
    P2["🧬 Phase 2: Mutation<br/>Input: top 3 ideas<br/>Process: Mutate x9 + user rates<br/>Output: 9 mutations + ratings"]
    
    P3["🎯 Phase 3: Features<br/>Input: selected mutations<br/>Process: Generate 50 features<br/>Output: Features + MoSCoW"]
    
    P4["📤 Phase 4: Synthesis<br/>Input: selected ideas/features<br/>Process: Merge & consolidate<br/>Output: 1 unified concept"]
    
    P5["💬 Phase 5: Refinement<br/>Input: synthesis concept<br/>Process: Q&A (user answers)<br/>Output: Enriched spec"]
    
    P6["🎨 Phase 6: Visual<br/>Input: refined concept<br/>Process: DALL-E 3 generation<br/>Output: 4 visual options"]
    
    P7["📄 Phase 7: Artifacts<br/>Input: all phase data<br/>Process: Generate PRD, Tech Doc<br/>Output: PDF + Markdown"]

    P0 --> P1
    P1 --> P2
    P2 --> P3
    P3 --> P4
    P4 --> P5
    P5 --> P6
    P6 --> P7

    style P0 fill:#ffffcc
    style P1 fill:#ff9500,stroke:#fff,color:#fff
    style P2 fill:#ff9500,stroke:#fff,color:#fff
    style P3 fill:#ff9500,stroke:#fff,color:#fff
    style P4 fill:#512BD4,stroke:#fff,color:#fff
    style P5 fill:#0078D4,stroke:#fff,color:#fff
    style P6 fill:#ff9500,stroke:#fff,color:#fff
    style P7 fill:#107c10,stroke:#fff,color:#fff
```

---

## 💾 CRUD Operations by Entity

### Create, Read, Update, Delete Patterns

#### SESSION
```
Create  → SessionService.CreateSession(userId, appType, complexity)
           → Inserts to Sessions table
           
Read    → SessionService.GetSession(sessionId)
           → Queries Sessions table
           
Update  → SessionService.UpdatePhase(sessionId, newPhase)
           → Updates Sessions row
           
Delete  → SessionService.DeleteSession(sessionId)
          → Soft-delete + cascade delete all related data
```

#### IDEA
```
Create  → SparkService.GenerateIdeas(sessionId, scope)
           → Call GPT-4o x20
           → Inserts 20 rows to Ideas table
           
Read    → IdeaRepository.GetIdeasBySession(sessionId)
           → Queries Ideas table with sessionId partition
           
Update  → IdeaRepository.UpdateIdea(idea) [rarely done]
           
Delete  → Auto-deleted when session deleted
```

#### SWIPE
```
Create  → Save on every swipe action
           → Inserts to Swipes table
           → Updates Idea ranking via SwipeScore
           
Read    → SwipeRepository.GetSwipesForSession(sessionId)
           → Returns ranked list
           
Delete  → Auto-deleted when session deleted
```

#### SYNTHESIS
```
Create  → SynthesisService.CreateSynthesis(sessionId, selectedIds)
           → Calls SynthesisEngine
           → Inserts to Synthesis table
           
Read    → SynthesisRepository.GetSynthesis(sessionId)
           
Update  → Update on refinement answers
           
Delete  → Deleted with session
```

#### ARTIFACT
```
Create  → ArtifactService.GenerateArtifacts(sessionId)
           → Call GPT-4o for content
           → Insert metadata to Artifacts table
           → Upload PDF/MD to Blob Storage
           
Read    → List artifacts by session
           → Download from Blob Storage
           
Delete  → Deleted with session
           → Blob file retention per policy
```

---

## 🚀 Async Pipeline Architecture

### Non-Blocking Long-Running Operations

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
sequenceDiagram
    participant Browser as 🖥️ Browser
    participant SignalR as 🔄 SignalR Hub
    participant Service as ⚙️ Service
    participant Queue as 📋 Background Queue
    participant OpenAI as 🤖 OpenAI
    participant Storage as 💾 Storage

    Browser->>SignalR: Start generation (async)
    SignalR->>Service: Trigger GenerateIdeasAsync
    Service->>Queue: Enqueue job
    Service->>SignalR: Return immediately
    SignalR->>Browser: "Starting..." (non-blocking!)
    
    par Background Work
        Queue->>OpenAI: Request ideas
        OpenAI->>Queue: Stream responses
    and Real-time Updates
        Queue->>SignalR: Progress 25% done
        SignalR->>Browser: Update UI
        Queue->>SignalR: Progress 50% done
        SignalR->>Browser: Update UI
    end
    
    Queue->>Storage: Save completed ideas
    Storage->>Queue: ✅ Saved
    Queue->>SignalR: Notify complete
    SignalR->>Browser: Display results ✅
```

---

## 📝 Data Validation Pipeline

### Input Validation & Sanitization

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart TD
    A["📥 Input Data<br/>from Browser"]
    
    B["✓ Model Validation<br/>(Type checking)"]
    B1{{"Valid?"}}
    
    C["🔍 FluentValidation<br/>(Business rules)"]
    C1{{"Valid?"}}
    
    D["🧹 Sanitize<br/>(XSS prevention)"]
    
    E["🔐 Authorize<br/>(Permission check)"]
    E1{{"Authorized?"}}
    
    F["✅ Store<br/>to Database"]
    
    G["❌ Return Error<br/>to User"]

    A --> B
    B --> B1
    B1 -->|"No"| G
    B1 -->|"Yes"| C
    C --> C1
    C1 -->|"No"| G
    C1 -->|"Yes"| D
    D --> E
    E --> E1
    E1 -->|"No"| G
    E1 -->|"Yes"| F

    style A fill:#d4e8ff
    style B fill:#ffffcc
    style C fill:#ffffcc
    style D fill:#ffffcc
    style E fill:#ffe5e5
    style F fill:#107c10,stroke:#fff,color:#fff
    style G fill:#ff4444,stroke:#fff,color:#fff
```

---

## 🔄 Data Consistency & Transactions

### Ensuring Data Integrity

| Scenario | Strategy | Guarantees |
|----------|----------|-----------|
| **Create Session** | Atomic insert | All-or-nothing |
| **Save Swipe** | Row-level transaction | Single swipe persists |
| **Aggregate Synthesis** | Multi-row update | Consistent merge |
| **Delete Session** | Cascade delete with soft-delete | GDPR compliance |
| **Download Artifacts** | Read-after-write consistency | User gets fresh data |

---

## 📊 Data Volume Estimates

### Expected Data Sizes

```
Per Complete Session:
  ├── Session metadata: ~2 KB
  ├── 20 Ideas × 500 B each: ~10 KB
  ├── 100 Swipes × 200 B each: ~20 KB
  ├── 9 Mutations × 800 B each: ~7 KB
  ├── 50 Features × 600 B each: ~30 KB
  ├── Synthesis + Answers: ~15 KB
  ├── Visual Asset (PNG): ~2-3 MB
  └── Artifacts (PDFs): ~500 KB
  ─────────────────────────
  Total per session: ~3 MB

Projection (1,000 sessions/month):
  Monthly: 3 GB
  Yearly: 36 GB
  Queries: ~10M/month
  Capacity: ✅ Well within budget
```

---

## 🔐 Data Privacy in Pipeline

### GDPR & Privacy Compliance

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
graph TD
    A["User Data<br/>Enters System"]
    B["🔐 Encrypt at Rest<br/>(Azure encryption)"]
    C["🔒 Encrypt in Transit<br/>(HTTPS/TLS)"]
    D["⚠️ Minimal Collection<br/>(Only what needed)"]
    E["🗑️ Data Retention<br/>Enforcement"]
    F["🛡️ Access Control<br/>(Auth + RBAC)"]
    G["📋 Audit Log<br/>(Who accessed what)"]
    
    H["User Requests<br/>Deletion"]
    I["🧹 Sanitize PII<br/>(Anonymize)"]
    J["🗑️ Soft Delete<br/>(Compliance)"]

    A --> B
    A --> C
    A --> D
    B --> E
    C --> F
    E --> G
    
    H --> I
    I --> J

    style A fill:#d4e8ff
    style B fill:#ffe5e5
    style C fill:#ffe5e5
    style F fill:#ffe5e5
    style J fill:#ff4444,stroke:#fff,color:#fff
```

---

## 📈 Monitoring Data Pipeline Health

### Observability & Metrics

| Metric | Target | Alert Threshold |
|--------|--------|---|
| **Pipeline Latency** | <30s for AI requests | >60s |
| **Storage Write Latency** | <100ms | >500ms |
| **Data Validation Success Rate** | >99.9% | <99% |
| **Cache Hit Rate** | >80% | <60% |
| **Error Rate** | <0.1% | >0.5% |
| **PII Exposure** | 0 incidents | 1+ = incident |

Each metric is tracked in ApplicationInsights and alerted via Azure Monitor.

