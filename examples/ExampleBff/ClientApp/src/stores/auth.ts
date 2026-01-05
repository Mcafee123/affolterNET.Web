import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/authService'
import type { UserResponse, Permission } from '@/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserResponse | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => user.value?.isAuthenticated ?? false)
  const userContext = computed(() => user.value?.userContext)
  const permissions = computed(() => user.value?.userContext?.permissions ?? [])
  const roles = computed(() => user.value?.userContext?.roles ?? [])
  const username = computed(() => user.value?.userContext?.username ?? '')
  const email = computed(() => user.value?.userContext?.email ?? '')

  async function fetchUser(): Promise<void> {
    loading.value = true
    error.value = null
    try {
      user.value = await authService.getCurrentUser()
    } catch (err) {
      error.value = 'Failed to fetch user information'
      user.value = { isAuthenticated: false, claims: [] }
    } finally {
      loading.value = false
    }
  }

  function hasPermission(resource: string, action?: string): boolean {
    if (!permissions.value.length) return false
    return permissions.value.some((p: Permission) =>
      p.resource === resource && (!action || p.action === action)
    )
  }

  function hasRole(role: string): boolean {
    return roles.value.includes(role)
  }

  function login(returnUrl?: string): void {
    authService.login(returnUrl)
  }

  function logout(): void {
    authService.logout()
  }

  return {
    user,
    loading,
    error,
    isAuthenticated,
    userContext,
    permissions,
    roles,
    username,
    email,
    fetchUser,
    hasPermission,
    hasRole,
    login,
    logout
  }
})
