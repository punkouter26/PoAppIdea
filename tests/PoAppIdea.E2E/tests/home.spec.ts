import { test, expect } from '@playwright/test';

/**
 * E2E Tests: Home Page - Critical User Path
 * 
 * Scope: Verify the landing page loads and displays key elements.
 * These tests run on Chromium and Mobile Chrome only.
 */

const BASE_URL = 'https://localhost:5001';

test.describe('Home Page', () => {
  test.beforeEach(async ({ page }) => {
    // Authenticate using test-login endpoint (Development only)
    await page.goto(`${BASE_URL}/auth/test-login?userId=e2e-home-test&email=home@test.com&name=Home%20Test%20User&returnUrl=/`);
    await page.waitForLoadState('networkidle');
  });

  test('should display the app title', async ({ page }) => {
    // Assert: Page title contains app name
    await expect(page).toHaveTitle(/PoAppIdea/);
  });

  test('should show welcome heading', async ({ page }) => {
    // Assert: Welcome message is visible
    const heading = page.getByRole('heading', { name: /Welcome to PoAppIdea/i });
    await expect(heading).toBeVisible();
  });

  test('should display navigation links', async ({ page, isMobile }) => {
    // On mobile, navigation is hidden in a collapsible sidebar
    // The test verifies the nav elements exist in the DOM
    
    if (isMobile) {
      // Mobile: Click sidebar toggle to expand nav menu
      const sidebarToggle = page.locator('.rz-sidebar-toggle');
      await sidebarToggle.click();
      await page.waitForTimeout(600); // Allow Radzen sidebar expansion animation
      
      // After toggle, wait for sidebar to be expanded
      await page.waitForSelector('.rz-sidebar:not(.rz-sidebar-collapsed)', { timeout: 5000 }).catch(() => {
        // Sidebar may use different class naming
      });
    }
    
    // Assert: Key navigation elements exist in DOM (use count check for mobile flexibility)
    const homeLink = page.locator('[data-testid="nav-home"]');
    const newSessionLink = page.locator('[data-testid="nav-new-session"]');
    const mySessionsLink = page.locator('[data-testid="nav-my-sessions"]');
    
    // On desktop, check visibility; on mobile check DOM presence
    if (isMobile) {
      // Verify elements exist in DOM (even if collapsed sidebar hides them)
      await expect(homeLink).toHaveCount(1);
      await expect(newSessionLink).toHaveCount(1);
      await expect(mySessionsLink).toHaveCount(1);
    } else {
      await expect(homeLink.first()).toBeVisible({ timeout: 10000 });
      await expect(newSessionLink.first()).toBeVisible({ timeout: 10000 });
      await expect(mySessionsLink.first()).toBeVisible({ timeout: 10000 });
    }
  });

  test('should show user menu when authenticated', async ({ page }) => {
    // Assert: User name or logout option is available (authenticated)
    const userElement = page.getByText(/Home Test User/i).or(
      page.getByRole('button', { name: /Logout/i })
    ).or(page.getByRole('link', { name: /Logout/i }));
    await expect(userElement.first()).toBeVisible();
  });

  test('should display "How It Works" section', async ({ page }) => {
    // Assert: Onboarding content is visible
    await expect(page.getByText(/How It Works/i)).toBeVisible();
    await expect(page.getByText(/Swipe Ideas/i)).toBeVisible();
  });
});
