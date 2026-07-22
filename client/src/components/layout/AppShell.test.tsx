import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { describe, expect, it, vi } from 'vitest'
import type { CurrentUserResponse } from '../../types/CurrentUserResponse'
import { AppShell } from './AppShell'

vi.mock('../theme/ThemeSelector', () => ({
  ThemeSelector: () => <button type="button">Appearance</button>,
}))

const currentUser: CurrentUserResponse = {
  id: '019c0000-0000-7000-8000-000000000001',
  email: 'fred@example.com',
  username: 'fred',
  displayName: 'Fred Mikhail',
  timeZone: 'America/Toronto',
}

function renderAppShell(initialPath: string, onLogout = vi.fn()) {
  render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route
          element={
            <AppShell
              currentUser={currentUser}
              isLogoutPending={false}
              onLogout={onLogout}
            />
          }
        >
          <Route path="/dashboard" element={<p>Dashboard route content</p>} />

          <Route path="/habits" element={<p>Habit route content</p>} />

          <Route path="/attributes" element={<p>Attribute route content</p>} />
        </Route>
      </Routes>
    </MemoryRouter>,
  )
}

describe('AppShell', () => {
  it('renders the current route inside a viewport-sized shell', () => {
    renderAppShell('/habits')

    expect(screen.getByTestId('app-shell')).toHaveClass('app-viewport')

    expect(
      screen.getByRole('heading', {
        name: 'Habits',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('Habit route content')).toBeInTheDocument()

    expect(
      screen.getByText(
        'Create, complete, edit, and manage your active habits.',
      ),
    ).toBeInTheDocument()

    const habitLinks = screen.getAllByRole('link', {
      name: 'Habits',
    })

    expect(habitLinks).toHaveLength(2)

    for (const habitLink of habitLinks) {
      expect(habitLink).toHaveAttribute('aria-current', 'page')
    }
  })

  it('uses a fixed mobile navigation grid instead of horizontal scrolling', () => {
    renderAppShell('/dashboard')

    const mobileNavigation = screen.getByRole('navigation', {
      name: 'Mobile navigation',
    })

    expect(mobileNavigation).toHaveClass('grid', 'grid-cols-3')

    expect(mobileNavigation).not.toHaveClass('overflow-x-auto')
  })

  it('calls the logout action from the shell', async () => {
    const user = userEvent.setup()
    const onLogout = vi.fn()

    renderAppShell('/attributes', onLogout)

    await user.click(
      screen.getByRole('button', {
        name: 'Sign out',
      }),
    )

    expect(onLogout).toHaveBeenCalledTimes(1)
  })
})
