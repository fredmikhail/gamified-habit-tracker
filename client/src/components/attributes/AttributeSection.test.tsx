import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getAttributes } from '../../api/attributesApi'
import { AttributeSection } from './AttributeSection'

vi.mock('../../api/attributesApi', () => ({
  getAttributes: vi.fn(),
}))

const getAttributesMock = vi.mocked(getAttributes)

describe('AttributeSection', () => {
  beforeEach(() => {
    getAttributesMock.mockReset()
  })

  it('displays backend-calculated attribute progress', async () => {
    getAttributesMock.mockResolvedValue([
      {
        attributeType: 'fitness',
        currentXp: 225,
        level: 3,
        xpIntoCurrentLevel: 0,
        xpNeededForNextLevel: 150,
      },
      {
        attributeType: 'discipline',
        currentXp: 99,
        level: 1,
        xpIntoCurrentLevel: 99,
        xpNeededForNextLevel: 100,
      },
    ])

    render(<AttributeSection refreshKey={0} />)

    expect(
      await screen.findByRole('heading', {
        name: 'Fitness',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('Level 3')).toBeInTheDocument()
    expect(screen.getByText('0 / 150 XP')).toBeInTheDocument()
    expect(screen.getByText('Total: 225 XP')).toBeInTheDocument()

    const disciplineProgress = screen.getByRole('progressbar', {
      name: 'Discipline level progress',
    })

    expect(disciplineProgress).toHaveAttribute('aria-valuenow', '99')
    expect(disciplineProgress).toHaveAttribute('aria-valuemax', '100')
  })

  it('displays the backend error when loading fails', async () => {
    getAttributesMock.mockRejectedValue(
      new Error('Attribute progress could not be loaded.'),
    )

    render(<AttributeSection refreshKey={0} />)

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Attribute loading error: Attribute progress could not be loaded.',
    )
  })
})
