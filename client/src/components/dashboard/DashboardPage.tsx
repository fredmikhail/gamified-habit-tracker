import {
  Activity,
  CheckCircle2,
  CircleGauge,
  Flame,
  PieChart,
  Shield,
  Sparkles,
  Target,
  Zap,
  type LucideIcon,
} from 'lucide-react'
import { useEffect, useState, type CSSProperties } from 'react'
import type { DashboardHabitResponse } from '../../types/DashboardHabitResponse'
import type { DashboardResponse } from '../../types/DashboardResponse'
import type { HabitStreakResponse } from '../../types/HabitStreakResponse'
import { useWorkspaceData } from '../../workspace/useWorkspaceData'
import { AttributeCard } from '../attributes/AttributeCard'
import {
  attributeOrder,
  getAttributeVisual,
} from '../attributes/attributeVisuals'
import { getHabitCategoryLabel } from '../habits/habitCategoryOptions'
import { CommandPanel } from '../ui/CommandPanel'
import { MetricPanel } from '../ui/MetricPanel'
import { DashboardHabitAction } from './DashboardHabitAction'

type MobilePanel = 'summary' | 'today' | 'attributes'

type PageControlsProps = {
  label: string
  pageIndex: number
  pageCount: number
  onPageChange: (pageIndex: number) => void
}

const habitPageSize = 4

function getPercentage(current: number, total: number): number {
  if (total <= 0) {
    return 0
  }

  return Math.min(100, Math.max(0, (current / total) * 100))
}

function getPageCount(itemCount: number): number {
  return Math.max(1, Math.ceil(itemCount / habitPageSize))
}

function getPageItems<T>(items: T[], pageIndex: number): T[] {
  return items.slice(
    pageIndex * habitPageSize,
    pageIndex * habitPageSize + habitPageSize,
  )
}

function formatDifficulty(
  difficulty: DashboardHabitResponse['difficulty'],
): string {
  return difficulty.charAt(0).toUpperCase() + difficulty.slice(1)
}

function formatFrequency(habit: DashboardHabitResponse): string {
  return habit.frequencyType === 'daily'
    ? 'Daily'
    : `${habit.targetCount}× weekly`
}

function formatStreak(habitStreak: HabitStreakResponse): string {
  const unit = habitStreak.frequencyType === 'daily' ? 'day' : 'week'

  return `${habitStreak.currentStreak} ${
    habitStreak.currentStreak === 1 ? unit : `${unit}s`
  }`
}

function getRewardTotal(habit: DashboardHabitResponse): number {
  return habit.attributeRewards.reduce(
    (total, reward) => total + reward.xpAmount,
    0,
  )
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

function getOrderedAttributes(
  dashboard: DashboardResponse,
): DashboardResponse['attributes'] {
  return [...dashboard.attributes].sort(
    (first, second) =>
      attributeOrder.indexOf(first.attributeType) -
      attributeOrder.indexOf(second.attributeType),
  )
}

function getAttributeXpDistributionGradient(
  attributes: DashboardResponse['attributes'],
): string {
  const totalXp = attributes.reduce(
    (total, attribute) => total + attribute.currentXp,
    0,
  )

  if (totalXp <= 0) {
    return 'conic-gradient(var(--theme-surface-muted) 0% 100%)'
  }

  let currentPercentage = 0

  const gradientSegments = attributes.map((attribute, index) => {
    const startPercentage = currentPercentage

    const attributePercentage = (attribute.currentXp / totalXp) * 100

    currentPercentage =
      index === attributes.length - 1
        ? 100
        : currentPercentage + attributePercentage

    const visual = getAttributeVisual(attribute.attributeType)

    return `${visual.colorVariable} ${startPercentage}% ${currentPercentage}%`
  })

  return `conic-gradient(${gradientSegments.join(', ')})`
}

function getMobilePanelClass(
  panel: MobilePanel,
  activePanel: MobilePanel,
): string {
  return panel === activePanel ? 'min-h-0' : 'hidden min-h-0 xl:block'
}

function PageControls({
  label,
  pageIndex,
  pageCount,
  onPageChange,
}: PageControlsProps) {
  if (pageCount <= 1) {
    return null
  }

  return (
    <div className="flex items-center gap-2">
      <button
        aria-label={`Previous ${label} page`}
        className="grid size-7 place-items-center rounded-lg border border-line bg-surface text-xs text-content-muted disabled:opacity-35"
        disabled={pageIndex === 0}
        type="button"
        onClick={() => onPageChange(pageIndex - 1)}
      >
        ‹
      </button>

      <span className="text-[9px] font-semibold text-content-subtle">
        {pageIndex + 1} / {pageCount}
      </span>

      <button
        aria-label={`Next ${label} page`}
        className="grid size-7 place-items-center rounded-lg border border-line bg-surface text-xs text-content-muted disabled:opacity-35"
        disabled={pageIndex === pageCount - 1}
        type="button"
        onClick={() => onPageChange(pageIndex + 1)}
      >
        ›
      </button>
    </div>
  )
}

function SummaryMetrics({ dashboard }: { dashboard: DashboardResponse }) {
  const executionPercentage = getPercentage(
    dashboard.todayExecution.completedDailyHabits,
    dashboard.todayExecution.totalDailyHabits,
  )

  const levelPercentage = getPercentage(
    dashboard.overallProgress.xpIntoCurrentLevel,
    dashboard.overallProgress.xpNeededForNextLevel,
  )

  const featuredStreak = getFeaturedStreak(dashboard)

  return (
    <div className="grid h-full min-h-0 gap-2.5 sm:grid-cols-2 xl:grid-cols-[1.15fr_1fr_0.9fr_0.8fr_1.35fr]">
      <MetricPanel
        Icon={CircleGauge}
        accentVariable="var(--theme-success)"
        label="Today's summary"
      >
        <div className="flex items-center gap-3">
          <div
            aria-label={`${Math.round(
              executionPercentage,
            )}% of daily habits completed`}
            className="grid size-14 shrink-0 place-items-center rounded-full p-1"
            role="img"
            style={{
              background: `conic-gradient(var(--theme-success) ${executionPercentage}%, var(--theme-surface-muted) 0)`,
            }}
          >
            <div className="grid size-full place-items-center rounded-full bg-surface-raised">
              <span className="text-sm font-bold">
                {Math.round(executionPercentage)}%
              </span>
            </div>
          </div>

          <div>
            <p className="text-xl font-bold">
              {dashboard.todayExecution.completedDailyHabits}{' '}
              <span className="text-xs font-medium text-content-muted">
                of {dashboard.todayExecution.totalDailyHabits}
              </span>
            </p>

            <p className="mt-1 text-[10px] text-content-subtle">
              daily habits completed
            </p>
          </div>
        </div>
      </MetricPanel>

      <MetricPanel
        Icon={Flame}
        accentVariable="var(--theme-streak)"
        label="Current streak"
      >
        <p className="text-2xl font-bold">
          {featuredStreak ? formatStreak(featuredStreak) : '0 days'}
        </p>

        <p className="mt-1 truncate text-[10px] text-content-muted">
          {featuredStreak
            ? featuredStreak.habitName
            : 'Complete habits to begin'}
        </p>
      </MetricPanel>

      <MetricPanel
        Icon={Zap}
        accentVariable="var(--theme-energy-violet)"
        label="XP today"
      >
        <p className="text-2xl font-bold">{dashboard.todayActivity.xpEarned}</p>

        <p className="mt-1 text-[10px] text-success">
          {dashboard.todayActivity.completions} completions
        </p>
      </MetricPanel>

      <MetricPanel
        Icon={Shield}
        accentVariable="var(--theme-energy-cyan)"
        label="Total level"
      >
        <p className="text-3xl font-black">{dashboard.overallProgress.level}</p>

        <p className="mt-1 text-[10px] text-content-muted">
          {dashboard.overallProgress.totalXp} total XP
        </p>
      </MetricPanel>

      <MetricPanel
        Icon={Target}
        accentVariable="var(--theme-accent-primary)"
        label="Next level"
      >
        <div className="flex items-baseline justify-between gap-3">
          <p className="text-base font-bold">
            {dashboard.overallProgress.xpIntoCurrentLevel}
          </p>

          <p className="text-[10px] text-content-muted">
            / {dashboard.overallProgress.xpNeededForNextLevel} XP
          </p>
        </div>

        <div
          aria-label="Overall level progress"
          aria-valuemax={dashboard.overallProgress.xpNeededForNextLevel}
          aria-valuemin={0}
          aria-valuenow={dashboard.overallProgress.xpIntoCurrentLevel}
          className="mt-3 h-2 overflow-hidden rounded-full bg-surface-muted"
          role="progressbar"
        >
          <div
            className="h-full rounded-full"
            style={{
              width: `${levelPercentage}%`,
              background: 'var(--theme-progress-gradient)',
            }}
          />
        </div>

        <p className="mt-1.5 text-right text-[9px] text-content-subtle">
          {Math.round(levelPercentage)}%
        </p>
      </MetricPanel>
    </div>
  )
}

function HabitIcon({ habit }: { habit: DashboardHabitResponse }) {
  const primaryReward = habit.attributeRewards[0]

  const visual = primaryReward
    ? getAttributeVisual(primaryReward.attributeType)
    : null

  const Icon: LucideIcon = visual?.Icon ?? Activity

  const style = {
    '--habit-accent': visual?.colorVariable ?? 'var(--theme-accent-primary)',
  } as CSSProperties

  return (
    <div
      className="grid size-8 shrink-0 place-items-center rounded-lg border"
      style={{
        ...style,
        borderColor: 'color-mix(in srgb, var(--habit-accent) 35%, transparent)',
        backgroundColor:
          'color-mix(in srgb, var(--habit-accent) 12%, transparent)',
        color: 'var(--habit-accent)',
      }}
    >
      <Icon aria-hidden="true" size={15} strokeWidth={1.9} />
    </div>
  )
}

function RecentCompletionsPanel({
  habits,
}: {
  habits: DashboardHabitResponse[]
}) {
  const [pageIndex, setPageIndex] = useState(0)

  const completedHabits = habits.filter((habit) => habit.isCompletedToday)

  const pageCount = getPageCount(completedHabits.length)

  const safePageIndex = Math.min(pageIndex, pageCount - 1)

  const visibleHabits = getPageItems(completedHabits, safePageIndex)

  return (
    <CommandPanel
      Icon={CheckCircle2}
      action={
        <PageControls
          label="completed habits"
          pageCount={pageCount}
          pageIndex={safePageIndex}
          onPageChange={setPageIndex}
        />
      }
      className="h-full"
      eyebrow="Activity"
      title="Completed today"
    >
      {visibleHabits.length === 0 ? (
        <div className="grid h-full place-items-center text-center">
          <div>
            <CheckCircle2
              aria-hidden="true"
              className="mx-auto text-content-subtle"
              size={22}
            />

            <p className="mt-2 text-xs font-semibold">Nothing completed yet</p>

            <p className="mt-1 text-[10px] text-content-muted">
              Your completed habits will appear here.
            </p>
          </div>
        </div>
      ) : (
        <ul className="space-y-1.5">
          {visibleHabits.map((habit) => (
            <li
              className="grid grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-2 rounded-xl border border-line bg-surface px-2.5 py-2"
              key={habit.id}
            >
              <HabitIcon habit={habit} />

              <div className="min-w-0">
                <p className="truncate text-[11px] font-semibold">
                  {habit.name}
                </p>

                <p className="mt-0.5 truncate text-[9px] text-content-subtle">
                  {getHabitCategoryLabel(habit.category)}
                </p>
              </div>

              <div className="text-right">
                <p className="text-[10px] font-semibold text-success">
                  +{getRewardTotal(habit)} XP
                </p>

                <p className="mt-0.5 text-[8px] text-content-subtle">
                  Completed
                </p>
              </div>
            </li>
          ))}
        </ul>
      )}
    </CommandPanel>
  )
}

function TodayHabitsPanel({
  habits,
  onCompletionStatusChanged,
}: {
  habits: DashboardHabitResponse[]
  onCompletionStatusChanged: (
    habitId: string,
    isCompletedToday: boolean,
  ) => void
}) {
  const [pageIndex, setPageIndex] = useState(0)

  const pageCount = getPageCount(habits.length)

  const safePageIndex = Math.min(pageIndex, pageCount - 1)

  const visibleHabits = getPageItems(habits, safePageIndex)

  return (
    <CommandPanel
      Icon={Activity}
      action={
        <PageControls
          label="today's habits"
          pageCount={pageCount}
          pageIndex={safePageIndex}
          onPageChange={setPageIndex}
        />
      }
      className="h-full"
      eyebrow="Primary actions"
      title="Today's habits"
    >
      {visibleHabits.length === 0 ? (
        <div className="grid h-full place-items-center text-center">
          <div>
            <Activity
              aria-hidden="true"
              className="mx-auto text-content-subtle"
              size={23}
            />

            <p className="mt-2 text-xs font-semibold">No active habits</p>

            <p className="mt-1 text-[10px] text-content-muted">
              Create a habit to begin your daily progression.
            </p>
          </div>
        </div>
      ) : (
        <ul className="space-y-1.5">
          {visibleHabits.map((habit) => (
            <li
              className="grid grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-2 rounded-xl border border-line bg-surface px-2.5 py-2"
              key={habit.id}
            >
              <HabitIcon habit={habit} />

              <div className="min-w-0">
                <p className="truncate text-[11px] font-semibold">
                  {habit.name}
                </p>

                <div className="mt-0.5 flex min-w-0 gap-1.5 text-[8px] text-content-subtle">
                  <span>{formatFrequency(habit)}</span>

                  <span aria-hidden="true">·</span>

                  <span>{formatDifficulty(habit.difficulty)}</span>

                  <span aria-hidden="true">·</span>

                  <span className="truncate">{habit.currentStreak} streak</span>
                </div>

                <p className="mt-0.5 text-[8px] text-content-muted">
                  +{getRewardTotal(habit)} XP
                </p>
              </div>

              <DashboardHabitAction
                habit={habit}
                onCompletionStatusChanged={onCompletionStatusChanged}
              />
            </li>
          ))}
        </ul>
      )}
    </CommandPanel>
  )
}

function AttributeOverviewPanel({
  dashboard,
}: {
  dashboard: DashboardResponse
}) {
  const orderedAttributes = getOrderedAttributes(dashboard)

  return (
    <CommandPanel
      Icon={Sparkles}
      bodyClassName="h-full"
      className="h-full"
      eyebrow="Character progression"
      title="Attribute overview"
    >
      <ul className="grid h-full min-h-0 grid-cols-2 grid-rows-4 gap-2 sm:grid-cols-4 sm:grid-rows-2">
        {orderedAttributes.map((attribute) => (
          <li className="min-h-0" key={attribute.attributeType}>
            <AttributeCard attribute={attribute} compact />
          </li>
        ))}
      </ul>
    </CommandPanel>
  )
}

function AttributeXpDistributionPanel({
  dashboard,
}: {
  dashboard: DashboardResponse
}) {
  const orderedAttributes = getOrderedAttributes(dashboard)

  const totalXp = orderedAttributes.reduce(
    (total, attribute) => total + attribute.currentXp,
    0,
  )

  const strongestAttribute =
    [...orderedAttributes].sort(
      (first, second) =>
        second.currentXp - first.currentXp ||
        attributeOrder.indexOf(first.attributeType) -
          attributeOrder.indexOf(second.attributeType),
    )[0] ?? null

  const lowestAttribute =
    [...orderedAttributes].sort(
      (first, second) =>
        first.currentXp - second.currentXp ||
        attributeOrder.indexOf(first.attributeType) -
          attributeOrder.indexOf(second.attributeType),
    )[0] ?? null

  const distributionGradient =
    getAttributeXpDistributionGradient(orderedAttributes)

  return (
    <CommandPanel
      Icon={PieChart}
      bodyClassName="grid min-h-0 grid-cols-[6.5rem_minmax(0,1fr)] items-center gap-3"
      className="h-full"
      eyebrow="Balance"
      title="Attribute XP distribution"
    >
      <div
        aria-label="Attribute XP distribution chart"
        className="relative mx-auto grid size-24 shrink-0 place-items-center rounded-full"
        role="img"
        style={{
          background: distributionGradient,
          boxShadow: 'var(--theme-energy-shadow)',
        }}
      >
        <div className="grid size-[4.5rem] place-items-center rounded-full border border-line bg-surface-raised text-center">
          <div>
            <p className="text-base font-bold">{totalXp.toLocaleString()}</p>

            <p className="text-[8px] font-semibold tracking-[0.12em] text-content-subtle uppercase">
              Total XP
            </p>
          </div>
        </div>
      </div>

      <div className="min-w-0">
        <ul className="grid grid-cols-2 gap-x-3 gap-y-1.5">
          {orderedAttributes.map((attribute) => {
            const visual = getAttributeVisual(attribute.attributeType)

            const percentage =
              totalXp <= 0
                ? 0
                : Math.round((attribute.currentXp / totalXp) * 100)

            return (
              <li
                className="flex min-w-0 items-center justify-between gap-2 text-[10px]"
                key={attribute.attributeType}
              >
                <div className="flex min-w-0 items-center gap-1.5">
                  <span
                    aria-hidden="true"
                    className="size-2 shrink-0 rounded-full"
                    style={{
                      backgroundColor: visual.colorVariable,
                    }}
                  />

                  <span className="truncate text-content-muted">
                    {visual.label}
                  </span>
                </div>

                <span
                  className="shrink-0 font-semibold"
                  style={{
                    color: visual.colorVariable,
                  }}
                >
                  {percentage}%
                </span>
              </li>
            )
          })}
        </ul>

        {strongestAttribute && lowestAttribute && (
          <p className="mt-2 border-t border-line pt-2 text-[9px] leading-4 text-content-subtle">
            Strongest:{' '}
            <span className="font-semibold text-content">
              {getAttributeVisual(strongestAttribute.attributeType).label}
            </span>
            {' · '}
            Lowest:{' '}
            <span className="font-semibold text-content">
              {getAttributeVisual(lowestAttribute.attributeType).label}
            </span>
          </p>
        )}
      </div>
    </CommandPanel>
  )
}

function ProtectTheChainPanel({
  habits,
  onCompletionStatusChanged,
}: {
  habits: DashboardHabitResponse[]
  onCompletionStatusChanged: (
    habitId: string,
    isCompletedToday: boolean,
  ) => void
}) {
  const activeDailyChains = habits.filter(
    (habit) => habit.frequencyType === 'daily' && habit.currentStreak > 0,
  )

  const chainsToProtect = activeDailyChains
    .filter((habit) => !habit.isCompletedToday)
    .sort(
      (first, second) =>
        second.currentStreak - first.currentStreak ||
        second.longestStreak - first.longestStreak ||
        first.name.localeCompare(second.name),
    )

  const visibleChains = chainsToProtect.slice(0, 3)

  const protectedChainCount = activeDailyChains.length - chainsToProtect.length

  let actionLabel = 'No active chains'

  if (activeDailyChains.length > 0 && chainsToProtect.length === 0) {
    actionLabel = `${protectedChainCount} protected`
  }

  if (chainsToProtect.length > 0) {
    actionLabel = `${chainsToProtect.length} ${
      chainsToProtect.length === 1 ? 'chain' : 'chains'
    } awaiting action`
  }

  return (
    <CommandPanel
      Icon={Flame}
      action={
        <span className="rounded-full border border-streak/30 bg-streak/10 px-2.5 py-1 text-[9px] font-bold tracking-[0.08em] text-streak uppercase">
          {actionLabel}
        </span>
      }
      bodyClassName="min-h-0 !p-3"
      className="h-full"
      eyebrow="Daily momentum"
      title="Protect the chain"
    >
      {activeDailyChains.length === 0 ? (
        <div className="grid h-full min-h-0 grid-cols-[auto_minmax(0,1fr)] items-center gap-4 rounded-xl border border-dashed border-line bg-surface/50 px-5">
          <div className="grid size-11 place-items-center rounded-xl border border-line bg-surface-muted text-content-subtle">
            <Flame aria-hidden="true" size={20} strokeWidth={1.8} />
          </div>

          <div className="min-w-0">
            <p className="text-sm font-semibold">No active daily chain yet</p>

            <p className="mt-1 text-[10px] leading-4 text-content-muted">
              Complete a daily habit on consecutive days to establish momentum.
            </p>
          </div>
        </div>
      ) : chainsToProtect.length === 0 ? (
        <div className="grid h-full min-h-0 grid-cols-[auto_minmax(0,1fr)_auto] items-center gap-4 rounded-xl border border-success/25 bg-success/5 px-5">
          <div
            className="grid size-12 place-items-center rounded-xl border border-success/30 bg-success/10 text-success"
            style={{
              boxShadow:
                '0 0 18px color-mix(in srgb, var(--theme-success) 20%, transparent)',
            }}
          >
            <Shield aria-hidden="true" size={22} strokeWidth={1.8} />
          </div>

          <div className="min-w-0">
            <p className="text-sm font-semibold text-success">
              All chains protected
            </p>

            <p className="mt-1 text-[10px] leading-4 text-content-muted">
              Every active daily streak has been secured for today.
            </p>
          </div>

          <div className="text-right">
            <p className="text-2xl font-black text-success">
              {protectedChainCount}
            </p>

            <p className="text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
              Chains secure
            </p>
          </div>
        </div>
      ) : (
        <ul className="grid h-full min-h-0 grid-cols-1 gap-2 sm:grid-cols-3">
          {visibleChains.map((habit, index) => {
            const primaryReward = habit.attributeRewards[0]

            const visual = primaryReward
              ? getAttributeVisual(primaryReward.attributeType)
              : null

            const Icon: LucideIcon = visual?.Icon ?? Flame

            const accentVariable =
              visual?.colorVariable ?? 'var(--theme-streak)'

            const personalBest = Math.max(
              habit.currentStreak,
              habit.longestStreak,
              1,
            )

            const recordPercentage = getPercentage(
              habit.currentStreak,
              personalBest,
            )

            const style = {
              '--chain-accent': accentVariable,
              borderColor:
                'color-mix(in srgb, var(--chain-accent) 30%, var(--theme-border))',
              backgroundImage:
                'linear-gradient(145deg, color-mix(in srgb, var(--chain-accent) 8%, transparent), transparent 62%)',
            } as CSSProperties

            return (
              <li
                className="relative flex h-full min-h-0 flex-col overflow-hidden rounded-xl border bg-surface px-3 py-2.5"
                key={habit.id}
                style={style}
              >
                <div
                  aria-hidden="true"
                  className="absolute inset-x-0 top-0 h-px"
                  style={{
                    background:
                      'linear-gradient(90deg, transparent, var(--chain-accent), transparent)',
                  }}
                />

                <div className="flex min-w-0 items-center justify-between gap-2">
                  <div className="flex min-w-0 items-center gap-2">
                    <div
                      className="grid size-8 shrink-0 place-items-center rounded-lg border"
                      style={{
                        borderColor:
                          'color-mix(in srgb, var(--chain-accent) 38%, transparent)',
                        backgroundColor:
                          'color-mix(in srgb, var(--chain-accent) 12%, transparent)',
                        color: 'var(--chain-accent)',
                      }}
                    >
                      <Icon aria-hidden="true" size={15} strokeWidth={1.9} />
                    </div>

                    <span className="text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
                      Priority {index + 1}
                    </span>
                  </div>

                  <span className="shrink-0 rounded-full border border-streak/30 bg-streak/10 px-2 py-0.5 text-[9px] font-bold text-streak">
                    {habit.currentStreak}{' '}
                    {habit.currentStreak === 1 ? 'day' : 'days'}
                  </span>
                </div>

                <div className="mt-2 min-w-0">
                  <h3
                    className="truncate text-xs font-semibold"
                    title={habit.name}
                  >
                    {habit.name}
                  </h3>

                  <div className="mt-1 flex items-center gap-1.5 text-[9px] text-content-muted">
                    <span>Best {habit.longestStreak}</span>

                    <span aria-hidden="true">·</span>

                    <span
                      className="font-semibold"
                      style={{ color: 'var(--chain-accent)' }}
                    >
                      +{getRewardTotal(habit)} XP
                    </span>
                  </div>
                </div>

                <div className="mt-auto pt-2">
                  <div className="flex items-center justify-between gap-2 text-[8px] font-semibold tracking-wide text-content-subtle uppercase">
                    <span>Chain strength</span>

                    <span>
                      {habit.currentStreak} / {personalBest}
                    </span>
                  </div>

                  <div className="mt-1.5 h-1.5 overflow-hidden rounded-full bg-surface-muted">
                    <div
                      className="h-full rounded-full"
                      style={{
                        width: `${recordPercentage}%`,
                        backgroundColor: 'var(--chain-accent)',
                        boxShadow:
                          '0 0 10px color-mix(in srgb, var(--chain-accent) 38%, transparent)',
                      }}
                    />
                  </div>

                  <div className="mt-2 flex items-center justify-between gap-2">
                    <span className="text-[9px] font-semibold text-content-muted">
                      Protect today
                    </span>

                    <DashboardHabitAction
                      habit={habit}
                      onCompletionStatusChanged={onCompletionStatusChanged}
                    />
                  </div>
                </div>
              </li>
            )
          })}
        </ul>
      )}
    </CommandPanel>
  )
}

export function DashboardPage() {
  const [activePanel, setActivePanel] = useState<MobilePanel>('today')

  const { dashboardResource, setHabitCompletionStatus, refreshProgress } =
    useWorkspaceData()

  const {
    data: dashboard,
    errorMessage,
    isInitialLoading,
    isRefreshing,
    ensureLoaded,
  } = dashboardResource

  useEffect(() => {
    void ensureLoaded()
  }, [ensureLoaded])

  function handleCompletionStatusChanged(
    habitId: string,
    isCompletedToday: boolean,
  ): void {
    setHabitCompletionStatus(habitId, isCompletedToday)

    void refreshProgress()
  }

  return (
    <section
      aria-label="Dashboard command center"
      className="relative flex h-full min-h-0 min-w-0 flex-col gap-3 overflow-hidden"
    >
      <div
        aria-label="Dashboard panels"
        className="grid shrink-0 grid-cols-3 gap-1.5 rounded-xl border border-line bg-surface p-1 xl:hidden"
        role="group"
      >
        {[
          {
            id: 'summary' as const,
            label: 'Summary',
            Icon: CircleGauge,
          },
          {
            id: 'today' as const,
            label: 'Today',
            Icon: Activity,
          },
          {
            id: 'attributes' as const,
            label: 'Attributes',
            Icon: Sparkles,
          },
        ].map(({ id, label, Icon }) => {
          const isActive = activePanel === id

          return (
            <button
              aria-pressed={isActive}
              className={[
                'flex min-h-10 items-center justify-center gap-1.5 rounded-lg border px-2 text-[11px] font-semibold',
                isActive
                  ? 'border-accent/40 bg-accent-soft text-accent'
                  : 'border-transparent text-content-muted',
              ].join(' ')}
              key={id}
              type="button"
              onClick={() => setActivePanel(id)}
            >
              <Icon aria-hidden="true" size={14} />

              {label}
            </button>
          )
        })}
      </div>

      {isInitialLoading && (
        <div className="grid min-h-0 flex-1 place-items-center rounded-2xl border border-line bg-surface-raised">
          <p className="text-sm text-content-muted">
            Loading command center...
          </p>
        </div>
      )}

      {dashboard === null && errorMessage && (
        <div className="grid min-h-0 flex-1 place-items-center rounded-2xl border border-danger/30 bg-danger/10 p-6 text-center">
          <p className="text-sm text-danger" role="alert">
            Dashboard loading error: {errorMessage}
          </p>
        </div>
      )}

      {dashboard && (
        <div className="grid min-h-0 flex-1 gap-3 overflow-hidden xl:grid-cols-[minmax(0,1.45fr)_minmax(24rem,0.9fr)] xl:grid-rows-[8rem_minmax(0,1fr)]">
          <div
            className={`${getMobilePanelClass(
              'summary',
              activePanel,
            )} xl:col-span-2`}
            data-testid="dashboard-summary-panel"
          >
            <SummaryMetrics dashboard={dashboard} />
          </div>

          <div
            className={`${getMobilePanelClass(
              'today',
              activePanel,
            )} xl:grid xl:min-h-0 xl:grid-cols-[0.85fr_1.25fr] xl:grid-rows-[minmax(0,1fr)_13.75rem] xl:gap-3`}
            data-testid="dashboard-today-panel"
          >
            <div className="hidden min-h-0 xl:block">
              <RecentCompletionsPanel habits={dashboard.todayHabits} />
            </div>

            <div className="h-full min-h-0">
              <TodayHabitsPanel
                habits={dashboard.todayHabits}
                onCompletionStatusChanged={handleCompletionStatusChanged}
              />
            </div>

            <div className="hidden min-h-0 xl:col-span-2 xl:block">
              <ProtectTheChainPanel
                habits={dashboard.todayHabits}
                onCompletionStatusChanged={handleCompletionStatusChanged}
              />
            </div>
          </div>

          <div
            className={`${getMobilePanelClass(
              'attributes',
              activePanel,
            )} xl:grid xl:min-h-0 xl:grid-rows-[minmax(0,1.15fr)_minmax(0,0.85fr)] xl:gap-3`}
            data-testid="dashboard-attributes-panel"
          >
            <div className="min-h-0">
              <AttributeOverviewPanel dashboard={dashboard} />
            </div>

            <div className="hidden min-h-0 xl:block">
              <AttributeXpDistributionPanel dashboard={dashboard} />
            </div>
          </div>
        </div>
      )}

      {dashboard && isRefreshing && (
        <p
          className="pointer-events-none absolute right-2 bottom-2 z-30 rounded-full border border-line bg-surface-raised px-3 py-1 text-[10px] text-content-muted shadow-lg"
          role="status"
        >
          Syncing command center...
        </p>
      )}

      {dashboard && errorMessage && (
        <p
          className="absolute right-2 bottom-2 z-30 max-w-sm rounded-xl border border-warning/30 bg-surface-raised px-3 py-2 text-[10px] text-warning shadow-lg"
          role="alert"
        >
          Dashboard refresh warning: {errorMessage}
        </p>
      )}
    </section>
  )
}
