import type { HabitCategory } from './HabitCategory'
import type { HabitDifficulty } from './HabitDifficulty'
import type { HabitFrequencyType } from './HabitFrequencyType'

export type CreateHabitRequest = {
  name: string
  description: string | null
  category: HabitCategory
  frequencyType: HabitFrequencyType
  targetCount: number
  difficulty: HabitDifficulty
}
