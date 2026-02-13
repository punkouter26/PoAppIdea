# PoAppIdea E2E Tests (Playwright)

End-to-end tests for PoAppIdea using Playwright with TypeScript.

## Configuration

- **Browsers**: Chromium and Mobile Chrome only (per project requirements)
- **Mode**: Headed during development, headless in CI
- **Base URL**: https://localhost:5001

## Quick Start

```bash
# Install dependencies
npm install

# Install Playwright browsers (Chromium only)
npx playwright install chromium

# Run all tests (headed for development)
npm run test:headed

# Run capped smoke suite (CI fast path)
npm run test:smoke

# Run full regression suite
npm run test:regression

# Run only Chromium desktop tests
npm run test:chromium

# Run only mobile tests
npm run test:mobile

# Run in debug mode with inspector
npm run test:debug

# Open interactive UI mode
npm run test:ui

# Generate tests with codegen
npm run codegen
```

## Test Structure

```
tests/
├── home.spec.ts        # Home page critical paths
├── navigation.spec.ts  # Navigation between pages
├── session.spec.ts     # Session creation flow
├── responsive.spec.ts  # Mobile responsiveness
└── api-health.spec.ts  # API endpoint smoke tests
```

## Running Specific Tests

```bash
# Run a single test file
npx playwright test tests/home.spec.ts

# Run tests with a specific title
npx playwright test -g "should display the app title"

# Run tests in headed mode (see browser)
npx playwright test --headed

# Run with trace on (for debugging)
npx playwright test --trace on
```

## CI Integration

Tests run headless in CI with the `CI` environment variable set:

```bash
CI=true npm test
```

## Development Workflow

1. Start the local server: `dotnet watch run --project ../../src/PoAppIdea.Web`
2. Run tests in headed mode: `npm run test:headed`
3. Use codegen for new tests: `npm run codegen`
4. View reports: `npm run report`

## Reports

After running tests, view the HTML report:

```bash
npm run report
```

Reports are generated in `playwright-report/`.
