import { useState } from 'react'
import { HabitForm } from './HabitForm'
import { HabitList } from './HabitList'

export function HabitSection() {
  const [refreshKey, setRefreshKey] = useState(0)

  function refreshHabits() {
    setRefreshKey((currentKey) => currentKey + 1)
  }

  return (
    <>
      <HabitForm onHabitCreated={refreshHabits} />

      <HabitList refreshKey={refreshKey} onHabitUpdated={refreshHabits} />
    </>
  )
}
