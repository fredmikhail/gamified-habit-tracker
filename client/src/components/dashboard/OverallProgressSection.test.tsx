import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getDashboard } from '../../api/dashboardApi'
import { OverallProgressSection } from './OverallProgressSection'

vi.mock('../../api/dashboardApi', () => ({
  getDashboard: vi.fn(),
}))

const getDashboardMock = vi.mocked(getDashboard)

describe('OverallProgressSection', () => {
  beforeEach(() => {
    getDashboardMock.mockReset()
  })

  it('displays backend-calculated overall progress', async () => {
    getDashboardMock.mockResolvedValue({
      overallProgress: {
        totalXp: 300,
        level: 2,
        xpIntoCurrentLevel: 100,
        xpNeededForNextLevel: 250,
      },
    })

    render(<OverallProgressSection refreshKey={0} />)

    expect(await screen.findByText('Level 2')).toBeInTheDocument()
    expect(screen.getByText('300')).toBeInTheDocument()
    expect(screen.getByText('100 / 250 XP')).toBeInTheDocument()

    const progressBar = screen.getByRole('progressbar', {
      name: 'Overall level progress',
    })

    expect(progressBar).toHaveAttribute('aria-valuenow', '100')
    expect(progressBar).toHaveAttribute('aria-valuemax', '250')
  })

  it('reloads progress when the refresh key changes', async () => {
    getDashboardMock
      .mockResolvedValueOnce({
        overallProgress: {
          totalXp: 0,
          level: 1,
          xpIntoCurrentLevel: 0,
          xpNeededForNextLevel: 200,
        },
      })
      .mockResolvedValueOnce({
        overallProgress: {
          totalXp: 20,
          level: 1,
          xpIntoCurrentLevel: 20,
          xpNeededForNextLevel: 200,
        },
      })

    const { rerender } = render(<OverallProgressSection refreshKey={0} />)

    expect(await screen.findByText('0 / 200 XP')).toBeInTheDocument()

    rerender(<OverallProgressSection refreshKey={1} />)

    expect(await screen.findByText('20 / 200 XP')).toBeInTheDocument()
    expect(getDashboardMock).toHaveBeenCalledTimes(2)
  })

  it('displays the backend error when loading fails', async () => {
    getDashboardMock.mockRejectedValue(
      new Error('Dashboard progress could not be loaded.'),
    )

    render(<OverallProgressSection refreshKey={0} />)

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Dashboard loading error: Dashboard progress could not be loaded.',
    )
  })
})
