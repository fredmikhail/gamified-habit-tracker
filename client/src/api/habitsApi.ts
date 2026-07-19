import type { CreateHabitRequest } from '../types/CreateHabitRequest'
import type { UpdateHabitRequest } from '../types/UpdateHabitRequest'
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

export async function createHabit(
  request: CreateHabitRequest,
): Promise<HabitResponse> {
  const response = await apiRequest('/api/habits', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const errorMessage = await readApiError(
      response,
      `Habit creation failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  const habit: HabitResponse = await response.json()

  return habit
}

export async function updateHabit(
  habitId: string,
  request: UpdateHabitRequest,
): Promise<HabitResponse> {
  const response = await apiRequest(`/api/habits/${habitId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const errorMessage = await readApiError(
      response,
      `Habit update failed with status ${response.status}.`,
    )

    throw new Error(errorMessage)
  }

  const habit: HabitResponse = await response.json()

  return habit
}
