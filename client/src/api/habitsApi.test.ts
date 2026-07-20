import { afterEach, describe, expect, it, vi } from 'vitest'
import type { CompleteHabitRequest } from '../types/CompleteHabitRequest'
import type { CreateHabitRequest } from '../types/CreateHabitRequest'
import type { UpdateHabitRequest } from '../types/UpdateHabitRequest'
import { clearCsrfToken } from './apiClient'
import {
  completeHabit,
  createHabit,
  deactivateHabit,
  getHabits,
  undoHabitCompletion,
  updateHabit,
} from './habitsApi'

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
        category: 'learningAndSkills',
        frequencyType: 'daily',
        targetCount: 1,
        difficulty: 'medium',
        isActive: true,
        isCompletedToday: false,
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
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
    }

    const createdHabit = {
      id: '019c0000-0000-7000-8000-000000000001',
      ...createRequest,
      isActive: true,
      isCompletedToday: false,
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
      category: 'learningAndSkills',
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
      category: 'learningAndSkills',
      frequencyType: 'weekly',
      targetCount: 3,
      difficulty: 'hard',
    }

    const updatedHabit = {
      id: habitId,
      ...updateRequest,
      isActive: true,
      isCompletedToday: false,
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
      category: 'learningAndSkills',
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
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      isActive: false,
      isCompletedToday: false,
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

  it('completes a habit using a CSRF-protected request', async () => {
    const habitId = '019c0000-0000-7000-8000-000000000001'

    const completeRequest: CompleteHabitRequest = {
      notes: 'Completed after dinner.',
    }

    const completionResponse = {
      completion: {
        id: '019c0000-0000-7000-8000-000000000002',
        habitId,
        completedDate: '2026-07-19',
        completedAtUtc: '2026-07-20T01:30:00Z',
        notes: 'Completed after dinner.',
      },
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
        new Response(JSON.stringify(completionResponse), {
          status: 201,
          headers: {
            'Content-Type': 'application/json',
          },
        }),
      )

    vi.stubGlobal('fetch', fetchMock)

    const response = await completeHabit(habitId, completeRequest)

    expect(fetchMock).toHaveBeenCalledTimes(2)

    const completionRequestCall = fetchMock.mock.calls[1]

    expect(completionRequestCall).toBeDefined()

    const [requestPath, requestOptions] = completionRequestCall!

    expect(requestPath).toBe(`/api/habits/${habitId}/completions`)

    expect(requestOptions).toEqual(
      expect.objectContaining({
        method: 'POST',
        credentials: 'include',
        body: JSON.stringify(completeRequest),
      }),
    )

    const headers = new Headers(requestOptions?.headers)

    expect(headers.get('Content-Type')).toBe('application/json')

    expect(headers.get('X-CSRF-TOKEN')).toBe('test-csrf-token')

    expect(response).toEqual(completionResponse)
  })

  it('throws the habit completion error returned by the API', async () => {
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
            status: 409,
            title: 'Habit already completed',
            detail: 'This habit has already been completed for today.',
          }),
          {
            status: 409,
            headers: {
              'Content-Type': 'application/problem+json',
            },
          },
        ),
      )

    vi.stubGlobal('fetch', fetchMock)

    await expect(
      completeHabit(habitId, {
        notes: null,
      }),
    ).rejects.toThrow('This habit has already been completed for today.')
  })

  it('undoes a habit completion using a CSRF-protected request', async () => {
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
        new Response(null, {
          status: 204,
        }),
      )

    vi.stubGlobal('fetch', fetchMock)

    await expect(undoHabitCompletion(habitId)).resolves.toBeUndefined()

    expect(fetchMock).toHaveBeenCalledTimes(2)

    const undoRequestCall = fetchMock.mock.calls[1]

    expect(undoRequestCall).toBeDefined()

    const [requestPath, requestOptions] = undoRequestCall!

    expect(requestPath).toBe(`/api/habits/${habitId}/completions/today`)

    expect(requestOptions).toEqual(
      expect.objectContaining({
        method: 'DELETE',
        credentials: 'include',
      }),
    )

    const headers = new Headers(requestOptions?.headers)

    expect(headers.get('X-CSRF-TOKEN')).toBe('test-csrf-token')
  })

  it('throws the habit completion undo error returned by the API', async () => {
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
            detail: 'No completion exists for today.',
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

    await expect(undoHabitCompletion(habitId)).rejects.toThrow(
      'No completion exists for today.',
    )
  })
})
