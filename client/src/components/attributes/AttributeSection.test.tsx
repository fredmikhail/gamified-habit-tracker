import { useState } from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getAttributes } from '../../api/attributesApi'
import { WorkspaceDataProvider } from '../../workspace/WorkspaceDataProvider'
import { AttributeSection } from './AttributeSection'

vi.mock('../../api/attributesApi', () => ({
  getAttributes: vi.fn(),
}))

const getAttributesMock = vi.mocked(getAttributes)

const attributeResponses = [
  {
    attributeType: 'fitness' as const,
    currentXp: 225,
    level: 3,
    xpIntoCurrentLevel: 0,
    xpNeededForNextLevel: 150,
  },
  {
    attributeType: 'discipline' as const,
    currentXp: 99,
    level: 1,
    xpIntoCurrentLevel: 99,
    xpNeededForNextLevel: 100,
  },
]

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
    getAttributesMock.mockReset()
  })

  it('displays backend-calculated progress using stable visual identities', async () => {
    getAttributesMock.mockResolvedValue(attributeResponses)

    renderAttributeSection()

    expect(
      await screen.findByRole('heading', {
        name: 'Fitness',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('Lv. 3')).toBeInTheDocument()

    const disciplineProgress = screen.getByRole('progressbar', {
      name: 'Discipline level progress',
    })

    expect(disciplineProgress).toHaveAttribute('aria-valuenow', '99')

    expect(
      document.querySelector('[data-attribute-type="discipline"]'),
    ).toBeInTheDocument()

    expect(
      document.querySelector('[data-attribute-type="fitness"]'),
    ).toBeInTheDocument()
  })

  it('displays the backend error when loading fails', async () => {
    getAttributesMock.mockRejectedValue(
      new Error('Attribute progress could not be loaded.'),
    )

    renderAttributeSection()

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Attribute loading error: Attribute progress could not be loaded.',
    )
  })

  it('reuses cached attributes when the section returns', async () => {
    const user = userEvent.setup()

    getAttributesMock.mockResolvedValue(attributeResponses)

    render(<PersistentAttributeHarness />)

    expect(
      await screen.findByRole('heading', {
        name: 'Fitness',
      }),
    ).toBeInTheDocument()

    expect(getAttributesMock).toHaveBeenCalledTimes(1)

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
        name: 'Fitness',
      }),
    ).toBeInTheDocument()

    expect(getAttributesMock).toHaveBeenCalledTimes(1)
  })
})
