/// <reference types="cypress" />

describe('BFF Health Check Priority over YARP', () => {
  // These tests verify health checks work even with a YARP catch-all route (/{**catch-all})
  // pointing to a non-existent backend. If health checks were proxied, we'd get 502.

  it('should return health check response from /health/ready, not proxied', () => {
    cy.request({
      url: '/health/ready',
      failOnStatusCode: false
    }).then((response) => {
      expect(response.status).to.eq(200)
      expect(response.body).to.have.property('status')
      expect(response.body.status).to.eq('Healthy')
    })
  })

  it('should return startup health check from BFF', () => {
    cy.request('/health/startup').then((response) => {
      expect(response.status).to.eq(200)
    })
  })

  it('should return liveness health check from BFF', () => {
    cy.request('/health/live').then((response) => {
      expect(response.status).to.eq(200)
    })
  })
})

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
