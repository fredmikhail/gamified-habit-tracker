import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getDashboard } from '../../api/dashboardApi'
import { OverallProgressSection } from './OverallProgressSection'

vi.mock('../../api/dashboardApi', () => ({
  getDashboard: vi.fn(),
}))

const getDashboardMock = vi.mocked(getDashboard)

describe('OverallProgressSection', () => {
  beforeEach(() => {
    getDashboardMock.mockReset()
  })

  it('displays backend-calculated overall progress', async () => {
    getDashboardMock.mockResolvedValue({
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
    })

    render(<OverallProgressSection refreshKey={0} />)

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

  it('reloads progress when the refresh key changes', async () => {
    getDashboardMock
      .mockResolvedValueOnce({
        overallProgress: {
          totalXp: 0,
          level: 1,
          xpIntoCurrentLevel: 0,
          xpNeededForNextLevel: 200,
        },
        todayActivity: {
          localDate: '2026-07-22',
          completions: 0,
          xpEarned: 0,
        },
        todayExecution: {
          completedDailyHabits: 0,
          totalDailyHabits: 0,
        },
        habitStreaks: [],
      })
      .mockResolvedValueOnce({
        overallProgress: {
          totalXp: 20,
          level: 1,
          xpIntoCurrentLevel: 20,
          xpNeededForNextLevel: 200,
        },
        todayActivity: {
          localDate: '2026-07-22',
          completions: 0,
          xpEarned: 0,
        },
        todayExecution: {
          completedDailyHabits: 0,
          totalDailyHabits: 0,
        },
        habitStreaks: [],
      })

    const { rerender } = render(<OverallProgressSection refreshKey={0} />)

    expect(await screen.findByText('0 / 200 XP')).toBeInTheDocument()

    rerender(<OverallProgressSection refreshKey={1} />)

    expect(await screen.findByText('20 / 200 XP')).toBeInTheDocument()

    expect(getDashboardMock).toHaveBeenCalledTimes(2)
  })

  it('displays the backend error when loading fails', async () => {
    getDashboardMock.mockRejectedValue(
      new Error('Dashboard progress could not be loaded.'),
    )

    render(<OverallProgressSection refreshKey={0} />)

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Dashboard loading error: Dashboard progress could not be loaded.',
    )
  })
})
