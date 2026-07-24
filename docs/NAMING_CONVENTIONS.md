# Gamified Habit Tracker — Naming Conventions

**Status:** Canonical through Phase 7

## Rule

Each stable concept has one name across:

- C#;
- TypeScript;
- JSON;
- PostgreSQL;
- tests;
- UI;
- documentation.

Friendly display labels may format a term without renaming the domain concept.

Renames require an explicit cross-layer decision.

## General Conventions

- Use descriptive responsibility-based names.
- Keep a file aligned with its primary type.
- Avoid aliases for an existing concept.
- Avoid speculative names for unimplemented features.
- Do not rename stable types during unrelated debugging.
- Avoid `Manager`, `Processor`, or `Helper` when an established service owns the behavior.
- Do not name components after a temporary screen position.
- Do not expose persistence-only terminology through public DTOs.

## C# Conventions

- namespaces, types, methods, and public properties: `PascalCase`;
- local variables and parameters: `camelCase`;
- private fields: `_camelCase`;
- interfaces: `I` prefix;
- asynchronous methods: `Async` suffix;
- request DTOs: `Request` suffix;
- response DTOs: `Response` suffix;
- controllers: `Controller` suffix;
- services: `Service` suffix;
- EF Core configurations: `Configuration` suffix;
- tests: `Tests` suffix.

Root backend namespace:

```text
HabitTracker.Api
```

## TypeScript and React Conventions

- components and exported types: `PascalCase`;
- variables and functions: `camelCase`;
- hooks: `use` prefix;
- component files: match the component;
- API modules: lower camel-case domain plus `Api`;
- test files: `.test.ts` or `.test.tsx`;
- browser tests: `.spec.ts`.

Examples:

- `AppShell`
- `DashboardPage`
- `HabitSection`
- `HabitList`
- `AttributeSection`
- `ThemeProvider`
- `WorkspaceDataProvider`
- `CommandPanel`
- `MetricPanel`
- `AttributeRadarChart`
- `useProgressionFeedback`
- `authApi.ts`
- `habitsApi.ts`
- `attributesApi.ts`
- `dashboardApi.ts`

## JSON and Routes

JSON properties use camel case:

- `currentUser`;
- `isCompletedToday`;
- `pendingConfiguration`;
- `xpIntoCurrentLevel`;
- `recentXpTransactions`.

Enum strings use camel case.

Routes use lowercase resource nouns and kebab-free segments:

```text
/api/auth
/api/habits
/api/attributes
/api/dashboard
```

Action segments use established verbs only when a resource-oriented method is not sufficient:

```text
POST /api/habits/{habitId}/activate
```

## PostgreSQL

Physical identifiers use lowercase `snake_case`.

Examples:

- `user_settings`;
- `habit_configuration_versions`;
- `completed_at_utc`;
- `effective_to_date_exclusive`;
- `week_starts_on`;
- `xp_transactions`.

Constraint and index names use descriptive `snake_case`.

## Canonical Entities

Implemented:

- `User`
- `UserSettings`
- `Habit`
- `HabitConfigurationVersion`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

Reserved post-MVP:

- `Milestone`
- `UserMilestone`

Avoid substitutes such as:

| Canonical                   | Avoid                                |
| --------------------------- | ------------------------------------ |
| `UserSettings`              | `Preferences`, `UserPreferences`     |
| `Habit`                     | `Routine`, `TaskDefinition`          |
| `HabitConfigurationVersion` | `HabitRule`, `HabitSnapshot`         |
| `HabitCompletion`           | `HabitLog`, `CompletionLog`          |
| `HabitAttributeReward`      | `RewardMapping`, `HabitStatReward`   |
| `UserAttribute`             | `CharacterStat`, `AttributeProgress` |
| `XpTransaction`             | `ExperienceEvent`, `XpRecord`        |

## Canonical Services

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

Responsibilities:

| Service             | Responsibility                                                                    |
| ------------------- | --------------------------------------------------------------------------------- |
| `AuthService`       | Registration, credentials, account lookup, current-user mapping                   |
| `HabitService`      | Habit lifecycle, reads, edits, activation, deactivation, configuration scheduling |
| `CompletionService` | Completion and undo orchestration                                                 |
| `XpService`         | Reward and level formulas                                                         |
| `AttributeService`  | Attribute reads, XP persistence and reversal, attribute overview                  |
| `StreakService`     | Daily and weekly streak calculation                                               |
| `DashboardService`  | Dashboard aggregation                                                             |

Do not create `ProgressService`, `GameService`, `StatsService`, or a service named after a UI panel.

## Canonical Domain Enums

- `AttributeType`
- `HabitCategory`
- `HabitDifficulty`
- `HabitFrequencyType`
- `WeekStartDay`

C# members use PascalCase.

Public JSON values use camel case.

Numeric enum values are not part of the API contract.

## Canonical Attributes

C#:

- `Discipline`
- `Fitness`
- `Vitality`
- `Focus`
- `Mind`
- `Resilience`
- `Social`
- `Purpose`

JSON:

- `discipline`
- `fitness`
- `vitality`
- `focus`
- `mind`
- `resilience`
- `social`
- `purpose`

Obsolete names must not be reintroduced:

| Obsolete                       | Canonical                |
| ------------------------------ | ------------------------ |
| `Strength`                     | `Fitness`                |
| `Recovery`                     | `Vitality`               |
| `Career` as an attribute       | `Focus` where applicable |
| `Spirituality` as an attribute | `Purpose`                |

These are replacements, not runtime aliases.

## Canonical Habit Categories

C#:

- `FitnessAndMovement`
- `HealthAndRecovery`
- `LearningAndSkills`
- `WorkAndCareer`
- `DailyLifeAndOrganization`
- `MoneyAndFinance`
- `RelationshipsAndCommunity`
- `EmotionalWellBeing`
- `SpiritualityAndPurpose`
- `CreativityAndRecreation`
- `SelfControlAndBoundaries`
- `GeneralGrowth`

JSON uses the corresponding camel-case values.

UI labels may add spaces and punctuation:

```text
LearningAndSkills -> Learning & Skills
SelfControlAndBoundaries -> Self-Control & Boundaries
```

The display label is not a persisted value.

## Canonical Difficulty and Frequency

`HabitDifficulty`:

- `Easy`
- `Medium`
- `Hard`
- `Elite`

`HabitFrequencyType`:

- `Daily`
- `Weekly`

Do not add `Monthly`, `Custom`, `Flexible`, or other scheduling terms without a domain decision.

## Week Vocabulary

Enum:

```text
WeekStartDay
```

Stored property:

```text
WeekStartsOn
```

Do not replace `WeekStartsOn` with `FirstDayOfWeek`, `StartOfWeek`, or `PreferredWeekStart`.

## Effective-Date Vocabulary

Entity:

```text
HabitConfigurationVersion
```

Properties:

- `VersionNumber`
- `EffectiveFromDate`
- `EffectiveToDateExclusive`

The word `Exclusive` is required because the end date is outside the effective interval.

Use:

- `effectiveConfiguration` when selected for a date;
- `currentConfiguration` for the currently active version;
- `pendingConfiguration` for the future next-boundary version.

Public DTO:

```text
PendingHabitConfigurationResponse
```

Do not rename it to `UpcomingRules`, `QueuedUpdate`, or `FutureSettings`.

## Completion and Undo Vocabulary

Event entity:

```text
HabitCompletion
```

Use:

- `CompletedDate` for the user-local date;
- `CompletedAtUtc` for the exact event timestamp;
- `UndoneAtUtc` for the reversal timestamp;
- `IsCompletedToday` for the current read-model state;
- `CompleteHabitRequest`;
- `CompleteHabitResponse`;
- `HabitCompletionResponse`.

Undo is not deletion.

Use `undo`, `reversal`, and `negative XP transaction` according to the represented behavior.

## XP and Level Vocabulary

Use:

- `XpTransaction`;
- `CurrentXp`;
- `TotalXp`;
- `XpAmount`;
- `XpIntoCurrentLevel`;
- `XpNeededForNextLevel`;
- `Level`;
- `BalanceScore`;
- `ClosestToLevelUp`.

Use `XP` in prose and UI.

Use `Xp` in C# and TypeScript identifiers.

Avoid parallel terms such as `experience points`, `power points`, or `score` when referring to XP.

## Streak Vocabulary

Use:

- `CurrentStreak`;
- `LongestStreak`;
- `HabitStreakResponse`;
- `HabitStreakResult`;
- `HabitStreakSummary`.

Do not use `ActiveStreak` as an alias.

A daily streak is measured in days.

A weekly streak is measured in weeks.

The UI may render singular and plural labels without changing the property name.

## DTO Names

Authentication:

- `RegisterRequest`
- `LoginRequest`
- `CurrentUserResponse`
- `AuthResponse`
- `AntiforgeryTokenResponse`

Habits:

- `CreateHabitRequest`
- `UpdateHabitRequest`
- `HabitResponse`
- `PendingHabitConfigurationResponse`
- `HabitAttributeRewardResponse`

Completion:

- `CompleteHabitRequest`
- `HabitCompletionResponse`
- `CompleteHabitResponse`

Progression:

- `UserAttributeResponse`
- `AttributeOverviewResponse`
- `AttributeLevelUpResponse`
- `XpTransactionResponse`

Dashboard:

- `DashboardResponse`
- `DashboardHabitResponse`
- `OverallProgressResponse`
- `TodayActivityResponse`
- `TodayExecutionResponse`
- `HabitStreakResponse`

Do not return entities where a response DTO exists.

## Frontend Page and Shell Names

Current routed presentation:

- `AuthenticatedWorkspace`
- `AppShell`
- `DashboardPage`
- `HabitSection`
- `AttributeSection`

Supporting Phase 7 names:

- `DashboardSummaryRail`
- `DashboardHabitAction`
- `HabitList`
- `HabitActivateButton`
- `AttributeCard`
- `AttributeRadarChart`
- `AttributeSupportPanels`
- `AttributeLevelUpStrip`
- `CommandPanel`
- `MetricPanel`
- `ThemeSelector`
- `ThemeProvider`
- `WorkspaceDataProvider`
- `useProgressionFeedback`

Do not rename these during documentation or deployment work unless the code changes first.

## Test Naming

Backend:

```text
MethodName_Condition_ExpectedResult
```

Frontend test descriptions should describe observable behavior.

Browser tests should name the user journey.

Avoid names that expose implementation details without expressing the protected behavior.

## Documentation Vocabulary

Use the official phase names:

1. Project Setup
2. Authentication
3. Habit CRUD
4. Habit Completion
5. XP and Attributes
6. Dashboard and Streaks
7. Game UI Polish
8. Deployment and Project Polish

Use:

- `frontend`;
- `backend`;
- `API`;
- `database`;
- `application`;
- `repository`.

Do not describe Phase 8 as a new feature phase.

Use “implemented,” “current,” “planned,” and “deferred” precisely.
