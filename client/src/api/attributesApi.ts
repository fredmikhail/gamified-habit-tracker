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
