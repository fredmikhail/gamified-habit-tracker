import { apiRequest } from './apiClient'
import type { HealthResponse } from '../types/HealthResponse'

export async function getHealth(): Promise<HealthResponse> {
  const response = await apiRequest('/api/health')

  if (!response.ok) {
    throw new Error(`Health request failed with status ${response.status}.`)
  }

  const healthResponse: HealthResponse = await response.json()

  return healthResponse
}
