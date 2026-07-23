import type { AttributeOverviewResponse } from '../types/AttributeOverviewResponse'
import type { UserAttributeResponse } from '../types/UserAttributeResponse'
import { apiRequest } from './apiClient'
import { readApiError } from './readApiError'

export async function getAttributes(): Promise<UserAttributeResponse[]> {
  const response = await apiRequest('/api/attributes')

  if (!response.ok) {
    const errorMessage = await readApiError(
      response,
      `Attribute request failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  const attributes: UserAttributeResponse[] = await response.json()

  return attributes
}

export async function getAttributeOverview(): Promise<AttributeOverviewResponse> {
  const response = await apiRequest('/api/attributes/overview')

  if (!response.ok) {
    const errorMessage = await readApiError(
      response,
      `Attribute overview request failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  const overview: AttributeOverviewResponse = await response.json()

  return overview
}
