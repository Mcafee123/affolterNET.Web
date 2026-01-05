/// <reference types="cypress" />

describe('Permission-Based Access', () => {
  const apiUrl = Cypress.env('apiUrl')

  describe('API Permission Endpoints (No Auth)', () => {
    it('should return 401 for admin endpoint without auth', () => {
      cy.request({
        url: `${apiUrl}/api/permission/admin`,
        failOnStatusCode: false
      }).then((response) => {
        expect(response.status).to.eq(401)
      })
    })

    it('should return 401 for user endpoint without auth', () => {
      cy.request({
        url: `${apiUrl}/api/permission/user`,
        failOnStatusCode: false
      }).then((response) => {
        expect(response.status).to.eq(401)
      })
    })
  })

  describe('Admin Link Visibility (No Auth)', () => {
    it('should not show admin link when not authenticated', () => {
      cy.visit('/')
      cy.contains('a', 'Admin').should('not.exist')
    })
  })

  /**
   * Permission-based tests using Keycloak's Authorization Services (RPT tokens).
   *
   * KNOWN LIMITATION: Keycloak's authorization services (resources, policies, permissions)
   * are difficult to configure correctly via JSON realm import. The role policies
   * reference realm roles by name, but the import process doesn't always link them
   * correctly, resulting in "access_denied" / "not_authorized" errors when requesting
   * RPT tokens.
   *
   * To enable these tests:
   * 1. Open Keycloak Admin Console: https://localhost:8443
   * 2. Select the "demo" realm
   * 3. Navigate to Clients > bff-client > Authorization tab
   * 4. Manually verify/recreate the role policies, ensuring they reference the correct roles
   * 5. Test the permissions via the "Evaluate" tab
   *
   * The role-based tests (User Roles Display) work correctly because they use
   * standard OIDC claims, not RPT-based permissions.
   */
  describe('Admin Resource Access', { tags: ['@requires-keycloak'] }, () => {
    describe('As admin user', () => {
      it.skip('should show admin link in navigation (requires manual Keycloak authorization setup)', () => {
        cy.loginAsAdmin()
        cy.contains('a', 'Admin').should('be.visible')
      })

      it.skip('should access admin page (requires manual Keycloak authorization setup)', () => {
        cy.loginAsAdmin()
        cy.contains('a', 'Admin').click()
        cy.url().should('include', '/admin')
      })

      it.skip('should show admin permissions on home page (requires manual Keycloak authorization setup)', () => {
        cy.loginAsAdmin()
        cy.visit('/')
        cy.contains('admin-resource').should('be.visible')
      })
    })

    describe('As standard user', () => {
      it('should not show admin link in navigation', () => {
        cy.loginAsUser()
        cy.contains('a', 'Admin').should('not.exist')
      })
    })

    describe('As viewer user', () => {
      it('should not show admin link in navigation', () => {
        cy.loginAsViewer()
        cy.contains('a', 'Admin').should('not.exist')
      })
    })
  })

  describe('User Roles Display', { tags: ['@requires-keycloak'] }, () => {
    it('should show admin roles for admin user', () => {
      cy.loginAsAdmin()
      cy.visit('/')
      cy.contains('admin').should('be.visible')
    })

    it('should show user roles for standard user', () => {
      cy.loginAsUser()
      cy.visit('/')
      cy.contains('user').should('be.visible')
    })

    it('should show viewer role for viewer user', () => {
      cy.loginAsViewer()
      cy.visit('/')
      cy.contains('viewer').should('be.visible')
    })
  })
})
