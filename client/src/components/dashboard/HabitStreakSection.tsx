import type { HabitStreakResponse } from '../../types/HabitStreakResponse'

type HabitStreakSectionProps = {
  habitStreaks: HabitStreakResponse[]
}

function getStreakUnit(
  frequencyType: HabitStreakResponse['frequencyType'],
  streak: number,
): string {
  const unit = frequencyType === 'daily' ? 'day' : 'week'

  return streak === 1 ? unit : `${unit}s`
}

export function HabitStreakSection({ habitStreaks }: HabitStreakSectionProps) {
  return (
    <section
      aria-labelledby="habit-streaks-heading"
      className="mt-6 border-t border-slate-200 pt-6"
    >
      <h3 id="habit-streaks-heading" className="text-xl font-bold">
        Habit streaks
      </h3>

      <p className="mt-1 text-sm text-slate-600">
        Current and longest streaks calculated from your completion history.
      </p>

      {habitStreaks.length === 0 ? (
        <p className="mt-4 text-slate-600">No active habit streaks yet.</p>
      ) : (
        <div className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {habitStreaks.map((habitStreak) => (
            <article
              className="rounded-lg border border-slate-200 p-4"
              key={habitStreak.habitId}
            >
              <h4 className="font-bold">{habitStreak.habitName}</h4>

              <p className="mt-1 text-sm capitalize text-slate-600">
                {habitStreak.frequencyType}
              </p>

              <div className="mt-4 grid grid-cols-2 gap-4">
                <div>
                  <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
                    Current
                  </p>

                  <p className="mt-1 text-xl font-bold">
                    {habitStreak.currentStreak}{' '}
                    {getStreakUnit(
                      habitStreak.frequencyType,
                      habitStreak.currentStreak,
                    )}
                  </p>
                </div>

                <div>
                  <p className="text-xs font-medium uppercase tracking-wide text-slate-500">
                    Longest
                  </p>

                  <p className="mt-1 text-xl font-bold">
                    {habitStreak.longestStreak}{' '}
                    {getStreakUnit(
                      habitStreak.frequencyType,
                      habitStreak.longestStreak,
                    )}
                  </p>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}
    </section>
  )
}
