import {
  CalendarDays,
  Check,
  ChevronLeft,
  ChevronRight,
  Circle,
  CircleOff,
  Clock3,
  Flame,
  LoaderCircle,
  Pencil,
  Plus,
  Power,
  Repeat2,
  Search,
  ShieldCheck,
  Target,
  X,
  Zap,
  type LucideIcon,
} from 'lucide-react'
import {
  useEffect,
  useMemo,
  useState,
  type CSSProperties,
  type FormEvent,
} from 'react'
import {
  completeHabit,
  createHabit,
  deactivateHabit,
  undoHabitCompletion,
  updateHabit,
} from '../../api/habitsApi'
import type { DashboardHabitResponse } from '../../types/DashboardHabitResponse'
import type { HabitAttributeRewardResponse } from '../../types/HabitAttributeRewardResponse'
import type { HabitCategory } from '../../types/HabitCategory'
import type { HabitDifficulty } from '../../types/HabitDifficulty'
import type { HabitFrequencyType } from '../../types/HabitFrequencyType'
import type { HabitResponse } from '../../types/HabitResponse'
import { useWorkspaceData } from '../../workspace/useWorkspaceData'
import { getAttributeVisual } from '../attributes/attributeVisuals'
import { HabitActivateButton } from './HabitActivateButton'
import {
  getHabitCategoryLabel,
  habitCategoryOptions,
} from './habitCategoryOptions'

type HabitListProps = {
  onHabitUpdated: (habit: HabitResponse) => void
  onHabitDeactivated: (habit: HabitResponse) => void
  onProgressChanged: () => void
}

type HabitTab = 'active' | 'inactive'
type InspectorMode = 'details' | 'create' | 'edit'

type HabitSortOption =
  'nameAsc' | 'nameDesc' | 'streakDesc' | 'rewardDesc' | 'newest'

type FrequencyConfiguration = {
  frequencyType: HabitFrequencyType
  targetCount: number
}

type HabitEditorProps = {
  mode: 'create' | 'edit'
  habit?: HabitResponse
  onCancel: () => void
  onSaved: (habit: HabitResponse) => void
}

type CompletionControlProps = {
  habit: HabitResponse
  variant: 'circle' | 'wide'
  onCompletionStatusChanged: (habit: HabitResponse) => void
  onProgressChanged: () => void
}

type DeactivateControlProps = {
  habit: HabitResponse
  onHabitDeactivated: (habit: HabitResponse) => void
}

type HabitInspectorProps = {
  mode: InspectorMode
  habit: HabitResponse | null
  dashboardHabit: DashboardHabitResponse | undefined
  onCancelMode: () => void
  onClose: () => void
  onCompletionStatusChanged: (habit: HabitResponse) => void
  onHabitActivated: (habit: HabitResponse) => void
  onHabitCreated: (habit: HabitResponse) => void
  onHabitDeactivated: (habit: HabitResponse) => void
  onHabitUpdated: (habit: HabitResponse) => void
  onStartEdit: () => void
  onProgressChanged: () => void
}

const fieldClassName =
  'mt-1.5 w-full rounded-xl border border-line bg-surface px-3 py-2.5 text-sm text-content outline-none transition-colors placeholder:text-content-subtle focus:border-accent/60 focus:ring-2 focus:ring-accent/15 disabled:cursor-not-allowed disabled:opacity-55'

const labelClassName =
  'block text-[9px] font-bold tracking-[0.12em] text-content-muted uppercase'

const monthNames = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
]

function formatDateOnly(value: string): string {
  const [year, month, day] = value.split('-').map(Number)

  return `${monthNames[month - 1]} ${day}, ${year}`
}

function formatLabel(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1)
}

function getFrequencyLabel(configuration: FrequencyConfiguration): string {
  if (configuration.frequencyType === 'daily') {
    return 'Daily'
  }

  return `${configuration.targetCount}× / week`
}

function getStreakUnit(frequencyType: HabitFrequencyType): string {
  return frequencyType === 'daily' ? 'days' : 'weeks'
}

function getRewardTotal(
  dashboardHabit: DashboardHabitResponse | undefined,
): number {
  return (
    dashboardHabit?.attributeRewards.reduce(
      (total, reward) => total + reward.xpAmount,
      0,
    ) ?? 0
  )
}

function getDifficultyLevel(difficulty: HabitDifficulty): number {
  switch (difficulty) {
    case 'easy':
      return 1
    case 'medium':
      return 2
    case 'hard':
      return 3
    case 'elite':
      return 4
  }
}

function getDifficultyColor(difficulty: HabitDifficulty): string {
  switch (difficulty) {
    case 'easy':
      return 'var(--theme-success)'
    case 'medium':
      return 'var(--theme-warning)'
    case 'hard':
      return 'var(--theme-danger)'
    case 'elite':
      return 'var(--theme-energy-violet)'
  }
}

function getPageSize(): number {
  if (typeof window === 'undefined') {
    return 6
  }

  if (window.innerHeight >= 1200) {
    return 10
  }

  if (window.innerHeight >= 900) {
    return 8
  }

  return 6
}

function getHabitErrorMessage(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback
}

function DifficultyIndicator({
  difficulty,
  compact = false,
}: {
  difficulty: HabitDifficulty
  compact?: boolean
}) {
  const difficultyLevel = getDifficultyLevel(difficulty)
  const difficultyColor = getDifficultyColor(difficulty)

  return (
    <div className="min-w-0">
      <p
        className={['font-medium', compact ? 'text-[10px]' : 'text-xs'].join(
          ' ',
        )}
        style={{ color: difficultyColor }}
      >
        {formatLabel(difficulty)}
      </p>

      <div
        aria-label={`${formatLabel(
          difficulty,
        )} difficulty, ${difficultyLevel} of 4`}
        className="mt-1 flex items-center gap-1"
      >
        {Array.from({ length: 4 }, (_, index) => {
          const isActive = index < difficultyLevel

          return (
            <span
              aria-hidden="true"
              className="size-1.5 rounded-full border"
              key={index}
              style={{
                borderColor: isActive
                  ? difficultyColor
                  : 'var(--theme-border-strong)',
                backgroundColor: isActive ? difficultyColor : 'transparent',
                boxShadow: isActive
                  ? `0 0 8px color-mix(in srgb, ${difficultyColor} 45%, transparent)`
                  : 'none',
              }}
            />
          )
        })}
      </div>
    </div>
  )
}

function HabitEditor({ mode, habit, onCancel, onSaved }: HabitEditorProps) {
  const editableConfiguration =
    mode === 'edit' && habit ? (habit.pendingConfiguration ?? habit) : null

  const [name, setName] = useState(habit?.name ?? '')
  const [description, setDescription] = useState(habit?.description ?? '')
  const [category, setCategory] = useState<HabitCategory | ''>(
    editableConfiguration?.category ?? '',
  )
  const [frequencyType, setFrequencyType] = useState<HabitFrequencyType>(
    editableConfiguration?.frequencyType ?? 'daily',
  )
  const [targetCount, setTargetCount] = useState(
    editableConfiguration?.targetCount ?? 1,
  )
  const [difficulty, setDifficulty] = useState<HabitDifficulty>(
    editableConfiguration?.difficulty ?? 'medium',
  )

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  function changeFrequencyType(value: HabitFrequencyType): void {
    setFrequencyType(value)

    if (value === 'daily') {
      setTargetCount(1)
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (category === '') {
      setErrorMessage('Choose a category.')
      return
    }

    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      const request = {
        name,
        description: description.trim() === '' ? null : description,
        category,
        frequencyType,
        targetCount,
        difficulty,
      }

      const savedHabit =
        mode === 'create'
          ? await createHabit(request)
          : await updateHabit(habit!.id, request)

      onSaved(savedHabit)
    } catch (error) {
      setErrorMessage(
        getHabitErrorMessage(
          error,
          mode === 'create'
            ? 'An unknown habit-creation error occurred.'
            : 'An unknown habit-update error occurred.',
        ),
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  const formLabel =
    mode === 'create' ? 'Create habit' : `Edit ${habit?.name ?? 'habit'}`

  return (
    <form
      aria-label={formLabel}
      className="space-y-3.5"
      onSubmit={handleSubmit}
    >
      <div className="rounded-xl border border-accent/20 bg-accent-soft/50 px-3 py-2.5">
        <p className="text-[10px] leading-4 text-content-muted">
          {mode === 'create'
            ? 'The backend assigns the authoritative XP and attribute rewards after creation.'
            : 'Name and description update immediately. Rule changes begin at the next configured week boundary.'}
        </p>

        {mode === 'edit' && habit?.pendingConfiguration && (
          <p
            className="mt-2 text-[10px] font-semibold text-warning"
            role="status"
          >
            Scheduled rule changes take effect on{' '}
            {formatDateOnly(habit.pendingConfiguration.effectiveFromDate)}.
          </p>
        )}
      </div>

      <div>
        <label className={labelClassName} htmlFor="workspace-habit-name">
          Name
        </label>

        <input
          required
          className={fieldClassName}
          disabled={isSubmitting}
          id="workspace-habit-name"
          maxLength={100}
          placeholder="Example: Read C# for 30 minutes"
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
        />
      </div>

      <div>
        <label className={labelClassName} htmlFor="workspace-habit-description">
          Description
        </label>

        <textarea
          className={fieldClassName}
          disabled={isSubmitting}
          id="workspace-habit-description"
          maxLength={500}
          placeholder="Describe the action or standard for success."
          rows={3}
          value={description}
          onChange={(event) => setDescription(event.target.value)}
        />
      </div>

      <div>
        <label className={labelClassName} htmlFor="workspace-habit-category">
          Category
        </label>

        <select
          required
          className={fieldClassName}
          disabled={isSubmitting}
          id="workspace-habit-category"
          value={category}
          onChange={(event) =>
            setCategory(event.target.value as HabitCategory | '')
          }
        >
          <option disabled value="">
            Select a category
          </option>

          {habitCategoryOptions.map((categoryOption) => (
            <option key={categoryOption.value} value={categoryOption.value}>
              {categoryOption.label}
            </option>
          ))}
        </select>
      </div>

      <div className="grid grid-cols-2 gap-3">
        <div>
          <label className={labelClassName} htmlFor="workspace-habit-frequency">
            Frequency
          </label>

          <select
            className={fieldClassName}
            disabled={isSubmitting}
            id="workspace-habit-frequency"
            value={frequencyType}
            onChange={(event) =>
              changeFrequencyType(event.target.value as HabitFrequencyType)
            }
          >
            <option value="daily">Daily</option>
            <option value="weekly">Weekly</option>
          </select>
        </div>

        <div>
          <label
            className={labelClassName}
            htmlFor="workspace-habit-difficulty"
          >
            Difficulty
          </label>

          <select
            className={fieldClassName}
            disabled={isSubmitting}
            id="workspace-habit-difficulty"
            value={difficulty}
            onChange={(event) =>
              setDifficulty(event.target.value as HabitDifficulty)
            }
          >
            <option value="easy">Easy</option>
            <option value="medium">Medium</option>
            <option value="hard">Hard</option>
            <option value="elite">Elite</option>
          </select>
        </div>
      </div>

      {frequencyType === 'weekly' && (
        <div>
          <label className={labelClassName} htmlFor="workspace-habit-target">
            Times per week
          </label>

          <input
            required
            className={fieldClassName}
            disabled={isSubmitting}
            id="workspace-habit-target"
            max={7}
            min={1}
            type="number"
            value={targetCount}
            onChange={(event) => setTargetCount(Number(event.target.value))}
          />
        </div>
      )}

      <div className="flex gap-2 pt-1">
        <button
          className="flex min-h-11 flex-1 items-center justify-center gap-2 rounded-xl border border-accent/40 bg-accent px-4 py-2.5 text-sm font-bold text-white transition hover:brightness-110 disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isSubmitting}
          type="submit"
        >
          {isSubmitting ? (
            <>
              <LoaderCircle
                aria-hidden="true"
                className="animate-spin motion-reduce:animate-none"
                size={15}
              />

              {mode === 'create' ? 'Creating habit...' : 'Saving changes...'}
            </>
          ) : (
            <>
              {mode === 'create' ? (
                <Plus aria-hidden="true" size={15} />
              ) : (
                <ShieldCheck aria-hidden="true" size={15} />
              )}

              {mode === 'create' ? 'Create habit' : 'Save changes'}
            </>
          )}
        </button>

        <button
          className="min-h-11 rounded-xl border border-line bg-surface-muted px-4 py-2.5 text-sm font-semibold text-content-muted transition hover:border-line-strong hover:text-content disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isSubmitting}
          type="button"
          onClick={onCancel}
        >
          Cancel
        </button>
      </div>

      {errorMessage && (
        <p
          className="rounded-xl border border-danger/30 bg-danger/10 px-3 py-2.5 text-[11px] text-danger"
          role="alert"
        >
          {mode === 'create' ? 'Habit creation error' : 'Habit update error'}:{' '}
          {errorMessage}
        </p>
      )}
    </form>
  )
}

function CompletionControl({
  habit,
  variant,
  onCompletionStatusChanged,
  onProgressChanged,
}: CompletionControlProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [earnedRewards, setEarnedRewards] = useState<
    HabitAttributeRewardResponse[] | null
  >(null)

  async function handleCompletionToggle() {
    setIsSaving(true)
    setErrorMessage(null)

    try {
      if (habit.isCompletedToday) {
        await undoHabitCompletion(habit.id)

        setEarnedRewards(null)

        onCompletionStatusChanged({
          ...habit,
          isCompletedToday: false,
        })
      } else {
        const response = await completeHabit(habit.id, {
          notes: null,
        })

        setEarnedRewards(response.rewards)

        onCompletionStatusChanged({
          ...habit,
          isCompletedToday: true,
        })
      }

      onProgressChanged()
    } catch (error) {
      setErrorMessage(
        getHabitErrorMessage(
          error,
          'An unknown habit-completion error occurred.',
        ),
      )
    } finally {
      setIsSaving(false)
    }
  }

  const visibleLabel = isSaving
    ? habit.isCompletedToday
      ? 'Undoing...'
      : 'Completing...'
    : habit.isCompletedToday
      ? 'Undo completion'
      : 'Mark complete'

  const accessibleLabel =
    variant === 'circle'
      ? habit.isCompletedToday
        ? `Undo completion for ${habit.name}`
        : `Complete ${habit.name}`
      : visibleLabel

  if (variant === 'circle') {
    return (
      <div className="relative grid place-items-center">
        <button
          aria-label={accessibleLabel}
          aria-pressed={habit.isCompletedToday}
          className={[
            'grid size-8 place-items-center rounded-full border transition',
            habit.isCompletedToday
              ? 'border-success bg-success/10 text-success shadow-[0_0_12px_color-mix(in_srgb,var(--theme-success)_24%,transparent)]'
              : 'border-line-strong bg-surface text-content-subtle hover:border-accent/55 hover:text-accent',
          ].join(' ')}
          disabled={isSaving}
          title={accessibleLabel}
          type="button"
          onClick={() => void handleCompletionToggle()}
        >
          {isSaving ? (
            <LoaderCircle
              aria-hidden="true"
              className="animate-spin motion-reduce:animate-none"
              size={14}
            />
          ) : habit.isCompletedToday ? (
            <Check aria-hidden="true" size={15} strokeWidth={2.2} />
          ) : (
            <Circle aria-hidden="true" size={14} />
          )}
        </button>

        {errorMessage && (
          <p
            className="absolute right-0 top-10 z-50 w-64 rounded-xl border border-danger/30 bg-surface-raised p-3 text-left text-[10px] text-danger shadow-[var(--theme-panel-shadow)]"
            role="alert"
          >
            Habit completion error: {errorMessage}
          </p>
        )}

        {earnedRewards && (
          <p className="sr-only" role="status">
            Habit completed!{' '}
            {earnedRewards
              .map((reward) => {
                const visual = getAttributeVisual(reward.attributeType)

                return `+${reward.xpAmount} ${visual.label} XP`
              })
              .join(', ')}
          </p>
        )}
      </div>
    )
  }

  return (
    <div>
      <button
        aria-pressed={habit.isCompletedToday}
        className={[
          'flex min-h-11 w-full items-center justify-center gap-2 rounded-xl border px-4 py-2.5 text-sm font-bold transition disabled:cursor-not-allowed disabled:opacity-50',
          habit.isCompletedToday
            ? 'border-success/35 bg-success/10 text-success hover:bg-success/15'
            : 'border-accent/45 bg-accent text-white hover:brightness-110',
        ].join(' ')}
        disabled={isSaving}
        type="button"
        onClick={() => void handleCompletionToggle()}
      >
        {isSaving ? (
          <LoaderCircle
            aria-hidden="true"
            className="animate-spin motion-reduce:animate-none"
            size={15}
          />
        ) : habit.isCompletedToday ? (
          <CircleOff aria-hidden="true" size={15} />
        ) : (
          <Check aria-hidden="true" size={15} />
        )}

        {visibleLabel}
      </button>

      {earnedRewards && (
        <div
          className="mt-2 rounded-xl border border-success/25 bg-success/5 px-3 py-2.5"
          role="status"
        >
          <p className="text-[10px] font-semibold text-success">
            Habit completed!
          </p>

          <p className="mt-1 text-[9px] text-content-muted">
            {earnedRewards
              .map((reward) => {
                const visual = getAttributeVisual(reward.attributeType)

                return `+${reward.xpAmount} ${visual.label} XP`
              })
              .join(' · ')}
          </p>
        </div>
      )}

      {errorMessage && (
        <p
          className="mt-2 rounded-xl border border-danger/30 bg-danger/10 px-3 py-2 text-[10px] text-danger"
          role="alert"
        >
          Habit completion error: {errorMessage}
        </p>
      )}
    </div>
  )
}

function DeactivateControl({
  habit,
  onHabitDeactivated,
}: DeactivateControlProps) {
  const [isConfirming, setIsConfirming] = useState(false)
  const [isDeactivating, setIsDeactivating] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  async function handleDeactivate() {
    setIsDeactivating(true)
    setErrorMessage(null)

    try {
      const deactivatedHabit = await deactivateHabit(habit.id)

      setIsConfirming(false)
      onHabitDeactivated(deactivatedHabit)
    } catch (error) {
      setErrorMessage(
        getHabitErrorMessage(
          error,
          'An unknown habit-deactivation error occurred.',
        ),
      )
    } finally {
      setIsDeactivating(false)
    }
  }

  if (!isConfirming) {
    return (
      <button
        className="flex min-h-11 w-full items-center justify-center gap-2 rounded-xl border border-danger/30 bg-danger/5 px-4 py-2.5 text-sm font-semibold text-danger transition hover:bg-danger/10"
        type="button"
        onClick={() => {
          setErrorMessage(null)
          setIsConfirming(true)
        }}
      >
        <Power aria-hidden="true" size={15} />
        Deactivate
      </button>
    )
  }

  return (
    <div className="rounded-xl border border-danger/30 bg-danger/5 p-3">
      <p className="text-xs font-semibold">
        Deactivate &quot;{habit.name}&quot;?
      </p>

      <p className="mt-1 text-[10px] leading-4 text-content-muted">
        It will move to the Inactive Habits tab and can no longer be completed.
      </p>

      <div className="mt-3 flex gap-2">
        <button
          className="min-h-10 flex-1 rounded-xl bg-danger px-3 py-2 text-xs font-bold text-white disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isDeactivating}
          type="button"
          onClick={() => void handleDeactivate()}
        >
          {isDeactivating ? 'Deactivating...' : 'Confirm deactivation'}
        </button>

        <button
          className="min-h-10 rounded-xl border border-line px-3 py-2 text-xs font-semibold text-content-muted disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isDeactivating}
          type="button"
          onClick={() => {
            setErrorMessage(null)
            setIsConfirming(false)
          }}
        >
          Keep habit
        </button>
      </div>

      {errorMessage && (
        <p className="mt-2 text-[10px] text-danger" role="alert">
          Habit deactivation error: {errorMessage}
        </p>
      )}
    </div>
  )
}

function HabitInspector({
  mode,
  habit,
  dashboardHabit,
  onCancelMode,
  onClose,
  onCompletionStatusChanged,
  onHabitActivated,
  onHabitCreated,
  onHabitDeactivated,
  onHabitUpdated,
  onStartEdit,
  onProgressChanged,
}: HabitInspectorProps) {
  if (mode === 'create') {
    return (
      <aside className="flex h-full min-h-0 flex-col overflow-hidden rounded-2xl border border-line bg-surface-raised shadow-[var(--theme-panel-shadow)]">
        <header className="flex shrink-0 items-start justify-between gap-3 border-b border-line px-4 py-4">
          <div>
            <p className="text-[9px] font-bold tracking-[0.16em] text-accent uppercase">
              New progression path
            </p>

            <h2 className="mt-1 text-lg font-semibold">Create habit</h2>
          </div>

          <button
            aria-label="Cancel habit creation"
            className="grid size-9 place-items-center rounded-xl border border-line bg-surface text-content-muted transition hover:text-content"
            type="button"
            onClick={onCancelMode}
          >
            <X aria-hidden="true" size={16} />
          </button>
        </header>

        <div className="min-h-0 flex-1 overflow-y-auto p-4">
          <HabitEditor
            mode="create"
            onCancel={onCancelMode}
            onSaved={onHabitCreated}
          />
        </div>
      </aside>
    )
  }

  if (mode === 'edit' && habit) {
    return (
      <aside className="flex h-full min-h-0 flex-col overflow-hidden rounded-2xl border border-line bg-surface-raised shadow-[var(--theme-panel-shadow)]">
        <header className="flex shrink-0 items-start justify-between gap-3 border-b border-line px-4 py-4">
          <div>
            <p className="text-[9px] font-bold tracking-[0.16em] text-accent uppercase">
              Configuration
            </p>

            <h2 className="mt-1 text-lg font-semibold">Edit habit</h2>
          </div>

          <button
            aria-label="Cancel habit editing"
            className="grid size-9 place-items-center rounded-xl border border-line bg-surface text-content-muted transition hover:text-content"
            type="button"
            onClick={onCancelMode}
          >
            <X aria-hidden="true" size={16} />
          </button>
        </header>

        <div className="min-h-0 flex-1 overflow-y-auto p-4">
          <HabitEditor
            habit={habit}
            mode="edit"
            onCancel={onCancelMode}
            onSaved={onHabitUpdated}
          />
        </div>
      </aside>
    )
  }

  if (!habit) {
    return (
      <aside className="grid h-full min-h-64 place-items-center rounded-2xl border border-dashed border-line bg-surface-raised/65 px-6 text-center">
        <div>
          <div className="mx-auto grid size-12 place-items-center rounded-2xl border border-line bg-surface text-content-subtle">
            <Target aria-hidden="true" size={21} />
          </div>

          <h2 className="mt-3 text-sm font-semibold">Select a habit</h2>

          <p className="mt-1 max-w-56 text-[10px] leading-4 text-content-muted">
            Choose a row to inspect its progression, rewards, and available
            actions.
          </p>
        </div>
      </aside>
    )
  }

  const primaryReward = dashboardHabit?.attributeRewards[0]

  const primaryVisual = primaryReward
    ? getAttributeVisual(primaryReward.attributeType)
    : null

  const HabitIcon: LucideIcon =
    primaryVisual?.Icon ??
    (habit.frequencyType === 'daily' ? CalendarDays : Repeat2)

  const accentColor =
    primaryVisual?.colorVariable ?? 'var(--theme-accent-primary)'

  const rewardTotal = getRewardTotal(dashboardHabit)

  const inspectorStyle = {
    '--habit-accent': accentColor,
  } as CSSProperties

  return (
    <aside
      className="flex h-full min-h-0 flex-col overflow-hidden rounded-2xl border border-line bg-surface-raised shadow-[var(--theme-panel-shadow)]"
      style={inspectorStyle}
    >
      <header className="relative shrink-0 border-b border-line px-4 py-4">
        <div
          aria-hidden="true"
          className="absolute inset-x-0 top-0 h-px"
          style={{
            background:
              'linear-gradient(90deg, transparent, var(--habit-accent), transparent)',
          }}
        />

        <div className="flex items-start justify-between gap-3">
          <div className="flex min-w-0 items-start gap-3">
            <div
              className="grid size-12 shrink-0 place-items-center rounded-2xl border"
              style={{
                borderColor:
                  'color-mix(in srgb, var(--habit-accent) 42%, transparent)',
                backgroundColor:
                  'color-mix(in srgb, var(--habit-accent) 12%, transparent)',
                color: 'var(--habit-accent)',
              }}
            >
              <HabitIcon aria-hidden="true" size={21} strokeWidth={1.8} />
            </div>

            <div className="min-w-0">
              <div className="flex items-center gap-2">
                <h2 className="truncate text-base font-semibold">
                  {habit.name}
                </h2>

                <span
                  className={[
                    'shrink-0 text-[8px] font-bold tracking-[0.1em] uppercase',
                    habit.isActive ? 'text-success' : 'text-content-subtle',
                  ].join(' ')}
                >
                  {habit.isActive ? 'Active' : 'Inactive'}
                </span>
              </div>

              <p className="mt-1 line-clamp-3 text-[10px] leading-4 text-content-muted">
                {habit.description ??
                  'No description has been added for this habit.'}
              </p>
            </div>
          </div>

          <button
            aria-label="Close selected habit"
            className="grid size-8 shrink-0 place-items-center rounded-lg text-content-subtle transition hover:bg-surface-hover hover:text-content"
            type="button"
            onClick={onClose}
          >
            <X aria-hidden="true" size={15} />
          </button>
        </div>
      </header>

      <div className="min-h-0 flex-1 overflow-y-auto">
        <dl className="divide-y divide-line px-4">
          <div className="flex items-center justify-between gap-4 py-3">
            <dt className="text-[10px] text-content-muted">Category</dt>

            <dd className="text-right text-[10px] font-medium">
              {getHabitCategoryLabel(habit.category)}
            </dd>
          </div>

          <div className="flex items-center justify-between gap-4 py-3">
            <dt className="text-[10px] text-content-muted">Frequency</dt>

            <dd className="text-right text-[10px] font-medium">
              {getFrequencyLabel(habit)}
            </dd>
          </div>

          <div className="flex items-center justify-between gap-4 py-3">
            <dt className="text-[10px] text-content-muted">Difficulty</dt>

            <dd>
              <DifficultyIndicator compact difficulty={habit.difficulty} />
            </dd>
          </div>

          <div className="flex items-center justify-between gap-4 py-3">
            <dt className="text-[10px] text-content-muted">Streak</dt>

            <dd className="flex items-center gap-1.5 text-[10px] font-semibold text-streak">
              <Flame aria-hidden="true" size={13} />

              {dashboardHabit
                ? `${dashboardHabit.currentStreak} ${getStreakUnit(
                    habit.frequencyType,
                  )}`
                : '—'}
            </dd>
          </div>

          <div className="flex items-center justify-between gap-4 py-3">
            <dt className="text-[10px] text-content-muted">XP reward</dt>

            <dd className="flex items-center gap-1.5 text-[10px] font-semibold text-energy-blue">
              <Zap aria-hidden="true" size={13} />

              {rewardTotal > 0 ? `+${rewardTotal} XP` : '—'}
            </dd>
          </div>
        </dl>

        {habit.pendingConfiguration && (
          <section
            aria-label={`Scheduled changes for ${habit.name}`}
            className="mx-4 mt-4 rounded-xl border border-warning/25 bg-warning/5 p-3"
          >
            <div className="flex items-start gap-2">
              <Clock3
                aria-hidden="true"
                className="mt-0.5 shrink-0 text-warning"
                size={14}
              />

              <div>
                <p className="text-[10px] font-semibold text-warning">
                  Scheduled for{' '}
                  {formatDateOnly(habit.pendingConfiguration.effectiveFromDate)}
                </p>

                <p className="mt-1 text-[9px] leading-4 text-content-muted">
                  Frequency: {getFrequencyLabel(habit.pendingConfiguration)}
                  {' · '}
                  Category:{' '}
                  {getHabitCategoryLabel(habit.pendingConfiguration.category)}
                  {' · '}
                  Difficulty:{' '}
                  {formatLabel(habit.pendingConfiguration.difficulty)}
                </p>
              </div>
            </div>
          </section>
        )}

        <section className="px-4 py-4">
          <p className="text-[9px] font-bold tracking-[0.14em] text-content-subtle uppercase">
            Attribute rewards preview
          </p>

          {dashboardHabit?.attributeRewards.length ? (
            <ul className="mt-3 grid grid-cols-2 gap-2">
              {dashboardHabit.attributeRewards.map((reward, index) => {
                const visual = getAttributeVisual(reward.attributeType)

                return (
                  <li
                    className="rounded-xl border bg-surface p-3"
                    key={reward.attributeType}
                    style={
                      {
                        borderColor:
                          'color-mix(in srgb, var(--reward-accent) 30%, var(--theme-border))',
                        '--reward-accent': visual.colorVariable,
                      } as CSSProperties
                    }
                  >
                    <div
                      className="grid size-8 place-items-center rounded-lg border"
                      style={{
                        borderColor:
                          'color-mix(in srgb, var(--reward-accent) 40%, transparent)',
                        backgroundColor:
                          'color-mix(in srgb, var(--reward-accent) 12%, transparent)',
                        color: 'var(--reward-accent)',
                      }}
                    >
                      <visual.Icon aria-hidden="true" size={15} />
                    </div>

                    <p className="mt-2 truncate text-[10px] font-semibold">
                      {visual.label}
                    </p>

                    <p
                      className="mt-0.5 text-sm font-bold"
                      style={{ color: 'var(--reward-accent)' }}
                    >
                      +{reward.xpAmount} XP
                    </p>

                    <p className="mt-0.5 text-[8px] text-content-subtle">
                      {index === 0 ? 'Primary reward' : 'Secondary reward'}
                    </p>

                    <div className="mt-2 h-1 overflow-hidden rounded-full bg-surface-muted">
                      <div
                        className="h-full rounded-full"
                        style={{
                          width: index === 0 ? '70%' : '30%',
                          backgroundColor: 'var(--reward-accent)',
                        }}
                      />
                    </div>
                  </li>
                )
              })}
            </ul>
          ) : (
            <div className="mt-3 rounded-xl border border-dashed border-line bg-surface/40 px-3 py-4 text-center">
              <p className="text-[9px] leading-4 text-content-muted">
                Reward details are unavailable for this inactive or
                unsynchronized habit.
              </p>
            </div>
          )}
        </section>
      </div>

      {habit.isActive ? (
        <div className="shrink-0 space-y-2 border-t border-line p-4">
          <CompletionControl
            habit={habit}
            variant="wide"
            onCompletionStatusChanged={onCompletionStatusChanged}
            onProgressChanged={onProgressChanged}
          />

          <button
            className="flex min-h-11 w-full items-center justify-center gap-2 rounded-xl border border-line bg-surface px-4 py-2.5 text-sm font-semibold text-content-muted transition hover:border-accent/35 hover:text-accent"
            type="button"
            onClick={onStartEdit}
          >
            <Pencil aria-hidden="true" size={15} />
            Edit
          </button>

          <DeactivateControl
            habit={habit}
            onHabitDeactivated={onHabitDeactivated}
          />
        </div>
      ) : (
        <div className="shrink-0 border-t border-line p-4">
          <HabitActivateButton
            habit={habit}
            onHabitActivated={onHabitActivated}
          />

          <p className="mt-2 text-center text-[9px] leading-4 text-content-subtle">
            Reactivating restores this habit to the active workspace without
            deleting its history.
          </p>
        </div>
      )}
    </aside>
  )
}

export function HabitList({
  onHabitUpdated,
  onHabitDeactivated,
  onProgressChanged,
}: HabitListProps) {
  const { dashboardResource, habitsResource } = useWorkspaceData()

  const {
    data: habits,
    errorMessage,
    isRefreshing,
    ensureLoaded,
    updateData: updateHabits,
  } = habitsResource

  const dashboard = dashboardResource.data

  const [activeTab, setActiveTab] = useState<HabitTab>('active')

  const [inspectorMode, setInspectorMode] = useState<InspectorMode>('details')

  const [selectedHabitId, setSelectedHabitId] = useState<string | null>(null)

  const [searchQuery, setSearchQuery] = useState('')

  const [categoryFilter, setCategoryFilter] = useState<HabitCategory | 'all'>(
    'all',
  )

  const [frequencyFilter, setFrequencyFilter] = useState<
    HabitFrequencyType | 'all'
  >('all')

  const [difficultyFilter, setDifficultyFilter] = useState<
    HabitDifficulty | 'all'
  >('all')

  const [sortOption, setSortOption] = useState<HabitSortOption>('nameAsc')

  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(getPageSize)

  useEffect(() => {
    void ensureLoaded()
  }, [ensureLoaded])

  useEffect(() => {
    function handleResize() {
      setPageSize(getPageSize())
    }

    window.addEventListener('resize', handleResize)

    return () => {
      window.removeEventListener('resize', handleResize)
    }
  }, [])

  const dashboardHabitById = useMemo(() => {
    return new Map(
      dashboard?.todayHabits.map((habit) => [habit.id, habit]) ?? [],
    )
  }, [dashboard])

  const activeHabits = habits?.filter((habit) => habit.isActive) ?? []

  const inactiveHabits = habits?.filter((habit) => !habit.isActive) ?? []

  const sourceHabits = activeTab === 'active' ? activeHabits : inactiveHabits

  const filteredHabits = useMemo(() => {
    const normalizedQuery = searchQuery.trim().toLocaleLowerCase()

    const matchingHabits = sourceHabits.filter((habit) => {
      const matchesSearch =
        normalizedQuery === '' ||
        habit.name.toLocaleLowerCase().includes(normalizedQuery) ||
        habit.description?.toLocaleLowerCase().includes(normalizedQuery) ||
        getHabitCategoryLabel(habit.category)
          .toLocaleLowerCase()
          .includes(normalizedQuery)

      const matchesCategory =
        categoryFilter === 'all' || habit.category === categoryFilter

      const matchesFrequency =
        frequencyFilter === 'all' || habit.frequencyType === frequencyFilter

      const matchesDifficulty =
        difficultyFilter === 'all' || habit.difficulty === difficultyFilter

      return (
        matchesSearch &&
        matchesCategory &&
        matchesFrequency &&
        matchesDifficulty
      )
    })

    return [...matchingHabits].sort((first, second) => {
      const firstProgression = dashboardHabitById.get(first.id)

      const secondProgression = dashboardHabitById.get(second.id)

      switch (sortOption) {
        case 'nameAsc':
          return first.name.localeCompare(second.name)

        case 'nameDesc':
          return second.name.localeCompare(first.name)

        case 'streakDesc':
          return (
            (secondProgression?.currentStreak ?? -1) -
              (firstProgression?.currentStreak ?? -1) ||
            first.name.localeCompare(second.name)
          )

        case 'rewardDesc':
          return (
            getRewardTotal(secondProgression) -
              getRewardTotal(firstProgression) ||
            first.name.localeCompare(second.name)
          )

        case 'newest':
          return (
            new Date(second.createdAtUtc).getTime() -
            new Date(first.createdAtUtc).getTime()
          )
      }
    })
  }, [
    categoryFilter,
    dashboardHabitById,
    difficultyFilter,
    frequencyFilter,
    searchQuery,
    sortOption,
    sourceHabits,
  ])

  const pageCount = Math.max(1, Math.ceil(filteredHabits.length / pageSize))

  const currentPage = Math.min(page, pageCount)

  const visibleHabits = filteredHabits.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize,
  )

  const selectedHabit =
    habits?.find((habit) => habit.id === selectedHabitId) ?? null

  const selectedDashboardHabit = selectedHabit
    ? dashboardHabitById.get(selectedHabit.id)
    : undefined

  useEffect(() => {
    setPage(1)
  }, [
    activeTab,
    categoryFilter,
    difficultyFilter,
    frequencyFilter,
    pageSize,
    searchQuery,
    sortOption,
  ])

  useEffect(() => {
    if (inspectorMode !== 'details') {
      return
    }

    const selectedIsVisible = filteredHabits.some(
      (habit) => habit.id === selectedHabitId,
    )

    if (!selectedIsVisible) {
      setSelectedHabitId(filteredHabits[0]?.id ?? null)
    }
  }, [filteredHabits, inspectorMode, selectedHabitId])

  useEffect(() => {
    if (
      habits !== null &&
      activeHabits.length === 0 &&
      inactiveHabits.length === 0
    ) {
      setInspectorMode('create')
    }
  }, [activeHabits.length, habits, inactiveHabits.length])

  function replaceCachedHabit(updatedHabit: HabitResponse): void {
    updateHabits((currentHabits) => {
      if (currentHabits === null) {
        return [updatedHabit]
      }

      const exists = currentHabits.some((habit) => habit.id === updatedHabit.id)

      if (!exists) {
        return [...currentHabits, updatedHabit]
      }

      return currentHabits.map((habit) =>
        habit.id === updatedHabit.id ? updatedHabit : habit,
      )
    })
  }

  function handleHabitCreated(createdHabit: HabitResponse): void {
    replaceCachedHabit(createdHabit)
    setActiveTab('active')
    setSelectedHabitId(createdHabit.id)
    setInspectorMode('details')
    onProgressChanged()
  }

  function handleHabitUpdated(updatedHabit: HabitResponse): void {
    replaceCachedHabit(updatedHabit)
    setSelectedHabitId(updatedHabit.id)
    setInspectorMode('details')
    onHabitUpdated(updatedHabit)
  }

  function handleCompletionStatusChanged(updatedHabit: HabitResponse): void {
    replaceCachedHabit(updatedHabit)
  }

  function handleHabitDeactivated(deactivatedHabit: HabitResponse): void {
    replaceCachedHabit(deactivatedHabit)
    setSelectedHabitId(null)
    setInspectorMode('details')
    onHabitDeactivated(deactivatedHabit)
  }

  function handleHabitActivated(activatedHabit: HabitResponse): void {
    replaceCachedHabit(activatedHabit)
    setActiveTab('active')
    setSelectedHabitId(activatedHabit.id)
    setInspectorMode('details')
    onHabitUpdated(activatedHabit)
  }

  function changeTab(tab: HabitTab): void {
    setActiveTab(tab)
    setInspectorMode('details')
    setSelectedHabitId(null)
  }

  const isWaitingForInitialData = habits === null && !errorMessage

  const rangeStart =
    filteredHabits.length === 0 ? 0 : (currentPage - 1) * pageSize + 1

  const rangeEnd = Math.min(currentPage * pageSize, filteredHabits.length)

  const tableGridClassName =
    'grid min-w-0 grid-cols-[minmax(12rem,2fr)_5.5rem_5.5rem_5rem] items-center gap-3 min-[1500px]:grid-cols-[minmax(13rem,2fr)_8rem_5.5rem_5.5rem_4.75rem_5.5rem]'

  return (
    <div className="grid h-full min-h-0 min-w-0 gap-3 xl:grid-cols-[minmax(0,1fr)_clamp(19rem,22vw,23rem)]">
      <section className="flex min-h-0 min-w-0 flex-col overflow-hidden rounded-2xl border border-line bg-surface-raised shadow-[var(--theme-panel-shadow)]">
        <div className="flex shrink-0 flex-col gap-3 border-b border-line p-3">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div
              aria-label="Habit status"
              className="flex items-center gap-5"
              role="tablist"
            >
              <button
                aria-selected={activeTab === 'active'}
                className={[
                  'relative flex min-h-10 items-center gap-2 px-1 text-sm font-semibold transition',
                  activeTab === 'active'
                    ? 'text-content'
                    : 'text-content-muted hover:text-content',
                ].join(' ')}
                role="tab"
                type="button"
                onClick={() => changeTab('active')}
              >
                Active Habits
                <span className="rounded-full border border-accent/25 bg-accent-soft px-2 py-0.5 text-[9px] font-bold text-accent">
                  {activeHabits.length}
                </span>
                {activeTab === 'active' && (
                  <span
                    aria-hidden="true"
                    className="absolute inset-x-0 bottom-0 h-0.5 rounded-full bg-accent shadow-[var(--theme-energy-shadow)]"
                  />
                )}
              </button>

              <button
                aria-selected={activeTab === 'inactive'}
                className={[
                  'relative flex min-h-10 items-center gap-2 px-1 text-sm font-semibold transition',
                  activeTab === 'inactive'
                    ? 'text-content'
                    : 'text-content-muted hover:text-content',
                ].join(' ')}
                role="tab"
                type="button"
                onClick={() => changeTab('inactive')}
              >
                Inactive Habits
                <span className="rounded-full border border-line bg-surface px-2 py-0.5 text-[9px] font-bold text-content-muted">
                  {inactiveHabits.length}
                </span>
                {activeTab === 'inactive' && (
                  <span
                    aria-hidden="true"
                    className="absolute inset-x-0 bottom-0 h-0.5 rounded-full bg-accent shadow-[var(--theme-energy-shadow)]"
                  />
                )}
              </button>
            </div>

            <div className="flex min-w-0 flex-1 items-center justify-end gap-2 sm:flex-none">
              <label className="relative min-w-0 flex-1 sm:w-56">
                <span className="sr-only">Search habits</span>

                <Search
                  aria-hidden="true"
                  className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-content-subtle"
                  size={15}
                />

                <input
                  className="h-10 w-full rounded-xl border border-line bg-surface pl-9 pr-3 text-xs text-content outline-none transition focus:border-accent/55 focus:ring-2 focus:ring-accent/15"
                  placeholder="Search habits..."
                  type="search"
                  value={searchQuery}
                  onChange={(event) => setSearchQuery(event.target.value)}
                />
              </label>

              <button
                className="flex h-10 shrink-0 items-center gap-2 rounded-xl border border-accent/45 bg-accent px-4 text-xs font-bold text-white transition hover:brightness-110"
                type="button"
                onClick={() => setInspectorMode('create')}
              >
                <Plus aria-hidden="true" size={15} />

                <span className="hidden sm:inline">Create Habit</span>
              </button>
            </div>
          </div>

          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex flex-1 flex-wrap gap-2">
              <label>
                <span className="sr-only">Filter by category</span>

                <select
                  className="h-9 rounded-xl border border-line bg-surface px-3 text-[10px] text-content-muted outline-none focus:border-accent/55"
                  value={categoryFilter}
                  onChange={(event) =>
                    setCategoryFilter(
                      event.target.value as HabitCategory | 'all',
                    )
                  }
                >
                  <option value="all">All categories</option>

                  {habitCategoryOptions.map((categoryOption) => (
                    <option
                      key={categoryOption.value}
                      value={categoryOption.value}
                    >
                      {categoryOption.label}
                    </option>
                  ))}
                </select>
              </label>

              <label>
                <span className="sr-only">Filter by frequency</span>

                <select
                  className="h-9 rounded-xl border border-line bg-surface px-3 text-[10px] text-content-muted outline-none focus:border-accent/55"
                  value={frequencyFilter}
                  onChange={(event) =>
                    setFrequencyFilter(
                      event.target.value as HabitFrequencyType | 'all',
                    )
                  }
                >
                  <option value="all">All frequencies</option>
                  <option value="daily">Daily</option>
                  <option value="weekly">Weekly</option>
                </select>
              </label>

              <label>
                <span className="sr-only">Filter by difficulty</span>

                <select
                  className="h-9 rounded-xl border border-line bg-surface px-3 text-[10px] text-content-muted outline-none focus:border-accent/55"
                  value={difficultyFilter}
                  onChange={(event) =>
                    setDifficultyFilter(
                      event.target.value as HabitDifficulty | 'all',
                    )
                  }
                >
                  <option value="all">All difficulties</option>
                  <option value="easy">Easy</option>
                  <option value="medium">Medium</option>
                  <option value="hard">Hard</option>
                  <option value="elite">Elite</option>
                </select>
              </label>
            </div>

            <label className="flex items-center gap-2">
              <span className="text-[9px] font-semibold text-content-subtle">
                Sort by
              </span>

              <select
                className="h-9 rounded-xl border border-line bg-surface px-3 text-[10px] text-content-muted outline-none focus:border-accent/55"
                value={sortOption}
                onChange={(event) =>
                  setSortOption(event.target.value as HabitSortOption)
                }
              >
                <option value="nameAsc">Name (A–Z)</option>
                <option value="nameDesc">Name (Z–A)</option>
                <option value="streakDesc">Highest streak</option>
                <option value="rewardDesc">Highest XP reward</option>
                <option value="newest">Newest first</option>
              </select>
            </label>
          </div>
        </div>

        <div className="flex min-h-0 flex-1 flex-col p-2">
          <div className="flex shrink-0 items-center border-b border-line px-2 py-2">
            <div className={`${tableGridClassName} min-w-0 flex-1`}>
              <span className="text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
                Habit
              </span>

              <span className="hidden text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase min-[1500px]:block">
                Category
              </span>

              <span className="text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
                Frequency
              </span>

              <span className="text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
                Difficulty
              </span>

              <span className="hidden text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase min-[1500px]:block">
                Streak
              </span>

              <span className="text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
                XP reward
              </span>
            </div>

            <span className="w-[3.25rem] shrink-0 text-center text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
              Today
            </span>
          </div>

          {isWaitingForInitialData && (
            <div className="grid min-h-0 flex-1 place-items-center">
              <div className="text-center">
                <LoaderCircle
                  aria-hidden="true"
                  className="mx-auto animate-spin text-accent motion-reduce:animate-none"
                  size={22}
                />

                <p className="mt-2 text-sm font-semibold">Loading habits...</p>
              </div>
            </div>
          )}

          {errorMessage && (
            <div
              className="m-2 rounded-xl border border-danger/30 bg-danger/10 px-4 py-3"
              role="alert"
            >
              <p className="text-sm font-semibold text-danger">
                Habit loading error: {errorMessage}
              </p>
            </div>
          )}

          {!isWaitingForInitialData &&
            !errorMessage &&
            filteredHabits.length === 0 && (
              <div className="grid min-h-0 flex-1 place-items-center px-6 text-center">
                <div>
                  <div className="mx-auto grid size-11 place-items-center rounded-xl border border-line bg-surface text-content-subtle">
                    <Target aria-hidden="true" size={20} />
                  </div>

                  <p className="mt-3 text-sm font-semibold">
                    {activeTab === 'active'
                      ? 'You do not have any habits yet.'
                      : 'No inactive habits found.'}
                  </p>

                  <p className="mt-1 text-[10px] text-content-muted">
                    {searchQuery ||
                    categoryFilter !== 'all' ||
                    frequencyFilter !== 'all' ||
                    difficultyFilter !== 'all'
                      ? 'Adjust the filters to reveal more records.'
                      : activeTab === 'active'
                        ? 'Create a progression path to begin.'
                        : 'Deactivated habits will appear here.'}
                  </p>
                </div>
              </div>
            )}

          {!errorMessage && visibleHabits.length > 0 && (
            <ul className="grid min-h-0 flex-1 auto-rows-fr gap-1.5 py-1.5">
              {visibleHabits.map((habit) => {
                const dashboardHabit = dashboardHabitById.get(habit.id)

                const primaryReward = dashboardHabit?.attributeRewards[0]

                const visual = primaryReward
                  ? getAttributeVisual(primaryReward.attributeType)
                  : null

                const HabitIcon: LucideIcon =
                  visual?.Icon ??
                  (habit.frequencyType === 'daily' ? CalendarDays : Repeat2)

                const accentColor =
                  visual?.colorVariable ?? getDifficultyColor(habit.difficulty)

                const isSelected =
                  habit.id === selectedHabitId && inspectorMode === 'details'

                const rowStyle = {
                  '--row-accent': accentColor,
                  borderColor: isSelected
                    ? 'color-mix(in srgb, var(--theme-accent-primary) 78%, var(--theme-border))'
                    : 'var(--theme-border)',
                  backgroundImage: isSelected
                    ? 'linear-gradient(90deg, color-mix(in srgb, var(--theme-accent-primary) 9%, transparent), transparent 72%)'
                    : 'none',
                  boxShadow: isSelected
                    ? '0 0 18px color-mix(in srgb, var(--theme-accent-primary) 14%, transparent), inset 0 0 0 1px color-mix(in srgb, var(--theme-accent-primary) 20%, transparent)'
                    : 'none',
                } as CSSProperties

                return (
                  <li
                    className="flex h-full min-h-[4.25rem] min-w-0 overflow-visible rounded-xl border bg-surface transition-colors"
                    key={habit.id}
                    style={rowStyle}
                  >
                    <button
                      aria-pressed={isSelected}
                      className={`${tableGridClassName} min-w-0 flex-1 px-3 text-left outline-none transition hover:bg-surface-hover/60`}
                      type="button"
                      onClick={() => {
                        setSelectedHabitId(habit.id)
                        setInspectorMode('details')
                      }}
                    >
                      <div className="flex min-w-0 items-center gap-2.5">
                        <div
                          className="grid size-9 shrink-0 place-items-center rounded-xl border"
                          style={{
                            borderColor:
                              'color-mix(in srgb, var(--row-accent) 38%, transparent)',
                            backgroundColor:
                              'color-mix(in srgb, var(--row-accent) 11%, transparent)',
                            color: 'var(--row-accent)',
                          }}
                        >
                          <HabitIcon aria-hidden="true" size={16} />
                        </div>

                        <div className="min-w-0">
                          <h3 className="truncate text-xs font-semibold">
                            {habit.name}
                          </h3>

                          <p className="mt-0.5 truncate text-[9px] text-content-subtle">
                            {habit.description ?? 'No description'}
                          </p>
                        </div>
                      </div>

                      <div className="hidden min-w-0 min-[1500px]:block">
                        <p className="truncate text-[10px] text-content-muted">
                          {getHabitCategoryLabel(habit.category)}
                        </p>

                        <span className="sr-only">
                          Category: {getHabitCategoryLabel(habit.category)}
                        </span>
                      </div>

                      <div>
                        <p className="text-[10px] text-content-muted">
                          {getFrequencyLabel(habit)}
                        </p>

                        <span className="sr-only">
                          Frequency:{' '}
                          {habit.frequencyType === 'daily'
                            ? 'Daily'
                            : `${habit.targetCount} times per week`}
                        </span>
                      </div>

                      <DifficultyIndicator
                        compact
                        difficulty={habit.difficulty}
                      />

                      <div className="hidden min-[1500px]:block">
                        <p className="text-sm font-bold text-streak">
                          {dashboardHabit?.currentStreak ?? '—'}
                        </p>

                        <p className="text-[8px] text-content-subtle">
                          {getStreakUnit(habit.frequencyType)}
                        </p>
                      </div>

                      <div className="flex items-center gap-1 text-[10px] font-semibold text-energy-blue">
                        <Zap aria-hidden="true" size={12} />

                        {dashboardHabit
                          ? `+${getRewardTotal(dashboardHabit)} XP`
                          : '—'}
                      </div>
                    </button>

                    <div className="grid w-[3.25rem] shrink-0 place-items-center">
                      {habit.isActive ? (
                        <CompletionControl
                          habit={habit}
                          variant="circle"
                          onCompletionStatusChanged={
                            handleCompletionStatusChanged
                          }
                          onProgressChanged={onProgressChanged}
                        />
                      ) : (
                        <CircleOff
                          aria-label="Inactive habit"
                          className="text-content-subtle"
                          size={17}
                        />
                      )}
                    </div>
                  </li>
                )
              })}
            </ul>
          )}

          <footer className="flex min-h-10 shrink-0 items-center justify-between gap-3 border-t border-line px-2 pt-2">
            <p className="text-[9px] text-content-subtle">
              {rangeStart}–{rangeEnd} of {filteredHabits.length}{' '}
              {filteredHabits.length === 1 ? 'habit' : 'habits'}
            </p>

            <div className="flex items-center gap-1">
              <button
                aria-label="Previous habits page"
                className="grid size-8 place-items-center rounded-lg border border-line bg-surface text-content-muted disabled:cursor-not-allowed disabled:opacity-35"
                disabled={currentPage <= 1}
                type="button"
                onClick={() => setPage((current) => Math.max(1, current - 1))}
              >
                <ChevronLeft aria-hidden="true" size={14} />
              </button>

              <span className="min-w-8 text-center text-[10px] font-semibold text-content-muted">
                {currentPage} / {pageCount}
              </span>

              <button
                aria-label="Next habits page"
                className="grid size-8 place-items-center rounded-lg border border-line bg-surface text-content-muted disabled:cursor-not-allowed disabled:opacity-35"
                disabled={currentPage >= pageCount}
                type="button"
                onClick={() =>
                  setPage((current) => Math.min(pageCount, current + 1))
                }
              >
                <ChevronRight aria-hidden="true" size={14} />
              </button>

              {isRefreshing && (
                <span className="ml-2 text-[9px] text-content-subtle">
                  Syncing...
                </span>
              )}
            </div>
          </footer>
        </div>
      </section>

      <HabitInspector
        dashboardHabit={selectedDashboardHabit}
        habit={selectedHabit}
        mode={inspectorMode}
        onCancelMode={() => setInspectorMode('details')}
        onClose={() => setSelectedHabitId(null)}
        onCompletionStatusChanged={handleCompletionStatusChanged}
        onHabitActivated={handleHabitActivated}
        onHabitCreated={handleHabitCreated}
        onHabitDeactivated={handleHabitDeactivated}
        onHabitUpdated={handleHabitUpdated}
        onProgressChanged={onProgressChanged}
        onStartEdit={() => setInspectorMode('edit')}
      />
    </div>
  )
}
