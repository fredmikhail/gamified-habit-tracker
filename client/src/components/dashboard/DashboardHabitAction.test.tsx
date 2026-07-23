import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { completeHabit, undoHabitCompletion } from '../../api/habitsApi'
import type { DashboardHabitResponse } from '../../types/DashboardHabitResponse'
import { DashboardHabitAction } from './DashboardHabitAction'

vi.mock('../../api/habitsApi', () => ({
  completeHabit: vi.fn(),
  undoHabitCompletion: vi.fn(),
}))

const completeHabitMock = vi.mocked(completeHabit)

const undoHabitCompletionMock = vi.mocked(undoHabitCompletion)

const habit: DashboardHabitResponse = {
  id: 'habit-1',
  name: 'Read C# textbook',
  category: 'learningAndSkills',
  frequencyType: 'daily',
  targetCount: 1,
  difficulty: 'medium',
  attributeRewards: [
    {
      attributeType: 'mind',
      xpAmount: 14,
    },
    {
      attributeType: 'focus',
      xpAmount: 6,
    },
  ],
  isCompletedToday: false,
  currentStreak: 2,
  longestStreak: 5,
}

describe('DashboardHabitAction', () => {
  beforeEach(() => {
    completeHabitMock.mockReset()
    undoHabitCompletionMock.mockReset()
  })

  it('completes a habit and reports earned rewards', async () => {
    const user = userEvent.setup()
    const onCompletionStatusChanged = vi.fn()

    completeHabitMock.mockResolvedValue({
      completion: {
        id: 'completion-1',
        habitId: habit.id,
        completedDate: '2026-07-22',
        completedAtUtc: '2026-07-22T14:00:00Z',
        notes: null,
      },
      rewards: habit.attributeRewards,
    })

    render(
      <DashboardHabitAction
        habit={habit}
        onCompletionStatusChanged={onCompletionStatusChanged}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Complete Read C# textbook',
      }),
    )

    expect(completeHabitMock).toHaveBeenCalledWith(habit.id, {
      notes: null,
    })

    expect(onCompletionStatusChanged).toHaveBeenCalledWith(habit.id, true)

    expect(screen.getByRole('status')).toHaveTextContent('+14 Mind · +6 Focus')
  })

  it('undoes a completed habit', async () => {
    const user = userEvent.setup()
    const onCompletionStatusChanged = vi.fn()

    undoHabitCompletionMock.mockResolvedValue()

    render(
      <DashboardHabitAction
        habit={{
          ...habit,
          isCompletedToday: true,
        }}
        onCompletionStatusChanged={onCompletionStatusChanged}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Undo completion for Read C# textbook',
      }),
    )

    expect(undoHabitCompletionMock).toHaveBeenCalledWith(habit.id)

    expect(onCompletionStatusChanged).toHaveBeenCalledWith(habit.id, false)
  })

  it('shows the backend error', async () => {
    const user = userEvent.setup()

    completeHabitMock.mockRejectedValue(
      new Error('The habit is already completed.'),
    )

    render(
      <DashboardHabitAction
        habit={habit}
        onCompletionStatusChanged={vi.fn()}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Complete Read C# textbook',
      }),
    )

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'The habit is already completed.',
    )
  })
})
