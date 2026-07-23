import { render, screen, waitFor, within } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { DashboardResponse } from '../../types/DashboardResponse'
import { DashboardSummaryRail } from './DashboardSummaryRail'

const dashboard: DashboardResponse = {
  overallProgress: {
    totalXp: 300,
    level: 2,
    xpIntoCurrentLevel: 100,
    xpNeededForNextLevel: 250,
  },
  todayActivity: {
    localDate: '2026-07-22',
    completions: 1,
    xpEarned: 20,
  },
  todayExecution: {
    completedDailyHabits: 1,
    totalDailyHabits: 3,
  },
  todayHabits: [],
  attributes: [],
  habitStreaks: [
    {
      habitId: 'habit-1',
      habitName: 'Read C# textbook',
      frequencyType: 'daily',
      currentStreak: 2,
      longestStreak: 5,
    },
  ],
}

describe('DashboardSummaryRail', () => {
  it('renders the five authoritative progression cards', () => {
    render(<DashboardSummaryRail dashboard={dashboard} />)

    for (const label of [
      "Today's summary",
      'Current streak',
      'XP today',
      'Total level',
      'Next level',
    ]) {
      expect(
        screen.getByRole('article', {
          name: label,
        }),
      ).toBeInTheDocument()
    }
  })

  it('shows useful streak and next-level context from dashboard data', () => {
    render(<DashboardSummaryRail dashboard={dashboard} />)

    const streakCard = screen.getByRole('article', {
      name: 'Current streak',
    })

    expect(within(streakCard).getByText('2')).toBeInTheDocument()
    expect(within(streakCard).getByText('days')).toBeInTheDocument()
    expect(within(streakCard).getByText('Best 5')).toBeInTheDocument()
    expect(within(streakCard).getByText('Read C# textbook')).toBeInTheDocument()

    const nextLevelCard = screen.getByRole('article', {
      name: 'Next level',
    })

    expect(within(nextLevelCard).getByText('Level 2 → 3')).toBeInTheDocument()
    expect(within(nextLevelCard).getByText('150')).toBeInTheDocument()
    expect(within(nextLevelCard).getByText('XP remaining')).toBeInTheDocument()

    expect(
      within(nextLevelCard).getByRole('progressbar', {
        name: 'Overall level progress',
      }),
    ).toHaveAttribute('aria-valuenow', '100')
  })

  it('does not animate progression during the initial dashboard load', () => {
    render(<DashboardSummaryRail dashboard={dashboard} />)

    for (const label of [
      "Today's summary",
      'Current streak',
      'XP today',
      'Total level',
      'Next level',
    ]) {
      expect(
        screen.getByRole('article', {
          name: label,
        }),
      ).toHaveAttribute('data-feedback', 'none')
    }

    expect(screen.queryByRole('status')).not.toBeInTheDocument()
  })

  it('pulses XP and smoothly advances progress after refreshed totals arrive', async () => {
    const { rerender } = render(<DashboardSummaryRail dashboard={dashboard} />)

    rerender(
      <DashboardSummaryRail
        dashboard={{
          ...dashboard,
          overallProgress: {
            ...dashboard.overallProgress,
            totalXp: 350,
            xpIntoCurrentLevel: 150,
          },
          todayActivity: {
            ...dashboard.todayActivity,
            completions: 2,
            xpEarned: 70,
          },
        }}
      />,
    )

    const xpCard = screen.getByRole('article', {
      name: 'XP today',
    })

    await waitFor(() => {
      expect(xpCard).toHaveAttribute('data-feedback', 'xpGain')
    })

    expect(screen.getByTestId('overall-level-progress-fill')).toHaveStyle({
      width: '60%',
    })

    expect(
      screen.getByTestId('overall-level-progress-fill'),
    ).not.toHaveAttribute('data-level-up')
  })

  it('celebrates only when the refreshed backend level is higher', async () => {
    const { rerender } = render(<DashboardSummaryRail dashboard={dashboard} />)

    rerender(
      <DashboardSummaryRail
        dashboard={{
          ...dashboard,
          overallProgress: {
            totalXp: 520,
            level: 3,
            xpIntoCurrentLevel: 20,
            xpNeededForNextLevel: 300,
          },
        }}
      />,
    )

    const totalLevelCard = screen.getByRole('article', {
      name: 'Total level',
    })

    const nextLevelCard = screen.getByRole('article', {
      name: 'Next level',
    })

    await waitFor(() => {
      expect(totalLevelCard).toHaveAttribute('data-feedback', 'levelUp')
      expect(nextLevelCard).toHaveAttribute('data-feedback', 'levelUp')
    })

    expect(within(totalLevelCard).getByRole('status')).toHaveTextContent(
      'Level up',
    )

    expect(within(totalLevelCard).getByText('3')).toHaveAttribute(
      'data-level-up',
      'true',
    )

    expect(screen.getByTestId('overall-level-progress-fill')).toHaveAttribute(
      'data-level-up',
      'true',
    )
  })
})
