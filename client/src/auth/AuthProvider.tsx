import { useEffect, useState, type PropsWithChildren } from 'react'
import { getCurrentUser } from '../api/authApi'
import type { CurrentUserResponse } from '../types/CurrentUserResponse'
import { AuthContext } from './AuthContext'

export function AuthProvider({ children }: PropsWithChildren) {
  const [currentUser, setCurrentUser] = useState<CurrentUserResponse | null>(
    null,
  )

  const [isAuthLoading, setIsAuthLoading] = useState(true)

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
          setAuthErrorMessage(
            error instanceof Error
              ? error.message
              : 'An unknown authentication error occurred.',
          )
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

  return (
    <AuthContext.Provider
      value={{
        currentUser,
        isAuthLoading,
        authErrorMessage,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}
