export interface ApiResponse<T = unknown> {
  message: string
  timestamp: string
  data?: T
}

export interface PublicResponse {
  message: string
  timestamp: string
}

export interface ProtectedResponse {
  message: string
  user: string
  claims: { type: string; value: string }[]
  timestamp: string
}

export interface PermissionResponse {
  message: string
  resource: string
  action: string
  user: string
  timestamp: string
}
