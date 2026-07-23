import { useCallback, type PropsWithChildren } from 'react'
import { getAttributeOverview } from '../api/attributesApi'
import { getDashboard } from '../api/dashboardApi'
import { getHabits } from '../api/habitsApi'
import { WorkspaceDataContext } from './WorkspaceDataContext'
import { useCachedResource } from './useCachedResource'

function getDashboardErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown dashboard-loading error occurred.'
}

function getAttributeOverviewErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown attribute-overview error occurred.'
}

function getHabitErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown habit-loading error occurred.'
}

export function WorkspaceDataProvider({ children }: PropsWithChildren) {
  const dashboardResource = useCachedResource({
    load: getDashboard,
    getErrorMessage: getDashboardErrorMessage,
  })

  const attributeOverviewResource = useCachedResource({
    load: getAttributeOverview,
    getErrorMessage: getAttributeOverviewErrorMessage,
  })

  const habitsResource = useCachedResource({
    load: () => getHabits(true),
    getErrorMessage: getHabitErrorMessage,
  })

  const {
    data: dashboardData,
    refresh: refreshDashboard,
    updateData: updateDashboard,
  } = dashboardResource

  const { data: attributeOverviewData, refresh: refreshAttributeOverview } =
    attributeOverviewResource

  const { updateData: updateHabits } = habitsResource

  const setHabitCompletionStatus = useCallback(
    (habitId: string, isCompletedToday: boolean): void => {
      updateDashboard((currentDashboard) => {
        if (currentDashboard === null) {
          return null
        }

        return {
          ...currentDashboard,
          todayHabits: currentDashboard.todayHabits.map((habit) =>
            habit.id === habitId
              ? {
                  ...habit,
                  isCompletedToday,
                }
              : habit,
          ),
        }
      })

      updateHabits((currentHabits) => {
        if (currentHabits === null) {
          return null
        }

        return currentHabits.map((habit) =>
          habit.id === habitId
            ? {
                ...habit,
                isCompletedToday,
              }
            : habit,
        )
      })
    },
    [updateDashboard, updateHabits],
  )

  const refreshProgress = useCallback(async (): Promise<void> => {
    const refreshRequests: Promise<void>[] = []

    if (dashboardData !== null) {
      refreshRequests.push(refreshDashboard())
    }

    if (attributeOverviewData !== null) {
      refreshRequests.push(refreshAttributeOverview())
    }

    await Promise.all(refreshRequests)
  }, [
    attributeOverviewData,
    dashboardData,
    refreshAttributeOverview,
    refreshDashboard,
  ])

  return (
    <WorkspaceDataContext.Provider
      value={{
        dashboardResource,
        attributeOverviewResource,
        habitsResource,
        setHabitCompletionStatus,
        refreshProgress,
      }}
    >
      {children}
    </WorkspaceDataContext.Provider>
  )
}
