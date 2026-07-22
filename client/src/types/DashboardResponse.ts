import type { OverallProgressResponse } from './OverallProgressResponse'
import type { TodayActivityResponse } from './TodayActivityResponse'
import type { TodayExecutionResponse } from './TodayExecutionResponse'

export type DashboardResponse = {
  overallProgress: OverallProgressResponse
  todayActivity: TodayActivityResponse
  todayExecution: TodayExecutionResponse
}
