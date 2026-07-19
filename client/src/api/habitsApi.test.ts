import { afterEach, describe, expect, it, vi } from 'vitest'
import type { CreateHabitRequest } from '../types/CreateHabitRequest'
import { clearCsrfToken } from './apiClient'
import {
  createHabit,
  deactivateHabit,
  getHabits,
  updateHabit,
} from './habitsApi'
import type { UpdateHabitRequest } from '../types/UpdateHabitRequest'

describe('habitsApi', () => {
  afterEach(() => {
    clearCsrfToken()
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

    await expect(getHabits()).rejects.toThrow('The habits could not be loaded.')
  })

  it('creates a habit using a CSRF-protected request', async () => {
    const createRequest: CreateHabitRequest = {
      name: 'Read C# textbook',
      description: 'Read one chapter.',
      category: 'Learning',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
    }

    const createdHabit = {
      id: '019c0000-0000-7000-8000-000000000001',
      ...createRequest,
      isActive: true,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-19T12:00:00Z',
    }

    const fetchMock = vi.fn<typeof fetch>()

    fetchMock
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            requestToken: 'test-csrf-token',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify(createdHabit), {
          status: 201,
          headers: {
            'Content-Type': 'application/json',
          },
        }),
      )

    vi.stubGlobal('fetch', fetchMock)

    const habit = await createHabit(createRequest)

    expect(fetchMock).toHaveBeenCalledTimes(2)

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/auth/csrf-token',
      expect.objectContaining({
        credentials: 'include',
      }),
    )

    const habitRequestCall = fetchMock.mock.calls[1]

    expect(habitRequestCall).toBeDefined()

    const [habitRequestPath, habitRequestOptions] = habitRequestCall!

    expect(habitRequestPath).toBe('/api/habits')

    expect(habitRequestOptions).toEqual(
      expect.objectContaining({
        method: 'POST',
        credentials: 'include',
        body: JSON.stringify(createRequest),
      }),
    )

    const headers = new Headers(habitRequestOptions?.headers)

    expect(headers.get('Content-Type')).toBe('application/json')

    expect(headers.get('X-CSRF-TOKEN')).toBe('test-csrf-token')

    expect(habit).toEqual(createdHabit)
  })

  it('throws the habit creation error returned by the API', async () => {
    const createRequest: CreateHabitRequest = {
      name: 'Read C# textbook',
      description: null,
      category: null,
      frequencyType: 'daily',
      targetCount: 2,
      difficulty: 'medium',
    }

    const fetchMock = vi.fn<typeof fetch>()

    fetchMock
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            requestToken: 'test-csrf-token',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            status: 400,
            title: 'Invalid habit target count',
            detail: 'Daily habits must have a target count of 1.',
          }),
          {
            status: 400,
            headers: {
              'Content-Type': 'application/problem+json',
            },
          },
        ),
      )

    vi.stubGlobal('fetch', fetchMock)

    await expect(createHabit(createRequest)).rejects.toThrow(
      'Daily habits must have a target count of 1.',
    )
  })

  it('updates a habit using a CSRF-protected request', async () => {
    const habitId = '019c0000-0000-7000-8000-000000000001'

    const updateRequest: UpdateHabitRequest = {
      name: 'Updated reading habit',
      description: 'Read two chapters.',
      category: 'Learning',
      frequencyType: 'weekly',
      targetCount: 3,
      difficulty: 'hard',
    }

    const updatedHabit = {
      id: habitId,
      ...updateRequest,
      isActive: true,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-20T12:00:00Z',
    }

    const fetchMock = vi.fn<typeof fetch>()

    fetchMock
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            requestToken: 'test-csrf-token',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify(updatedHabit), {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        }),
      )

    vi.stubGlobal('fetch', fetchMock)

    const habit = await updateHabit(habitId, updateRequest)

    expect(fetchMock).toHaveBeenCalledTimes(2)

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/auth/csrf-token',
      expect.objectContaining({
        credentials: 'include',
      }),
    )

    const habitRequestCall = fetchMock.mock.calls[1]

    expect(habitRequestCall).toBeDefined()

    const [habitRequestPath, habitRequestOptions] = habitRequestCall!

    expect(habitRequestPath).toBe(`/api/habits/${habitId}`)

    expect(habitRequestOptions).toEqual(
      expect.objectContaining({
        method: 'PUT',
        credentials: 'include',
        body: JSON.stringify(updateRequest),
      }),
    )

    const headers = new Headers(habitRequestOptions?.headers)

    expect(headers.get('Content-Type')).toBe('application/json')

    expect(headers.get('X-CSRF-TOKEN')).toBe('test-csrf-token')

    expect(habit).toEqual(updatedHabit)
  })

  it('throws the habit update error returned by the API', async () => {
    const habitId = '019c0000-0000-7000-8000-000000000001'

    const updateRequest: UpdateHabitRequest = {
      name: 'Updated habit',
      description: null,
      category: null,
      frequencyType: 'daily',
      targetCount: 2,
      difficulty: 'medium',
    }

    const fetchMock = vi.fn<typeof fetch>()

    fetchMock
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            requestToken: 'test-csrf-token',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            status: 400,
            title: 'Invalid habit target count',
            detail: 'Daily habits must have a target count of 1.',
          }),
          {
            status: 400,
            headers: {
              'Content-Type': 'application/problem+json',
            },
          },
        ),
      )

    vi.stubGlobal('fetch', fetchMock)

    await expect(updateHabit(habitId, updateRequest)).rejects.toThrow(
      'Daily habits must have a target count of 1.',
    )
  })

  it('deactivates a habit using a CSRF-protected request', async () => {
    const habitId = '019c0000-0000-7000-8000-000000000001'

    const deactivatedHabit = {
      id: habitId,
      name: 'Read C# textbook',
      description: 'Read one chapter.',
      category: 'Learning',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      isActive: false,
      createdAtUtc: '2026-07-19T12:00:00Z',
      updatedAtUtc: '2026-07-20T12:00:00Z',
    }

    const fetchMock = vi.fn<typeof fetch>()

    fetchMock
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            requestToken: 'test-csrf-token',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify(deactivatedHabit), {
          status: 200,
          headers: {
            'Content-Type': 'application/json',
          },
        }),
      )

    vi.stubGlobal('fetch', fetchMock)

    const habit = await deactivateHabit(habitId)

    expect(fetchMock).toHaveBeenCalledTimes(2)

    expect(fetchMock).toHaveBeenNthCalledWith(
      1,
      '/api/auth/csrf-token',
      expect.objectContaining({
        credentials: 'include',
      }),
    )

    const habitRequestCall = fetchMock.mock.calls[1]

    expect(habitRequestCall).toBeDefined()

    const [habitRequestPath, habitRequestOptions] = habitRequestCall!

    expect(habitRequestPath).toBe(`/api/habits/${habitId}`)

    expect(habitRequestOptions).toEqual(
      expect.objectContaining({
        method: 'DELETE',
        credentials: 'include',
      }),
    )

    const headers = new Headers(habitRequestOptions?.headers)

    expect(headers.get('X-CSRF-TOKEN')).toBe('test-csrf-token')

    expect(habit).toEqual(deactivatedHabit)
  })

  it('throws the habit deactivation error returned by the API', async () => {
    const habitId = '019c0000-0000-7000-8000-000000000001'

    const fetchMock = vi.fn<typeof fetch>()

    fetchMock
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            requestToken: 'test-csrf-token',
          }),
          {
            status: 200,
            headers: {
              'Content-Type': 'application/json',
            },
          },
        ),
      )
      .mockResolvedValueOnce(
        new Response(
          JSON.stringify({
            status: 404,
            title: 'Not Found',
            detail: 'The habit could not be found.',
          }),
          {
            status: 404,
            headers: {
              'Content-Type': 'application/problem+json',
            },
          },
        ),
      )

    vi.stubGlobal('fetch', fetchMock)

    await expect(deactivateHabit(habitId)).rejects.toThrow(
      'The habit could not be found.',
    )
  })
})
