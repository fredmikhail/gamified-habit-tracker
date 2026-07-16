import { createContext } from 'react'
import type { CurrentUserResponse } from '../types/CurrentUserResponse'

export type AuthContextValue = {
  currentUser: CurrentUserResponse | null
  isAuthLoading: boolean
  authErrorMessage: string | null
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
)
