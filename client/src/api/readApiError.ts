import type { ProblemDetailsResponse } from '../types/ProblemDetailsResponse'

export async function readApiError(
  response: Response,
  fallbackMessage: string,
): Promise<string> {
  const contentType = response.headers.get('content-type')

  if (!contentType?.includes('json')) {
    return fallbackMessage
  }

  try {
    const problemDetails: ProblemDetailsResponse = await response.json()

    if (problemDetails.detail) {
      return problemDetails.detail
    }

    if (problemDetails.errors) {
      const validationMessages = Object.values(problemDetails.errors).flat()

      if (validationMessages.length > 0) {
        return validationMessages.join(' ')
      }
    }

    if (problemDetails.title) {
      return problemDetails.title
    }
  } catch {
    return fallbackMessage
  }

  return fallbackMessage
}
