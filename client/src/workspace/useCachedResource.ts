import { useCallback, useEffect, useRef, useState } from 'react'

type CachedResourceOptions<T> = {
  load: () => Promise<T>
  getErrorMessage: (error: unknown) => string
}

export type CachedResource<T> = {
  data: T | null
  errorMessage: string | null
  isInitialLoading: boolean
  isRefreshing: boolean
  ensureLoaded: () => Promise<void>
  refresh: () => Promise<void>
  updateData: (updater: (currentData: T | null) => T | null) => void
}

export function useCachedResource<T>({
  load,
  getErrorMessage,
}: CachedResourceOptions<T>): CachedResource<T> {
  const [data, setData] = useState<T | null>(null)
  const [errorMessage, setErrorMessage] = useState<string | null>(null)
  const [isInitialLoading, setIsInitialLoading] = useState(false)
  const [isRefreshing, setIsRefreshing] = useState(false)

  const dataRef = useRef<T | null>(null)
  const requestRef = useRef<Promise<void> | null>(null)
  const isMountedRef = useRef(true)

  useEffect(() => {
    isMountedRef.current = true

    return () => {
      isMountedRef.current = false
    }
  }, [])

  const runRequest = useCallback(
    (forceRefresh: boolean): Promise<void> => {
      if (!forceRefresh && dataRef.current !== null) {
        return Promise.resolve()
      }

      if (requestRef.current) {
        return requestRef.current
      }

      const hasCachedData = dataRef.current !== null

      if (hasCachedData) {
        setIsRefreshing(true)
      } else {
        setIsInitialLoading(true)
      }

      setErrorMessage(null)

      const request = load()
        .then((loadedData) => {
          dataRef.current = loadedData

          if (isMountedRef.current) {
            setData(loadedData)
            setErrorMessage(null)
          }
        })
        .catch((error: unknown) => {
          if (!isMountedRef.current) {
            return
          }

          setErrorMessage(getErrorMessage(error))

          if (!hasCachedData) {
            dataRef.current = null
            setData(null)
          }
        })
        .finally(() => {
          requestRef.current = null

          if (isMountedRef.current) {
            setIsInitialLoading(false)
            setIsRefreshing(false)
          }
        })

      requestRef.current = request

      return request
    },
    [getErrorMessage, load],
  )

  const ensureLoaded = useCallback(
    (): Promise<void> => runRequest(false),
    [runRequest],
  )

  const refresh = useCallback(
    (): Promise<void> => runRequest(true),
    [runRequest],
  )

  const updateData = useCallback(
    (updater: (currentData: T | null) => T | null): void => {
      setData((currentData) => {
        const updatedData = updater(currentData)

        dataRef.current = updatedData

        return updatedData
      })
    },
    [],
  )

  return {
    data,
    errorMessage,
    isInitialLoading,
    isRefreshing,
    ensureLoaded,
    refresh,
    updateData,
  }
}
