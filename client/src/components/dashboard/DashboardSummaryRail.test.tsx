import { render, screen, within } from '@testing-library/react'
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
})
