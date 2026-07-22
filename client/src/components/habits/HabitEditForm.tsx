import { useState, type FormEvent } from 'react'
import { updateHabit } from '../../api/habitsApi'
import type { HabitCategory } from '../../types/HabitCategory'
import type { HabitDifficulty } from '../../types/HabitDifficulty'
import type { HabitFrequencyType } from '../../types/HabitFrequencyType'
import type { HabitResponse } from '../../types/HabitResponse'
import { habitCategoryOptions } from './habitCategoryOptions'

type HabitEditFormProps = {
  habit: HabitResponse
  onHabitUpdated: (habit: HabitResponse) => void
  onCancel: () => void
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

function formatDateOnly(value: string): string {
  const [year, month, day] = value.split('-').map(Number)

  return `${monthNames[month - 1]} ${day}, ${year}`
}

function getHabitErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown habit-update error occurred.'
}

export function HabitEditForm({
  habit,
  onHabitUpdated,
  onCancel,
}: HabitEditFormProps) {
  const editableConfiguration = habit.pendingConfiguration ?? habit

  const [name, setName] = useState(habit.name)
  const [description, setDescription] = useState(habit.description ?? '')
  const [category, setCategory] = useState<HabitCategory>(
    editableConfiguration.category,
  )
  const [frequencyType, setFrequencyType] = useState<HabitFrequencyType>(
    editableConfiguration.frequencyType,
  )
  const [targetCount, setTargetCount] = useState(
    editableConfiguration.targetCount,
  )
  const [difficulty, setDifficulty] = useState<HabitDifficulty>(
    editableConfiguration.difficulty,
  )

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  function changeFrequencyType(value: HabitFrequencyType) {
    setFrequencyType(value)

    if (value === 'daily') {
      setTargetCount(1)
    }
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setIsSubmitting(true)
    setErrorMessage(null)

    try {
      const updatedHabit = await updateHabit(habit.id, {
        name,
        description: description === '' ? null : description,
        category,
        frequencyType,
        targetCount,
        difficulty,
      })

      onHabitUpdated(updatedHabit)
    } catch (error) {
      setErrorMessage(getHabitErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form
      aria-label={`Edit ${habit.name}`}
      className="mt-4 space-y-4 rounded-lg border border-slate-300 bg-slate-50 p-4"
      onSubmit={handleSubmit}
    >
      <div className="rounded border border-slate-200 bg-white p-3 text-sm text-slate-700">
        <p>
          Name and description update immediately. Category, frequency, target,
          and difficulty changes begin at the next week boundary.
        </p>

        {habit.pendingConfiguration && (
          <p className="mt-2 font-medium text-amber-800" role="status">
            Scheduled rule changes take effect on{' '}
            {formatDateOnly(habit.pendingConfiguration.effectiveFromDate)}. The
            rule fields below show those scheduled values.
          </p>
        )}
      </div>

      <div>
        <label className="block font-medium" htmlFor={`habit-name-${habit.id}`}>
          Name
        </label>

        <input
          required
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id={`habit-name-${habit.id}`}
          maxLength={100}
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
        />
      </div>

      <div>
        <label
          className="block font-medium"
          htmlFor={`habit-description-${habit.id}`}
        >
          Description
        </label>

        <textarea
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id={`habit-description-${habit.id}`}
          maxLength={500}
          rows={3}
          value={description}
          onChange={(event) => setDescription(event.target.value)}
        />
      </div>

      <div>
        <label
          className="block font-medium"
          htmlFor={`habit-category-${habit.id}`}
        >
          Category
        </label>

        <select
          required
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id={`habit-category-${habit.id}`}
          value={category}
          onChange={(event) => setCategory(event.target.value as HabitCategory)}
        >
          {habitCategoryOptions.map((categoryOption) => (
            <option key={categoryOption.value} value={categoryOption.value}>
              {categoryOption.label}
            </option>
          ))}
        </select>
      </div>

      <div>
        <label
          className="block font-medium"
          htmlFor={`habit-frequency-${habit.id}`}
        >
          Frequency
        </label>

        <select
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id={`habit-frequency-${habit.id}`}
          value={frequencyType}
          onChange={(event) =>
            changeFrequencyType(event.target.value as HabitFrequencyType)
          }
        >
          <option value="daily">Daily</option>
          <option value="weekly">Weekly</option>
        </select>
      </div>

      {frequencyType === 'weekly' && (
        <div>
          <label
            className="block font-medium"
            htmlFor={`habit-target-count-${habit.id}`}
          >
            Times per week
          </label>

          <input
            required
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
            id={`habit-target-count-${habit.id}`}
            max={7}
            min={1}
            type="number"
            value={targetCount}
            onChange={(event) => setTargetCount(Number(event.target.value))}
          />
        </div>
      )}

      <div>
        <label
          className="block font-medium"
          htmlFor={`habit-difficulty-${habit.id}`}
        >
          Difficulty
        </label>

        <select
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id={`habit-difficulty-${habit.id}`}
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

      <div className="flex gap-3">
        <button
          className="flex-1 rounded bg-slate-900 px-4 py-2 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isSubmitting}
          type="submit"
        >
          {isSubmitting ? 'Saving changes...' : 'Save changes'}
        </button>

        <button
          className="rounded border border-slate-300 px-4 py-2 font-semibold disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isSubmitting}
          type="button"
          onClick={onCancel}
        >
          Cancel
        </button>
      </div>

      {errorMessage && (
        <p className="text-red-700" role="alert">
          Habit update error: {errorMessage}
        </p>
      )}
    </form>
  )
}
