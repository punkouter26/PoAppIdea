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

  test('should display navigation links', async ({ page }) => {
    // Assert: Key navigation elements are present
    await expect(page.getByRole('link', { name: /Home/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /New Session/i })).toBeVisible();
    await expect(page.getByRole('link', { name: /My Sessions/i })).toBeVisible();
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
