import { useId, type ReactNode } from 'react'
import type { LucideIcon } from 'lucide-react'

type CommandPanelProps = {
  title: string
  eyebrow?: string
  Icon?: LucideIcon
  action?: ReactNode
  children: ReactNode
  className?: string
  bodyClassName?: string
}

export function CommandPanel({
  title,
  eyebrow,
  Icon,
  action,
  children,
  className = '',
  bodyClassName = '',
}: CommandPanelProps) {
  const headingId = useId()

  return (
    <section
      aria-labelledby={headingId}
      className={[
        'ui-adaptive-panel relative flex min-h-0 flex-col overflow-hidden rounded-2xl border border-line bg-surface-raised shadow-[var(--theme-panel-shadow)]',
        className,
      ].join(' ')}
    >
      <header className="flex shrink-0 items-start justify-between gap-[clamp(0.75rem,0.65rem_+_0.12vw,1rem)] border-b border-line px-[clamp(0.875rem,0.72rem_+_0.18vw,1.125rem)] py-[clamp(0.75rem,0.64rem_+_0.12vw,0.9375rem)]">
        <div className="min-w-0">
          {eyebrow && (
            <div className="flex items-center gap-1.5 text-accent">
              {Icon && (
                <Icon
                  aria-hidden="true"
                  strokeWidth={1.9}
                  style={{
                    width: 'var(--ui-icon-size)',
                    height: 'var(--ui-icon-size)',
                  }}
                />
              )}

              <p className="text-[var(--ui-eyebrow-size)] font-bold tracking-[0.18em] uppercase">
                {eyebrow}
              </p>
            </div>
          )}

          <h2
            className="mt-1 truncate text-[var(--ui-panel-title-size)] font-semibold"
            id={headingId}
          >
            {title}
          </h2>
        </div>

        {action && <div className="shrink-0">{action}</div>}
      </header>

      <div
        className={[
          'min-h-0 flex-1 p-[clamp(0.75rem,0.66rem_+_0.1vw,0.9375rem)]',
          bodyClassName,
        ].join(' ')}
      >
        {children}
      </div>
    </section>
  )
}
