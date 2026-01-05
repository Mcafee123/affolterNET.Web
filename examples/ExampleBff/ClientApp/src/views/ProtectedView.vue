<script setup lang="ts">
import { ref } from 'vue'
import api from '@/services/api'
import type { ProtectedResponse } from '@/types/api'

const response = ref<ProtectedResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

async function callProtectedApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.get<ProtectedResponse>('/api/protected')
    response.value = result.data
  } catch (err: unknown) {
    if (err && typeof err === 'object' && 'response' in err) {
      const axiosError = err as { response?: { status?: number } }
      if (axiosError.response?.status === 401) {
        error.value = 'Unauthorized - you need to be logged in'
      } else if (axiosError.response?.status === 403) {
        error.value = 'Forbidden - you do not have permission'
      } else {
        error.value = 'Failed to call protected API'
      }
    } else {
      error.value = 'Failed to call protected API'
    }
    console.error(err)
  } finally {
    loading.value = false
  }
}

async function callProfileApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.get<ProtectedResponse>('/api/protected/profile')
    response.value = result.data
  } catch (err: unknown) {
    if (err && typeof err === 'object' && 'response' in err) {
      const axiosError = err as { response?: { status?: number } }
      if (axiosError.response?.status === 401) {
        error.value = 'Unauthorized - you need to be logged in'
      } else {
        error.value = 'Failed to call profile API'
      }
    } else {
      error.value = 'Failed to call profile API'
    }
    console.error(err)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="protected-view">
    <h1>Protected API</h1>
    <p class="description">
      These endpoints require authentication (Authenticate mode).
      The BFF forwards your access token to the API via YARP.
    </p>

    <div class="actions">
      <button class="btn" @click="callProtectedApi" :disabled="loading">
        Call /api/protected
      </button>
      <button class="btn" @click="callProfileApi" :disabled="loading">
        Call /api/protected/profile
      </button>
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
.protected-view {
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

.actions {
  display: flex;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.btn {
  background: #1976d2;
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: 4px;
  cursor: pointer;
  font-size: 1rem;
}

.btn:hover:not(:disabled) {
  background: #1565c0;
}

.btn:disabled {
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
  margin-bottom: 1rem;
}

.response {
  background: white;
  padding: 1.5rem;
  border-radius: 8px;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
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
