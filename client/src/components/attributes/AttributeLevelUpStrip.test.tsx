import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { AttributeLevelUpResponse } from '../../types/AttributeLevelUpResponse'
import { AttributeLevelUpStrip } from './AttributeLevelUpStrip'

const items: AttributeLevelUpResponse[] = [
  {
    attributeType: 'purpose',
    currentLevel: 12,
    xpRemaining: 80,
  },
  {
    attributeType: 'vitality',
    currentLevel: 9,
    xpRemaining: 85,
  },
  {
    attributeType: 'mind',
    currentLevel: 13,
    xpRemaining: 90,
  },
]

describe('AttributeLevelUpStrip', () => {
  it('shows the three backend-ranked attributes and their remaining XP', () => {
    render(<AttributeLevelUpStrip items={items} />)

    expect(
      screen.getByRole('region', {
        name: 'Next attribute levels',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('Purpose')).toBeInTheDocument()
    expect(screen.getByText('Vitality')).toBeInTheDocument()
    expect(screen.getByText('Mind')).toBeInTheDocument()

    expect(screen.getByText('80 XP')).toBeInTheDocument()
    expect(screen.getByText('85 XP')).toBeInTheDocument()
    expect(screen.getByText('90 XP')).toBeInTheDocument()
  })

  it('renders nothing when the backend queue is empty', () => {
    const { container } = render(<AttributeLevelUpStrip items={[]} />)

    expect(container).toBeEmptyDOMElement()
  })
})
