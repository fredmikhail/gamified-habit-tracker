import { LoaderCircle, Power } from 'lucide-react'
import { useState } from 'react'
import { activateHabit } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'

type HabitActivateButtonProps = {
  habit: HabitResponse
  onHabitActivated: (habit: HabitResponse) => void
}

function getActivationErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown habit-activation error occurred.'
}

export function HabitActivateButton({
  habit,
  onHabitActivated,
}: HabitActivateButtonProps) {
  const [isActivating, setIsActivating] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  async function handleActivate() {
    setIsActivating(true)
    setErrorMessage(null)

    try {
      const activatedHabit = await activateHabit(habit.id)

      onHabitActivated(activatedHabit)
    } catch (error) {
      setErrorMessage(getActivationErrorMessage(error))
    } finally {
      setIsActivating(false)
    }
  }

  return (
    <div>
      <button
        className="flex min-h-11 w-full items-center justify-center gap-2 rounded-xl border border-success/35 bg-success/10 px-4 py-2.5 text-sm font-bold text-success transition hover:border-success/55 hover:bg-success/15 disabled:cursor-not-allowed disabled:opacity-50"
        disabled={isActivating}
        type="button"
        onClick={() => void handleActivate()}
      >
        {isActivating ? (
          <LoaderCircle
            aria-hidden="true"
            className="animate-spin motion-reduce:animate-none"
            size={15}
          />
        ) : (
          <Power aria-hidden="true" size={15} />
        )}

        {isActivating ? 'Reactivating...' : 'Reactivate habit'}
      </button>

      {errorMessage && (
        <p
          className="mt-2 rounded-xl border border-danger/30 bg-danger/10 px-3 py-2 text-[10px] text-danger"
          role="alert"
        >
          Habit activation error: {errorMessage}
        </p>
      )}
    </div>
  )
}
