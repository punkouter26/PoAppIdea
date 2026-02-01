import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright Configuration for PoAppIdea E2E Tests
 * 
 * Constraints:
 * - Chromium desktop only (no Firefox/WebKit)
 * - Mobile Chrome for responsive testing
 * - Runs headed during development for visual verification
 * 
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './tests',
  
  /* Run tests in parallel */
  fullyParallel: true,
  
  /* Fail the build on CI if you accidentally left test.only in the source code */
  forbidOnly: !!process.env.CI,
  
  /* Retry on CI only */
  retries: process.env.CI ? 2 : 0,
  
  /* Limit workers on CI */
  workers: process.env.CI ? 1 : undefined,
  
  /* Reporter configuration */
  reporter: [
    ['html', { open: 'never' }],
    ['list']
  ],
  
  /* Shared settings for all projects */
  use: {
    /* Base URL for the local development server */
    baseURL: 'https://localhost:5001',
    
    /* Ignore HTTPS errors for self-signed certificates */
    ignoreHTTPSErrors: true,
    
    /* Collect trace on first retry */
    trace: 'on-first-retry',
    
    /* Take screenshot on failure */
    screenshot: 'only-on-failure',
    
    /* Video on failure */
    video: 'on-first-retry',
  },

  /* Configure projects - Chromium and Mobile only */
  projects: [
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        // Run headed during development for visual verification
        headless: !!process.env.CI,
      },
    },
    {
      name: 'Mobile Chrome',
      use: { 
        ...devices['Pixel 7'],
        headless: !!process.env.CI,
      },
    },
  ],

  /* Run local dev server before starting the tests */
  webServer: {
    command: 'dotnet run --project ../../src/PoAppIdea.Web/PoAppIdea.Web.csproj',
    url: 'https://localhost:5001',
    reuseExistingServer: !process.env.CI,
    ignoreHTTPSErrors: true,
    timeout: 120 * 1000, // 2 minutes for initial build
    env: {
      ASPNETCORE_ENVIRONMENT: 'Development',
    },
  },
});
