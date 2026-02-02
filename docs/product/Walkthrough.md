# PoAppIdea Walkthrough

Step-by-step guide to using PoAppIdea for idea evolution.

---

## Quick Start (5 Minutes)

### 1. Sign In

1. Navigate to `https://localhost:5001` (or production URL)
2. Click **Sign in with Google** (or GitHub/Microsoft)
3. Authorize the application
4. You'll land on the **Dashboard**

### 2. Create Your First Session

1. Click **New Session** button
2. Select **App Type**:
   - 🎮 Game
   - 📱 Mobile App
   - 🖥️ Productivity Tool
   - ⚙️ Automation Script
3. Set **Complexity** (1-5 slider)
   - 1 = Simple utility
   - 5 = Complex platform
4. Click **Start Session**

---

## The 6-Phase Journey

### ⚡ Phase 1: Spark (Idea Discovery)

**Goal**: Discover and rate 20 AI-generated ideas

**How to Use**:
1. You'll see idea cards one at a time
2. **Swipe Right** (or click 👍) to Like
3. **Swipe Left** (or click 👎) to Dislike
4. Complete 10 swipes for Batch 1
5. AI generates 10 more based on your likes
6. Complete 10 more swipes
7. View your **Top 3 Ideas**

**Pro Tips**:
- Swipe quickly on ideas you love (faster = higher weight)
- Take your time on ideas you're unsure about
- The AI learns from your speed patterns

---

### 🧬 Phase 2: Mutations (Evolution)

**Goal**: Rate 9 evolved versions of your top ideas

**How to Use**:
1. Each of your Top 3 ideas gets 3 mutations:
   - **Crossover**: Blends with other liked ideas
   - **Repurposing**: Different audience/use case
   - **Feature Integration**: Added capabilities
2. View mutation cards with evolved descriptions
3. Rate each mutation **1-5 stars**
4. Complete all ratings
5. View **Top 10 Mutations**

**Pro Tips**:
- Look for unexpected combinations
- Higher complexity unlocks more creative mutations
- Consider market fit, not just personal preference

---

### 🎯 Phase 3: Feature Expansion

**Goal**: Explore 50 feature variations with MoSCoW priorities

**How to Use**:
1. Each top mutation generates 5 feature variations
2. Variations include:
   - Feature lists (3-10 features each)
   - Service integrations (APIs, SDKs)
   - Variation themes (Enterprise, Consumer, Minimal)
3. Features are tagged with MoSCoW:
   - 🔴 **Must**: Critical for launch
   - 🟡 **Should**: High value, not blocking
   - 🟢 **Could**: Nice to have
   - ⚪ **Won't**: Out of scope
4. Rate variations to identify **Top 10 Proto-Apps**

---

### 📤 Phase 4: Submission

**Goal**: Select 1-10 ideas for final synthesis

**How to Use**:
1. Review your Top 10 proto-apps
2. Click to select ideas (multi-select enabled)
3. Click **Submit Selection**

**Single Selection**:
- Proceeds directly to refinement

**Multiple Selections**:
- AI performs **Cohesive Synthesis**
- Finds thematic bridge between ideas
- Creates unified concept

---

### 💬 Phase 5: Refinement

**Goal**: Answer 20 questions from PM and Architect personas

**How to Use**:

**PM Phase (10 Questions)**:
1. Answer questions about:
   - Target users and personas
   - Core value proposition
   - Feature priorities
   - Success metrics
2. Be specific and detailed
3. Click **Submit Answers**

**Architect Phase (10 Questions)**:
1. Answer questions about:
   - Deployment preferences
   - Scaling requirements
   - Data storage and privacy
   - Security needs
2. Technical precision helps
3. Click **Submit Answers**

**Pro Tips**:
- Answers directly influence final artifacts
- You can edit answers before submitting
- Leave fields blank if unsure (AI will make reasonable assumptions)

---

### 🎨 Phase 6: Artifacts

**Goal**: Generate and download your specification package

**Visual Direction**:
1. AI generates 3 mockups/mood boards
2. Review visual styles
3. Click to select your preferred direction

**Artifact Generation**:
1. Click **Generate Artifacts**
2. Wait for AI to create documents (30-60 seconds)
3. Preview each artifact:
   - 📄 **PRD**: Product Requirements Document
   - 🔧 **Tech Deep-Dive**: Architecture specification
   - 🎨 **Visual Pack**: Selected mockups and style guide

**Download**:
1. Click **Download Package** for ZIP bundle
2. Or download individual artifacts

---

## Session Management

### Resume a Session

1. From Dashboard, view **Session History**
2. Click on any incomplete session
3. Continue from where you left off

### Session States

| State | Description |
|-------|-------------|
| 🟡 In Progress | Active session, any phase |
| ✅ Completed | All artifacts generated |
| ⚪ Abandoned | Inactive for 24+ hours |

---

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `→` | Swipe Right (Like) |
| `←` | Swipe Left (Dislike) |
| `Enter` | Submit / Continue |
| `Esc` | Close modal |

---

## Troubleshooting

### "AI Generation Failed"

- Check your internet connection
- Wait 30 seconds and retry
- AI service may be rate-limited

### "Session Not Found"

- Session may have expired
- Start a new session

### "Authentication Error"

- Clear browser cookies
- Try a different OAuth provider

---

## Next Steps

After completing a session:

1. **Review Artifacts**: Read through PRD and Tech doc
2. **Share**: Export to Notion, Confluence, or GitHub
3. **Iterate**: Start a new session with refinements
4. **Build**: Use artifacts to guide development
