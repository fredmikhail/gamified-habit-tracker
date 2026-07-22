import type { HabitCategory } from './HabitCategory'
import type { HabitDifficulty } from './HabitDifficulty'
import type { HabitFrequencyType } from './HabitFrequencyType'
import type { PendingHabitConfigurationResponse } from './PendingHabitConfigurationResponse'

export type HabitResponse = {
  id: string
  name: string
  description: string | null
  category: HabitCategory
  frequencyType: HabitFrequencyType
  targetCount: number
  difficulty: HabitDifficulty
  pendingConfiguration?: PendingHabitConfigurationResponse | null
  isActive: boolean
  isCompletedToday: boolean
  createdAtUtc: string
  updatedAtUtc: string
}
