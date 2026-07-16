import { useState, type FormEvent } from 'react'
import { useAuth } from '../../auth/useAuth'

export function LoginForm() {
  const { loginUser, isAuthActionLoading } = useAuth()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    try {
      await loginUser({
        email,
        password,
        rememberMe,
      })
    } catch {
      return
    }
  }

  return (
    <form className="space-y-4 text-left" onSubmit={handleSubmit}>
      <div>
        <label className="block font-medium" htmlFor="login-email">
          Email
        </label>

        <input
          required
          autoComplete="email"
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id="login-email"
          maxLength={254}
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
        />
      </div>

      <div>
        <label className="block font-medium" htmlFor="login-password">
          Password
        </label>

        <input
          required
          autoComplete="current-password"
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id="login-password"
          maxLength={128}
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />
      </div>

      <label className="flex items-center gap-2">
        <input
          checked={rememberMe}
          type="checkbox"
          onChange={(event) => setRememberMe(event.target.checked)}
        />

        <span>Keep me signed in</span>
      </label>

      <button
        className="w-full rounded bg-slate-900 px-5 py-3 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
        disabled={isAuthActionLoading}
        type="submit"
      >
        {isAuthActionLoading ? 'Signing in...' : 'Sign in'}
      </button>
    </form>
  )
}
