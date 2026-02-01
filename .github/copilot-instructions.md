General Engineering Principles
Unified Identity: Use Po{SolutionName} as the master prefix for all namespaces, Azure Resource Groups, and Aspire resource names (e.g., PoTask1.API, rg-PoTask1-prod).
Global Cleanup: Actively delete unused files, dead code, or obsolete assets. Maintain a "zero-waste" codebase.
Directory.Build.props: Enforce <TreatWarningsAsErrors>true</TreatWarningsAsErrors> and <Nullable>enable</Nullable> at the repository root to ensure all projects inherit strict safety standards.
Have /health endpoint on the server project that checks connections to all APIs and databases used
Context Management: Maintain a .copilotignore file to exclude bin/, obj/, and node_modules/, keeping AI focus on source logic.
Modern Tooling: Use Context7 MCP to fetch the latest SDKs and NuGet versions, ensuring the AI agent is working with up-to-date documentation.
Package Management: Use Central Package Management (CPM) via Directory.Packages.props with transitive pinning enabled.
Local Development: Primary secrets reside in dotnet user-secrets with the key vault in PoShared resource group as a backup
Cloud/Production: Use Azure Key Vault via Managed Identity.
When deploying services to Azure make sure you use this subscription Punkouter26 Bbb8dfbe-9169-432f-9b7a-fbf861b51037
Create .http files that can be helpful for debugging API endpoints (https://learn.microsoft.com/en-us/aspnet/core/test/http-files?view=aspnetcore-10.0)
Look in the Azure resource group PoShared for services to use
Looking into the Key Vault service contained in the resource group PoShared for Secrets and Keys 
Create debug logs in the browser console and server to provide enough information during function calls to be helpful when debugging
Use Gof Patterns and SOLID / Leave comments when these patterns are used and why they are used
For server app, Use port 5000 (http) and port 5001 (https)


Create a set of 3 test project 
.NET Unit Tests (C#): Focus on pure logic and domain rules (High speed).
.NET Integration Tests (C#): Target the API and Database. Use Testcontainers to spin up ephemeral SQL/Redis instances to verify real-world behavior.
Playwright E2E headless (TypeScript) (Chromium and mobile only):
Scope: Critical user paths only.
Constraints: Limit rendering to Chromium and Mobile.
Workflow: Run headed during development to verify functionality alongside the local server.














App Stack: Blazor Web App (Azure App Service)
Template: Target .NET 10 Unified Blazor Web App (Server SSR + WASM Client).
Vertical Slice Architecture (VSA): Organized by features, not layers. Group Endpoints, DTOs, and Logic within single, flattened feature folders.
Use OpenApi instead of Swashbuckle for API endpoint UI
Code will be deployed to a Azure App Service so offer suggestions to take advantage of App Service features
Telemetry: Enable OpenTelemetry (Logs, Traces, Metrics) globally, aggregating into Application Insights within the PoShared resource group.
PoShared will contain all other services needed and will have a key vault service for saving secrets as needed
