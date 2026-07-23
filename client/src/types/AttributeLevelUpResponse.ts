import type { AttributeType } from './AttributeType'

export type AttributeLevelUpResponse = {
  attributeType: AttributeType
  currentLevel: number
  xpRemaining: number
}
