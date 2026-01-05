<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>

<template>
  <div class="home">
    <h1>affolterNET.Web Example</h1>
    <p class="subtitle">BFF + Vue SPA Authentication Demo</p>

    <div class="auth-status">
      <div v-if="authStore.loading" class="loading">
        Loading authentication status...
      </div>
      <div v-else-if="authStore.isAuthenticated" class="authenticated">
        <h2>Welcome, {{ authStore.userContext?.name || authStore.username }}!</h2>
        <div class="user-details">
          <p><strong>Email:</strong> {{ authStore.email }}</p>
          <p><strong>Roles:</strong> {{ authStore.roles.join(', ') || 'None' }}</p>
          <p><strong>Permissions:</strong></p>
          <ul v-if="authStore.permissions.length">
            <li v-for="perm in authStore.permissions" :key="`${perm.resource}:${perm.action}`">
              {{ perm.resource }}:{{ perm.action }}
            </li>
          </ul>
          <p v-else>No permissions assigned</p>
        </div>
      </div>
      <div v-else class="not-authenticated">
        <h2>Not Logged In</h2>
        <p>Click the Login button to authenticate with Keycloak.</p>
        <button class="btn-primary" @click="authStore.login()">Login Now</button>
      </div>
    </div>

    <div class="features">
      <h3>Example Features</h3>
      <div class="feature-grid">
        <RouterLink to="/public" class="feature-card">
          <h4>Public API</h4>
          <p>Call public API endpoints without authentication</p>
        </RouterLink>
        <RouterLink to="/protected" class="feature-card">
          <h4>Protected API</h4>
          <p>Call API endpoints that require authentication</p>
        </RouterLink>
        <RouterLink
          v-if="authStore.hasPermission('admin-resource', 'view')"
          to="/admin"
          class="feature-card"
        >
          <h4>Admin API</h4>
          <p>Call API endpoints that require specific permissions</p>
        </RouterLink>
      </div>
    </div>
  </div>
</template>

<style scoped>
.home {
  text-align: center;
}

h1 {
  color: #1976d2;
  margin-bottom: 0.5rem;
}

.subtitle {
  color: #666;
  margin-bottom: 2rem;
}

.auth-status {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  margin-bottom: 2rem;
}

.loading {
  color: #666;
}

.authenticated h2 {
  color: #2e7d32;
  margin-bottom: 1rem;
}

.user-details {
  text-align: left;
  max-width: 400px;
  margin: 0 auto;
}

.user-details ul {
  margin: 0.5rem 0;
  padding-left: 1.5rem;
}

.not-authenticated h2 {
  color: #666;
  margin-bottom: 1rem;
}

.btn-primary {
  background: #1976d2;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
  margin-top: 1rem;
}

.btn-primary:hover {
  background: #1565c0;
}

.features {
  text-align: left;
}

.features h3 {
  margin-bottom: 1rem;
}

.feature-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.feature-card {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  text-decoration: none;
  color: inherit;
  transition: transform 0.2s, box-shadow 0.2s;
}

.feature-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0,0,0,0.15);
}

.feature-card h4 {
  color: #1976d2;
  margin-bottom: 0.5rem;
}

.feature-card p {
  color: #666;
  font-size: 0.9rem;
}
</style>
