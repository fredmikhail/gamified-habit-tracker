import type { HabitAttributeRewardResponse } from './HabitAttributeRewardResponse'
import type { HabitCategory } from './HabitCategory'
import type { HabitDifficulty } from './HabitDifficulty'
import type { HabitFrequencyType } from './HabitFrequencyType'

export type DashboardHabitResponse = {
  id: string
  name: string
  category: HabitCategory
  frequencyType: HabitFrequencyType
  targetCount: number
  difficulty: HabitDifficulty
  attributeRewards: HabitAttributeRewardResponse[]
  isCompletedToday: boolean
  currentStreak: number
  longestStreak: number
}
