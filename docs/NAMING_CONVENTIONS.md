# Gamified Habit Tracker — Naming Conventions

**Status:** Canonical naming guide
**Current implementation status:** Phase 5 complete
**Next phase:** Phase 6 — Dashboard and Streaks
**Rule:** One stable name for each concept across code, API contracts, tests, UI, database mapping, and documentation

---

## 1. Purpose

This document defines the naming conventions for the Gamified Habit Tracker.

Consistent naming makes the codebase easier to:

- understand,
- search,
- debug,
- test,
- document,
- hand off,
- extend safely.

Names should describe purpose clearly.

Alternate names for the same concept should be avoided unless a deliberate rename has been approved across every affected layer.

A core domain name must not change merely because:

- a new phase begins,
- a UI mockup uses different wording,
- a developer prefers another synonym,
- a temporary implementation detail appears,
- a refactor happens nearby.

---

## 2. General Naming Rules

- Use one stable name for each domain concept.
- Prefer descriptive names over short names.
- Avoid unnecessary abbreviations.
- Avoid vague names such as `Data`, `Manager`, `Helper`, `Thing`, or `Item` when a more precise name exists.
- Keep the same concept recognizable across C#, TypeScript, JSON, tests, UI labels, and documentation.
- File names should normally match the main class, component, type, or module they contain.
- Use names that describe responsibility rather than implementation accident.
- Do not rename stable concepts during unrelated debugging.
- Do not create new aliases for existing services, entities, DTOs, or routes.
- Planned UI labels may be friendlier than code names, but they must not change the underlying domain concept.
- Mockup wording is not automatically a canonical code name.
- Post-MVP status does not make a reserved name unstable.

When two names are both technically possible, prefer the one already used by:

1. the domain model,
2. the API contract,
3. existing services,
4. existing tests,
5. current documentation.

---

## 3. Stable Core Entity Names

The following entity names are canonical:

- `User`
- `UserSettings`
- `Habit`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`
- `Milestone`
- `UserMilestone`

### Implemented MVP entities

- `User`
- `UserSettings`
- `Habit`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

### Reserved post-MVP entities

- `Milestone`
- `UserMilestone`

Reserved names must remain stable even before implementation.

### Disallowed substitutes

Do not replace canonical entities with alternate names such as:

- `HabitLog`
- `CompletedHabit`
- `CompletionLog`
- `CharacterStat`
- `Stat`
- `AttributeProgress`
- `RewardLog`
- `ExperienceEvent`
- `ExperienceTransaction`
- `Preferences`
- `UserPreferences`
- `Achievement`
- `UnlockedAchievement`

A UI component may use a friendly label such as “Stats” or “Achievements,” but the backend domain entities remain canonical.

---

## 4. Stable Core Service Names

The following service names are canonical:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

### Implemented services

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`

### Planned Phase 6 services

- `StreakService`
- `DashboardService`

### Responsibility-based naming

Use service names that reflect ownership.

Examples:

- `AuthService` owns authentication application behavior.
- `HabitService` owns habit management behavior.
- `CompletionService` owns completion and undo orchestration.
- `XpService` owns reward and progression formulas.
- `AttributeService` owns persisted attribute XP updates and attribute responses.
- `StreakService` will own deterministic streak rules.
- `DashboardService` will own dashboard aggregation.

Avoid vague or overlapping alternatives:

- `DataService`
- `AppManager`
- `HabitManager`
- `ProgressService`
- `GameService`
- `GamificationService`
- `StatsService`
- `UtilityService`
- `GeneralService`
- `DashboardManager`

Do not add a new service merely because a UI panel exists.

A service name should represent a real backend responsibility.

---

## 5. Canonical Character Attributes

The eight canonical attributes are:

- `Discipline`
- `Fitness`
- `Vitality`
- `Focus`
- `Mind`
- `Resilience`
- `Social`
- `Purpose`

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
- documentation,
- future dashboard responses.

### Deprecated names

The following earlier names are obsolete and must not be reintroduced:

- `Strength`
- `Recovery`
- `Career`
- `Spirituality`

Approved replacements:

| Obsolete | Canonical |
|---|---|
| Strength | Fitness |
| Recovery | Vitality |
| Career | Focus, depending on category mapping |
| Spirituality | Purpose |

Do not treat those replacements as aliases in code.

Only the canonical eight values belong in `AttributeType`.

---

## 6. Canonical Habit Categories

The canonical `HabitCategory` members are:

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

Public JSON values use camel case:

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

Habit categories are controlled domain values.

Do not replace them with:

- arbitrary free text,
- display labels stored as domain data,
- shorter unofficial values such as `Fitness`,
- frontend-only category strings,
- mockup-only labels.

The UI may render friendly labels with spaces and punctuation.

Example:

```text
learningAndSkills → Learning & Skills
```

The underlying enum remains `LearningAndSkills`.

---

## 7. Canonical Habit Difficulty Names

The canonical `HabitDifficulty` members are:

- `Easy`
- `Medium`
- `Hard`
- `Elite`

Public JSON values:

- `easy`
- `medium`
- `hard`
- `elite`

Do not introduce alternate names such as:

- `Normal`
- `Advanced`
- `Extreme`
- `Legendary`

without an approved domain change.

---

## 8. Canonical Habit Frequency Names

The canonical `HabitFrequencyType` members are:

- `Daily`
- `Weekly`

Public JSON values:

- `daily`
- `weekly`

Possible future scheduling concepts must not be added to the enum until their behavior is explicitly designed.

Do not add speculative values such as:

- `CustomDays`
- `TimesPerWeek`
- `Monthly`
- `Flexible`

merely to prepare for future UI.

---

## 9. C# Naming Conventions

## 9.1 Classes, records, enums, and interfaces

Use PascalCase.

Examples:

- `Habit`
- `HabitService`
- `CreateHabitRequest`
- `HabitDifficulty`
- `LevelProgress`

Interfaces begin with `I` when introduced.

Example:

- `IHabitRepository`

Do not create an interface automatically for every concrete class.

Add an interface only when it provides a clear abstraction, testing, or dependency-boundary benefit.

Current services do not require renaming merely because interfaces may be introduced later.

---

## 9.2 Methods

Use PascalCase.

Asynchronous methods end with `Async`.

Examples:

- `RegisterAsync`
- `LoginAsync`
- `CreateHabitAsync`
- `GetUserHabitsAsync`
- `CompleteHabitAsync`
- `UndoTodayAsync`
- `GetUserAttributesAsync`
- `ApplyCompletionRewardsAsync`
- `ReverseCompletionRewardsAsync`

Synchronous calculation methods should use clear action names.

Examples:

- `CalculateRewards`
- `CalculateAttributeLevelProgress`
- `CalculateOverallLevelProgress`
- `GetLocalDate`

Avoid vague methods such as:

- `Process`
- `Handle`
- `DoWork`
- `Run`
- `Manage`

unless the surrounding type and scope make the behavior unmistakable.

---

## 9.3 Properties

Use PascalCase.

Examples:

- `Id`
- `UserId`
- `HabitId`
- `HabitCompletionId`
- `CompletedDate`
- `CreatedAtUtc`
- `IsActive`
- `IsCompletedToday`
- `CurrentXp`
- `XpAmount`

Boolean properties should normally begin with:

- `Is`
- `Has`
- `Can`
- `Should`

Examples:

- `IsActive`
- `IsCompletedToday`
- `HasCompletedToday`
- `CanBeCompleted`

Use the property already established by the contract.

For example, the current habit response uses:

```text
IsCompletedToday
```

Do not create a duplicate `HasCompletedToday` property for the same meaning.

---

## 9.4 Local variables and parameters

Use camelCase.

Examples:

- `userId`
- `habitId`
- `completion`
- `completedDate`
- `currentXp`
- `xpAmount`
- `attributeType`
- `cancellationToken`

Prefer names that identify both type and purpose.

Good:

- `storedAttributes`
- `currentUtc`
- `completedAtUtc`
- `calculatedRewards`
- `userAttributes`

Avoid:

- `x`
- `data`
- `temp`
- `obj`
- `value`
- `thing`
- `result2`

Short names are acceptable only in tiny, obvious scopes.

---

## 9.5 Private fields

Use underscore followed by camelCase.

Examples:

- `_dbContext`
- `_timeProvider`
- `_authService`
- `_habitService`
- `_completionService`
- `_xpService`
- `_attributeService`

---

## 9.6 Constants

Use PascalCase.

Examples:

- `HabitCompletionReason`
- `PrimaryRewardPercentage`
- `AttributeBaseXp`
- `OverallBaseXp`

A constant name should communicate business meaning.

Avoid magic-number names such as:

- `Value1`
- `Number`
- `Limit`

---

## 9.7 Namespaces

Use PascalCase segments.

Examples:

- `HabitTracker.Api`
- `HabitTracker.Api.Controllers`
- `HabitTracker.Api.Data`
- `HabitTracker.Api.Domain.Entities`
- `HabitTracker.Api.Domain.Enums`
- `HabitTracker.Api.Domain.ValueObjects`
- `HabitTracker.Api.DTOs`
- `HabitTracker.Api.Services`
- `HabitTracker.Tests.Services`
- `HabitTracker.IntegrationTests.Controllers`

Namespace segments should match folder responsibilities where practical.

---

## 10. Entity Property Conventions

Primary keys use:

```text
Id
```

Foreign keys use the directly referenced entity name followed by `Id`.

Examples:

- `UserId`
- `HabitId`
- `HabitCompletionId`
- `MilestoneId`

`XpTransaction` uses `HabitCompletionId` because the transaction belongs to the exact completion that created it.

Do not substitute `HabitId` for that direct relationship.

UTC timestamp properties end with `Utc`.

Examples:

- `CreatedAtUtc`
- `UpdatedAtUtc`
- `CompletedAtUtc`
- `LastLoginAtUtc`
- `UnlockedAtUtc`

Date-only business properties describe the date being represented.

Example:

- `CompletedDate`

Collection navigation properties use plural names.

Examples:

- `Habits`
- `HabitCompletions`
- `HabitAttributeRewards`
- `UserAttributes`
- `XpTransactions`
- `UserMilestones`

---

## 11. XP and Progression Naming

Use `Xp`, not:

- `XP` in C# identifiers,
- `Experience`,
- `Exp`,
- `Points`.

Correct C# identifiers:

- `XpService`
- `XpTransaction`
- `XpAmount`
- `CurrentXp`
- `TotalXp`
- `XpIntoCurrentLevel`
- `XpNeededForNextLevel`

JSON uses camel case:

- `xpAmount`
- `currentXp`
- `totalXp`
- `xpIntoCurrentLevel`
- `xpNeededForNextLevel`

UI labels may use uppercase:

```text
XP
```

### Canonical progress names

Attribute progress uses:

- `CurrentXp`
- `Level`
- `XpIntoCurrentLevel`
- `XpNeededForNextLevel`

Do not use ambiguous alternatives such as:

- `XpRequiredForNextLevel`
- `NextLevelXp`
- `RemainingXp`
- `CurrentLevelXp`

unless an explicit contract change is approved.

The current implemented response uses:

```text
XpNeededForNextLevel
```

Phase 6 overall progress should use the same conceptual naming where practical.

---

## 12. DTO Naming Conventions

DTOs define data crossing the HTTP boundary.

Request DTOs end with `Request`.

Examples:

- `RegisterRequest`
- `LoginRequest`
- `CreateHabitRequest`
- `UpdateHabitRequest`
- `CompleteHabitRequest`

Response DTOs end with `Response`.

Implemented examples:

- `HealthResponse`
- `AntiforgeryTokenResponse`
- `AuthResponse`
- `CurrentUserResponse`
- `HabitResponse`
- `HabitAttributeRewardResponse`
- `HabitCompletionResponse`
- `CompleteHabitResponse`
- `UserAttributeResponse`

Planned examples:

- `DashboardResponse`
- focused nested dashboard responses when they improve clarity,
- `XpTransactionResponse` when XP history is exposed.

Do not expose entity classes directly.

Do not add both `Dto` and `Request` or `Response`.

Preferred:

```text
CreateHabitRequest
```

Avoid:

```text
CreateHabitRequestDto
```

### Response DTO calculated fields

Response DTOs may contain backend-calculated values that are not stored as entity properties.

Examples:

- `Level`
- `XpIntoCurrentLevel`
- `XpNeededForNextLevel`
- future `TotalXp`
- future `CurrentStreak`
- future `LongestStreak`

The presence of a response property does not require a matching database column.

---

## 13. DTO Evolution Rules

DTOs may gain fields when the same concept expands.

Implemented examples:

- `HabitResponse` gained `IsCompletedToday` in Phase 4.
- `HabitResponse` gained `AttributeRewards` in Phase 5.
- `CompleteHabitResponse` gained `Rewards` in Phase 5.

Important correction:

Habit create and update requests do **not** accept `AttributeRewards`.

The backend derives and synchronizes automatic rewards from category and difficulty.

Do not document or implement client-defined reward configuration during MVP.

A DTO should not be renamed merely because it gains related fields.

A contract change must update:

- backend DTO,
- service mapping,
- controller or endpoint behavior,
- backend tests,
- integration tests,
- TypeScript type,
- API tests,
- component fixtures,
- documentation.

---

## 14. Dashboard and Streak Naming

Phase 6 canonical service names are:

- `DashboardService`
- `StreakService`

Preferred controller name:

- `DashboardController`

Preferred initial endpoint direction:

```text
GET /api/dashboard
```

Preferred primary response:

- `DashboardResponse`

Potential nested response names should describe their exact purpose.

Examples:

- `OverallProgressResponse`
- `TodaySummaryResponse`
- `StreakSummaryResponse`
- `WeeklySummaryResponse`
- `RecentActivityResponse`

Do not create every possible nested type in advance.

Introduce one only when it improves clarity or reuse.

### Streak naming

Before implementation, approve the meaning of:

- `CurrentStreak`
- `LongestStreak`

Avoid vague alternatives:

- `Streak`
- `BestStreak`
- `ActiveDays`
- `Combo`

unless the product meaning is explicitly different.

Do not name a value `CurrentStreak` until the exact rule is documented and tested.

---

## 15. Controller Naming Conventions

Controller class names end with `Controller`.

Implemented:

- `HealthController`
- `AuthController`
- `HabitsController`
- `CompletionsController`
- `AttributesController`

Planned:

- `DashboardController`

Collection resource controllers generally use plural nouns.

Examples:

- `HabitsController`
- `CompletionsController`
- `AttributesController`

Authentication uses the domain grouping:

- `AuthController`

Controllers remain thin.

Do not create controllers named:

- `HabitManagementController`
- `GameController`
- `StatsController`
- `ProgressController`

when an established controller or service already owns the concept.

---

## 16. API Route Naming Conventions

Routes use:

- lowercase letters,
- plural resources where appropriate,
- nouns instead of redundant verbs,
- hyphens for multi-word route segments,
- stable paths once consumed by the frontend.

Implemented examples:

- `/api/health`
- `/api/auth/csrf-token`
- `/api/auth/register`
- `/api/auth/login`
- `/api/auth/logout`
- `/api/auth/me`
- `/api/habits`
- `/api/habits/{habitId}`
- `/api/habits/{habitId}/completions`
- `/api/habits/{habitId}/completions/today`
- `/api/attributes`

Proposed Phase 6 direction:

- `/api/dashboard`

Avoid unnecessary verbs when HTTP methods already express the operation.

Good:

```text
DELETE /api/habits/{habitId}
```

Avoid:

```text
POST /api/habits/{habitId}/deactivate
```

The completion endpoint is a resource collection.

Undoing today’s completion deletes the current-date resource.

---

## 17. Public JSON Naming

C# property names use PascalCase.

Public JSON uses camel case.

Examples:

| C# | JSON |
|---|---|
| `IsCompletedToday` | `isCompletedToday` |
| `AttributeRewards` | `attributeRewards` |
| `XpAmount` | `xpAmount` |
| `CurrentXp` | `currentXp` |
| `XpIntoCurrentLevel` | `xpIntoCurrentLevel` |
| `XpNeededForNextLevel` | `xpNeededForNextLevel` |
| `CompletedAtUtc` | `completedAtUtc` |

Enum members use PascalCase in C# and camel case in JSON.

Examples:

| C# | JSON |
|---|---|
| `LearningAndSkills` | `learningAndSkills` |
| `FitnessAndMovement` | `fitnessAndMovement` |
| `Discipline` | `discipline` |
| `Elite` | `elite` |
| `Weekly` | `weekly` |

Numeric enum values are not part of the API contract.

---

## 18. React Component Naming Conventions

React components use PascalCase.

Component files match component names.

Implemented examples:

- `LoginForm.tsx`
- `RegisterForm.tsx`
- `HabitForm.tsx`
- `HabitEditForm.tsx`
- `HabitList.tsx`
- `HabitSection.tsx`
- `HabitCompletionButton.tsx`
- `HabitDeactivateButton.tsx`
- `AttributeSection.tsx`

Potential later examples:

- `DashboardPage.tsx`
- `AppShell.tsx`
- `Sidebar.tsx`
- `TopBar.tsx`
- `AttributeCard.tsx`
- `RewardPreview.tsx`
- `XpProgressBar.tsx`
- `LevelBadge.tsx`
- `StreakBadge.tsx`
- `WeeklyReviewPage.tsx`

Potential names are not instructions to create files early.

Create a component when a real screen needs it.

### Component responsibility names

Use names that describe what the component renders or does.

Good:

- `HabitCompletionButton`
- `HabitDeactivateButton`
- `AttributeSection`

Avoid:

- `HabitThing`
- `ActionButton`
- `MainCard`
- `StatsWidget`
- `GamePanel`

unless the component is truly generic and its API proves that abstraction.

---

## 19. TypeScript Type Naming

TypeScript types use PascalCase.

Frontend types representing backend DTOs use the same conceptual name.

Implemented examples:

- `AuthResponse`
- `CurrentUserResponse`
- `HabitResponse`
- `HabitAttributeRewardResponse`
- `HabitCompletionResponse`
- `CompleteHabitResponse`
- `UserAttributeResponse`
- `AttributeType`
- `HabitCategory`
- `HabitDifficulty`
- `HabitFrequencyType`

Do not rename the same contract differently in React.

Avoid:

- `HabitModel`
- `HabitData`
- `AttributeDto`
- `CompletionResult`

when the backend contract is already named.

---

## 20. TypeScript Variables and Functions

Use camelCase.

Examples:

- `habitId`
- `refreshKey`
- `earnedRewards`
- `isSaving`
- `errorMessage`
- `completeHabit`
- `undoHabitCompletion`
- `getAttributes`
- `refreshAttributes`

Functions should describe the action.

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

Avoid vague names:

- `fetchData`
- `send`
- `handleApi`
- `doRequest`

unless scoped within a clearly generic module.

---

## 21. React Hook Naming

Custom hooks begin with `use`.

Examples:

- `useAuth`
- future `useDashboard`

Do not create hooks solely to move code out of a component.

A hook should provide real reusable state or behavior.

Hooks may own:

- request orchestration,
- loading state,
- local state,
- reusable interaction behavior.

Hooks must not own:

- XP formulas,
- reward mapping,
- level formulas,
- streak rules,
- ownership rules,
- completion validation.

---

## 22. Boolean Naming

Boolean values use clear prefixes.

Preferred prefixes:

- `is`
- `has`
- `can`
- `should`

Examples:

- `isLoading`
- `isSaving`
- `isAuthenticated`
- `isActive`
- `isCompletedToday`
- `hasAttachment`
- `canSubmit`
- `shouldRefresh`

Avoid:

- `loadingFlag`
- `completed`
- `activeValue`
- `statusBoolean`

when a clearer boolean name exists.

---

## 23. Frontend File and Folder Naming

Top-level source folders use lowercase names.

Current folders include:

- `api`
- `auth`
- `components`
- `types`

Current feature component folders include:

- `components/auth`
- `components/habits`
- `components/attributes`

Later folders may be introduced only when required:

- `pages`
- `hooks`
- `layout`
- `dashboard`
- `utils`

Do not create empty architecture folders.

### Component files

Use PascalCase:

- `HabitList.tsx`
- `HabitCompletionButton.tsx`
- `AttributeSection.tsx`

### Non-component TypeScript files

Use camelCase:

- `apiClient.ts`
- `authApi.ts`
- `habitsApi.ts`
- `attributesApi.ts`
- `healthApi.ts`
- `readApiError.ts`
- `useAuth.ts`

### Test files

Frontend unit and component tests use:

- `.test.ts`
- `.test.tsx`

Examples:

- `attributesApi.test.ts`
- `HabitCompletionButton.test.tsx`
- `AttributeSection.test.tsx`

Playwright tests use:

- `.spec.ts`

Example:

- `auth-smoke.spec.ts`

---

## 24. Frontend Formatting and Validation

Prettier is the canonical frontend formatter.

After frontend changes, use project scripts rather than manually preserving formatting.

Typical validation:

```text
cd client
npm run format
npm run format:check
npm run lint
npm run test
npm run build
```

For a small slice:

1. run Prettier on changed files,
2. run focused tests,
3. run the full frontend suite,
4. run lint,
5. run the production build.

Do not interpret formatting-only diffs as architecture changes.

Do not manually fight Prettier line wrapping.

---

## 25. Backend File Naming

A C# file normally matches its main type.

Examples:

- `Habit.cs`
- `HabitCompletion.cs`
- `HabitAttributeReward.cs`
- `UserAttribute.cs`
- `XpTransaction.cs`
- `HabitService.cs`
- `CompletionService.cs`
- `XpService.cs`
- `AttributeService.cs`
- `HabitsController.cs`
- `AttributesController.cs`
- `CreateHabitRequest.cs`
- `CompleteHabitResponse.cs`
- `HabitCategory.cs`
- `AttributeType.cs`
- `AppDbContext.cs`

Avoid putting unrelated public types in one file.

---

## 26. Database Naming

C# entities and properties use C# conventions.

Physical PostgreSQL identifiers use `snake_case`.

Examples:

| C# | PostgreSQL |
|---|---|
| `UserSettings` | `user_settings` |
| `HabitCompletions` | `habit_completions` |
| `HabitAttributeRewards` | `habit_attribute_rewards` |
| `UserAttributes` | `user_attributes` |
| `XpTransactions` | `xp_transactions` |
| `CreatedAtUtc` | `created_at_utc` |
| `AttributeType` | `attribute_type` |

The normal local database is:

```text
habit_tracker
```

The application role is:

```text
habit_tracker_app
```

Use actual lowercase identifiers in PostgreSQL queries.

Correct:

```sql
SELECT id, user_id, name, category, frequency_type, difficulty
FROM habits
ORDER BY created_at_utc DESC;
```

Avoid guessed quoted PascalCase identifiers:

```sql
SELECT "Id", "UserId"
FROM "Habits";
```

Inspect the schema before writing direct queries.

Examples:

```text
\d habits
\d habit_completions
\d habit_attribute_rewards
\d user_attributes
\d xp_transactions
```

Database names, constraints, tables, columns, and indexes must not be renamed casually after migrations exist.

---

## 27. Database Constraint and Index Naming

Physical constraint and index names use `snake_case`.

Examples:

- `pk_habits`
- `fk_habits_users_user_id`
- `ix_habits_user_id_is_active`
- `ix_habit_completions_habit_id_completed_date`
- `ck_habits_frequency_target_count`
- `ck_habit_attribute_rewards_xp_amount`
- `ck_user_attributes_current_xp`
- `ck_xp_transactions_amount`

Names should identify:

- object type,
- table,
- relevant columns or rule.

Do not rename constraints merely for cosmetic preference after migrations and data exist.

---

## 28. Migration Naming

EF Core migration names use PascalCase and describe the schema change.

Examples:

- `AddAuthentication`
- `AddHabits`
- `AddHabitCompletions`
- `AddXpAndAttributes`

Use a name that describes the change, not the date or ticket number alone.

Avoid:

- `UpdateDatabase`
- `Changes`
- `Fix`
- `Migration2`

Do not delete or rewrite migration history casually.

---

## 29. Test Project Naming

Backend projects:

- `HabitTracker.Api`
- `HabitTracker.Tests`
- `HabitTracker.IntegrationTests`

`HabitTracker.Tests` contains focused backend tests.

`HabitTracker.IntegrationTests` contains HTTP pipeline tests.

Do not merge them merely for convenience.

---

## 30. Test Class Naming

Test classes describe the system under test.

Examples:

- `AuthControllerTests`
- `HabitsControllerTests`
- `CompletionsControllerTests`
- `AttributesControllerTests`
- `HabitServiceTests`
- `CompletionServiceTests`
- `XpServiceTests`
- `AttributeServiceTests`
- future `StreakServiceTests`
- future `DashboardServiceTests`

Frontend test names match their source:

- `HabitCompletionButton.test.tsx`
- `HabitList.test.tsx`
- `AttributeSection.test.tsx`
- `attributesApi.test.ts`

---

## 31. Test Method Naming

Backend test methods should communicate:

1. behavior or method,
2. condition,
3. expected outcome.

Examples:

- `CompleteHabitAsync_WhenHabitAlreadyCompleted_ThrowsConflictException`
- `UndoTodayAsync_WhenCompletionExists_RemovesCompletionAndRewards`
- `GetUserAttributesAsync_ReturnsAllSupportedAttributes`
- `GetCurrentUser_WhenAnonymous_ReturnsUnauthorized`

Use readable names over clever abbreviations.

Frontend test descriptions should describe observable behavior.

Examples:

- `shows earned XP after completion`
- `refreshes attributes after completion`
- `shows an error when the request fails`

Tests should describe user-visible or contract-visible behavior rather than private implementation details.

---

## 32. UI Naming and Display Labels

Code names and UI labels may differ in presentation while preserving meaning.

Examples:

| Domain name | UI label |
|---|---|
| `LearningAndSkills` | Learning & Skills |
| `DailyLifeAndOrganization` | Daily Life & Organization |
| `XpNeededForNextLevel` | XP to next level |
| `IsCompletedToday` | Completed today |

Do not rename backend concepts solely to make labels prettier.

Use formatting functions or label maps in the frontend.

The same attribute should use the same display label and visual identity on every screen.

Examples:

- `Discipline` always displays as Discipline.
- `Fitness` must not display as Strength.
- `Purpose` must not display as Spirituality in one screen and Purpose in another.

---

## 33. Approved UI Component Direction

Potential Phase 7 names include:

### Layout

- `AppShell`
- `Sidebar`
- `TopBar`
- `PageContent`
- `PageHeader`
- `Toolbar`
- `DetailDrawer`

### Foundation

- `Panel`
- `Card`
- `Button`
- `IconButton`
- `Tabs`
- `Badge`
- `Pill`
- `ProgressBar`
- `Modal`
- `ConfirmationDialog`

### Progression

- `XpProgressBar`
- `LevelBadge`
- `StreakBadge`
- `AttributeCard`
- `AttributeGrid`
- `RewardPreview`
- `XpTransactionList`

### Analytics

- `StatCard`
- `DonutChart`
- `LineChart`
- `RadarChart`
- `Heatmap`
- `InsightCard`

These names describe the approved direction.

They must not be created as empty speculative components.

---

## 34. Names to Avoid from Mockups

UI reference images may contain placeholder concepts.

Do not introduce names such as:

- `FocusScore`
- `Coins`
- `Gold`
- `Mana`
- `Rank`
- `Quest`
- `InventoryItem`
- `Avatar`
- `LeaderboardEntry`
- `Notification`
- `PublicProfile`

until their behavior is explicitly approved and implemented.

A visual placeholder is not a canonical domain name.

---

## 35. Rename Rules

Before changing a stable name:

1. Explain why the current name is inadequate.
2. Confirm that the change solves a real problem.
3. Identify every affected layer.
4. Search entities, enums, DTOs, services, controllers, routes, TypeScript types, components, tests, migrations, and documentation.
5. Identify database and API compatibility impact.
6. Decide whether migration or backward compatibility is required.
7. Update documentation alongside implementation.
8. Make the rename in one focused commit.
9. Run the full relevant test suite.
10. Manually verify the user-visible flow.

Core names must not change during unrelated debugging.

---

## 36. New Name Approval Filter

Before introducing a new domain, service, DTO, or component name, ask:

1. Does an existing canonical concept already own this responsibility?
2. Is this current-phase work?
3. Is the name describing domain behavior or only a visual panel?
4. Will the name remain understandable outside the current screen?
5. Does the backend or frontend actually need a new type?
6. Is the term already used differently elsewhere?
7. Does the name imply unsupported behavior?
8. Is the new abstraction more useful than the existing concrete code?

When uncertain, preserve existing names and add the smallest specific type required.

---

## 37. Contract Synchronization Rule

When a backend response property is added or renamed, update together:

- backend DTO,
- service mapping,
- controller tests,
- integration tests,
- JSON contract tests,
- frontend TypeScript type,
- API fixture,
- component fixtures,
- consuming components,
- `API_CONTRACT.md`,
- this naming guide when the concept is stable.

Do not allow backend and frontend names to drift.

Example:

```text
CompleteHabitResponse.Rewards
```

must correspond to:

```text
CompleteHabitResponse.rewards
```

in TypeScript and JSON.

---

## 38. File-Safety Rule

Before replacing an entire file:

1. confirm the exact file path,
2. confirm the tab or editor buffer,
3. confirm the file name matches the intended type,
4. inspect `git status --short` afterward,
5. inspect `git diff -- <path>` before staging.

This prevents accidentally copying one component or test into another file with a similar name.

Before committing:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Every staged filename should be intentional.

---

## 39. Final Rule

The domain model supplies the canonical names.

The API preserves those names across the HTTP boundary.

TypeScript mirrors the API contract.

React presents friendly labels without redefining the domain.

PostgreSQL uses predictable `snake_case` mappings.

Tests and documentation use the same vocabulary.

A name should change only through an explicit, cross-layer decision.
