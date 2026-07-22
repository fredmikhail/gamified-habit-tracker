import {
  BrainCircuit,
  LayoutDashboard,
  ListChecks,
  LogOut,
  Sparkles,
  type LucideIcon,
} from 'lucide-react'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import type { CurrentUserResponse } from '../../types/CurrentUserResponse'
import { ThemeSelector } from '../theme/ThemeSelector'

type AppShellProps = {
  currentUser: CurrentUserResponse
  isLogoutPending: boolean
  onLogout: () => void
}

type NavigationItem = {
  to: string
  label: string
  Icon: LucideIcon
}

const navigationItems: readonly NavigationItem[] = [
  {
    to: '/dashboard',
    label: 'Dashboard',
    Icon: LayoutDashboard,
  },
  {
    to: '/habits',
    label: 'Habits',
    Icon: ListChecks,
  },
  {
    to: '/attributes',
    label: 'Attributes',
    Icon: BrainCircuit,
  },
]

const pageDetails = {
  dashboard: {
    eyebrow: 'Command center',
    title: 'Dashboard',
    description: 'Review today’s execution and your current progression.',
  },
  habits: {
    eyebrow: 'Habit workspace',
    title: 'Habits',
    description: 'Create, complete, edit, and manage your active habits.',
  },
  attributes: {
    eyebrow: 'Character progression',
    title: 'Attributes',
    description: 'Track the areas of your character shaped by your habits.',
  },
} as const

function getPageDetails(pathname: string) {
  if (pathname.startsWith('/habits')) {
    return pageDetails.habits
  }

  if (pathname.startsWith('/attributes')) {
    return pageDetails.attributes
  }

  return pageDetails.dashboard
}

function getUserInitials(displayName: string): string {
  const words = displayName.trim().split(/\s+/).filter(Boolean)

  if (words.length === 0) {
    return '?'
  }

  return words
    .slice(0, 2)
    .map((word) => word.charAt(0).toUpperCase())
    .join('')
}

function BrandMark() {
  return (
    <div
      aria-hidden="true"
      className="relative grid size-11 shrink-0 place-items-center text-accent"
    >
      <span className="absolute inset-1 rotate-45 rounded-[11px] border border-accent/70 bg-accent-soft shadow-[var(--theme-energy-shadow)]" />

      <span className="absolute inset-2.5 rotate-45 rounded-[7px] border border-energy-cyan/40" />

      <Sparkles className="relative" size={20} strokeWidth={1.8} />
    </div>
  )
}

function NavigationLink({ to, label, Icon }: NavigationItem) {
  return (
    <NavLink
      className={({ isActive }) =>
        [
          'group flex min-h-11 items-center gap-3 rounded-xl border px-3.5 py-2.5 text-sm font-medium transition-colors',
          isActive
            ? 'border-accent/40 bg-accent-soft text-content shadow-[var(--theme-energy-shadow)]'
            : 'border-transparent text-content-muted hover:border-line hover:bg-surface-hover hover:text-content',
        ].join(' ')
      }
      to={to}
    >
      <Icon
        aria-hidden="true"
        className="shrink-0 transition-colors group-hover:text-accent"
        size={19}
        strokeWidth={1.7}
      />

      <span>{label}</span>
    </NavLink>
  )
}

export function AppShell({
  currentUser,
  isLogoutPending,
  onLogout,
}: AppShellProps) {
  const location = useLocation()
  const currentPage = getPageDetails(location.pathname)

  return (
    <div className="min-h-screen bg-canvas text-content">
      <aside className="fixed inset-y-0 left-0 z-40 hidden w-72 flex-col border-r border-line bg-sidebar lg:flex">
        <div className="flex items-center gap-3 border-b border-line px-6 py-5">
          <BrandMark />

          <div className="min-w-0">
            <p className="truncate text-sm font-bold tracking-[0.16em] text-content">
              GAMIFIED
            </p>

            <p className="mt-0.5 truncate text-xs tracking-[0.22em] text-content-muted">
              HABIT TRACKER
            </p>
          </div>
        </div>

        <div className="px-6 pt-6">
          <p className="text-[11px] font-semibold tracking-[0.2em] text-content-subtle uppercase">
            Navigation
          </p>
        </div>

        <nav aria-label="Primary navigation" className="mt-3 space-y-1.5 px-4">
          {navigationItems.map((item) => (
            <NavigationLink key={item.to} {...item} />
          ))}
        </nav>

        <div className="relative mt-auto overflow-hidden border-t border-line p-4">
          <div
            aria-hidden="true"
            className="pointer-events-none absolute inset-x-0 bottom-0 h-48 bg-[radial-gradient(circle_at_50%_100%,var(--theme-accent-soft),transparent_68%)]"
          />

          <div className="relative rounded-2xl border border-line bg-surface p-4 shadow-[var(--theme-panel-shadow)]">
            <div className="flex items-center gap-3">
              <div className="grid size-10 shrink-0 place-items-center rounded-xl border border-accent/30 bg-accent-soft text-sm font-bold text-accent">
                {getUserInitials(currentUser.displayName)}
              </div>

              <div className="min-w-0">
                <p className="truncate text-sm font-semibold">
                  {currentUser.displayName}
                </p>

                <p className="mt-0.5 truncate text-xs text-content-muted">
                  @{currentUser.username}
                </p>
              </div>
            </div>

            <p className="mt-4 text-[10px] font-semibold tracking-[0.18em] text-content-subtle uppercase">
              Level up every day
            </p>
          </div>
        </div>
      </aside>

      <div className="min-h-screen lg:pl-72">
        <header className="sticky top-0 z-30 border-b border-line bg-canvas shadow-[0_10px_35px_rgb(0_0_0/0.12)]">
          <div className="mx-auto flex max-w-[1600px] items-center justify-between gap-4 px-4 py-4 sm:px-6 lg:px-8">
            <div className="flex min-w-0 items-center gap-3">
              <div className="lg:hidden">
                <BrandMark />
              </div>

              <div className="min-w-0">
                <p className="text-[10px] font-semibold tracking-[0.2em] text-accent uppercase">
                  {currentPage.eyebrow}
                </p>

                <h1 className="mt-1 truncate text-xl font-semibold sm:text-2xl">
                  {currentPage.title}
                </h1>
              </div>
            </div>

            <div className="flex shrink-0 items-center gap-2">
              <ThemeSelector />

              <button
                aria-label="Sign out"
                className="inline-flex min-h-11 items-center gap-2 rounded-xl border border-line bg-surface-raised px-3.5 py-2.5 text-sm font-semibold text-content transition-colors hover:border-danger/50 hover:bg-surface-hover hover:text-danger disabled:cursor-not-allowed disabled:opacity-50"
                disabled={isLogoutPending}
                type="button"
                onClick={onLogout}
              >
                <LogOut aria-hidden="true" size={18} strokeWidth={1.8} />

                <span className="hidden sm:inline">
                  {isLogoutPending ? 'Signing out...' : 'Sign out'}
                </span>
              </button>
            </div>
          </div>

          <nav
            aria-label="Mobile navigation"
            className="flex gap-2 overflow-x-auto border-t border-line px-4 py-3 lg:hidden"
          >
            {navigationItems.map((item) => (
              <NavigationLink key={item.to} {...item} />
            ))}
          </nav>
        </header>

        <main className="mx-auto w-full max-w-[1600px] px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
          <div className="mb-6">
            <p className="max-w-2xl text-sm leading-6 text-content-muted">
              {currentPage.description}
            </p>
          </div>

          <Outlet />
        </main>
      </div>
    </div>
  )
}
