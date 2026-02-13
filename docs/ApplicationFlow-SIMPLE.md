# PoAppIdea Application Flow (Simplified)

> **Version:** 1.0 (Simplified for Quick Reference)  
> **Last Updated:** 2026-02-12  
> **Audience:** New developers, QA, stakeholders

---

## 🔐 How Login Works

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart LR
    A["👤 You<br/>Click Login"]
    B["🔑 Choose Provider<br/>(Google/GitHub)"]
    C["✅ Enter Your<br/>Credentials"]
    D["🚀 Welcome!<br/>You're In"]

    A --> B --> C --> D

    style A fill:#d4e8ff
    style B fill:#512BD4,stroke:#fff,color:#fff
    style C fill:#ffe5cc
    style D fill:#107c10,stroke:#fff,color:#fff
```

---

## 📋 The 7-Phase Journey

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
graph LR
    A["⚙️ Setup<br/>Choose app type<br/>& complexity"]
    B["⚡ Swipe<br/>Rate 20 ideas<br/>Pick top 3"]
    C["🧬 Evolve<br/>Rate 9 mutations<br/>Keep best"]
    D["🎯 Features<br/>Pick important<br/>features"]
    E["💬 Refine<br/>Answer questions<br/>about your idea"]
    F["🎨 Image<br/>AI creates<br/>visual"]
    G["📄 Download<br/>Get PRD + Docs<br/>+ Images"]

    A --> B --> C --> D --> E --> F --> G

    style A fill:#ffe5cc
    style B fill:#ff9500,stroke:#fff,color:#fff
    style C fill:#ff9500,stroke:#fff,color:#fff
    style D fill:#0078D4,stroke:#fff,color:#fff
    style E fill:#0078D4,stroke:#fff,color:#fff
    style F fill:#ff9500,stroke:#fff,color:#fff
    style G fill:#107c10,stroke:#fff,color:#fff
```

---

## 🎯 What Happens in Each Phase

| Phase | What You Do | How Many? | Outcome |
|-------|-----------|----------|---------|
| ⚙️ **Setup** | Tell us your app type | 1 choice | Session starts |
| ⚡ **Spark** | Swipe on AI ideas | 20 ideas | Pick top 3 |
| 🧬 **Evolve** | Rate AI mutations | 9 concepts | Keep best ones |
| 🎯 **Features** | Choose key features | 50 features | Organize into buckets |
| 💬 **Refine** | Answer about your idea | 10 questions | AI understands your vision |
| 🎨 **Image** | Pick image style | 4 options | Visual representation |
| 📄 **Download** | Get your documents | 3 formats | PRD, Docs, Web pack |

---

## 🧬 Idea Evolution (Simplified)

### How AI Helps Your Ideas Grow

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart TD
    A["Your App Type<br/>+ Complexity"]
    B["🤖 AI generates 20 ideas"]
    C["👤 You swipe & pick 3"]
    D["🤖 AI mutates your 3<br/>Creates 9 variations"]
    E["👤 You rate mutations"]
    F["🤖 AI expands into<br/>50 detailed features"]
    G["👤 You pick important ones"]
    H["🤖 AI merges everything<br/>into 1 final product"]

    A --> B --> C --> D --> E --> F --> G --> H

    style B fill:#ff9500,stroke:#fff,color:#fff
    style C fill:#107c10,stroke:#fff,color:#fff
    style D fill:#ff9500,stroke:#fff,color:#fff
    style E fill:#107c10,stroke:#fff,color:#fff
    style F fill:#ff9500,stroke:#fff,color:#fff
    style G fill:#107c10,stroke:#fff,color:#fff
    style H fill:#512BD4,stroke:#fff,color:#fff
```

---

## 📊 Real-Time Updates

### Live Progress as AI Works

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart LR
    A["You Click<br/>'Generate'"]
    B["⏳ Generating...<br/>25%"]
    C["⏳ Generating...<br/>50%"]
    D["⏳ Generating...<br/>75%"]
    E["✅ Done!<br/>See Results"]

    A --> B --> C --> D --> E

    style A fill:#d4e8ff
    style B fill:#ffffcc
    style C fill:#ffffcc
    style D fill:#ffffcc
    style E fill:#107c10,stroke:#fff,color:#fff
```

**You don't wait in the dark—you see updates as AI works!**

---

## 💥 What If Something Goes Wrong?

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'primaryColor': '#512BD4', 'primaryTextColor': '#fff', 'lineColor': '#666'}}}%%
flowchart TD
    A["❌ Error<br/>Happens"]
    B{"What Kind?"}
    C["📝 Your Form<br/>Has Mistakes"]
    D["🔐 You're Not<br/>Logged In"]
    E["⚠️ Server<br/>Error"]
    
    C --> C1["Fix your form<br/>& try again"]
    D --> D1["Log in again"]
    E --> E1["Refresh and retry<br/>or contact support"]
    
    A --> B
    B -->|Input Problem| C
    B -->|Login Problem| D
    B -->|Server Problem| E

    style A fill:#ff4444,stroke:#fff,color:#fff
    style C fill:#ffe5e5
    style D fill:#ffe5e5
    style E fill:#ffe5e5
    style C1 fill:#d4e8ff
    style D1 fill:#d4e8ff
    style E1 fill:#d4e8ff
```

**Built-in error handling means you get helpful messages, not cryptic errors.**

