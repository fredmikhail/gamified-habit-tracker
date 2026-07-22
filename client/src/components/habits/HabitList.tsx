import { useEffect, useState } from 'react'
import type { HabitResponse } from '../../types/HabitResponse'
import { useWorkspaceData } from '../../workspace/useWorkspaceData'
import { HabitCompletionButton } from './HabitCompletionButton'
import { HabitDeactivateButton } from './HabitDeactivateButton'
import { HabitEditForm } from './HabitEditForm'
import { getHabitCategoryLabel } from './habitCategoryOptions'

type HabitListProps = {
  onHabitUpdated: (habit: HabitResponse) => void
  onHabitDeactivated: (habit: HabitResponse) => void
  onProgressChanged: () => void
}

type FrequencyConfiguration = {
  frequencyType: HabitResponse['frequencyType']
  targetCount: number
}

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

function formatLabel(value: string): string {
  return value.charAt(0).toUpperCase() + value.slice(1)
}

function formatDateOnly(value: string): string {
  const [year, month, day] = value.split('-').map(Number)

  return `${monthNames[month - 1]} ${day}, ${year}`
}

function getFrequencyLabel(configuration: FrequencyConfiguration): string {
  if (configuration.frequencyType === 'daily') {
    return 'Daily'
  }

  return `${configuration.targetCount} times per week`
}

export function HabitList({
  onHabitUpdated,
  onHabitDeactivated,
  onProgressChanged,
}: HabitListProps) {
  const { habitsResource } = useWorkspaceData()

  const {
    data: habits,
    errorMessage,
    isRefreshing,
    ensureLoaded,
    updateData: updateHabits,
  } = habitsResource

  const [editingHabitId, setEditingHabitId] = useState<string | null>(null)

  useEffect(() => {
    void ensureLoaded()
  }, [ensureLoaded])

  const isWaitingForInitialData = habits === null && !errorMessage

  function replaceCachedHabit(updatedHabit: HabitResponse): void {
    updateHabits((currentHabits) => {
      if (currentHabits === null) {
        return [updatedHabit]
      }

      return currentHabits.map((habit) =>
        habit.id === updatedHabit.id ? updatedHabit : habit,
      )
    })
  }

  function handleHabitUpdated(updatedHabit: HabitResponse): void {
    replaceCachedHabit(updatedHabit)
    setEditingHabitId(null)
    onHabitUpdated(updatedHabit)
  }

  function handleCompletionStatusChanged(updatedHabit: HabitResponse): void {
    replaceCachedHabit(updatedHabit)
  }

  function handleHabitDeactivated(deactivatedHabit: HabitResponse): void {
    updateHabits((currentHabits) => {
      if (currentHabits === null) {
        return []
      }

      return currentHabits.filter((habit) => habit.id !== deactivatedHabit.id)
    })

    onHabitDeactivated(deactivatedHabit)
  }

  return (
    <section
      aria-labelledby="habit-list-heading"
      className="mt-8 rounded-lg bg-white p-6 text-left shadow-sm"
    >
      <h2 id="habit-list-heading" className="text-2xl font-bold">
        Habits
      </h2>

      {isWaitingForInitialData && (
        <p className="mt-4 text-slate-600">Loading habits...</p>
      )}

      {isRefreshing && (
        <p className="mt-4 text-sm text-slate-500" role="status">
          Refreshing habits...
        </p>
      )}

      {errorMessage && (
        <p className="mt-4 text-red-700" role="alert">
          Habit loading error: {errorMessage}
        </p>
      )}

      {habits !== null && habits.length === 0 && (
        <p className="mt-4 text-slate-600">You do not have any habits yet.</p>
      )}

      {habits !== null && habits.length > 0 && (
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

                <p>Category: {getHabitCategoryLabel(habit.category)}</p>
              </div>

              {habit.pendingConfiguration && (
                <section
                  aria-label={`Scheduled changes for ${habit.name}`}
                  className="mt-4 rounded border border-amber-200 bg-amber-50 p-3 text-sm text-amber-950"
                >
                  <p className="font-semibold">
                    Scheduled for{' '}
                    {formatDateOnly(
                      habit.pendingConfiguration.effectiveFromDate,
                    )}
                  </p>

                  <div className="mt-2 flex flex-wrap gap-x-4 gap-y-2">
                    <p>
                      Frequency: {getFrequencyLabel(habit.pendingConfiguration)}
                    </p>

                    <p>
                      Category:{' '}
                      {getHabitCategoryLabel(
                        habit.pendingConfiguration.category,
                      )}
                    </p>

                    <p>
                      Difficulty:{' '}
                      {formatLabel(habit.pendingConfiguration.difficulty)}
                    </p>
                  </div>
                </section>
              )}

              <HabitCompletionButton
                habit={habit}
                onCompletionStatusChanged={handleCompletionStatusChanged}
                onProgressChanged={onProgressChanged}
              />

              {editingHabitId === habit.id ? (
                <HabitEditForm
                  habit={habit}
                  onCancel={() => setEditingHabitId(null)}
                  onHabitUpdated={handleHabitUpdated}
                />
              ) : (
                <div className="flex flex-wrap items-start gap-3">
                  <button
                    className="mt-4 min-h-11 rounded border border-slate-300 px-4 py-2 font-semibold"
                    type="button"
                    onClick={() => setEditingHabitId(habit.id)}
                  >
                    Edit
                  </button>

                  <HabitDeactivateButton
                    habit={habit}
                    onHabitDeactivated={handleHabitDeactivated}
                  />
                </div>
              )}
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}
