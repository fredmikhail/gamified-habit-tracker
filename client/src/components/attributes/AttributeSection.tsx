import { useEffect, useState } from 'react'
import { getAttributes } from '../../api/attributesApi'
import type { AttributeType } from '../../types/AttributeType'
import type { UserAttributeResponse } from '../../types/UserAttributeResponse'

type AttributeSectionProps = {
  refreshKey: number
}

function getAttributeLabel(attributeType: AttributeType): string {
  return attributeType.charAt(0).toUpperCase() + attributeType.slice(1)
}

function getAttributeErrorMessage(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'An unknown attribute-loading error occurred.'
}

function getProgressPercentage(attribute: UserAttributeResponse): number {
  if (attribute.xpNeededForNextLevel <= 0) {
    return 0
  }

  return Math.min(
    100,
    (attribute.xpIntoCurrentLevel / attribute.xpNeededForNextLevel) * 100,
  )
}

export function AttributeSection({ refreshKey }: AttributeSectionProps) {
  const [attributes, setAttributes] = useState<UserAttributeResponse[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)

  useEffect(() => {
    let isActive = true

    async function loadAttributes() {
      setIsLoading(true)

      try {
        const loadedAttributes = await getAttributes()

        if (isActive) {
          setAttributes(loadedAttributes)
          setErrorMessage(null)
        }
      } catch (error) {
        if (isActive) {
          setAttributes([])
          setErrorMessage(getAttributeErrorMessage(error))
        }
      } finally {
        if (isActive) {
          setIsLoading(false)
        }
      }
    }

    void loadAttributes()

    return () => {
      isActive = false
    }
  }, [refreshKey])

  return (
    <section
      aria-labelledby="attribute-section-heading"
      className="mt-8 rounded-lg bg-white p-6 text-left shadow-sm"
    >
      <h2 id="attribute-section-heading" className="text-2xl font-bold">
        Attributes
      </h2>

      <p className="mt-2 text-slate-600">
        Complete related habits to develop specific areas of your character.
      </p>

      {isLoading && (
        <p className="mt-4 text-slate-600">Loading attributes...</p>
      )}

      {!isLoading && errorMessage && (
        <p className="mt-4 text-red-700" role="alert">
          Attribute loading error: {errorMessage}
        </p>
      )}

      {!isLoading && !errorMessage && (
        <ul className="mt-5 grid gap-4 sm:grid-cols-2">
          {attributes.map((attribute) => {
            const label = getAttributeLabel(attribute.attributeType)
            const progressPercentage = getProgressPercentage(attribute)

            return (
              <li
                key={attribute.attributeType}
                className="rounded-lg border border-slate-200 p-4"
              >
                <div className="flex items-center justify-between gap-4">
                  <h3 className="font-semibold">{label}</h3>

                  <span className="rounded bg-slate-100 px-2 py-1 text-sm font-semibold">
                    Level {attribute.level}
                  </span>
                </div>

                <div
                  aria-label={`${label} level progress`}
                  aria-valuemax={attribute.xpNeededForNextLevel}
                  aria-valuemin={0}
                  aria-valuenow={attribute.xpIntoCurrentLevel}
                  className="mt-4 h-3 overflow-hidden rounded-full bg-slate-200"
                  role="progressbar"
                >
                  <div
                    className="h-full rounded-full bg-slate-700"
                    style={{
                      width: `${progressPercentage}%`,
                    }}
                  />
                </div>

                <div className="mt-2 flex justify-between gap-4 text-sm text-slate-600">
                  <span>
                    {attribute.xpIntoCurrentLevel} /{' '}
                    {attribute.xpNeededForNextLevel} XP
                  </span>

                  <span>Total: {attribute.currentXp} XP</span>
                </div>
              </li>
            )
          })}
        </ul>
      )}
    </section>
  )
}
