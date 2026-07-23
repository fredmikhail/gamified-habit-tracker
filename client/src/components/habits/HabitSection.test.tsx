import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createHabit, deactivateHabit, getHabits } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { WorkspaceDataProvider } from '../../workspace/WorkspaceDataProvider'
import { HabitSection } from './HabitSection'

vi.mock('../../api/habitsApi', () => ({
  completeHabit: vi.fn(),
  createHabit: vi.fn(),
  deactivateHabit: vi.fn(),
  getHabits: vi.fn(),
  undoHabitCompletion: vi.fn(),
  updateHabit: vi.fn(),
}))

const createHabitMock = vi.mocked(createHabit)
const deactivateHabitMock = vi.mocked(deactivateHabit)
const getHabitsMock = vi.mocked(getHabits)
const onProgressChanged = vi.fn()

function createHabitFixture(
  overrides: Partial<HabitResponse> = {},
): HabitResponse {
  return {
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
    ...overrides,
  }
}

function renderHabitSection() {
  return render(
    <WorkspaceDataProvider>
      <HabitSection onProgressChanged={onProgressChanged} />
    </WorkspaceDataProvider>,
  )
}

describe('HabitSection', () => {
  beforeEach(() => {
    createHabitMock.mockReset()
    deactivateHabitMock.mockReset()
    getHabitsMock.mockReset()
    onProgressChanged.mockReset()
  })

  it('adds a created habit without reloading the habit list', async () => {
    const user = userEvent.setup()
    const createdHabit = createHabitFixture()

    getHabitsMock.mockResolvedValue([])
    createHabitMock.mockResolvedValue(createdHabit)

    renderHabitSection()

    expect(
      await screen.findByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Create Habit',
      }),
    )

    expect(
      screen.getByRole('form', {
        name: 'Create habit',
      }),
    ).toBeInTheDocument()

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

    const habitList = await screen.findByRole('list')

    expect(
      within(habitList).getByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).toBeInTheDocument()

    const inspector = screen.getByRole('complementary')

    expect(
      within(inspector).getByRole('heading', {
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

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
    expect(getHabitsMock).toHaveBeenCalledWith(true)

    await waitFor(() => {
      expect(onProgressChanged).toHaveBeenCalledTimes(1)
    })
  })

  it('moves a deactivated habit out of the active view without reloading', async () => {
    const user = userEvent.setup()
    const activeHabit = createHabitFixture()

    const deactivatedHabit = createHabitFixture({
      isActive: false,
      updatedAtUtc: '2026-07-20T12:00:00Z',
    })

    getHabitsMock.mockResolvedValue([activeHabit])
    deactivateHabitMock.mockResolvedValue(deactivatedHabit)

    renderHabitSection()

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

    expect(
      await screen.findByText('You do not have any habits yet.'),
    ).toBeInTheDocument()

    expect(deactivateHabitMock).toHaveBeenCalledTimes(1)
    expect(deactivateHabitMock).toHaveBeenCalledWith(activeHabit.id)

    expect(getHabitsMock).toHaveBeenCalledTimes(1)
    expect(getHabitsMock).toHaveBeenCalledWith(true)

    await waitFor(() => {
      expect(onProgressChanged).toHaveBeenCalledTimes(1)
    })

    expect(
      screen.queryByRole('heading', {
        name: 'Read C# textbook',
      }),
    ).not.toBeInTheDocument()
  })
})
