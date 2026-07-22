import type { DashboardResponse } from '../types/DashboardResponse'
import { apiRequest } from './apiClient'
import { readApiError } from './readApiError'

export async function getDashboard(): Promise<DashboardResponse> {
  const response = await apiRequest('/api/dashboard')

  if (!response.ok) {
    const errorMessage = await readApiError(
      response,
      `Dashboard request failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  const dashboard: DashboardResponse = await response.json()

  return dashboard
}
