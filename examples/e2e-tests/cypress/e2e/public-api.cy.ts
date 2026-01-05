/// <reference types="cypress" />

describe('Public API', () => {
  const apiUrl = Cypress.env('apiUrl')

  describe('Public Endpoints (Direct API)', () => {
    it('should access public endpoint without authentication', () => {
      cy.request(`${apiUrl}/api/public`).then((response) => {
        expect(response.status).to.eq(200)
        expect(response.body).to.have.property('message')
        expect(response.body.message).to.include('Public')
      })
    })

    it('should access health endpoint', () => {
      cy.request(`${apiUrl}/api/public/health`).then((response) => {
        expect(response.status).to.eq(200)
      })
    })
  })

  describe('Public Page (SPA)', () => {
    it('should display public page without authentication', () => {
      cy.visit('/public')
      cy.contains(/public/i).should('be.visible')
    })

    it('should be accessible from navigation', () => {
      cy.visit('/')
      cy.contains('a', 'Public API').click()
      cy.url().should('include', '/public')
    })
  })
})
