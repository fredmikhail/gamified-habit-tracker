import type { HabitCategory } from './HabitCategory'
import type { HabitDifficulty } from './HabitDifficulty'
import type { HabitFrequencyType } from './HabitFrequencyType'

export type PendingHabitConfigurationResponse = {
  effectiveFromDate: string
  category: HabitCategory
  frequencyType: HabitFrequencyType
  targetCount: number
  difficulty: HabitDifficulty
}
