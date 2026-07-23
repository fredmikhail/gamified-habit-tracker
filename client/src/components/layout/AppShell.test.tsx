import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { CurrentUserResponse } from '../../types/CurrentUserResponse'
import { AppShell } from './AppShell'

const { ensureDashboardLoadedMock } = vi.hoisted(() => ({
  ensureDashboardLoadedMock: vi.fn().mockResolvedValue(undefined),
}))

vi.mock('../theme/ThemeSelector', () => ({
  ThemeSelector: () => (
    <button
      aria-label="Choose appearance theme"
      className="size-11"
      type="button"
    >
      Appearance
    </button>
  ),
}))

vi.mock('../../workspace/useWorkspaceData', () => ({
  useWorkspaceData: () => ({
    dashboardResource: {
      data: {
        overallProgress: {
          totalXp: 18450,
          level: 27,
          xpIntoCurrentLevel: 100,
          xpNeededForNextLevel: 200,
        },
        todayActivity: {
          localDate: '2026-07-23',
          completions: 2,
          xpEarned: 40,
        },
        todayExecution: {
          completedDailyHabits: 2,
          totalDailyHabits: 4,
        },
        todayHabits: [],
        attributes: [],
        habitStreaks: [],
      },
      errorMessage: null,
      isRefreshing: false,
      ensureLoaded: ensureDashboardLoadedMock,
      refresh: vi.fn(),
      updateData: vi.fn(),
    },
  }),
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
  beforeEach(() => {
    ensureDashboardLoadedMock.mockClear()
  })

  it('renders the current route inside the fluid application shell', async () => {
    renderAppShell('/habits')

    expect(screen.getByTestId('app-shell')).toHaveClass('app-viewport')

    expect(
      screen.getByRole('heading', {
        name: 'Habits',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('Habit route content')).toBeInTheDocument()

    expect(
      screen.getByText('Create, complete, edit, and manage your habits.'),
    ).toBeInTheDocument()

    const habitLinks = screen.getAllByRole('link', {
      name: 'Habits',
    })

    expect(habitLinks).toHaveLength(2)

    for (const habitLink of habitLinks) {
      expect(habitLink).toHaveAttribute('aria-current', 'page')
    }

    await waitFor(() => {
      expect(ensureDashboardLoadedMock).toHaveBeenCalledTimes(1)
    })
  })

  it('shows authoritative XP, level, and progress in the global header', () => {
    renderAppShell('/dashboard')

    const progressCluster = screen.getByTestId('global-progress-cluster')

    expect(within(progressCluster).getByText('18,450')).toBeInTheDocument()

    expect(
      within(progressCluster).getByLabelText('Level 27'),
    ).toBeInTheDocument()

    const progressBar = within(progressCluster).getByRole('progressbar', {
      name: 'Overall level progress',
    })

    expect(progressBar).toHaveAttribute('aria-valuenow', '100')

    expect(progressBar).toHaveAttribute('aria-valuemax', '200')

    expect(progressBar.firstElementChild).toHaveStyle({
      width: '50%',
    })
  })

  it('shows the account identity without a surrounding account card', () => {
    renderAppShell('/attributes')

    const accountStatus = screen.getByTestId('global-account-status')

    expect(accountStatus).toHaveAccessibleName('Fred Mikhail is online')

    expect(within(accountStatus).getByText('Fred Mikhail')).toBeInTheDocument()

    expect(within(accountStatus).getByText('Online')).toBeInTheDocument()

    expect(accountStatus).not.toHaveClass('border')

    expect(accountStatus).not.toHaveClass('bg-surface-raised')
  })

  it('renders matching appearance and sign-out controls', () => {
    renderAppShell('/dashboard')

    const appearanceButton = screen.getByRole('button', {
      name: 'Choose appearance theme',
    })

    const signOutButton = screen.getByRole('button', {
      name: 'Sign out',
    })

    expect(appearanceButton).toHaveClass('size-11')

    expect(signOutButton).toHaveClass('size-11')
  })

  it('renders the global application footer on every route', () => {
    renderAppShell('/dashboard')

    expect(screen.getByText('Gamified Habit Tracker')).toBeInTheDocument()

    expect(
      screen.getByText('Stay consistent. Earn XP. Become Legendary.'),
    ).toBeInTheDocument()
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
