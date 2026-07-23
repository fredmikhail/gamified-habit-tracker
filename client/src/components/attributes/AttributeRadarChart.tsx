import type { UserAttributeResponse } from '../../types/UserAttributeResponse'
import { getAttributeVisual } from './attributeVisuals'

type AttributeRadarChartProps = {
  attributes: readonly UserAttributeResponse[]
}

type Point = {
  x: number
  y: number
}

const viewBoxSize = 480
const center = viewBoxSize / 2
const chartRadius = 150
const labelRadius = 202
const ringCount = 5

function getPolarPoint(index: number, total: number, radius: number): Point {
  const angle = -Math.PI / 2 + (index / total) * Math.PI * 2

  return {
    x: center + Math.cos(angle) * radius,
    y: center + Math.sin(angle) * radius,
  }
}

function toPointString(points: readonly Point[]): string {
  return points.map((point) => `${point.x},${point.y}`).join(' ')
}

function getTextAnchor(x: number): 'start' | 'middle' | 'end' {
  if (x < center - 18) {
    return 'end'
  }

  if (x > center + 18) {
    return 'start'
  }

  return 'middle'
}

function getLabelOffsetY(y: number): number {
  if (y < center - 120) {
    return -7
  }

  if (y > center + 120) {
    return 15
  }

  return 4
}

export function AttributeRadarChart({ attributes }: AttributeRadarChartProps) {
  const strongestXp = Math.max(
    0,
    ...attributes.map((attribute) => attribute.currentXp),
  )

  const normalizedValues = attributes.map((attribute) =>
    strongestXp === 0 ? 0 : attribute.currentXp / strongestXp,
  )

  const averageNormalizedValue =
    normalizedValues.length === 0
      ? 0
      : normalizedValues.reduce((total, value) => total + value, 0) /
        normalizedValues.length

  const ringPolygons = Array.from({ length: ringCount }, (_, ringIndex) => {
    const radius = chartRadius * ((ringIndex + 1) / ringCount)

    return toPointString(
      attributes.map((_, attributeIndex) =>
        getPolarPoint(attributeIndex, attributes.length, radius),
      ),
    )
  })

  const dataPoints = attributes.map((_, attributeIndex) =>
    getPolarPoint(
      attributeIndex,
      attributes.length,
      chartRadius * normalizedValues[attributeIndex]!,
    ),
  )

  const equilibriumPoints = attributes.map((_, attributeIndex) =>
    getPolarPoint(
      attributeIndex,
      attributes.length,
      chartRadius * averageNormalizedValue,
    ),
  )

  const accessibleSummary = attributes
    .map((attribute, index) => {
      const visual = getAttributeVisual(attribute.attributeType)
      const percentage = Math.round((normalizedValues[index] ?? 0) * 100)

      return `${visual.label} ${percentage} percent`
    })
    .join(', ')

  return (
    <div
      className="relative h-full min-h-0 w-full overflow-hidden"
      data-testid="attribute-radar"
      style={{ containerType: 'size' }}
    >
      <div
        className="absolute top-1/2 left-1/2 aspect-square -translate-x-1/2 -translate-y-1/2"
        style={{
          height: 'min(100cqh, 100cqw)',
          width: 'min(100cqh, 100cqw)',
        }}
      >
        <div
          aria-hidden="true"
          className="pointer-events-none absolute inset-[13%] rounded-full blur-3xl"
          style={{
            background:
              'radial-gradient(circle, color-mix(in srgb, var(--theme-energy-blue) 14%, transparent), color-mix(in srgb, var(--theme-energy-violet) 6%, transparent) 44%, transparent 72%)',
          }}
        />

        <svg
          aria-label={`Attribute balance radar. ${accessibleSummary}.`}
          className="relative h-full w-full overflow-visible"
          preserveAspectRatio="xMidYMid meet"
          role="img"
          viewBox={`0 0 ${viewBoxSize} ${viewBoxSize}`}
        >
          <defs>
            <linearGradient
              id="attribute-radar-gradient"
              x1="14%"
              x2="86%"
              y1="8%"
              y2="92%"
            >
              <stop offset="0%" stopColor="var(--theme-energy-cyan)" />
              <stop offset="52%" stopColor="var(--theme-energy-blue)" />
              <stop offset="100%" stopColor="var(--theme-energy-violet)" />
            </linearGradient>

            <filter
              id="attribute-radar-glow"
              height="180%"
              width="180%"
              x="-40%"
              y="-40%"
            >
              <feGaussianBlur result="blur" stdDeviation="4" />

              <feMerge>
                <feMergeNode in="blur" />
                <feMergeNode in="SourceGraphic" />
              </feMerge>
            </filter>
          </defs>

          {ringPolygons.map((points, index) => (
            <polygon
              key={points}
              fill="none"
              opacity={0.42 + index * 0.08}
              points={points}
              stroke="var(--theme-border-strong)"
              strokeWidth={index === ringPolygons.length - 1 ? 1.5 : 1}
            />
          ))}

          {attributes.map((attribute, index) => {
            const axisEnd = getPolarPoint(index, attributes.length, chartRadius)

            return (
              <line
                key={attribute.attributeType}
                opacity="0.5"
                stroke="var(--theme-border-strong)"
                strokeWidth="1"
                x1={center}
                x2={axisEnd.x}
                y1={center}
                y2={axisEnd.y}
              />
            )
          })}

          {strongestXp > 0 && (
            <polygon
              fill="none"
              opacity="0.55"
              points={toPointString(equilibriumPoints)}
              stroke="var(--theme-energy-cyan)"
              strokeDasharray="4 7"
              strokeWidth="1.2"
            />
          )}

          <polygon
            fill="url(#attribute-radar-gradient)"
            fillOpacity={strongestXp === 0 ? 0.04 : 0.18}
            filter="url(#attribute-radar-glow)"
            points={toPointString(dataPoints)}
            stroke="url(#attribute-radar-gradient)"
            strokeLinejoin="round"
            strokeWidth="2.4"
          />

          {attributes.map((attribute, index) => {
            const visual = getAttributeVisual(attribute.attributeType)
            const point = dataPoints[index]!
            const labelPoint = getPolarPoint(
              index,
              attributes.length,
              labelRadius,
            )
            const percentage = Math.round((normalizedValues[index] ?? 0) * 100)

            return (
              <g key={attribute.attributeType}>
                <circle
                  cx={point.x}
                  cy={point.y}
                  fill={visual.colorVariable}
                  filter="url(#attribute-radar-glow)"
                  r="4.2"
                  stroke="var(--theme-surface-raised)"
                  strokeWidth="2"
                />

                <text
                  fill="var(--theme-text-muted)"
                  fontSize="12"
                  fontWeight="600"
                  textAnchor={getTextAnchor(labelPoint.x)}
                  x={labelPoint.x}
                  y={labelPoint.y + getLabelOffsetY(labelPoint.y)}
                >
                  <tspan x={labelPoint.x}>{visual.label}</tspan>

                  <tspan
                    fill={visual.colorVariable}
                    fontSize="10"
                    fontWeight="700"
                    x={labelPoint.x}
                    dy="15"
                  >
                    {percentage}%
                  </tspan>
                </text>
              </g>
            )
          })}

          <circle
            aria-hidden="true"
            cx={center}
            cy={center}
            fill="var(--theme-surface-raised)"
            r="5"
            stroke="var(--theme-energy-blue)"
            strokeWidth="2"
          />

          {strongestXp === 0 && (
            <text
              fill="var(--theme-text-muted)"
              fontSize="13"
              fontWeight="600"
              textAnchor="middle"
              x={center}
              y={center + 30}
            >
              Complete habits to shape your profile
            </text>
          )}
        </svg>
      </div>

      <span className="sr-only">
        The dotted ring marks average attribute strength.
      </span>
    </div>
  )
}
