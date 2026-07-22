import { afterEach, describe, expect, it, vi } from 'vitest'
import { clearCsrfToken } from './apiClient'
import { getDashboard } from './dashboardApi'

describe('dashboardApi', () => {
  afterEach(() => {
    clearCsrfToken()
    vi.unstubAllGlobals()
  })

  it('requests the authenticated users dashboard', async () => {
    const responseBody = {
      overallProgress: {
        totalXp: 300,
        level: 2,
        xpIntoCurrentLevel: 100,
        xpNeededForNextLevel: 250,
      },
      todayActivity: {
        localDate: '2026-07-22',
        completions: 0,
        xpEarned: 0,
      },
      todayExecution: {
        completedDailyHabits: 0,
        totalDailyHabits: 0,
      },
      habitStreaks: [
        {
          habitId: '019c0000-0000-7000-8000-000000000001',
          habitName: 'Read C# textbook',
          frequencyType: 'daily',
          currentStreak: 2,
          longestStreak: 5,
        },
      ],
    }

    const fetchMock = vi.fn<typeof fetch>()

    fetchMock.mockResolvedValue(
      new Response(JSON.stringify(responseBody), {
        status: 200,
        headers: {
          'Content-Type': 'application/json',
        },
      }),
    )

    vi.stubGlobal('fetch', fetchMock)

    const dashboard = await getDashboard()

    expect(fetchMock).toHaveBeenCalledTimes(1)

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/dashboard',
      expect.objectContaining({
        method: 'GET',
        credentials: 'include',
      }),
    )

    expect(dashboard).toEqual(responseBody)
  })

  it('throws the API problem-details message when loading fails', async () => {
    const fetchMock = vi.fn<typeof fetch>()

    fetchMock.mockResolvedValue(
      new Response(
        JSON.stringify({
          status: 500,
          title: 'Unexpected error',
          detail: 'Dashboard progress could not be loaded.',
        }),
        {
          status: 500,
          headers: {
            'Content-Type': 'application/problem+json',
          },
        },
      ),
    )

    vi.stubGlobal('fetch', fetchMock)

    await expect(getDashboard()).rejects.toThrow(
      'Dashboard progress could not be loaded.',
    )
  })
})
