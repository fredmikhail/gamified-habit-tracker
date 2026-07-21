import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  completeHabit,
  deactivateHabit,
  getHabits,
  updateHabit,
} from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitList } from './HabitList'

vi.mock('../../api/habitsApi', () => ({
  completeHabit: vi.fn(),
  deactivateHabit: vi.fn(),
  getHabits: vi.fn(),
  undoHabitCompletion: vi.fn(),
  updateHabit: vi.fn(),
}))

const deactivateHabitMock = vi.mocked(deactivateHabit)
const getHabitsMock = vi.mocked(getHabits)
const updateHabitMock = vi.mocked(updateHabit)
const completeHabitMock = vi.mocked(completeHabit)

describe('HabitList', () => {
  beforeEach(() => {
    completeHabitMock.mockReset()
    deactivateHabitMock.mockReset()
    getHabitsMock.mockReset()
    updateHabitMock.mockReset()
  })

  it('updates the displayed completion state without reloading the habit list', async () => {
    const user = userEvent.setup()

    const habit: HabitResponse = {
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

    getHabitsMock.mockResolvedValue([habit])

    completeHabitMock.mockResolvedValue({
      completion: {
        id: '019c0000-0000-7000-8000-000000000002',
        habitId: habit.id,
        completedDate: '2026-07-19',
        completedAtUtc: '2026-07-20T01:30:00Z',
        notes: null,
      },
      rewards: [
        {
          attributeType: 'mind',
          xpAmount: 14,
        },
        {
          attributeType: 'focus',
          xpAmount: 6,
        },
      ],
    })

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    await screen.findByRole('heading', {
      name: 'Read C# textbook',
    })

    await user.click(
      screen.getByRole('button', {
        name: 'Mark complete',
      }),
    )

    expect(completeHabitMock).toHaveBeenCalledWith(habit.id, {
      notes: null,
    })

    expect(
      screen.getByRole('button', {
        name: 'Undo completion',
      }),
    ).toHaveAttribute('aria-pressed', 'true')

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
    expect(screen.getByRole('status')).toHaveTextContent('+14 Mind XP')
    expect(screen.getByRole('status')).toHaveTextContent('+6 Focus XP')
  })

  it('shows a loading message while habits are being requested', () => {
    getHabitsMock.mockImplementation(
      () => new Promise<HabitResponse[]>(() => undefined),
    )

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    expect(screen.getByText('Loading habits...')).toBeInTheDocument()
  })

  it('shows an error message when habits cannot be loaded', async () => {
    getHabitsMock.mockRejectedValue(
      new Error('The habits could not be loaded.'),
    )

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    expect(
      await screen.findByText(
        'Habit loading error: The habits could not be loaded.',
      ),
    ).toBeInTheDocument()
  })

  it('shows an empty message when the user has no active habits', async () => {
    getHabitsMock.mockResolvedValue([])

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    expect(
      await screen.findByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
    expect(getHabitsMock).toHaveBeenCalledWith()
  })

  it('renders the loaded habits', async () => {
    const habits: HabitResponse[] = [
      {
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
      },
      {
        id: '019c0000-0000-7000-8000-000000000002',
        name: 'Go to gym',
        description: null,
        category: 'fitnessAndMovement',
        frequencyType: 'weekly',
        targetCount: 3,
        difficulty: 'elite',
        isActive: true,
        isCompletedToday: false,
        createdAtUtc: '2026-07-18T12:00:00Z',
        updatedAtUtc: '2026-07-18T12:00:00Z',
      },
    ]

    getHabitsMock.mockResolvedValue(habits)

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    expect(
      await screen.findByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'Go to gym',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('Read one chapter.')).toBeInTheDocument()

    expect(screen.getByText('Frequency: Daily')).toBeInTheDocument()

    expect(screen.getByText('Frequency: 3 times per week')).toBeInTheDocument()

    expect(screen.getByText('Category: Learning & Skills')).toBeInTheDocument()

    expect(screen.getByText('Category: Fitness & Movement')).toBeInTheDocument()

    expect(screen.getByText('Medium')).toBeInTheDocument()

    expect(screen.getByText('Elite')).toBeInTheDocument()
  })

  it('reloads habits when the refresh key changes', async () => {
    getHabitsMock.mockResolvedValue([])

    const { rerender } = render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    await screen.findByText('You do not have any habits yet.')

    expect(getHabitsMock).toHaveBeenCalledTimes(1)

    rerender(
      <HabitList
        refreshKey={1}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    await waitFor(() => {
      expect(getHabitsMock).toHaveBeenCalledTimes(2)
    })
  })

  it('updates a habit and reports that the list should refresh', async () => {
    const user = userEvent.setup()
    const onHabitUpdated = vi.fn()

    const habit: HabitResponse = {
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

    const updatedHabit: HabitResponse = {
      ...habit,
      name: 'Read TypeScript book',
      difficulty: 'hard',
      updatedAtUtc: '2026-07-20T12:00:00Z',
    }

    getHabitsMock.mockResolvedValue([habit])
    updateHabitMock.mockResolvedValue(updatedHabit)

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={onHabitUpdated}
        onProgressChanged={vi.fn()}
      />,
    )

    await screen.findByRole('heading', {
      name: 'Read C# textbook',
    })

    await user.click(
      screen.getByRole('button', {
        name: 'Edit',
      }),
    )

    const nameInput = screen.getByRole('textbox', {
      name: 'Name',
    })

    await user.clear(nameInput)
    await user.type(nameInput, 'Read TypeScript book')

    await user.selectOptions(
      screen.getByRole('combobox', {
        name: 'Difficulty',
      }),
      'hard',
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Save changes',
      }),
    )

    expect(updateHabitMock).toHaveBeenCalledWith(habit.id, {
      name: 'Read TypeScript book',
      description: 'Read one chapter.',
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'hard',
    })

    expect(onHabitUpdated).toHaveBeenCalledWith(updatedHabit)

    expect(
      screen.queryByRole('form', {
        name: 'Edit Read C# textbook',
      }),
    ).not.toBeInTheDocument()
  })

  it('closes the edit form without updating when canceled', async () => {
    const user = userEvent.setup()

    const habit: HabitResponse = {
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

    getHabitsMock.mockResolvedValue([habit])

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={vi.fn()}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    await screen.findByRole('heading', {
      name: 'Read C# textbook',
    })

    await user.click(
      screen.getByRole('button', {
        name: 'Edit',
      }),
    )

    expect(
      screen.getByRole('form', {
        name: 'Edit Read C# textbook',
      }),
    ).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    )

    expect(
      screen.queryByRole('form', {
        name: 'Edit Read C# textbook',
      }),
    ).not.toBeInTheDocument()

    expect(updateHabitMock).not.toHaveBeenCalled()
  })

  it('deactivates a habit and reports that the list should refresh', async () => {
    const user = userEvent.setup()
    const onHabitDeactivated = vi.fn()

    const habit: HabitResponse = {
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

    const deactivatedHabit: HabitResponse = {
      ...habit,
      isActive: false,
      isCompletedToday: false,
      updatedAtUtc: '2026-07-20T12:00:00Z',
    }

    getHabitsMock.mockResolvedValue([habit])

    deactivateHabitMock.mockResolvedValue(deactivatedHabit)

    render(
      <HabitList
        refreshKey={0}
        onHabitDeactivated={onHabitDeactivated}
        onHabitUpdated={vi.fn()}
        onProgressChanged={vi.fn()}
      />,
    )

    await screen.findByRole('heading', {
      name: 'Read C# textbook',
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

    expect(deactivateHabitMock).toHaveBeenCalledTimes(1)

    expect(deactivateHabitMock).toHaveBeenCalledWith(habit.id)

    expect(onHabitDeactivated).toHaveBeenCalledTimes(1)

    expect(onHabitDeactivated).toHaveBeenCalledWith(deactivatedHabit)
  })
})
