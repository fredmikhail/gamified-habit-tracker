import { useEffect, useRef, useState } from 'react'

export type ProgressionFeedbackKind = 'none' | 'xpGain' | 'levelUp'

type ProgressionSnapshot = {
  level: number
  currentXp: number
  progressPercentage: number
}

type ProgressionFeedback = {
  kind: ProgressionFeedbackKind
  animationKey: number
  previousProgressPercentage: number
}

type UseProgressionFeedbackOptions = ProgressionSnapshot

const xpFeedbackDurationMs = 650
const levelUpFeedbackDurationMs = 1100

export function useProgressionFeedback({
  level,
  currentXp,
  progressPercentage,
}: UseProgressionFeedbackOptions): ProgressionFeedback {
  const previousSnapshotRef = useRef<ProgressionSnapshot | null>(null)
  const timeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const [feedback, setFeedback] = useState<ProgressionFeedback>({
    kind: 'none',
    animationKey: 0,
    previousProgressPercentage: progressPercentage,
  })

  useEffect(() => {
    const previousSnapshot = previousSnapshotRef.current

    previousSnapshotRef.current = {
      level,
      currentXp,
      progressPercentage,
    }

    if (previousSnapshot === null) {
      return
    }

    let nextKind: ProgressionFeedbackKind = 'none'

    if (level > previousSnapshot.level) {
      nextKind = 'levelUp'
    } else if (currentXp > previousSnapshot.currentXp) {
      nextKind = 'xpGain'
    }

    if (nextKind === 'none') {
      return
    }

    if (timeoutRef.current !== null) {
      clearTimeout(timeoutRef.current)
    }

    setFeedback((currentFeedback) => ({
      kind: nextKind,
      animationKey: currentFeedback.animationKey + 1,
      previousProgressPercentage: previousSnapshot.progressPercentage,
    }))

    timeoutRef.current = setTimeout(
      () => {
        setFeedback((currentFeedback) => ({
          ...currentFeedback,
          kind: 'none',
        }))

        timeoutRef.current = null
      },
      nextKind === 'levelUp' ? levelUpFeedbackDurationMs : xpFeedbackDurationMs,
    )

    return () => {
      if (timeoutRef.current !== null) {
        clearTimeout(timeoutRef.current)
        timeoutRef.current = null
      }
    }
  }, [currentXp, level, progressPercentage])

  return feedback
}
