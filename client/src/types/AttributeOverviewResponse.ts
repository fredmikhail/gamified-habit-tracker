import type { AttributeLevelUpResponse } from './AttributeLevelUpResponse'
import type { UserAttributeResponse } from './UserAttributeResponse'
import type { XpTransactionResponse } from './XpTransactionResponse'

export type AttributeOverviewResponse = {
  attributes: UserAttributeResponse[]
  totalAttributeXp: number
  balanceScore: number
  strongestAttribute: UserAttributeResponse | null
  needsFocusAttribute: UserAttributeResponse | null
  closestToLevelUp: AttributeLevelUpResponse[]
  recentXpTransactions: XpTransactionResponse[]
}
