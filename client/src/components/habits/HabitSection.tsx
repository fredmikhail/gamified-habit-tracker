import type { HabitResponse } from '../../types/HabitResponse'
import { useWorkspaceData } from '../../workspace/useWorkspaceData'
import { HabitForm } from './HabitForm'
import { HabitList } from './HabitList'

type HabitSectionProps = {
  onProgressChanged: () => void
}

export function HabitSection({ onProgressChanged }: HabitSectionProps) {
  const { habitsResource } = useWorkspaceData()
  const { updateData: updateHabits } = habitsResource

  function handleHabitCreated(createdHabit: HabitResponse): void {
    updateHabits((currentHabits) => {
      if (currentHabits === null) {
        return [createdHabit]
      }

      const habitAlreadyExists = currentHabits.some(
        (habit) => habit.id === createdHabit.id,
      )

      if (habitAlreadyExists) {
        return currentHabits.map((habit) =>
          habit.id === createdHabit.id ? createdHabit : habit,
        )
      }

      return [...currentHabits, createdHabit]
    })

    onProgressChanged()
  }

  function handleHabitChanged(): void {
    onProgressChanged()
  }

  return (
    <>
      <HabitForm onHabitCreated={handleHabitCreated} />

      <HabitList
        onHabitUpdated={handleHabitChanged}
        onHabitDeactivated={handleHabitChanged}
        onProgressChanged={onProgressChanged}
      />
    </>
  )
}
