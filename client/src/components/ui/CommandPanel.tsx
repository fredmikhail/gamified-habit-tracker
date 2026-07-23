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
        'relative flex min-h-0 flex-col overflow-hidden rounded-2xl border border-line bg-surface-raised shadow-[var(--theme-panel-shadow)]',
        className,
      ].join(' ')}
    >
      <header className="flex shrink-0 items-start justify-between gap-3 border-b border-line px-4 py-3.5">
        <div className="min-w-0">
          {eyebrow && (
            <div className="flex items-center gap-1.5 text-accent">
              {Icon && <Icon aria-hidden="true" size={13} strokeWidth={1.9} />}

              <p className="text-[10px] font-bold tracking-[0.18em] uppercase">
                {eyebrow}
              </p>
            </div>
          )}

          <h2
            className="mt-1 truncate text-[15px] font-semibold"
            id={headingId}
          >
            {title}
          </h2>
        </div>

        {action && <div className="shrink-0">{action}</div>}
      </header>

      <div className={['min-h-0 flex-1 p-3.5', bodyClassName].join(' ')}>
        {children}
      </div>
    </section>
  )
}
