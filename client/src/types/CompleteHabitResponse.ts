import type { HabitAttributeRewardResponse } from './HabitAttributeRewardResponse'
import type { HabitCompletionResponse } from './HabitCompletionResponse'

export type CompleteHabitResponse = {
  completion: HabitCompletionResponse
  rewards: HabitAttributeRewardResponse[]
}
