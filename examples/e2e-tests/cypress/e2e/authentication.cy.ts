/// <reference types="cypress" />

describe('Authentication', () => {
  describe('BFF User Endpoint', () => {
    it('should return unauthenticated when not logged in', () => {
      cy.visit('/')
      cy.getBffUser().then((user) => {
        expect(user.isAuthenticated).to.be.false
        expect(user.claims).to.be.empty
      })
    })
  })

  describe('Login Flow', { tags: ['@requires-keycloak'] }, () => {
    it('should redirect to Keycloak when clicking login', () => {
      cy.visit('/')
      cy.contains('button', 'Login').click()
      cy.url({ timeout: 15000 }).should('include', 'realms/demo')
    })

    it('should login as admin successfully', () => {
      cy.loginAsAdmin()
      cy.contains('button', 'Logout').should('be.visible')
      cy.contains('admin@example.com').should('be.visible')
    })

    it('should login as standard user successfully', () => {
      cy.loginAsUser()
      cy.contains('button', 'Logout').should('be.visible')
      cy.contains('user@example.com').should('be.visible')
    })

    it('should login as viewer successfully', () => {
      cy.loginAsViewer()
      cy.contains('button', 'Logout').should('be.visible')
      cy.contains('viewer@example.com').should('be.visible')
    })
  })

  describe('Logout Flow', { tags: ['@requires-keycloak'] }, () => {
    it('should logout successfully', () => {
      cy.loginAsAdmin()
      cy.logout()
      cy.contains('button', 'Login').should('be.visible')
    })
  })
})
