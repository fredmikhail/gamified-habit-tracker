import type { CSSProperties, ReactNode } from 'react'
import type { LucideIcon } from 'lucide-react'

type MetricPanelProps = {
  label: string
  Icon: LucideIcon
  accentVariable: string
  children: ReactNode
  className?: string
}

export function MetricPanel({
  label,
  Icon,
  accentVariable,
  children,
  className = '',
}: MetricPanelProps) {
  const style = {
    '--metric-accent': accentVariable,
  } as CSSProperties

  return (
    <article
      aria-label={label}
      className={[
        'relative min-h-0 overflow-hidden rounded-2xl border border-line bg-surface-raised p-3.5 shadow-[var(--theme-panel-shadow)]',
        className,
      ].join(' ')}
      style={{
        ...style,
        backgroundImage:
          'linear-gradient(145deg, color-mix(in srgb, var(--metric-accent) 8%, transparent), transparent 60%)',
      }}
    >
      <div className="flex items-center gap-2">
        <div
          className="grid size-8 shrink-0 place-items-center rounded-lg border"
          style={{
            borderColor:
              'color-mix(in srgb, var(--metric-accent) 35%, transparent)',
            backgroundColor:
              'color-mix(in srgb, var(--metric-accent) 12%, transparent)',
            color: 'var(--metric-accent)',
          }}
        >
          <Icon aria-hidden="true" size={15} strokeWidth={1.9} />
        </div>

        <p className="truncate text-[10px] font-bold tracking-[0.16em] text-content-subtle uppercase">
          {label}
        </p>
      </div>

      <div className="mt-3">{children}</div>
    </article>
  )
}
