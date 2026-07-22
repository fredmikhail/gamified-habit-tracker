export const themePreferences = [
  'system',
  'abyss',
  'obsidian',
  'lumen',
] as const

export type ThemePreference = (typeof themePreferences)[number]

export const resolvedThemes = ['abyss', 'obsidian', 'lumen'] as const

export type ResolvedTheme = (typeof resolvedThemes)[number]

export const defaultThemePreference: ThemePreference = 'abyss'

export function isThemePreference(value: unknown): value is ThemePreference {
  return (
    typeof value === 'string' &&
    themePreferences.includes(value as ThemePreference)
  )
}

export function resolveThemePreference(
  preference: ThemePreference,
  systemTheme: ResolvedTheme,
): ResolvedTheme {
  return preference === 'system' ? systemTheme : preference
}
