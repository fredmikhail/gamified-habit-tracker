import type { AuthResponse } from '../types/AuthResponse'
import type { CurrentUserResponse } from '../types/CurrentUserResponse'
import type { LoginRequest } from '../types/LoginRequest'
import type { RegisterRequest } from '../types/RegisterRequest'
import { apiRequest, clearCsrfToken } from './apiClient'
import { readApiError } from './readApiError'

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
    const errorMessage = await readApiError(
      response,
      `Registration failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
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
    const errorMessage = await readApiError(
      response,
      `Login failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
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
    const errorMessage = await readApiError(
      response,
      `Logout failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  clearCsrfToken()
}

export async function getCurrentUser(): Promise<CurrentUserResponse | null> {
  const response = await apiRequest('/api/auth/me')

  if (response.status === 401) {
    return null
  }

  if (!response.ok) {
    const errorMessage = await readApiError(
      response,
      `Current-user request failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  const currentUser: CurrentUserResponse = await response.json()

  return currentUser
}
