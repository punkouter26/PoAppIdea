# PoAppIdea Application Flow

> **Version:** 2.0 (Enhanced)  
> **Last Updated:** 2026-02-12  
> **Audience:** Frontend developers, product managers, QA

---

## 🔐 Authentication Flow

### Multi-Provider OAuth 2.0 Implementation

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'primaryBorderColor': '#512BD4', 'lineColor': '#666'}}}%%
sequenceDiagram
    participant Browser as 🖥️ Browser
    participant App as 🚀 PoAppIdea App
    participant Provider as 🔑 OAuth Provider<br/>(Google/GitHub/MS)
    participant KeyVault as 🗝️ Key Vault
    participant Storage as 💾 Database

    Browser->>App: Click Login Button
    App->>Provider: Redirect to Provider OAuth
    Provider->>Browser: Show Login Form
    Browser->>Provider: Enter Credentials
    Provider->>Browser: Request Authorization
    Browser->>Provider: Grant Permission
    Provider->>App: Return Auth Code
    App->>KeyVault: Get Client Secret
    App->>Provider: Exchange Code + Secret for Token
    Provider->>App: Return JWT Token
    App->>Storage: Create/Load User Profile
    App->>Browser: Set Session Cookie + JWT
    Browser->>App: Authenticated ✅
```

---

## 📋 Session Lifecycle

### From Creation to Completion

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
stateDiagram-v2
    [*] --> Phase0_Scope: Create Session
    
    Phase0_Scope --> Phase1_Spark: Confirm Scope
    note right of Phase0_Scope
        - App Type Selection
        - Complexity Level (1-5)
        - Session Created
    end note
    
    Phase1_Spark --> Phase2_Mutation: Select Top 3
    note right of Phase1_Spark
        - Generate 20 Ideas
        - Swipe Interface
        - Rank by Speed & Direction
    end note
    
    Phase2_Mutation --> Phase3_Features: Rate Mutations
    note right of Phase2_Mutation
        - Generate 9 Mutations
        - Rate 1-5 Stars
        - Keep Top Mutations
    end note
    
    Phase3_Features --> Phase4_Submission: Confirm Features
    note right of Phase3_Features
        - Generate 50 Features
        - MoSCoW Prioritization
        - Category Organization
    end note
    
    Phase4_Submission --> Phase5_PM_Refinement: Select Ideas
    note right of Phase4_Submission
        - Merge Selected Ideas
        - Create Synthesis
    end note
    
    Phase5_PM_Refinement --> Phase6_Tech_Refinement: Answer PM Questions
    note right of Phase5_PM_Refinement
        - 5-10 PM Questions
        - Dynamic Q&A
    end note
    
    Phase6_Tech_Refinement --> Phase7_Visual: Answer Tech Questions
    note right of Phase6_Tech_Refinement
        - 5-10 Tech Questions
        - Architect Focus
    end note
    
    Phase7_Visual --> Phase8_Artifacts: Generate Image
    note right of Phase7_Visual
        - DALL-E 3 Generation
        - Multiple Styles
        - User Selection
    end note
    
    Phase8_Artifacts --> Completed: Generate Artifacts
    note right of Phase8_Artifacts
        - PRD Document
        - Technical Deep Dive
        - Visual Asset Pack
    end note
    
    Completed --> [*]
```

---

## 👥 User Journey Map

### Complete User Experience Flow

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
journey
    title PoAppIdea User Journey
    
    section Discovery & Authentication
      Visit Homepage: 5: User
      Read About Platform: 4: User
      Understand Benefits: 4: User
      Click Login: 5: User
      Authenticate via OAuth: 4: User
    
    section Ideation Phase
      Set App Scope: 5: User
      Choose Complexity: 5: User
      Complete Setup: 4: User
      Begin Swiping: 5: User, AI
      Provide Feedback: 4: User
    
    section Synthesis Phase
      Rate Mutations: 4: User, AI
      Select Features: 4: User, AI
      Answer Questions: 4: User, AI
      See Synthesis: 5: User
    
    section Refinement Phase
      Answer PM Questions: 4: User
      Answer Tech Questions: 4: User
      Refine Responses: 3: User
      Provide Feedback: 4: User
    
    section Visualization Phase
      Generate Image: 5: User, AI
      Review Styles: 4: User
      Select Favorite: 5: User
    
    section Output & Sharing
      Generate Artifacts: 5: User, AI
      Download PRD: 5: User
      Download Tech Docs: 4: User
      Share Session: 3: User
      Export to Gallery: 4: User
```

---

## 🎯 Page State Machine

### Navigation & Page Transitions

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart TD
    Start["🏠 Home"]
    Auth["🔑 Auth Check"]
    Authed["✅ Authenticated"]
    NotAuthed["❌ Not Authenticated"]
    Login["🔐 Login Page"]
    Scope["⚙️ Scope Page<br/>(Phase 0)"]
    Spark["⚡ Spark Page<br/>(Phase 1)"]
    Mutation["🧬 Mutation Page<br/>(Phase 2)"]
    Features["🎯 Features Page<br/>(Phase 3)"]
    Submit["📤 Submit Page<br/>(Phase 4)"]
    PMRefine["💬 PM Refinement<br/>(Phase 5)"]
    TechRefine["🔧 Tech Refinement<br/>(Phase 6)"]
    Visual["🎨 Visual Page<br/>(Phase 7)"]
    Artifacts["📄 Artifacts Page<br/>(Phase 8)"]
    Gallery["🖼️ Gallery"]
    Sessions["📋 Sessions"]
    History["⏰ History"]

    Start --> Auth
    Auth -->|"Has Token"| Authed
    Auth -->|"No Token"| NotAuthed
    NotAuthed --> Login
    Login -->|"Success"| Authed
    Authed --> Sessions
    Authed --> Gallery
    Sessions --> Scope
    Scope -->|"Next"| Spark
    Spark -->|"Next"| Mutation
    Mutation -->|"Next"| Features
    Features -->|"Next"| Submit
    Submit -->|"Next"| PMRefine
    PMRefine -->|"Next"| TechRefine
    TechRefine -->|"Next"| Visual
    Visual -->|"Next"| Artifacts
    Artifacts -->|"Complete"| History
    Artifacts -->|"Share"| Gallery

    style Start fill:#d4e8ff
    style Auth fill:#ffffcc
    style Authed fill:#107c10,stroke:#fff,color:#fff
    style NotAuthed fill:#ff4444,stroke:#fff,color:#fff
    style Login fill:#0078D4,stroke:#fff,color:#fff
    style Artifacts fill:#512BD4,stroke:#fff,color:#fff
    style Gallery fill:#50e6ff,stroke:#000,color:#000
```

---

## 🧬 Idea Generation & Mutation Pipeline

### AI-Powered Concept Evolution

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
graph TB
    Start["👤 User Input<br/>AppType + Complexity"]
    
    Spark["⚡ Spark Phase<br/>Generate 20 Ideas<br/>(GPT-4o)"]
    SwipeInterface["🎨 Swipe Interface<br/>(Tinder-style)"]
    SparkRate["📊 Ranking<br/>(Speed + Direction)"]
    SelectTop3["🏆 Select Top 3<br/>User Favorites"]
    
    Mutation["🧬 Mutation Phase<br/>Generate 9 Mutations<br/>(From Top 3)"]
    MutationTypes["📝 Mutation Types<br/>- Recombine<br/>- Enhance<br/>- Simplify"]
    RateMutations["⭐ Rate Mutations<br/>(1-5 Stars)"]
    SelectMutations["🎯 Keep Top Rated"]
    
    Features["🎯 Feature Expansion<br/>Generate 50 Features<br/>(From Selections)"]
    MoSCoW["📋 MoSCoW Priority<br/>- Must Have<br/>- Should Have<br/>- Could Have<br/>- Won't Have"]
    Categories["🗂️ Categorize<br/>(UI, Backend, etc)"]
    SelectFeatures["✅ Confirm Features"]

    Start --> Spark
    Spark --> SwipeInterface
    SwipeInterface --> SparkRate
    SparkRate --> SelectTop3
    SelectTop3 --> Mutation
    
    Mutation --> MutationTypes
    MutationTypes --> RateMutations
    RateMutations --> SelectMutations
    SelectMutations --> Features
    
    Features --> MoSCoW
    MoSCoW --> Categories
    Categories --> SelectFeatures
    
    SelectFeatures --> End["📤 Submit for Synthesis"]

    style Spark fill:#ff9500,stroke:#fff,color:#fff
    style SwipeInterface fill:#0078D4,stroke:#fff,color:#fff
    style Mutation fill:#ff9500,stroke:#fff,color:#fff
    style Features fill:#ff9500,stroke:#fff,color:#fff
    style SelectTop3 fill:#107c10,stroke:#fff,color:#fff
    style SelectMutations fill:#107c10,stroke:#fff,color:#fff
    style SelectFeatures fill:#107c10,stroke:#fff,color:#fff
    style End fill:#512BD4,stroke:#fff,color:#fff
```

---

## 💡 Synthesis & Refinement Flow

### From Selection to Final Document

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
graph TB
    Selected["🏆 Selected Ideas<br/>+ Features + Mutations"]
    
    Synthesis["🔗 Synthesis Phase<br/>(Merge selected concepts)"]
    SynthesisEngine["⚙️ Synthesis Engine<br/>- Create unified vision<br/>- Define target audience<br/>- Extract unique value props"]
    SynthesisOutput["📄 Synthesis Output<br/>- Merged Concept<br/>- Vision Statement<br/>- Target Audience"]
    
    PMQuestions["💬 PM Refinement<br/>(5-10 dynamic questions)<br/>e.g., Business Model, GTM"]
    PMAnswers["📝 User Answers PM Q's"]
    
    TechQuestions["🔧 Tech Refinement<br/>(5-10 dynamic questions)<br/>e.g., Architecture, Stack"]
    TechAnswers["📝 User Answers Tech Q's"]
    
    Visual["🎨 Visual Generation<br/>(DALL-E 3)"]
    StyleOptions["🎭 Style Selection<br/>- Modern<br/>- Minimalist<br/>- Futuristic<br/>- Professional"]
    SelectImage["✅ User Selects<br/>Preferred Image"]
    
    Artifacts["📦 Artifact Generation<br/>(GPT-4o + Semantic Kernel)"]
    PRD["📋 Product Spec (PRD)<br/>- Features<br/>- Success Metrics<br/>- Roadmap"]
    TechDoc["🔧 Technical Deep Dive<br/>- Architecture<br/>- Tech Stack<br/>- Implementation Plan"]
    VisualPack["📁 Visual Asset Pack<br/>- High-res Images<br/>- Color Palette<br/>- Asset Manifest"]
    
    Download["📥 Download Artifacts<br/>- PDF Format<br/>- Markdown Format<br/>- ZIP Package"]
    Share["🌐 Share to Gallery<br/>(Optional)"]

    Selected --> Synthesis
    Synthesis --> SynthesisEngine
    SynthesisEngine --> SynthesisOutput
    SynthesisOutput --> PMQuestions
    PMQuestions --> PMAnswers
    PMAnswers --> TechQuestions
    TechQuestions --> TechAnswers
    TechAnswers --> Visual
    Visual --> StyleOptions
    StyleOptions --> SelectImage
    SelectImage --> Artifacts
    
    Artifacts --> PRD
    Artifacts --> TechDoc
    Artifacts --> VisualPack
    
    PRD --> Download
    TechDoc --> Download
    VisualPack --> Download
    Download --> Share

    style Synthesis fill:#512BD4,stroke:#fff,color:#fff
    style PMQuestions fill:#0078D4,stroke:#fff,color:#fff
    style TechQuestions fill:#0078D4,stroke:#fff,color:#fff
    style Visual fill:#ff9500,stroke:#fff,color:#fff
    style Artifacts fill:#ff9500,stroke:#fff,color:#fff
    style Download fill:#107c10,stroke:#fff,color:#fff
    style Share fill:#50e6ff,stroke:#000,color:#000
```

---

## 🔄 SignalR Real-Time Updates

### How the App Stays Responsive During Long Operations

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
sequenceDiagram
    participant Browser as 🖥️ Browser
    participant SignalR as 🔄 SignalR Hub
    participant Service as ⚙️ Backend Service
    participant OpenAI as 🤖 Azure OpenAI

    Browser->>SignalR: Start Idea Generation
    SignalR->>Service: InvokeAsync GenerateIdeas
    Service->>OpenAI: Request 20 Ideas (takes 5-15s)
    
    par Real-time Updates
        Service->>SignalR: "Generating 5 ideas..."
        SignalR->>Browser: Update Progress: 25%
    and OpenAI Processing
        OpenAI->>Service: Stream Ideas
    end
    
    Service->>SignalR: "Generating 10 ideas..."
    SignalR->>Browser: Update Progress: 50%
    
    Service->>SignalR: "Generating 15 ideas..."
    SignalR->>Browser: Update Progress: 75%
    
    OpenAI->>Service: ✅ Complete: [Idea Objects]
    Service->>SignalR: "Generation Complete"
    SignalR->>Browser: Display 20 Ideas ✅
```

---

## 🎛️ Feature Flags & Experimentation

### Enabling Features, A/B Testing, Feature Gates

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart TD
    Request["User Request"]
    FeatureCheck["Check Feature Flags<br/>(appsettings.json)"]
    
    FeatureEnabled["✅ Feature<br/>Enabled?"]
    ShowNewUI["Show New UI<br/>Version"]
    ShowOldUI["Show Old UI<br/>Version"]
    
    UserSegment["User Segment?"]
    BetaUser["🔬 Beta User<br/>(Internal)"]
    Production["👥 Production<br/>(External)"]
    
    LogMetric["📊 Log to<br/>App Insights"]
    
    Request --> FeatureCheck
    FeatureCheck --> FeatureEnabled
    FeatureEnabled -->|Yes| ShowNewUI
    FeatureEnabled -->|No| ShowOldUI
    ShowNewUI --> UserSegment
    UserSegment -->|Internal| BetaUser
    UserSegment -->|External| Production
    BetaUser --> LogMetric
    Production --> LogMetric

    style Request fill:#d4e8ff
    style FeatureCheck fill:#ffffcc
    style FeatureEnabled fill:#ffffcc
    style ShowNewUI fill:#107c10,stroke:#fff,color:#fff
    style ShowOldUI fill:#0078D4,stroke:#fff,color:#fff
    style LogMetric fill:#512BD4,stroke:#fff,color:#fff
```

---

## 🚨 Error Handling & Recovery

### Graceful Error Management

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart TD
    Error["❌ Error Occurs"]
    Catch["Try-Catch<br/>Catches Exception"]
    LogError["📝 Log to<br/>App Insights"]
    UserFriendly["👤 Show User<br/>Friendly Message"]
    
    Type{{"Error Type?"}}
    
    Validation["❌ Validation Error<br/>(Input)"]
    ShowValidation["Display Validation<br/>Error on Form"]
    
    NotFound["❌ Not Found (404)"]
    ShowNotFound["Show 'Item Not Found'<br/>Suggest Navigation"]
    
    Unauthorized["❌ Unauthorized (401/403)"]
    Redirect["Redirect to<br/>Login Page"]
    
    ServerError["❌ Server Error (500)"]
    ShowRetry["Show 'Please Retry'<br/>with Retry Button"]
    
    Critical["❌ Critical Error"]
    ShowCritical["Log to Monitoring<br/>Alert Engineer"]

    Error --> Catch
    Catch --> LogError
    LogError --> Type
    
    Type -->|Input| Validation
    Type -->|Not Found| NotFound
    Type -->|Unauthorized| Unauthorized
    Type -->|Server| ServerError
    Type -->|Critical| Critical
    
    Validation --> ShowValidation
    NotFound --> ShowNotFound
    Unauthorized --> Redirect
    ServerError --> ShowRetry
    Critical --> ShowCritical
    
    ShowValidation --> UserFriendly
    ShowNotFound --> UserFriendly
    Redirect --> UserFriendly
    ShowRetry --> UserFriendly
    ShowCritical --> UserFriendly

    style Error fill:#ff4444,stroke:#fff,color:#fff
    style Catch fill:#ff9500,stroke:#fff,color:#fff
    style LogError fill:#fffaaa
    style UserFriendly fill:#0078D4,stroke:#fff,color:#fff
```

