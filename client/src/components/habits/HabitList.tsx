import { useEffect, useState } from 'react'
import { getHabits } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitEditForm } from './HabitEditForm'

type HabitListProps = {
  refreshKey: number
  onHabitUpdated: (habit: HabitResponse) => void
}

function formatLabel(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1)
}

function getFrequencyLabel(habit: HabitResponse): string {
  if (habit.frequencyType === 'daily') {
    return 'Daily'
  }

  return `${habit.targetCount} times per week`
}

function getHabitErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown habit-loading error occurred.'
}

export function HabitList({ refreshKey, onHabitUpdated }: HabitListProps) {
  const [habits, setHabits] = useState<HabitResponse[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [editingHabitId, setEditingHabitId] = useState<string | null>(null)

  useEffect(() => {
    let isActive = true

    async function loadHabits() {
      try {
        const loadedHabits = await getHabits()

        if (isActive) {
          setHabits(loadedHabits)
          setErrorMessage(null)
        }
      } catch (error) {
        if (isActive) {
          setHabits([])
          setErrorMessage(getHabitErrorMessage(error))
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadHabits()

    return () => {
      isActive = false
    }
  }, [refreshKey])

  function handleHabitUpdated(updatedHabit: HabitResponse) {
    setEditingHabitId(null)
    onHabitUpdated(updatedHabit)
  }

  return (
    <section
      aria-labelledby="habit-list-heading"
      className="mt-8 rounded-lg bg-white p-6 text-left shadow-sm"
    >
      <h2 id="habit-list-heading" className="text-2xl font-bold">
        Habits
      </h2>

      {isLoading && <p className="mt-4 text-slate-600">Loading habits...</p>}

      {!isLoading && errorMessage && (
        <p className="mt-4 text-red-700">Habit loading error: {errorMessage}</p>
      )}

      {!isLoading && !errorMessage && habits.length === 0 && (
        <p className="mt-4 text-slate-600">You do not have any habits yet.</p>
      )}

      {!isLoading && !errorMessage && habits.length > 0 && (
        <ul className="mt-4 space-y-4">
          {habits.map((habit) => (
            <li
              key={habit.id}
              className="rounded-lg border border-slate-200 p-4"
            >
              <div className="flex items-start justify-between gap-4">
                <h3 className="text-lg font-semibold">{habit.name}</h3>

                <span className="rounded bg-slate-100 px-2 py-1 text-sm font-medium">
                  {formatLabel(habit.difficulty)}
                </span>
              </div>

              {habit.description && (
                <p className="mt-2 text-slate-600">{habit.description}</p>
              )}

              <div className="mt-3 flex flex-wrap gap-x-4 gap-y-2 text-sm text-slate-600">
                <p>Frequency: {getFrequencyLabel(habit)}</p>

                {habit.category && <p>Category: {habit.category}</p>}
              </div>

              {editingHabitId === habit.id ? (
                <HabitEditForm
                  habit={habit}
                  onCancel={() => setEditingHabitId(null)}
                  onHabitUpdated={handleHabitUpdated}
                />
              ) : (
                <button
                  className="mt-4 rounded border border-slate-300 px-4 py-2 font-semibold"
                  type="button"
                  onClick={() => setEditingHabitId(habit.id)}
                >
                  Edit
                </button>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
