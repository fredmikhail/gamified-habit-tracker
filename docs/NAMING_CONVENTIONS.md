# Gamified Habit Tracker — Naming Conventions

**Status:** Canonical through Phase 6

**Current phase:** Phase 7 — Game UI Polish

**Rule:** Each concept has one stable name across C#, TypeScript, JSON, PostgreSQL, tests, UI labels, and documentation

---

## 1. Purpose

This document defines the canonical vocabulary and naming conventions for the Gamified Habit Tracker.

Names must remain consistent across:

- domain entities,
- enums,
- value objects,
- services,
- controllers,
- DTOs,
- API routes,
- JSON properties,
- TypeScript types,
- React components,
- tests,
- PostgreSQL mappings,
- migrations,
- documentation.

A stable concept must not be renamed because:

- a new phase begins,
- a screen uses a friendlier label,
- a nearby refactor uses different terminology,
- a mockup suggests another word,
- a synonym appears more concise.

Renames require an explicit cross-layer decision.

---

## 2. Naming Priority

When multiple names seem reasonable, prefer the name already established by:

1. the domain model,
2. the API contract,
3. the owning service,
4. existing tests,
5. current documentation.

Do not create aliases for an existing concept.

Examples:

- use `HabitCompletion`, not both `HabitCompletion` and `CompletionLog`,
- use `XpTransaction`, not both `XpTransaction` and `ExperienceEvent`,
- use `WeekStartsOn`, not both `WeekStartsOn` and `FirstDayOfWeek`,
- use `CurrentStreak`, not both `CurrentStreak` and `ActiveStreak`.

UI labels may format a domain name differently without creating a second domain term.

---

## 3. General Rules

- Use descriptive names.
- Avoid unnecessary abbreviations.
- Use responsibility-based names.
- Keep file names aligned with their main type.
- Preserve the same concept across backend and frontend contracts.
- Do not rename stable types during unrelated debugging.
- Do not create speculative domain names for unimplemented features.
- Do not name a type after a temporary layout or screen position.
- Do not use vague names when the responsibility is known.
- Do not add `Manager`, `Processor`, or `Helper` when an established service already owns the behavior.
- Do not expose database terminology when a clearer application term exists.
- Do not store friendly UI labels as replacements for controlled domain values.

Avoid vague names such as:

- `Data`
- `Thing`
- `Item`
- `Object`
- `Manager`
- `GeneralService`
- `UtilityService`
- `ProcessData`
- `HandleEverything`

---

## 4. Canonical Entity Names

### Implemented entities

- `User`
- `UserSettings`
- `Habit`
- `HabitConfigurationVersion`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

### Reserved post-MVP entities

- `Milestone`
- `UserMilestone`

Reserved names remain stable even though their entities are not implemented.

### Disallowed substitutes

Do not replace canonical entities with names such as:

| Canonical                   | Avoid                                                |
| --------------------------- | ---------------------------------------------------- |
| `UserSettings`              | `Preferences`, `UserPreferences`                     |
| `Habit`                     | `Routine`, `TaskDefinition`                          |
| `HabitConfigurationVersion` | `HabitRule`, `HabitSettingsHistory`, `HabitSnapshot` |
| `HabitCompletion`           | `HabitLog`, `CompletedHabit`, `CompletionLog`        |
| `HabitAttributeReward`      | `HabitStatReward`, `RewardMapping`                   |
| `UserAttribute`             | `CharacterStat`, `AttributeProgress`                 |
| `XpTransaction`             | `ExperienceEvent`, `RewardLog`, `XpRecord`           |
| `Milestone`                 | `Achievement`                                        |
| `UserMilestone`             | `UnlockedAchievement`                                |

Friendly UI wording does not rename the underlying entity.

---

## 5. Canonical Service Names

Implemented services:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

### Responsibilities

| Service             | Responsibility                                                             |
| ------------------- | -------------------------------------------------------------------------- |
| `AuthService`       | Registration, login, account validation, current-user mapping              |
| `HabitService`      | Habit creation, retrieval, updates, deactivation, configuration scheduling |
| `CompletionService` | Completion and undo orchestration                                          |
| `XpService`         | Reward allocation and level formulas                                       |
| `AttributeService`  | Attribute reads, XP application, XP reversal                               |
| `StreakService`     | Daily and weekly streak calculation                                        |
| `DashboardService`  | Dashboard aggregation                                                      |

### Names to avoid

- `HabitManager`
- `ProgressService`
- `ProgressionService`
- `GameService`
- `GamificationService`
- `StatsService`
- `DashboardManager`
- `StreakManager`
- `RuleService`
- `DataService`

Do not create a service solely because the frontend introduces a card, panel, tab, or page.

---

## 6. Canonical Domain Enums

Implemented domain enums:

- `AttributeType`
- `HabitCategory`
- `HabitDifficulty`
- `HabitFrequencyType`
- `WeekStartDay`

Enum type names use PascalCase.

Enum members use PascalCase.

Public JSON values use camel case.

Numeric enum values are not part of the API contract.

---

## 7. Canonical Character Attributes

The eight canonical `AttributeType` members are:

- `Discipline`
- `Fitness`
- `Vitality`
- `Focus`
- `Mind`
- `Resilience`
- `Social`
- `Purpose`

JSON values:

- `discipline`
- `fitness`
- `vitality`
- `focus`
- `mind`
- `resilience`
- `social`
- `purpose`

These names must remain consistent in:

- `AttributeType`,
- `HabitAttributeReward`,
- `UserAttribute`,
- `XpTransaction`,
- backend DTOs,
- frontend TypeScript types,
- API JSON,
- tests,
- UI labels,
- documentation.

### Obsolete names

Do not reintroduce:

- `Strength`
- `Recovery`
- `Career`
- `Spirituality`

The canonical replacements are:

| Obsolete     | Canonical              |
| ------------ | ---------------------- |
| Strength     | Fitness                |
| Recovery     | Vitality               |
| Career       | Focus where applicable |
| Spirituality | Purpose                |

These are replacements, not runtime aliases.

---

## 8. Canonical Habit Categories

`HabitCategory` members:

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

JSON values:

- `fitnessAndMovement`
- `healthAndRecovery`
- `learningAndSkills`
- `workAndCareer`
- `dailyLifeAndOrganization`
- `moneyAndFinance`
- `relationshipsAndCommunity`
- `emotionalWellBeing`
- `spiritualityAndPurpose`
- `creativityAndRecreation`
- `selfControlAndBoundaries`
- `generalGrowth`

The UI may render friendly labels:

```text
LearningAndSkills -> Learning & Skills
SelfControlAndBoundaries -> Self-Control & Boundaries
```

The underlying enum names remain unchanged.

Do not use:

- free-form category strings,
- shortened domain values such as `Fitness`,
- display labels as persisted values,
- frontend-only category identifiers.

---

## 9. Canonical Difficulty Names

`HabitDifficulty` members:

- `Easy`
- `Medium`
- `Hard`
- `Elite`

JSON values:

- `easy`
- `medium`
- `hard`
- `elite`

Do not introduce alternatives such as:

- `Normal`
- `Advanced`
- `Extreme`
- `Legendary`

without an approved domain and data migration.

---

## 10. Canonical Frequency Names

`HabitFrequencyType` members:

- `Daily`
- `Weekly`

JSON values:

- `daily`
- `weekly`

Do not add speculative values such as:

- `Monthly`
- `Custom`
- `Flexible`
- `TimesPerWeek`
- `CustomDays`

until scheduling behavior is explicitly designed.

The current weekly frequency already uses `TargetCount` to represent required distinct completion dates within the week.

---

## 11. Canonical Week-Start Names

The enum is:

```text
WeekStartDay
```

Its members are:

- `Monday`
- `Tuesday`
- `Wednesday`
- `Thursday`
- `Friday`
- `Saturday`
- `Sunday`

The user-setting property is:

```text
WeekStartsOn
```

Do not replace it with:

- `WeekStart`
- `WeekStartDay`
- `FirstDayOfWeek`
- `PreferredWeekStart`
- `StartOfWeek`

`WeekStartDay` names the enum type.

`WeekStartsOn` names the stored user preference.

---

## 12. Habit Configuration Vocabulary

The canonical entity is:

```text
HabitConfigurationVersion
```

Canonical properties:

- `HabitId`
- `VersionNumber`
- `Category`
- `FrequencyType`
- `TargetCount`
- `Difficulty`
- `EffectiveFromDate`
- `EffectiveToDateExclusive`
- `CreatedAtUtc`

### Effective-date terminology

Use:

```text
EffectiveFromDate
EffectiveToDateExclusive
```

Do not use:

- `StartDate`
- `EndDate`
- `ActiveFrom`
- `ActiveUntil`
- `ValidTo`
- `EffectiveEndDate`

The word `Exclusive` is required because the end date does not belong to the version’s effective range.

Canonical condition:

```text
EffectiveFromDate <= date
AND
(
    EffectiveToDateExclusive is null
    OR date < EffectiveToDateExclusive
)
```

### Current configuration

Use:

```text
currentConfiguration
```

or:

```text
effectiveConfiguration
```

depending on context.

`effectiveConfiguration` is preferred when the value is selected for a specific date.

### Pending configuration

The public DTO is:

```text
PendingHabitConfigurationResponse
```

The response property is:

```text
PendingConfiguration
```

JSON:

```text
pendingConfiguration
```

Use `pendingConfiguration` for a future effective version.

Do not use:

- `UpcomingRules`
- `ScheduledHabit`
- `FutureSettings`
- `NextConfiguration`
- `QueuedUpdate`

unless the represented concept is materially different.

### Configuration history

Use:

```text
HabitConfigurationVersions
```

for the collection navigation property.

Do not call the collection:

- `Versions`
- `RuleHistory`
- `HabitHistory`
- `Configurations`

when referring to the established navigation property.

---

## 13. Completion and Undo Vocabulary

The canonical event entity is:

```text
HabitCompletion
```

Canonical properties include:

- `HabitConfigurationVersionId`
- `CompletedDate`
- `CompletedAtUtc`
- `UndoneAtUtc`

### Completion date

Use:

```text
CompletedDate
```

for the user-local calendar date.

Use:

```text
CompletedAtUtc
```

for the exact UTC timestamp.

Do not treat the two names as interchangeable.

### Undo state

Use:

```text
UndoneAtUtc
```

A completion is active when:

```text
UndoneAtUtc is null
```

Do not add duplicate state properties such as:

- `IsUndone`
- `WasReversed`
- `IsDeleted`
- `IsActiveCompletion`

unless a separate persisted requirement is approved.

### Undo terminology

Use:

- `undo`,
- `undone`,
- `reversal`,
- `active completion`,
- `auditable undo`.

Do not describe the implemented workflow as:

- deleting the completion,
- removing completion history,
- deleting XP transactions,
- erasing the completion.

The user-facing action remains “Undo.”

The persistence behavior is a reversal.

### Completion uniqueness

Use:

```text
active completion
```

to describe a completion where `UndoneAtUtc` is null.

The rule is:

```text
one active completion per habit and completed date
```

Do not write “one completion per date” without the `active` qualification when discussing the current schema.

---

## 14. XP Transaction Vocabulary

Use `Xp`, not `XP`, inside C# and TypeScript identifiers.

Correct identifiers:

- `XpService`
- `XpTransaction`
- `XpAmount`
- `CurrentXp`
- `TotalXp`
- `XpIntoCurrentLevel`
- `XpNeededForNextLevel`

UI labels may display:

```text
XP
```

Avoid identifier alternatives:

- `Experience`
- `Exp`
- `Points`
- `ExperiencePoints`

### Transaction amount

The canonical property is:

```text
Amount
```

The amount is signed:

- positive for awards,
- negative for reversals.

Do not name it:

- `XpAmount` on `XpTransaction`,
- `Delta`,
- `Value`,
- `Points`.

`XpAmount` remains correct on reward DTOs and `HabitAttributeReward`.

### Transaction reasons

Canonical reason values:

```text
Habit completion
Habit completion undo
```

Canonical constant names:

```text
HabitCompletionReason
HabitCompletionUndoReason
```

Do not introduce alternate strings for the same events.

### Ledger terminology

Use:

- `positive transaction`,
- `negative transaction`,
- `reversal transaction`,
- `signed transaction`,
- `append-only XP history`.

Do not describe undo as modifying or deleting the original transaction.

---

## 15. XP and Progression Naming

Canonical progress properties:

- `CurrentXp`
- `TotalXp`
- `Level`
- `XpIntoCurrentLevel`
- `XpNeededForNextLevel`

Avoid alternatives such as:

- `NextLevelXp`
- `RemainingXp`
- `XpRequired`
- `CurrentLevelXp`
- `LevelXp`
- `ExperienceToNextLevel`

The current contract uses:

```text
XpNeededForNextLevel
```

Attribute and overall progression should continue using the same conceptual property names.

### Calculation type

The value object is:

```text
LevelProgress
```

Do not rename it to:

- `LevelResult`
- `ProgressInfo`
- `XpProgress`
- `LevelCalculation`

unless its represented concept changes.

---

## 16. Calendar Value-Object Names

Canonical calendar types include:

- `WeekPeriod`
- `CalendarPeriodCalculator`
- `LocalDateCalculator`

Canonical week-period properties:

- `StartDateInclusive`
- `EndDateInclusive`

Use `Inclusive` where the boundary belongs to the period.

Do not shorten these to ambiguous names such as:

- `Start`
- `End`
- `From`
- `To`

The configuration end property remains `EffectiveToDateExclusive`, which is intentionally different.

---

## 17. Streak Vocabulary

The canonical service is:

```text
StreakService
```

Canonical result and supporting types include:

- `HabitStreakResult`
- `HabitStreakSummary`
- `WeeklyStreakTarget`

Canonical response type:

- `HabitStreakResponse`

Canonical response properties:

- `HabitId`
- `HabitName`
- `FrequencyType`
- `CurrentStreak`
- `LongestStreak`

### Method naming

The main calculation method is:

```text
CalculateHabitStreak
```

Calculation methods remain synchronous unless they perform asynchronous work.

Do not name the method:

- `GetStreak`
- `ProcessStreak`
- `BuildStreak`
- `UpdateStreak`
- `SaveStreak`

Streaks are calculated, not persisted or updated.

### Current and longest values

Use:

```text
CurrentStreak
LongestStreak
```

Do not use:

- `ActiveStreak`
- `BestStreak`
- `HighestStreak`
- `StreakCount`
- `Combo`
- `Run`

unless a future concept has a different defined meaning.

### Unit terminology

The backend returns frequency, not a separate streak-unit string.

The frontend derives display labels:

```text
Daily  -> day / days
Weekly -> week / weeks
```

Use `FrequencyType` to select the presentation unit.

Do not add a duplicate `StreakUnit` property without a contract requirement.

### Streak scope

Use:

```text
per-habit streak
habit streak
```

Do not use `overall streak` for the implemented system.

There is no combined cross-habit streak.

### Frequency series

Use:

```text
frequency segment
streak series
current contiguous frequency segment
```

for the period after the most recent daily/weekly transition.

Do not describe a frequency transition as deleting earlier history.

---

## 18. Dashboard Vocabulary

Canonical backend names:

- `DashboardController`
- `DashboardService`
- `DashboardResponse`

Canonical nested responses:

- `OverallProgressResponse`
- `TodayActivityResponse`
- `TodayExecutionResponse`
- `HabitStreakResponse`

Canonical `DashboardResponse` properties:

- `OverallProgress`
- `TodayActivity`
- `TodayExecution`
- `HabitStreaks`

JSON:

- `overallProgress`
- `todayActivity`
- `todayExecution`
- `habitStreaks`

### Overall progression properties

- `TotalXp`
- `Level`
- `XpIntoCurrentLevel`
- `XpNeededForNextLevel`

### Today activity properties

- `LocalDate`
- `Completions`
- `XpEarned`

Do not rename `Completions` to:

- `CompletionCount`
- `CompletedCount`
- `Activities`

without changing the API contract deliberately.

### Today execution properties

- `CompletedDailyHabits`
- `TotalDailyHabits`

Do not use the more ambiguous:

- `CompletedHabits`
- `TotalHabits`
- `DailyCompleted`
- `DailyTotal`

The current values apply specifically to active daily habits.

### Dashboard frontend names

Implemented names include:

- `dashboardApi.ts`
- `getDashboard`
- `OverallProgressSection`
- `HabitStreakSection`

Do not rename `OverallProgressSection` to a vague name such as:

- `DashboardStats`
- `ProgressWidget`
- `MainSummary`

Do not rename `HabitStreakSection` to:

- `Streaks`
- `StreakWidget`
- `ComboSection`

unless the component’s responsibility changes.

---

## 19. C# Type Naming

Use PascalCase for:

- classes,
- records,
- enums,
- structs,
- interfaces.

Examples:

- `Habit`
- `HabitConfigurationVersion`
- `HabitService`
- `DashboardResponse`
- `HabitStreakResult`
- `WeekStartDay`

Interfaces begin with `I` when a real abstraction is required.

Example:

```text
IHabitRepository
```

Do not create an interface automatically for every service.

---

## 20. C# Method Naming

Use PascalCase.

Asynchronous methods end with `Async`.

Examples:

- `RegisterAsync`
- `LoginAsync`
- `CreateHabitAsync`
- `GetUserHabitsAsync`
- `UpdateHabitAsync`
- `DeactivateHabitAsync`
- `CompleteHabitAsync`
- `UndoTodayAsync`
- `GetUserAttributesAsync`
- `ApplyCompletionRewardsAsync`
- `ReverseCompletionRewardsAsync`
- `GetDashboardAsync`

Synchronous calculation methods do not use the `Async` suffix.

Examples:

- `CalculateRewards`
- `CalculateAttributeLevelProgress`
- `CalculateOverallLevelProgress`
- `CalculateHabitStreak`
- `GetLocalDate`
- `GetWeekPeriod`

Avoid vague method names such as:

- `Process`
- `Handle`
- `DoWork`
- `Run`
- `Manage`
- `CalculateData`

---

## 21. C# Property Naming

Use PascalCase.

### Identifiers

Primary keys:

```text
Id
```

Foreign keys use the referenced entity name followed by `Id`:

- `UserId`
- `HabitId`
- `HabitConfigurationVersionId`
- `HabitCompletionId`
- `MilestoneId`

Do not use `HabitId` when the direct relationship is to `HabitCompletion`.

### UTC timestamps

UTC timestamp properties end with `Utc`:

- `CreatedAtUtc`
- `UpdatedAtUtc`
- `CompletedAtUtc`
- `UndoneAtUtc`
- `LastLoginAtUtc`
- `UnlockedAtUtc`

### Date-only properties

Date-only properties describe the represented business date:

- `CompletedDate`
- `EffectiveFromDate`
- `EffectiveToDateExclusive`
- `LocalDate`
- `StartDateInclusive`
- `EndDateInclusive`

### Boolean properties

Use clear prefixes:

- `Is`
- `Has`
- `Can`
- `Should`

Examples:

- `IsActive`
- `IsCompletedToday`
- `IsAuthenticated`
- `CanSubmit`
- `ShouldRefresh`

Do not create a second boolean for a meaning already represented by an existing property.

---

## 22. C# Local Variables and Parameters

Use camelCase.

Examples:

- `userId`
- `habitId`
- `localDate`
- `completedDate`
- `completedAtUtc`
- `currentConfiguration`
- `pendingConfiguration`
- `effectiveConfiguration`
- `currentWeek`
- `nextWeekStart`
- `awardedTransactions`
- `habitStreakCalculations`
- `cancellationToken`

Names should reflect purpose rather than only type.

Prefer:

- `activeHabits`
- `todayCompletions`
- `completedDailyHabits`
- `effectiveConfiguration`
- `awardedTransactions`

Avoid:

- `data`
- `result2`
- `temp`
- `obj`
- `thing`
- `items` when a specific collection name is available.

---

## 23. Private Fields and Constants

Private instance fields use underscore followed by camelCase.

Examples:

- `_dbContext`
- `_timeProvider`
- `_authService`
- `_habitService`
- `_completionService`
- `_xpService`
- `_attributeService`
- `_streakService`
- `_dashboardService`

Constants use PascalCase.

Examples:

- `HabitCompletionReason`
- `HabitCompletionUndoReason`
- `PrimaryRewardPercentage`
- `AttributeBaseXp`
- `OverallBaseXp`

Constant names must describe their application meaning.

---

## 24. Namespace Naming

Use PascalCase namespace segments.

Current namespaces include:

- `HabitTracker.Api`
- `HabitTracker.Api.Controllers`
- `HabitTracker.Api.Data`
- `HabitTracker.Api.Data.Configurations`
- `HabitTracker.Api.Domain.Entities`
- `HabitTracker.Api.Domain.Enums`
- `HabitTracker.Api.Domain.ValueObjects`
- `HabitTracker.Api.DTOs`
- `HabitTracker.Api.ExceptionHandling`
- `HabitTracker.Api.Exceptions`
- `HabitTracker.Api.Services`
- `HabitTracker.Tests.Services`
- `HabitTracker.IntegrationTests.Controllers`

Namespace segments should align with folder responsibilities.

---

## 25. Collection Navigation Naming

Collection navigation properties use plural concept names.

Examples:

- `Habits`
- `HabitConfigurationVersions`
- `HabitCompletions`
- `HabitAttributeRewards`
- `UserAttributes`
- `XpTransactions`
- `UserMilestones`

Avoid generic collection names:

- `Items`
- `Records`
- `Children`
- `Entries`

when the domain relationship has a precise name.

---

## 26. DTO Naming

Request DTOs end with:

```text
Request
```

Response DTOs end with:

```text
Response
```

Do not append both `Dto` and `Request` or `Response`.

Correct:

```text
CreateHabitRequest
```

Avoid:

```text
CreateHabitRequestDto
```

### Implemented request DTOs

- `RegisterRequest`
- `LoginRequest`
- `CreateHabitRequest`
- `UpdateHabitRequest`
- `CompleteHabitRequest`

### Implemented response DTOs

- `HealthResponse`
- `AntiforgeryTokenResponse`
- `AuthResponse`
- `CurrentUserResponse`
- `HabitResponse`
- `PendingHabitConfigurationResponse`
- `HabitAttributeRewardResponse`
- `HabitCompletionResponse`
- `CompleteHabitResponse`
- `UserAttributeResponse`
- `DashboardResponse`
- `OverallProgressResponse`
- `TodayActivityResponse`
- `TodayExecutionResponse`
- `HabitStreakResponse`

Do not return entity classes directly.

### Calculated response properties

Response DTOs may contain calculated values without corresponding database columns.

Examples:

- `Level`
- `XpIntoCurrentLevel`
- `XpNeededForNextLevel`
- `TotalXp`
- `CurrentStreak`
- `LongestStreak`
- `IsCompletedToday`

A response property does not imply that the value must be persisted.

---

## 27. DTO Evolution Rules

A DTO may gain a field when its established concept expands.

Implemented examples:

- `HabitResponse` gained `IsCompletedToday`.
- `HabitResponse` gained `AttributeRewards`.
- `HabitResponse` gained `PendingConfiguration`.
- `CompleteHabitResponse` exposes `Rewards`.
- `DashboardResponse` gained `HabitStreaks`.

Do not rename a DTO only because it gains related properties.

Habit requests do not accept:

- `AttributeRewards`
- `PendingConfiguration`
- `IsCompletedToday`
- `UserId`
- completion dates,
- XP amounts.

Those remain backend-owned.

When a DTO changes, update the backend and frontend contract together.

---

## 28. Controller Naming

Controller class names end with:

```text
Controller
```

Implemented controllers:

- `HealthController`
- `AuthController`
- `HabitsController`
- `CompletionsController`
- `AttributesController`
- `DashboardController`

Collection resource controllers generally use plural names:

- `HabitsController`
- `CompletionsController`
- `AttributesController`

The dashboard uses singular naming because it represents one aggregated user dashboard:

- `DashboardController`

Avoid:

- `HabitManagementController`
- `ProgressController`
- `StatsController`
- `GameController`
- `DashboardManagerController`

Controllers remain thin regardless of their names.

---

## 29. API Route Naming

Routes use:

- lowercase letters,
- stable resource nouns,
- plural collection resources where appropriate,
- hyphens for multi-word segments,
- HTTP methods to express the operation.

Implemented routes:

```text
GET  /api/health

GET  /api/auth/csrf-token
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/me

GET    /api/habits
GET    /api/habits/{habitId}
POST   /api/habits
PUT    /api/habits/{habitId}
DELETE /api/habits/{habitId}

POST   /api/habits/{habitId}/completions
DELETE /api/habits/{habitId}/completions/today

GET /api/attributes
GET /api/dashboard
```

Avoid redundant verb routes.

Preferred:

```text
DELETE /api/habits/{habitId}
```

Avoid:

```text
POST /api/habits/{habitId}/deactivate
```

The route still uses `DELETE` for undoing today’s active completion even though the database record is preserved.

The HTTP operation removes the active resource state; the persistence layer records an auditable reversal.

---

## 30. Public JSON Naming

C# properties use PascalCase.

JSON properties use camel case.

Examples:

| C#                     | JSON                   |
| ---------------------- | ---------------------- |
| `IsCompletedToday`     | `isCompletedToday`     |
| `PendingConfiguration` | `pendingConfiguration` |
| `EffectiveFromDate`    | `effectiveFromDate`    |
| `AttributeRewards`     | `attributeRewards`     |
| `XpAmount`             | `xpAmount`             |
| `CurrentXp`            | `currentXp`            |
| `TotalXp`              | `totalXp`              |
| `XpIntoCurrentLevel`   | `xpIntoCurrentLevel`   |
| `XpNeededForNextLevel` | `xpNeededForNextLevel` |
| `CompletedAtUtc`       | `completedAtUtc`       |
| `CurrentStreak`        | `currentStreak`        |
| `LongestStreak`        | `longestStreak`        |
| `HabitStreaks`         | `habitStreaks`         |

Enum examples:

| C#                   | JSON                 |
| -------------------- | -------------------- |
| `LearningAndSkills`  | `learningAndSkills`  |
| `FitnessAndMovement` | `fitnessAndMovement` |
| `Discipline`         | `discipline`         |
| `Elite`              | `elite`              |
| `Weekly`             | `weekly`             |

Numeric enums are rejected.

---

## 31. React Component Naming

React components use PascalCase.

Component files match component names.

### Implemented authentication components

- `LoginForm`
- `RegisterForm`

### Implemented habit components

- `HabitSection`
- `HabitForm`
- `HabitList`
- `HabitEditForm`
- `HabitCompletionButton`
- `HabitDeactivateButton`

### Implemented attribute components

- `AttributeSection`

### Implemented dashboard components

- `OverallProgressSection`
- `HabitStreakSection`

Component names should describe what the component renders or controls.

Avoid vague names such as:

- `MainCard`
- `ActionBox`
- `StatsWidget`
- `GamePanel`
- `DashboardThing`

### Phase 7 components

Phase 7 may introduce names such as:

- `AppShell`
- `Sidebar`
- `TopBar`
- `PageHeader`
- `Panel`
- `Card`
- `Button`
- `IconButton`
- `Badge`
- `ProgressBar`
- `Modal`
- `ConfirmationDialog`
- `LoadingSkeleton`
- `EmptyState`
- `ErrorPanel`
- `Toast`

A component is created only when current screen work requires it.

Potential names are not instructions to build an unused component framework.

---

## 32. TypeScript Contract Naming

TypeScript API types use PascalCase and mirror backend DTO concepts.

Implemented examples include:

- `AuthResponse`
- `CurrentUserResponse`
- `HabitResponse`
- `PendingHabitConfigurationResponse`
- `HabitAttributeRewardResponse`
- `HabitCompletionResponse`
- `CompleteHabitResponse`
- `UserAttributeResponse`
- `DashboardResponse`
- `OverallProgressResponse`
- `TodayActivityResponse`
- `TodayExecutionResponse`
- `HabitStreakResponse`
- `AttributeType`
- `HabitCategory`
- `HabitDifficulty`
- `HabitFrequencyType`

Do not rename a backend contract in TypeScript.

Avoid:

- `HabitModel`
- `HabitData`
- `DashboardData`
- `AttributeDto`
- `CompletionResult`

when the backend contract already has a canonical name.

---

## 33. TypeScript Variables and Functions

Use camelCase.

Examples:

- `habitId`
- `refreshKey`
- `progressRefreshKey`
- `earnedRewards`
- `pendingConfiguration`
- `habitStreaks`
- `isSaving`
- `errorMessage`

API functions should align with endpoint behavior.

Implemented examples:

- `register`
- `login`
- `logout`
- `getCurrentUser`
- `getHabits`
- `createHabit`
- `updateHabit`
- `deactivateHabit`
- `completeHabit`
- `undoHabitCompletion`
- `getAttributes`
- `getDashboard`

Avoid generic API function names:

- `fetchData`
- `send`
- `makeRequest`
- `handleApi`
- `loadStuff`

---

## 34. React Hook Naming

Custom hooks begin with:

```text
use
```

Implemented example:

- `useAuth`

A future hook may be introduced when reusable behavior exists.

Do not create hooks solely to move code out of a component.

Hooks may own:

- request orchestration,
- loading state,
- local interaction state,
- reusable UI behavior.

Hooks must not own:

- XP formulas,
- level formulas,
- reward mapping,
- calendar-period rules,
- configuration selection,
- streak calculation,
- ownership validation.

---

## 35. Frontend File Naming

Top-level frontend folders use lowercase names.

Current folders include:

- `api`
- `auth`
- `components`
- `test`
- `types`

Feature component folders include:

- `components/auth`
- `components/habits`
- `components/attributes`
- `components/dashboard`

### Component files

Use PascalCase:

- `HabitList.tsx`
- `AttributeSection.tsx`
- `OverallProgressSection.tsx`
- `HabitStreakSection.tsx`

### Non-component TypeScript files

Use camelCase:

- `apiClient.ts`
- `authApi.ts`
- `habitsApi.ts`
- `attributesApi.ts`
- `dashboardApi.ts`
- `healthApi.ts`
- `readApiError.ts`
- `useAuth.ts`

### Type files

Type files use PascalCase when they represent one named contract:

- `HabitResponse.ts`
- `DashboardResponse.ts`
- `HabitStreakResponse.ts`
- `PendingHabitConfigurationResponse.ts`

### Test files

Vitest and React Testing Library:

- `.test.ts`
- `.test.tsx`

Examples:

- `dashboardApi.test.ts`
- `HabitStreakSection.test.tsx`
- `OverallProgressSection.test.tsx`

Playwright:

- `.spec.ts`

---

## 36. Backend File Naming

A C# file normally matches its primary public type.

Examples:

- `Habit.cs`
- `HabitConfigurationVersion.cs`
- `HabitCompletion.cs`
- `XpTransaction.cs`
- `HabitService.cs`
- `StreakService.cs`
- `DashboardService.cs`
- `DashboardController.cs`
- `DashboardResponse.cs`
- `HabitStreakResponse.cs`
- `WeekStartDay.cs`
- `CalendarPeriodCalculator.cs`

Avoid placing unrelated public types in one file.

Private nested records or classes may remain inside the owning implementation when they are not reusable contract or domain types.

---

## 37. PostgreSQL Naming

C# uses PascalCase.

Physical PostgreSQL identifiers use lowercase `snake_case`.

Examples:

| C#                            | PostgreSQL                       |
| ----------------------------- | -------------------------------- |
| `UserSettings`                | `user_settings`                  |
| `HabitConfigurationVersions`  | `habit_configuration_versions`   |
| `HabitCompletions`            | `habit_completions`              |
| `HabitAttributeRewards`       | `habit_attribute_rewards`        |
| `UserAttributes`              | `user_attributes`                |
| `XpTransactions`              | `xp_transactions`                |
| `WeekStartsOn`                | `week_starts_on`                 |
| `EffectiveFromDate`           | `effective_from_date`            |
| `EffectiveToDateExclusive`    | `effective_to_date_exclusive`    |
| `HabitConfigurationVersionId` | `habit_configuration_version_id` |
| `UndoneAtUtc`                 | `undone_at_utc`                  |
| `CreatedAtUtc`                | `created_at_utc`                 |

Normal local database:

```text
habit_tracker
```

Application role:

```text
habit_tracker_app
```

Use actual lowercase identifiers in direct PostgreSQL queries.

Do not guess quoted PascalCase table or column names.

---

## 38. Constraint and Index Naming

Physical constraint and index names use `snake_case`.

Prefixes:

- `pk_` for primary keys,
- `fk_` for foreign keys,
- `ix_` for indexes,
- `ck_` for check constraints.

Current patterns include:

- `pk_habits`
- `fk_habits_users_user_id`
- `ix_habits_user_id_is_active`
- `ix_habit_configuration_versions_habit_id_version_number`
- `ix_habit_configuration_versions_habit_id_effective_from_date`
- `ix_habit_completions_habit_id_completed_date`
- `ix_xp_transactions_habit_completion_id_attribute_type_reason`
- `ck_habits_frequency_target_count`
- `ck_habit_configuration_versions_effective_date_range`
- `ck_user_settings_week_starts_on`
- `ck_user_attributes_current_xp`
- `ck_xp_transactions_amount`

Names should identify:

- object type,
- table,
- columns or protected rule.

Do not rename migrated database objects for cosmetic preference.

---

## 39. Migration Naming

EF Core migration names use PascalCase and describe the schema change.

Examples:

- `AddAuthentication`
- `AddHabits`
- `AddHabitCompletions`
- `AddXpAndAttributes`
- `AddAuditableCompletionUndo`

Use names that describe the model change.

Avoid:

- `UpdateDatabase`
- `Changes`
- `Fix`
- `Migration2`
- date-only names,
- ticket-only names.

Do not rename, delete, or rewrite existing migration history during unrelated work.

Generated migration files retain their generated timestamp prefix.

---

## 40. Test Project Naming

Backend projects:

- `HabitTracker.Api`
- `HabitTracker.Tests`
- `HabitTracker.IntegrationTests`

`HabitTracker.Tests` contains focused backend tests.

`HabitTracker.IntegrationTests` contains ASP.NET Core HTTP pipeline tests.

The two projects remain separate.

---

## 41. Test Class Naming

Test classes describe the system under test.

Implemented examples include:

- `AuthControllerTests`
- `HabitsControllerTests`
- `CompletionsControllerTests`
- `AttributesControllerTests`
- `DashboardControllerTests`
- `HabitServiceTests`
- `CompletionServiceTests`
- `XpServiceTests`
- `AttributeServiceTests`
- `StreakServiceTests`
- `DashboardServiceTests`

Frontend test files mirror their source:

- `HabitCompletionButton.test.tsx`
- `HabitList.test.tsx`
- `AttributeSection.test.tsx`
- `HabitStreakSection.test.tsx`
- `OverallProgressSection.test.tsx`
- `dashboardApi.test.ts`

---

## 42. Backend Test Method Naming

Test method names should communicate:

1. method or behavior,
2. condition,
3. expected result.

Pattern:

```text
Method_WhenCondition_ExpectedResult
```

Examples:

- `CompleteHabitAsync_WhenHabitAlreadyCompleted_ThrowsConflictException`
- `GetUserAttributesAsync_ReturnsAllSupportedAttributes`
- `GetDashboardAsync_ReturnsCurrentAndLongestHabitStreaks`
- `CalculateHabitStreak_WhenCurrentDayIsIncomplete_PreservesStreakThroughYesterday`
- `CalculateHabitStreak_WhenFrequencyChanges_StartsNewSeries`
- `UndoTodayAsync_WhenCompletionExists_MarksCompletionUndone`
- `UndoTodayAsync_WhenCompletionExists_AppendsNegativeXpTransactions`

Do not use obsolete test wording such as:

```text
RemovesCompletionAndRewards
DeletesXpTransactions
```

for the current undo implementation.

Tests should describe current observable or persisted behavior.

---

## 43. Frontend Test Description Naming

Frontend test descriptions use direct observable language.

Examples:

- `shows earned XP after completion`
- `shows pending configuration details`
- `shows daily streaks in days`
- `shows weekly streaks in weeks`
- `refreshes the dashboard after habit creation`
- `refreshes the dashboard after deactivation`
- `shows an empty state when there are no active habits`
- `shows an error when the dashboard request fails`

Avoid descriptions based only on internal implementation details.

---

## 44. UI Labels

Code names and UI labels may differ in formatting while preserving the same meaning.

Examples:

| Domain or contract name    | UI label                  |
| -------------------------- | ------------------------- |
| `LearningAndSkills`        | Learning & Skills         |
| `DailyLifeAndOrganization` | Daily Life & Organization |
| `XpNeededForNextLevel`     | XP to next level          |
| `IsCompletedToday`         | Completed today           |
| `PendingConfiguration`     | Scheduled changes         |
| `CurrentStreak`            | Current streak            |
| `LongestStreak`            | Longest streak            |
| `TodayExecution`           | Today’s execution         |

Do not rename backend concepts only to improve UI wording.

Use label maps or formatting functions in React.

The same attribute must retain the same label across every screen.

Examples:

- `Fitness` must not display as Strength.
- `Vitality` must not display as Recovery.
- `Purpose` must not display as Spirituality on a different page.

---

## 45. Phase 7 Naming Constraints

Phase 7 may introduce presentation and layout names.

New component names should describe reusable visual responsibility rather than backend behavior.

Suitable categories include:

### Layout

- `AppShell`
- `Sidebar`
- `TopBar`
- `PageHeader`
- `PageContent`

### Feedback and state

- `LoadingSkeleton`
- `EmptyState`
- `ErrorPanel`
- `Toast`
- `ConfirmationDialog`

### Progression presentation

- `XpProgressBar`
- `LevelBadge`
- `StreakBadge`
- `AttributeCard`
- `AttributeGrid`
- `RewardPreview`

A presentation component must not imply that it owns domain calculation.

Example:

```text
StreakBadge
```

may display a streak.

It must not calculate the streak.

Avoid names such as:

- `StreakCalculator`
- `XpEngine`
- `LevelManager`

inside the React application.

---

## 46. Unsupported Product Names

Do not introduce names for unimplemented systems such as:

- `FocusScore`
- `Coin`
- `Gold`
- `Mana`
- `Rank`
- `Quest`
- `Inventory`
- `Avatar`
- `LeaderboardEntry`
- `Notification`
- `PublicProfile`
- `SocialConnection`
- `AiRecommendation`

A visual placeholder does not create an approved domain concept.

Reserved names `Milestone` and `UserMilestone` remain exceptions because they are explicitly reserved for post-MVP work.

---

## 47. Rename Process

Before renaming a stable concept:

1. identify the concrete problem with the current name,
2. confirm the new name represents the same or a deliberately changed concept,
3. search entities, enums, value objects, services, DTOs, controllers, routes, frontend types, components, tests, migrations, database objects, and documentation,
4. identify API compatibility effects,
5. identify database migration effects,
6. update all layers together,
7. run the complete relevant test suites,
8. verify the user-visible workflow,
9. commit the rename separately from unrelated feature work.

Core names must not change during unrelated debugging.

---

## 48. Contract Synchronization

When a backend response property is added or renamed, update together:

- backend DTO,
- service mapping,
- controller behavior,
- backend unit tests,
- HTTP integration tests,
- frontend TypeScript type,
- API module,
- API tests,
- component fixtures,
- consuming components,
- `API_CONTRACT.md`,
- this guide when the concept becomes canonical.

Example:

```text
HabitStreakResponse.CurrentStreak
```

must correspond to:

```text
currentStreak
```

in JSON and TypeScript.

Backend and frontend contract names must not drift.

---

## 49. File-Safety Rules

Before replacing a complete file:

1. confirm the full file path,
2. confirm the editor tab,
3. confirm the file name matches the intended type,
4. save the replacement,
5. inspect `git status --short`,
6. inspect the path-specific diff.

Before committing:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Every staged filename must be intentional.

A similarly named component, DTO, or test file must not be overwritten accidentally.

---

## 50. Summary

Canonical vocabulary follows the application boundaries:

```text
Entities name persistent concepts.

Value objects name calculated domain results.

Services name backend responsibilities.

DTOs name HTTP contracts.

TypeScript mirrors those contracts.

React components name presentation responsibilities.

PostgreSQL uses predictable snake_case mappings.

Tests and documentation use the same terms.
```

A stable name changes only through an explicit cross-layer decision.
