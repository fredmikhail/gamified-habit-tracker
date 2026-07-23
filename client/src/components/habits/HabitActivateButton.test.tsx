import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { activateHabit } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitActivateButton } from './HabitActivateButton'

vi.mock('../../api/habitsApi', () => ({
  activateHabit: vi.fn(),
}))

const activateHabitMock = vi.mocked(activateHabit)

const inactiveHabit: HabitResponse = {
  id: '019c0000-0000-7000-8000-000000000001',
  name: 'Read C# textbook',
  description: null,
  category: 'learningAndSkills',
  frequencyType: 'daily',
  targetCount: 1,
  difficulty: 'medium',
  isActive: false,
  isCompletedToday: false,
  createdAtUtc: '2026-07-19T12:00:00Z',
  updatedAtUtc: '2026-07-20T12:00:00Z',
}

describe('HabitActivateButton', () => {
  beforeEach(() => {
    activateHabitMock.mockReset()
  })

  it('activates the habit and reports the updated response', async () => {
    const user = userEvent.setup()
    const onHabitActivated = vi.fn()

    const activatedHabit = {
      ...inactiveHabit,
      isActive: true,
      updatedAtUtc: '2026-07-23T14:00:00Z',
    }

    activateHabitMock.mockResolvedValue(activatedHabit)

    render(
      <HabitActivateButton
        habit={inactiveHabit}
        onHabitActivated={onHabitActivated}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Reactivate habit',
      }),
    )

    expect(activateHabitMock).toHaveBeenCalledWith(inactiveHabit.id)
    expect(onHabitActivated).toHaveBeenCalledWith(activatedHabit)
  })

  it('shows the API error when activation fails', async () => {
    const user = userEvent.setup()

    activateHabitMock.mockRejectedValue(
      new Error('The habit could not be activated.'),
    )

    render(
      <HabitActivateButton habit={inactiveHabit} onHabitActivated={vi.fn()} />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Reactivate habit',
      }),
    )

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Habit activation error: The habit could not be activated.',
    )
  })
})
