import { createContext } from 'react'
import type { ResolvedTheme, ThemePreference } from './themeTypes'

export type ThemeContextValue = {
  themePreference: ThemePreference
  resolvedTheme: ResolvedTheme
  setThemePreference: (preference: ThemePreference) => void
}

export const ThemeContext = createContext<ThemeContextValue | undefined>(
  undefined,
)
