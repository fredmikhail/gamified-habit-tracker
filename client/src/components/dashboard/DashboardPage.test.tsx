import { act, render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { getDashboard } from '../../api/dashboardApi'
import { completeHabit } from '../../api/habitsApi'
import type { DashboardResponse } from '../../types/DashboardResponse'
import { WorkspaceDataProvider } from '../../workspace/WorkspaceDataProvider'
import { DashboardPage } from './DashboardPage'

vi.mock('../../api/dashboardApi', () => ({
  getDashboard: vi.fn(),
}))

vi.mock('../../api/habitsApi', () => ({
  completeHabit: vi.fn(),
  undoHabitCompletion: vi.fn(),
  getHabits: vi.fn(),
}))

const getDashboardMock = vi.mocked(getDashboard)

const completeHabitMock = vi.mocked(completeHabit)

type ResizeObserverRecord = {
  callback: ResizeObserverCallback
  observedElements: Set<Element>
  observer: ResizeObserver
}

const resizeObserverRecords: ResizeObserverRecord[] = []

class ResizeObserverMock implements ResizeObserver {
  readonly observedElements = new Set<Element>()

  constructor(callback: ResizeObserverCallback) {
    resizeObserverRecords.push({
      callback,
      observedElements: this.observedElements,
      observer: this,
    })
  }

  observe(target: Element): void {
    this.observedElements.add(target)
  }

  unobserve(target: Element): void {
    this.observedElements.delete(target)
  }

  disconnect(): void {
    this.observedElements.clear()
  }
}

function emitResize(target: Element, height: number): void {
  const contentRect = {
    bottom: height,
    height,
    left: 0,
    right: 0,
    top: 0,
    width: 0,
    x: 0,
    y: 0,
    toJSON: () => ({}),
  } as DOMRectReadOnly

  const entry = {
    borderBoxSize: [],
    contentBoxSize: [],
    contentRect,
    devicePixelContentBoxSize: [],
    target,
  } as ResizeObserverEntry

  for (const record of resizeObserverRecords) {
    if (!record.observedElements.has(target)) {
      continue
    }

    record.callback([entry], record.observer)
  }
}

const dashboardResponse: DashboardResponse = {
  overallProgress: {
    totalXp: 300,
    level: 2,
    xpIntoCurrentLevel: 100,
    xpNeededForNextLevel: 250,
  },
  todayActivity: {
    localDate: '2026-07-22',
    completions: 0,
    xpEarned: 0,
  },
  todayExecution: {
    completedDailyHabits: 0,
    totalDailyHabits: 1,
  },
  todayHabits: [
    {
      id: 'habit-1',
      name: 'Read C# textbook',
      category: 'learningAndSkills',
      frequencyType: 'daily',
      targetCount: 1,
      difficulty: 'medium',
      attributeRewards: [
        {
          attributeType: 'mind',
          xpAmount: 14,
        },
        {
          attributeType: 'focus',
          xpAmount: 6,
        },
      ],
      isCompletedToday: false,
      currentStreak: 2,
      longestStreak: 5,
    },
  ],
  attributes: [
    'discipline',
    'fitness',
    'vitality',
    'focus',
    'mind',
    'resilience',
    'social',
    'purpose',
  ].map((attributeType) => ({
    attributeType: attributeType as
      | 'discipline'
      | 'fitness'
      | 'vitality'
      | 'focus'
      | 'mind'
      | 'resilience'
      | 'social'
      | 'purpose',
    currentXp: 50,
    level: 1,
    xpIntoCurrentLevel: 50,
    xpNeededForNextLevel: 100,
  })),
  habitStreaks: [
    {
      habitId: 'habit-1',
      habitName: 'Read C# textbook',
      frequencyType: 'daily',
      currentStreak: 2,
      longestStreak: 5,
    },
  ],
}

function createDashboardWithHabitCount(habitCount: number): DashboardResponse {
  const templateHabit = dashboardResponse.todayHabits[0]

  const todayHabits = Array.from({ length: habitCount }, (_, index) => ({
    ...templateHabit,
    id: `habit-${index + 1}`,
    name: `Responsive habit ${index + 1}`,
    currentStreak: index,
    longestStreak: Math.max(index, 1),
  }))

  return {
    ...dashboardResponse,
    todayExecution: {
      completedDailyHabits: 0,
      totalDailyHabits: habitCount,
    },
    todayHabits,
    habitStreaks: todayHabits.map((habit) => ({
      habitId: habit.id,
      habitName: habit.name,
      frequencyType: habit.frequencyType,
      currentStreak: habit.currentStreak,
      longestStreak: habit.longestStreak,
    })),
  }
}

function renderDashboard() {
  return render(
    <WorkspaceDataProvider>
      <DashboardPage />
    </WorkspaceDataProvider>,
  )
}

describe('DashboardPage', () => {
  beforeEach(() => {
    resizeObserverRecords.length = 0

    vi.stubGlobal('ResizeObserver', ResizeObserverMock)

    getDashboardMock.mockReset()
    completeHabitMock.mockReset()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('renders the action-first command center from one aggregate response', async () => {
    getDashboardMock.mockResolvedValue(dashboardResponse)

    renderDashboard()

    expect(
      await screen.findByRole('heading', {
        name: "Today's habits",
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'Protect the chain',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('heading', {
        name: 'Attribute XP distribution',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('img', {
        name: 'Attribute XP distribution chart',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getByRole('article', {
        name: 'XP today',
      }),
    ).toBeInTheDocument()

    expect(
      screen.getAllByRole('button', {
        name: 'Complete Read C# textbook',
      }),
    ).toHaveLength(2)

    for (const attribute of [
      'Discipline',
      'Fitness',
      'Vitality',
      'Focus',
      'Mind',
      'Resilience',
      'Social',
      'Purpose',
    ]) {
      expect(
        screen.getByRole('heading', {
          name: attribute,
        }),
      ).toBeInTheDocument()
    }

    expect(getDashboardMock).toHaveBeenCalledTimes(1)
  })

  it('adapts today pagination to the measured panel height', async () => {
    getDashboardMock.mockResolvedValue(createDashboardWithHabitCount(8))

    renderDashboard()

    await screen.findByRole('heading', {
      name: "Today's habits",
    })

    const measurementRegion = screen.getByTestId(
      'today-habits-measurement-region',
    )

    expect(
      within(
        screen.getByRole('list', {
          name: "Today's habits list",
        }),
      ).getAllByRole('listitem'),
    ).toHaveLength(4)

    expect(
      screen.getByRole('button', {
        name: "Next today's habits page",
      }),
    ).toBeInTheDocument()

    act(() => {
      emitResize(measurementRegion, 550)
    })

    await waitFor(() => {
      expect(
        within(
          screen.getByRole('list', {
            name: "Today's habits list",
          }),
        ).getAllByRole('listitem'),
      ).toHaveLength(8)
    })

    expect(
      screen.queryByRole('button', {
        name: "Next today's habits page",
      }),
    ).not.toBeInTheDocument()

    act(() => {
      emitResize(measurementRegion, 270)
    })

    await waitFor(() => {
      expect(
        within(
          screen.getByRole('list', {
            name: "Today's habits list",
          }),
        ).getAllByRole('listitem'),
      ).toHaveLength(4)
    })

    expect(
      screen.getByRole('button', {
        name: "Next today's habits page",
      }),
    ).toBeInTheDocument()
  })

  it('completes a habit and refreshes authoritative dashboard totals', async () => {
    const user = userEvent.setup()

    getDashboardMock
      .mockResolvedValueOnce(dashboardResponse)
      .mockResolvedValueOnce({
        ...dashboardResponse,
        todayActivity: {
          ...dashboardResponse.todayActivity,
          completions: 1,
          xpEarned: 20,
        },
        todayExecution: {
          completedDailyHabits: 1,
          totalDailyHabits: 1,
        },
        todayHabits: dashboardResponse.todayHabits.map((habit) => ({
          ...habit,
          isCompletedToday: true,
        })),
      })

    completeHabitMock.mockResolvedValue({
      completion: {
        id: 'completion-1',
        habitId: 'habit-1',
        completedDate: '2026-07-22',
        completedAtUtc: '2026-07-22T14:00:00Z',
        notes: null,
      },
      rewards: dashboardResponse.todayHabits[0].attributeRewards,
    })

    renderDashboard()

    const completionButtons = await screen.findAllByRole('button', {
      name: 'Complete Read C# textbook',
    })

    await user.click(completionButtons[0])

    expect(
      await screen.findByRole('button', {
        name: 'Undo completion for Read C# textbook',
      }),
    ).toBeInTheDocument()

    await waitFor(() => {
      expect(getDashboardMock).toHaveBeenCalledTimes(2)
    })

    expect(completeHabitMock).toHaveBeenCalledWith('habit-1', {
      notes: null,
    })
  })

  it('celebrates when all active daily chains are protected', async () => {
    getDashboardMock.mockResolvedValue({
      ...dashboardResponse,
      todayExecution: {
        completedDailyHabits: 1,
        totalDailyHabits: 1,
      },
      todayHabits: dashboardResponse.todayHabits.map((habit) => ({
        ...habit,
        isCompletedToday: true,
      })),
    })

    renderDashboard()

    expect(await screen.findByText('All chains protected')).toBeInTheDocument()

    expect(
      screen.getByText('Every active daily streak has been secured for today.'),
    ).toBeInTheDocument()

    expect(screen.queryByText('Protect today')).not.toBeInTheDocument()
  })

  it('switches the primary small-screen panel', async () => {
    const user = userEvent.setup()

    getDashboardMock.mockResolvedValue(dashboardResponse)

    renderDashboard()

    await screen.findByRole('heading', {
      name: "Today's habits",
    })

    const summaryPanel = screen.getByTestId('dashboard-summary-panel')

    const todayPanel = screen.getByTestId('dashboard-today-panel')

    expect(summaryPanel).toHaveClass('hidden')
    expect(todayPanel).not.toHaveClass('hidden')

    await user.click(
      screen.getByRole('button', {
        name: 'Summary',
      }),
    )

    expect(summaryPanel).not.toHaveClass('hidden')
    expect(todayPanel).toHaveClass('hidden')
  })
})
