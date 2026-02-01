import { test, expect } from '@playwright/test';

/**
 * E2E Tests: Session Creation - Critical User Path
 * 
 * Scope: Verify the session creation flow works end-to-end.
 * This is the primary user journey through the app.
 */

const BASE_URL = 'https://localhost:5001';

test.describe('Session Creation Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Authenticate using test-login endpoint (Development only)
    await page.goto(`${BASE_URL}/auth/test-login?userId=e2e-session-test&email=session@test.com&name=Session%20Test%20User&returnUrl=/`);
    await page.waitForLoadState('networkidle');
  });

  test('should display session creation form', async ({ page }) => {
    // Arrange & Act: Navigate to new session page
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Assert: Form elements are present - looking for App Type dropdown
    const appTypeDropdown = page.getByLabel(/App Type/i).or(
      page.locator('.rz-dropdown')
    );
    
    // Dropdown should be visible
    await expect(appTypeDropdown.first()).toBeVisible({ timeout: 10000 });
  });

  test('should show start session button', async ({ page }) => {
    // Arrange & Act
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Assert: Submit button exists (may be disabled initially)
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await expect(startButton).toBeVisible();
  });

  test('should enable button after selecting app type', async ({ page }) => {
    // Arrange
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Act: Select an app type from dropdown
    const dropdown = page.locator('.rz-dropdown').first();
    await dropdown.click({ force: true });
    // Wait for dropdown to open and select option (use force for mobile visibility issues)
    await page.waitForSelector('.rz-dropdown-item', { state: 'visible', timeout: 5000 });
    await page.locator('.rz-dropdown-item').filter({ hasText: /Mobile/i }).first().click({ force: true });
    
    // Assert: Start button becomes enabled
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await expect(startButton).toBeEnabled();
  });

  test.skip('should navigate to spark page after starting session', async ({ page }) => {
    // SKIP: This test requires Azure Storage backend to create session
    // Run manually with a running server: npx playwright test session.spec.ts -g "navigate to spark"
    
    // Arrange
    await page.goto(`${BASE_URL}/session/new`);
    await page.waitForLoadState('networkidle');
    
    // Act: Select app type and start session
    const dropdown = page.locator('.rz-dropdown').first();
    await dropdown.click();
    await page.getByText(/Mobile/i).first().click();
    
    const startButton = page.getByRole('button', { name: /Start Session/i });
    await startButton.click();
    
    // Assert: Navigate to spark page
    await page.waitForURL(/\/session\/.*\/spark/, { timeout: 20000 });
    expect(page.url()).toContain('/spark');
  });
});
