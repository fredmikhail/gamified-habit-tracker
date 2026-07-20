import type { HabitCategory } from '../../types/HabitCategory'

export const habitCategoryOptions = [
  {
    value: 'fitnessAndMovement',
    label: 'Fitness & Movement',
  },
  {
    value: 'healthAndRecovery',
    label: 'Health & Recovery',
  },
  {
    value: 'learningAndSkills',
    label: 'Learning & Skills',
  },
  {
    value: 'workAndCareer',
    label: 'Work & Career',
  },
  {
    value: 'dailyLifeAndOrganization',
    label: 'Daily Life & Organization',
  },
  {
    value: 'moneyAndFinance',
    label: 'Money & Finance',
  },
  {
    value: 'relationshipsAndCommunity',
    label: 'Relationships & Community',
  },
  {
    value: 'emotionalWellBeing',
    label: 'Emotional Well-Being',
  },
  {
    value: 'spiritualityAndPurpose',
    label: 'Spirituality & Purpose',
  },
  {
    value: 'creativityAndRecreation',
    label: 'Creativity & Recreation',
  },
  {
    value: 'selfControlAndBoundaries',
    label: 'Self-Control & Boundaries',
  },
  {
    value: 'generalGrowth',
    label: 'General Growth',
  },
] as const satisfies ReadonlyArray<{
  value: HabitCategory
  label: string
}>

export function getHabitCategoryLabel(category: HabitCategory): string {
  const option = habitCategoryOptions.find(
    (categoryOption) => categoryOption.value === category,
  )

  return option?.label ?? category
}
