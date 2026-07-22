import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, test } from 'vitest'
import { ThemeProvider } from '../../theme/ThemeProvider'
import { themeStorageKey } from '../../theme/themeStorage'
import { ThemeSelector } from './ThemeSelector'

function renderThemeSelector() {
  return render(
    <ThemeProvider>
      <ThemeSelector />
    </ThemeProvider>,
  )
}

afterEach(() => {
  window.localStorage.clear()
  document.documentElement.removeAttribute('data-theme')
  document.documentElement.style.colorScheme = ''
})

describe('ThemeSelector', () => {
  test('opens the appearance chooser', async () => {
    const user = userEvent.setup()

    renderThemeSelector()

    const trigger = screen.getByRole('button', {
      name: 'Choose appearance theme',
    })

    expect(trigger).toHaveAttribute('aria-expanded', 'false')
    expect(
      screen.queryByRole('dialog', { name: 'Appearance' }),
    ).not.toBeInTheDocument()

    await user.click(trigger)

    expect(trigger).toHaveAttribute('aria-expanded', 'true')

    expect(
      screen.getByRole('dialog', { name: 'Appearance' }),
    ).toBeInTheDocument()

    expect(screen.getByRole('button', { name: /Abyss/ })).toHaveAttribute(
      'aria-pressed',
      'true',
    )
  })

  test('selects and stores another theme', async () => {
    const user = userEvent.setup()

    renderThemeSelector()

    await user.click(
      screen.getByRole('button', {
        name: 'Choose appearance theme',
      }),
    )

    await user.click(
      screen.getByRole('button', {
        name: /Obsidian/,
      }),
    )

    expect(window.localStorage.getItem(themeStorageKey)).toBe('obsidian')

    expect(document.documentElement).toHaveAttribute('data-theme', 'obsidian')

    expect(
      screen.queryByRole('dialog', { name: 'Appearance' }),
    ).not.toBeInTheDocument()
  })

  test('closes the chooser with Escape and returns focus', async () => {
    const user = userEvent.setup()

    renderThemeSelector()

    const trigger = screen.getByRole('button', {
      name: 'Choose appearance theme',
    })

    await user.click(trigger)

    expect(
      screen.getByRole('dialog', { name: 'Appearance' }),
    ).toBeInTheDocument()

    await user.keyboard('{Escape}')

    expect(
      screen.queryByRole('dialog', { name: 'Appearance' }),
    ).not.toBeInTheDocument()

    expect(trigger).toHaveFocus()
  })
})
