/// <reference types="cypress" />

describe('Home Page', () => {
  beforeEach(() => {
    cy.visit('/')
  })

  it('should display the home page', () => {
    cy.contains('h1', 'affolterNET.Web Example').should('be.visible')
  })

  it('should show navigation links', () => {
    cy.contains('a', 'Home').should('be.visible')
    cy.contains('a', 'Public API').should('be.visible')
    cy.contains('a', 'Protected').should('be.visible')
  })

  it('should show login button when not authenticated', () => {
    cy.contains('button', 'Login').should('be.visible')
  })

  it('should indicate user is not authenticated', () => {
    cy.contains('h2', 'Not Logged In').should('be.visible')
  })

  it('should navigate to public page', () => {
    cy.contains('a', 'Public API').click()
    cy.url().should('include', '/public')
  })
})
