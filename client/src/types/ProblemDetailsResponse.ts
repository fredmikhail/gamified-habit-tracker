export type ProblemDetailsResponse = {
  title?: string
  detail?: string
  status?: number
  instance?: string
  errors?: Record<string, string[]>
}
