import { useState } from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getDashboard } from '../../api/dashboardApi'
import type { DashboardResponse } from '../../types/DashboardResponse'
import { WorkspaceDataProvider } from '../../workspace/WorkspaceDataProvider'
import { OverallProgressSection } from './OverallProgressSection'

vi.mock('../../api/dashboardApi', () => ({
  getDashboard: vi.fn(),
}))

const getDashboardMock = vi.mocked(getDashboard)

const dashboardResponse: DashboardResponse = {
  overallProgress: {
    totalXp: 300,
    level: 2,
    xpIntoCurrentLevel: 100,
    xpNeededForNextLevel: 250,
  },
  todayActivity: {
    localDate: '2026-07-22',
    completions: 2,
    xpEarned: 30,
  },
  todayExecution: {
    completedDailyHabits: 1,
    totalDailyHabits: 2,
  },
  habitStreaks: [
    {
      habitId: '019c0000-0000-7000-8000-000000000001',
      habitName: 'Read C# textbook',
      frequencyType: 'daily',
      currentStreak: 2,
      longestStreak: 5,
    },
  ],
}

function renderOverallProgressSection() {
  return render(
    <WorkspaceDataProvider>
      <OverallProgressSection />
    </WorkspaceDataProvider>,
  )
}

function PersistentDashboardHarness() {
  const [isVisible, setIsVisible] = useState(true)

  return (
    <WorkspaceDataProvider>
      <button type="button" onClick={() => setIsVisible((current) => !current)}>
        Toggle dashboard
      </button>

      {isVisible && <OverallProgressSection />}
    </WorkspaceDataProvider>
  )
}

describe('OverallProgressSection', () => {
  beforeEach(() => {
    getDashboardMock.mockReset()
  })

  it('displays backend-calculated overall progress', async () => {
    getDashboardMock.mockResolvedValue(dashboardResponse)

    renderOverallProgressSection()

    expect(await screen.findByText('Level 2')).toBeInTheDocument()
    expect(screen.getByText('300')).toBeInTheDocument()
    expect(screen.getByText('100 / 250 XP')).toBeInTheDocument()

    const progressBar = screen.getByRole('progressbar', {
      name: 'Overall level progress',
    })

    expect(progressBar).toHaveAttribute('aria-valuenow', '100')
    expect(progressBar).toHaveAttribute('aria-valuemax', '250')

    expect(screen.getByText('2 completions')).toBeInTheDocument()
    expect(screen.getByText('30 XP earned')).toBeInTheDocument()
    expect(screen.getByText('1 of 2')).toBeInTheDocument()

    const executionProgressBar = screen.getByRole('progressbar', {
      name: 'Daily execution progress',
    })

    expect(executionProgressBar).toHaveAttribute('aria-valuenow', '1')
    expect(executionProgressBar).toHaveAttribute('aria-valuemax', '2')

    expect(
      screen.getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('2 days')).toBeInTheDocument()
    expect(screen.getByText('5 days')).toBeInTheDocument()
  })

  it('displays the backend error when initial loading fails', async () => {
    getDashboardMock.mockRejectedValue(
      new Error('Dashboard progress could not be loaded.'),
    )

    renderOverallProgressSection()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Dashboard loading error: Dashboard progress could not be loaded.',
    )
  })

  it('reuses cached dashboard data when the section returns', async () => {
    const user = userEvent.setup()

    getDashboardMock.mockResolvedValue(dashboardResponse)

    render(<PersistentDashboardHarness />)

    expect(await screen.findByText('Level 2')).toBeInTheDocument()
    expect(getDashboardMock).toHaveBeenCalledTimes(1)

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle dashboard',
      }),
    )

    expect(screen.queryByText('Level 2')).not.toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle dashboard',
      }),
    )

    expect(screen.getByText('Level 2')).toBeInTheDocument()

    expect(
      screen.queryByText('Loading overall progress...'),
    ).not.toBeInTheDocument()

    expect(getDashboardMock).toHaveBeenCalledTimes(1)
  })
})
