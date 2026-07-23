import { useState, type ComponentProps } from 'react'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  completeHabit,
  deactivateHabit,
  getHabits,
  updateHabit,
} from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { WorkspaceDataProvider } from '../../workspace/WorkspaceDataProvider'
import { HabitList } from './HabitList'

vi.mock('../../api/habitsApi', () => ({
  completeHabit: vi.fn(),
  createHabit: vi.fn(),
  deactivateHabit: vi.fn(),
  getHabits: vi.fn(),
  undoHabitCompletion: vi.fn(),
  updateHabit: vi.fn(),
}))

const completeHabitMock = vi.mocked(completeHabit)
const deactivateHabitMock = vi.mocked(deactivateHabit)
const getHabitsMock = vi.mocked(getHabits)
const updateHabitMock = vi.mocked(updateHabit)

function createHabitFixture(
  overrides: Partial<HabitResponse> = {},
): HabitResponse {
  return {
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
    ...overrides,
  }
}

function renderHabitList(
  overrides: Partial<ComponentProps<typeof HabitList>> = {},
) {
  const props: ComponentProps<typeof HabitList> = {
    onHabitDeactivated: vi.fn(),
    onHabitUpdated: vi.fn(),
    onProgressChanged: vi.fn(),
    ...overrides,
  }

  return render(
    <WorkspaceDataProvider>
      <HabitList {...props} />
    </WorkspaceDataProvider>,
  )
}

describe('HabitList', () => {
  beforeEach(() => {
    completeHabitMock.mockReset()
    deactivateHabitMock.mockReset()
    getHabitsMock.mockReset()
    updateHabitMock.mockReset()
  })

  it('updates the displayed completion state without reloading the habit list', async () => {
    const user = userEvent.setup()
    const habit = createHabitFixture()

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

    renderHabitList()

    const habitList = await screen.findByRole('list')

    expect(
      within(habitList).getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Complete Read C# textbook',
      }),
    )

    expect(completeHabitMock).toHaveBeenCalledWith(habit.id, {
      notes: null,
    })

    expect(
      screen.getByRole('button', {
        name: 'Undo completion for Read C# textbook',
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

    renderHabitList()

    expect(screen.getByText('Loading habits...')).toBeInTheDocument()
  })

  it('shows an error message when habits cannot be loaded', async () => {
    getHabitsMock.mockRejectedValue(
      new Error('The habits could not be loaded.'),
    )

    renderHabitList()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Habit loading error: The habits could not be loaded.',
    )
  })

  it('shows an empty message when the user has no active habits', async () => {
    getHabitsMock.mockResolvedValue([])

    renderHabitList()

    expect(
      await screen.findByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
    expect(getHabitsMock).toHaveBeenCalledWith(true)
  })

  it('renders the loaded habits', async () => {
    const habits: HabitResponse[] = [
      createHabitFixture(),
      createHabitFixture({
        id: '019c0000-0000-7000-8000-000000000002',
        name: 'Go to gym',
        description: null,
        category: 'fitnessAndMovement',
        frequencyType: 'weekly',
        targetCount: 3,
        difficulty: 'elite',
        createdAtUtc: '2026-07-18T12:00:00Z',
        updatedAtUtc: '2026-07-18T12:00:00Z',
      }),
    ]

    getHabitsMock.mockResolvedValue(habits)

    renderHabitList()

    const habitList = await screen.findByRole('list')

    expect(
      within(habitList).getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    expect(
      within(habitList).getByRole('heading', {
        name: 'Go to gym',
      }),
    ).toBeInTheDocument()

    expect(within(habitList).getByText('Read one chapter.')).toBeInTheDocument()

    expect(within(habitList).getByText('Frequency: Daily')).toBeInTheDocument()

    expect(
      within(habitList).getByText('Frequency: 3 times per week'),
    ).toBeInTheDocument()

    expect(
      within(habitList).getByText('Category: Learning & Skills'),
    ).toBeInTheDocument()

    expect(
      within(habitList).getByText('Category: Fitness & Movement'),
    ).toBeInTheDocument()

    expect(within(habitList).getByText('Medium')).toBeInTheDocument()
    expect(within(habitList).getByText('Elite')).toBeInTheDocument()
  })

  it('reuses cached habits when the list returns', async () => {
    const user = userEvent.setup()

    getHabitsMock.mockResolvedValue([])

    function PersistentHabitListHarness() {
      const [isVisible, setIsVisible] = useState(true)

      return (
        <WorkspaceDataProvider>
          <button
            type="button"
            onClick={() => setIsVisible((currentValue) => !currentValue)}
          >
            Toggle habits
          </button>

          {isVisible && (
            <HabitList
              onHabitDeactivated={vi.fn()}
              onHabitUpdated={vi.fn()}
              onProgressChanged={vi.fn()}
            />
          )}
        </WorkspaceDataProvider>
      )
    }

    render(<PersistentHabitListHarness />)

    expect(
      await screen.findByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    expect(getHabitsMock).toHaveBeenCalledTimes(1)

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle habits',
      }),
    )

    expect(
      screen.queryByText('You do not have any habits yet.'),
    ).not.toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle habits',
      }),
    )

    expect(
      screen.getByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    expect(screen.queryByText('Loading habits...')).not.toBeInTheDocument()

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
  })

  it('updates a habit and displays its scheduled rule changes', async () => {
    const user = userEvent.setup()
    const onHabitUpdated = vi.fn()

    const habit = createHabitFixture()

    const updatedHabit = createHabitFixture({
      name: 'Read TypeScript book',
      pendingConfiguration: {
        effectiveFromDate: '2026-07-27',
        category: 'learningAndSkills',
        frequencyType: 'daily',
        targetCount: 1,
        difficulty: 'hard',
      },
      updatedAtUtc: '2026-07-20T12:00:00Z',
    })

    getHabitsMock.mockResolvedValue([habit])
    updateHabitMock.mockResolvedValue(updatedHabit)

    renderHabitList({
      onHabitUpdated,
    })

    const habitList = await screen.findByRole('list')

    expect(
      within(habitList).getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    await user.click(
      await screen.findByRole('button', {
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

    expect(
      within(habitList).getByRole('heading', {
        name: 'Read TypeScript book',
      }),
    ).toBeInTheDocument()

    const inspector = screen.getByRole('complementary')

    expect(
      within(inspector).getByRole('heading', {
        name: 'Read TypeScript book',
      }),
    ).toBeInTheDocument()

    const scheduledChanges = screen.getByRole('region', {
      name: 'Scheduled changes for Read TypeScript book',
    })

    expect(scheduledChanges).toHaveTextContent('Scheduled for July 27, 2026')

    expect(scheduledChanges).toHaveTextContent('Difficulty: Hard')

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
  })

  it('closes the edit form without updating when canceled', async () => {
    const user = userEvent.setup()

    const habit = createHabitFixture({
      description: null,
    })

    getHabitsMock.mockResolvedValue([habit])

    renderHabitList()

    const habitList = await screen.findByRole('list')

    expect(
      within(habitList).getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    await user.click(
      await screen.findByRole('button', {
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

  it('moves a deactivated habit from the active tab to the inactive tab', async () => {
    const user = userEvent.setup()
    const onHabitDeactivated = vi.fn()
    const habit = createHabitFixture()

    const deactivatedHabit = createHabitFixture({
      isActive: false,
      updatedAtUtc: '2026-07-20T12:00:00Z',
    })

    getHabitsMock.mockResolvedValue([habit])
    deactivateHabitMock.mockResolvedValue(deactivatedHabit)

    renderHabitList({
      onHabitDeactivated,
    })

    const activeHabitList = await screen.findByRole('list')

    expect(
      within(activeHabitList).getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    await user.click(
      await screen.findByRole('button', {
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

    expect(
      await screen.findByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    await user.click(
      screen.getByRole('tab', {
        name: /Inactive Habits/,
      }),
    )

    const inactiveHabitList = await screen.findByRole('list')

    expect(
      within(inactiveHabitList).getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    expect(screen.getByLabelText('Inactive habit')).toBeInTheDocument()

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
  })
})
