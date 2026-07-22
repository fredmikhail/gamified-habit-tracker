import { useState } from 'react'
import { AuthenticatedWorkspace } from './AuthenticatedWorkspace'
import { useAuth } from './auth/useAuth'
import { LoginForm } from './components/auth/LoginForm'
import { RegisterForm } from './components/auth/RegisterForm'
import { ThemeSelector } from './components/theme/ThemeSelector'
import './viewport.css'

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

  function changeAuthMode(mode: AuthMode): void {
    clearAuthError()
    setAuthMode(mode)
  }

  async function handleLogout(): Promise<void> {
    try {
      await logoutUser()
    } catch {
      return
    }
  }

  if (isAuthLoading) {
    return (
      <main className="app-viewport grid place-items-center bg-canvas px-6 text-content">
        <div className="text-center">
          <div
            aria-hidden="true"
            className="mx-auto size-10 animate-pulse rounded-xl border border-accent/50 bg-accent-soft shadow-[var(--theme-energy-shadow)]"
          />

          <p className="mt-4 text-sm text-content-muted">
            Restoring your command center...
          </p>
        </div>
      </main>
    )
  }

  if (currentUser) {
    return (
      <AuthenticatedWorkspace
        currentUser={currentUser}
        isLogoutPending={isAuthActionLoading}
        onLogout={handleLogout}
      />
    )
  }

  return (
    <main className="app-viewport relative overflow-hidden bg-canvas text-content">
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_20%_10%,var(--theme-accent-soft),transparent_34%),radial-gradient(circle_at_85%_90%,rgb(139_92_246/0.1),transparent_36%)]"
      />

      <div className="absolute top-3 right-3 z-20 sm:top-4 sm:right-4">
        <ThemeSelector />
      </div>

      <div className="app-route-scroll absolute inset-0">
        <div className="grid min-h-full place-items-center px-5 py-16 sm:py-20">
          <section className="relative z-10 w-full max-w-md rounded-3xl border border-line bg-surface-raised p-6 shadow-[var(--theme-panel-shadow)] sm:p-8">
            <div className="text-center">
              <p className="text-[11px] font-semibold tracking-[0.22em] text-accent uppercase">
                Personal progression system
              </p>

              <h1 className="mt-3 text-3xl font-semibold tracking-tight">
                Gamified Habit Tracker
              </h1>

              <p className="mt-3 text-sm leading-6 text-content-muted">
                Build stronger habits. Develop your attributes. Level up through
                consistent action.
              </p>
            </div>

            <div className="mt-7 flex rounded-xl border border-line bg-surface-muted p-1">
              <button
                className={`min-h-10 flex-1 rounded-lg px-3 py-2 text-sm font-semibold transition-colors ${
                  authMode === 'login'
                    ? 'bg-surface-raised text-content shadow-sm'
                    : 'text-content-muted hover:text-content'
                }`}
                type="button"
                onClick={() => changeAuthMode('login')}
              >
                Sign in
              </button>

              <button
                className={`min-h-10 flex-1 rounded-lg px-3 py-2 text-sm font-semibold transition-colors ${
                  authMode === 'register'
                    ? 'bg-surface-raised text-content shadow-sm'
                    : 'text-content-muted hover:text-content'
                }`}
                type="button"
                onClick={() => changeAuthMode('register')}
              >
                Register
              </button>
            </div>

            <div className="mt-6">
              {authMode === 'login' && <LoginForm />}

              {authMode === 'register' && <RegisterForm />}
            </div>

            {authErrorMessage && (
              <p
                className="mt-5 rounded-xl border border-danger/30 bg-danger/10 p-3 text-sm text-danger"
                role="alert"
              >
                Authentication error: {authErrorMessage}
              </p>
            )}
          </section>
        </div>
      </div>
    </main>
  )
}

export default App
