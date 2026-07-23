import {
  Check,
  CircleGauge,
  Flame,
  Shield,
  Sparkles,
  Target,
  Zap,
  type LucideIcon,
} from 'lucide-react'
import type { CSSProperties, ReactNode } from 'react'
import type { DashboardResponse } from '../../types/DashboardResponse'
import type { HabitStreakResponse } from '../../types/HabitStreakResponse'

type DashboardSummaryRailProps = {
  dashboard: DashboardResponse
}

type ProgressionMetricCardProps = {
  label: string
  Icon: LucideIcon
  accentVariable: string
  className?: string
  status?: string
  children: ReactNode
}

function getPercentage(current: number, total: number): number {
  if (total <= 0) {
    return 0
  }

  return Math.min(100, Math.max(0, (current / total) * 100))
}

function getFeaturedStreak(
  dashboard: DashboardResponse,
): HabitStreakResponse | null {
  return (
    [...dashboard.habitStreaks].sort(
      (first, second) =>
        second.currentStreak - first.currentStreak ||
        second.longestStreak - first.longestStreak ||
        first.habitName.localeCompare(second.habitName),
    )[0] ?? null
  )
}

function getStreakUnit(streak: HabitStreakResponse, value: number): string {
  const singularUnit = streak.frequencyType === 'daily' ? 'day' : 'week'

  return value === 1 ? singularUnit : `${singularUnit}s`
}

function ProgressionMetricCard({
  label,
  Icon,
  accentVariable,
  className = '',
  status,
  children,
}: ProgressionMetricCardProps) {
  const style = {
    '--metric-accent': accentVariable,
    borderColor:
      'color-mix(in srgb, var(--metric-accent) 34%, var(--theme-border))',
    backgroundImage:
      'radial-gradient(circle at 12% 0%, color-mix(in srgb, var(--metric-accent) 17%, transparent), transparent 48%), linear-gradient(145deg, color-mix(in srgb, var(--metric-accent) 8%, transparent), transparent 62%)',
  } as CSSProperties

  return (
    <article
      aria-label={label}
      className={[
        'ui-adaptive-panel relative min-h-0 overflow-hidden rounded-2xl border bg-surface-raised p-[clamp(0.7rem,0.62rem_+_0.08vw,0.9rem)] shadow-[var(--theme-panel-shadow)]',
        className,
      ].join(' ')}
      style={style}
    >
      <span
        aria-hidden="true"
        className="pointer-events-none absolute inset-x-4 top-0 h-px"
        style={{
          background:
            'linear-gradient(90deg, transparent, var(--metric-accent), transparent)',
          boxShadow:
            '0 0 16px color-mix(in srgb, var(--metric-accent) 36%, transparent)',
        }}
      />

      <Icon
        aria-hidden="true"
        className="pointer-events-none absolute -right-3 -bottom-5 opacity-[0.055]"
        strokeWidth={1.35}
        style={{
          width: 'clamp(4.5rem, 19cqi, 7rem)',
          height: 'clamp(4.5rem, 19cqi, 7rem)',
          color: 'var(--metric-accent)',
        }}
      />

      <div className="relative flex h-full min-h-0 flex-col">
        <div className="flex min-w-0 items-center justify-between gap-2">
          <div className="flex min-w-0 items-center gap-2">
            <span
              className="grid size-[clamp(1.85rem,8cqi,2.35rem)] shrink-0 place-items-center rounded-xl border"
              style={{
                borderColor:
                  'color-mix(in srgb, var(--metric-accent) 45%, transparent)',
                backgroundColor:
                  'color-mix(in srgb, var(--metric-accent) 13%, transparent)',
                color: 'var(--metric-accent)',
                boxShadow:
                  '0 0 16px color-mix(in srgb, var(--metric-accent) 12%, transparent)',
              }}
            >
              <Icon
                aria-hidden="true"
                strokeWidth={1.9}
                style={{
                  width: 'clamp(0.8rem,3.4cqi,1.05rem)',
                  height: 'clamp(0.8rem,3.4cqi,1.05rem)',
                }}
              />
            </span>

            <p className="truncate text-[clamp(0.56rem,2.35cqi,0.72rem)] font-bold tracking-[0.13em] text-content-subtle uppercase">
              {label}
            </p>
          </div>

          {status && (
            <span
              className="shrink-0 rounded-full border px-2 py-0.5 text-[clamp(0.48rem,1.9cqi,0.6rem)] font-bold tracking-[0.08em] uppercase"
              style={{
                borderColor:
                  'color-mix(in srgb, var(--metric-accent) 35%, transparent)',
                backgroundColor:
                  'color-mix(in srgb, var(--metric-accent) 10%, transparent)',
                color: 'var(--metric-accent)',
              }}
            >
              {status}
            </span>
          )}
        </div>

        <div className="mt-[clamp(0.45rem,1.6cqh,0.7rem)] min-h-0 flex-1">
          {children}
        </div>
      </div>
    </article>
  )
}

export function DashboardSummaryRail({ dashboard }: DashboardSummaryRailProps) {
  const completedDailyHabits = dashboard.todayExecution.completedDailyHabits

  const totalDailyHabits = dashboard.todayExecution.totalDailyHabits

  const remainingDailyHabits = Math.max(
    0,
    totalDailyHabits - completedDailyHabits,
  )

  const executionPercentage = getPercentage(
    completedDailyHabits,
    totalDailyHabits,
  )

  const levelPercentage = getPercentage(
    dashboard.overallProgress.xpIntoCurrentLevel,
    dashboard.overallProgress.xpNeededForNextLevel,
  )

  const xpRemaining = Math.max(
    0,
    dashboard.overallProgress.xpNeededForNextLevel -
      dashboard.overallProgress.xpIntoCurrentLevel,
  )

  const nextLevel = dashboard.overallProgress.level + 1
  const featuredStreak = getFeaturedStreak(dashboard)

  return (
    <div
      className="grid h-full min-h-0 gap-2.5 sm:grid-cols-2 xl:grid-cols-[1.18fr_1.02fr_0.88fr_0.82fr_1.28fr]"
      data-testid="dashboard-summary-rail"
    >
      <ProgressionMetricCard
        Icon={CircleGauge}
        accentVariable="var(--theme-success)"
        className="xl:shadow-[var(--theme-panel-shadow),0_0_24px_color-mix(in_srgb,var(--theme-success)_8%,transparent)]"
        label="Today's summary"
        status={`${completedDailyHabits}/${totalDailyHabits}`}
      >
        <div className="flex h-full min-h-0 items-center gap-[clamp(0.6rem,4cqi,1rem)]">
          <div
            aria-label={`${Math.round(
              executionPercentage,
            )}% of daily habits completed`}
            className="grid size-[clamp(3.15rem,20cqi,4.3rem)] shrink-0 place-items-center rounded-full p-[clamp(0.18rem,1.1cqi,0.28rem)]"
            role="img"
            style={{
              background: `conic-gradient(var(--theme-success) ${executionPercentage}%, var(--theme-surface-muted) 0)`,
              boxShadow:
                '0 0 22px color-mix(in srgb, var(--theme-success) 18%, transparent)',
            }}
          >
            <div className="grid size-full place-items-center rounded-full border border-success/15 bg-surface-raised">
              {executionPercentage === 100 && totalDailyHabits > 0 ? (
                <Check
                  aria-hidden="true"
                  className="text-success"
                  size={21}
                  strokeWidth={2.3}
                />
              ) : (
                <span className="text-[clamp(0.85rem,4.1cqi,1.15rem)] font-black">
                  {Math.round(executionPercentage)}%
                </span>
              )}
            </div>
          </div>

          <div className="min-w-0">
            <p className="text-[clamp(1.25rem,7cqi,1.9rem)] leading-none font-black tracking-tight">
              {completedDailyHabits}
              <span className="ml-1 text-[clamp(0.62rem,2.6cqi,0.82rem)] font-semibold text-content-muted">
                of {totalDailyHabits}
              </span>
            </p>

            <p className="mt-1.5 truncate text-[clamp(0.56rem,2.3cqi,0.7rem)] text-content-muted">
              {remainingDailyHabits === 0 && totalDailyHabits > 0
                ? 'Daily targets secured'
                : `${remainingDailyHabits} ${
                    remainingDailyHabits === 1 ? 'habit' : 'habits'
                  } remaining`}
            </p>
          </div>
        </div>
      </ProgressionMetricCard>

      <ProgressionMetricCard
        Icon={Flame}
        accentVariable="var(--theme-streak)"
        label="Current streak"
        status={
          featuredStreak ? `Best ${featuredStreak.longestStreak}` : 'Unstarted'
        }
      >
        <div className="flex h-full min-h-0 flex-col justify-end">
          <p className="text-[clamp(1.4rem,8cqi,2.2rem)] leading-none font-black tracking-tight text-streak">
            {featuredStreak ? featuredStreak.currentStreak : 0}
            <span className="ml-1.5 text-[clamp(0.62rem,2.8cqi,0.82rem)] font-semibold text-content-muted">
              {featuredStreak
                ? getStreakUnit(featuredStreak, featuredStreak.currentStreak)
                : 'days'}
            </span>
          </p>

          <p className="mt-1.5 truncate text-[clamp(0.56rem,2.3cqi,0.7rem)] text-content-muted">
            {featuredStreak
              ? featuredStreak.habitName
              : 'Complete habits to establish momentum'}
          </p>

          <div className="mt-auto pt-2">
            <div className="h-1.5 overflow-hidden rounded-full bg-surface-muted">
              <div
                className="h-full rounded-full bg-streak"
                style={{
                  width: featuredStreak
                    ? `${getPercentage(
                        featuredStreak.currentStreak,
                        Math.max(featuredStreak.longestStreak, 1),
                      )}%`
                    : '0%',
                  boxShadow:
                    '0 0 12px color-mix(in srgb, var(--theme-streak) 42%, transparent)',
                }}
              />
            </div>
          </div>
        </div>
      </ProgressionMetricCard>

      <ProgressionMetricCard
        Icon={Zap}
        accentVariable="var(--theme-energy-violet)"
        label="XP today"
        status={`${dashboard.todayActivity.completions} done`}
      >
        <div className="flex h-full min-h-0 flex-col justify-end">
          <p className="text-[clamp(1.5rem,9cqi,2.35rem)] leading-none font-black tracking-tight text-energy-violet">
            +{dashboard.todayActivity.xpEarned.toLocaleString()}
          </p>

          <p className="mt-1.5 text-[clamp(0.56rem,2.5cqi,0.7rem)] text-content-muted">
            XP earned today
          </p>

          <div className="mt-auto flex items-center gap-1.5 pt-2 text-[clamp(0.5rem,2.1cqi,0.62rem)] font-semibold text-success">
            <Sparkles aria-hidden="true" size={11} />
            <span>
              {dashboard.todayActivity.completions}{' '}
              {dashboard.todayActivity.completions === 1
                ? 'completion'
                : 'completions'}
            </span>
          </div>
        </div>
      </ProgressionMetricCard>

      <ProgressionMetricCard
        Icon={Shield}
        accentVariable="var(--theme-energy-cyan)"
        label="Total level"
        status="Lifetime"
      >
        <div className="flex h-full min-h-0 items-end gap-[clamp(0.45rem,3cqi,0.75rem)]">
          <div
            className="grid aspect-square w-[clamp(3.25rem,24cqi,4.4rem)] shrink-0 place-items-center border border-energy-cyan/35 bg-energy-cyan/10 text-energy-cyan"
            style={{
              clipPath:
                'polygon(50% 0%, 92% 20%, 84% 76%, 50% 100%, 16% 76%, 8% 20%)',
              boxShadow:
                '0 0 20px color-mix(in srgb, var(--theme-energy-cyan) 16%, transparent)',
            }}
          >
            <span className="text-[clamp(1.35rem,9cqi,2.1rem)] leading-none font-black">
              {dashboard.overallProgress.level}
            </span>
          </div>

          <div className="min-w-0 pb-0.5">
            <p className="text-[clamp(0.5rem,2.2cqi,0.65rem)] font-bold tracking-[0.11em] text-content-subtle uppercase">
              Character level
            </p>

            <p className="mt-1 truncate text-[clamp(0.58rem,2.4cqi,0.72rem)] font-semibold text-content-muted">
              {dashboard.overallProgress.totalXp.toLocaleString()} total XP
            </p>
          </div>
        </div>
      </ProgressionMetricCard>

      <ProgressionMetricCard
        Icon={Target}
        accentVariable="var(--theme-accent-primary)"
        label="Next level"
        status={`${Math.round(levelPercentage)}%`}
      >
        <div className="flex h-full min-h-0 flex-col">
          <div className="flex items-end justify-between gap-3">
            <div>
              <p className="text-[clamp(0.52rem,2.1cqi,0.65rem)] font-bold tracking-[0.1em] text-content-subtle uppercase">
                Level {dashboard.overallProgress.level} → {nextLevel}
              </p>

              <p className="mt-1 text-[clamp(0.82rem,3.7cqi,1.1rem)] font-bold">
                {xpRemaining.toLocaleString()}{' '}
                <span className="text-[clamp(0.52rem,2.1cqi,0.65rem)] font-semibold text-content-muted">
                  XP remaining
                </span>
              </p>
            </div>

            <span className="shrink-0 text-[clamp(0.54rem,2.2cqi,0.68rem)] font-semibold text-accent">
              {dashboard.overallProgress.xpIntoCurrentLevel.toLocaleString()}
              {' / '}
              {dashboard.overallProgress.xpNeededForNextLevel.toLocaleString()}
            </span>
          </div>

          <div className="mt-auto pt-2.5">
            <div
              aria-label="Overall level progress"
              aria-valuemax={dashboard.overallProgress.xpNeededForNextLevel}
              aria-valuemin={0}
              aria-valuenow={dashboard.overallProgress.xpIntoCurrentLevel}
              className="relative h-2.5 overflow-hidden rounded-full border border-accent/15 bg-surface-muted"
              role="progressbar"
            >
              <div
                className="h-full rounded-full"
                style={{
                  width: `${levelPercentage}%`,
                  background: 'var(--theme-progress-gradient)',
                  boxShadow:
                    '0 0 16px color-mix(in srgb, var(--theme-accent-primary) 48%, transparent)',
                }}
              />
            </div>
          </div>
        </div>
      </ProgressionMetricCard>
    </div>
  )
}
