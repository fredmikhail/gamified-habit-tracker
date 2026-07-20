import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { updateHabit } from '../../api/habitsApi'
import type { HabitResponse } from '../../types/HabitResponse'
import { HabitEditForm } from './HabitEditForm'

vi.mock('../../api/habitsApi', () => ({
  updateHabit: vi.fn(),
}))

const updateHabitMock = vi.mocked(updateHabit)

const existingHabit: HabitResponse = {
  id: '019c0000-0000-7000-8000-000000000001',
  name: 'Read C# textbook',
  description: 'Read one chapter.',
  category: 'learningAndSkills',
  frequencyType: 'weekly',
  targetCount: 3,
  difficulty: 'medium',
  isActive: true,
  isCompletedToday: false,
  createdAtUtc: '2026-07-19T12:00:00Z',
  updatedAtUtc: '2026-07-19T12:00:00Z',
}

describe('HabitEditForm', () => {
  beforeEach(() => {
    updateHabitMock.mockReset()
  })

  it('prefills the form with the existing habit values', () => {
    render(
      <HabitEditForm
        habit={existingHabit}
        onCancel={vi.fn()}
        onHabitUpdated={vi.fn()}
      />,
    )

    expect(
      screen.getByRole('textbox', {
        name: 'Name',
      }),
    ).toHaveValue('Read C# textbook')

    expect(
      screen.getByRole('textbox', {
        name: 'Description',
      }),
    ).toHaveValue('Read one chapter.')

    expect(
      screen.getByRole('combobox', {
        name: 'Category',
      }),
    ).toHaveValue('learningAndSkills')

    expect(
      screen.getByRole('combobox', {
        name: 'Frequency',
      }),
    ).toHaveValue('weekly')

    expect(
      screen.getByRole('spinbutton', {
        name: 'Times per week',
      }),
    ).toHaveValue(3)

    expect(
      screen.getByRole('combobox', {
        name: 'Difficulty',
      }),
    ).toHaveValue('medium')
  })

  it('submits the changed values and reports the updated habit', async () => {
    const user = userEvent.setup()
    const onHabitUpdated = vi.fn()

    const updatedHabit: HabitResponse = {
      ...existingHabit,
      name: 'Build TypeScript portfolio',
      description: null,
      category: 'workAndCareer',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'hard',
      updatedAtUtc: '2026-07-20T12:00:00Z',
    }

    updateHabitMock.mockResolvedValue(updatedHabit)

    render(
      <HabitEditForm
        habit={existingHabit}
        onCancel={vi.fn()}
        onHabitUpdated={onHabitUpdated}
      />,
    )

    const nameInput = screen.getByRole('textbox', {
      name: 'Name',
    })

    await user.clear(nameInput)
    await user.type(
      nameInput,
      'Build TypeScript portfolio',
    )

    await user.clear(
      screen.getByRole('textbox', {
        name: 'Description',
      }),
    )

    await user.selectOptions(
      screen.getByRole('combobox', {
        name: 'Category',
      }),
      'workAndCareer',
    )

    await user.selectOptions(
      screen.getByRole('combobox', {
        name: 'Frequency',
      }),
      'daily',
    )

    await user.selectOptions(
      screen.getByRole('combobox', {
        name: 'Difficulty',
      }),
      'hard',
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Save changes',
      }),
    )

    expect(updateHabitMock).toHaveBeenCalledTimes(1)

    expect(updateHabitMock).toHaveBeenCalledWith(
      existingHabit.id,
      {
        name: 'Build TypeScript portfolio',
        description: null,
        category: 'workAndCareer',
        frequencyType: 'daily',
        targetCount: 1,
        difficulty: 'hard',
      },
    )

    expect(onHabitUpdated).toHaveBeenCalledWith(
      updatedHabit,
    )
  })

  it('shows the pending state while the update is running', async () => {
    const user = userEvent.setup()

    updateHabitMock.mockImplementation(
      () =>
        new Promise<HabitResponse>(
          () => undefined,
        ),
    )

    render(
      <HabitEditForm
        habit={existingHabit}
        onCancel={vi.fn()}
        onHabitUpdated={vi.fn()}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Save changes',
      }),
    )

    expect(
      screen.getByRole('button', {
        name: 'Saving changes...',
      }),
    ).toBeDisabled()

    expect(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    ).toBeDisabled()
  })

  it('shows the backend error when the update fails', async () => {
    const user = userEvent.setup()

    updateHabitMock.mockRejectedValue(
      new Error(
        'Daily habits must have a target count of 1.',
      ),
    )

    render(
      <HabitEditForm
        habit={existingHabit}
        onCancel={vi.fn()}
        onHabitUpdated={vi.fn()}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Save changes',
      }),
    )

    expect(
      await screen.findByRole('alert'),
    ).toHaveTextContent(
      'Habit update error: Daily habits must have a target count of 1.',
    )
  })

  it('reports when the user cancels editing', async () => {
    const user = userEvent.setup()
    const onCancel = vi.fn()

    render(
      <HabitEditForm
        habit={existingHabit}
        onCancel={onCancel}
        onHabitUpdated={vi.fn()}
      />,
    )

    await user.click(
      screen.getByRole('button', {
        name: 'Cancel',
      }),
    )

    expect(onCancel).toHaveBeenCalledTimes(1)
    expect(updateHabitMock).not.toHaveBeenCalled()
  })
})