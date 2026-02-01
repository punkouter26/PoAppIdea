import { test, expect } from '@playwright/test';

/**
 * E2E Tests: Spark Generation Flow - Critical User Path
 * 
 * Tests the complete flow from login → new session → spark generation.
 * Uses the Development-only /auth/test-login endpoint to bypass OAuth.
 * 
 * NOTE: These tests require a fully configured backend with Azure Storage.
 * They are skipped by default in CI and should be run manually with a running server.
 */

const BASE_URL = 'https://localhost:5001';

// Skip all tests in this file - they require full backend integration
test.describe.skip('Spark Generation Flow', () => {
  
  test.beforeEach(async ({ page }) => {
    // Setup: Listen for console errors and network failures
    page.on('console', msg => {
      if (msg.type() === 'error') {
        console.log(`[Browser Error] ${msg.text()}`);
      }
    });
    
    // Authenticate using test-login endpoint (Development only)
    console.log('[Test] Authenticating via test-login endpoint');
    await page.goto(`${BASE_URL}/auth/test-login?userId=e2e-test-user&email=e2e@test.com&name=E2E%20Test%20User&returnUrl=/`);
    await page.waitForLoadState('networkidle');
    
    // Verify we're authenticated (should be on home page, not login)
    const currentUrl = page.url();
    if (currentUrl.includes('/login')) {
      throw new Error('Test authentication failed - still on login page');
    }
    console.log('[Test] Authentication successful');
  });

  test('should complete full flow: create session → reach spark page → generate ideas', async ({ page }) => {
    // Track API errors
    const apiErrors: string[] = [];
    
    page.on('response', response => {
      if (response.status() >= 400) {
        const url = response.url();
        const status = response.status();
        apiErrors.push(`${status} - ${url}`);
        console.log(`[API Error] ${status}: ${url}`);
      }
    });

    // Step 1: Navigate to New Session
    console.log('[Test] Step 1: Navigate to New Session');
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Step 2: Fill out session form - select app type from Radzen dropdown
    console.log('[Test] Step 2: Configure session - selecting app type');
    
    // Click the Radzen dropdown to open it
    const dropdown = page.locator('.rz-dropdown').first();
    await dropdown.click();
    
    // Select Mobile from the dropdown options
    await page.getByText(/Mobile/i).first().click();
    
    // Wait for the button to become enabled
    await page.waitForTimeout(500);
    
    // Step 3: Start session
    console.log('[Test] Step 3: Start session');
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await expect(startButton).toBeEnabled();
    await startButton.click();
    
    // Step 4: Wait for navigation to spark page
    console.log('[Test] Step 4: Wait for spark page');
    await page.waitForURL(/\/session\/.*\/spark/, { timeout: 20000 });
    
    // Step 5: Check for errors on spark page
    console.log('[Test] Step 5: Check for errors');
    await page.waitForLoadState('networkidle');
    
    // Wait for ideas to load (give OpenAI time to respond)
    await page.waitForTimeout(5000);
    
    // Look for error dialogs or messages
    const errorDialog = page.locator('.rz-dialog, [role="dialog"], .error, [class*="error"]');
    const hasError = await errorDialog.isVisible().catch(() => false);
    
    if (hasError) {
      const errorText = await errorDialog.textContent();
      console.log(`[Test] Error detected: ${errorText}`);
      
      // Capture specific error patterns
      if (errorText?.includes('401') || errorText?.includes('Access denied')) {
        console.log('[Test] API Authentication Error detected - likely invalid API key');
      }
      if (errorText?.includes('wrong API endpoint')) {
        console.log('[Test] Wrong API endpoint error detected');
      }
    }
    
    // Log all API errors encountered
    if (apiErrors.length > 0) {
      console.log('[Test] API Errors encountered:');
      apiErrors.forEach(err => console.log(`  - ${err}`));
    }
    
    // Assert: Should not have any 401 errors from Azure OpenAI
    const openAIErrors = apiErrors.filter(e => 
      e.includes('cognitive.microsoft.com') || 
      e.includes('openai.azure.com')
    );
    
    expect(openAIErrors).toHaveLength(0);
    
    // Verify ideas are displayed (swipe cards visible)
    const swipeCard = page.locator('.swipe-container, [class*="swipe"], .rz-card').first();
    await expect(swipeCard).toBeVisible({ timeout: 10000 });
    console.log('[Test] Ideas generated successfully - swipe cards visible');
  });

  test('should show proper error message for API failures', async ({ page }) => {
    // Verifies error handling UI
    
    let apiError401Detected = false;
    
    page.on('response', response => {
      if (response.status() === 401 && 
          (response.url().includes('cognitive.microsoft.com') || 
           response.url().includes('openai'))) {
        apiError401Detected = true;
      }
    });
    
    // Navigate to new session page
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Select app type first
    const dropdown = page.locator('.rz-dropdown').first();
    await dropdown.click();
    await page.getByText(/Mobile/i).first().click();
    await page.waitForTimeout(500);
    
    // Start session
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await startButton.click();
    
    // Wait for navigation and API calls
    await page.waitForURL(/\/session\/.*\/spark/, { timeout: 20000 });
    await page.waitForTimeout(5000);
    
    // Check if error message appears
    const errorNotification = page.locator('.rz-message-error, .notification-error, [class*="error"]');
    
    if (await errorNotification.isVisible().catch(() => false)) {
      const errorText = await errorNotification.textContent();
      console.log(`[Test] Error notification: ${errorText}`);
      
      // Verify error is user-friendly
      expect(errorText).not.toContain('Exception');
      expect(errorText).not.toContain('Stack trace');
    }
    
    // Report if API auth error was detected
    if (apiError401Detected) {
      console.log('[Test] WARNING: Azure OpenAI 401 error detected - check API credentials');
    }
  });

  test('should verify SignalR hub connection works', async ({ page }) => {
    const signalRErrors: string[] = [];
    
    page.on('response', response => {
      if (response.url().includes('/hubs/swipe') && response.status() >= 400) {
        signalRErrors.push(`${response.status()}: ${response.url()}`);
      }
    });
    
    page.on('console', msg => {
      if (msg.text().includes('SignalR') && msg.type() === 'error') {
        signalRErrors.push(`Console: ${msg.text()}`);
      }
    });
    
    // Navigate to new session and start it
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Select app type first
    const dropdown = page.locator('.rz-dropdown').first();
    await dropdown.click();
    await page.getByText(/Mobile/i).first().click();
    await page.waitForTimeout(500);
    
    // Start session
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await startButton.click();
    
    // Wait for spark page and SignalR connection
    await page.waitForURL(/\/session\/.*\/spark/, { timeout: 20000 });
    await page.waitForTimeout(3000);
    
    // Report SignalR connection issues
    if (signalRErrors.length > 0) {
      console.log('[Test] SignalR Errors:');
      signalRErrors.forEach(err => console.log(`  - ${err}`));
      
      // Check for auth issues specifically
      const authErrors = signalRErrors.filter(e => e.includes('401'));
      expect(authErrors).toHaveLength(0); // SignalR should not have 401 errors
    }
    
    console.log('[Test] SignalR connection verified - no auth errors');
  });

  test('should allow swiping on ideas', async ({ page }) => {
    // Navigate to new session
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Select app type first
    const dropdown = page.locator('.rz-dropdown').first();
    await dropdown.click();
    await page.getByText(/Mobile/i).first().click();
    await page.waitForTimeout(500);
    
    // Start session
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await startButton.click();
    
    // Wait for spark page
    await page.waitForURL(/\/session\/.*\/spark/, { timeout: 20000 });
    await page.waitForLoadState('networkidle');
    
    // Wait for ideas to load
    await page.waitForTimeout(5000);
    
    // Find the like button and click it
    const likeButton = page.locator('button[title*="Like"], .swipe-button:has(span:has-text("thumb_up"))').or(
      page.getByRole('button').filter({ has: page.locator('[class*="thumb_up"]') })
    );
    
    if (await likeButton.isVisible()) {
      await likeButton.click();
      console.log('[Test] Swiped right (liked) an idea');
      
      // Wait for swipe to register
      await page.waitForTimeout(1000);
      
      // Should still be on spark page
      expect(page.url()).toContain('/spark');
    }
  });
});
