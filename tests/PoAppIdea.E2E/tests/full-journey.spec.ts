import { test, expect, Page } from '@playwright/test';

/**
 * E2E Test: Full User Journey - Critical User Path
 * 
 * This test goes through the complete idea evolution flow:
 * 1. Login → 2. New Session → 3. Spark (Swipe Ideas) → 4. Mutations (Rate)
 * 5. Feature Expansion → 6. Submission/Synthesis → 7. Refinement
 * 8. Visual → 9. Artifacts
 * 
 * Prerequisites:
 * - App running on https://localhost:5001
 * - Azure Storage Emulator running (Azurite)
 * - Azure OpenAI configured in user-secrets
 * 
 * Run with: npx playwright test full-journey.spec.ts --headed
 */

const BASE_URL = 'https://localhost:5001';

// Timeout for AI operations (generation can take 10-30 seconds)
const AI_TIMEOUT = 60000;

// Helper to log test progress
function logStep(step: string) {
  console.log(`[E2E Journey] ${step}`);
}

// Helper to swipe right on idea cards (click the green button)
async function swipeRight(page: Page, count: number = 3) {
  for (let i = 0; i < count; i++) {
    const rightButton = page.locator('button.btn-success, button:has(.rzi-thumb-up), [data-testid="swipe-right"]').first();
    if (await rightButton.isVisible({ timeout: 5000 }).catch(() => false)) {
      await rightButton.click();
      await page.waitForTimeout(500); // Wait for animation
    }
  }
}

// Helper to swipe left on idea cards
async function swipeLeft(page: Page, count: number = 2) {
  for (let i = 0; i < count; i++) {
    const leftButton = page.locator('button.btn-danger, button:has(.rzi-close), [data-testid="swipe-left"]').first();
    if (await leftButton.isVisible({ timeout: 5000 }).catch(() => false)) {
      await leftButton.click();
      await page.waitForTimeout(500);
    }
  }
}

test.describe('Full User Journey', () => {
  // Use longer timeout for the full journey
  test.setTimeout(300000); // 5 minutes

  test('should complete entire idea evolution flow', async ({ page }) => {
    // Track API errors
    const apiErrors: string[] = [];
    page.on('response', response => {
      if (response.status() >= 400 && !response.url().includes('chrome.devtools')) {
        apiErrors.push(`${response.status()} - ${response.url()}`);
        console.log(`[API Error] ${response.status()}: ${response.url()}`);
      }
    });

    // ═══════════════════════════════════════════════════════════
    // STEP 1: Authenticate
    // ═══════════════════════════════════════════════════════════
    logStep('Step 1: Authenticating via test-login endpoint');
    await page.goto(`${BASE_URL}/auth/test-login?userId=e2e-full-journey&email=journey@test.com&name=E2E%20Journey%20User&returnUrl=/`);
    await page.waitForLoadState('networkidle');
    
    // Verify we're not on login page
    expect(page.url()).not.toContain('/login');
    logStep('Step 1: ✅ Authentication successful');

    // ═══════════════════════════════════════════════════════════
    // STEP 2: Create New Session
    // ═══════════════════════════════════════════════════════════
    logStep('Step 2: Creating new session');
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Select app type from Radzen dropdown
    const appTypeDropdown = page.locator('.rz-dropdown').first();
    await appTypeDropdown.click();
    await page.waitForSelector('.rz-dropdown-item', { state: 'visible', timeout: 5000 });
    await page.locator('.rz-dropdown-item').filter({ hasText: /Web App/i }).first().click();
    await page.waitForTimeout(500);
    
    // Start session
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await expect(startButton).toBeEnabled();
    await startButton.click();
    
    // Wait for navigation to spark page
    await page.waitForURL(/\/session\/.*\/spark/, { timeout: 30000 });
    logStep('Step 2: ✅ Session created, navigated to Spark page');

    // Extract session ID from URL
    const sparkUrl = page.url();
    const sessionIdMatch = sparkUrl.match(/\/session\/([^/]+)\/spark/);
    const sessionId = sessionIdMatch?.[1];
    expect(sessionId).toBeDefined();
    logStep(`Session ID: ${sessionId}`);

    // ═══════════════════════════════════════════════════════════
    // STEP 3: Spark Phase - Swipe through ideas
    // ═══════════════════════════════════════════════════════════
    logStep('Step 3: Spark Phase - Swiping through ideas');
    
    // Wait for ideas to load (AI generation)
    await page.waitForTimeout(AI_TIMEOUT / 10); // Initial wait
    
    // Wait for idea cards to appear
    const ideaCard = page.locator('.idea-card, .swipe-card, .rz-card').first();
    await expect(ideaCard).toBeVisible({ timeout: AI_TIMEOUT });
    
    // Swipe through batch 1 (mix of right and left)
    for (let i = 0; i < 10; i++) {
      // Alternate: 3 right, 2 left pattern to ensure we have liked ideas
      if (i % 5 < 3) {
        await swipeRight(page, 1);
      } else {
        await swipeLeft(page, 1);
      }
      await page.waitForTimeout(300);
    }
    
    // Wait for batch 2 if it loads
    await page.waitForTimeout(3000);
    
    // Check if "Continue" or "Evolve" button appears
    const continueButton = page.getByRole('button', { name: /Continue|Evolve|Next/i });
    if (await continueButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await continueButton.click();
    }
    
    // Wait for navigation to mutations page
    await page.waitForURL(/\/session\/.*\/(mutations|evolve)/, { timeout: 30000 });
    logStep('Step 3: ✅ Spark phase complete, navigated to Mutations page');

    // ═══════════════════════════════════════════════════════════
    // STEP 4: Mutations Phase - Rate evolved ideas
    // ═══════════════════════════════════════════════════════════
    logStep('Step 4: Mutations Phase - Rating mutations');
    await page.waitForLoadState('networkidle');
    
    // Wait for mutations to load
    await page.waitForTimeout(AI_TIMEOUT / 10);
    
    // Look for "Start Rating" button
    const startRatingButton = page.getByRole('button', { name: /Start Rating/i });
    if (await startRatingButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await startRatingButton.click();
      await page.waitForLoadState('networkidle');
    }
    
    // Rate mutations (similar to spark phase)
    const mutationCard = page.locator('.mutation-card, .swipe-card, .rz-card').first();
    if (await mutationCard.isVisible({ timeout: 30000 }).catch(() => false)) {
      for (let i = 0; i < 10; i++) {
        if (i % 5 < 3) {
          await swipeRight(page, 1);
        } else {
          await swipeLeft(page, 1);
        }
        await page.waitForTimeout(300);
      }
    }
    
    // Continue to features
    const toFeaturesButton = page.getByRole('button', { name: /Continue|Features|Expand|Next/i });
    if (await toFeaturesButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await toFeaturesButton.click();
    }
    
    await page.waitForURL(/\/session\/.*\/(features|expand)/, { timeout: 30000 });
    logStep('Step 4: ✅ Mutations phase complete, navigated to Features page');

    // ═══════════════════════════════════════════════════════════
    // STEP 5: Feature Expansion Phase
    // ═══════════════════════════════════════════════════════════
    logStep('Step 5: Feature Expansion Phase');
    await page.waitForLoadState('networkidle');
    
    // Generate features if needed
    const generateFeaturesButton = page.getByRole('button', { name: /Generate|Expand Features/i });
    if (await generateFeaturesButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await generateFeaturesButton.click();
      await page.waitForTimeout(AI_TIMEOUT / 5);
    }
    
    // Rate features (if rating interface exists)
    const featureStartRating = page.getByRole('button', { name: /Start Rating/i });
    if (await featureStartRating.isVisible({ timeout: 10000 }).catch(() => false)) {
      await featureStartRating.click();
      
      // Rate some features
      for (let i = 0; i < 5; i++) {
        await swipeRight(page, 1);
        await page.waitForTimeout(300);
      }
    }
    
    // Continue to submission
    const toSubmitButton = page.getByRole('button', { name: /Continue|Submit|Synthesis|Next/i });
    if (await toSubmitButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await toSubmitButton.click();
    }
    
    await page.waitForURL(/\/session\/.*\/(submit|synthesize)/, { timeout: 30000 });
    logStep('Step 5: ✅ Feature Expansion complete, navigated to Submission page');

    // ═══════════════════════════════════════════════════════════
    // STEP 6: Submission & Synthesis Phase
    // ═══════════════════════════════════════════════════════════
    logStep('Step 6: Submission & Synthesis Phase');
    await page.waitForLoadState('networkidle');
    
    // Select ideas for synthesis (if selection interface exists)
    const ideaCheckbox = page.locator('.selection-card, input[type="checkbox"], .rz-chkbox').first();
    if (await ideaCheckbox.isVisible({ timeout: 10000 }).catch(() => false)) {
      // Select first few ideas
      const checkboxes = page.locator('.selection-card, .rz-chkbox');
      const count = await checkboxes.count();
      for (let i = 0; i < Math.min(3, count); i++) {
        await checkboxes.nth(i).click();
        await page.waitForTimeout(200);
      }
    }
    
    // Submit selection / Synthesize
    const synthesizeButton = page.getByRole('button', { name: /Synthesize|Submit Selection|Generate/i });
    if (await synthesizeButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await synthesizeButton.click();
      await page.waitForTimeout(AI_TIMEOUT / 5);
    }
    
    // Continue to refinement
    const toRefinementButton = page.getByRole('button', { name: /Continue|Refine|Refinement|Next/i });
    if (await toRefinementButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await toRefinementButton.click();
    }
    
    await page.waitForURL(/\/session\/.*\/(refinement|refine)/, { timeout: 30000 });
    logStep('Step 6: ✅ Synthesis complete, navigated to Refinement page');

    // ═══════════════════════════════════════════════════════════
    // STEP 7: Refinement Phase - Answer Questions
    // ═══════════════════════════════════════════════════════════
    logStep('Step 7: Refinement Phase - Answering questions');
    await page.waitForLoadState('networkidle');
    
    // Wait for questions to load
    await page.waitForTimeout(5000);
    
    // Answer questions (look for text areas or input fields)
    const questionInputs = page.locator('textarea, input[type="text"]').filter({ hasNotText: '' });
    const inputCount = await questionInputs.count();
    
    for (let i = 0; i < inputCount; i++) {
      const input = questionInputs.nth(i);
      if (await input.isVisible().catch(() => false)) {
        await input.fill(`Test answer ${i + 1} - This is a sample response for E2E testing.`);
        await page.waitForTimeout(200);
      }
    }
    
    // Submit answers
    const submitAnswersButton = page.getByRole('button', { name: /Submit|Save|Next Phase|Continue/i });
    if (await submitAnswersButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await submitAnswersButton.click();
      await page.waitForTimeout(3000);
    }
    
    // Continue to visual
    const toVisualButton = page.getByRole('button', { name: /Continue|Visual|Visualize|Next/i });
    if (await toVisualButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await toVisualButton.click();
    }
    
    await page.waitForURL(/\/session\/.*\/(visual|visualize)/, { timeout: 30000 });
    logStep('Step 7: ✅ Refinement complete, navigated to Visual page');

    // ═══════════════════════════════════════════════════════════
    // STEP 8: Visual Phase - Generate Visuals
    // ═══════════════════════════════════════════════════════════
    logStep('Step 8: Visual Phase - Generating visuals');
    await page.waitForLoadState('networkidle');
    
    // Generate visuals
    const generateVisualsButton = page.getByRole('button', { name: /Generate|Create Visual/i });
    if (await generateVisualsButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await generateVisualsButton.click();
      // Wait for DALL-E generation (can take longer)
      await page.waitForTimeout(AI_TIMEOUT / 2);
    }
    
    // Select a visual (if selection needed)
    const visualCard = page.locator('.visual-card, .visual-option, img').first();
    if (await visualCard.isVisible({ timeout: 30000 }).catch(() => false)) {
      await visualCard.click();
    }
    
    // Continue to artifacts
    const toArtifactsButton = page.getByRole('button', { name: /Continue|Artifacts|Complete|Generate Docs|Next/i });
    if (await toArtifactsButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await toArtifactsButton.click();
    }
    
    await page.waitForURL(/\/session\/.*\/(artifacts|complete)/, { timeout: 30000 });
    logStep('Step 8: ✅ Visual phase complete, navigated to Artifacts page');

    // ═══════════════════════════════════════════════════════════
    // STEP 9: Artifacts Phase - Generate Documents
    // ═══════════════════════════════════════════════════════════
    logStep('Step 9: Artifacts Phase - Generating documents');
    await page.waitForLoadState('networkidle');
    
    // Generate artifacts
    const generateArtifactsButton = page.getByRole('button', { name: /Generate|Create|PRD|Artifacts/i });
    if (await generateArtifactsButton.isVisible({ timeout: 10000 }).catch(() => false)) {
      await generateArtifactsButton.click();
      await page.waitForTimeout(AI_TIMEOUT / 2);
    }
    
    // Verify artifacts are displayed
    const artifactCard = page.locator('.artifact-card, .rz-card').first();
    await expect(artifactCard).toBeVisible({ timeout: AI_TIMEOUT });
    
    logStep('Step 9: ✅ Artifacts generated - JOURNEY COMPLETE!');

    // ═══════════════════════════════════════════════════════════
    // Final Assertions
    // ═══════════════════════════════════════════════════════════
    logStep('Verifying final state');
    
    // We should be on the artifacts page
    expect(page.url()).toMatch(/\/session\/.*\/(artifacts|complete)/);
    
    // Check for critical API errors (ignore 404s for optional resources)
    const criticalErrors = apiErrors.filter(e => 
      !e.includes('404') && 
      !e.includes('chrome.devtools')
    );
    
    if (criticalErrors.length > 0) {
      console.warn('[E2E Journey] API errors encountered:', criticalErrors);
    }
    
    logStep('═══════════════════════════════════════════════════════════');
    logStep('🎉 FULL USER JOURNEY COMPLETED SUCCESSFULLY!');
    logStep('═══════════════════════════════════════════════════════════');
  });
});

test.describe('Quick Smoke Test', () => {
  test.setTimeout(120000); // 2 minutes
  
  test('should navigate through all phases (UI only)', async ({ page }) => {
    // This test just verifies navigation works, without full AI generation
    
    logStep('Quick Smoke Test: Authenticating');
    await page.goto(`${BASE_URL}/auth/test-login?userId=e2e-smoke&email=smoke@test.com&name=Smoke%20Test&returnUrl=/`);
    await page.waitForLoadState('networkidle');
    
    // Use a mock session ID to test page rendering
    const mockSessionId = '00000000-0000-0000-0000-000000000001';
    
    const phases = [
      { path: `/session/${mockSessionId}/spark`, name: 'Spark' },
      { path: `/session/${mockSessionId}/evolve`, name: 'Mutations' },
      { path: `/session/${mockSessionId}/features`, name: 'Feature Expansion' },
      { path: `/session/${mockSessionId}/submit`, name: 'Submission' },
      { path: `/session/${mockSessionId}/refinement`, name: 'Refinement' },
      { path: `/session/${mockSessionId}/visual`, name: 'Visual' },
      { path: `/session/${mockSessionId}/artifacts`, name: 'Artifacts' },
    ];
    
    for (const phase of phases) {
      logStep(`Testing ${phase.name} page loads`);
      await page.goto(`${BASE_URL}${phase.path}`);
      await page.waitForLoadState('networkidle');
      
      // Page should render without fatal errors
      const bodyContent = await page.locator('body').textContent();
      expect(bodyContent?.length).toBeGreaterThan(50);
      
      // Should not show unhandled exception page
      const errorPage = page.locator('text=/unhandled exception/i');
      const hasError = await errorPage.isVisible().catch(() => false);
      expect(hasError).toBeFalsy();
      
      logStep(`✅ ${phase.name} page loaded successfully`);
    }
    
    logStep('🎉 Quick Smoke Test PASSED - All pages render correctly');
  });
});
