import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { deactivateHabit } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitDeactivateButton } from './HabitDeactivateButton'

vi.mock('../../api/habitsApi', () => ({
  deactivateHabit: vi.fn(),
}))

const deactivateHabitMock = vi.mocked(deactivateHabit)

const existingHabit: HabitResponse = {
  id: '019c0000-0000-7000-8000-000000000001',
  name: 'Read C# textbook',
  description: 'Read one chapter.',
  category: 'Learning',
  frequencyType: 'daily',
  targetCount: 1,
  difficulty: 'medium',
  isActive: true,
  isCompletedToday: false,
  createdAtUtc: '2026-07-19T12:00:00Z',
  updatedAtUtc: '2026-07-19T12:00:00Z',
}

describe('HabitDeactivateButton', () => {
  beforeEach(() => {
    deactivateHabitMock.mockReset()
  })

  it('asks for confirmation before deactivating', async () => {
    const user = userEvent.setup()

    render(
      <HabitDeactivateButton
        habit={existingHabit}
        onHabitDeactivated={vi.fn()}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Deactivate',
      }),
    )

    expect(
      screen.getByText('Deactivate "Read C# textbook"?'),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('button', {
        name: 'Confirm deactivation',
      }),
    ).toBeInTheDocument()

    expect(deactivateHabitMock).not.toHaveBeenCalled()
  })

  it('cancels without deactivating the habit', async () => {
    const user = userEvent.setup()

    render(
      <HabitDeactivateButton
        habit={existingHabit}
        onHabitDeactivated={vi.fn()}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Deactivate',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Keep habit',
      }),
    )

    expect(
      screen.queryByRole('button', {
        name: 'Confirm deactivation',
      }),
    ).not.toBeInTheDocument()

    expect(
      screen.getByRole('button', {
        name: 'Deactivate',
      }),
    ).toBeInTheDocument()

    expect(deactivateHabitMock).not.toHaveBeenCalled()
  })

  it('deactivates the habit and reports the backend response', async () => {
    const user = userEvent.setup()
    const onHabitDeactivated = vi.fn()

    const deactivatedHabit: HabitResponse = {
      ...existingHabit,
      isActive: false,
      isCompletedToday: false,
      updatedAtUtc: '2026-07-20T12:00:00Z',
    }

    deactivateHabitMock.mockResolvedValue(deactivatedHabit)

    render(
      <HabitDeactivateButton
        habit={existingHabit}
        onHabitDeactivated={onHabitDeactivated}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Deactivate',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Confirm deactivation',
      }),
    )

    expect(deactivateHabitMock).toHaveBeenCalledTimes(1)
    expect(deactivateHabitMock).toHaveBeenCalledWith(existingHabit.id)

    expect(onHabitDeactivated).toHaveBeenCalledTimes(1)
    expect(onHabitDeactivated).toHaveBeenCalledWith(deactivatedHabit)
  })

  it('shows the pending state while deactivation is running', async () => {
    const user = userEvent.setup()

    deactivateHabitMock.mockImplementation(
      () => new Promise<HabitResponse>(() => undefined),
    )

    render(
      <HabitDeactivateButton
        habit={existingHabit}
        onHabitDeactivated={vi.fn()}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Deactivate',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Confirm deactivation',
      }),
    )

    expect(
      screen.getByRole('button', {
        name: 'Deactivating...',
      }),
    ).toBeDisabled()

    expect(
      screen.getByRole('button', {
        name: 'Keep habit',
      }),
    ).toBeDisabled()
  })

  it('shows the backend error when deactivation fails', async () => {
    const user = userEvent.setup()
    const onHabitDeactivated = vi.fn()

    deactivateHabitMock.mockRejectedValue(
      new Error('The habit could not be found.'),
    )

    render(
      <HabitDeactivateButton
        habit={existingHabit}
        onHabitDeactivated={onHabitDeactivated}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Deactivate',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Confirm deactivation',
      }),
    )

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Habit deactivation error: The habit could not be found.',
    )

    expect(onHabitDeactivated).not.toHaveBeenCalled()
  })
})
