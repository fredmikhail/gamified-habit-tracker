import { Sparkles } from 'lucide-react'
import { useEffect } from 'react'
import { useWorkspaceData } from '../../workspace/useWorkspaceData'
import { CommandPanel } from '../ui/CommandPanel'
import { AttributeCard } from './AttributeCard'
import { attributeOrder } from './attributeVisuals'

export function AttributeSection() {
  const { attributesResource } = useWorkspaceData()

  const {
    data: attributes,
    errorMessage,
    isInitialLoading,
    isRefreshing,
    ensureLoaded,
  } = attributesResource

  useEffect(() => {
    void ensureLoaded()
  }, [ensureLoaded])

  const orderedAttributes =
    attributes === null
      ? []
      : [...attributes].sort(
          (first, second) =>
            attributeOrder.indexOf(first.attributeType) -
            attributeOrder.indexOf(second.attributeType),
        )

  return (
    <CommandPanel
      Icon={Sparkles}
      className="min-h-full"
      eyebrow="Character matrix"
      title="Core attributes"
    >
      <p className="text-sm leading-6 text-content-muted">
        Complete related habits to develop specific areas of your character.
      </p>

      {isInitialLoading && (
        <p className="mt-5 text-sm text-content-muted">Loading attributes...</p>
      )}

      {isRefreshing && attributes && (
        <p className="mt-3 text-xs text-content-muted" role="status">
          Refreshing attributes...
        </p>
      )}

      {errorMessage && (
        <p
          className="mt-4 rounded-xl border border-danger/30 bg-danger/10 p-3 text-sm text-danger"
          role="alert"
        >
          Attribute loading error: {errorMessage}
        </p>
      )}

      {attributes !== null && attributes.length === 0 && (
        <p className="mt-5 text-sm text-content-muted">
          No attribute progression is available yet.
        </p>
      )}

      {orderedAttributes.length > 0 && (
        <ul className="mt-5 grid gap-3 sm:grid-cols-2 xl:grid-cols-4 2xl:grid-cols-8">
          {orderedAttributes.map((attribute) => (
            <li key={attribute.attributeType}>
              <AttributeCard attribute={attribute} />
            </li>
          ))}
        </ul>
      )}
    </CommandPanel>
  )
}
