import type { CSSProperties, ReactNode } from 'react'
import type { AttributeType } from '../../types/AttributeType'
import type { UserAttributeResponse } from '../../types/UserAttributeResponse'
import { getAttributeVisual } from './attributeVisuals'

type AttributeCardProps = {
  attribute: UserAttributeResponse
  compact?: boolean
  isSpotlighted?: boolean
  onSelect?: (attributeType: AttributeType) => void
  onSpotlightChange?: (attributeType: AttributeType | null) => void
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

function FullCardContent({
  attribute,
  progressPercentage,
}: {
  attribute: UserAttributeResponse
  progressPercentage: number
}) {
  const visual = getAttributeVisual(attribute.attributeType)

  return (
    <>
      <span
        aria-hidden="true"
        className="pointer-events-none absolute inset-x-5 top-0 h-px"
        style={{
          background:
            'linear-gradient(90deg, transparent, var(--attribute-accent), transparent)',
          opacity: 0.8,
        }}
      />

      <div className="flex items-start justify-between gap-3">
        <div
          className="grid size-[clamp(2.25rem,1.95rem_+_0.25vw,2.75rem)] shrink-0 place-items-center rounded-xl border"
          style={{
            borderColor:
              'color-mix(in srgb, var(--attribute-accent) 48%, transparent)',
            backgroundColor:
              'color-mix(in srgb, var(--attribute-accent) 13%, transparent)',
            color: 'var(--attribute-accent)',
            boxShadow:
              '0 0 18px color-mix(in srgb, var(--attribute-accent) 13%, transparent)',
          }}
        >
          <visual.Icon aria-hidden="true" size={18} strokeWidth={1.85} />
        </div>

        <span
          className="shrink-0 rounded-lg border px-2 py-1 text-[clamp(0.625rem,0.58rem_+_0.04vw,0.6875rem)] font-bold tracking-[0.06em] uppercase"
          style={{
            borderColor:
              'color-mix(in srgb, var(--attribute-accent) 36%, transparent)',
            backgroundColor:
              'color-mix(in srgb, var(--attribute-accent) 10%, transparent)',
            color: 'var(--attribute-accent)',
          }}
        >
          Lv. {attribute.level}
        </span>
      </div>

      <div className="mt-[clamp(0.65rem,0.55rem_+_0.08vw,0.8rem)] min-w-0">
        <h3 className="truncate text-[clamp(0.78rem,0.7rem_+_0.07vw,0.9rem)] font-semibold tracking-tight">
          {visual.label}
        </h3>

        <p className="mt-0.5 truncate text-[clamp(0.6rem,0.56rem_+_0.035vw,0.66rem)] text-content-subtle">
          {attribute.currentXp.toLocaleString()} total XP
        </p>
      </div>

      <div className="mt-auto pt-[clamp(0.8rem,0.68rem_+_0.1vw,1rem)]">
        <div className="mb-1.5 flex items-center justify-between gap-2 text-[clamp(0.58rem,0.54rem_+_0.035vw,0.64rem)] font-semibold">
          <span className="truncate text-content-muted">
            {attribute.xpIntoCurrentLevel.toLocaleString()} /{' '}
            {attribute.xpNeededForNextLevel.toLocaleString()}
          </span>

          <span
            className="shrink-0"
            style={{ color: 'var(--attribute-accent)' }}
          >
            {Math.round(progressPercentage)}%
          </span>
        </div>

        <div
          aria-label={`${visual.label} level progress`}
          aria-valuemax={attribute.xpNeededForNextLevel}
          aria-valuemin={0}
          aria-valuenow={attribute.xpIntoCurrentLevel}
          className="h-1.5 overflow-hidden rounded-full bg-surface-muted"
          role="progressbar"
        >
          <div
            className="h-full rounded-full transition-[width] duration-300"
            style={{
              width: `${progressPercentage}%`,
              backgroundColor: 'var(--attribute-accent)',
              boxShadow:
                '0 0 14px color-mix(in srgb, var(--attribute-accent) 52%, transparent)',
            }}
          />
        </div>
      </div>
    </>
  )
}

export function AttributeCard({
  attribute,
  compact = false,
  isSpotlighted = false,
  onSelect,
  onSpotlightChange,
}: AttributeCardProps) {
  const visual = getAttributeVisual(attribute.attributeType)
  const progressPercentage = getProgressPercentage(attribute)

  const style = {
    '--attribute-accent': visual.colorVariable,
  } as CSSProperties

  if (compact) {
    return (
      <article
        className="relative flex h-full min-h-[6.5rem] flex-col overflow-hidden rounded-xl border bg-surface"
        data-attribute-type={attribute.attributeType}
        data-testid="compact-attribute-card"
        style={{
          ...style,
          containerType: 'inline-size',
          padding: 'clamp(0.5rem, 5.5cqi, 0.75rem)',
          borderColor:
            'color-mix(in srgb, var(--attribute-accent) 34%, var(--theme-border))',
          backgroundImage:
            'linear-gradient(145deg, color-mix(in srgb, var(--attribute-accent) 9%, transparent), transparent 62%)',
        }}
      >
        <div className="flex min-w-0 items-center justify-between gap-1.5">
          <div
            className="grid shrink-0 place-items-center rounded-lg border"
            style={{
              width: 'clamp(1.75rem, 18cqi, 2.25rem)',
              height: 'clamp(1.75rem, 18cqi, 2.25rem)',
              borderColor:
                'color-mix(in srgb, var(--attribute-accent) 42%, transparent)',
              backgroundColor:
                'color-mix(in srgb, var(--attribute-accent) 14%, transparent)',
              color: 'var(--attribute-accent)',
            }}
          >
            <visual.Icon
              aria-hidden="true"
              strokeWidth={1.9}
              style={{
                width: 'clamp(0.8125rem, 8cqi, 1rem)',
                height: 'clamp(0.8125rem, 8cqi, 1rem)',
              }}
            />
          </div>

          <span
            className="shrink-0 rounded-md border px-1.5 py-0.5 font-bold"
            style={{
              fontSize: 'clamp(0.5rem, 4.7cqi, 0.625rem)',
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

        <h3
          className="min-w-0 whitespace-nowrap font-semibold"
          style={{
            marginTop: 'clamp(0.25rem, 2.6cqi, 0.45rem)',
            fontSize: 'clamp(0.6875rem, 6.8cqi, 0.8125rem)',
            lineHeight: 1.1,
          }}
        >
          {visual.label}
        </h3>

        <div
          className="mt-auto min-h-0"
          style={{ paddingTop: 'clamp(0.25rem, 2.6cqi, 0.45rem)' }}
        >
          <div
            className="flex items-center justify-between gap-1.5 font-medium"
            style={{
              fontSize: 'clamp(0.5625rem, 5.5cqi, 0.6875rem)',
              lineHeight: 1.1,
            }}
          >
            <span className="truncate text-content-muted">
              {attribute.currentXp.toLocaleString()} XP
            </span>

            <span
              className="shrink-0"
              style={{ color: 'var(--attribute-accent)' }}
            >
              {Math.round(progressPercentage)}%
            </span>
          </div>

          <div
            aria-label={`${visual.label} level progress`}
            aria-valuemax={attribute.xpNeededForNextLevel}
            aria-valuemin={0}
            aria-valuenow={attribute.xpIntoCurrentLevel}
            className="h-1.5 overflow-hidden rounded-full bg-surface-muted"
            role="progressbar"
            style={{
              marginTop: 'clamp(0.25rem, 2.4cqi, 0.4rem)',
            }}
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

          <p
            className="truncate text-content-subtle"
            style={{
              marginTop: 'clamp(0.1875rem, 1.8cqi, 0.3rem)',
              fontSize: 'clamp(0.5rem, 4.6cqi, 0.625rem)',
              lineHeight: 1.1,
            }}
          >
            {attribute.xpIntoCurrentLevel} / {attribute.xpNeededForNextLevel}{' '}
            toward next level
          </p>
        </div>
      </article>
    )
  }

  const className = [
    'relative flex min-h-[9.25rem] w-full flex-col overflow-hidden rounded-2xl border p-[clamp(0.75rem,0.66rem_+_0.1vw,0.9375rem)] text-left shadow-[var(--theme-panel-shadow)] transition-[border-color,box-shadow,transform,background-color] duration-200',
    onSelect ? 'cursor-pointer hover:-translate-y-0.5' : '',
  ].join(' ')

  const interactiveStyle = {
    ...style,
    borderColor: isSpotlighted
      ? 'color-mix(in srgb, var(--attribute-accent) 72%, var(--theme-border-strong))'
      : 'color-mix(in srgb, var(--attribute-accent) 30%, var(--theme-border))',
    backgroundColor: 'var(--theme-surface-raised)',
    backgroundImage: isSpotlighted
      ? 'radial-gradient(circle at 18% 0%, color-mix(in srgb, var(--attribute-accent) 18%, transparent), transparent 48%), linear-gradient(145deg, color-mix(in srgb, var(--attribute-accent) 9%, transparent), transparent 62%)'
      : 'linear-gradient(145deg, color-mix(in srgb, var(--attribute-accent) 7%, transparent), transparent 62%)',
    boxShadow: isSpotlighted
      ? 'var(--theme-panel-shadow), 0 0 28px color-mix(in srgb, var(--attribute-accent) 16%, transparent)'
      : 'var(--theme-panel-shadow)',
  } as CSSProperties

  const content: ReactNode = (
    <FullCardContent
      attribute={attribute}
      progressPercentage={progressPercentage}
    />
  )

  if (!onSelect) {
    return (
      <article
        className={className}
        data-attribute-type={attribute.attributeType}
        style={interactiveStyle}
      >
        {content}
      </article>
    )
  }

  return (
    <button
      aria-label={`Inspect ${visual.label}`}
      aria-pressed={isSpotlighted}
      className={className}
      data-attribute-type={attribute.attributeType}
      style={interactiveStyle}
      type="button"
      onBlur={() => onSpotlightChange?.(null)}
      onClick={() => onSelect(attribute.attributeType)}
      onFocus={() => onSpotlightChange?.(attribute.attributeType)}
      onMouseEnter={() => onSpotlightChange?.(attribute.attributeType)}
      onMouseLeave={() => onSpotlightChange?.(null)}
    >
      {content}
    </button>
  )
}
