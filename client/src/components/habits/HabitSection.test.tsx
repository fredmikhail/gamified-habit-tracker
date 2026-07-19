import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createHabit, deactivateHabit, getHabits } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitSection } from './HabitSection'

vi.mock('../../api/habitsApi', () => ({
  createHabit: vi.fn(),
  deactivateHabit: vi.fn(),
  getHabits: vi.fn(),
  updateHabit: vi.fn(),
}))

const createHabitMock = vi.mocked(createHabit)
const deactivateHabitMock = vi.mocked(deactivateHabit)
const getHabitsMock = vi.mocked(getHabits)

describe('HabitSection', () => {
  beforeEach(() => {
    createHabitMock.mockReset()
    deactivateHabitMock.mockReset()
    getHabitsMock.mockReset()
  })

  it('reloads and displays habits after creating one', async () => {
    const user = userEvent.setup()

    const createdHabit: HabitResponse = {
      id: '019c0000-0000-7000-8000-000000000001',
      name: 'Read C# textbook',
      description: null,
      category: 'Learning',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      isActive: true,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-19T12:00:00Z',
    }

    getHabitsMock
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([createdHabit])

    createHabitMock.mockResolvedValue(createdHabit)

    render(<HabitSection />)

    await screen.findByText('You do not have any habits yet.')

    await user.type(
      screen.getByRole('textbox', {
        name: 'Name',
      }),
      'Read C# textbook',
    )

    await user.type(
      screen.getByRole('textbox', {
        name: 'Category',
      }),
      'Learning',
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Create habit',
      }),
    )

    expect(
      await screen.findByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    expect(createHabitMock).toHaveBeenCalledTimes(1)
    expect(getHabitsMock).toHaveBeenCalledTimes(2)
  })

  it('reloads the active list after deactivating a habit', async () => {
    const user = userEvent.setup()

    const activeHabit: HabitResponse = {
      id: '019c0000-0000-7000-8000-000000000001',
      name: 'Read C# textbook',
      description: null,
      category: 'Learning',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      isActive: true,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-19T12:00:00Z',
    }

    const deactivatedHabit: HabitResponse = {
      ...activeHabit,
      isActive: false,
      updatedAtUtc: '2026-07-20T12:00:00Z',
    }

    getHabitsMock.mockResolvedValueOnce([activeHabit]).mockResolvedValueOnce([])

    deactivateHabitMock.mockResolvedValue(deactivatedHabit)

    render(<HabitSection />)

    expect(
      await screen.findByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

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
      await screen.findByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    expect(deactivateHabitMock).toHaveBeenCalledTimes(1)
    expect(deactivateHabitMock).toHaveBeenCalledWith(activeHabit.id)
    expect(getHabitsMock).toHaveBeenCalledTimes(2)

    expect(
      screen.queryByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).not.toBeInTheDocument()
  })
})
