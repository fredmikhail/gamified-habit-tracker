import { useEffect, useState } from 'react'
import { getDashboard } from '../../api/dashboardApi'
import type { DashboardResponse } from '../../types/DashboardResponse'
import { HabitStreakSection } from './HabitStreakSection'

type OverallProgressSectionProps = {
  refreshKey: number
}

function getDashboardErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown dashboard-loading error occurred.'
}

function getProgressPercentage(dashboard: DashboardResponse): number {
  const progress = dashboard.overallProgress

  if (progress.xpNeededForNextLevel <= 0) {
    return 0
  }

  return Math.min(
    100,
    Math.max(
      0,
      (progress.xpIntoCurrentLevel / progress.xpNeededForNextLevel) * 100,
    ),
  )
}

function getExecutionPercentage(dashboard: DashboardResponse): number {
  const execution = dashboard.todayExecution

  if (execution.totalDailyHabits <= 0) {
    return 0
  }

  return Math.min(
    100,
    Math.max(
      0,
      (execution.completedDailyHabits / execution.totalDailyHabits) * 100,
    ),
  )
}

export function OverallProgressSection({
  refreshKey,
}: OverallProgressSectionProps) {
  const [dashboard, setDashboard] = useState<DashboardResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    let isActive = true

    async function loadDashboard() {
      setIsLoading(true)

      try {
        const loadedDashboard = await getDashboard()

        if (isActive) {
          setDashboard(loadedDashboard)
          setErrorMessage(null)
        }
      } catch (error) {
        if (isActive) {
          setDashboard(null)
          setErrorMessage(getDashboardErrorMessage(error))
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadDashboard()

    return () => {
      isActive = false
    }
  }, [refreshKey])

  const progressPercentage = dashboard ? getProgressPercentage(dashboard) : 0

  const executionPercentage = dashboard ? getExecutionPercentage(dashboard) : 0

  return (
    <section
      aria-labelledby="overall-progress-heading"
      className="mt-8 rounded-lg bg-white p-6 text-left shadow-sm"
    >
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 id="overall-progress-heading" className="text-2xl font-bold">
            Overall Progress
          </h2>

          <p className="mt-2 text-slate-600">
            Every completed habit contributes to your overall level.
          </p>
        </div>

        {!isLoading && dashboard && (
          <span className="rounded bg-slate-900 px-4 py-2 font-bold text-white">
            Level {dashboard.overallProgress.level}
          </span>
        )}
      </div>

      {isLoading && (
        <p className="mt-4 text-slate-600">Loading overall progress...</p>
      )}

      {!isLoading && errorMessage && (
        <p className="mt-4 text-red-700" role="alert">
          Dashboard loading error: {errorMessage}
        </p>
      )}

      {!isLoading && dashboard && !errorMessage && (
        <>
          <div className="mt-6 flex flex-wrap items-end justify-between gap-4">
            <div>
              <p className="text-sm font-medium text-slate-600">Total XP</p>

              <p className="text-3xl font-bold">
                {dashboard.overallProgress.totalXp}
              </p>
            </div>

            <p className="text-sm text-slate-600">
              {dashboard.overallProgress.xpIntoCurrentLevel} /{' '}
              {dashboard.overallProgress.xpNeededForNextLevel} XP
            </p>
          </div>

          <div
            aria-label="Overall level progress"
            aria-valuemax={dashboard.overallProgress.xpNeededForNextLevel}
            aria-valuemin={0}
            aria-valuenow={dashboard.overallProgress.xpIntoCurrentLevel}
            className="mt-4 h-4 overflow-hidden rounded-full bg-slate-200"
            role="progressbar"
          >
            <div
              className="h-full rounded-full bg-slate-900"
              style={{
                width: `${progressPercentage}%`,
              }}
            />
          </div>

          <div className="mt-6 grid gap-4 sm:grid-cols-2">
            <article className="rounded-lg border border-slate-200 p-4">
              <p className="text-sm font-medium text-slate-600">
                Today's activity
              </p>

              <p className="mt-2 text-2xl font-bold">
                {dashboard.todayActivity.completions}{' '}
                {dashboard.todayActivity.completions === 1
                  ? 'completion'
                  : 'completions'}
              </p>

              <p className="mt-1 text-sm text-slate-600">
                {dashboard.todayActivity.xpEarned} XP earned
              </p>

              <p className="mt-3 text-xs text-slate-500">
                Local date: {dashboard.todayActivity.localDate}
              </p>
            </article>

            <article className="rounded-lg border border-slate-200 p-4">
              <p className="text-sm font-medium text-slate-600">
                Daily execution
              </p>

              {dashboard.todayExecution.totalDailyHabits === 0 ? (
                <p className="mt-2 text-slate-600">No active daily habits.</p>
              ) : (
                <>
                  <p className="mt-2 text-2xl font-bold">
                    {dashboard.todayExecution.completedDailyHabits} of{' '}
                    {dashboard.todayExecution.totalDailyHabits}
                  </p>

                  <p className="mt-1 text-sm text-slate-600">
                    active daily habits completed
                  </p>

                  <div
                    aria-label="Daily execution progress"
                    aria-valuemax={dashboard.todayExecution.totalDailyHabits}
                    aria-valuemin={0}
                    aria-valuenow={
                      dashboard.todayExecution.completedDailyHabits
                    }
                    className="mt-4 h-3 overflow-hidden rounded-full bg-slate-200"
                    role="progressbar"
                  >
                    <div
                      className="h-full rounded-full bg-slate-700"
                      style={{
                        width: `${executionPercentage}%`,
                      }}
                    />
                  </div>
                </>
              )}
            </article>
          </div>

          <HabitStreakSection habitStreaks={dashboard.habitStreaks} />
        </>
      )}
    </section>
  )
}
