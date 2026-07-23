import { describe, expect, it } from 'vitest'
import type { AttributeType } from '../../types/AttributeType'
import {
  attributeOrder,
  attributeVisuals,
  getAttributeVisual,
} from './attributeVisuals'

describe('attributeVisuals', () => {
  it('defines all eight supported attributes in canonical order', () => {
    expect(attributeOrder).toEqual([
      'discipline',
      'fitness',
      'vitality',
      'focus',
      'mind',
      'resilience',
      'social',
      'purpose',
    ])
  })

  it('assigns every attribute a unique stable color token', () => {
    const colorVariables = attributeOrder.map(
      (attributeType) => getAttributeVisual(attributeType).colorVariable,
    )

    expect(new Set(colorVariables).size).toBe(attributeOrder.length)
  })

  it('assigns every attribute a label, icon, and description', () => {
    for (const attributeType of attributeOrder) {
      const visual = attributeVisuals[attributeType as AttributeType]

      expect(visual.label).not.toBe('')
      expect(visual.description).not.toBe('')
      expect(visual.Icon).toBeTypeOf('object')
    }
  })
})
