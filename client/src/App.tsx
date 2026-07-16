import { useState } from 'react'
import { getHealth } from './api/healthApi'
import { useAuth } from './auth/useAuth'
import type { HealthResponse } from './types/HealthResponse'

function App() {
  const { currentUser, isAuthLoading, authErrorMessage } = useAuth()

  const [healthResponse, setHealthResponse] = useState<HealthResponse | null>(
    null,
  )
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  async function handleCheckBackend() {
    setIsLoading(true)
    setErrorMessage(null)

    try {
      const response = await getHealth()
      setHealthResponse(response)
    } catch (error) {
      setHealthResponse(null)

      setErrorMessage(
        error instanceof Error ? error.message : 'An unknown error occurred.',
      )
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <main className="min-h-screen bg-slate-100 px-6 py-12 text-slate-900">
      <div className="mx-auto max-w-3xl text-center">
        <h1 className="text-4xl font-bold">Gamified Habit Tracker</h1>

        <p className="mt-4 text-lg text-slate-600">
          Frontend and Tailwind CSS setup are working.
        </p>

        <div className="mt-6 rounded-lg bg-white p-4 shadow-sm">
          {isAuthLoading && <p>Checking authentication...</p>}

          {!isAuthLoading && currentUser && (
            <p>
              Signed in as{' '}
              <span className="font-semibold">{currentUser.displayName}</span>
            </p>
          )}

          {!isAuthLoading && !currentUser && !authErrorMessage && (
            <p>Not signed in.</p>
          )}

          {authErrorMessage && (
            <p className="text-red-700">
              Authentication check failed: {authErrorMessage}
            </p>
          )}
        </div>

        <button
          type="button"
          className="mt-8 rounded bg-slate-900 px-5 py-3 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isLoading}
          onClick={handleCheckBackend}
        >
          {isLoading ? 'Checking...' : 'Check backend'}
        </button>

        {healthResponse && (
          <div className="mt-6">
            <p>Backend status: {healthResponse.status}</p>
            <p>Checked at UTC: {healthResponse.checkedAtUtc}</p>
          </div>
        )}

        {errorMessage && (
          <p className="mt-6 text-red-700">Request failed: {errorMessage}</p>
        )}
      </div>
    </main>
  )
}

export default App
