import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, test, vi } from 'vitest'
import { getAttributes } from '../api/attributesApi'
import { getDashboard } from '../api/dashboardApi'
import { getHabits } from '../api/habitsApi'
import type { DashboardResponse } from '../types/DashboardResponse'
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
    completions: 1,
    xpEarned: 20,
  },
  todayExecution: {
    completedDailyHabits: 1,
    totalDailyHabits: 2,
  },
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

function WorkspaceDataControls() {
  const { dashboardResource, attributesResource, refreshProgress } =
    useWorkspaceData()

  const loadDashboard = dashboardResource.ensureLoaded
  const loadAttributes = attributesResource.ensureLoaded

  return (
    <>
      <button type="button" onClick={() => void loadDashboard()}>
        Load dashboard
      </button>

      <button type="button" onClick={() => void loadAttributes()}>
        Load attributes
      </button>

      <button type="button" onClick={() => void refreshProgress()}>
        Refresh progress
      </button>

      {dashboardResource.data && (
        <p>Total XP: {dashboardResource.data.overallProgress.totalXp}</p>
      )}

      {attributesResource.data && (
        <p>Attribute: {attributesResource.data[0]?.attributeType}</p>
      )}
    </>
  )
}

function renderWorkspaceDataControls() {
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
    getHabitsMock.mockResolvedValue([])
  })

  test('loads only the resource requested by a consumer', async () => {
    const user = userEvent.setup()

    renderWorkspaceDataControls()

    await user.click(
      screen.getByRole('button', {
        name: 'Load attributes',
      }),
    )

    expect(await screen.findByText('Attribute: discipline')).toBeInTheDocument()

    expect(getAttributesMock).toHaveBeenCalledTimes(1)
    expect(getDashboardMock).not.toHaveBeenCalled()
    expect(getHabitsMock).not.toHaveBeenCalled()
  })

  test('refreshes only progression resources already loaded', async () => {
    const user = userEvent.setup()

    renderWorkspaceDataControls()

    await user.click(
      screen.getByRole('button', {
        name: 'Load attributes',
      }),
    )

    expect(await screen.findByText('Attribute: discipline')).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Refresh progress',
      }),
    )

    await waitFor(() => {
      expect(getAttributesMock).toHaveBeenCalledTimes(2)
    })

    expect(getDashboardMock).not.toHaveBeenCalled()
    expect(getHabitsMock).not.toHaveBeenCalled()
  })

  test('refreshes dashboard and attributes together when both were loaded', async () => {
    const user = userEvent.setup()

    renderWorkspaceDataControls()

    await user.click(
      screen.getByRole('button', {
        name: 'Load dashboard',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Load attributes',
      }),
    )

    expect(await screen.findByText('Total XP: 120')).toBeInTheDocument()

    expect(await screen.findByText('Attribute: discipline')).toBeInTheDocument()

    expect(getDashboardMock).toHaveBeenCalledTimes(1)
    expect(getAttributesMock).toHaveBeenCalledTimes(1)

    await user.click(
      screen.getByRole('button', {
        name: 'Refresh progress',
      }),
    )

    await waitFor(() => {
      expect(getDashboardMock).toHaveBeenCalledTimes(2)
      expect(getAttributesMock).toHaveBeenCalledTimes(2)
    })

    expect(getHabitsMock).not.toHaveBeenCalled()
  })
})
