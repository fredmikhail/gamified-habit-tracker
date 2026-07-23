import {
  BrainCircuit,
  ChevronRight,
  LayoutDashboard,
  ListChecks,
  LogOut,
  Sparkles,
  Zap,
  type LucideIcon,
} from 'lucide-react'
import { useEffect } from 'react'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import type { CurrentUserResponse } from '../../types/CurrentUserResponse'
import { useWorkspaceData } from '../../workspace/useWorkspaceData'
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

type BrandMarkProps = {
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
    description: 'Create, complete, edit, and manage your habits.',
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

function getPercentage(value: number, total: number): number {
  if (total <= 0) {
    return 0
  }

  return Math.min(100, Math.max(0, (value / total) * 100))
}

function BrandMark({ compact = false }: BrandMarkProps) {
  const outerClassName = compact
    ? 'relative grid size-6 shrink-0 place-items-center text-accent'
    : 'relative grid size-11 shrink-0 place-items-center text-accent'

  const outerDiamondClassName = compact
    ? 'absolute inset-1 rotate-45 rounded-[5px] border border-accent/70 bg-accent-soft'
    : 'absolute inset-1 rotate-45 rounded-[11px] border border-accent/70 bg-accent-soft shadow-[var(--theme-energy-shadow)]'

  const innerDiamondClassName = compact
    ? 'absolute inset-2 rotate-45 rounded-[3px] border border-energy-cyan/40'
    : 'absolute inset-2.5 rotate-45 rounded-[7px] border border-energy-cyan/40'

  return (
    <div aria-hidden="true" className={outerClassName}>
      <span className={outerDiamondClassName} />
      <span className={innerDiamondClassName} />

      <Sparkles
        className="relative"
        size={compact ? 11 : 20}
        strokeWidth={1.8}
      />
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

  const { dashboardResource } = useWorkspaceData()

  const {
    data: dashboard,
    errorMessage: dashboardErrorMessage,
    ensureLoaded: ensureDashboardLoaded,
  } = dashboardResource

  useEffect(() => {
    void ensureDashboardLoaded()
  }, [ensureDashboardLoaded])

  const overallProgress = dashboard?.overallProgress ?? null

  const levelPercentage = overallProgress
    ? getPercentage(
        overallProgress.xpIntoCurrentLevel,
        overallProgress.xpNeededForNextLevel,
      )
    : 0

  const totalXpLabel = overallProgress
    ? overallProgress.totalXp.toLocaleString()
    : '—'

  const levelLabel = overallProgress
    ? overallProgress.level.toLocaleString()
    : '—'

  return (
    <div
      className="app-viewport grid min-h-0 min-w-0 bg-canvas text-content lg:grid-cols-[clamp(18rem,15vw,23rem)_minmax(0,1fr)]"
      data-testid="app-shell"
    >
      <aside className="hidden h-full min-h-0 flex-col overflow-hidden border-r border-line bg-sidebar lg:flex">
        <div className="flex shrink-0 items-center gap-3 border-b border-line px-[clamp(1.25rem,1.4vw,2rem)] py-5">
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

        <div className="shrink-0 px-[clamp(1.25rem,1.4vw,2rem)] pt-6">
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

      <section className="grid h-full min-h-0 min-w-0 grid-rows-[auto_minmax(0,1fr)_2.5rem] overflow-hidden">
        <header className="z-30 shrink-0 border-b border-line bg-canvas shadow-[0_10px_35px_rgb(0_0_0/0.12)]">
          <div className="flex min-h-[72px] w-full items-center justify-between gap-[clamp(0.75rem,1vw,1.25rem)] px-[clamp(1rem,1.6vw,2.75rem)]">
            <div className="flex min-w-0 items-center gap-3">
              <div className="lg:hidden">
                <BrandMark />
              </div>

              <div className="min-w-0">
                <p className="hidden text-[9px] font-semibold tracking-[0.2em] text-accent uppercase min-[430px]:block">
                  {currentPage.eyebrow}
                </p>

                <div className="flex min-w-0 items-baseline gap-3">
                  <h1 className="truncate text-xl font-semibold sm:text-2xl">
                    {currentPage.title}
                  </h1>

                  <p className="hidden truncate text-xs text-content-muted 2xl:block">
                    {currentPage.description}
                  </p>
                </div>
              </div>
            </div>

            <div
              className="flex min-w-0 shrink-0 items-center gap-[clamp(0.5rem,0.75vw,0.875rem)]"
              data-testid="global-header-controls"
            >
              <div
                aria-busy={overallProgress === null}
                aria-label="Overall progression"
                className="relative hidden h-11 min-w-[clamp(12.5rem,16vw,17rem)] items-center gap-2.5 px-1 min-[760px]:flex"
                data-testid="global-progress-cluster"
                title={
                  dashboardErrorMessage
                    ? 'Overall progression is temporarily unavailable.'
                    : undefined
                }
              >
                <Zap
                  aria-hidden="true"
                  className="shrink-0 text-warning"
                  fill="currentColor"
                  size={17}
                  strokeWidth={1.8}
                />

                <div className="min-w-0">
                  <p className="text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
                    Total XP
                  </p>

                  <p className="text-sm font-semibold tabular-nums text-content">
                    {totalXpLabel}
                  </p>
                </div>

                <ChevronRight
                  aria-hidden="true"
                  className="shrink-0 text-content-subtle"
                  size={13}
                />

                <span
                  aria-label={`Level ${levelLabel}`}
                  className="min-w-6 text-center text-sm font-semibold tabular-nums text-content"
                >
                  {levelLabel}
                </span>

                <div className="min-w-[4.5rem] flex-1">
                  <div
                    aria-label="Overall level progress"
                    aria-valuemax={
                      overallProgress?.xpNeededForNextLevel ?? undefined
                    }
                    aria-valuemin={0}
                    aria-valuenow={
                      overallProgress?.xpIntoCurrentLevel ?? undefined
                    }
                    className="h-1.5 overflow-hidden rounded-full bg-surface-muted"
                    role="progressbar"
                  >
                    <div
                      className="h-full rounded-full"
                      style={{
                        width: `${levelPercentage}%`,
                        background: 'var(--theme-progress-gradient)',
                        boxShadow:
                          '0 0 12px color-mix(in srgb, var(--theme-energy-violet) 38%, transparent)',
                      }}
                    />
                  </div>
                </div>

                <span
                  aria-hidden="true"
                  className="absolute inset-x-0 bottom-0 h-px"
                  style={{
                    background:
                      'linear-gradient(90deg, transparent, var(--theme-accent-primary), var(--theme-energy-violet), transparent)',
                  }}
                />
              </div>

              <span
                aria-hidden="true"
                className="hidden h-7 w-px bg-line min-[900px]:block"
              />

              <div
                aria-label={`${currentUser.displayName} is online`}
                className="hidden min-w-0 items-center gap-2 min-[900px]:flex"
                data-testid="global-account-status"
              >
                <span
                  aria-hidden="true"
                  className="size-2.5 shrink-0 rounded-full bg-success shadow-[0_0_10px_color-mix(in_srgb,var(--theme-success)_45%,transparent)]"
                />

                <div className="min-w-0">
                  <p className="max-w-[clamp(6rem,9vw,10rem)] truncate text-xs font-semibold text-content">
                    {currentUser.displayName}
                  </p>

                  <p className="text-[8px] font-bold tracking-[0.12em] text-success uppercase">
                    Online
                  </p>
                </div>
              </div>

              <ThemeSelector />

              <button
                aria-label="Sign out"
                className="grid size-11 shrink-0 place-items-center rounded-xl border border-line bg-surface-raised text-content-muted shadow-[var(--theme-panel-shadow)] transition-colors hover:border-danger/50 hover:bg-surface-hover hover:text-danger disabled:cursor-not-allowed disabled:opacity-50"
                disabled={isLogoutPending}
                title={isLogoutPending ? 'Signing out...' : 'Sign out'}
                type="button"
                onClick={onLogout}
              >
                <LogOut aria-hidden="true" size={18} strokeWidth={1.8} />
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
          <div className="grid h-full min-h-0 w-full px-[clamp(1rem,1.6vw,2.75rem)] py-[clamp(0.875rem,1.15vw,1.5rem)]">
            <div
              className="app-route-scroll min-h-0 min-w-0"
              data-testid="route-content-region"
            >
              <div className="h-full min-w-0">
                <Outlet />
              </div>
            </div>
          </div>
        </main>

        <footer className="flex min-w-0 items-center border-t border-line bg-sidebar px-[clamp(1rem,1.6vw,2.75rem)]">
          <div className="flex min-w-0 items-center gap-2">
            <BrandMark compact />

            <span className="hidden truncate text-[8px] font-semibold tracking-[0.16em] text-content-subtle uppercase sm:inline">
              Gamified Habit Tracker
            </span>

            <span
              aria-hidden="true"
              className="hidden h-3 w-px bg-line sm:block"
            />

            <p
              className="truncate text-[9px] font-semibold tracking-[0.04em] text-accent sm:text-[10px]"
              style={{
                background: 'var(--theme-progress-gradient)',
                backgroundClip: 'text',
                color: 'transparent',
                WebkitBackgroundClip: 'text',
              }}
            >
              Stay consistent. Earn XP. Become Legendary.
            </p>
          </div>
        </footer>
      </section>
    </div>
  )
}
