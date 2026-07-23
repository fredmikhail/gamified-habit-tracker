import type { CSSProperties } from 'react'
import type { UserAttributeResponse } from '../../types/UserAttributeResponse'
import { getAttributeVisual } from './attributeVisuals'

type AttributeCardProps = {
  attribute: UserAttributeResponse
  compact?: boolean
}

function getProgressPercentage(attribute: UserAttributeResponse): number {
  if (attribute.xpNeededForNextLevel <= 0) {
    return 0
  }

  return Math.min(
    100,
    Math.max(
      0,
      (attribute.xpIntoCurrentLevel / attribute.xpNeededForNextLevel) * 100,
    ),
  )
}

export function AttributeCard({
  attribute,
  compact = false,
}: AttributeCardProps) {
  const visual = getAttributeVisual(attribute.attributeType)

  const progressPercentage = getProgressPercentage(attribute)

  const style = {
    '--attribute-accent': visual.colorVariable,
  } as CSSProperties

  if (compact) {
    return (
      <article
        className="relative flex h-full min-h-0 flex-col overflow-hidden rounded-xl border bg-surface p-2.5"
        data-attribute-type={attribute.attributeType}
        style={{
          ...style,
          borderColor:
            'color-mix(in srgb, var(--attribute-accent) 34%, var(--theme-border))',
          backgroundImage:
            'linear-gradient(145deg, color-mix(in srgb, var(--attribute-accent) 9%, transparent), transparent 62%)',
        }}
      >
        <div className="flex items-center justify-between gap-2">
          <div
            className="grid size-8 shrink-0 place-items-center rounded-lg border"
            style={{
              borderColor:
                'color-mix(in srgb, var(--attribute-accent) 42%, transparent)',
              backgroundColor:
                'color-mix(in srgb, var(--attribute-accent) 14%, transparent)',
              color: 'var(--attribute-accent)',
            }}
          >
            <visual.Icon aria-hidden="true" size={15} strokeWidth={1.9} />
          </div>

          <span
            className="shrink-0 rounded-md border px-1.5 py-0.5 text-[9px] font-bold"
            style={{
              borderColor:
                'color-mix(in srgb, var(--attribute-accent) 38%, transparent)',
              backgroundColor:
                'color-mix(in srgb, var(--attribute-accent) 12%, transparent)',
              color: 'var(--attribute-accent)',
            }}
          >
            Lv. {attribute.level}
          </span>
        </div>

        <h3 className="mt-1.5 truncate text-xs leading-4 font-semibold">
          {visual.label}
        </h3>

        <div className="mt-auto pt-1.5">
          <div className="flex items-center justify-between gap-2 text-[10px] font-medium">
            <span className="text-content-muted">
              {attribute.currentXp.toLocaleString()} XP
            </span>

            <span style={{ color: 'var(--attribute-accent)' }}>
              {Math.round(progressPercentage)}%
            </span>
          </div>

          <div
            aria-label={`${visual.label} level progress`}
            aria-valuemax={attribute.xpNeededForNextLevel}
            aria-valuemin={0}
            aria-valuenow={attribute.xpIntoCurrentLevel}
            className="mt-1.5 h-1.5 overflow-hidden rounded-full bg-surface-muted"
            role="progressbar"
          >
            <div
              className="h-full rounded-full"
              style={{
                width: `${progressPercentage}%`,
                backgroundColor: 'var(--attribute-accent)',
                boxShadow:
                  '0 0 12px color-mix(in srgb, var(--attribute-accent) 45%, transparent)',
              }}
            />
          </div>

          <p className="mt-1 truncate text-[9px] text-content-subtle">
            {attribute.xpIntoCurrentLevel} / {attribute.xpNeededForNextLevel}{' '}
            toward next level
          </p>
        </div>
      </article>
    )
  }

  return (
    <article
      className="relative flex h-full min-h-[10rem] flex-col overflow-hidden rounded-2xl border bg-surface p-4"
      data-attribute-type={attribute.attributeType}
      style={{
        ...style,
        borderColor:
          'color-mix(in srgb, var(--attribute-accent) 30%, var(--theme-border))',
        backgroundImage:
          'linear-gradient(145deg, color-mix(in srgb, var(--attribute-accent) 8%, transparent), transparent 55%)',
      }}
    >
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-center gap-3">
          <div
            className="grid size-10 shrink-0 place-items-center rounded-xl border"
            style={{
              borderColor:
                'color-mix(in srgb, var(--attribute-accent) 40%, transparent)',
              backgroundColor:
                'color-mix(in srgb, var(--attribute-accent) 12%, transparent)',
              color: 'var(--attribute-accent)',
            }}
          >
            <visual.Icon aria-hidden="true" size={19} strokeWidth={1.9} />
          </div>

          <div className="min-w-0">
            <h3 className="truncate text-sm font-semibold">{visual.label}</h3>

            <p className="mt-0.5 text-xs text-content-subtle">
              {attribute.currentXp.toLocaleString()} total XP
            </p>
          </div>
        </div>

        <span
          className="shrink-0 rounded-lg border px-2 py-1 text-xs font-bold"
          style={{
            borderColor:
              'color-mix(in srgb, var(--attribute-accent) 35%, transparent)',
            backgroundColor:
              'color-mix(in srgb, var(--attribute-accent) 10%, transparent)',
            color: 'var(--attribute-accent)',
          }}
        >
          Lv. {attribute.level}
        </span>
      </div>

      <div className="mt-auto pt-4">
        <div
          aria-label={`${visual.label} level progress`}
          aria-valuemax={attribute.xpNeededForNextLevel}
          aria-valuemin={0}
          aria-valuenow={attribute.xpIntoCurrentLevel}
          className="h-2.5 overflow-hidden rounded-full bg-surface-muted"
          role="progressbar"
        >
          <div
            className="h-full rounded-full"
            style={{
              width: `${progressPercentage}%`,
              backgroundColor: 'var(--attribute-accent)',
              boxShadow:
                '0 0 12px color-mix(in srgb, var(--attribute-accent) 45%, transparent)',
            }}
          />
        </div>

        <div className="mt-2 flex items-center justify-between gap-3 text-[11px] font-medium text-content-muted">
          <span>
            {attribute.xpIntoCurrentLevel} / {attribute.xpNeededForNextLevel} XP
          </span>

          <span style={{ color: 'var(--attribute-accent)' }}>
            {Math.round(progressPercentage)}%
          </span>
        </div>
      </div>
    </article>
  )
}
