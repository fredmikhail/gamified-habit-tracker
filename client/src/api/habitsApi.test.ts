import { afterEach, describe, expect, it, vi } from 'vitest'
import { getHabits } from './habitsApi'

describe('getHabits', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('requests active habits by default', async () => {
    const responseBody = [
      {
        id: '019c0000-0000-7000-8000-000000000001',
        name: 'Read C# textbook',
        description: 'Read one chapter.',
        category: 'Learning',
        frequencyType: 'daily',
        targetCount: 1,
        difficulty: 'medium',
        isActive: true,
        createdAtUtc: '2026-07-19T12:00:00Z',
        updatedAtUtc: '2026-07-19T12:00:00Z',
      },
    ]

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

    const habits = await getHabits()

    expect(fetchMock).toHaveBeenCalledTimes(1)

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/habits',
      expect.objectContaining({
        method: 'GET',
        credentials: 'include',
      }),
    )

    expect(habits).toEqual(responseBody)
  })

  it('includes inactive habits when requested', async () => {
    const fetchMock = vi.fn<typeof fetch>()

    fetchMock.mockResolvedValue(
      new Response(JSON.stringify([]), {
        status: 200,
        headers: {
          'Content-Type': 'application/json',
        },
      }),
    )

    vi.stubGlobal('fetch', fetchMock)

    await getHabits(true)

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/habits?includeInactive=true',
      expect.objectContaining({
        method: 'GET',
        credentials: 'include',
      }),
    )
  })

  it('throws the API problem-details message when the request fails', async () => {
    const fetchMock = vi.fn<typeof fetch>()

    fetchMock.mockResolvedValue(
      new Response(
        JSON.stringify({
          status: 500,
          title: 'Unexpected error',
          detail: 'The habits could not be loaded.',
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

    await expect(getHabits()).rejects.toThrow(
      'The habits could not be loaded.',
    )
  })
})