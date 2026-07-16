import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../../auth/AuthContext'
import { LoginForm } from './LoginForm'

describe('LoginForm', () => {
  it('submits the entered login information', async () => {
    const user = userEvent.setup()

    const loginUser = vi.fn<AuthContextValue['loginUser']>()

    loginUser.mockResolvedValue(undefined)

    const authContextValue: AuthContextValue = {
      currentUser: null,
      isAuthLoading: false,
      isAuthActionLoading: false,
      authErrorMessage: null,
      clearAuthError: vi.fn(),
      registerUser: vi.fn<AuthContextValue['registerUser']>(),
      loginUser,
      logoutUser: vi.fn<AuthContextValue['logoutUser']>(),
    }

    render(
      <AuthContext.Provider value={authContextValue}>
        <LoginForm />
      </AuthContext.Provider>,
    )

    await user.type(
      screen.getByRole('textbox', { name: 'Email' }),
      'fred@example.com',
    )

    await user.type(screen.getByLabelText('Password'), 'ManualTestPassword123!')

    await user.click(
      screen.getByRole('checkbox', {
        name: 'Keep me signed in',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Sign in',
      }),
    )

    expect(loginUser).toHaveBeenCalledTimes(1)

    expect(loginUser).toHaveBeenCalledWith({
      email: 'fred@example.com',
      password: 'ManualTestPassword123!',
      rememberMe: true,
    })
  })
})
