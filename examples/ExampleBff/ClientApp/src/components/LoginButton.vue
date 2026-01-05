<script setup lang="ts">
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>

<template lang="pug">
button.btn.btn-loading(v-if="authStore.loading" disabled) Loading...
button.btn.btn-logout(v-else-if="authStore.isAuthenticated" @click="authStore.logout") Logout
button.btn.btn-login(v-else @click="authStore.login()") Login
</template>

<style scoped>
.btn {
  padding: 0.5rem 1rem;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
  transition: background 0.2s;
}

.btn-login {
  background: white;
  color: #1976d2;
}

.btn-login:hover {
  background: #f5f5f5;
}

.btn-logout {
  background: rgba(255,255,255,0.2);
  color: white;
  border: 1px solid rgba(255,255,255,0.3);
}

.btn-logout:hover {
  background: rgba(255,255,255,0.3);
}

.btn-loading {
  background: rgba(255,255,255,0.1);
  color: rgba(255,255,255,0.7);
  cursor: wait;
}
</style>
