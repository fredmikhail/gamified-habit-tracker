import { createContext } from 'react'
import type { AttributeOverviewResponse } from '../types/AttributeOverviewResponse'
import type { DashboardResponse } from '../types/DashboardResponse'
import type { HabitResponse } from '../types/HabitResponse'
import type { CachedResource } from './useCachedResource'

export type WorkspaceDataContextValue = {
  dashboardResource: CachedResource<DashboardResponse>
  attributeOverviewResource: CachedResource<AttributeOverviewResponse>
  habitsResource: CachedResource<HabitResponse[]>
  setHabitCompletionStatus: (habitId: string, isCompletedToday: boolean) => void
  refreshProgress: () => Promise<void>
}

export const WorkspaceDataContext = createContext<
  WorkspaceDataContextValue | undefined
>(undefined)
