import { afterEach, describe, expect, it, vi } from 'vitest'
import { clearCsrfToken } from './apiClient'
import { getAttributes } from './attributesApi'

describe('attributesApi', () => {
  afterEach(() => {
    clearCsrfToken()
    vi.unstubAllGlobals()
  })

  it('requests the authenticated users attributes', async () => {
    const responseBody = [
      {
        attributeType: 'fitness',
        currentXp: 225,
        level: 3,
        xpIntoCurrentLevel: 0,
        xpNeededForNextLevel: 150,
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

    const attributes = await getAttributes()

    expect(fetchMock).toHaveBeenCalledTimes(1)

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/attributes',
      expect.objectContaining({
        method: 'GET',
        credentials: 'include',
      }),
    )

    expect(attributes).toEqual(responseBody)
  })

  it('throws the API problem-details message when loading fails', async () => {
    const fetchMock = vi.fn<typeof fetch>()

    fetchMock.mockResolvedValue(
      new Response(
        JSON.stringify({
          status: 500,
          title: 'Unexpected error',
          detail: 'Attribute progress could not be loaded.',
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

    await expect(getAttributes()).rejects.toThrow(
      'Attribute progress could not be loaded.',
    )
  })
})
