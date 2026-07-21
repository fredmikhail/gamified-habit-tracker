import { useState } from 'react'
import { AttributeSection } from '../attributes/AttributeSection'
import { HabitForm } from './HabitForm'
import { HabitList } from './HabitList'

export function HabitSection() {
  const [habitRefreshKey, setHabitRefreshKey] = useState(0)
  const [attributeRefreshKey, setAttributeRefreshKey] = useState(0)

  function refreshHabits() {
    setHabitRefreshKey((currentKey) => currentKey + 1)
  }

  function refreshAttributes() {
    setAttributeRefreshKey((currentKey) => currentKey + 1)
  }

  return (
    <>
      <AttributeSection refreshKey={attributeRefreshKey} />

      <HabitForm onHabitCreated={refreshHabits} />

      <HabitList
        refreshKey={habitRefreshKey}
        onHabitUpdated={refreshHabits}
        onHabitDeactivated={refreshHabits}
        onProgressChanged={refreshAttributes}
      />
    </>
  )
}
