import { Gauge, ShieldCheck, Sparkles, TrendingDown } from 'lucide-react'
import { useEffect, useMemo, type CSSProperties } from 'react'
import type { UserAttributeResponse } from '../../types/UserAttributeResponse'
import { useWorkspaceData } from '../../workspace/useWorkspaceData'
import { CommandPanel } from '../ui/CommandPanel'
import { AttributeCard } from './AttributeCard'
import { AttributeRadarChart } from './AttributeRadarChart'
import { AttributeSupportPanels } from './AttributeSupportPanels'
import { attributeOrder, getAttributeVisual } from './attributeVisuals'

function sortAttributes(
  attributes: readonly UserAttributeResponse[],
): UserAttributeResponse[] {
  return [...attributes].sort(
    (first, second) =>
      attributeOrder.indexOf(first.attributeType) -
      attributeOrder.indexOf(second.attributeType),
  )
}

function AttributeIdentity({
  attribute,
  fallbackLabel,
}: {
  attribute: UserAttributeResponse | null
  fallbackLabel: string
}) {
  if (attribute === null) {
    return (
      <p className="mt-1 text-xs font-semibold text-content-muted">
        {fallbackLabel}
      </p>
    )
  }

  const visual = getAttributeVisual(attribute.attributeType)

  const style = {
    '--attribute-accent': visual.colorVariable,
  } as CSSProperties

  return (
    <div className="mt-1 flex min-w-0 items-center gap-2" style={style}>
      <span
        className="grid size-7 shrink-0 place-items-center rounded-lg border"
        style={{
          borderColor:
            'color-mix(in srgb, var(--attribute-accent) 42%, transparent)',
          backgroundColor:
            'color-mix(in srgb, var(--attribute-accent) 12%, transparent)',
          color: 'var(--attribute-accent)',
        }}
      >
        <visual.Icon aria-hidden="true" size={13} strokeWidth={1.9} />
      </span>

      <span className="min-w-0">
        <span className="block truncate text-xs font-semibold">
          {visual.label}
        </span>

        <span className="block truncate text-[9px] text-content-subtle">
          Lv. {attribute.level} · {attribute.currentXp.toLocaleString()} XP
        </span>
      </span>
    </div>
  )
}

export function AttributeSection() {
  const { attributeOverviewResource } = useWorkspaceData()

  const {
    data: overview,
    errorMessage,
    isInitialLoading,
    isRefreshing,
    ensureLoaded,
  } = attributeOverviewResource

  useEffect(() => {
    void ensureLoaded()
  }, [ensureLoaded])

  const orderedAttributes = useMemo(
    () => sortAttributes(overview?.attributes ?? []),
    [overview],
  )

  return (
    <section
      className="relative grid h-full min-h-0 min-w-0 grid-rows-[auto_minmax(0,1fr)] gap-[clamp(0.6rem,0.48rem_+_0.12vw,0.875rem)] overflow-hidden"
      data-testid="attribute-page"
    >
      <div
        aria-hidden="true"
        className="pointer-events-none absolute inset-0 opacity-65"
        style={{
          background:
            'radial-gradient(circle at 18% 3%, color-mix(in srgb, var(--theme-energy-cyan) 7%, transparent), transparent 28%), radial-gradient(circle at 82% 22%, color-mix(in srgb, var(--theme-energy-violet) 8%, transparent), transparent 32%)',
        }}
      />

      <div className="relative min-w-0">
        <div className="flex items-end justify-between gap-3">
          <div className="min-w-0">
            <div className="flex items-center gap-2 text-accent">
              <Sparkles aria-hidden="true" size={13} strokeWidth={1.9} />

              <p className="text-[9px] font-bold tracking-[0.18em] uppercase">
                Character matrix
              </p>
            </div>

            <h2 className="mt-0.5 truncate text-[clamp(0.95rem,0.84rem_+_0.12vw,1.15rem)] font-semibold tracking-tight">
              Your Core Attributes
            </h2>
          </div>

          {isRefreshing && overview && (
            <p
              className="shrink-0 rounded-full border border-accent/25 bg-accent-soft px-2.5 py-1 text-[9px] font-semibold tracking-[0.1em] text-accent uppercase"
              role="status"
            >
              Recalibrating
            </p>
          )}
        </div>

        {isInitialLoading && (
          <div
            aria-label="Loading attribute overview"
            className="mt-2 grid grid-cols-4 gap-2 xl:grid-cols-8"
            role="status"
          >
            {Array.from({ length: 8 }, (_, index) => (
              <div
                key={index}
                className="h-[6.75rem] animate-pulse rounded-xl border border-line bg-surface-raised"
              />
            ))}
          </div>
        )}

        {errorMessage && (
          <p
            className="mt-2 rounded-xl border border-danger/30 bg-danger/10 p-3 text-sm text-danger"
            role="alert"
          >
            Attribute overview error: {errorMessage}
          </p>
        )}

        {overview && (
          <div
            className="mt-2 grid grid-cols-4 gap-[clamp(0.4rem,0.3rem_+_0.08vw,0.65rem)] xl:grid-cols-8"
            data-testid="attribute-card-grid"
          >
            {orderedAttributes.map((attribute) => (
              <AttributeCard
                compact
                key={attribute.attributeType}
                attribute={attribute}
              />
            ))}
          </div>
        )}
      </div>

      {overview && (
        <div className="relative grid min-h-0 min-w-0 gap-[clamp(0.6rem,0.48rem_+_0.12vw,0.875rem)] lg:grid-cols-[minmax(0,1.18fr)_minmax(0,0.72fr)_minmax(0,0.72fr)]">
          <CommandPanel
            className="h-full min-h-0"
            eyebrow="Attribute overview"
            title="Character Balance Web"
            action={
              <span className="rounded-full border border-energy-cyan/25 bg-accent-soft px-2.5 py-1 text-[8px] font-bold tracking-[0.12em] text-energy-cyan uppercase">
                Live profile
              </span>
            }
            bodyClassName="grid min-h-0 grid-rows-[minmax(0,1fr)_auto] p-0"
          >
            <div className="relative min-h-0 overflow-hidden px-2 py-1">
              <AttributeRadarChart attributes={orderedAttributes} />
            </div>

            <div className="grid shrink-0 grid-cols-3 border-t border-line">
              <div className="min-w-0 border-r border-line px-2.5 py-2">
                <div className="flex items-center gap-1.5 text-content-subtle">
                  <Gauge aria-hidden="true" size={12} strokeWidth={1.9} />

                  <p className="truncate text-[8px] font-bold tracking-[0.12em] uppercase">
                    Balance
                  </p>
                </div>

                <div className="mt-1 flex items-baseline gap-1">
                  <span className="text-lg font-semibold tracking-tight">
                    {overview.balanceScore}
                  </span>

                  <span className="text-[9px] text-content-subtle">/ 100</span>
                </div>
              </div>

              <div className="min-w-0 border-r border-line px-2.5 py-2">
                <div className="flex items-center gap-1.5 text-success">
                  <ShieldCheck aria-hidden="true" size={12} strokeWidth={1.9} />

                  <p className="truncate text-[8px] font-bold tracking-[0.12em] uppercase">
                    Strongest
                  </p>
                </div>

                <AttributeIdentity
                  attribute={overview.strongestAttribute}
                  fallbackLabel="No progress"
                />
              </div>

              <div className="min-w-0 px-2.5 py-2">
                <div className="flex items-center gap-1.5 text-warning">
                  <TrendingDown
                    aria-hidden="true"
                    size={12}
                    strokeWidth={1.9}
                  />

                  <p className="truncate text-[8px] font-bold tracking-[0.12em] uppercase">
                    Needs focus
                  </p>
                </div>

                <AttributeIdentity
                  attribute={overview.needsFocusAttribute}
                  fallbackLabel="No progress"
                />
              </div>
            </div>
          </CommandPanel>

          <AttributeSupportPanels overview={overview} />
        </div>
      )}
    </section>
  )
}
