# Feature Specification: PoAppIdea - The Self-Evolving Ideation Engine

**Feature Branch**: `001-idea-evolution-engine`  
**Created**: 2026-01-29  
**Status**: Draft  
**Input**: User description: "PoAppIdea: The Self-Evolving Ideation Engine - a revolutionary platform for directed evolution of ideas through phases, transforming vague concepts into professional-grade roadmaps"

## Clarifications

### Session 2026-01-29

- Q: What authentication method should the platform use? → A: OAuth providers (Google, GitHub, Microsoft) with optional email/password fallback
- Q: What uptime/availability target for the production service? → A: 99.5% uptime target with scheduled maintenance windows
- Q: What data privacy compliance requirements apply? → A: Privacy policy disclosure only, no formal compliance framework
- Q: Which AI image service for Phase 6 visual generation? → A: Azure OpenAI DALL-E with queue-based retry on rate limits
- Q: What data retention policy for sessions and artifacts? → A: Indefinite retention for all data (sessions, swipes, artifacts)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Phase 0-1: Scope Configuration & Spark Discovery (Priority: P1)

A creator opens PoAppIdea to begin generating ideas for their next project. They first configure the session by selecting an App Type (game, productivity tool, mobile utility, or automation script) and setting a Complexity Slider (1-5). The system then presents 20 one-to-two sentence app ideas in a rapid-fire swipe interface, delivered in batches of ten. The user swipes right to "Like" and left to "Dislike" each idea, with the system learning in real-time and tailoring the second batch based on preferences.

**Why this priority**: This is the core entry point and discovery mechanism. Without the ability to configure scope and discover initial ideas through the swipe mechanic, no other phases can function. This delivers immediate value as a standalone brainstorming tool.

**Independent Test**: Can be fully tested by completing a full 20-idea swipe session and verifying that batch 2 shows measurably different themes aligned with the user's likes from batch 1.

**Acceptance Scenarios**:

1. **Given** a user opens the app for the first time, **When** they select "Mobile Utility" as App Type and set Complexity to 3, **Then** the system generates 20 context-appropriate ideas and presents the first batch of 10.
2. **Given** a user is swiping through batch 1, **When** they swipe right on 3 ideas related to "fitness tracking," **Then** batch 2 contains noticeably more fitness-related concepts.
3. **Given** a user quickly swipes right (under 1 second) on an idea, **When** compared to an idea they hesitated on (3+ seconds) before swiping right, **Then** the fast-swipe idea's traits are weighted more heavily in subsequent batches.
4. **Given** a user completes all 20 swipes, **When** the system calculates results, **Then** the top 3 highest-rated ideas are identified based on likes and swipe speed weighting.

---

### User Story 2 - Phase 2: Evolution & Feature Integration (Priority: P2)

After completing the Spark phase, the user enters the Evolution phase where the top 3 ideas undergo directed evolution WITH integrated feature sets. Each idea receives 3 unique evolved variations that combine Crossover (blending elements from multiple liked ideas), Repurposing (shifting target audience), AND feature integration (APIs, services, capabilities) into a single cohesive mutation. This produces 9 total evolved concepts. The user rates these to identify the top 3 refined ideas.

**Why this priority**: This transforms raw discovery into refined concepts with actionable feature sets in a single streamlined phase. It's the differentiating feature that makes PoAppIdea an "evolution engine" rather than just a brainstorming list. Depends on P1 completion.

**Independent Test**: Can be tested by verifying that the 9 evolved ideas meaningfully combine mutation strategies with feature requirements, and that user ratings correctly identify the top 3 concepts.

**Acceptance Scenarios**:

1. **Given** the user has completed Phase 1 with 3 top ideas, **When** Phase 2 begins, **Then** the system generates 3 evolved variations per idea (9 total) combining mutation and feature integration.
2. **Given** a user liked "fitness tracker" and "roguelike game" in Phase 1, **When** viewing evolutions, **Then** at least one variation combines these into a "gamified fitness progression" concept with specific API integrations (health APIs, achievement systems).
3. **Given** an idea about "location-based reminders," **When** viewing evolutions, **Then** options include different integration approaches (geolocation APIs, offline-capable, privacy-first) baked into the concept.
4. **Given** the user completes rating all 9 evolutions, **When** results are calculated, **Then** the top 3 highest-rated evolved ideas with defined feature sets are identified.

---

### User Story 3 - Submission & Cohesive Synthesis (Priority: P3)

At the end of Phase 2, the user sees the final Top 3 candidates as "proto-apps" with defined scopes and feature sets. They can select between 1 and 3 ideas to proceed. If multiple ideas are selected, the system performs a "Cohesive Synthesis"—finding a thematic bridge to combine them into a single comprehensive application.

**Why this priority**: This enables flexible project scoping and creative merging of concepts. Depends on P2 completion.

**Independent Test**: Can be tested by selecting 2 distinct ideas and verifying the system produces a coherent merged concept with a clear thematic bridge.

**Acceptance Scenarios**:

1. **Given** the user sees the Top 3 proto-apps, **When** they select exactly 1 idea, **Then** that idea proceeds directly to refinement without merging.
2. **Given** the user selects "Task Manager" and "Habit RPG," **When** cohesive synthesis runs, **Then** the system produces a unified "gamified productivity platform" with a clear explanation of how elements combine.
3. **Given** the user selects all 3 ideas, **When** synthesis completes, **Then** the resulting merged concept identifies the common thread and explains which elements from each idea are retained.

---

### User Story 4 - Phase 3-4: Deep Refinement via Interactive Inquiry (Priority: P4)

Once the idea is finalized, the system acts as a Lead Product Manager (Phase 3) and Senior Architect (Phase 4), asking 5 clarifying questions per phase. Phase 3 covers Product Requirements (user personas, core loops, feature priorities, success metrics) while Phase 4 addresses Implementation Details (deployment, scaling, data integrity, security, maintenance).

**Why this priority**: This transforms proto-apps into professional-grade specifications. High value but requires previous phases. Depends on P3 completion.

**Independent Test**: Can be tested by verifying that 10 total questions are asked across both phases and that answers are incorporated into the final artifacts.

**Acceptance Scenarios**:

1. **Given** a finalized merged concept, **When** Phase 3 (PM Refinement) begins, **Then** the system presents 5 questions about user personas, core loops, feature conflicts, and success metrics.
2. **Given** the user answers all Phase 3 questions, **When** Phase 4 (Architect Refinement) begins, **Then** the system presents 5 questions about deployment, scaling, data handling, security, and maintenance.
3. **Given** the user specifies "privacy-first local storage," **When** reviewing later artifacts, **Then** this requirement is reflected in the Technical Deep-Dive document.

---

### User Story 5 - Phase 5: Aesthetic Vision & Visual Direction (Priority: P5)

Based on all gathered data—from swipe patterns to technical requirements—the system generates 3 graphical representations including mood boards and mockups representing potential layouts, color palettes, and UI styles. The user selects the visual direction that best matches their mental model.

**Why this priority**: Visual direction is valuable but depends on all prior refinement. Can be considered optional for MVP. Depends on P4 completion.

**Independent Test**: Can be tested by verifying that 3 distinct visual representations are generated and that the selected option is included in final artifacts.

**Acceptance Scenarios**:

1. **Given** the user has completed Phases 3-4, **When** Phase 5 begins, **Then** the system generates 3 mood boards/mockups reflecting the project's "vibe."
2. **Given** the user selected "gamified productivity" with "dark mode preference" inferred from swipes, **When** viewing mockups, **Then** the options include game-inspired UI elements and dark color palettes.
3. **Given** the user selects a specific visual direction, **When** viewing final artifacts, **Then** the Visual Asset Pack includes the selected style and layout descriptions.

---

### User Story 6 - Final Artifact Generation (Priority: P6)

The culmination produces a comprehensive project starter pack: (1) Project Requirements Document (PRD) detailing every feature, user story, and business rule; (2) Technical Deep-Dive with services, data structures, and projected cost analysis; (3) Visual Asset Pack with selected images and layout descriptions.

**Why this priority**: This is the deliverable but requires all phases. Depends on P5 completion.

**Independent Test**: Can be tested by verifying that all three documents are generated with content derived from previous phases.

**Acceptance Scenarios**:

1. **Given** all phases are complete, **When** artifact generation runs, **Then** a PRD, Technical Deep-Dive, and Visual Asset Pack are produced.
2. **Given** the user selected Complexity Level 5, **When** reviewing the Technical Deep-Dive, **Then** it includes enterprise-grade architecture, scalability considerations, and security protocols.
3. **Given** the user answered "subscription-based" in Phase 3, **When** reviewing the PRD, **Then** subscription monetization is documented as a business rule.

---

### User Story 7 - Product Personality & Persistent Learning (Priority: P7)

The system builds a "Product Personality" profile for each user by tracking decision speed, thematic preferences, and consistent down-votes across sessions. Over time, Phase 1 ideas are pre-aligned with the user's creative taste, ranking disliked patterns lower automatically.

**Why this priority**: Long-term user value but not required for core functionality. Can be implemented incrementally.

**Independent Test**: Can be tested by completing multiple sessions and verifying that subsequent sessions show measurably different initial idea rankings.

**Acceptance Scenarios**:

1. **Given** a user consistently down-votes "subscription-based" models across 3 sessions, **When** starting a new session, **Then** subscription-based ideas appear less frequently in Phase 1.
2. **Given** a user has completed 5 sessions with "gaming" preferences, **When** starting session 6, **Then** the initial batch skews toward game-related concepts without the user adjusting settings.

---

### User Story 8 - Public Gallery & Community Discovery (Priority: P8)

All users can browse a public gallery of ideas, swipes, and final artifacts (with user consent). This creates a "community brain" where one user's rejected mutation might become another's breakthrough. Assets are stored with human-readable identifiers for browsable history.

**Why this priority**: Community features add long-term platform value but are not required for individual user workflow. Requires privacy considerations.

**Independent Test**: Can be tested by publishing an artifact and verifying it appears in the public gallery with proper attribution and search capability.

**Acceptance Scenarios**:

1. **Given** a user completes a session and opts to publish, **When** visiting the public gallery, **Then** their final artifacts appear with human-readable identifiers.
2. **Given** a user is browsing the gallery, **When** they find an interesting rejected mutation, **Then** they can import it as a starting point for their own session.
3. **Given** a user wants to browse their own history, **When** accessing their profile, **Then** all past sessions, swipes, and artifacts are organized chronologically with searchable identifiers.

---

### Edge Cases

- What happens when a user swipes "Like" on all 20 ideas in Phase 1? (System should still identify top 3 by swipe speed and recency weighting)
- What happens when a user swipes "Dislike" on all 20 ideas? (System should offer to restart with different parameters or expand idea categories)
- What happens when the Cohesive Synthesis cannot find a thematic bridge between selected ideas? (System should present individual refinement paths with an explanation)
- What happens when a user abandons a session mid-phase? (System should persist progress and allow resumption)
- What happens when a user has no internet during Phase 6 image generation? (System should queue request and complete when connectivity restored)
- What happens when Complexity Level changes mid-session? (System should recalibrate remaining phases without losing prior work)

## Requirements *(mandatory)*

### Functional Requirements

#### Phase 0-1: Discovery

- **FR-001**: System MUST allow users to select an App Type from: competitive game, web-based productivity tool, mobile utility, automation script
- **FR-002**: System MUST provide a Complexity Slider with 5 levels (1=MVP, 3=Standard, 5=Enterprise)
- **FR-003**: System MUST generate 20 unique app ideas based on selected App Type and Complexity
- **FR-004**: System MUST present ideas in batches of 10 with a swipe-based interface (right=Like, left=Dislike)
- **FR-005**: System MUST analyze likes/dislikes between batches and adjust subsequent batch content
- **FR-006**: System MUST track swipe speed (time between idea display and swipe action)
- **FR-007**: System MUST weight fast swipes (under 1 second) more heavily than hesitated swipes (3+ seconds)

#### Phase 2: Evolution & Feature Integration

- **FR-008**: System MUST identify top 3 ideas from Phase 1 based on likes and swipe-speed weighting
- **FR-009**: System MUST generate 3 evolved variations per top idea (9 total) combining Crossover, Repurposing, AND feature integration
- **FR-010**: System MUST identify top 3 evolved ideas from Phase 2 ratings with defined feature sets

#### Submission & Synthesis

- **FR-011**: System MUST allow users to select between 1 and 3 ideas from the final Top 3
- **FR-012**: System MUST perform Cohesive Synthesis when multiple ideas are selected, identifying thematic bridges
- **FR-013**: System MUST explain the synthesis rationale to the user

#### Phases 3-4: Refinement

- **FR-014**: System MUST present 5 clarifying questions in Phase 3 covering user personas, core loops, feature priorities, and success metrics
- **FR-015**: System MUST present 5 clarifying questions in Phase 4 covering deployment, scaling, data integrity, security, and maintenance
- **FR-016**: System MUST incorporate all answers into final artifact generation

#### Phase 5: Visual

- **FR-017**: System MUST generate 3 graphical representations (mood boards/mockups) based on session data
- **FR-018**: System MUST allow users to select their preferred visual direction

#### Artifacts

- **FR-019**: System MUST generate a Project Requirements Document (PRD) with features, user stories, and business rules
- **FR-020**: System MUST generate a Technical Deep-Dive with services, data structures, and cost projections
- **FR-021**: System MUST generate a Visual Asset Pack with selected images and layout descriptions

#### Persistence & Learning

- **FR-022**: System MUST build and update a Product Personality profile based on user decisions across sessions
- **FR-023**: System MUST persist session progress for resumption
- **FR-024**: System MUST store all artifacts with human-readable identifiers

#### Community

- **FR-025**: System MUST allow users to publish artifacts to a public gallery (opt-in)
- **FR-026**: System MUST allow users to browse and import ideas from the public gallery

### Key Entities

- **User**: Account holder authenticated via OAuth (Google, GitHub, Microsoft) or email/password, with Product Personality profile and session history
- **Session**: A single idea evolution journey through phases 0-6, tied to a user
- **Idea**: A generated app concept with title, description, App Type, Complexity level
- **Swipe**: User interaction with an idea (direction, timestamp, duration)
- **Mutation**: A derived idea from Crossover or Repurposing of parent ideas
- **FeatureVariation**: A set of capabilities and service integrations for an evolved idea
- **Synthesis**: A merged concept from multiple selected ideas with thematic bridge
- **RefinementAnswer**: User response to a Phase 4 or 5 clarifying question
- **VisualAsset**: Generated mockup/mood board with style attributes
- **Artifact**: Final deliverable (PRD, Technical Deep-Dive, or Visual Asset Pack)
- **ProductPersonality**: Accumulated user preferences, biases, and decision patterns

## Assumptions

- Users have basic familiarity with ideation and project planning concepts
- Swipe interface patterns are intuitive to modern mobile/web users
- 20 ideas in Phase 1 provides sufficient variety without overwhelming users (reduced from 50)
- Batches of 10 balance learning efficiency with user cognitive load
- 5 questions per refinement phase captures essential requirements without fatigue (reduced from 10)
- Users can complete a full session in one sitting (10-20 minutes for standard complexity)
- Community gallery content will be moderated for quality and appropriateness
- Visual generation uses Azure OpenAI DALL-E service with queue-based retry on rate limits
- Privacy policy disclosure is sufficient; no formal GDPR/CCPA compliance framework required for initial launch
- All user data (sessions, swipes, artifacts) retained indefinitely with no automatic purging

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users complete the full Phase 0-2 discovery flow in under 5 minutes on average
- **SC-002**: 80% of users who start Phase 1 complete through to final artifact generation
- **SC-003**: Batch 2 ideas receive 40% higher like rates than Batch 1 (demonstrating learning effectiveness)
- **SC-004**: Users can identify meaningful evolution from their original liked ideas to final selected concepts
- **SC-005**: 90% of generated PRDs are deemed "actionable" by users in post-session surveys
- **SC-006**: Technical Deep-Dive cost projections are within 30% of actual implementation costs (validated via follow-up)
- **SC-007**: Returning users complete Phase 1 15% faster due to Product Personality pre-alignment
- **SC-008**: Public gallery generates at least 100 idea imports per 1,000 published artifacts
- **SC-009**: System supports 500 concurrent users without degradation in idea generation or swipe responsiveness
- **SC-010**: Users rate the platform 4+ out of 5 for "transforming vague ideas into clear project plans"
- **SC-011**: Platform maintains 99.5% uptime with scheduled maintenance windows communicated 48 hours in advance
