import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { activateHabit, getHabits } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { WorkspaceDataProvider } from '../../workspace/WorkspaceDataProvider'
import { HabitList } from './HabitList'

vi.mock('../../api/habitsApi', () => ({
  activateHabit: vi.fn(),
  completeHabit: vi.fn(),
  createHabit: vi.fn(),
  deactivateHabit: vi.fn(),
  getHabits: vi.fn(),
  undoHabitCompletion: vi.fn(),
  updateHabit: vi.fn(),
}))

const activateHabitMock = vi.mocked(activateHabit)
const getHabitsMock = vi.mocked(getHabits)

const inactiveHabit: HabitResponse = {
  id: '019c0000-0000-7000-8000-000000000001',
  name: 'Read C# textbook',
  description: 'Read one chapter.',
  category: 'learningAndSkills',
  frequencyType: 'daily',
  targetCount: 1,
  difficulty: 'medium',
  isActive: false,
  isCompletedToday: false,
  createdAtUtc: '2026-07-19T12:00:00Z',
  updatedAtUtc: '2026-07-20T12:00:00Z',
}

describe('HabitList layout', () => {
  beforeEach(() => {
    activateHabitMock.mockReset()
    getHabitsMock.mockReset()
    getHabitsMock.mockResolvedValue([inactiveHabit])
  })

  it('keeps one inactive habit at a normal adaptive row height', async () => {
    const user = userEvent.setup()

    render(
      <WorkspaceDataProvider>
        <HabitList
          onHabitDeactivated={vi.fn()}
          onHabitUpdated={vi.fn()}
          onProgressChanged={vi.fn()}
        />
      </WorkspaceDataProvider>,
    )

    await user.click(
      await screen.findByRole('tab', {
        name: /Inactive Habits/,
      }),
    )

    const rowList = await screen.findByTestId('habit-row-list')

    expect(rowList).toHaveClass(
      'content-start',
      'auto-rows-[var(--ui-list-row-height)]',
    )

    expect(rowList).not.toHaveClass('auto-rows-fr')

    const row = screen.getByRole('listitem')

    expect(row).toHaveClass('h-full', 'min-h-0')

    const rowButton = screen.getByRole('button', {
      name: /Read C# textbook/,
    })

    expect(rowButton).toHaveClass('habit-table-grid')

    expect(
      within(rowButton).getByText('Learning & Skills').closest('div'),
    ).toHaveClass('habit-category-column', 'text-center')
  })

  it('keeps rows inside a clipped viewport above a dedicated footer boundary', async () => {
    const user = userEvent.setup()

    render(
      <WorkspaceDataProvider>
        <HabitList
          onHabitDeactivated={vi.fn()}
          onHabitUpdated={vi.fn()}
          onProgressChanged={vi.fn()}
        />
      </WorkspaceDataProvider>,
    )

    await user.click(
      await screen.findByRole('tab', {
        name: /Inactive Habits/,
      }),
    )

    expect(screen.getByTestId('habit-list-rows-viewport')).toHaveClass(
      'overflow-hidden',
    )

    expect(screen.getByTestId('habit-list-footer')).toHaveClass(
      'relative',
      'z-10',
      'bg-surface-raised',
    )
  })

  it('aligns centered column headings with centered row values', async () => {
    const user = userEvent.setup()

    render(
      <WorkspaceDataProvider>
        <HabitList
          onHabitDeactivated={vi.fn()}
          onHabitUpdated={vi.fn()}
          onProgressChanged={vi.fn()}
        />
      </WorkspaceDataProvider>,
    )

    await user.click(
      await screen.findByRole('tab', {
        name: /Inactive Habits/,
      }),
    )

    const headerGrid = screen.getByTestId('habit-table-header-grid')

    expect(headerGrid).toHaveClass('habit-table-grid', 'px-3')

    for (const heading of [
      'Category',
      'Frequency',
      'Difficulty',
      'Streak',
      'XP reward',
    ]) {
      expect(within(headerGrid).getByText(heading)).toHaveClass('text-center')
    }
  })
})
