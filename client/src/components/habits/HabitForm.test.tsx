import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createHabit } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitForm } from './HabitForm'

vi.mock('../../api/habitsApi', () => ({
  createHabit: vi.fn(),
}))

const createHabitMock = vi.mocked(createHabit)

describe('HabitForm', () => {
  beforeEach(() => {
    createHabitMock.mockReset()
  })

  it('submits the entered weekly habit and reports the created habit', async () => {
    const user = userEvent.setup()
    const onHabitCreated = vi.fn()

    const createdHabit: HabitResponse = {
      id: '019c0000-0000-7000-8000-000000000001',
      name: 'Go to gym',
      description: 'Complete a planned workout.',
      category: 'Fitness',
      frequencyType: 'weekly',
      targetCount: 3,
      difficulty: 'elite',
      isActive: true,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-19T12:00:00Z',
    }

    createHabitMock.mockResolvedValue(createdHabit)

    render(<HabitForm onHabitCreated={onHabitCreated} />)

    await user.type(screen.getByRole('textbox', { name: 'Name' }), 'Go to gym')

    await user.type(
      screen.getByRole('textbox', { name: 'Description' }),
      'Complete a planned workout.',
    )

    await user.type(
      screen.getByRole('textbox', { name: 'Category' }),
      'Fitness',
    )

    await user.selectOptions(
      screen.getByRole('combobox', { name: 'Frequency' }),
      'weekly',
    )

    const targetCountInput = screen.getByRole('spinbutton', {
      name: 'Times per week',
    })

    await user.clear(targetCountInput)
    await user.type(targetCountInput, '3')

    await user.selectOptions(
      screen.getByRole('combobox', { name: 'Difficulty' }),
      'elite',
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Create habit',
      }),
    )

    expect(createHabitMock).toHaveBeenCalledTimes(1)

    expect(createHabitMock).toHaveBeenCalledWith({
      name: 'Go to gym',
      description: 'Complete a planned workout.',
      category: 'Fitness',
      frequencyType: 'weekly',
      targetCount: 3,
      difficulty: 'elite',
    })

    expect(onHabitCreated).toHaveBeenCalledTimes(1)
    expect(onHabitCreated).toHaveBeenCalledWith(createdHabit)
  })

  it('submits blank optional fields as null', async () => {
    const user = userEvent.setup()
    const onHabitCreated = vi.fn()

    const createdHabit: HabitResponse = {
      id: '019c0000-0000-7000-8000-000000000002',
      name: 'Read',
      description: null,
      category: null,
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      isActive: true,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-19T12:00:00Z',
    }

    createHabitMock.mockResolvedValue(createdHabit)

    render(<HabitForm onHabitCreated={onHabitCreated} />)

    await user.type(screen.getByRole('textbox', { name: 'Name' }), 'Read')

    await user.click(
      screen.getByRole('button', {
        name: 'Create habit',
      }),
    )

    expect(createHabitMock).toHaveBeenCalledWith({
      name: 'Read',
      description: null,
      category: null,
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
    })
  })

  it('shows the submitting state while creation is pending', async () => {
    const user = userEvent.setup()

    createHabitMock.mockImplementation(
      () => new Promise<HabitResponse>(() => undefined),
    )

    render(<HabitForm onHabitCreated={vi.fn()} />)

    await user.type(screen.getByRole('textbox', { name: 'Name' }), 'Read')

    await user.click(
      screen.getByRole('button', {
        name: 'Create habit',
      }),
    )

    expect(
      screen.getByRole('button', {
        name: 'Creating habit...',
      }),
    ).toBeDisabled()
  })

  it('shows the backend error when habit creation fails', async () => {
    const user = userEvent.setup()

    createHabitMock.mockRejectedValue(
      new Error('Daily habits must have a target count of 1.'),
    )

    render(<HabitForm onHabitCreated={vi.fn()} />)

    await user.type(screen.getByRole('textbox', { name: 'Name' }), 'Read')

    await user.click(
      screen.getByRole('button', {
        name: 'Create habit',
      }),
    )

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Habit creation error: Daily habits must have a target count of 1.',
    )
  })
})
