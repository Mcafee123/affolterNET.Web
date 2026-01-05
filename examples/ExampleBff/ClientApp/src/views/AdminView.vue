<script setup lang="ts">
import { ref } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/stores/auth'
import type { PermissionResponse } from '@/types/api'

const authStore = useAuthStore()
const response = ref<PermissionResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

async function callAdminViewApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.get<PermissionResponse>('/api/permission/admin')
    response.value = result.data
  } catch (err: unknown) {
    handleError(err)
  } finally {
    loading.value = false
  }
}

async function callAdminManageApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.post<PermissionResponse>('/api/permission/admin', {})
    response.value = result.data
  } catch (err: unknown) {
    handleError(err)
  } finally {
    loading.value = false
  }
}

async function callUserReadApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.get<PermissionResponse>('/api/permission/user')
    response.value = result.data
  } catch (err: unknown) {
    handleError(err)
  } finally {
    loading.value = false
  }
}

async function callUserCreateApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.post<PermissionResponse>('/api/permission/user', {})
    response.value = result.data
  } catch (err: unknown) {
    handleError(err)
  } finally {
    loading.value = false
  }
}

function handleError(err: unknown) {
  if (err && typeof err === 'object' && 'response' in err) {
    const axiosError = err as { response?: { status?: number } }
    if (axiosError.response?.status === 401) {
      error.value = 'Unauthorized - you need to be logged in'
    } else if (axiosError.response?.status === 403) {
      error.value = 'Forbidden - you do not have the required permission'
    } else {
      error.value = 'Failed to call API'
    }
  } else {
    error.value = 'Failed to call API'
  }
  console.error(err)
}
</script>

<template>
  <div class="admin-view">
    <h1>Permission-Based API</h1>
    <p class="description">
      These endpoints require specific permissions (Authorize mode).
      Keycloak RPT tokens are used to validate resource permissions.
    </p>

    <div class="permissions-info">
      <h3>Your Permissions</h3>
      <ul v-if="authStore.permissions.length">
        <li v-for="perm in authStore.permissions" :key="`${perm.resource}:${perm.action}`">
          <code>{{ perm.resource }}:{{ perm.action }}</code>
        </li>
      </ul>
      <p v-else>No permissions assigned to your account.</p>
    </div>

    <div class="api-section">
      <h3>Admin Resource Endpoints</h3>
      <p class="hint">Requires <code>admin-resource:view</code> or <code>admin-resource:manage</code></p>
      <div class="actions">
        <button
          class="btn"
          @click="callAdminViewApi"
          :disabled="loading"
          :class="{ disabled: !authStore.hasPermission('admin-resource', 'view') }"
        >
          GET /api/permission/admin
        </button>
        <button
          class="btn"
          @click="callAdminManageApi"
          :disabled="loading"
          :class="{ disabled: !authStore.hasPermission('admin-resource', 'manage') }"
        >
          POST /api/permission/admin
        </button>
      </div>
    </div>

    <div class="api-section">
      <h3>User Resource Endpoints</h3>
      <p class="hint">Requires <code>user-resource:read</code> or <code>user-resource:create</code></p>
      <div class="actions">
        <button
          class="btn"
          @click="callUserReadApi"
          :disabled="loading"
          :class="{ disabled: !authStore.hasPermission('user-resource', 'read') }"
        >
          GET /api/permission/user
        </button>
        <button
          class="btn"
          @click="callUserCreateApi"
          :disabled="loading"
          :class="{ disabled: !authStore.hasPermission('user-resource', 'create') }"
        >
          POST /api/permission/user
        </button>
      </div>
    </div>

    <div v-if="loading" class="loading">Loading...</div>

    <div v-if="error" class="error">{{ error }}</div>

    <div v-if="response" class="response">
      <h3>Response</h3>
      <pre>{{ JSON.stringify(response, null, 2) }}</pre>
    </div>
  </div>
</template>

<style scoped>
.admin-view {
  max-width: 800px;
  margin: 0 auto;
}

h1 {
  color: #1976d2;
  margin-bottom: 0.5rem;
}

.description {
  color: #666;
  margin-bottom: 2rem;
}

.permissions-info {
  background: #e3f2fd;
  padding: 1rem 1.5rem;
  border-radius: 8px;
  margin-bottom: 2rem;
}

.permissions-info h3 {
  margin-bottom: 0.5rem;
  color: #1565c0;
}

.permissions-info ul {
  margin: 0;
  padding-left: 1.5rem;
}

.permissions-info code {
  background: rgba(0,0,0,0.1);
  padding: 0.1rem 0.3rem;
  border-radius: 3px;
}

.api-section {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  margin-bottom: 1.5rem;
}

.api-section h3 {
  margin-bottom: 0.25rem;
}

.hint {
  color: #666;
  font-size: 0.9rem;
  margin-bottom: 1rem;
}

.hint code {
  background: #f5f5f5;
  padding: 0.1rem 0.3rem;
  border-radius: 3px;
}

.actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
}

.btn {
  background: #1976d2;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 0.9rem;
}

.btn:hover:not(:disabled):not(.disabled) {
  background: #1565c0;
}

.btn:disabled,
.btn.disabled {
  background: #ccc;
  cursor: not-allowed;
}

.loading {
  color: #666;
  padding: 1rem;
}

.error {
  background: #ffebee;
  color: #c62828;
  padding: 1rem;
  border-radius: 4px;
  margin-top: 1rem;
}

.response {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  margin-top: 1.5rem;
}

.response h3 {
  margin-bottom: 1rem;
  color: #2e7d32;
}

.response pre {
  background: #f5f5f5;
  padding: 1rem;
  border-radius: 4px;
  overflow-x: auto;
  font-size: 0.9rem;
}
</style>
