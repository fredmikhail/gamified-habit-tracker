import type { HabitDifficulty } from './HabitDifficulty'
import type { HabitFrequencyType } from './HabitFrequencyType'

export type HabitResponse = {
  id: string
  name: string
  description: string | null
  category: string | null
  frequencyType: HabitFrequencyType
  targetCount: number
  difficulty: HabitDifficulty
  isActive: boolean
  isCompletedToday: boolean
  createdAtUtc: string
  updatedAtUtc: string
}
