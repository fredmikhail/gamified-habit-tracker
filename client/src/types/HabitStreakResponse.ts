import type { HabitFrequencyType } from './HabitFrequencyType'

export type HabitStreakResponse = {
  habitId: string
  habitName: string
  frequencyType: HabitFrequencyType
  currentStreak: number
  longestStreak: number
}
