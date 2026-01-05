import { defineConfig } from 'cypress'

export default defineConfig({
  e2e: {
    baseUrl: 'https://localhost:5004',
    supportFile: 'cypress/support/e2e.ts',
    specPattern: 'cypress/e2e/**/*.cy.ts',
    viewportWidth: 1280,
    viewportHeight: 720,
    video: true,
    screenshotOnRunFailure: true,
    defaultCommandTimeout: 10000,
    requestTimeout: 10000,
    responseTimeout: 30000,
    chromeWebSecurity: false,
    experimentalModifyObstructiveThirdPartyCode: true,
    env: {
      // Keycloak URLs
      keycloakUrl: 'https://localhost:8443',
      keycloakRealm: 'demo',
      // Direct API URL (bypassing BFF proxy for testing)
      apiUrl: 'https://localhost:5003',
      // Test users
      adminUser: 'admin@example.com',
      adminPassword: 'admin123',
      standardUser: 'user@example.com',
      standardPassword: 'user123',
      viewerUser: 'viewer@example.com',
      viewerPassword: 'viewer123'
    },
    setupNodeEvents(on, config) {
      on('task', {
        log(message) {
          console.log(message)
          return null
        }
      })
    }
  }
})
