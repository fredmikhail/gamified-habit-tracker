import { render, screen, waitFor } from '@testing-library/react'
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

  it('pulses once when authoritative attribute XP increases', async () => {
    const { rerender } = render(
      <AttributeCard compact attribute={resilienceAttribute} />,
    )

    expect(screen.getByTestId('compact-attribute-card')).toHaveAttribute(
      'data-feedback',
      'none',
    )

    rerender(
      <AttributeCard
        compact
        attribute={{
          ...resilienceAttribute,
          currentXp: 181,
          xpIntoCurrentLevel: 81,
        }}
      />,
    )

    await waitFor(() => {
      expect(screen.getByTestId('compact-attribute-card')).toHaveAttribute(
        'data-feedback',
        'xpGain',
      )
    })

    expect(screen.getByTestId('attribute-progress-fill')).toHaveStyle({
      width: '64.8%',
    })
  })

  it('shows a level-up state only after the backend level increases', async () => {
    const { rerender } = render(
      <AttributeCard compact attribute={resilienceAttribute} />,
    )

    expect(screen.queryByRole('status')).not.toBeInTheDocument()

    rerender(
      <AttributeCard
        compact
        attribute={{
          ...resilienceAttribute,
          currentXp: 230,
          level: 3,
          xpIntoCurrentLevel: 5,
          xpNeededForNextLevel: 150,
        }}
      />,
    )

    await waitFor(() => {
      expect(screen.getByTestId('compact-attribute-card')).toHaveAttribute(
        'data-feedback',
        'levelUp',
      )
    })

    expect(
      screen.getByRole('status', {
        name: 'Resilience leveled up to level 3',
      }),
    ).toHaveTextContent('Lv. 3 ↑')

    expect(screen.getByTestId('attribute-progress-fill')).toHaveAttribute(
      'data-level-up',
      'true',
    )
  })
})
