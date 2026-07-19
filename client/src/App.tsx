import { useState } from 'react'
import { getHealth } from './api/healthApi'
import { useAuth } from './auth/useAuth'
import { LoginForm } from './components/auth/LoginForm'
import { RegisterForm } from './components/auth/RegisterForm'
import { HabitList } from './components/habits/HabitList'
import type { HealthResponse } from './types/HealthResponse'

type AuthMode = 'login' | 'register'

function App() {
  const {
    currentUser,
    isAuthLoading,
    isAuthActionLoading,
    authErrorMessage,
    clearAuthError,
    logoutUser,
  } = useAuth()

  const [authMode, setAuthMode] = useState<AuthMode>('login')

  const [healthResponse, setHealthResponse] = useState<HealthResponse | null>(
    null,
  )

  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)

  function changeAuthMode(mode: AuthMode) {
    clearAuthError()
    setAuthMode(mode)
  }

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

  async function handleLogout() {
    try {
      await logoutUser()
    } catch {
      return
    }
  }

  return (
    <main className="min-h-screen bg-slate-100 px-6 py-12 text-slate-900">
      <div className="mx-auto max-w-3xl text-center">
        <h1 className="text-4xl font-bold">Gamified Habit Tracker</h1>

        <p className="mt-4 text-lg text-slate-600">
          Frontend and Tailwind CSS setup are working.
        </p>

        <div className="mx-auto mt-6 max-w-md rounded-lg bg-white p-6 shadow-sm">
          {isAuthLoading && <p>Checking authentication...</p>}

          {!isAuthLoading && !currentUser && (
            <>
              <div className="mb-6 flex rounded bg-slate-100 p-1">
                <button
                  className={`flex-1 rounded px-3 py-2 font-semibold ${
                    authMode === 'login' ? 'bg-white shadow-sm' : ''
                  }`}
                  type="button"
                  onClick={() => changeAuthMode('login')}
                >
                  Sign in
                </button>

                <button
                  className={`flex-1 rounded px-3 py-2 font-semibold ${
                    authMode === 'register' ? 'bg-white shadow-sm' : ''
                  }`}
                  type="button"
                  onClick={() => changeAuthMode('register')}
                >
                  Register
                </button>
              </div>

              {authMode === 'login' && <LoginForm />}

              {authMode === 'register' && <RegisterForm />}
            </>
          )}

          {!isAuthLoading && currentUser && (
            <>
              <p>
                Signed in as{' '}
                <span className="font-semibold">{currentUser.displayName}</span>
              </p>

              <button
                className="mt-4 rounded border border-slate-300 px-4 py-2 font-semibold disabled:cursor-not-allowed disabled:opacity-50"
                disabled={isAuthActionLoading}
                type="button"
                onClick={handleLogout}
              >
                {isAuthActionLoading ? 'Signing out...' : 'Sign out'}
              </button>
            </>
          )}

          {authErrorMessage && (
            <p className="mt-4 text-red-700">
              Authentication error: {authErrorMessage}
            </p>
          )}
        </div>

        {!isAuthLoading && currentUser && <HabitList />}

        <button
          className="mt-8 rounded bg-slate-900 px-5 py-3 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
          disabled={isLoading}
          type="button"
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