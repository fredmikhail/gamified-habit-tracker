import { useState, type FormEvent } from 'react'
import { useAuth } from '../../auth/useAuth'

export function RegisterForm() {
  const { registerUser, isAuthActionLoading } = useAuth()

  const [email, setEmail] = useState('')
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [timeZone, setTimeZone] = useState(
    () => Intl.DateTimeFormat().resolvedOptions().timeZone,
  )

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    try {
      await registerUser({
        email,
        username,
        password,
        timeZone,
      })
    } catch {
      return
    }
  }

  return (
    <form className="space-y-4 text-left" onSubmit={handleSubmit}>
      <div>
        <label className="block font-medium" htmlFor="register-email">
          Email
        </label>

        <input
          required
          autoComplete="email"
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id="register-email"
          maxLength={254}
          type="email"
          value={email}
          onChange={(event) => setEmail(event.target.value)}
        />
      </div>

      <div>
        <label className="block font-medium" htmlFor="register-username">
          Username
        </label>

        <input
          required
          autoComplete="username"
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id="register-username"
          maxLength={30}
          minLength={3}
          pattern="[A-Za-z0-9_]+"
          title="Use only letters, numbers, and underscores."
          type="text"
          value={username}
          onChange={(event) => setUsername(event.target.value)}
        />

        <p className="mt-1 text-sm text-slate-500">
          3–30 characters. Letters, numbers, and underscores only.
        </p>
      </div>

      <div>
        <label className="block font-medium" htmlFor="register-password">
          Password
        </label>

        <input
          required
          autoComplete="new-password"
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id="register-password"
          maxLength={128}
          minLength={15}
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
        />

        <p className="mt-1 text-sm text-slate-500">
          Use at least 15 characters.
        </p>
      </div>

      <div>
        <label className="block font-medium" htmlFor="register-time-zone">
          Time zone
        </label>

        <input
          required
          className="mt-1 w-full rounded border border-slate-300 px-3 py-2"
          id="register-time-zone"
          maxLength={100}
          type="text"
          value={timeZone}
          onChange={(event) => setTimeZone(event.target.value)}
        />

        <p className="mt-1 text-sm text-slate-500">
          Automatically detected from your browser.
        </p>
      </div>

      <button
        className="w-full rounded bg-slate-900 px-5 py-3 font-semibold text-white disabled:cursor-not-allowed disabled:opacity-50"
        disabled={isAuthActionLoading}
        type="submit"
      >
        {isAuthActionLoading ? 'Creating account...' : 'Create account'}
      </button>
    </form>
  )
}
