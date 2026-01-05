import api from './api'
import type { UserResponse } from '@/types/auth'

export const authService = {
  async getCurrentUser(): Promise<UserResponse> {
    const response = await api.get<UserResponse>('/bff/user')
    return response.data
  },

  login(returnUrl?: string): void {
    const url = returnUrl
      ? `/bff/account/login?returnUrl=${encodeURIComponent(returnUrl)}`
      : '/bff/account/login'
    window.location.href = url
  },

  logout(): void {
    window.location.href = '/bff/account/logout'
  }
}
