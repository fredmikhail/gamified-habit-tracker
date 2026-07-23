import type { DashboardHabitResponse } from './DashboardHabitResponse'
import type { HabitStreakResponse } from './HabitStreakResponse'
import type { OverallProgressResponse } from './OverallProgressResponse'
import type { TodayActivityResponse } from './TodayActivityResponse'
import type { TodayExecutionResponse } from './TodayExecutionResponse'
import type { UserAttributeResponse } from './UserAttributeResponse'

export type DashboardResponse = {
  overallProgress: OverallProgressResponse
  todayActivity: TodayActivityResponse
  todayExecution: TodayExecutionResponse
  todayHabits: DashboardHabitResponse[]
  attributes: UserAttributeResponse[]
  habitStreaks: HabitStreakResponse[]
}
