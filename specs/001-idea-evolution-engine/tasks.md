# Tasks: PoAppIdea - The Self-Evolving Ideation Engine

**Input**: Design documents from `/specs/001-idea-evolution-engine/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/openapi.yaml ✅, quickstart.md ✅

**Tests**: Not explicitly requested in feature specification. Test tasks are NOT included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Project structure, build configuration, and foundational files

- [X] T001 Create solution file at src/PoAppIdea.sln
- [X] T002 Create Directory.Build.props with TreatWarningsAsErrors, Nullable=enable, ImplicitUsings at src/Directory.Build.props
- [X] T003 [P] Create Directory.Packages.props with central package management at src/Directory.Packages.props
- [X] T004 [P] Create .copilotignore to exclude bin/obj/node_modules at src/.copilotignore
- [X] T005 [P] Create PoAppIdea.Core class library project at src/PoAppIdea.Core/PoAppIdea.Core.csproj
- [X] T006 [P] Create PoAppIdea.Shared class library project at src/PoAppIdea.Shared/PoAppIdea.Shared.csproj
- [X] T007 Create PoAppIdea.Web Blazor Web App project at src/PoAppIdea.Web/PoAppIdea.Web.csproj
- [X] T008 Add project references: Web→Core, Web→Shared, Shared→Core

**Checkpoint**: Solution structure ready - all projects build successfully

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Core Entities (PoAppIdea.Core)

- [X] T009 [P] Create AuthProvider enum in src/PoAppIdea.Core/Enums/AuthProvider.cs
- [X] T010 [P] Create AppType enum in src/PoAppIdea.Core/Enums/AppType.cs
- [X] T011 [P] Create SessionPhase enum in src/PoAppIdea.Core/Enums/SessionPhase.cs
- [X] T012 [P] Create SessionStatus enum in src/PoAppIdea.Core/Enums/SessionStatus.cs
- [X] T013 [P] Create SwipeDirection enum in src/PoAppIdea.Core/Enums/SwipeDirection.cs
- [X] T014 [P] Create SwipeSpeed enum in src/PoAppIdea.Core/Enums/SwipeSpeed.cs
- [X] T015 [P] Create MutationType enum in src/PoAppIdea.Core/Enums/MutationType.cs
- [X] T016 [P] Create FeaturePriority enum in src/PoAppIdea.Core/Enums/FeaturePriority.cs
- [X] T017 [P] Create RefinementPhase enum in src/PoAppIdea.Core/Enums/RefinementPhase.cs
- [X] T018 [P] Create ArtifactType enum in src/PoAppIdea.Core/Enums/ArtifactType.cs
- [X] T019 [P] Create User entity in src/PoAppIdea.Core/Entities/User.cs
- [X] T020 [P] Create ProductPersonality entity in src/PoAppIdea.Core/Entities/ProductPersonality.cs
- [X] T021 [P] Create Session entity in src/PoAppIdea.Core/Entities/Session.cs
- [X] T022 [P] Create Idea entity in src/PoAppIdea.Core/Entities/Idea.cs
- [X] T023 [P] Create Swipe entity in src/PoAppIdea.Core/Entities/Swipe.cs
- [X] T024 [P] Create Mutation entity in src/PoAppIdea.Core/Entities/Mutation.cs
- [X] T025 [P] Create FeatureVariation entity with nested Feature type in src/PoAppIdea.Core/Entities/FeatureVariation.cs
- [X] T026 [P] Create Synthesis entity in src/PoAppIdea.Core/Entities/Synthesis.cs
- [X] T027 [P] Create RefinementAnswer entity in src/PoAppIdea.Core/Entities/RefinementAnswer.cs
- [X] T028 [P] Create VisualAsset entity with nested StyleInfo type in src/PoAppIdea.Core/Entities/VisualAsset.cs
- [X] T029 [P] Create Artifact entity in src/PoAppIdea.Core/Entities/Artifact.cs

### Repository Interfaces (PoAppIdea.Core)

- [X] T030 [P] Create ISessionRepository interface in src/PoAppIdea.Core/Interfaces/ISessionRepository.cs
- [X] T031 [P] Create IPersonalityRepository interface in src/PoAppIdea.Core/Interfaces/IPersonalityRepository.cs
- [X] T032 [P] Create IArtifactRepository interface in src/PoAppIdea.Core/Interfaces/IArtifactRepository.cs
- [X] T033 [P] Create IIdeaRepository interface in src/PoAppIdea.Core/Interfaces/IIdeaRepository.cs
- [X] T034 [P] Create ISwipeRepository interface in src/PoAppIdea.Core/Interfaces/ISwipeRepository.cs

### Shared DTOs (PoAppIdea.Shared)

- [X] T035 [P] Create SessionDto in src/PoAppIdea.Shared/Contracts/SessionDto.cs
- [X] T036 [P] Create IdeaDto in src/PoAppIdea.Shared/Contracts/IdeaDto.cs
- [X] T037 [P] Create SwipeDto in src/PoAppIdea.Shared/Contracts/SwipeDto.cs
- [X] T038 [P] Create MutationDto in src/PoAppIdea.Shared/Contracts/MutationDto.cs
- [X] T039 [P] Create FeatureVariationDto in src/PoAppIdea.Shared/Contracts/FeatureVariationDto.cs
- [X] T040 [P] Create SynthesisDto in src/PoAppIdea.Shared/Contracts/SynthesisDto.cs
- [X] T041 [P] Create ArtifactDto in src/PoAppIdea.Shared/Contracts/ArtifactDto.cs
- [X] T042 [P] Create Constants class with app-wide constants in src/PoAppIdea.Shared/Constants/AppConstants.cs

### Infrastructure - Storage (PoAppIdea.Web)

- [X] T043 Create TableStorageClient for Azure Table Storage operations in src/PoAppIdea.Web/Infrastructure/Storage/TableStorageClient.cs
- [X] T044 [P] Create BlobStorageClient for Azure Blob Storage operations in src/PoAppIdea.Web/Infrastructure/Storage/BlobStorageClient.cs
- [X] T045 Implement SessionRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/SessionRepository.cs
- [X] T046 [P] Implement PersonalityRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/PersonalityRepository.cs
- [X] T047 [P] Implement IdeaRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/IdeaRepository.cs
- [X] T048 [P] Implement SwipeRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/SwipeRepository.cs
- [X] T049 [P] Implement ArtifactRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/ArtifactRepository.cs
- [X] T049B [P] Implement session state persistence with resumption support in SessionRepository (FR-024)

### Infrastructure - AI Orchestration (PoAppIdea.Web)

- [X] T050 Configure Semantic Kernel with Azure OpenAI GPT-4o in src/PoAppIdea.Web/Infrastructure/AI/SemanticKernelConfig.cs
- [X] T051 Create base IdeaGenerator service with prompt templates in src/PoAppIdea.Web/Infrastructure/AI/IdeaGenerator.cs

### Infrastructure - Resilience (PoAppIdea.Web)

- [X] T052 Create ResiliencePolicies with Polly (exponential backoff, circuit breaker) in src/PoAppIdea.Web/Infrastructure/Resilience/ResiliencePolicies.cs

### Infrastructure - Telemetry (PoAppIdea.Web)

- [X] T053 Configure OpenTelemetry with Application Insights in src/PoAppIdea.Web/Infrastructure/Telemetry/TelemetryConfiguration.cs

### Authentication (PoAppIdea.Web)

- [X] T054 Configure ASP.NET Core Identity with OAuth providers (Google, GitHub, Microsoft) in src/PoAppIdea.Web/Program.cs
- [X] T055 [P] Create authentication middleware and user claims handling in src/PoAppIdea.Web/Infrastructure/Auth/AuthConfiguration.cs

### Health Endpoint (PoAppIdea.Web)

- [X] T056 Create HealthEndpoint with dependency checks in src/PoAppIdea.Web/Features/Health/HealthEndpoint.cs

### Program.cs Configuration

- [X] T057 Configure Program.cs with all services, SignalR, OpenAPI, and middleware in src/PoAppIdea.Web/Program.cs
- [X] T058 [P] Create appsettings.json with configuration sections in src/PoAppIdea.Web/appsettings.json
- [X] T059 [P] Create appsettings.Development.json with local development settings in src/PoAppIdea.Web/appsettings.Development.json

### Blazor Layout & Components

- [X] T060 Create MainLayout.razor with navigation structure in src/PoAppIdea.Web/Components/Layout/MainLayout.razor
- [X] T061 [P] Create NavMenu.razor with phase navigation in src/PoAppIdea.Web/Components/Layout/NavMenu.razor
- [X] T062 [P] Create App.razor with routing configuration in src/PoAppIdea.Web/Components/App.razor

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Scope Configuration & Spark Discovery (Priority: P1) 🎯 MVP

**Goal**: Users can configure session scope (App Type, Complexity) and discover ideas via swipe interface with real-time learning

**Independent Test**: Complete a full 50-idea swipe session and verify that batch 2-5 show measurably different themes aligned with user's likes from batch 1

### Session Feature (Phase 0: Scope)

- [X] T063 [US1] Create StartSessionRequest DTO in src/PoAppIdea.Web/Features/Session/StartSessionRequest.cs
- [X] T064 [US1] Create StartSessionResponse DTO in src/PoAppIdea.Web/Features/Session/StartSessionResponse.cs
- [X] T065 [US1] Implement SessionService with scope configuration logic in src/PoAppIdea.Web/Features/Session/SessionService.cs
- [X] T066 [US1] Create StartSessionEndpoint (POST /api/sessions) in src/PoAppIdea.Web/Features/Session/StartSessionEndpoint.cs
- [X] T067 [US1] Create GetSessionEndpoint (GET /api/sessions/{id}) in src/PoAppIdea.Web/Features/Session/GetSessionEndpoint.cs
- [X] T067B [US1] Create ResumeSessionEndpoint (GET /api/sessions/{id}/resume) with phase state restoration in src/PoAppIdea.Web/Features/Session/ResumeSessionEndpoint.cs (FR-024)
- [X] T068 [P] [US1] Create ScopePage.razor for App Type and Complexity selection in src/PoAppIdea.Web/Components/Pages/ScopePage.razor

### Spark Feature (Phase 1: Idea Discovery)

- [X] T069 [US1] Create GenerateIdeasRequest DTO in src/PoAppIdea.Web/Features/Spark/GenerateIdeasRequest.cs
- [X] T070 [US1] Create GenerateIdeasResponse DTO with batch of 10 ideas in src/PoAppIdea.Web/Features/Spark/GenerateIdeasResponse.cs
- [X] T071 [US1] Implement SparkService with batch generation and learning logic in src/PoAppIdea.Web/Features/Spark/SparkService.cs
- [X] T072 [US1] Create GenerateIdeasEndpoint (POST /api/sessions/{id}/ideas) in src/PoAppIdea.Web/Features/Spark/GenerateIdeasEndpoint.cs
- [X] T073 [US1] Create RecordSwipeRequest DTO with direction and timestamp in src/PoAppIdea.Web/Features/Spark/RecordSwipeRequest.cs
- [X] T074 [US1] Create RecordSwipeEndpoint (POST /api/sessions/{id}/swipes) in src/PoAppIdea.Web/Features/Spark/RecordSwipeEndpoint.cs
- [X] T075 [US1] Create SwipeHub SignalR hub for real-time swipe capture in src/PoAppIdea.Web/Features/Spark/SwipeHub.cs
- [X] T076 [US1] Create GetTopIdeasEndpoint (GET /api/sessions/{id}/ideas/top) in src/PoAppIdea.Web/Features/Spark/GetTopIdeasEndpoint.cs
- [X] T077 [US1] Create SparkPage.razor with swipe interface (Radzen cards) in src/PoAppIdea.Web/Components/Pages/SparkPage.razor
- [X] T078 [US1] Create SwipeCard.razor component with animation in src/PoAppIdea.Web/Components/Shared/SwipeCard.razor
- [X] T079 [US1] Implement swipe speed tracking (fast <1s, hesitated >3s weighting) in SparkService

**Checkpoint**: User Story 1 complete - users can configure scope, swipe through 50 ideas in batches of 10, and see learning between batches

---

## Phase 4: User Story 2 - Mutation & Directed Evolution (Priority: P2)

**Goal**: Top 5 ideas undergo directed evolution with Crossover and Repurposing mutations

**Independent Test**: Verify that 50 mutations are meaningfully derived from top 5 ideas and user ratings correctly identify top 10 evolved concepts

### Mutation Feature

- [X] T080 [US2] Create MutateIdeasRequest DTO in src/PoAppIdea.Web/Features/Mutation/MutateIdeasRequest.cs
- [X] T081 [US2] Create MutateIdeasResponse DTO in src/PoAppIdea.Web/Features/Mutation/MutateIdeasResponse.cs
- [X] T082 [US2] Create MutationEngine with Crossover and Repurposing strategies in src/PoAppIdea.Web/Infrastructure/AI/MutationEngine.cs
- [X] T083 [US2] Implement MutationService with 10 mutations per idea logic in src/PoAppIdea.Web/Features/Mutation/MutationService.cs
- [X] T084 [US2] Create MutateIdeasEndpoint (POST /api/sessions/{id}/mutations) in src/PoAppIdea.Web/Features/Mutation/MutateIdeasEndpoint.cs
- [X] T085 [US2] Create GetMutationsEndpoint (GET /api/sessions/{id}/mutations) in src/PoAppIdea.Web/Features/Mutation/GetMutationsEndpoint.cs
- [X] T086 [US2] Create RateMutationRequest DTO in src/PoAppIdea.Web/Features/Mutation/RateMutationRequest.cs
- [X] T087 [US2] Create RateMutationEndpoint (POST /api/sessions/{id}/mutations/{mutationId}/rate) in src/PoAppIdea.Web/Features/Mutation/RateMutationEndpoint.cs
- [X] T088 [US2] Create GetTopMutationsEndpoint (GET /api/sessions/{id}/mutations/top) in src/PoAppIdea.Web/Features/Mutation/GetTopMutationsEndpoint.cs
- [X] T089 [P] [US2] Implement MutationRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/MutationRepository.cs
- [X] T090 [US2] Create MutationPage.razor with rating interface in src/PoAppIdea.Web/Components/Pages/MutationPage.razor
- [X] T091 [P] [US2] Create MutationCard.razor component showing mutation rationale in src/PoAppIdea.Web/Components/Shared/MutationCard.razor

**Checkpoint**: User Story 2 complete - top 5 ideas mutate into 50 variations, user rates them, top 10 identified

---

## Phase 5: User Story 3 - Feature Expansion & Service Integration (Priority: P3)

**Goal**: Top 10 evolved ideas receive 5 feature variations each with different service integrations

**Independent Test**: Verify each of 10 ideas has 5 feature variations with distinct integrations and ratings inform submission candidates

### FeatureExpansion Feature

- [X] T092 [US3] Create ExpandFeaturesRequest DTO in src/PoAppIdea.Web/Features/FeatureExpansion/ExpandFeaturesRequest.cs
- [X] T093 [US3] Create ExpandFeaturesResponse DTO in src/PoAppIdea.Web/Features/FeatureExpansion/ExpandFeaturesResponse.cs
- [X] T094 [US3] Implement FeatureExpansionService with variation generation in src/PoAppIdea.Web/Features/FeatureExpansion/FeatureExpansionService.cs
- [X] T095 [US3] Create ExpandFeaturesEndpoint (POST /api/sessions/{id}/features) in src/PoAppIdea.Web/Features/FeatureExpansion/ExpandFeaturesEndpoint.cs
- [X] T096 [US3] Create GetFeatureVariationsEndpoint (GET /api/sessions/{id}/features) in src/PoAppIdea.Web/Features/FeatureExpansion/GetFeatureVariationsEndpoint.cs
- [X] T097 [US3] Create RateFeatureVariationRequest DTO in src/PoAppIdea.Web/Features/FeatureExpansion/RateFeatureVariationRequest.cs
- [X] T098 [US3] Create RateFeatureVariationEndpoint (POST /api/sessions/{id}/features/{variationId}/rate) in src/PoAppIdea.Web/Features/FeatureExpansion/RateFeatureVariationEndpoint.cs
- [X] T099 [US3] Create GetTopFeaturesEndpoint (GET /api/sessions/{id}/features/top) in src/PoAppIdea.Web/Features/FeatureExpansion/GetTopFeaturesEndpoint.cs
- [X] T100 [P] [US3] Implement FeatureVariationRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/FeatureVariationRepository.cs
- [X] T101 [US3] Create FeatureExpansionPage.razor with feature comparison interface in src/PoAppIdea.Web/Components/Pages/FeatureExpansionPage.razor
- [X] T102 [P] [US3] Create FeatureCard.razor component with MoSCoW priorities in src/PoAppIdea.Web/Components/Shared/FeatureCard.razor

**Checkpoint**: User Story 3 complete - 50 feature variations generated, rated, Top 10 proto-apps identified

---

## Phase 6: User Story 4 - Submission & Cohesive Synthesis (Priority: P4)

**Goal**: Users select 1-10 ideas for submission; multiple selections trigger cohesive synthesis with thematic bridging

**Independent Test**: Select 2-3 distinct ideas and verify system produces coherent merged concept with clear thematic bridge

### Synthesis Feature

- [X] T103 [US4] Create SubmitSelectionsRequest DTO in src/PoAppIdea.Web/Features/Synthesis/SubmitSelectionsRequest.cs
- [X] T104 [US4] Create SubmitSelectionsResponse DTO in src/PoAppIdea.Web/Features/Synthesis/SubmitSelectionsResponse.cs
- [X] T105 [US4] Create SynthesisEngine with thematic bridging logic in src/PoAppIdea.Web/Infrastructure/AI/SynthesisEngine.cs
- [X] T106 [US4] Implement SynthesisService with single vs multi-select handling in src/PoAppIdea.Web/Features/Synthesis/SynthesisService.cs
- [X] T107 [US4] Create SubmitSelectionsEndpoint (POST /api/sessions/{id}/submit) in src/PoAppIdea.Web/Features/Synthesis/SubmitSelectionsEndpoint.cs
- [X] T108 [US4] Create SynthesizeEndpoint (POST /api/sessions/{id}/synthesize) in src/PoAppIdea.Web/Features/Synthesis/SynthesizeEndpoint.cs
- [X] T109 [US4] Create GetSynthesisEndpoint (GET /api/sessions/{id}/synthesis) in src/PoAppIdea.Web/Features/Synthesis/GetSynthesisEndpoint.cs
- [X] T110 [P] [US4] Implement SynthesisRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/SynthesisRepository.cs
- [X] T111 [US4] Create SubmissionPage.razor with multi-select and synthesis preview in src/PoAppIdea.Web/Components/Pages/SubmissionPage.razor
- [X] T112 [P] [US4] Create SynthesisPreview.razor component showing thematic bridge in src/PoAppIdea.Web/Components/Shared/SynthesisPreview.razor

**Checkpoint**: User Story 4 complete - users can select ideas and see cohesive synthesis with retained elements explanation

---

## Phase 7: User Story 5 - Deep Refinement via Interactive Inquiry (Priority: P5)

**Goal**: System asks 10 PM questions (Phase 4) and 10 Architect questions (Phase 5) to refine the concept

**Independent Test**: Verify 20 total questions asked across both phases and answers incorporated into final artifacts

### Refinement Feature

- [X] T113 [US5] Create GetQuestionsRequest DTO with phase parameter in src/PoAppIdea.Web/Features/Refinement/GetQuestionsRequest.cs
- [X] T114 [US5] Create GetQuestionsResponse DTO with question list in src/PoAppIdea.Web/Features/Refinement/GetQuestionsResponse.cs
- [X] T115 [US5] Create SubmitAnswersRequest DTO in src/PoAppIdea.Web/Features/Refinement/SubmitAnswersRequest.cs
- [X] T116 [US5] Create SubmitAnswersResponse DTO in src/PoAppIdea.Web/Features/Refinement/SubmitAnswersResponse.cs
- [X] T117 [US5] Implement RefinementService with PM and Architect question generation in src/PoAppIdea.Web/Features/Refinement/RefinementService.cs
- [X] T118 [US5] Create GetQuestionsEndpoint (GET /api/sessions/{id}/refinement/questions) in src/PoAppIdea.Web/Features/Refinement/GetQuestionsEndpoint.cs
- [X] T119 [US5] Create SubmitAnswersEndpoint (POST /api/sessions/{id}/refinement/answers) in src/PoAppIdea.Web/Features/Refinement/SubmitAnswersEndpoint.cs
- [X] T120 [US5] Create GetAnswersEndpoint (GET /api/sessions/{id}/refinement/answers) in src/PoAppIdea.Web/Features/Refinement/GetAnswersEndpoint.cs
- [X] T121 [P] [US5] Implement RefinementAnswerRepository using TableStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/RefinementAnswerRepository.cs
- [X] T122 [US5] Create RefinementPage.razor with question-answer interface in src/PoAppIdea.Web/Components/Pages/RefinementPage.razor
- [X] T123 [P] [US5] Create QuestionCard.razor component with category grouping in src/PoAppIdea.Web/Components/Shared/QuestionCard.razor

**Checkpoint**: User Story 5 complete - 20 clarifying questions answered and persisted for artifact generation

---

## Phase 8: User Story 6 - Aesthetic Vision & Visual Direction (Priority: P6)

**Goal**: System generates 10 visual mockups/mood boards via DALL-E; user selects preferred direction

**Independent Test**: Verify 10 distinct visual representations generated and selected option included in final artifacts

### Visual Feature

- [X] T124 [US6] Create GenerateVisualsRequest DTO in src/PoAppIdea.Web/Features/Visual/GenerateVisualsRequest.cs
- [X] T125 [US6] Create GenerateVisualsResponse DTO in src/PoAppIdea.Web/Features/Visual/GenerateVisualsResponse.cs
- [X] T126 [US6] Create VisualGenerator service with DALL-E integration in src/PoAppIdea.Web/Infrastructure/AI/VisualGenerator.cs
- [X] T127 [US6] Implement VisualService with queue-based retry for rate limits in src/PoAppIdea.Web/Features/Visual/VisualService.cs
- [X] T128 [US6] Create GenerateVisualsEndpoint (POST /api/sessions/{id}/visuals) in src/PoAppIdea.Web/Features/Visual/GenerateVisualsEndpoint.cs
- [X] T129 [US6] Create GetVisualsEndpoint (GET /api/sessions/{id}/visuals) in src/PoAppIdea.Web/Features/Visual/GetVisualsEndpoint.cs
- [X] T130 [US6] Create SelectVisualRequest DTO in src/PoAppIdea.Web/Features/Visual/SelectVisualRequest.cs
- [X] T131 [US6] Create SelectVisualEndpoint (POST /api/sessions/{id}/visuals/{assetId}/select) in src/PoAppIdea.Web/Features/Visual/SelectVisualEndpoint.cs
- [X] T132 [P] [US6] Implement VisualAssetRepository using TableStorageClient and BlobStorageClient in src/PoAppIdea.Web/Infrastructure/Storage/VisualAssetRepository.cs
- [X] T133 [US6] Create VisualPage.razor with gallery selection interface in src/PoAppIdea.Web/Components/Pages/VisualPage.razor
- [X] T134 [P] [US6] Create VisualCard.razor component with style attributes display in src/PoAppIdea.Web/Components/Shared/VisualCard.razor

**Checkpoint**: User Story 6 complete - 10 visual assets generated, user selects preferred direction

---

## Phase 9: User Story 7 - Final Artifact Generation (Priority: P7)

**Goal**: Generate PRD, Technical Deep-Dive, and Visual Asset Pack based on all session data

**Independent Test**: Verify all three documents generated with content derived from previous phases

### Artifacts Feature

- [ ] T135 [US7] Create GenerateArtifactsRequest DTO in src/PoAppIdea.Web/Features/Artifacts/GenerateArtifactsRequest.cs
- [ ] T136 [US7] Create GenerateArtifactsResponse DTO in src/PoAppIdea.Web/Features/Artifacts/GenerateArtifactsResponse.cs
- [ ] T137 [US7] Create ArtifactGenerator with PRD, TechnicalDeepDive, VisualPack templates in src/PoAppIdea.Web/Infrastructure/AI/ArtifactGenerator.cs
- [ ] T138 [US7] Implement ArtifactService with artifact assembly logic in src/PoAppIdea.Web/Features/Artifacts/ArtifactService.cs
- [ ] T139 [US7] Create GenerateArtifactsEndpoint (POST /api/sessions/{id}/artifacts) in src/PoAppIdea.Web/Features/Artifacts/GenerateArtifactsEndpoint.cs
- [X] T140 [US7] Create GetArtifactsEndpoint (GET /api/sessions/{id}/artifacts) in src/PoAppIdea.Web/Features/Artifacts/GetArtifactsEndpoint.cs
- [X] T141 [US7] Create GetArtifactEndpoint (GET /api/artifacts/{id}) in src/PoAppIdea.Web/Features/Artifacts/GetArtifactEndpoint.cs
- [X] T142 [US7] Create DownloadArtifactEndpoint (GET /api/artifacts/{id}/download) in src/PoAppIdea.Web/Features/Artifacts/DownloadArtifactEndpoint.cs
- [X] T143 [US7] Create ArtifactsPage.razor with document preview and download in src/PoAppIdea.Web/Components/Pages/ArtifactsPage.razor
- [X] T144 [P] [US7] Create ArtifactViewer.razor component with markdown rendering in src/PoAppIdea.Web/Components/Shared/ArtifactViewer.razor

**Checkpoint**: User Story 7 complete - all three artifacts generated with comprehensive content from session

---

## Phase 10: User Story 8 - Product Personality & Persistent Learning (Priority: P8)

**Goal**: Build persistent user preference profile that improves idea generation across sessions

**Independent Test**: Complete multiple sessions and verify subsequent sessions show different initial idea rankings

### Personality Feature

- [X] T145 [US8] Create GetPersonalityRequest DTO in src/PoAppIdea.Web/Features/Personality/GetPersonalityRequest.cs
- [X] T146 [US8] Create GetPersonalityResponse DTO with preferences in src/PoAppIdea.Web/Features/Personality/GetPersonalityResponse.cs
- [X] T147 [US8] Implement PersonalityService with preference aggregation logic in src/PoAppIdea.Web/Features/Personality/PersonalityService.cs
- [X] T148 [US8] Create GetPersonalityEndpoint (GET /api/users/{id}/personality) in src/PoAppIdea.Web/Features/Personality/GetPersonalityEndpoint.cs
- [X] T149 [US8] Create UpdatePersonalityEndpoint (POST /api/users/{id}/personality) in src/PoAppIdea.Web/Features/Personality/UpdatePersonalityEndpoint.cs
- [X] T150 [US8] Integrate personality data into IdeaGenerator for Phase 1 pre-alignment in src/PoAppIdea.Web/Infrastructure/AI/IdeaGenerator.cs
- [X] T151 [US8] Create PersonalityPage.razor with preference visualization in src/PoAppIdea.Web/Components/Pages/PersonalityPage.razor
- [X] T152 [P] [US8] Create PreferenceChart.razor component with themed categories in src/PoAppIdea.Web/Components/Shared/PreferenceChart.razor

**Checkpoint**: User Story 8 complete - personality profile persists and influences subsequent sessions

---

## Phase 11: User Story 9 - Public Gallery & Community Discovery (Priority: P9)

**Goal**: Users can publish artifacts to public gallery; others can browse and import ideas

**Independent Test**: Publish an artifact and verify it appears in gallery with proper attribution and is searchable

### Gallery Feature

- [X] T153 [US9] Create BrowseGalleryRequest DTO with pagination in src/PoAppIdea.Web/Features/Gallery/BrowseGalleryRequest.cs
- [X] T154 [US9] Create BrowseGalleryResponse DTO with artifact summaries in src/PoAppIdea.Web/Features/Gallery/BrowseGalleryResponse.cs
- [X] T155 [US9] Implement GalleryService with search and pagination in src/PoAppIdea.Web/Features/Gallery/GalleryService.cs
- [X] T156 [US9] Create BrowseGalleryEndpoint (GET /api/gallery) in src/PoAppIdea.Web/Features/Gallery/BrowseGalleryEndpoint.cs
- [X] T157 [US9] Create PublishArtifactRequest DTO in src/PoAppIdea.Web/Features/Gallery/PublishArtifactRequest.cs
- [X] T158 [US9] Create PublishArtifactEndpoint (POST /api/artifacts/{id}/publish) in src/PoAppIdea.Web/Features/Gallery/PublishArtifactEndpoint.cs
- [X] T159 [US9] Create ImportIdeaRequest DTO in src/PoAppIdea.Web/Features/Gallery/ImportIdeaRequest.cs
- [X] T160 [US9] Create ImportIdeaEndpoint (POST /api/gallery/{artifactId}/import) in src/PoAppIdea.Web/Features/Gallery/ImportIdeaEndpoint.cs
- [X] T161 [US9] Create GalleryPage.razor with browsable artifact grid in src/PoAppIdea.Web/Components/Pages/GalleryPage.razor
- [X] T162 [P] [US9] Create GalleryCard.razor component with publish status in src/PoAppIdea.Web/Components/Shared/GalleryCard.razor
- [X] T163 [US9] Create UserHistoryPage.razor with session history and artifacts in src/PoAppIdea.Web/Components/Pages/UserHistoryPage.razor

**Checkpoint**: User Story 9 complete - artifacts publishable to gallery, browsable and importable by community

---

## Phase 12: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

### Documentation

- [X] T164 [P] Create API.http file with all endpoint examples in src/PoAppIdea.Web/API.http
- [X] T165 [P] Update README.md with project overview and setup instructions at src/README.md
- [X] T166 [P] Create CONTRIBUTING.md with development guidelines at src/CONTRIBUTING.md

### Error Handling & Validation

- [X] T167 Add global exception handler middleware in src/PoAppIdea.Web/Infrastructure/ExceptionHandlerMiddleware.cs
- [X] T168 [P] Add FluentValidation validators for all request DTOs in src/PoAppIdea.Web/Infrastructure/Validators/
- [X] T169 Add rate limiting middleware for API endpoints in src/PoAppIdea.Web/Infrastructure/RateLimitingMiddleware.cs

### Edge Cases

- [X] T170 [P] Handle "all likes" edge case in SparkService (use swipe speed for ranking)
- [X] T171 [P] Handle "all dislikes" edge case in SparkService (offer restart with new parameters)
- [X] T172 Handle synthesis failure edge case (present individual paths) in SynthesisService
- [X] T173 [P] Handle offline visual generation (queue and retry) in VisualService

### Performance Optimization

- [X] T174 Add response caching for gallery endpoints in GalleryService
- [X] T175 [P] Optimize SignalR message size for large artifact outputs in SwipeHub configuration

### Final Validation

- [X] T176 Run quickstart.md validation steps to confirm setup works
- [X] T177 Verify all /health endpoint dependencies report healthy
- [X] T178 Confirm OpenAPI documentation generates correctly at /scalar/v1

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-11)**: All depend on Foundational phase completion
  - User stories can proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → ... → P9)
- **Polish (Phase 12)**: Depends on all desired user stories being complete

### User Story Dependencies

| Story | Depends On | Can Start After |
|-------|-----------|----------------|
| US1 (P1) | Foundational only | Phase 2 complete |
| US2 (P2) | US1 (needs top 5 ideas) | US1 complete |
| US3 (P3) | US2 (needs top 10 mutations) | US2 complete |
| US4 (P4) | US3 (needs Top 10 proto-apps) | US3 complete |
| US5 (P5) | US4 (needs finalized concept) | US4 complete |
| US6 (P6) | US5 (needs refinement answers) | US5 complete |
| US7 (P7) | US6 (needs visual selection) | US6 complete |
| US8 (P8) | Foundational only | Phase 2 complete |
| US9 (P9) | US7 (needs artifacts to publish) | US7 complete |

**Note**: US8 (Personality) can run in parallel with US1-US7 since it's a cross-session enhancement.

### Within Each User Story

- DTOs before Services
- Services before Endpoints
- Endpoints before Pages
- Repositories can be parallel with DTOs

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- All entity/enum definitions in Phase 2 can run in parallel
- All repository implementations marked [P] can run in parallel
- DTOs within a user story marked [P] can run in parallel
- Components within a user story marked [P] can run in parallel
- US8 (Personality) can run in parallel with US1-US7

---

## Parallel Example: User Story 1

```bash
# Launch all DTOs for User Story 1 together:
Task: "T063 Create StartSessionRequest DTO"
Task: "T064 Create StartSessionResponse DTO"
Task: "T069 Create GenerateIdeasRequest DTO"
Task: "T070 Create GenerateIdeasResponse DTO"
Task: "T073 Create RecordSwipeRequest DTO"

# After DTOs, launch services:
Task: "T065 Implement SessionService"
Task: "T071 Implement SparkService"

# After services, endpoints and pages:
Task: "T066 Create StartSessionEndpoint"
Task: "T068 Create ScopePage.razor" (can parallel with endpoints)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T008)
2. Complete Phase 2: Foundational (T009-T062) **CRITICAL - blocks all stories**
3. Complete Phase 3: User Story 1 (T063-T079)
4. **STOP and VALIDATE**: Test scope configuration and 50-idea swipe flow
5. Deploy/demo if ready - users can brainstorm ideas immediately

### Incremental Delivery

| Increment | Stories | User Value |
|-----------|---------|------------|
| MVP | US1 | Standalone brainstorming tool with learning |
| v0.2 | US1 + US2 | Ideas evolve through mutation |
| v0.3 | US1-US3 | Feature-complete proto-apps identified |
| v0.4 | US1-US4 | Creative synthesis of multiple ideas |
| v0.5 | US1-US5 | Professional PM/Architect refinement |
| v0.6 | US1-US6 | Visual mockups included |
| v1.0 | US1-US7 | Full artifact generation (PRD, Tech, Visual) |
| v1.1 | US1-US8 | Persistent learning across sessions |
| v1.2 | US1-US9 | Community gallery for discovery |

### Parallel Team Strategy

With 3 developers after Foundational phase:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - **Developer A**: US1 → US2 → US3 (core flow)
   - **Developer B**: US8 (Personality - independent)
   - **Developer C**: Polish/Validation tasks
3. After US3 complete, Developer C can help with US4-US7
4. US9 (Gallery) after US7 artifacts exist

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story delivers testable, deployable value
- US1 alone is a complete MVP brainstorming tool
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Edge cases (T170-T174) should be addressed before production
