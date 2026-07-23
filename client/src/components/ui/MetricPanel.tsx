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
        'relative min-h-0 overflow-hidden rounded-2xl border border-line bg-surface-raised p-[clamp(0.75rem,0.66rem_+_0.1vw,0.9375rem)] shadow-[var(--theme-panel-shadow)]',
        className,
      ].join(' ')}
      style={{
        ...style,
        backgroundImage:
          'linear-gradient(145deg, color-mix(in srgb, var(--metric-accent) 8%, transparent), transparent 60%)',
      }}
    >
      <div className="flex items-center gap-[clamp(0.5rem,0.44rem_+_0.06vw,0.625rem)]">
        <div
          className="grid size-[clamp(2rem,1.85rem_+_0.12vw,2.25rem)] shrink-0 place-items-center rounded-lg border"
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

        <p className="truncate text-[clamp(0.625rem,0.58rem_+_0.04vw,0.6875rem)] font-bold tracking-[0.16em] text-content-subtle uppercase">
          {label}
        </p>
      </div>

      <div className="mt-[clamp(0.625rem,0.53rem_+_0.1vw,0.8125rem)]">
        {children}
      </div>
    </article>
  )
}
