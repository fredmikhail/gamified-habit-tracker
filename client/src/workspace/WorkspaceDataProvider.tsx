import { useCallback, type PropsWithChildren } from 'react'
import { getAttributes } from '../api/attributesApi'
import { getDashboard } from '../api/dashboardApi'
import { getHabits } from '../api/habitsApi'
import { WorkspaceDataContext } from './WorkspaceDataContext'
import { useCachedResource } from './useCachedResource'

function getDashboardErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown dashboard-loading error occurred.'
}

function getAttributeErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown attribute-loading error occurred.'
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

  const attributesResource = useCachedResource({
    load: getAttributes,
    getErrorMessage: getAttributeErrorMessage,
  })

  const habitsResource = useCachedResource({
    load: getHabits,
    getErrorMessage: getHabitErrorMessage,
  })

  const { data: dashboardData, refresh: refreshDashboard } = dashboardResource

  const { data: attributesData, refresh: refreshAttributes } =
    attributesResource

  const refreshProgress = useCallback(async (): Promise<void> => {
    const refreshRequests: Promise<void>[] = []

    if (dashboardData !== null) {
      refreshRequests.push(refreshDashboard())
    }

    if (attributesData !== null) {
      refreshRequests.push(refreshAttributes())
    }

    await Promise.all(refreshRequests)
  }, [attributesData, dashboardData, refreshAttributes, refreshDashboard])

  return (
    <WorkspaceDataContext.Provider
      value={{
        dashboardResource,
        attributesResource,
        habitsResource,
        refreshProgress,
      }}
    >
      {children}
    </WorkspaceDataContext.Provider>
  )
}
