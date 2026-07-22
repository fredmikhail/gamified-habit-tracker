# Gamified Habit Tracker — Project Overview

**Project status:** Active development

**Completed phases:** 1 through 6

**Current phase:** Phase 7 — Game UI Polish

**Architecture:** React client, ASP.NET Core API, PostgreSQL database

**Core principle:** Persistent progression is calculated by the backend and derived from auditable history

---

## 1. Product Summary

Gamified Habit Tracker is a full-stack habit-tracking application that converts completed habits into persistent XP, character attributes, levels, dashboard metrics, and habit-specific streaks.

The application is built around a simple progression loop:

1. A user creates a habit.
2. The backend assigns deterministic attribute rewards.
3. The user completes the habit for the current local date.
4. The completion is stored with the configuration that was active at that time.
5. XP transactions and attribute progression are applied.
6. Dashboard values and streaks are derived from the persisted history.
7. An undo records a reversal without deleting the original action.

The frontend presents the current state and sends user actions. It does not calculate XP, levels, completion dates, streaks, reward allocation, or historical rule interpretation.

---

## 2. Current Status

The following phases are complete:

- Phase 1 — Project Setup
- Phase 2 — Authentication
- Phase 3 — Habit CRUD
- Phase 4 — Habit Completion
- Phase 5 — XP and Attributes
- Phase 6 — Dashboard and Streaks

The current application supports:

- registration, login, logout, and session restoration,
- private user-owned data,
- daily and weekly habits,
- controlled categories and difficulties,
- habit creation, editing, and deactivation,
- effective-dated habit rule history,
- scheduled rule changes,
- local-date completion,
- duplicate-completion protection,
- auditable completion undo,
- deterministic XP rewards,
- character attribute progression,
- append-only XP reversals,
- overall level progression,
- dashboard aggregation,
- daily execution metrics,
- daily and weekly streaks,
- backend, frontend, integration, and browser tests,
- automated validation through GitHub Actions.

The application is functionally complete through the dashboard and streak phase.

Phase 7 will replace the current functional interface with the final application shell, visual system, responsive layouts, and interaction polish.

---

## 3. Product Principles

### 3.1 Persisted behavior before presentation

The interface only displays values backed by implemented application behavior.

The application does not expose placeholder systems such as:

- currencies,
- quests,
- ranks,
- inventories,
- social metrics,
- leaderboards,
- notifications,
- artificial scores.

### 3.2 Backend authority

The backend owns:

- authentication,
- authorization,
- resource ownership,
- input validation,
- habit rule validation,
- local-date calculation,
- calendar-period calculation,
- configuration-history interpretation,
- completion rules,
- undo behavior,
- XP allocation,
- attribute progression,
- level calculation,
- dashboard aggregation,
- streak calculation.

React displays backend responses and submits user actions.

### 3.3 PostgreSQL as the source of truth

Persistent application state is stored in PostgreSQL.

This includes:

- users,
- user settings,
- habits,
- habit configuration versions,
- completions,
- reward mappings,
- user attributes,
- XP transactions.

Frontend state is temporary and must not replace persisted state.

### 3.4 History is not rewritten

Past events retain their original meaning.

Later edits must not silently change:

- the rules that applied to an earlier completion,
- the XP awarded by an earlier completion,
- the weekly target used for an earlier streak period,
- the frequency that was active during an earlier period,
- the record of a completion that was later undone.

This rule is enforced through effective-dated habit configurations, completion-to-configuration linkage, undo timestamps, and append-only XP reversals.

### 3.5 Future-compatible, not speculative

The current architecture supports the approved direction without creating unused systems.

Appropriate preparation includes:

- stable domain names,
- service boundaries,
- DTO contracts,
- effective-dated history,
- auditable transactions,
- reusable frontend components,
- aggregated dashboard responses.

The project does not create unused tables, placeholder APIs, fake progression systems, or abstractions without a current requirement.

---

## 4. Technology Stack

### Frontend

- React
- TypeScript
- Vite
- Tailwind CSS

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core

### Database

- PostgreSQL

### Backend testing

- xUnit
- ASP.NET Core `WebApplicationFactory`
- EF Core InMemory provider for isolated service tests
- PostgreSQL-backed integration and browser environments

### Frontend testing

- Vitest
- React Testing Library
- Playwright

### Code quality and automation

- Prettier
- Oxlint
- GitHub Actions
- Repository-local `dotnet-ef` tool manifest

---

## 5. High-Level Architecture

```text
React + TypeScript client
            |
            | HTTP / JSON
            v
ASP.NET Core Web API
            |
            | Entity Framework Core
            v
PostgreSQL
```

Frontend and backend are separate applications.

During local development:

```text
Browser
   |
   | /api
   v
Vite development server
   |
   | proxy
   v
ASP.NET Core HTTPS endpoint
```

The Vite proxy keeps browser requests same-origin during development while preserving the frontend/backend separation.

---

## 6. Architecture Rules

- Frontend and backend remain separate applications.
- Communication occurs through HTTP and JSON.
- Controllers remain thin.
- Services contain application and domain behavior.
- DTOs define the HTTP boundary.
- Database entities are not returned directly by API controllers.
- Entity Framework Core owns database access.
- PostgreSQL stores persistent truth.
- User identity comes from authenticated backend claims.
- User-owned endpoints do not accept a client-supplied `userId`.
- Business calculations remain outside React components.
- Tests are added alongside protected behavior.
- Stable domain names are preserved.
- Changes to architecture, stack, naming, or MVP scope require an explicit decision.

---

## 7. Canonical Domain Model

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

The reserved names remain canonical but are not implemented in the current MVP.

---

## 8. Core Backend Services

Implemented services:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

Supporting calculation components include:

- `LocalDateCalculator`
- `CalendarPeriodCalculator`

Responsibilities remain separated:

- `AuthService` handles registration, login, and current-user behavior.
- `HabitService` handles habit creation, retrieval, editing, deactivation, and rule scheduling.
- `CompletionService` handles completion and undo workflows.
- `XpService` handles reward values and overall level calculations.
- `AttributeService` applies and returns attribute progression.
- `StreakService` derives daily and weekly streak results.
- `DashboardService` aggregates the authenticated user’s dashboard response.

---

## 9. Character Attributes

The eight character attributes are:

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

The same identity is preserved across:

- backend enums,
- entity data,
- API JSON,
- frontend types,
- component labels,
- tests,
- documentation.

Older attribute names are obsolete and must not be reintroduced.

---

## 10. Habit Categories

Supported habit categories:

- Fitness & Movement
- Health & Recovery
- Learning & Skills
- Work & Career
- Daily Life & Organization
- Money & Finance
- Relationships & Community
- Emotional Well-Being
- Spirituality & Purpose
- Creativity & Recreation
- Self-Control & Boundaries
- General Growth

Canonical enum members:

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

Categories are controlled domain values rather than free-form strings.

---

## 11. Difficulty and Reward Rules

Supported difficulties:

- Easy
- Medium
- Hard
- Elite

XP awarded by difficulty:

| Difficulty | Total XP |
| ---------- | -------: |
| Easy       |       10 |
| Medium     |       20 |
| Hard       |       30 |
| Elite      |       50 |

Each category maps to:

- one primary attribute,
- one secondary attribute.

Reward distribution:

| Reward role         | Share |
| ------------------- | ----: |
| Primary attribute   |   70% |
| Secondary attribute |   30% |

The backend creates and synchronizes the reward mapping.

The frontend may display the returned reward information but does not reproduce the authoritative calculation.

---

## 12. Authentication

Authentication uses ASP.NET Core encrypted cookies.

Implemented behavior includes:

- registration,
- login,
- logout,
- current-session restoration,
- optional remembered sessions,
- password hashing,
- case-insensitive email uniqueness,
- case-insensitive username uniqueness,
- Guid version 7 identifiers,
- authenticated ownership identity,
- Problem Details error responses.

### Cookie behavior

The authentication credential is stored in an encrypted browser cookie.

The frontend does not receive or store an authentication token.

Default sessions:

- expire after no more than 12 hours,
- are non-persistent.

Remembered sessions:

- may persist for up to 30 days.

Sliding expiration is disabled.

Cookie settings include:

- `HttpOnly`,
- `SameSite=Lax`,
- `Secure` outside local development.

### Antiforgery protection

State-changing requests are protected by ASP.NET Core antiforgery validation.

The frontend obtains a request token through:

```text
GET /api/auth/csrf-token
```

The token is sent through:

```text
X-CSRF-TOKEN
```

---

## 13. User Calendar Settings

`UserSettings` stores calendar behavior that affects completion and streak calculations.

Current settings include:

- IANA time-zone identifier,
- preferred first day of the week.

The default first day of the week is Monday.

The backend derives the current local date from:

```text
Current UTC timestamp + stored IANA time zone
```

The client cannot choose the date assigned to a completion.

Weekly periods are calculated from the user’s `WeekStartsOn` preference through `CalendarPeriodCalculator`.

---

## 14. Habit Management

Implemented habit behavior includes:

- create,
- list,
- retrieve,
- edit,
- soft-deactivate,
- active-only listing by default,
- optional inactive inclusion,
- deterministic ordering,
- ownership enforcement.

Habit fields include:

- name,
- optional description,
- category,
- frequency,
- target count,
- difficulty,
- active status,
- creation timestamp,
- update timestamp.

### Frequency rules

Supported frequencies:

- Daily
- Weekly

Daily habits require:

```text
TargetCount = 1
```

Weekly habits require:

```text
1 <= TargetCount <= 7
```

The backend validates these rules.

---

## 15. Effective-Dated Habit Configuration

Habit rule changes do not overwrite historical meaning.

The following values are versioned through `HabitConfigurationVersion`:

- category,
- frequency,
- target count,
- difficulty.

Each version records:

- habit identifier,
- version number,
- effective start date,
- optional exclusive end date,
- rule values,
- creation timestamp.

Effective periods use half-open intervals:

```text
EffectiveFromDate <= date < EffectiveToDateExclusive
```

The active configuration has:

```text
EffectiveToDateExclusive = null
```

### Scheduled edits

Changes to category, frequency, target count, or difficulty are scheduled for the next user-local week boundary.

Until that date:

- the current configuration remains effective,
- the new configuration is returned as pending,
- completion and streak logic continue using the current configuration.

Only one pending configuration is maintained for the next week boundary.

Editing a pending configuration updates that pending version.

Editing the rules back to the current effective values cancels the pending version and restores the current version to an open-ended period.

Name and description changes take effect immediately because they do not alter historical reward or streak interpretation.

---

## 16. Completion-to-Configuration Linkage

Each `HabitCompletion` stores the `HabitConfigurationVersionId` that was effective when the completion was created.

This preserves the relationship between an event and the rule set that produced it.

The linkage allows the backend to determine:

- which difficulty produced the XP reward,
- which category produced the attribute mapping,
- which frequency was active,
- which weekly target applied,
- which configuration should be referenced during undo,
- where a streak frequency segment begins and ends.

A later habit edit does not reinterpret an earlier completion.

---

## 17. Habit Completion

Implemented completion behavior includes:

- complete an active owned habit,
- assign the current user-local date,
- store the exact UTC completion timestamp,
- link the effective configuration version,
- reject inactive habits,
- reject another user’s habit,
- reject duplicate active completion for the same local date,
- apply XP and attributes atomically,
- return the applied rewards to the frontend.

The client does not submit `CompletedDate`.

### Active completion uniqueness

Only one active completion may exist for:

```text
HabitId + CompletedDate
```

The uniqueness rule applies only when:

```text
UndoneAtUtc IS NULL
```

An undone record remains in history but no longer prevents a new active completion for the same habit and date.

---

## 18. Auditable Undo

Undo does not delete the completion.

When a completion is undone:

- the original `HabitCompletion` remains,
- `UndoneAtUtc` records the reversal time,
- the original positive XP transactions remain,
- matching negative XP transactions are appended,
- user attribute totals are reduced,
- overall XP is reduced,
- dashboard values are recalculated,
- streaks are recalculated.

The XP ledger is append-only.

Historical XP transactions are not edited or deleted to simulate a reversal.

This preserves both sides of the event:

```text
Original completion
Reversal of that completion
```

An undone completion is ignored by:

- active completion counts,
- today activity,
- daily execution,
- daily streaks,
- weekly streaks.

---

## 19. XP and Attribute Progression

Completing a habit updates:

- `HabitCompletion`,
- `XpTransaction`,
- `UserAttribute`.

The operation is processed through one EF Core unit of work so completion history and progression remain consistent.

XP transactions identify:

- the user,
- the related completion,
- the affected attribute,
- the signed XP amount,
- the transaction timestamp.

Positive values represent earned XP.

Negative values represent undo reversals.

### Attribute levels

`UserAttribute` stores current XP.

Attribute levels and progress toward the next level are calculated by the backend rather than stored as mutable counters.

### Overall level

Overall progression is derived from the user’s XP transaction history.

The dashboard returns:

- total XP,
- current level,
- XP earned within the current level,
- XP required for the next level.

---

## 20. Dashboard

The dashboard is returned through:

```text
GET /api/dashboard
```

`DashboardService` aggregates the authenticated user’s current progression state.

The response includes:

### Overall progression

- total XP,
- current overall level,
- XP within the current level,
- XP needed for the next level.

### Today activity

- current user-local date,
- number of active completions today,
- XP earned today.

### Daily execution

- active daily habits completed today,
- total active daily habits.

Daily execution uses the configuration effective on the current local date.

### Habit streaks

Each active habit returns:

- habit identifier,
- habit name,
- current effective frequency,
- current streak,
- longest streak.

Inactive habits are excluded from the active dashboard response.

Their configuration, completion, and XP history remains in PostgreSQL.

### Frontend refresh behavior

Dashboard data reloads after:

- habit creation,
- habit editing,
- habit deactivation,
- habit completion,
- completion undo.

This keeps progression, execution, and streak cards synchronized with the latest backend state without a full browser refresh.

---

## 21. Streak Calculation

Streaks are derived by `StreakService`.

They are not stored as mutable counters.

The calculation uses:

- effective-dated configuration history,
- active non-undone completions,
- the current user-local date,
- the user’s preferred week start.

Streaks are calculated per habit.

There is no combined cross-habit streak.

Each result contains:

- `CurrentStreak`
- `LongestStreak`

Daily streak values use days.

Weekly streak values use weeks.

---

## 22. Daily Streak Rules

A daily period is successful when the habit has an active, non-undone completion on that local date.

Rules:

- A completion today immediately counts toward the current streak.
- An incomplete current day does not break an existing streak before the day ends.
- When today is incomplete, the current streak may continue through yesterday.
- A missed past day breaks the current streak.
- The longest streak is the longest consecutive run of successful eligible days.
- Completions before the current frequency segment are ignored.
- Future completion dates are ignored.
- Undone completions are ignored.

Example:

```text
Monday     completed
Tuesday    completed
Wednesday  currently incomplete
```

On Wednesday, the current streak remains two days because the current day has not ended.

---

## 23. Weekly Streak Rules

A weekly period is successful when its active completion count reaches the target that was effective for that week.

Rules:

- Week boundaries follow `UserSettings.WeekStartsOn`.
- Reaching the target early counts immediately.
- An incomplete current week does not break the streak before the week ends.
- A failed completed week breaks the current streak.
- Historical weeks use their historical target counts.
- A later target change does not reinterpret earlier weeks.
- The first partial week is a grace period when its target is not reached.
- A successful first partial week still counts.
- Future completion dates are ignored.
- Undone completions are ignored.

Example:

```text
Historical target: 3
Later target:      5
```

Earlier weeks continue to require three completions. Only weeks within the later configuration period require five.

---

## 24. Configuration Changes and Streak Continuity

Category and difficulty changes do not reset a streak.

Weekly target changes do not reset a streak. Each week continues to use the target effective during that period.

Frequency changes create a new streak series:

```text
Daily → Weekly
Weekly → Daily
```

The new frequency segment begins on the configuration’s effective date.

Completions from an earlier frequency segment do not carry into the new current streak.

Historical records remain available in the database, but the active streak calculation follows only the current contiguous frequency segment.

---

## 25. Undo and Streak Recalculation

Undo affects streaks immediately.

Because streaks are derived from completion history:

- undoing today may reduce the current streak,
- undoing a historical completion may break a longer run,
- the longest streak may decrease when the completion supporting it is undone,
- completing the habit again may rebuild the current result.

No separate streak row needs to be repaired or synchronized.

---

## 26. Current API Surface

### Health

```text
GET /api/health
```

### Authentication

```text
GET  /api/auth/csrf-token
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/me
```

### Habits

```text
GET    /api/habits
GET    /api/habits/{habitId}
POST   /api/habits
PUT    /api/habits/{habitId}
DELETE /api/habits/{habitId}
```

### Completions

```text
POST   /api/habits/{habitId}/completions
DELETE /api/habits/{habitId}/completions/today
```

### Attributes

```text
GET /api/attributes
```

### Dashboard

```text
GET /api/dashboard
```

All user-owned operations resolve the user from the authenticated session.

---

## 27. Frontend Application

The current frontend includes:

- registration and login forms,
- authenticated-session restoration,
- habit creation,
- habit editing,
- pending configuration display,
- habit deactivation confirmation,
- completion and undo controls,
- earned-XP feedback,
- attribute progress cards,
- overall XP and level progress,
- today activity,
- daily execution,
- daily and weekly streak cards,
- loading states,
- pending states,
- empty states,
- error states.

The frontend uses TypeScript types matching the API response contracts.

Progression and streak calculations remain in the backend.

The current interface is functional scaffolding. Phase 7 will replace its presentation without changing the ownership of application logic.

---

## 28. Testing Strategy

### Backend unit tests

Backend unit tests protect:

- authentication service behavior,
- habit validation,
- configuration scheduling,
- completion rules,
- undo behavior,
- XP calculations,
- attribute calculations,
- dashboard aggregation,
- daily streak rules,
- weekly streak rules,
- frequency-change behavior,
- target-history behavior,
- undone-completion behavior.

### HTTP integration tests

Integration tests protect:

- route behavior,
- authentication requirements,
- antiforgery behavior,
- status codes,
- Problem Details responses,
- ownership boundaries,
- database integration,
- dashboard serialization,
- streak results through the HTTP endpoint.

### Frontend tests

Frontend tests protect:

- API client behavior,
- form behavior,
- component rendering,
- loading states,
- error states,
- completion interactions,
- undo interactions,
- dashboard refresh behavior,
- daily and weekly streak units,
- empty streak states.

### Browser tests

Playwright covers complete user journeys against a running frontend, API, and PostgreSQL database.

### Manual validation

Phase 6 was manually verified through the running application.

The validation covered:

- creating daily and weekly habits,
- zero-value streak display,
- completing habits,
- XP and attribute refresh,
- current and longest streak updates,
- undo recalculation,
- completing again after undo,
- habit rename propagation,
- weekly streak display,
- deactivation removal,
- dashboard refresh without browser reload.

---

## 29. Continuous Integration

GitHub Actions validates pushes and pull requests.

The current pipeline covers:

- backend restore,
- backend build,
- backend unit tests,
- backend integration tests,
- frontend dependency installation,
- formatting checks,
- linting,
- frontend tests,
- production frontend build,
- Playwright browser tests,
- PostgreSQL-backed test execution.

Generated migrations and unrelated files are not reformatted as part of feature commits unless a dedicated formatting change is being made.

---

## 30. Current Scope

The current MVP includes:

- registration,
- login,
- logout,
- session restoration,
- user settings,
- private user-owned data,
- habit creation,
- habit editing,
- habit deactivation,
- daily habits,
- weekly habits,
- scheduled rule changes,
- configuration history,
- local-date completion,
- duplicate protection,
- completion undo,
- auditable completion history,
- deterministic XP,
- character attributes,
- attribute levels,
- overall level,
- today activity,
- daily execution,
- current per-habit streak,
- longest per-habit streak,
- configurable week start,
- dashboard aggregation,
- frontend dashboard presentation,
- automated testing,
- continuous integration.

---

## 31. Deferred Scope

The following are outside the current MVP:

- advanced scheduling,
- sleep tracking,
- bad-habit tracking,
- journal entries,
- reminders,
- notifications,
- AI recommendations,
- social features,
- leaderboards,
- public profiles,
- avatars,
- theme marketplace,
- quests,
- inventory,
- currencies,
- partial completion,
- complex analytics,
- mobile applications,
- payments,
- administration panel,
- calendar integration,
- milestones,
- achievements.

Deferred features require separate product, domain, and architecture decisions.

---

## 32. Phase 7 — Game UI Polish

Phase 7 focuses on the presentation layer.

Planned work includes:

- application shell,
- sidebar and top navigation,
- final color and typography system,
- dashboard layout refinement,
- habit workspace refinement,
- attribute presentation,
- responsive layouts,
- accessible interaction states,
- keyboard focus behavior,
- loading and empty-state polish,
- restrained transitions and motion,
- reduced-motion support.

Phase 7 must use the existing backend contracts and must not move business logic into React.

---

## 33. Phase 8 — Deployment and Project Polish

Phase 8 focuses on release readiness.

Planned work includes:

- production hosting,
- production PostgreSQL configuration,
- environment-variable documentation,
- migration execution,
- production cookie verification,
- antiforgery verification,
- logging review,
- error-handling review,
- deployment validation,
- application screenshots,
- final documentation review,
- release preparation.

Phase 8 is not an additional feature phase.

---

## 34. Development Workflow

Development is organized into small vertical slices.

A typical slice includes:

1. Inspect the current implementation.
2. Identify the owning service.
3. Define or update the contract.
4. Implement the smallest backend change.
5. Add focused unit tests.
6. Expose or update the endpoint.
7. Add integration tests.
8. Update frontend types.
9. Update the API client.
10. Add or update the React presentation.
11. Add frontend tests.
12. Run the relevant validation suites.
13. Verify the behavior manually when appropriate.
14. Inspect the Git diff.
15. Commit one coherent change.

Before committing:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Every staged file must belong to the intended change.

Repository-wide formatting is kept separate from feature work.

---

## 35. Documentation Responsibilities

Each document answers a different question.

### `README.md`

How does a developer understand, run, and validate the repository?

### `PROJECT_OVERVIEW.md`

What does the product currently do, and what decisions define it?

### `ROADMAP.md`

What has been completed, what is next, and what remains deferred?

### `ARCHITECTURE.md`

Where does each responsibility belong?

### `DATA_MODEL.md`

What data is persisted, and how is history preserved?

### `API_CONTRACT.md`

What requests and responses cross the HTTP boundary?

### `NAMING_CONVENTIONS.md`

Which names, values, and casing rules are canonical?

Documentation must clearly distinguish between:

- implemented behavior,
- current phase work,
- future phase work,
- post-MVP scope.

---

## 36. Current Milestone

The current system supports the complete progression and dashboard loop:

```text
React action
  → authenticated HTTP request
  → ASP.NET Core controller
  → application service
  → Entity Framework Core
  → PostgreSQL
  → response DTO
  → React refresh
```

A user can:

- create a habit,
- complete it,
- earn deterministic XP,
- increase mapped attributes,
- view overall progression,
- view daily execution,
- view daily or weekly streaks,
- undo the completion,
- retain the original audit history,
- observe the dashboard recalculate immediately.

The next milestone is a complete visual and interaction pass over the existing behavior without changing the backend authority model.
