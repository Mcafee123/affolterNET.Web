export interface Claim {
  type: string
  value: string
}

export interface Permission {
  resource: string
  action: string
}

export interface UserContext {
  userId: string
  username: string
  email: string
  name: string
  roles: string[]
  permissions: Permission[]
}

export interface UserResponse {
  isAuthenticated: boolean
  claims: Claim[]
  userContext?: UserContext
}
