import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, test, vi } from 'vitest'
import { ThemeProvider } from './ThemeProvider'
import { themeStorageKey } from './themeStorage'
import { useTheme } from './useTheme'

const originalMatchMedia = window.matchMedia

function ThemeTestConsumer() {
  const { themePreference, resolvedTheme, setThemePreference } = useTheme()

  return (
    <>
      <p data-testid="theme-preference">{themePreference}</p>
      <p data-testid="resolved-theme">{resolvedTheme}</p>

      <button type="button" onClick={() => setThemePreference('obsidian')}>
        Use Obsidian
      </button>
    </>
  )
}

function setSystemDarkMode(matches: boolean): void {
  Object.defineProperty(window, 'matchMedia', {
    configurable: true,
    writable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches,
      media: query,
      onchange: null,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      addListener: vi.fn(),
      removeListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  })
}

afterEach(() => {
  window.localStorage.clear()
  document.documentElement.removeAttribute('data-theme')
  document.documentElement.style.colorScheme = ''

  Object.defineProperty(window, 'matchMedia', {
    configurable: true,
    writable: true,
    value: originalMatchMedia,
  })

  vi.restoreAllMocks()
})

describe('ThemeProvider', () => {
  test('uses Abyss when no preference has been saved', () => {
    render(
      <ThemeProvider>
        <ThemeTestConsumer />
      </ThemeProvider>,
    )

    expect(screen.getByTestId('theme-preference')).toHaveTextContent('abyss')

    expect(screen.getByTestId('resolved-theme')).toHaveTextContent('abyss')

    expect(document.documentElement).toHaveAttribute('data-theme', 'abyss')

    expect(document.documentElement.style.colorScheme).toBe('dark')
  })

  test('restores a saved theme preference', () => {
    window.localStorage.setItem(themeStorageKey, 'lumen')

    render(
      <ThemeProvider>
        <ThemeTestConsumer />
      </ThemeProvider>,
    )

    expect(screen.getByTestId('theme-preference')).toHaveTextContent('lumen')

    expect(screen.getByTestId('resolved-theme')).toHaveTextContent('lumen')

    expect(document.documentElement).toHaveAttribute('data-theme', 'lumen')

    expect(document.documentElement.style.colorScheme).toBe('light')
  })

  test('updates and stores a selected theme', async () => {
    const user = userEvent.setup()

    render(
      <ThemeProvider>
        <ThemeTestConsumer />
      </ThemeProvider>,
    )

    await user.click(screen.getByRole('button', { name: 'Use Obsidian' }))

    expect(screen.getByTestId('theme-preference')).toHaveTextContent('obsidian')

    expect(screen.getByTestId('resolved-theme')).toHaveTextContent('obsidian')

    expect(window.localStorage.getItem(themeStorageKey)).toBe('obsidian')

    expect(document.documentElement).toHaveAttribute('data-theme', 'obsidian')
  })

  test('resolves System mode from the operating-system preference', () => {
    setSystemDarkMode(false)
    window.localStorage.setItem(themeStorageKey, 'system')

    render(
      <ThemeProvider>
        <ThemeTestConsumer />
      </ThemeProvider>,
    )

    expect(screen.getByTestId('theme-preference')).toHaveTextContent('system')

    expect(screen.getByTestId('resolved-theme')).toHaveTextContent('lumen')

    expect(document.documentElement).toHaveAttribute('data-theme', 'lumen')
  })
})
