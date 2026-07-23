import { afterEach, describe, expect, it, vi } from 'vitest'
import { clearCsrfToken } from './apiClient'
import { getAttributeOverview, getAttributes } from './attributesApi'

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

  it('requests the authenticated users attribute overview', async () => {
    const responseBody = {
      attributes: [],
      totalAttributeXp: 0,
      balanceScore: 0,
      strongestAttribute: null,
      needsFocusAttribute: null,
      closestToLevelUp: [],
      recentXpTransactions: [],
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

    const overview = await getAttributeOverview()

    expect(fetchMock).toHaveBeenCalledTimes(1)

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/attributes/overview',
      expect.objectContaining({
        method: 'GET',
        credentials: 'include',
      }),
    )

    expect(overview).toEqual(responseBody)
  })

  it('throws the API problem-details message when loading attributes fails', async () => {
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

  it('throws the API problem-details message when loading the overview fails', async () => {
    const fetchMock = vi.fn<typeof fetch>()

    fetchMock.mockResolvedValue(
      new Response(
        JSON.stringify({
          status: 500,
          title: 'Unexpected error',
          detail: 'Attribute overview could not be loaded.',
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

    await expect(getAttributeOverview()).rejects.toThrow(
      'Attribute overview could not be loaded.',
    )
  })
})
