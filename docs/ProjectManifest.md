# PoAppIdea Project Manifest

> **Version:** 1.0  
> **Last Updated:** 2026-02-12

---

## 📁 Documentation Inventory

### Architecture & Visualization

| Document | Type | Purpose | Location |
|----------|------|---------|----------|
| **Architecture.mmd** | Mermaid | System context + container architecture | `docs/Architecture.mmd` |
| **ApplicationFlow.mmd** | Mermaid | Auth flow + user journey | `docs/ApplicationFlow.mmd` |
| **DataModel.mmd** | Mermaid | Database schema + state transitions | `docs/DataModel.mmd` |
| **ComponentMap.mmd** | Mermaid | Component tree + service dependencies | `docs/ComponentMap.mmd` |
| **DataPipeline.mmd** | Mermaid | Data workflow + user workflow | `docs/DataPipeline.mmd` |

### Product & Technical Specs

| Document | Type | Purpose | Location |
|----------|------|---------|----------|
| **ProductSpec.md** | Markdown | PRD + success metrics | `docs/ProductSpec.md` |
| **ApiContract.md** | Markdown | API specs + error handling | `docs/ApiContract.md` |
| **DevOps.md** | Markdown | Deployment + CI/CD | `docs/DevOps.md` |
| **LocalSetup.md** | Markdown | Onboarding guide | `docs/LocalSetup.md` |
| **ProjectManifest.md** | Markdown | This file - asset inventory | `docs/ProjectManifest.md` |

---

## 🎯 Documentation Purpose

### For Human Developers
- Quick onboarding for new team members
- Understanding system architecture
- Troubleshooting issues
- Contributing to codebase

### For AI Agents
- Context for code generation
- Understanding data models
- API contract reference
- Deployment procedures

---

## 📊 Document Summaries

### Architecture.mmd
- **Summary:** High-level system architecture showing Azure hosting, Blazor frontend, AI services, and storage
- **Key Diagrams:** Container diagram, C4 context, simplified flow
- **Audience:** Architects, developers, DevOps

### ApplicationFlow.mmd
- **Summary:** Authentication flow and user journey through 7 phases
- **Key Diagrams:** OAuth sequence, state machine, security flow
- **Audience:** Developers, product managers

### DataModel.mmd
- **Summary:** Entity relationships and database schema for Azure Table Storage
- **Key Diagrams:** ERD, state transitions, storage schema
- **Audience:** Backend developers, DBAs

### ComponentMap.mmd
- **Summary:** Blazor components, services, and external dependencies
- **Key Diagrams:** Component tree, service dependencies
- **Audience:** Frontend developers, full-stack devs

### DataPipeline.mmd
- **Summary:** Data flow from user input through AI processing to storage
- **Key Diagrams:** CRUD operations, workflow pipeline
- **Audience:** Backend developers, architects

### ProductSpec.md
- **Summary:** Product requirements, success metrics, feature list
- **Key Sections:** Vision, audience, metrics, roadmap
- **Audience:** Product managers, stakeholders

### ApiContract.md
- **Summary:** REST API endpoints, request/response formats, error codes
- **Key Sections:** Endpoints, authentication, rate limiting
- **Audience:** Frontend devs, API consumers

### DevOps.md
- **Summary:** CI/CD pipeline, environment config, monitoring
- **Key Sections:** GitHub Actions, Key Vault, Application Insights
- **Audience:** DevOps engineers, developers

### LocalSetup.md
- **Summary:** Step-by-step local development setup
- **Key Sections:** Prerequisites, configuration, troubleshooting
- **Audience:** New developers, contributors

---

## 🔗 Cross-References

### Documentation Dependencies
```
README.md
    ├── Architecture.mmd
    ├── ApplicationFlow.mmd
    ├── DataModel.mmd
    ├── ComponentMap.mmd
    ├── DataPipeline.mmd
    ├── ProductSpec.md
    ├── ApiContract.md
    ├── DevOps.md
    ├── LocalSetup.md
    └── ProjectManifest.md
```

### Related Files
- `infra/main.bicep` - Infrastructure as Code
- `src/PoAppIdea.Web/Program.cs` - Application entry point
- `src/PoAppIdea.Core/Entities/` - Domain entities
- `.github/workflows/` - CI/CD workflows

---

## 📋 Simplified Manifest

### Quick Reference

| Need | Read |
|------|------|
| How it works | Architecture.mmd |
| User flow | ApplicationFlow.mmd |
| Data structure | DataModel.mmd |
| Components | ComponentMap.mmd |
| APIs | ApiContract.md |
| Deploy | DevOps.md |
| Run locally | LocalSetup.md |
| Product details | ProductSpec.md |

### File List (docs folder)
```
docs/
├── Architecture.mmd      # System architecture
├── ApplicationFlow.mmd   # User journey
├── DataModel.mmd         # Database schema
├── ComponentMap.mmd      # Component tree
├── DataPipeline.mmd      # Data workflows
├── ProductSpec.md        # PRD
├── ApiContract.md        # API spec
├── DevOps.md             # Deployment
├── LocalSetup.md         # Onboarding
└── ProjectManifest.md    # This file
```
