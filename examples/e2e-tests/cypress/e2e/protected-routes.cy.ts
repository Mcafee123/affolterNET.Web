/// <reference types="cypress" />

describe('Protected Routes', () => {
  const apiUrl = Cypress.env('apiUrl')

  describe('When not authenticated', () => {
    it('should show protected link in navigation (route guards in SPA)', () => {
      cy.visit('/')
      cy.contains('a', 'Protected').should('be.visible')
    })

    it('should show not logged in message on home page', () => {
      cy.visit('/')
      cy.contains('h2', 'Not Logged In').should('be.visible')
    })

    it('should return 401 for protected API endpoints', () => {
      cy.request({
        url: `${apiUrl}/api/protected`,
        failOnStatusCode: false
      }).then((response) => {
        expect(response.status).to.eq(401)
      })
    })
  })

  describe('When authenticated', { tags: ['@requires-keycloak'] }, () => {
    it('should access protected page after login', () => {
      cy.loginAsUser()
      cy.contains('a', 'Protected').click()
      cy.url().should('include', '/protected')
    })

    it('should display user info on home page', () => {
      cy.loginAsUser()
      cy.visit('/')
      cy.contains('user@example.com').should('be.visible')
    })
  })
})
