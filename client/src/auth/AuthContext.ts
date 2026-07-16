import { createContext } from 'react'
import type { CurrentUserResponse } from '../types/CurrentUserResponse'
import type { LoginRequest } from '../types/LoginRequest'
import type { RegisterRequest } from '../types/RegisterRequest'

export type AuthContextValue = {
  currentUser: CurrentUserResponse | null
  isAuthLoading: boolean
  isAuthActionLoading: boolean
  authErrorMessage: string | null
  registerUser: (request: RegisterRequest) => Promise<void>
  loginUser: (request: LoginRequest) => Promise<void>
  logoutUser: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
)
