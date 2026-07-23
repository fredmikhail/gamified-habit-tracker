import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { AttributeOverviewResponse } from '../../types/AttributeOverviewResponse'
import { AttributeSupportPanels } from './AttributeSupportPanels'

const overview: AttributeOverviewResponse = {
  attributes: [],
  totalAttributeXp: 0,
  balanceScore: 0,
  strongestAttribute: null,
  needsFocusAttribute: null,
  closestToLevelUp: [],
  recentXpTransactions: [],
}

describe('AttributeSupportPanels', () => {
  it('fits all eight attribute meanings into a fixed non-scrollable grid', () => {
    render(<AttributeSupportPanels overview={overview} />)

    const meaningGrid = screen.getByTestId('attribute-meaning-grid')

    expect(meaningGrid).toHaveClass(
      'h-full',
      'grid-rows-[repeat(8,minmax(0,1fr))]',
    )

    expect(meaningGrid.querySelectorAll('[data-attribute-type]')).toHaveLength(
      8,
    )

    expect(meaningGrid.closest('div')?.parentElement).not.toHaveClass(
      'overflow-y-auto',
    )
  })

  it('scales meaning content from the available panel height', () => {
    render(<AttributeSupportPanels overview={overview} />)

    expect(screen.getByTestId('attribute-meaning-container')).toHaveStyle({
      containerType: 'size',
    })

    const disciplineRow = screen
      .getByTestId('attribute-meaning-grid')
      .querySelector('[data-attribute-type="discipline"]')

    expect(disciplineRow).toHaveStyle({
      gap: 'clamp(0.5rem, 1.2cqh, 0.75rem)',
      paddingInline: 'clamp(0.5rem, 1.3cqh, 0.85rem)',
    })
  })
})
