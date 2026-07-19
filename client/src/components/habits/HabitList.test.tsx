import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getHabits } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitList } from './HabitList'

vi.mock('../../api/habitsApi', () => ({
  getHabits: vi.fn(),
}))

const getHabitsMock = vi.mocked(getHabits)

describe('HabitList', () => {
  beforeEach(() => {
    getHabitsMock.mockReset()
  })

  it('shows a loading message while habits are being requested', () => {
    getHabitsMock.mockImplementation(
      () => new Promise<HabitResponse[]>(() => undefined),
    )

    render(<HabitList />)

    expect(screen.getByText('Loading habits...')).toBeInTheDocument()
  })

  it('shows an error message when habits cannot be loaded', async () => {
    getHabitsMock.mockRejectedValue(
      new Error('The habits could not be loaded.'),
    )

    render(<HabitList />)

    expect(
      await screen.findByText(
        'Habit loading error: The habits could not be loaded.',
      ),
    ).toBeInTheDocument()
  })

  it('shows an empty message when the user has no active habits', async () => {
    getHabitsMock.mockResolvedValue([])

    render(<HabitList />)

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
        category: 'Learning',
        frequencyType: 'daily',
        targetCount: 1,
        difficulty: 'medium',
        isActive: true,
        createdAtUtc: '2026-07-19T12:00:00Z',
        updatedAtUtc: '2026-07-19T12:00:00Z',
      },
      {
        id: '019c0000-0000-7000-8000-000000000002',
        name: 'Go to gym',
        description: null,
        category: 'Fitness',
        frequencyType: 'weekly',
        targetCount: 3,
        difficulty: 'elite',
        isActive: true,
        createdAtUtc: '2026-07-18T12:00:00Z',
        updatedAtUtc: '2026-07-18T12:00:00Z',
      },
    ]

    getHabitsMock.mockResolvedValue(habits)

    render(<HabitList />)

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

    expect(
      screen.getByText('Frequency: 3 times per week'),
    ).toBeInTheDocument()

    expect(screen.getByText('Category: Learning')).toBeInTheDocument()
    expect(screen.getByText('Category: Fitness')).toBeInTheDocument()
    expect(screen.getByText('Medium')).toBeInTheDocument()
    expect(screen.getByText('Elite')).toBeInTheDocument()
  })
})