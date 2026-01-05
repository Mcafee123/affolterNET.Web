/// <reference types="cypress" />

declare global {
  namespace Cypress {
    interface Chainable {
      /**
       * Login via Keycloak UI
       */
      login(username: string, password: string): Chainable<void>

      /**
       * Login as admin user
       */
      loginAsAdmin(): Chainable<void>

      /**
       * Login as standard user
       */
      loginAsUser(): Chainable<void>

      /**
       * Login as viewer user
       */
      loginAsViewer(): Chainable<void>

      /**
       * Logout from the application
       */
      logout(): Chainable<void>

      /**
       * Get the BFF user info
       */
      getBffUser(): Chainable<any>
    }
  }
}

// Login via Keycloak UI
Cypress.Commands.add('login', (username: string, password: string) => {
  const keycloakUrl = Cypress.env('keycloakUrl')

  // Visit the app
  cy.visit('/')

  // Click login button - this triggers navigation to /bff/account/login -> Keycloak
  cy.contains('button', 'Login').click()

  // Wait for Keycloak login page to appear
  cy.url({ timeout: 15000 }).should('include', 'realms/demo')

  // Now we're on Keycloak - fill in credentials
  cy.origin(keycloakUrl, { args: { username, password } }, ({ username, password }) => {
    cy.get('#username', { timeout: 10000 }).should('be.visible').type(username)
    cy.get('#password').type(password)
    cy.get('#kc-login').click()
  })

  // Wait for redirect back to app
  cy.url({ timeout: 15000 }).should('include', 'localhost:5004')

  // Wait for auth state to be loaded
  cy.contains('button', 'Logout', { timeout: 10000 }).should('be.visible')
})

// Login as admin
Cypress.Commands.add('loginAsAdmin', () => {
  cy.login(Cypress.env('adminUser'), Cypress.env('adminPassword'))
})

// Login as standard user
Cypress.Commands.add('loginAsUser', () => {
  cy.login(Cypress.env('standardUser'), Cypress.env('standardPassword'))
})

// Login as viewer
Cypress.Commands.add('loginAsViewer', () => {
  cy.login(Cypress.env('viewerUser'), Cypress.env('viewerPassword'))
})

// Logout
Cypress.Commands.add('logout', () => {
  cy.contains('button', 'Logout').click()

  // Wait for logout to complete and login button to appear
  cy.contains('button', 'Login', { timeout: 15000 }).should('be.visible')
})

// Get BFF user info
Cypress.Commands.add('getBffUser', () => {
  return cy.request('/bff/user').its('body')
})

export {}
