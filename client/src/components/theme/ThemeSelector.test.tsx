import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ThemeSelector } from './ThemeSelector'

const { setThemePreferenceMock, useThemeMock } = vi.hoisted(() => ({
  setThemePreferenceMock: vi.fn(),
  useThemeMock: vi.fn(),
}))

vi.mock('../../theme/useTheme', () => ({
  useTheme: useThemeMock,
}))

describe('ThemeSelector', () => {
  beforeEach(() => {
    setThemePreferenceMock.mockReset()

    useThemeMock.mockReturnValue({
      themePreference: 'abyss',
      resolvedTheme: 'abyss',
      setThemePreference: setThemePreferenceMock,
    })
  })

  it('renders an icon-sized appearance trigger', () => {
    render(<ThemeSelector />)

    const trigger = screen.getByRole('button', {
      name: 'Choose appearance theme',
    })

    expect(trigger).toHaveClass('size-11')

    expect(trigger).toHaveAttribute('title', 'Appearance')
  })

  it('opens the appearance dialog', async () => {
    const user = userEvent.setup()

    render(<ThemeSelector />)

    await user.click(
      screen.getByRole('button', {
        name: 'Choose appearance theme',
      }),
    )

    expect(screen.getByRole('dialog')).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'Appearance',
      }),
    ).toBeInTheDocument()

    expect(screen.getByText('Rendering Abyss')).toBeInTheDocument()
  })

  it('marks the current theme as selected', async () => {
    const user = userEvent.setup()

    render(<ThemeSelector />)

    await user.click(
      screen.getByRole('button', {
        name: 'Choose appearance theme',
      }),
    )

    expect(
      screen.getByRole('button', {
        name: /Abyss/,
      }),
    ).toHaveAttribute('aria-pressed', 'true')

    expect(
      screen.getByRole('button', {
        name: /Obsidian/,
      }),
    ).toHaveAttribute('aria-pressed', 'false')
  })

  it('changes the selected theme and closes the dialog', async () => {
    const user = userEvent.setup()

    render(<ThemeSelector />)

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

    expect(setThemePreferenceMock).toHaveBeenCalledWith('obsidian')

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })

  it('closes the dialog with Escape', async () => {
    const user = userEvent.setup()

    render(<ThemeSelector />)

    const trigger = screen.getByRole('button', {
      name: 'Choose appearance theme',
    })

    await user.click(trigger)

    expect(screen.getByRole('dialog')).toBeInTheDocument()

    await user.keyboard('{Escape}')

    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()

    expect(trigger).toHaveFocus()
  })
})
