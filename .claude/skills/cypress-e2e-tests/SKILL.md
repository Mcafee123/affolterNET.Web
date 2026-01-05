# Cypress E2E Tests Skill

This skill documents the end-to-end testing setup using Cypress for the affolterNET.Web example applications.

## Overview

The E2E tests validate the complete authentication and authorization flow including:

- BFF cookie-based authentication with Keycloak
- Login/logout flows via Keycloak UI
- Permission-based access control (RPT tokens)
- Protected route access
- API endpoint authorization

## Project Location

```
examples/e2e-tests/
├── cypress/
│   ├── e2e/                    # Test specifications
│   │   ├── home.cy.ts          # Home page tests
│   │   ├── authentication.cy.ts # Login/logout flow tests
│   │   ├── protected-routes.cy.ts # Protected route access tests
│   │   ├── permissions.cy.ts   # Permission-based access tests
│   │   └── public-api.cy.ts    # Public API tests
│   └── support/
│       ├── commands.ts         # Custom Cypress commands
│       └── e2e.ts              # E2E test configuration
├── cypress.config.ts           # Cypress configuration
├── package.json                # Dependencies and scripts
└── tsconfig.json               # TypeScript configuration
```

## Running Tests

### Prerequisites

1. Node.js 18+
2. Docker containers running (Keycloak, API, BFF):
   ```bash
   cd examples/
   docker-compose up -d
   ```
3. Install dependencies:
   ```bash
   cd examples/e2e-tests/
   npm install
   ```

### Commands

```bash
# Interactive mode (recommended for development)
npm run cy:open
npm run test:interactive

# Headless mode (CI/CD)
npm run test
npm run cy:run

# Specific browser
npm run cy:run:chrome
npm run cy:run:firefox

# Headed mode (see browser during headless run)
npm run cy:run:headed
```

## Configuration

### cypress.config.ts

```typescript
export default defineConfig({
  e2e: {
    baseUrl: 'https://localhost:5004',        // BFF application URL
    supportFile: 'cypress/support/e2e.ts',
    specPattern: 'cypress/e2e/**/*.cy.ts',
    viewportWidth: 1280,
    viewportHeight: 720,
    video: true,
    screenshotOnRunFailure: true,
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 30000,
    chromeWebSecurity: false,                  // Handle self-signed certs
    experimentalModifyObstructiveThirdPartyCode: true,
    env: {
      keycloakUrl: 'https://localhost:8443',
      keycloakRealm: 'demo',
      apiUrl: 'https://localhost:5003',        // Direct API URL
      adminUser: 'admin@example.com',
      adminPassword: 'admin123',
      standardUser: 'user@example.com',
      standardPassword: 'user123',
      viewerUser: 'viewer@example.com',
      viewerPassword: 'viewer123'
    }
  }
})
```

## Custom Commands

Custom Cypress commands are defined in `cypress/support/commands.ts`:

### Authentication Commands

```typescript
// Login via Keycloak UI with specific credentials
cy.login('user@example.com', 'password')

// Login as predefined user types
cy.loginAsAdmin()    // admin@example.com
cy.loginAsUser()     // user@example.com (standard user)
cy.loginAsViewer()   // viewer@example.com

// Logout
cy.logout()

// Get BFF user info
cy.getBffUser().then((user) => {
  expect(user.isAuthenticated).to.be.true
  expect(user.claims).to.not.be.empty
})
```

### How Login Works

The `cy.login()` command handles the full OIDC flow:

1. Visits the application (`/`)
2. Clicks the Login button (triggers redirect to `/bff/account/login`)
3. Waits for Keycloak login page (`realms/demo`)
4. Uses `cy.origin()` to interact with Keycloak domain
5. Fills credentials and submits form
6. Waits for redirect back to app
7. Verifies Logout button appears (auth complete)

```typescript
Cypress.Commands.add('login', (username: string, password: string) => {
  const keycloakUrl = Cypress.env('keycloakUrl')
  cy.visit('/')
  cy.contains('button', 'Login').click()
  cy.url({ timeout: 15000 }).should('include', 'realms/demo')

  cy.origin(keycloakUrl, { args: { username, password } }, ({ username, password }) => {
    cy.get('#username', { timeout: 10000 }).should('be.visible').type(username)
    cy.get('#password').type(password)
    cy.get('#kc-login').click()
  })

  cy.url({ timeout: 15000 }).should('include', 'localhost:5004')
  cy.contains('button', 'Logout', { timeout: 10000 }).should('be.visible')
})
```

## Test Users

| User | Password | Roles | Permissions |
|------|----------|-------|-------------|
| admin@example.com | admin123 | admin, user, viewer | All (admin-resource, user-resource) |
| user@example.com | user123 | user, viewer | user-resource (read, create, update) |
| viewer@example.com | viewer123 | viewer | user-resource (read only) |

## Writing Tests

### Test Structure

```typescript
/// <reference types="cypress" />

describe('Feature Name', () => {
  describe('Scenario Group', () => {
    it('should do something specific', () => {
      // Test implementation
    })
  })

  // Tests requiring Keycloak
  describe('Authenticated Tests', { tags: ['@requires-keycloak'] }, () => {
    it('should access protected resource', () => {
      cy.loginAsAdmin()
      // Test implementation
    })
  })
})
```

### Testing API Endpoints

```typescript
// Test unauthenticated access
cy.request({
  url: `${apiUrl}/api/permission/admin`,
  failOnStatusCode: false
}).then((response) => {
  expect(response.status).to.eq(401)
})

// Test BFF user endpoint
cy.getBffUser().then((user) => {
  expect(user.isAuthenticated).to.be.false
  expect(user.claims).to.be.empty
})
```

### Testing Permission-Based UI

```typescript
describe('Admin Resource Access', () => {
  describe('As admin user', () => {
    it('should show admin link in navigation', () => {
      cy.loginAsAdmin()
      cy.contains('a', 'Admin').should('be.visible')
    })
  })

  describe('As standard user', () => {
    it('should not show admin link in navigation', () => {
      cy.loginAsUser()
      cy.contains('a', 'Admin').should('not.exist')
    })
  })
})
```

## Cross-Origin Handling

Cypress requires special handling for cross-origin navigation (e.g., to Keycloak):

```typescript
// Use cy.origin() for cross-origin interactions
cy.origin(keycloakUrl, { args: { username, password } }, ({ username, password }) => {
  // Code runs in Keycloak's origin context
  cy.get('#username').type(username)
  cy.get('#password').type(password)
  cy.get('#kc-login').click()
})
```

**Important Settings:**
- `chromeWebSecurity: false` - Allows navigation to different origins
- `experimentalModifyObstructiveThirdPartyCode: true` - Handles third-party scripts

## Error Handling

Global error handling is configured in `cypress/support/e2e.ts`:

```typescript
// Don't fail tests on uncaught exceptions from the app
Cypress.on('uncaught:exception', (err, runnable) => {
  console.log('Uncaught exception:', err.message)
  return false
})
```

## Troubleshooting

### Certificate Errors

Cypress is configured with `chromeWebSecurity: false` to handle self-signed certificates.

### Login Redirects Failing

- Ensure Keycloak is running and healthy
- Check that redirect URIs are configured in Keycloak client
- Verify the `keycloakUrl` env variable matches your setup

### Tests Timing Out

- Increase `defaultCommandTimeout` in cypress.config.ts
- Check that API endpoints are responding
- Verify network connectivity between services

### "cy.origin() requires the host to be same-origin"

- Ensure `experimentalModifyObstructiveThirdPartyCode: true` is set
- Verify the Keycloak URL in env matches the actual redirect

### Keycloak Permission Tests Failing

RPT-based permission tests may fail if authorization services weren't properly configured. See `.claude/skills/keycloak-configuration/SKILL.md` for details on configuring authorization services manually.

## Related Files

- `examples/docker-compose.yml` - Docker services (Keycloak, API, BFF)
- `examples/keycloak/realm-export.json` - Keycloak realm configuration
- `examples/ExampleBff/` - BFF application under test
- `examples/ExampleApi/` - API application under test
- `.claude/skills/keycloak-configuration/SKILL.md` - Keycloak setup guide
