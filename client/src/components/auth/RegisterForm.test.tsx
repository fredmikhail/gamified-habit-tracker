import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../../auth/AuthContext'
import { RegisterForm } from './RegisterForm'

describe('RegisterForm', () => {
  it('submits the entered registration information', async () => {
    const user = userEvent.setup()

    const registerUser = vi.fn<AuthContextValue['registerUser']>()

    registerUser.mockResolvedValue(undefined)

    const authContextValue: AuthContextValue = {
      currentUser: null,
      isAuthLoading: false,
      isAuthActionLoading: false,
      authErrorMessage: null,
      clearAuthError: vi.fn(),
      registerUser,
      loginUser: vi.fn<AuthContextValue['loginUser']>(),
      logoutUser: vi.fn<AuthContextValue['logoutUser']>(),
    }

    render(
      <AuthContext.Provider value={authContextValue}>
        <RegisterForm />
      </AuthContext.Provider>,
    )

    await user.type(
      screen.getByRole('textbox', { name: 'Email' }),
      'newuser@example.com',
    )

    await user.type(
      screen.getByRole('textbox', { name: 'Username' }),
      'new_user',
    )

    await user.type(screen.getByLabelText('Password'), 'RegistrationTest123!')

    const timeZoneInput = screen.getByRole('textbox', {
      name: 'Time zone',
    })

    await user.clear(timeZoneInput)
    await user.type(timeZoneInput, 'America/Toronto')

    await user.click(
      screen.getByRole('button', {
        name: 'Create account',
      }),
    )

    expect(registerUser).toHaveBeenCalledTimes(1)

    expect(registerUser).toHaveBeenCalledWith({
      email: 'newuser@example.com',
      username: 'new_user',
      password: 'RegistrationTest123!',
      timeZone: 'America/Toronto',
    })
  })
})
