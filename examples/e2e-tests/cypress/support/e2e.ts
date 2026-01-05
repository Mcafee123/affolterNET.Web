/// <reference types="cypress" />

// Import commands
import './commands'

// Handle uncaught exceptions from the app
Cypress.on('uncaught:exception', (err, runnable) => {
  // Don't fail tests on uncaught exceptions from the app
  console.log('Uncaught exception:', err.message)
  return false
})

// Log test name before each test
beforeEach(() => {
  cy.task('log', `Running: ${Cypress.currentTest.title}`)
})
