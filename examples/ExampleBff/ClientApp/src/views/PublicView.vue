<script setup lang="ts">
import { ref } from 'vue'
import api from '@/services/api'
import type { PublicResponse } from '@/types/api'

const response = ref<PublicResponse | null>(null)
const loading = ref(false)
const error = ref<string | null>(null)

async function callPublicApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.get<PublicResponse>('/api/public')
    response.value = result.data
  } catch (err) {
    error.value = 'Failed to call public API'
    console.error(err)
  } finally {
    loading.value = false
  }
}

async function callHealthApi() {
  loading.value = true
  error.value = null
  try {
    const result = await api.get<PublicResponse>('/api/public/health')
    response.value = result.data
  } catch (err) {
    error.value = 'Failed to call health API'
    console.error(err)
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="public-view">
    <h1>Public API</h1>
    <p class="description">
      These endpoints are accessible without authentication.
      The requests are proxied through the BFF via YARP to the API.
    </p>

    <div class="actions">
      <button class="btn" @click="callPublicApi" :disabled="loading">
        Call /api/public
      </button>
      <button class="btn" @click="callHealthApi" :disabled="loading">
        Call /api/public/health
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
.public-view {
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
