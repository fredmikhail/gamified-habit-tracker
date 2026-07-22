import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { HabitStreakSection } from './HabitStreakSection'

describe('HabitStreakSection', () => {
  it('displays daily and weekly streaks using the correct units', () => {
    render(
      <HabitStreakSection
        habitStreaks={[
          {
            habitId: 'daily-habit',
            habitName: 'Read C# textbook',
            frequencyType: 'daily',
            currentStreak: 2,
            longestStreak: 5,
          },
          {
            habitId: 'weekly-habit',
            habitName: 'Exercise',
            frequencyType: 'weekly',
            currentStreak: 1,
            longestStreak: 3,
          },
        ]}
      />,
    )

    expect(
      screen.getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'Exercise',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('2 days')).toBeInTheDocument()
    expect(screen.getByText('5 days')).toBeInTheDocument()
    expect(screen.getByText('1 week')).toBeInTheDocument()
    expect(screen.getByText('3 weeks')).toBeInTheDocument()
  })

  it('displays an empty state when there are no active habits', () => {
    render(<HabitStreakSection habitStreaks={[]} />)

    expect(screen.getByText('No active habit streaks yet.')).toBeInTheDocument()
  })
})
