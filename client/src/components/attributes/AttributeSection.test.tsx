import { useState } from 'react'
import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getAttributeOverview } from '../../api/attributesApi'
import type { AttributeOverviewResponse } from '../../types/AttributeOverviewResponse'
import { WorkspaceDataProvider } from '../../workspace/WorkspaceDataProvider'
import { AttributeSection } from './AttributeSection'

vi.mock('../../api/attributesApi', () => ({
  getAttributeOverview: vi.fn(),
}))

const getAttributeOverviewMock = vi.mocked(getAttributeOverview)

const overviewResponse: AttributeOverviewResponse = {
  attributes: [
    {
      attributeType: 'fitness',
      currentXp: 1300,
      level: 11,
      xpIntoCurrentLevel: 25,
      xpNeededForNextLevel: 350,
    },
    {
      attributeType: 'discipline',
      currentXp: 1860,
      level: 14,
      xpIntoCurrentLevel: 85,
      xpNeededForNextLevel: 425,
    },
    {
      attributeType: 'vitality',
      currentXp: 890,
      level: 9,
      xpIntoCurrentLevel: 90,
      xpNeededForNextLevel: 300,
    },
    {
      attributeType: 'focus',
      currentXp: 1250,
      level: 11,
      xpIntoCurrentLevel: 0,
      xpNeededForNextLevel: 350,
    },
    {
      attributeType: 'mind',
      currentXp: 1710,
      level: 13,
      xpIntoCurrentLevel: 60,
      xpNeededForNextLevel: 400,
    },
    {
      attributeType: 'resilience',
      currentXp: 980,
      level: 9,
      xpIntoCurrentLevel: 180,
      xpNeededForNextLevel: 300,
    },
    {
      attributeType: 'social',
      currentXp: 650,
      level: 7,
      xpIntoCurrentLevel: 25,
      xpNeededForNextLevel: 250,
    },
    {
      attributeType: 'purpose',
      currentXp: 1420,
      level: 12,
      xpIntoCurrentLevel: 45,
      xpNeededForNextLevel: 375,
    },
  ],
  totalAttributeXp: 10060,
  balanceScore: 68,
  strongestAttribute: {
    attributeType: 'discipline',
    currentXp: 1860,
    level: 14,
    xpIntoCurrentLevel: 85,
    xpNeededForNextLevel: 425,
  },
  needsFocusAttribute: {
    attributeType: 'social',
    currentXp: 650,
    level: 7,
    xpIntoCurrentLevel: 25,
    xpNeededForNextLevel: 250,
  },
  closestToLevelUp: [
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
  ],
  recentXpTransactions: [
    {
      id: 'transaction-1',
      habitName: 'Read C# textbook',
      attributeType: 'mind',
      amount: 30,
      reason: 'Habit completion',
      createdAtUtc: '2026-07-23T12:00:00Z',
    },
  ],
}

function renderAttributeSection() {
  return render(
    <WorkspaceDataProvider>
      <AttributeSection />
    </WorkspaceDataProvider>,
  )
}

function PersistentAttributeHarness() {
  const [isVisible, setIsVisible] = useState(true)

  return (
    <WorkspaceDataProvider>
      <button type="button" onClick={() => setIsVisible((current) => !current)}>
        Toggle attributes
      </button>

      {isVisible && <AttributeSection />}
    </WorkspaceDataProvider>
  )
}

describe('AttributeSection', () => {
  beforeEach(() => {
    getAttributeOverviewMock.mockReset()
  })

  it('fits the character command center and level-up strip into one bounded page', async () => {
    getAttributeOverviewMock.mockResolvedValue(overviewResponse)

    renderAttributeSection()

    expect(
      await screen.findByRole('heading', {
        name: 'Your Core Attributes',
      }),
    ).toBeInTheDocument()

    expect(screen.getByTestId('attribute-page')).toHaveClass(
      'h-full',
      'overflow-hidden',
    )

    expect(
      screen.getByRole('heading', {
        name: 'Character Balance Web',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'What Each Attribute Means',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'Recent XP Transactions',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'Attribute XP Distribution',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('region', {
        name: 'Next attribute levels',
      }),
    ).toBeInTheDocument()
  })

  it('renders the backend-ranked next-level queue without recalculating it', async () => {
    getAttributeOverviewMock.mockResolvedValue(overviewResponse)

    renderAttributeSection()

    const levelUpStrip = await screen.findByRole('region', {
      name: 'Next attribute levels',
    })

    expect(within(levelUpStrip).getByText('Purpose')).toBeInTheDocument()
    expect(within(levelUpStrip).getByText('80 XP')).toBeInTheDocument()
    expect(within(levelUpStrip).getByText('Vitality')).toBeInTheDocument()
    expect(within(levelUpStrip).getByText('85 XP')).toBeInTheDocument()
    expect(within(levelUpStrip).getByText('Mind')).toBeInTheDocument()
    expect(within(levelUpStrip).getByText('90 XP')).toBeInTheDocument()
  })

  it('renders real overview data without interactive radar hover controls', async () => {
    getAttributeOverviewMock.mockResolvedValue(overviewResponse)

    renderAttributeSection()

    expect(await screen.findByText('10,060')).toBeInTheDocument()

    expect(screen.getByText('Read C# textbook')).toBeInTheDocument()

    expect(
      screen.getByRole('img', {
        name: /Attribute balance radar/,
      }),
    ).toBeInTheDocument()

    expect(
      screen.queryByRole('button', {
        name: /percent of strongest attribute/,
      }),
    ).not.toBeInTheDocument()
  })

  it('displays the backend error when loading fails', async () => {
    getAttributeOverviewMock.mockRejectedValue(
      new Error('Attribute overview could not be loaded.'),
    )

    renderAttributeSection()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Attribute overview error: Attribute overview could not be loaded.',
    )
  })

  it('reuses the cached overview when the section returns', async () => {
    const user = userEvent.setup()

    getAttributeOverviewMock.mockResolvedValue(overviewResponse)

    render(<PersistentAttributeHarness />)

    expect(
      await screen.findByRole('heading', {
        name: 'Character Balance Web',
      }),
    ).toBeInTheDocument()

    expect(getAttributeOverviewMock).toHaveBeenCalledTimes(1)

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle attributes',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle attributes',
      }),
    )

    expect(
      screen.getByRole('heading', {
        name: 'Character Balance Web',
      }),
    ).toBeInTheDocument()

    expect(getAttributeOverviewMock).toHaveBeenCalledTimes(1)
  })
})
