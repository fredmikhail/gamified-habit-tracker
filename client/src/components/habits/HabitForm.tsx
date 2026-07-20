import { useState, type FormEvent } from 'react'
import { createHabit } from '../../api/habitsApi'
import type { HabitCategory } from '../../types/HabitCategory'
import type { HabitDifficulty } from '../../types/HabitDifficulty'
import type { HabitFrequencyType } from '../../types/HabitFrequencyType'
import type { HabitResponse } from '../../types/HabitResponse'
import { habitCategoryOptions } from './habitCategoryOptions'

type HabitFormProps = {
  onHabitCreated: (habit: HabitResponse) => void
}

function getHabitErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown habit-creation error occurred.'
}

export function HabitForm({ onHabitCreated }: HabitFormProps) {
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState<HabitCategory | ''>('')
  const [frequencyType, setFrequencyType] =
    useState<HabitFrequencyType>('daily')
  const [targetCount, setTargetCount] = useState(1)
  const [difficulty, setDifficulty] = useState<HabitDifficulty>('medium')

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  function changeFrequencyType(value: HabitFrequencyType) {
    setFrequencyType(value)

    if (value === 'daily') {
      setTargetCount(1)
    }
  }

  function resetForm() {
    setName('')
    setDescription('')
    setCategory('')
    setFrequencyType('daily')
    setTargetCount(1)
    setDifficulty('medium')
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
      const createdHabit = await createHabit({
        name,
        description: description === '' ? null : description,
        category,
        frequencyType,
        targetCount,
        difficulty,
      })

      onHabitCreated(createdHabit)
      resetForm()
    } catch (error) {
      setErrorMessage(getHabitErrorMessage(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section
      aria-labelledby="create-habit-heading"
      className="mt-8 rounded-lg bg-white p-6 text-left shadow-sm"
    >
      <h2 id="create-habit-heading" className="text-2xl font-bold">
        Create habit
      </h2>

      <form className="mt-4 space-y-4" onSubmit={handleSubmit}>
        <div>
          <label className="block font-medium" htmlFor="habit-name">
            Name
          </label>

          <input
            required
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
            id="habit-name"
            maxLength={100}
            type="text"
            value={name}
            onChange={(event) => setName(event.target.value)}
          />
        </div>

        <div>
          <label className="block font-medium" htmlFor="habit-description">
            Description
          </label>

          <textarea
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
            id="habit-description"
            maxLength={500}
            rows={3}
            value={description}
            onChange={(event) => setDescription(event.target.value)}
          />
        </div>

        <div>
          <label className="block font-medium" htmlFor="habit-category">
            Category
          </label>

          <select
            required
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
            id="habit-category"
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

        <div>
          <label className="block font-medium" htmlFor="habit-frequency">
            Frequency
          </label>

          <select
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
            id="habit-frequency"
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
            <label className="block font-medium" htmlFor="habit-target-count">
              Times per week
            </label>

            <input
              required
              className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
              id="habit-target-count"
              max={7}
              min={1}
              type="number"
              value={targetCount}
              onChange={(event) => setTargetCount(Number(event.target.value))}
            />
          </div>
        )}

        <div>
          <label className="block font-medium" htmlFor="habit-difficulty">
            Difficulty
          </label>

          <select
            className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
            id="habit-difficulty"
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

        <button
          className="w-full rounded bg-slate-900 px-5 py-3 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isSubmitting}
          type="submit"
        >
          {isSubmitting ? 'Creating habit...' : 'Create habit'}
        </button>

        {errorMessage && (
          <p className="text-red-700" role="alert">
            Habit creation error: {errorMessage}
          </p>
        )}
      </form>
    </section>
  )
}
