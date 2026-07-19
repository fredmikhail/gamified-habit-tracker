import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createHabit, getHabits } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitSection } from './HabitSection'

vi.mock('../../api/habitsApi', () => ({
  createHabit: vi.fn(),
  getHabits: vi.fn(),
}))

const createHabitMock = vi.mocked(createHabit)
const getHabitsMock = vi.mocked(getHabits)

describe('HabitSection', () => {
  beforeEach(() => {
    createHabitMock.mockReset()
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
})
