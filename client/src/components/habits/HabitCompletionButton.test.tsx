import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { completeHabit, undoHabitCompletion } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitCompletionButton } from './HabitCompletionButton'

vi.mock('../../api/habitsApi', () => ({
  completeHabit: vi.fn(),
  undoHabitCompletion: vi.fn(),
}))

const completeHabitMock = vi.mocked(completeHabit)
const undoHabitCompletionMock = vi.mocked(undoHabitCompletion)

const existingHabit: HabitResponse = {
  id: '019c0000-0000-7000-8000-000000000001',
  name: 'Read C# textbook',
  description: 'Read one chapter.',
  category: 'learningAndSkills',
  frequencyType: 'daily',
  targetCount: 1,
  difficulty: 'medium',
  isActive: true,
  isCompletedToday: false,
  createdAtUtc: '2026-07-19T12:00:00Z',
  updatedAtUtc: '2026-07-19T12:00:00Z',
}

describe('HabitCompletionButton', () => {
  beforeEach(() => {
    completeHabitMock.mockReset()
    undoHabitCompletionMock.mockReset()
  })

  it('shows the incomplete state', () => {
    render(
      <HabitCompletionButton
        habit={existingHabit}
        onCompletionStatusChanged={vi.fn()}
      />,
    )

    const button = screen.getByRole('button', {
      name: 'Mark complete',
    })

    expect(button).toHaveAttribute('aria-pressed', 'false')
  })

  it('completes the habit and reports the new status', async () => {
    const user = userEvent.setup()
    const onCompletionStatusChanged = vi.fn()

    completeHabitMock.mockResolvedValue({
      completion: {
        id: '019c0000-0000-7000-8000-000000000002',
        habitId: existingHabit.id,
        completedDate: '2026-07-19',
        completedAtUtc: '2026-07-20T01:30:00Z',
        notes: null,
      },
    })

    render(
      <HabitCompletionButton
        habit={existingHabit}
        onCompletionStatusChanged={onCompletionStatusChanged}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Mark complete',
      }),
    )

    expect(completeHabitMock).toHaveBeenCalledTimes(1)

    expect(completeHabitMock).toHaveBeenCalledWith(existingHabit.id, {
      notes: null,
    })

    expect(undoHabitCompletionMock).not.toHaveBeenCalled()

    expect(onCompletionStatusChanged).toHaveBeenCalledWith({
      ...existingHabit,
      isCompletedToday: true,
    })
  })

  it('undoes the completion and reports the new status', async () => {
    const user = userEvent.setup()
    const onCompletionStatusChanged = vi.fn()

    const completedHabit: HabitResponse = {
      ...existingHabit,
      isCompletedToday: true,
    }

    undoHabitCompletionMock.mockResolvedValue()

    render(
      <HabitCompletionButton
        habit={completedHabit}
        onCompletionStatusChanged={onCompletionStatusChanged}
      />,
    )

    const button = screen.getByRole('button', {
      name: 'Undo completion',
    })

    expect(button).toHaveAttribute('aria-pressed', 'true')

    await user.click(button)

    expect(undoHabitCompletionMock).toHaveBeenCalledTimes(1)

    expect(undoHabitCompletionMock).toHaveBeenCalledWith(completedHabit.id)

    expect(completeHabitMock).not.toHaveBeenCalled()

    expect(onCompletionStatusChanged).toHaveBeenCalledWith({
      ...completedHabit,
      isCompletedToday: false,
    })
  })

  it('shows the backend error when completion fails', async () => {
    const user = userEvent.setup()
    const onCompletionStatusChanged = vi.fn()

    completeHabitMock.mockRejectedValue(
      new Error('This habit has already been completed for today.'),
    )

    render(
      <HabitCompletionButton
        habit={existingHabit}
        onCompletionStatusChanged={onCompletionStatusChanged}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Mark complete',
      }),
    )

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Habit completion error: This habit has already been completed for today.',
    )

    expect(onCompletionStatusChanged).not.toHaveBeenCalled()
  })
})
