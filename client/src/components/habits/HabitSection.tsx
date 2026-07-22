import { useState } from 'react'
import { AttributeSection } from '../attributes/AttributeSection'
import { OverallProgressSection } from '../dashboard/OverallProgressSection'
import { HabitForm } from './HabitForm'
import { HabitList } from './HabitList'

export function HabitSection() {
  const [habitRefreshKey, setHabitRefreshKey] = useState(0)
  const [progressRefreshKey, setProgressRefreshKey] = useState(0)

  function refreshHabits() {
    setHabitRefreshKey((currentKey) => currentKey + 1)
  }

  function refreshProgress() {
    setProgressRefreshKey((currentKey) => currentKey + 1)
  }

  return (
    <>
      <OverallProgressSection refreshKey={progressRefreshKey} />

      <AttributeSection refreshKey={progressRefreshKey} />

      <HabitForm onHabitCreated={refreshHabits} />

      <HabitList
        refreshKey={habitRefreshKey}
        onHabitUpdated={refreshHabits}
        onHabitDeactivated={refreshHabits}
        onProgressChanged={refreshProgress}
      />
    </>
  )
}
