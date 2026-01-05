<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import LoginButton from './LoginButton.vue'

const authStore = useAuthStore()
</script>

<template>
  <nav class="navbar">
    <div class="navbar-brand">
      <RouterLink to="/" class="brand-link">Example BFF</RouterLink>
    </div>

    <div class="navbar-menu">
      <RouterLink to="/" class="nav-link">Home</RouterLink>
      <RouterLink to="/public" class="nav-link">Public API</RouterLink>
      <RouterLink to="/protected" class="nav-link">Protected</RouterLink>
      <RouterLink
        v-if="authStore.hasPermission('admin-resource', 'view')"
        to="/admin"
        class="nav-link"
      >
        Admin
      </RouterLink>
    </div>

    <div class="navbar-auth">
      <span v-if="authStore.isAuthenticated" class="user-info">
        {{ authStore.email || authStore.username }}
      </span>
      <LoginButton />
    </div>
  </nav>
</template>

<style scoped>
.navbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 2rem;
  background: #1976d2;
  color: white;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.navbar-brand {
  font-size: 1.25rem;
  font-weight: bold;
}

.brand-link {
  color: white;
  text-decoration: none;
}

.navbar-menu {
  display: flex;
  gap: 1rem;
}

.nav-link {
  color: rgba(255,255,255,0.9);
  text-decoration: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.nav-link:hover {
  background: rgba(255,255,255,0.1);
}

.nav-link.router-link-active {
  background: rgba(255,255,255,0.2);
}

.navbar-auth {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.user-info {
  font-size: 0.9rem;
  opacity: 0.9;
}
</style>
