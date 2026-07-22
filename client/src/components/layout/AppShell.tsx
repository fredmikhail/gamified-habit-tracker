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

type NavigationLinkProps = NavigationItem & {
  compact?: boolean
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

function NavigationLink({
  to,
  label,
  Icon,
  compact = false,
}: NavigationLinkProps) {
  return (
    <NavLink
      className={({ isActive }) =>
        [
          'group flex min-h-11 w-full min-w-0 items-center rounded-xl border font-medium transition-colors',
          compact
            ? 'justify-center gap-2 px-2 py-2.5 text-xs min-[390px]:justify-start min-[390px]:px-3 min-[390px]:text-sm'
            : 'gap-3 px-3.5 py-2.5 text-sm',
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

      <span
        className={compact ? 'hidden truncate min-[390px]:inline' : 'truncate'}
      >
        {label}
      </span>
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
    <div
      className="app-viewport grid min-h-0 min-w-0 bg-canvas text-content lg:grid-cols-[18rem_minmax(0,1fr)]"
      data-testid="app-shell"
    >
      <aside className="hidden h-full min-h-0 flex-col overflow-hidden border-r border-line bg-sidebar lg:flex">
        <div className="flex shrink-0 items-center gap-3 border-b border-line px-6 py-5">
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

        <div className="shrink-0 px-6 pt-6">
          <p className="text-[11px] font-semibold tracking-[0.2em] text-content-subtle uppercase">
            Navigation
          </p>
        </div>

        <nav
          aria-label="Primary navigation"
          className="mt-3 shrink-0 space-y-1.5 px-4"
        >
          {navigationItems.map((item) => (
            <NavigationLink key={item.to} {...item} />
          ))}
        </nav>

        <div className="relative mt-auto shrink-0 overflow-hidden border-t border-line p-4">
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

      <section className="grid h-full min-h-0 min-w-0 grid-rows-[auto_minmax(0,1fr)] overflow-hidden">
        <header className="z-30 shrink-0 border-b border-line bg-canvas shadow-[0_10px_35px_rgb(0_0_0/0.12)]">
          <div className="mx-auto flex min-h-[72px] max-w-[1600px] items-center justify-between gap-3 px-3 sm:px-6 lg:px-8">
            <div className="flex min-w-0 items-center gap-3">
              <div className="lg:hidden">
                <BrandMark />
              </div>

              <div className="hidden min-w-0 min-[430px]:block">
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
                className="inline-flex min-h-11 items-center gap-2 rounded-xl border border-line bg-surface-raised px-3 py-2.5 text-sm font-semibold text-content transition-colors hover:border-danger/50 hover:bg-surface-hover hover:text-danger disabled:cursor-not-allowed disabled:opacity-50 sm:px-3.5"
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
            className="grid grid-cols-3 gap-2 border-t border-line px-3 py-2.5 lg:hidden"
          >
            {navigationItems.map((item) => (
              <NavigationLink compact key={item.to} {...item} />
            ))}
          </nav>
        </header>

        <main className="min-h-0 min-w-0 overflow-hidden">
          <div className="mx-auto grid h-full min-h-0 w-full max-w-[1600px] grid-rows-[auto_minmax(0,1fr)] px-4 pt-4 sm:px-6 sm:pt-5 lg:px-8 lg:pt-6">
            <div className="shrink-0 pb-4 lg:pb-5">
              <p className="max-w-2xl text-sm leading-6 text-content-muted">
                {currentPage.description}
              </p>
            </div>

            <div
              className="app-route-scroll min-h-0 min-w-0"
              data-testid="route-content-region"
            >
              <div className="min-w-0 pb-5 pr-1 lg:pb-7">
                <Outlet />
              </div>
            </div>
          </div>
        </main>
      </section>
    </div>
  )
}
