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
        'ui-adaptive-panel relative min-h-0 overflow-hidden rounded-2xl border border-line bg-surface-raised p-[clamp(0.75rem,0.66rem_+_0.1vw,0.9375rem)] shadow-[var(--theme-panel-shadow)]',
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
          className="grid size-[var(--ui-icon-box-size)] shrink-0 place-items-center rounded-lg border"
          style={{
            borderColor:
              'color-mix(in srgb, var(--metric-accent) 35%, transparent)',
            backgroundColor:
              'color-mix(in srgb, var(--metric-accent) 12%, transparent)',
            color: 'var(--metric-accent)',
          }}
        >
          <Icon
            aria-hidden="true"
            strokeWidth={1.9}
            style={{
              width: 'var(--ui-icon-size)',
              height: 'var(--ui-icon-size)',
            }}
          />
        </div>

        <p className="truncate text-[var(--ui-eyebrow-size)] font-bold tracking-[0.16em] text-content-subtle uppercase">
          {label}
        </p>
      </div>

      <div className="mt-[clamp(0.625rem,0.53rem_+_0.1vw,0.8125rem)]">
        {children}
      </div>
    </article>
  )
}
