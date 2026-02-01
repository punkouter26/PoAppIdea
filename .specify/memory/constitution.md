<!--
================================================================================
SYNC IMPACT REPORT
================================================================================
Version Change: 0.0.0 → 1.0.0 (MAJOR - Initial ratification)

Modified Principles: N/A (Initial creation)

Added Sections:
  - I. Unified Identity (naming conventions)
  - II. Zero-Waste Codebase (cleanup discipline)
  - III. Strict Compiler Safety (warnings as errors, nullable)
  - IV. Observability & Health (telemetry, health endpoints)
  - V. Secrets Management (user-secrets, Key Vault)
  - VI. Three-Tier Testing (Unit, Integration, E2E)
  - VII. Design Patterns (GoF, SOLID)
  - Technology Stack (Blazor, .NET 10, VSA, Azure)
  - Azure Deployment (subscription, resource groups, App Service)
  - Development Standards (ports, .http files, CPM)

Removed Sections: N/A (Initial creation)

Templates Requiring Updates:
  - plan-template.md ✅ Compatible (Constitution Check section exists)
  - spec-template.md ✅ Compatible (requirements align)
  - tasks-template.md ✅ Compatible (testing phases align with Three-Tier Testing)

Follow-up TODOs: None
================================================================================
-->

# PoAppIdea Constitution

## Core Principles

### I. Unified Identity

All project artifacts MUST use the `Po{SolutionName}` naming convention as the master prefix:

- **Namespaces**: `PoTask1.API`, `PoTask1.Core`, `PoTask1.Tests`
- **Azure Resource Groups**: `rg-PoTask1-prod`, `rg-PoTask1-dev`
- **Aspire Resources**: Prefixed consistently with `Po{SolutionName}`

**Rationale**: Unified naming eliminates ambiguity, enables quick resource identification across Azure subscriptions, and enforces organizational consistency.

### II. Zero-Waste Codebase

The codebase MUST maintain a "zero-waste" policy:

- Actively delete unused files, dead code, and obsolete assets
- Maintain `.copilotignore` to exclude `bin/`, `obj/`, and `node_modules/` from AI context
- Remove commented-out code blocks; use version control for history

**Rationale**: Clean codebases reduce cognitive load, improve build times, and keep AI agents focused on relevant source logic.

### III. Strict Compiler Safety

All projects MUST inherit strict compiler settings via `Directory.Build.props` at repository root:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<Nullable>enable</Nullable>
```

- No project may override these settings to weaken enforcement
- All warnings MUST be resolved, not suppressed without documented justification

**Rationale**: Treating warnings as errors catches bugs at compile time; nullable reference types prevent null reference exceptions.

### IV. Observability & Health

All services MUST implement comprehensive observability:

- **Health Endpoints**: Every server project MUST expose `/health` endpoint that checks connections to all APIs and databases
- **Telemetry**: OpenTelemetry (Logs, Traces, Metrics) MUST be enabled globally
- **Aggregation**: All telemetry MUST flow to Application Insights in the `PoShared` resource group
- **Debug Logging**: Browser console and server logs MUST provide sufficient information during function calls for debugging

**Rationale**: Observable systems enable rapid incident response and performance optimization.

### V. Secrets Management

Secrets MUST follow a tiered approach:

- **Local Development**: Primary secrets in `dotnet user-secrets`; Key Vault in `PoShared` as backup
- **Cloud/Production**: Azure Key Vault via Managed Identity exclusively
- **Never**: Secrets in source code, environment variables in plain text, or committed configuration files

**Rationale**: Layered secrets management balances developer experience with production security.

### VI. Three-Tier Testing (NON-NEGOTIABLE)

Every feature MUST have appropriate test coverage across three tiers:

1. **.NET Unit Tests (C#)**
   - Focus: Pure logic and domain rules
   - Characteristics: High speed, isolated, no external dependencies

2. **.NET Integration Tests (C#)**
   - Focus: API and Database interactions
   - Tools: Testcontainers for ephemeral SQL/Redis instances
   - Characteristics: Real-world behavior verification

3. **Playwright E2E Tests (TypeScript)**
   - Scope: Critical user paths only
   - Browsers: Chromium and Mobile only (no Firefox/WebKit)
   - Mode: Headless in CI; headed during local development

**Rationale**: Tiered testing provides fast feedback loops while ensuring real-world behavior validation.

### VII. Design Patterns & Architecture

Code MUST follow established design principles:

- **GoF Patterns**: Apply where appropriate; document pattern name and rationale in comments
- **SOLID Principles**: All classes MUST adhere to Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **Vertical Slice Architecture (VSA)**: Organize by features, not layers; group Endpoints, DTOs, and Logic within single flattened feature folders

**Rationale**: Established patterns reduce cognitive load and enable team scalability; VSA improves feature cohesion and reduces cross-cutting changes.

## Technology Stack

### Application Framework

- **Platform**: .NET 10 Unified Blazor Web App (Server SSR + WASM Client)
- **Hosting**: Azure App Service
- **API Documentation**: OpenAPI (not Swashbuckle)
- **Architecture**: Vertical Slice Architecture (VSA)

### Package & Dependency Management

- **Central Package Management (CPM)**: Via `Directory.Packages.props` with transitive pinning enabled
- **Modern Tooling**: Use Context7 MCP to fetch latest SDKs and NuGet versions
- **Debugging**: Create `.http` files for API endpoint debugging

### Development Ports

- **HTTP**: Port 5000
- **HTTPS**: Port 5001

## Azure Deployment

### Subscription & Resources

- **Subscription**: Punkouter26 (`bbb8dfbe-9169-432f-9b7a-fbf861b51037`)
- **Shared Services**: `PoShared` resource group contains:
  - Application Insights (telemetry aggregation)
  - Key Vault (secrets and keys)
  - Shared APIs and databases

### Deployment Target

- **Platform**: Azure App Service
- **Features**: Leverage App Service capabilities (deployment slots, auto-scaling, managed certificates)

## Development Standards

### File Organization

- **Feature Folders**: Group related code by feature, not by layer
- **.http Files**: Create for each API endpoint to aid debugging
- **Documentation**: Inline comments for pattern usage with rationale

### Context Management

```gitignore
# .copilotignore
bin/
obj/
node_modules/
```

## Governance

This constitution supersedes all other development practices for the PoAppIdea project:

1. **Compliance**: All PRs MUST verify adherence to these principles before merge
2. **Amendments**: Changes require documented justification, team approval, and migration plan for affected code
3. **Exceptions**: Deviations MUST be justified in code comments with ticket reference
4. **Review**: Constitution compliance checked at design review and code review stages

**Version**: 1.0.0 | **Ratified**: 2026-01-29 | **Last Amended**: 2026-01-29
