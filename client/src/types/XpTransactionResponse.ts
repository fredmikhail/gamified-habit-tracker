import type { AttributeType } from './AttributeType'

export type XpTransactionResponse = {
  id: string
  habitName: string
  attributeType: AttributeType
  amount: number
  reason: string
  createdAtUtc: string
}
