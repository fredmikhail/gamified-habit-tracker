import { createContext } from 'react'
import type { DashboardResponse } from '../types/DashboardResponse'
import type { HabitResponse } from '../types/HabitResponse'
import type { UserAttributeResponse } from '../types/UserAttributeResponse'
import type { CachedResource } from './useCachedResource'

export type WorkspaceDataContextValue = {
  dashboardResource: CachedResource<DashboardResponse>
  attributesResource: CachedResource<UserAttributeResponse[]>
  habitsResource: CachedResource<HabitResponse[]>
  refreshProgress: () => Promise<void>
}

export const WorkspaceDataContext = createContext<
  WorkspaceDataContextValue | undefined
>(undefined)
