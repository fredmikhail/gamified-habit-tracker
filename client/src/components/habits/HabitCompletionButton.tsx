import { useState } from 'react'
import { completeHabit, undoHabitCompletion } from '../../api/habitsApi'
import type { AttributeType } from '../../types/AttributeType'
import type { HabitAttributeRewardResponse } from '../../types/HabitAttributeRewardResponse'
import type { HabitResponse } from '../../types/HabitResponse'

type HabitCompletionButtonProps = {
  habit: HabitResponse
  onProgressChanged?: () => void
  onCompletionStatusChanged: (habit: HabitResponse) => void
}

function getHabitErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown habit-completion error occurred.'
}

function getAttributeLabel(attributeType: AttributeType): string {
  return attributeType.charAt(0).toUpperCase() + attributeType.slice(1)
}

export function HabitCompletionButton({
  habit,
  onCompletionStatusChanged,
  onProgressChanged,
}: HabitCompletionButtonProps) {
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

        onProgressChanged?.()
      } else {
        const response = await completeHabit(habit.id, {
          notes: null,
        })

        setEarnedRewards(response.rewards)

        onCompletionStatusChanged({
          ...habit,
          isCompletedToday: true,
        })

        onProgressChanged?.()
      }
    } catch (error) {
      setErrorMessage(getHabitErrorMessage(error))
    } finally {
      setIsSaving(false)
    }
  }

  const buttonText = isSaving
    ? habit.isCompletedToday
      ? 'Undoing...'
      : 'Completing...'
    : habit.isCompletedToday
      ? 'Undo completion'
      : 'Mark complete'

  const buttonClassName = habit.isCompletedToday
    ? 'min-h-11 rounded border border-green-300 bg-green-50 px-4 py-2 font-semibold text-green-800 disabled:cursor-not-allowed disabled:opacity-50'
    : 'min-h-11 rounded bg-green-700 px-4 py-2 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50'

  return (
    <div className="mt-4">
      <button
        aria-pressed={habit.isCompletedToday}
        className={buttonClassName}
        disabled={isSaving}
        type="button"
        onClick={() => void handleCompletionToggle()}
      >
        {buttonText}
      </button>

      {earnedRewards && (
        <div
          className="mt-3 rounded-lg border border-green-200 bg-green-50 p-3 text-green-900"
          role="status"
        >
          <p className="font-semibold">Habit completed!</p>

          <ul className="mt-1 flex flex-wrap gap-x-4 gap-y-1 text-sm">
            {earnedRewards.map((reward) => (
              <li key={reward.attributeType}>
                +{reward.xpAmount} {getAttributeLabel(reward.attributeType)} XP
              </li>
            ))}
          </ul>
        </div>
      )}

      {errorMessage && (
        <p className="mt-3 text-red-700" role="alert">
          Habit completion error: {errorMessage}
        </p>
      )}
    </div>
  )
}
