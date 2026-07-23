import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import type { UserAttributeResponse } from '../../types/UserAttributeResponse'
import { AttributeCard } from './AttributeCard'

const resilienceAttribute: UserAttributeResponse = {
  attributeType: 'resilience',
  currentXp: 161,
  level: 2,
  xpIntoCurrentLevel: 61,
  xpNeededForNextLevel: 125,
}

describe('AttributeCard', () => {
  it('uses inline-size containment without collapsing an auto-height grid row', () => {
    render(<AttributeCard compact attribute={resilienceAttribute} />)

    const card = screen.getByTestId('compact-attribute-card')

    expect(card).toHaveStyle({
      containerType: 'inline-size',
    })

    expect(card).toHaveClass('min-h-[6.5rem]')
  })

  it('keeps the full canonical attribute name visible in compact mode', () => {
    render(<AttributeCard compact attribute={resilienceAttribute} />)

    const heading = screen.getByRole('heading', {
      name: 'Resilience',
    })

    expect(heading).toHaveClass('whitespace-nowrap')
    expect(heading).not.toHaveClass('truncate')
  })

  it('preserves the authoritative level progress values', () => {
    render(<AttributeCard compact attribute={resilienceAttribute} />)

    expect(
      screen.getByRole('progressbar', {
        name: 'Resilience level progress',
      }),
    ).toHaveAttribute('aria-valuenow', '61')
  })
})
