import { Zap } from 'lucide-react'
import type { CSSProperties } from 'react'
import type { AttributeLevelUpResponse } from '../../types/AttributeLevelUpResponse'
import { getAttributeVisual } from './attributeVisuals'

type AttributeLevelUpStripProps = {
  items: readonly AttributeLevelUpResponse[]
}

const positionLabels = ['Closest', 'Next', 'Third'] as const

export function AttributeLevelUpStrip({ items }: AttributeLevelUpStripProps) {
  const visibleItems = items.slice(0, 3)

  if (visibleItems.length === 0) {
    return null
  }

  return (
    <section
      aria-label="Next attribute levels"
      className="relative grid min-h-[3.25rem] grid-cols-[2.75rem_repeat(3,minmax(0,1fr))] overflow-hidden rounded-xl border border-line bg-surface-raised shadow-[var(--theme-panel-shadow)] sm:grid-cols-[auto_minmax(0,1.2fr)_minmax(0,0.8fr)_minmax(0,0.8fr)]"
      data-testid="attribute-level-up-strip"
    >
      <div className="flex items-center justify-center gap-2 px-2 text-accent sm:justify-start sm:px-3">
        <span className="grid size-7 shrink-0 place-items-center rounded-lg border border-accent/30 bg-accent-soft">
          <Zap
            aria-hidden="true"
            fill="currentColor"
            size={13}
            strokeWidth={1.9}
          />
        </span>

        <span className="hidden whitespace-nowrap text-[9px] font-bold tracking-[0.14em] uppercase sm:inline">
          Next level
        </span>
      </div>

      {visibleItems.map((item, index) => {
        const visual = getAttributeVisual(item.attributeType)

        const style = {
          '--attribute-accent': visual.colorVariable,
        } as CSSProperties

        return (
          <article
            key={item.attributeType}
            className={[
              'relative flex min-w-0 items-center justify-between gap-2 border-l border-line px-[clamp(0.55rem,0.45rem_+_0.1vw,0.8rem)] py-1.5',
              index === 0 ? 'bg-accent-soft/45' : '',
            ].join(' ')}
            data-attribute-type={item.attributeType}
            style={style}
          >
            {index === 0 && (
              <span
                aria-hidden="true"
                className="absolute inset-x-3 top-0 h-px"
                style={{
                  background:
                    'linear-gradient(90deg, transparent, var(--attribute-accent), transparent)',
                }}
              />
            )}

            <div className="flex min-w-0 items-center gap-2">
              <span
                aria-hidden="true"
                className="hidden size-7 shrink-0 place-items-center rounded-lg border min-[520px]:grid"
                style={{
                  borderColor:
                    'color-mix(in srgb, var(--attribute-accent) 40%, transparent)',
                  backgroundColor:
                    'color-mix(in srgb, var(--attribute-accent) 11%, transparent)',
                  color: 'var(--attribute-accent)',
                }}
              >
                <visual.Icon size={13} strokeWidth={1.9} />
              </span>

              <span className="min-w-0">
                <span className="block text-[8px] font-bold tracking-[0.12em] text-content-subtle uppercase">
                  {positionLabels[index]}
                </span>

                <span className="mt-0.5 flex min-w-0 items-baseline gap-1.5">
                  <span className="truncate text-[clamp(0.65rem,0.58rem_+_0.07vw,0.78rem)] font-semibold">
                    {visual.label}
                  </span>

                  <span className="hidden shrink-0 text-[9px] text-content-subtle min-[680px]:inline">
                    Lv. {item.currentLevel} → {item.currentLevel + 1}
                  </span>
                </span>
              </span>
            </div>

            <span
              className="shrink-0 text-right text-[clamp(0.62rem,0.56rem_+_0.06vw,0.72rem)] font-bold tabular-nums"
              style={{ color: 'var(--attribute-accent)' }}
            >
              {item.xpRemaining.toLocaleString()} XP
            </span>
          </article>
        )
      })}
    </section>
  )
}
