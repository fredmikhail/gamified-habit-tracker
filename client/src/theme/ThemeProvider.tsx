import { useEffect, useMemo, useState, type PropsWithChildren } from 'react'
import { ThemeContext } from './ThemeContext'
import { readThemePreference, writeThemePreference } from './themeStorage'
import {
  resolveThemePreference,
  type ResolvedTheme,
  type ThemePreference,
} from './themeTypes'

const darkSystemPreferenceQuery = '(prefers-color-scheme: dark)'

function getSystemTheme(): ResolvedTheme {
  if (
    typeof window === 'undefined' ||
    typeof window.matchMedia !== 'function'
  ) {
    return 'abyss'
  }

  return window.matchMedia(darkSystemPreferenceQuery).matches
    ? 'abyss'
    : 'lumen'
}

export function ThemeProvider({ children }: PropsWithChildren) {
  const [themePreference, setThemePreferenceState] = useState<ThemePreference>(
    () => readThemePreference(),
  )

  const [systemTheme, setSystemTheme] = useState<ResolvedTheme>(getSystemTheme)

  const resolvedTheme = resolveThemePreference(themePreference, systemTheme)

  useEffect(() => {
    if (
      themePreference !== 'system' ||
      typeof window.matchMedia !== 'function'
    ) {
      return
    }

    const mediaQuery = window.matchMedia(darkSystemPreferenceQuery)

    function handleSystemThemeChange(event: MediaQueryListEvent) {
      setSystemTheme(event.matches ? 'abyss' : 'lumen')
    }

    setSystemTheme(mediaQuery.matches ? 'abyss' : 'lumen')

    mediaQuery.addEventListener('change', handleSystemThemeChange)

    return () => {
      mediaQuery.removeEventListener('change', handleSystemThemeChange)
    }
  }, [themePreference])

  useEffect(() => {
    const documentRoot = document.documentElement

    documentRoot.dataset.theme = resolvedTheme
    documentRoot.style.colorScheme =
      resolvedTheme === 'lumen' ? 'light' : 'dark'
  }, [resolvedTheme])

  function setThemePreference(preference: ThemePreference): void {
    writeThemePreference(preference)
    setThemePreferenceState(preference)
  }

  const contextValue = useMemo(
    () => ({
      themePreference,
      resolvedTheme,
      setThemePreference,
    }),
    [themePreference, resolvedTheme],
  )

  return (
    <ThemeContext.Provider value={contextValue}>
      {children}
    </ThemeContext.Provider>
  )
}
