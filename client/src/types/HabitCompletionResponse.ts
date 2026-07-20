export type HabitCompletionResponse = {
  id: string
  habitId: string
  completedDate: string
  completedAtUtc: string
  notes: string | null
}
