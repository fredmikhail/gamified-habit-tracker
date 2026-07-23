import {
  Brain,
  Compass,
  Crosshair,
  Dumbbell,
  HeartPulse,
  Shield,
  Sprout,
  Users,
  type LucideIcon,
} from 'lucide-react'
import type { AttributeType } from '../../types/AttributeType'

export type AttributeVisualDefinition = {
  attributeType: AttributeType
  label: string
  description: string
  Icon: LucideIcon
  colorVariable: string
}

export const attributeOrder: readonly AttributeType[] = [
  'discipline',
  'fitness',
  'vitality',
  'focus',
  'mind',
  'resilience',
  'social',
  'purpose',
]

export const attributeVisuals = {
  discipline: {
    attributeType: 'discipline',
    label: 'Discipline',
    description: 'Consistency, self-control, and follow-through.',
    Icon: Sprout,
    colorVariable: 'var(--theme-attribute-discipline)',
  },
  fitness: {
    attributeType: 'fitness',
    label: 'Fitness',
    description: 'Physical strength, movement, and conditioning.',
    Icon: Dumbbell,
    colorVariable: 'var(--theme-attribute-fitness)',
  },
  vitality: {
    attributeType: 'vitality',
    label: 'Vitality',
    description: 'Energy, health, recovery, and physical well-being.',
    Icon: HeartPulse,
    colorVariable: 'var(--theme-attribute-vitality)',
  },
  focus: {
    attributeType: 'focus',
    label: 'Focus',
    description: 'Concentration, clarity, and directed attention.',
    Icon: Crosshair,
    colorVariable: 'var(--theme-attribute-focus)',
  },
  mind: {
    attributeType: 'mind',
    label: 'Mind',
    description: 'Knowledge, learning, creativity, and mental agility.',
    Icon: Brain,
    colorVariable: 'var(--theme-attribute-mind)',
  },
  resilience: {
    attributeType: 'resilience',
    label: 'Resilience',
    description: 'Emotional strength and the ability to recover.',
    Icon: Shield,
    colorVariable: 'var(--theme-attribute-resilience)',
  },
  social: {
    attributeType: 'social',
    label: 'Social',
    description: 'Relationships, communication, and community.',
    Icon: Users,
    colorVariable: 'var(--theme-attribute-social)',
  },
  purpose: {
    attributeType: 'purpose',
    label: 'Purpose',
    description: 'Values, spirituality, direction, and meaning.',
    Icon: Compass,
    colorVariable: 'var(--theme-attribute-purpose)',
  },
} satisfies Record<AttributeType, AttributeVisualDefinition>

export function getAttributeVisual(
  attributeType: AttributeType,
): AttributeVisualDefinition {
  return attributeVisuals[attributeType]
}
