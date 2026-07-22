import { StrictMode, useEffect, useState } from 'react'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, test, vi } from 'vitest'
import { useCachedResource, type CachedResource } from './useCachedResource'

type Deferred<T> = {
  promise: Promise<T>
  resolve: (value: T) => void
}

function createDeferred<T>(): Deferred<T> {
  let resolvePromise: ((value: T) => void) | undefined

  const promise = new Promise<T>((resolve) => {
    resolvePromise = resolve
  })

  return {
    promise,
    resolve(value: T) {
      resolvePromise?.(value)
    },
  }
}

function getErrorMessage(error: unknown): string {
  return error instanceof Error ? error.message : 'Unknown loading error.'
}

type ResourceViewProps = {
  resource: CachedResource<string[]>
}

function ResourceView({ resource }: ResourceViewProps) {
  const { data, errorMessage, isInitialLoading, ensureLoaded } = resource

  useEffect(() => {
    void ensureLoaded()
  }, [ensureLoaded])

  if (isInitialLoading) {
    return <p>Initial loading</p>
  }

  if (errorMessage) {
    return <p>{errorMessage}</p>
  }

  return (
    <ul>
      {data?.map((item) => (
        <li key={item}>{item}</li>
      ))}
    </ul>
  )
}

type TestHarnessProps = {
  load: () => Promise<string[]>
}

function TestHarness({ load }: TestHarnessProps) {
  const [isVisible, setIsVisible] = useState(true)

  const resource = useCachedResource({
    load,
    getErrorMessage,
  })

  return (
    <>
      <button type="button" onClick={() => setIsVisible((current) => !current)}>
        Toggle resource
      </button>

      <button type="button" onClick={() => void resource.refresh()}>
        Refresh resource
      </button>

      {resource.isRefreshing && <p>Refreshing</p>}

      {isVisible && <ResourceView resource={resource} />}
    </>
  )
}

describe('useCachedResource', () => {
  test('deduplicates StrictMode effect requests', async () => {
    const deferred = createDeferred<string[]>()
    const load = vi.fn(() => deferred.promise)

    render(
      <StrictMode>
        <TestHarness load={load} />
      </StrictMode>,
    )

    await waitFor(() => {
      expect(load).toHaveBeenCalledTimes(1)
    })

    deferred.resolve(['Discipline'])

    expect(await screen.findByText('Discipline')).toBeInTheDocument()
  })

  test('keeps loaded data when its consumer unmounts and returns', async () => {
    const user = userEvent.setup()
    const load = vi.fn().mockResolvedValue(['Focus'])

    render(<TestHarness load={load} />)

    expect(await screen.findByText('Focus')).toBeInTheDocument()
    expect(load).toHaveBeenCalledTimes(1)

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle resource',
      }),
    )

    expect(screen.queryByText('Focus')).not.toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Toggle resource',
      }),
    )

    expect(screen.getByText('Focus')).toBeInTheDocument()
    expect(load).toHaveBeenCalledTimes(1)
  })

  test('keeps cached data visible during a background refresh', async () => {
    const user = userEvent.setup()
    const refreshDeferred = createDeferred<string[]>()

    const load = vi
      .fn()
      .mockResolvedValueOnce(['Mind'])
      .mockImplementationOnce(() => refreshDeferred.promise)

    render(<TestHarness load={load} />)

    expect(await screen.findByText('Mind')).toBeInTheDocument()

    await user.click(
      screen.getByRole('button', {
        name: 'Refresh resource',
      }),
    )

    expect(screen.getByText('Mind')).toBeInTheDocument()
    expect(screen.getByText('Refreshing')).toBeInTheDocument()
    expect(load).toHaveBeenCalledTimes(2)

    refreshDeferred.resolve(['Resilience'])

    expect(await screen.findByText('Resilience')).toBeInTheDocument()

    expect(screen.queryByText('Mind')).not.toBeInTheDocument()
    expect(screen.queryByText('Refreshing')).not.toBeInTheDocument()
  })
})
