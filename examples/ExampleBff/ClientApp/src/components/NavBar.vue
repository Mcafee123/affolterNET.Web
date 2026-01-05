<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'
import LoginButton from './LoginButton.vue'

const authStore = useAuthStore()
</script>

<template lang="pug">
nav.navbar
  .navbar-brand
    RouterLink.brand-link(to="/") Example BFF
  .navbar-menu
    RouterLink.nav-link(to="/") Home
    RouterLink.nav-link(to="/public") Public API
    RouterLink.nav-link(to="/protected") Protected
    RouterLink.nav-link(v-if="authStore.hasPermission('admin-resource', 'view')" to="/admin") Admin
  .navbar-auth
    span.user-info(v-if="authStore.isAuthenticated") {{ authStore.email || authStore.username }}
    LoginButton
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
