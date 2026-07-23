import { Check, LoaderCircle, RotateCcw } from 'lucide-react'
import { useState } from 'react'
import { completeHabit, undoHabitCompletion } from '../../api/habitsApi'
import type { HabitAttributeRewardResponse } from '../../types/HabitAttributeRewardResponse'
import type { DashboardHabitResponse } from '../../types/DashboardHabitResponse'
import { getAttributeVisual } from '../attributes/attributeVisuals'

type DashboardHabitActionProps = {
  habit: DashboardHabitResponse
  onCompletionStatusChanged: (
    habitId: string,
    isCompletedToday: boolean,
  ) => void
}

function getErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown completion error occurred.'
}

export function DashboardHabitAction({
  habit,
  onCompletionStatusChanged,
}: DashboardHabitActionProps) {
  const [isSaving, setIsSaving] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  const [earnedRewards, setEarnedRewards] = useState<
    HabitAttributeRewardResponse[] | null
  >(null)

  async function handleComplete(): Promise<void> {
    setIsSaving(true)
    setErrorMessage(null)

    try {
      const response = await completeHabit(habit.id, {
        notes: null,
      })

      setEarnedRewards(response.rewards)

      onCompletionStatusChanged(habit.id, true)
    } catch (error) {
      setErrorMessage(getErrorMessage(error))
    } finally {
      setIsSaving(false)
    }
  }

  async function handleUndo(): Promise<void> {
    setIsSaving(true)
    setErrorMessage(null)

    try {
      await undoHabitCompletion(habit.id)

      setEarnedRewards(null)

      onCompletionStatusChanged(habit.id, false)
    } catch (error) {
      setErrorMessage(getErrorMessage(error))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="shrink-0">
      <div className="flex items-center justify-end gap-1.5">
        <button
          aria-label={
            habit.isCompletedToday
              ? `${habit.name} completed`
              : `Complete ${habit.name}`
          }
          aria-pressed={habit.isCompletedToday}
          className={[
            'inline-flex min-h-8 items-center justify-center gap-1.5 rounded-lg border px-2.5 py-1 text-[11px] font-semibold transition-colors disabled:cursor-not-allowed disabled:opacity-60',
            habit.isCompletedToday
              ? 'border-success/30 bg-success/10 text-success'
              : 'border-accent/40 bg-accent-soft text-accent hover:border-accent hover:bg-accent hover:text-white',
          ].join(' ')}
          disabled={isSaving || habit.isCompletedToday}
          type="button"
          onClick={() => void handleComplete()}
        >
          {isSaving && !habit.isCompletedToday ? (
            <LoaderCircle
              aria-hidden="true"
              className="animate-spin"
              size={13}
            />
          ) : (
            <Check aria-hidden="true" size={13} strokeWidth={2} />
          )}

          {habit.isCompletedToday
            ? 'Completed'
            : isSaving
              ? 'Completing'
              : 'Complete'}
        </button>

        {habit.isCompletedToday && (
          <button
            aria-label={`Undo completion for ${habit.name}`}
            className="grid size-8 place-items-center rounded-lg border border-line bg-surface text-content-muted transition-colors hover:border-warning/40 hover:text-warning disabled:cursor-not-allowed disabled:opacity-50"
            disabled={isSaving}
            type="button"
            onClick={() => void handleUndo()}
          >
            {isSaving ? (
              <LoaderCircle
                aria-hidden="true"
                className="animate-spin"
                size={13}
              />
            ) : (
              <RotateCcw aria-hidden="true" size={13} strokeWidth={1.9} />
            )}
          </button>
        )}
      </div>

      {earnedRewards && (
        <p
          className="mt-1.5 max-w-44 text-right text-[9px] leading-4 text-success"
          role="status"
        >
          {earnedRewards
            .map((reward) => {
              const visual = getAttributeVisual(reward.attributeType)

              return `+${reward.xpAmount} ${visual.label}`
            })
            .join(' · ')}
        </p>
      )}

      {errorMessage && (
        <p
          className="mt-1.5 max-w-48 text-right text-[9px] leading-4 text-danger"
          role="alert"
        >
          {errorMessage}
        </p>
      )}
    </div>
  )
}
