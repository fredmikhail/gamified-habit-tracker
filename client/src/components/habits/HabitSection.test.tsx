import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getAttributes } from '../../api/attributesApi'
import { getDashboard } from '../../api/dashboardApi'
import { createHabit, deactivateHabit, getHabits } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitSection } from './HabitSection'

vi.mock('../../api/attributesApi', () => ({
  getAttributes: vi.fn(),
}))

vi.mock('../../api/dashboardApi', () => ({
  getDashboard: vi.fn(),
}))

vi.mock('../../api/habitsApi', () => ({
  completeHabit: vi.fn(),
  createHabit: vi.fn(),
  deactivateHabit: vi.fn(),
  getHabits: vi.fn(),
  undoHabitCompletion: vi.fn(),
  updateHabit: vi.fn(),
}))

const getAttributesMock = vi.mocked(getAttributes)
const getDashboardMock = vi.mocked(getDashboard)
const createHabitMock = vi.mocked(createHabit)
const deactivateHabitMock = vi.mocked(deactivateHabit)
const getHabitsMock = vi.mocked(getHabits)

describe('HabitSection', () => {
  beforeEach(() => {
    getDashboardMock.mockReset()

    getDashboardMock.mockResolvedValue({
      overallProgress: {
        totalXp: 0,
        level: 1,
        xpIntoCurrentLevel: 0,
        xpNeededForNextLevel: 200,
      },
      todayActivity: {
        localDate: '2026-07-22',
        completions: 0,
        xpEarned: 0,
      },
      todayExecution: {
        completedDailyHabits: 0,
        totalDailyHabits: 0,
      },
      habitStreaks: [],
    })

    getAttributesMock.mockReset()
    getAttributesMock.mockResolvedValue([])

    createHabitMock.mockReset()
    deactivateHabitMock.mockReset()
    getHabitsMock.mockReset()
  })

  it('reloads habits and dashboard after creating one', async () => {
    const user = userEvent.setup()

    const createdHabit: HabitResponse = {
      id: '019c0000-0000-7000-8000-000000000001',
      name: 'Read C# textbook',
      description: null,
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      isActive: true,
      isCompletedToday: false,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-19T12:00:00Z',
    }

    getHabitsMock
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([createdHabit])

    createHabitMock.mockResolvedValue(createdHabit)

    render(<HabitSection />)

    await screen.findByText('You do not have any habits yet.')

    await waitFor(() => {
      expect(getDashboardMock).toHaveBeenCalledTimes(1)
    })

    await user.type(
      screen.getByRole('textbox', {
        name: 'Name',
      }),
      'Read C# textbook',
    )

    await user.selectOptions(
      screen.getByRole('combobox', {
        name: 'Category',
      }),
      'learningAndSkills',
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

    expect(createHabitMock).toHaveBeenCalledWith({
      name: 'Read C# textbook',
      description: null,
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
    })

    expect(getHabitsMock).toHaveBeenCalledTimes(2)

    await waitFor(() => {
      expect(getDashboardMock).toHaveBeenCalledTimes(2)
    })
  })

  it('reloads the active list and dashboard after deactivating a habit', async () => {
    const user = userEvent.setup()

    const activeHabit: HabitResponse = {
      id: '019c0000-0000-7000-8000-000000000001',
      name: 'Read C# textbook',
      description: null,
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      isActive: true,
      isCompletedToday: false,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-19T12:00:00Z',
    }

    const deactivatedHabit: HabitResponse = {
      ...activeHabit,
      isActive: false,
      isCompletedToday: false,
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

    await waitFor(() => {
      expect(getDashboardMock).toHaveBeenCalledTimes(1)
    })

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

    await waitFor(() => {
      expect(getDashboardMock).toHaveBeenCalledTimes(2)
    })

    expect(
      screen.queryByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).not.toBeInTheDocument()
  })
})
