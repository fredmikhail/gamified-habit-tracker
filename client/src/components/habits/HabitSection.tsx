import type { HabitResponse } from '../../types/HabitResponse'
import { HabitList } from './HabitList'

type HabitSectionProps = {
  onProgressChanged: () => void
}

export function HabitSection({ onProgressChanged }: HabitSectionProps) {
  function handleHabitChanged(_habit: HabitResponse): void {
    onProgressChanged()
  }

  return (
    <section
      aria-label="Habit workspace"
      className="h-full min-h-0 min-w-0 overflow-hidden"
    >
      <HabitList
        onHabitDeactivated={handleHabitChanged}
        onHabitUpdated={handleHabitChanged}
        onProgressChanged={onProgressChanged}
      />
    </section>
  )
}
