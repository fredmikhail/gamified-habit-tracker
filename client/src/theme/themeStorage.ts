import {
  defaultThemePreference,
  isThemePreference,
  type ThemePreference,
} from './themeTypes'

export const themeStorageKey = 'gamified-habit-tracker-theme'

export function readThemePreference(
  storage: Pick<Storage, 'getItem'> = window.localStorage,
): ThemePreference {
  try {
    const storedPreference = storage.getItem(themeStorageKey)

    return isThemePreference(storedPreference)
      ? storedPreference
      : defaultThemePreference
  } catch {
    return defaultThemePreference
  }
}

export function writeThemePreference(
  preference: ThemePreference,
  storage: Pick<Storage, 'setItem'> = window.localStorage,
): void {
  try {
    storage.setItem(themeStorageKey, preference)
  } catch {
    return
  }
}
