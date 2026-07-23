import { afterEach, describe, expect, it, vi } from 'vitest'
import { clearCsrfToken } from './apiClient'
import { getDashboard } from './dashboardApi'

describe('dashboardApi', () => {
  afterEach(() => {
    clearCsrfToken()
    vi.unstubAllGlobals()
  })

  it('requests the authenticated users aggregate dashboard', async () => {
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
        totalDailyHabits: 1,
      },
      todayHabits: [
        {
          id: 'habit-1',
          name: 'Read C# textbook',
          category: 'learningAndSkills',
          frequencyType: 'daily',
          targetCount: 1,
          difficulty: 'medium',
          attributeRewards: [
            {
              attributeType: 'mind',
              xpAmount: 14,
            },
            {
              attributeType: 'focus',
              xpAmount: 6,
            },
          ],
          isCompletedToday: false,
          currentStreak: 2,
          longestStreak: 5,
        },
      ],
      attributes: [
        {
          attributeType: 'discipline',
          currentXp: 99,
          level: 1,
          xpIntoCurrentLevel: 99,
          xpNeededForNextLevel: 100,
        },
      ],
      habitStreaks: [],
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
