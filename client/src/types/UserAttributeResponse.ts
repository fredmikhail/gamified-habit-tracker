import type { AttributeType } from './AttributeType'

export type UserAttributeResponse = {
  attributeType: AttributeType
  currentXp: number
  level: number
  xpIntoCurrentLevel: number
  xpNeededForNextLevel: number
}
