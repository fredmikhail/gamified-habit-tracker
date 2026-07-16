import { useEffect, useState, type PropsWithChildren } from 'react'
import {
  getCurrentUser,
  login as loginApi,
  logout as logoutApi,
  register as registerApi,
} from '../api/authApi'
import type { CurrentUserResponse } from '../types/CurrentUserResponse'
import type { LoginRequest } from '../types/LoginRequest'
import type { RegisterRequest } from '../types/RegisterRequest'
import { AuthContext } from './AuthContext'

function getAuthErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown authentication error occurred.'
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [currentUser, setCurrentUser] = useState<CurrentUserResponse | null>(
    null,
  )

  const [isAuthLoading, setIsAuthLoading] = useState(true)

  const [isAuthActionLoading, setIsAuthActionLoading] = useState(false)

  const [authErrorMessage, setAuthErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    let isActive = true

    async function loadCurrentUser() {
      try {
        const user = await getCurrentUser()

        if (isActive) {
          setCurrentUser(user)
        }
      } catch (error) {
        if (isActive) {
          setAuthErrorMessage(getAuthErrorMessage(error))
        }
      } finally {
        if (isActive) {
          setIsAuthLoading(false)
        }
      }
    }

    void loadCurrentUser()

    return () => {
      isActive = false
    }
  }, [])

  function clearAuthError(): void {
    setAuthErrorMessage(null)
  }

  async function registerUser(request: RegisterRequest): Promise<void> {
    setIsAuthActionLoading(true)
    setAuthErrorMessage(null)

    try {
      const authResponse = await registerApi(request)

      setCurrentUser(authResponse.user)
    } catch (error) {
      setAuthErrorMessage(getAuthErrorMessage(error))

      throw error
    } finally {
      setIsAuthActionLoading(false)
    }
  }

  async function loginUser(request: LoginRequest): Promise<void> {
    setIsAuthActionLoading(true)
    setAuthErrorMessage(null)

    try {
      const authResponse = await loginApi(request)

      setCurrentUser(authResponse.user)
    } catch (error) {
      setAuthErrorMessage(getAuthErrorMessage(error))

      throw error
    } finally {
      setIsAuthActionLoading(false)
    }
  }

  async function logoutUser(): Promise<void> {
    setIsAuthActionLoading(true)
    setAuthErrorMessage(null)

    try {
      await logoutApi()

      setCurrentUser(null)
    } catch (error) {
      setAuthErrorMessage(getAuthErrorMessage(error))

      throw error
    } finally {
      setIsAuthActionLoading(false)
    }
  }

  return (
    <AuthContext.Provider
      value={{
        currentUser,
        isAuthLoading,
        isAuthActionLoading,
        authErrorMessage,
        clearAuthError,
        registerUser,
        loginUser,
        logoutUser,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}
