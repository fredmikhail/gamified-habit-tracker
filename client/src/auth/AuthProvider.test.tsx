import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import * as authApi from '../api/authApi'
import type { CurrentUserResponse } from '../types/CurrentUserResponse'
import type { LoginRequest } from '../types/LoginRequest'
import { AuthProvider } from './AuthProvider'
import { useAuth } from './useAuth'

vi.mock('../api/authApi', () => ({
  getCurrentUser: vi.fn(),
  login: vi.fn(),
  logout: vi.fn(),
  register: vi.fn(),
}))

const mockedGetCurrentUser = vi.mocked(authApi.getCurrentUser)
const mockedLogin = vi.mocked(authApi.login)
const mockedLogout = vi.mocked(authApi.logout)

const testUser: CurrentUserResponse = {
  id: '01912345-6789-7000-8000-000000000001',
  email: 'fred@example.com',
  username: 'fred_test',
  displayName: 'fred_test',
  timeZone: 'America/Toronto',
}

const loginRequest: LoginRequest = {
  email: 'fred@example.com',
  password: 'ManualTestPassword123!',
  rememberMe: true,
}

function AuthTestConsumer() {
  const {
    currentUser,
    isAuthLoading,
    authErrorMessage,
    loginUser,
    logoutUser,
  } = useAuth()

  return (
    <>
      {isAuthLoading && <p>Loading authentication</p>}

      {!isAuthLoading && currentUser && <p>Signed in: {currentUser.email}</p>}

      {!isAuthLoading && !currentUser && <p>Signed out</p>}

      {authErrorMessage && <p>Authentication error: {authErrorMessage}</p>}

      <button type="button" onClick={() => void loginUser(loginRequest)}>
        Log in test user
      </button>

      <button type="button" onClick={() => void logoutUser()}>
        Log out test user
      </button>
    </>
  )
}

function renderAuthProvider() {
  render(
    <AuthProvider>
      <AuthTestConsumer />
    </AuthProvider>,
  )
}

describe('AuthProvider', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  it('loads the current user when authentication initializes', async () => {
    mockedGetCurrentUser.mockResolvedValue(testUser)

    renderAuthProvider()

    expect(
      await screen.findByText(`Signed in: ${testUser.email}`),
    ).toBeInTheDocument()

    expect(mockedGetCurrentUser).toHaveBeenCalledTimes(1)
  })

  it('updates the current user after login and logout', async () => {
    const user = userEvent.setup()

    mockedGetCurrentUser.mockResolvedValue(null)
    mockedLogin.mockResolvedValue({
      user: testUser,
    })
    mockedLogout.mockResolvedValue(undefined)

    renderAuthProvider()

    expect(await screen.findByText('Signed out')).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Log in test user',
      }),
    )

    expect(
      await screen.findByText(`Signed in: ${testUser.email}`),
    ).toBeInTheDocument()

    expect(mockedLogin).toHaveBeenCalledTimes(1)
    expect(mockedLogin).toHaveBeenCalledWith(loginRequest)

    await user.click(
      screen.getByRole('button', {
        name: 'Log out test user',
      }),
    )

    expect(await screen.findByText('Signed out')).toBeInTheDocument()
    expect(mockedLogout).toHaveBeenCalledTimes(1)
  })
})
