import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { UserAttributeResponse } from '../../types/UserAttributeResponse'
import { AttributeRadarChart } from './AttributeRadarChart'

const attributes: UserAttributeResponse[] = [
  {
    attributeType: 'discipline',
    currentXp: 200,
    level: 2,
    xpIntoCurrentLevel: 100,
    xpNeededForNextLevel: 125,
  },
  {
    attributeType: 'fitness',
    currentXp: 100,
    level: 2,
    xpIntoCurrentLevel: 0,
    xpNeededForNextLevel: 125,
  },
]

describe('AttributeRadarChart', () => {
  it('exposes normalized values through an accessible chart summary', () => {
    render(<AttributeRadarChart attributes={attributes} />)

    expect(
      screen.getByRole('img', {
        name: /Discipline 100 percent, Fitness 50 percent/,
      }),
    ).toBeInTheDocument()
  })

  it('renders as a non-interactive overview without hover controls', () => {
    render(<AttributeRadarChart attributes={attributes} />)

    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })
})
