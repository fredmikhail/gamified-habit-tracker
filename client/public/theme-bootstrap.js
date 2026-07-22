;(() => {
  const storageKey = 'gamified-habit-tracker-theme'
  const validPreferences = ['system', 'abyss', 'obsidian', 'lumen']

  let preference = 'abyss'

  try {
    const storedPreference = window.localStorage.getItem(storageKey)

    if (validPreferences.includes(storedPreference)) {
      preference = storedPreference
    }
  } catch {
    preference = 'abyss'
  }

  const systemPrefersDark =
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-color-scheme: dark)').matches

  const resolvedTheme =
    preference === 'system'
      ? systemPrefersDark
        ? 'abyss'
        : 'lumen'
      : preference

  document.documentElement.dataset.theme = resolvedTheme
  document.documentElement.style.colorScheme =
    resolvedTheme === 'lumen' ? 'light' : 'dark'
})()
