import { Activity, BookOpenText, ChartNoAxesColumn } from 'lucide-react'
import type { CSSProperties } from 'react'
import type { AttributeOverviewResponse } from '../../types/AttributeOverviewResponse'
import type { UserAttributeResponse } from '../../types/UserAttributeResponse'
import { CommandPanel } from '../ui/CommandPanel'
import { attributeOrder, getAttributeVisual } from './attributeVisuals'

type AttributeSupportPanelsProps = {
  overview: AttributeOverviewResponse
}

function formatTransactionTime(createdAtUtc: string): string {
  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  }).format(new Date(createdAtUtc))
}

function sortAttributes(
  attributes: readonly UserAttributeResponse[],
): UserAttributeResponse[] {
  return [...attributes].sort(
    (first, second) =>
      attributeOrder.indexOf(first.attributeType) -
      attributeOrder.indexOf(second.attributeType),
  )
}

function AttributeMeaningPanel() {
  return (
    <CommandPanel
      Icon={BookOpenText}
      className="h-full min-h-0"
      eyebrow="Character guide"
      title="What Each Attribute Means"
      bodyClassName="min-h-0 overflow-y-auto"
    >
      <div className="space-y-2">
        {attributeOrder.map((attributeType) => {
          const visual = getAttributeVisual(attributeType)

          const style = {
            '--attribute-accent': visual.colorVariable,
          } as CSSProperties

          return (
            <article
              key={attributeType}
              className="flex gap-2.5 rounded-xl border border-line bg-surface px-2.5 py-2"
              data-attribute-type={attributeType}
              style={style}
            >
              <span
                className="grid size-8 shrink-0 place-items-center rounded-lg border"
                style={{
                  borderColor:
                    'color-mix(in srgb, var(--attribute-accent) 38%, transparent)',
                  backgroundColor:
                    'color-mix(in srgb, var(--attribute-accent) 11%, transparent)',
                  color: 'var(--attribute-accent)',
                }}
              >
                <visual.Icon aria-hidden="true" size={15} strokeWidth={1.9} />
              </span>

              <span className="min-w-0">
                <span className="block text-xs font-semibold">
                  {visual.label}
                </span>

                <span className="mt-0.5 block text-[10px] leading-4 text-content-muted">
                  {visual.description}
                </span>
              </span>
            </article>
          )
        })}
      </div>
    </CommandPanel>
  )
}

function RecentXpPanel({ overview }: { overview: AttributeOverviewResponse }) {
  return (
    <CommandPanel
      Icon={Activity}
      className="h-full min-h-0"
      eyebrow="Audit feed"
      title="Recent XP Transactions"
      bodyClassName="min-h-0 overflow-y-auto"
    >
      {overview.recentXpTransactions.length === 0 ? (
        <div className="grid h-full min-h-28 place-items-center text-center">
          <div>
            <p className="text-xs font-semibold text-content">
              No XP activity yet
            </p>

            <p className="mt-1 text-[10px] leading-4 text-content-muted">
              Completed habits and undo actions will appear here.
            </p>
          </div>
        </div>
      ) : (
        <div className="space-y-2">
          {overview.recentXpTransactions.map((transaction) => {
            const visual = getAttributeVisual(transaction.attributeType)
            const isPositive = transaction.amount > 0

            return (
              <article
                key={transaction.id}
                className="rounded-xl border border-line bg-surface px-2.5 py-2"
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="min-w-0">
                    <p className="truncate text-xs font-semibold">
                      {transaction.habitName}
                    </p>

                    <p className="mt-0.5 truncate text-[9px] text-content-subtle">
                      {formatTransactionTime(transaction.createdAtUtc)}
                    </p>
                  </div>

                  <span
                    className={`shrink-0 text-xs font-bold ${
                      isPositive ? 'text-success' : 'text-danger'
                    }`}
                  >
                    {isPositive ? '+' : ''}
                    {transaction.amount} XP
                  </span>
                </div>

                <div className="mt-2 flex items-center justify-between gap-2">
                  <span
                    className="truncate text-[10px] font-semibold"
                    style={{ color: visual.colorVariable }}
                  >
                    {visual.label}
                  </span>

                  <span className="truncate text-[9px] text-content-subtle">
                    {transaction.reason}
                  </span>
                </div>
              </article>
            )
          })}
        </div>
      )}
    </CommandPanel>
  )
}

function AttributeDistributionPanel({
  overview,
}: {
  overview: AttributeOverviewResponse
}) {
  const attributes = sortAttributes(overview.attributes)
  const totalXp = overview.totalAttributeXp

  let consumedPercentage = 0

  const segments = attributes.map((attribute) => {
    const percentage = totalXp === 0 ? 0 : (attribute.currentXp / totalXp) * 100
    const dashLength = Math.max(0, percentage - 0.9)
    const dashOffset = -consumedPercentage

    consumedPercentage += percentage

    return {
      attribute,
      dashLength,
      dashOffset,
      percentage,
    }
  })

  return (
    <CommandPanel
      Icon={ChartNoAxesColumn}
      className="h-full min-h-0"
      eyebrow="Current build"
      title="Attribute XP Distribution"
      bodyClassName="grid min-h-0 grid-cols-[minmax(6.5rem,0.75fr)_minmax(0,1fr)] items-center gap-3 overflow-hidden"
    >
      <div className="relative mx-auto aspect-square w-full max-w-36">
        <svg
          aria-label="Current attribute XP distribution"
          className="h-full w-full -rotate-90"
          role="img"
          viewBox="0 0 120 120"
        >
          <circle
            cx="60"
            cy="60"
            fill="none"
            r="45"
            stroke="var(--theme-surface-muted)"
            strokeWidth="12"
          />

          {segments.map(({ attribute, dashLength, dashOffset }) => (
            <circle
              key={attribute.attributeType}
              cx="60"
              cy="60"
              fill="none"
              pathLength="100"
              r="45"
              stroke={getAttributeVisual(attribute.attributeType).colorVariable}
              strokeDasharray={`${dashLength} ${100 - dashLength}`}
              strokeDashoffset={dashOffset}
              strokeLinecap="round"
              strokeWidth="12"
            />
          ))}
        </svg>

        <div className="absolute inset-0 grid place-items-center text-center">
          <div>
            <p className="text-[clamp(0.9rem,0.78rem_+_0.12vw,1.1rem)] font-semibold">
              {totalXp.toLocaleString()}
            </p>

            <p className="text-[9px] font-bold tracking-[0.12em] text-content-subtle uppercase">
              Total XP
            </p>
          </div>
        </div>
      </div>

      <div className="min-h-0 overflow-y-auto">
        <div className="grid grid-cols-2 gap-x-3 gap-y-2">
          {segments.map(({ attribute, percentage }) => {
            const visual = getAttributeVisual(attribute.attributeType)

            return (
              <div key={attribute.attributeType} className="min-w-0">
                <div className="flex items-center gap-1.5">
                  <span
                    aria-hidden="true"
                    className="size-1.5 shrink-0 rounded-full"
                    style={{ backgroundColor: visual.colorVariable }}
                  />

                  <span className="truncate text-[9px] font-semibold text-content-muted">
                    {visual.label}
                  </span>
                </div>

                <p className="mt-0.5 pl-3 text-[10px] font-bold tabular-nums">
                  {Math.round(percentage)}%
                </p>
              </div>
            )
          })}
        </div>
      </div>
    </CommandPanel>
  )
}

export function AttributeSupportPanels({
  overview,
}: AttributeSupportPanelsProps) {
  return (
    <>
      <AttributeMeaningPanel />

      <div className="grid min-h-0 grid-rows-[minmax(0,1fr)_minmax(0,1fr)] gap-[clamp(0.6rem,0.48rem_+_0.12vw,0.875rem)]">
        <RecentXpPanel overview={overview} />

        <AttributeDistributionPanel overview={overview} />
      </div>
    </>
  )
}
