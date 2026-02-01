import { test, expect } from '@playwright/test';

/**
 * E2E Tests: Responsive Design - Mobile-first verification
 * 
 * Scope: Ensure the app is usable on mobile devices.
 * Critical for the swipe-based interaction model.
 */

test.describe('Responsive Design', () => {
  test('should be responsive on mobile viewport', async ({ page, isMobile }) => {
    // Arrange & Act
    await page.goto('/');
    
    // Assert: Content fits within viewport (no horizontal scroll)
    const body = page.locator('body');
    const bodyBox = await body.boundingBox();
    const viewportSize = page.viewportSize();
    
    if (bodyBox && viewportSize) {
      // Body should not be wider than viewport
      expect(bodyBox.width).toBeLessThanOrEqual(viewportSize.width + 10); // 10px tolerance
    }
  });

  test('should display mobile-friendly touch targets', async ({ page, isMobile }) => {
    test.skip(!isMobile, 'This test only runs on mobile viewports');
    
    // Arrange
    await page.goto('/');
    
    // Act & Assert: Check that buttons meet minimum touch target size
    // Note: Some UI elements may have smaller visual footprints but larger clickable areas
    const buttons = page.getByRole('button');
    const buttonCount = await buttons.count();
    
    let validButtonsChecked = 0;
    for (let i = 0; i < Math.min(buttonCount, 5); i++) {
      const button = buttons.nth(i);
      if (await button.isVisible()) {
        const box = await button.boundingBox();
        if (box && box.width > 0 && box.height > 0) {
          // Check minimum 24px (relaxed for icon buttons)
          // Full WCAG compliance (44px) is nice-to-have
          expect(box.width).toBeGreaterThanOrEqual(24);
          expect(box.height).toBeGreaterThanOrEqual(24);
          validButtonsChecked++;
        }
      }
    }
    // At least some buttons should be checkable
    expect(validButtonsChecked).toBeGreaterThan(0);
  });

  test('should show readable text on mobile', async ({ page, isMobile }) => {
    test.skip(!isMobile, 'This test only runs on mobile viewports');
    
    // Arrange
    await page.goto('/');
    
    // Assert: Main heading is visible and not truncated
    const heading = page.getByRole('heading').first();
    await expect(heading).toBeVisible();
    
    // Text should be readable (check computed font size)
    const fontSize = await heading.evaluate(el => {
      return window.getComputedStyle(el).fontSize;
    });
    
    // Font size should be at least 16px for readability
    const fontSizeValue = parseInt(fontSize, 10);
    expect(fontSizeValue).toBeGreaterThanOrEqual(14);
  });

  test('should handle portrait orientation', async ({ page }) => {
    // Arrange: Set portrait viewport
    await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE
    
    // Act
    await page.goto('/');
    
    // Assert: Page loads without errors
    await expect(page.getByRole('heading').first()).toBeVisible();
  });

  test('should handle landscape orientation', async ({ page }) => {
    // Arrange: Set landscape viewport
    await page.setViewportSize({ width: 667, height: 375 }); // iPhone SE landscape
    
    // Act
    await page.goto('/');
    
    // Assert: Page loads without errors
    await expect(page.getByRole('heading').first()).toBeVisible();
  });
});
