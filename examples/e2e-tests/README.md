# E2E Tests

End-to-end tests for the affolterNET.Web example applications using Cypress.

## Prerequisites

- Node.js 18+
- Docker containers running (Keycloak, API, BFF)

## Setup

```bash
# Install dependencies
npm install

# Verify Cypress installation
npx cypress verify
```

## Running Tests

### Interactive Mode (Recommended for development)

```bash
npm run cy:open
# or
npm run test:interactive
```

This opens the Cypress Test Runner where you can:
- Select tests to run
- Watch tests execute in real-time
- Debug failures with time-travel

### Headless Mode (CI/CD)

```bash
npm run test
# or
npm run cy:run
```

### With Specific Browser

```bash
npm run cy:run:chrome
npm run cy:run:firefox
```

### Headed Mode (See browser during headless run)

```bash
npm run cy:run:headed
```

## Test Structure

```
cypress/
├── e2e/
│   ├── home.cy.ts              # Home page tests
│   ├── authentication.cy.ts    # Login/logout flow tests
│   ├── protected-routes.cy.ts  # Protected route access tests
│   ├── permissions.cy.ts       # Permission-based access tests
│   └── public-api.cy.ts        # Public API tests
├── fixtures/
│   └── users.json              # Test user data
└── support/
    ├── commands.ts             # Custom Cypress commands
    └── e2e.ts                  # E2E test configuration
```

## Test Users

| User | Password | Roles | Permissions |
|------|----------|-------|-------------|
| admin@example.com | admin123 | admin, user, viewer | All |
| user@example.com | user123 | user, viewer | user-resource (read, create, update) |
| viewer@example.com | viewer123 | viewer | user-resource (read only) |

## Custom Commands

The following custom commands are available:

```typescript
// Login via Keycloak
cy.login('user@example.com', 'password')

// Login as specific user type
cy.loginAsAdmin()
cy.loginAsUser()
cy.loginAsViewer()

// Logout
cy.logout()

// Get BFF user info
cy.getBffUser().then((user) => {
  expect(user.isAuthenticated).to.be.true
})
```

## Configuration

Edit `cypress.config.ts` to change:
- `baseUrl` - Application URL (default: https://localhost:5004)
- `env.keycloakUrl` - Keycloak URL (default: https://localhost:8443)
- Test user credentials

## Running Before Tests

Ensure the Docker containers are running:

```bash
cd ../
docker-compose up -d
```

Wait for all services to be healthy before running tests.

## Troubleshooting

### Certificate Errors

Cypress is configured with `chromeWebSecurity: false` to handle self-signed certificates.

### Login Redirects Failing

- Ensure Keycloak is running and healthy
- Check that redirect URIs are configured in Keycloak
- Verify the `keycloakUrl` env variable matches your setup

### Tests Timing Out

- Increase `defaultCommandTimeout` in cypress.config.ts
- Check that API endpoints are responding
- Verify network connectivity between services
