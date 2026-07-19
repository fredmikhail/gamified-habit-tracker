import { useState } from 'react'
import { deactivateHabit } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'

type HabitDeactivateButtonProps = {
  habit: HabitResponse
  onHabitDeactivated: (habit: HabitResponse) => void
}

function getHabitErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown habit-deactivation error occurred.'
}

export function HabitDeactivateButton({
  habit,
  onHabitDeactivated,
}: HabitDeactivateButtonProps) {
  const [isConfirming, setIsConfirming] = useState(false)
  const [isDeactivating, setIsDeactivating] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  function startConfirmation() {
    setIsConfirming(true)
    setErrorMessage(null)
  }

  function cancelConfirmation() {
    setIsConfirming(false)
    setErrorMessage(null)
  }

  async function handleDeactivate() {
    setIsDeactivating(true)
    setErrorMessage(null)

    try {
      const deactivatedHabit = await deactivateHabit(habit.id)

      setIsConfirming(false)
      onHabitDeactivated(deactivatedHabit)
    } catch (error) {
      setErrorMessage(getHabitErrorMessage(error))
    } finally {
      setIsDeactivating(false)
    }
  }

  return (
    <div className="mt-4">
      {!isConfirming ? (
        <button
          className="rounded border border-red-300 px-4 py-2 font-semibold text-red-700"
          type="button"
          onClick={startConfirmation}
        >
          Deactivate
        </button>
      ) : (
        <div className="rounded border border-red-200 bg-red-50 p-4">
          <p className="font-medium">Deactivate &quot;{habit.name}&quot;?</p>

          <p className="mt-1 text-sm text-slate-600">
            It will no longer appear in your active habit list.
          </p>

          <div className="mt-3 flex flex-wrap gap-3">
            <button
              className="rounded bg-red-700 px-4 py-2 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
              disabled={isDeactivating}
              type="button"
              onClick={() => void handleDeactivate()}
            >
              {isDeactivating ? 'Deactivating...' : 'Confirm deactivation'}
            </button>

            <button
              className="rounded border border-slate-300 px-4 py-2 font-semibold disabled:cursor-not-allowed disabled:opacity-50"
              disabled={isDeactivating}
              type="button"
              onClick={cancelConfirmation}
            >
              Keep habit
            </button>
          </div>
        </div>
      )}

      {errorMessage && (
        <p className="mt-3 text-red-700" role="alert">
          Habit deactivation error: {errorMessage}
        </p>
      )}
    </div>
  )
}
