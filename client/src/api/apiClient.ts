import type { AntiforgeryTokenResponse } from '../types/AntiforgeryTokenResponse'

const stateChangingMethods = new Set(['POST', 'PUT', 'PATCH', 'DELETE'])

let csrfToken: string | null = null
let csrfTokenRequest: Promise<string> | null = null

async function fetchCsrfToken(): Promise<string> {
  const response = await fetch('/api/auth/csrf-token', {
    credentials: 'include',
  })

  if (!response.ok) {
    throw new Error(`CSRF token request failed with status ${response.status}.`)
  }

  const responseBody: AntiforgeryTokenResponse = await response.json()

  if (!responseBody.requestToken) {
    throw new Error('The API returned an empty CSRF token.')
  }

  return responseBody.requestToken
}

async function getCsrfToken(): Promise<string> {
  if (csrfToken) {
    return csrfToken
  }

  csrfTokenRequest ??= fetchCsrfToken()

  try {
    csrfToken = await csrfTokenRequest

    return csrfToken
  } finally {
    csrfTokenRequest = null
  }
}

export function clearCsrfToken(): void {
  csrfToken = null
  csrfTokenRequest = null
}

export async function apiRequest(
  path: string,
  options: RequestInit = {},
): Promise<Response> {
  const method = (options.method ?? 'GET').toUpperCase()
  const headers = new Headers(options.headers)

  if (stateChangingMethods.has(method)) {
    const requestToken = await getCsrfToken()

    headers.set('X-CSRF-TOKEN', requestToken)
  }

  return fetch(path, {
    ...options,
    method,
    headers,
    credentials: 'include',
  })
}
