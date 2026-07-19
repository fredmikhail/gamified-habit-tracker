import type { HabitResponse } from '../types/HabitResponse'
import { apiRequest } from './apiClient'
import { readApiError } from './readApiError'

export async function getHabits(
  includeInactive = false,
): Promise<HabitResponse[]> {
  const queryString = includeInactive ? '?includeInactive=true' : ''

  const response = await apiRequest(`/api/habits${queryString}`)

  if (!response.ok) {
    const errorMessage = await readApiError(
      response,
      `Habit request failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  const habits: HabitResponse[] = await response.json()

  return habits
}