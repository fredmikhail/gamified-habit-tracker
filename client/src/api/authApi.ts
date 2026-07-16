import type { AuthResponse } from '../types/AuthResponse'
import type { CurrentUserResponse } from '../types/CurrentUserResponse'
import type { LoginRequest } from '../types/LoginRequest'
import type { RegisterRequest } from '../types/RegisterRequest'
import { apiRequest, clearCsrfToken } from './apiClient'

export async function register(
  request: RegisterRequest,
): Promise<AuthResponse> {
  const response = await apiRequest('/api/auth/register', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`Registration failed with status ${response.status}.`)
  }

  const authResponse: AuthResponse = await response.json()

  clearCsrfToken()

  return authResponse
}

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const response = await apiRequest('/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`Login failed with status ${response.status}.`)
  }

  const authResponse: AuthResponse = await response.json()

  clearCsrfToken()

  return authResponse
}

export async function logout(): Promise<void> {
  const response = await apiRequest('/api/auth/logout', {
    method: 'POST',
  })

  if (!response.ok) {
    throw new Error(`Logout failed with status ${response.status}.`)
  }

  clearCsrfToken()
}

export async function getCurrentUser(): Promise<CurrentUserResponse | null> {
  const response = await apiRequest('/api/auth/me')

  if (response.status === 401) {
    return null
  }

  if (!response.ok) {
    throw new Error(
      `Current-user request failed with status ${response.status}.`,
    )
  }

  const currentUser: CurrentUserResponse = await response.json()

  return currentUser
}
