import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, test, vi } from 'vitest'
import { getAttributes } from '../api/attributesApi'
import { getDashboard } from '../api/dashboardApi'
import { getHabits } from '../api/habitsApi'
import type { DashboardResponse } from '../types/DashboardResponse'
import type { HabitResponse } from '../types/HabitResponse'
import type { UserAttributeResponse } from '../types/UserAttributeResponse'
import { WorkspaceDataProvider } from './WorkspaceDataProvider'
import { useWorkspaceData } from './useWorkspaceData'

vi.mock('../api/attributesApi', () => ({
  getAttributes: vi.fn(),
}))

vi.mock('../api/dashboardApi', () => ({
  getDashboard: vi.fn(),
}))

vi.mock('../api/habitsApi', () => ({
  getHabits: vi.fn(),
}))

const getAttributesMock = vi.mocked(getAttributes)

const getDashboardMock = vi.mocked(getDashboard)

const getHabitsMock = vi.mocked(getHabits)

const dashboardResponse: DashboardResponse = {
  overallProgress: {
    totalXp: 120,
    level: 2,
    xpIntoCurrentLevel: 20,
    xpNeededForNextLevel: 300,
  },
  todayActivity: {
    localDate: '2026-07-22',
    completions: 0,
    xpEarned: 0,
  },
  todayExecution: {
    completedDailyHabits: 0,
    totalDailyHabits: 1,
  },
  todayHabits: [
    {
      id: 'habit-1',
      name: 'Read',
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      attributeRewards: [],
      isCompletedToday: false,
      currentStreak: 0,
      longestStreak: 0,
    },
  ],
  attributes: [],
  habitStreaks: [],
}

const attributeResponses: UserAttributeResponse[] = [
  {
    attributeType: 'discipline',
    currentXp: 70,
    level: 1,
    xpIntoCurrentLevel: 70,
    xpNeededForNextLevel: 200,
  },
]

const habitResponses: HabitResponse[] = [
  {
    id: 'habit-1',
    name: 'Read',
    description: null,
    category: 'learningAndSkills',
    frequencyType: 'daily',
    targetCount: 1,
    difficulty: 'medium',
    isActive: true,
    isCompletedToday: false,
    createdAtUtc: '2026-07-22T10:00:00Z',
    updatedAtUtc: '2026-07-22T10:00:00Z',
  },
]

function WorkspaceDataControls() {
  const {
    dashboardResource,
    attributesResource,
    habitsResource,
    setHabitCompletionStatus,
    refreshProgress,
  } = useWorkspaceData()

  return (
    <>
      <button
        type="button"
        onClick={() => void dashboardResource.ensureLoaded()}
      >
        Load dashboard
      </button>

      <button
        type="button"
        onClick={() => void attributesResource.ensureLoaded()}
      >
        Load attributes
      </button>

      <button type="button" onClick={() => void habitsResource.ensureLoaded()}>
        Load habits
      </button>

      <button
        type="button"
        onClick={() => setHabitCompletionStatus('habit-1', true)}
      >
        Mark cached habit complete
      </button>

      <button type="button" onClick={() => void refreshProgress()}>
        Refresh progress
      </button>

      {dashboardResource.data && (
        <>
          <p>Total XP: {dashboardResource.data.overallProgress.totalXp}</p>

          <p>
            Dashboard completed:{' '}
            {String(dashboardResource.data.todayHabits[0]?.isCompletedToday)}
          </p>
        </>
      )}

      {attributesResource.data && (
        <p>Attribute: {attributesResource.data[0]?.attributeType}</p>
      )}

      {habitsResource.data && (
        <p>
          Habit completed: {String(habitsResource.data[0]?.isCompletedToday)}
        </p>
      )}
    </>
  )
}

function renderControls() {
  return render(
    <WorkspaceDataProvider>
      <WorkspaceDataControls />
    </WorkspaceDataProvider>,
  )
}

describe('WorkspaceDataProvider', () => {
  beforeEach(() => {
    getAttributesMock.mockReset()
    getDashboardMock.mockReset()
    getHabitsMock.mockReset()

    getAttributesMock.mockResolvedValue(attributeResponses)

    getDashboardMock.mockResolvedValue(dashboardResponse)

    getHabitsMock.mockResolvedValue(habitResponses)
  })

  test('loads only the requested resource', async () => {
    const user = userEvent.setup()

    renderControls()

    await user.click(
      screen.getByRole('button', {
        name: 'Load attributes',
      }),
    )

    expect(await screen.findByText('Attribute: discipline')).toBeInTheDocument()

    expect(getDashboardMock).not.toHaveBeenCalled()
    expect(getHabitsMock).not.toHaveBeenCalled()
  })

  test('refreshes only progression resources already loaded', async () => {
    const user = userEvent.setup()

    renderControls()

    await user.click(
      screen.getByRole('button', {
        name: 'Load dashboard',
      }),
    )

    expect(await screen.findByText('Total XP: 120')).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Refresh progress',
      }),
    )

    await waitFor(() => {
      expect(getDashboardMock).toHaveBeenCalledTimes(2)
    })

    expect(getAttributesMock).not.toHaveBeenCalled()
    expect(getHabitsMock).not.toHaveBeenCalled()
  })

  test('synchronizes successful completion status across loaded caches', async () => {
    const user = userEvent.setup()

    renderControls()

    await user.click(
      screen.getByRole('button', {
        name: 'Load dashboard',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Load habits',
      }),
    )

    expect(
      await screen.findByText('Dashboard completed: false'),
    ).toBeInTheDocument()

    expect(screen.getByText('Habit completed: false')).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Mark cached habit complete',
      }),
    )

    expect(screen.getByText('Dashboard completed: true')).toBeInTheDocument()

    expect(screen.getByText('Habit completed: true')).toBeInTheDocument()
  })
})
