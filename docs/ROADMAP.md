# Gamified Habit Tracker — Development Roadmap

**Current status:** Phase 6 complete

**Current phase:** Phase 7 — Game UI Polish

**Architecture:** React client, ASP.NET Core API, PostgreSQL database

**Implementation rule:** Backend-owned behavior, auditable persistence, and small vertical slices

---

## 1. Purpose

This roadmap defines the development sequence for the Gamified Habit Tracker.

It records:

- completed phases,
- approved domain decisions,
- current work,
- deferred work,
- phase completion requirements.

The roadmap defines when work belongs.

The architecture documentation defines where responsibilities belong.

The data model defines how persistent state and history are represented.

The API contract defines what crosses the HTTP boundary.

A later phase must not silently reinterpret or redesign behavior completed in an earlier phase.

---

## 2. Roadmap Rules

### 2.1 Phase discipline

Before starting a feature, identify:

1. the current phase,
2. the user-visible behavior,
3. the owning backend service,
4. the affected entities,
5. the request or response DTOs,
6. the frontend consumer,
7. the required tests,
8. the manual verification path.

Work from later phases must not be introduced only because it would look useful in the interface.

### 2.2 Architecture discipline

The following rules remain fixed:

- React displays data and sends user actions.
- ASP.NET Core owns validation and business logic.
- PostgreSQL is the persistent source of truth.
- Entity Framework Core handles database access.
- DTOs define the frontend/backend boundary.
- Database entities are not returned directly as API responses.
- Controllers remain thin.
- Services contain application and domain behavior.
- User identity comes from authenticated backend claims.
- User-owned requests do not accept a client-supplied `userId`.
- XP, attributes, levels, completion dates, configuration history, undo behavior, and streak calculations remain outside React.

### 2.3 Historical integrity

Past events must retain the meaning they had when they occurred.

Later edits must not silently change:

- the configuration used by an earlier completion,
- the XP awarded for an earlier completion,
- the attribute mapping used by an earlier completion,
- the target required during an earlier week,
- the frequency active during an earlier streak period,
- the existence of a completion that was later undone.

History is preserved through effective-dated configuration versions, completion linkage, undo timestamps, and append-only XP transactions.

### 2.4 Contract discipline

When an API response changes, review together:

- backend response DTOs,
- backend service mapping,
- backend unit tests,
- HTTP integration tests,
- frontend TypeScript types,
- frontend API tests,
- component fixtures,
- consuming React components.

### 2.5 Commit discipline

Before committing:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Confirm that:

- every modified file belongs to the change,
- unrelated formatting changes are removed or separated,
- generated build output is not staged,
- migrations are modified only when the database model changes,
- the commit represents one coherent change.

Repository-wide formatting is kept separate from feature work.

---

# Phase 1 — Project Setup

**Status:** Complete

## Goal

Establish the frontend, backend, database, and test foundation.

## Implemented

- Created the ASP.NET Core Web API.
- Created the React, TypeScript, and Vite client.
- Added Tailwind CSS.
- Configured PostgreSQL.
- Added Entity Framework Core.
- Connected the API to PostgreSQL.
- Created the xUnit test project.
- Added a backend health endpoint.
- Confirmed the frontend can call the backend.
- Configured local HTTPS.
- Configured local CORS behavior.
- Added the repository-local .NET tool manifest.

## Definition of Done

- The API runs locally.
- The React application runs locally.
- PostgreSQL runs locally.
- The API connects to PostgreSQL.
- Backend tests execute successfully.
- The browser can call a backend endpoint.

## Excluded From This Phase

- authentication,
- habit management,
- completion,
- XP,
- attributes,
- dashboard,
- streaks.

---

# Phase 2 — Authentication

**Status:** Complete

## Goal

Provide secure registration, login, logout, session restoration, and authenticated ownership.

## Implemented

- Added `User`.
- Added `UserSettings`.
- Created default settings during registration.
- Stored display name.
- Stored an IANA time-zone identifier.
- Added password hashing and verification.
- Added registration.
- Added login.
- Added logout.
- Added current-user restoration.
- Added optional remembered sessions.
- Added encrypted cookie authentication.
- Added antiforgery protection.
- Added backend authorization.
- Added frontend authentication state.
- Added registration and login interfaces.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend tests.
- Added Playwright authentication coverage.
- Added GitHub Actions validation.

## Authentication Decisions

- Identifiers use application-generated Guid version 7 values.
- Email and username comparisons use normalized values.
- Normalized email and username have unique indexes.
- `User` and `UserSettings` are created atomically.
- Time-zone identifiers are validated by the backend.
- Authentication uses an encrypted `HttpOnly` cookie.
- Default sessions are non-persistent and last no longer than 12 hours.
- `rememberMe` permits persistence for no longer than 30 days.
- Sliding expiration is disabled.
- `SameSite=Lax` is used.
- Cookies are secure outside local development.
- `GET /api/auth/me` restores the authenticated user.
- Authenticated identity comes from backend claims.
- Logout is idempotent.
- Expected failures use Problem Details.
- Antiforgery tokens are sent through `X-CSRF-TOKEN`.
- Cached antiforgery tokens are cleared when authentication identity changes.

## Definition of Done

- A new user can register.
- Registration creates default settings.
- Registration is atomic.
- An existing user can log in.
- The user may choose a temporary or remembered session.
- The user can log out.
- Authentication is restored after browser refresh.
- Anonymous users cannot access protected endpoints.
- Passwords are stored only as hashes.
- State-changing browser requests require antiforgery validation.
- Authentication behavior is covered by automated tests.

## Deferred

- email verification,
- password reset,
- external identity providers,
- account recovery workflows.

---

# Phase 3 — Habit CRUD

**Status:** Complete

## Goal

Allow authenticated users to create and manage their own habits.

## Implemented

- Added `Habit`.
- Added controlled category values.
- Added daily and weekly frequency values.
- Added Easy, Medium, Hard, and Elite difficulty values.
- Added Entity Framework Core configuration.
- Added PostgreSQL constraints and indexes.
- Added create, list, retrieve, edit, and deactivate behavior.
- Added `HabitService`.
- Added backend-owned validation and normalization.
- Added ownership protection.
- Added deterministic ordering.
- Added soft deactivation.
- Added active-only listing by default.
- Added optional inactive inclusion.
- Returned `404 Not Found` for missing or foreign-owned habits.
- Added the habit creation form.
- Added the habit editing form.
- Added the habit list.
- Added guarded deactivation.
- Reloaded authoritative data after mutations.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend API tests.
- Added frontend component tests.
- Verified create, edit, and deactivate behavior against PostgreSQL.

## Frequency Rules

Daily habits require:

```text
TargetCount = 1
```

Weekly habits require:

```text
1 <= TargetCount <= 7
```

## Definition of Done

- A user creates a habit.
- A user sees only their own habits.
- A user edits only their own habits.
- A user deactivates only their own habits.
- Foreign-owned habits are not exposed.
- Deactivation preserves historical data.
- Ownership behavior is covered by automated tests.

## Deferred

- completion,
- XP,
- streaks,
- advanced scheduling,
- search and filtering,
- final workspace layout.

---

# Phase 4 — Habit Completion

**Status:** Complete

## Goal

Allow users to complete active habits for the current local date and undo the current active completion.

## Implemented

- Added `HabitCompletion`.
- Added local completion-date storage.
- Added exact UTC completion timestamps.
- Added backend-owned local-date calculation.
- Added `TimeProvider`.
- Used the stored IANA time zone.
- Added `CompletionService`.
- Added ownership checks.
- Added active-habit checks.
- Added duplicate-completion prevention.
- Added optional note normalization.
- Added completion behavior.
- Added undo behavior.
- Added completion and undo endpoints.
- Returned `404 Not Found` for missing or foreign-owned habits.
- Returned conflict responses for invalid completion state.
- Allowed completion undo after later habit deactivation.
- Added `isCompletedToday` to habit responses.
- Added frontend completion and undo state.
- Added pending and error behavior.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend API and component tests.
- Verified completion and undo through the running application and PostgreSQL.

## Initial Completion Decision

The backend determines `CompletedDate` from:

```text
Current UTC timestamp + stored IANA time zone
```

The client cannot submit the completion date.

## Initial Uniqueness Rule

At the end of Phase 4, the intended business rule was one completion per habit and local date.

Phase 6 refined this rule to one **active, non-undone** completion per habit and local date so an undone completion could remain in history without preventing a later replacement completion.

## Definition of Done

- A user completes an active owned habit for the current local date.
- The same active completion cannot be created twice.
- A user can undo today’s completion.
- Completion records are persisted.
- Completion rules remain in the backend.
- Duplicate and undo behavior are covered by tests.

## Deferred

- XP rewards,
- attribute rewards,
- historical configuration linkage,
- auditable undo,
- streak calculation,
- completion-history screen,
- partial completion.

---

# Phase 5 — XP and Attributes

**Status:** Complete

## Goal

Reward habit completion with deterministic XP and persistent attribute progression.

## Implemented

- Added `HabitAttributeReward`.
- Added `UserAttribute`.
- Added `XpTransaction`.
- Added `XpService`.
- Added `AttributeService`.
- Defined the eight canonical attributes.
- Calculated habit rewards from category and difficulty.
- Synchronized reward mappings when applicable.
- Applied XP when a habit was completed.
- Reversed XP when the current completion was undone.
- Stored completion, attributes, and XP through one EF Core unit of work.
- Calculated attribute levels in the backend.
- Calculated progress toward the next attribute level.
- Added overall-level calculation behavior for dashboard use.
- Added `GET /api/attributes`.
- Returned reward details in habit responses.
- Returned applied XP in completion responses.
- Added attribute progress cards.
- Added earned-XP feedback.
- Refreshed attributes after completion and undo.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend tests.
- Verified XP gain, persistence, level progress, refresh, and reversal.

## Canonical Attributes

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

## Difficulty XP

| Difficulty | Total XP |
| ---------- | -------: |
| Easy       |       10 |
| Medium     |       20 |
| Hard       |       30 |
| Elite      |       50 |

## Reward Allocation

Each category maps to:

- one primary attribute,
- one secondary attribute.

Allocation:

| Reward role | Share |
| ----------- | ----: |
| Primary     |   70% |
| Secondary   |   30% |

The backend owns:

- difficulty totals,
- category mappings,
- reward allocation,
- attribute-level calculations,
- overall-level calculations.

## Definition of Done

- Completion creates XP transactions.
- Completion updates the mapped attributes.
- Undo reverses attribute and overall XP.
- Attribute XP is persisted.
- Habit responses expose reward mappings.
- Completion responses expose applied XP.
- The frontend displays attribute progress.
- The frontend displays earned-XP feedback.
- Core reward behavior is covered by automated tests.

## Decisions Refined During Phase 6

Phase 6 changed undo from an event deletion model to an auditable reversal model.

The completed behavior now preserves:

- the original completion,
- the original positive transactions,
- the undo timestamp,
- appended negative transactions.

The Phase 5 reward rules did not change. Only the persistence and reversal history were strengthened.

## Deferred

- milestones,
- achievements,
- multipliers,
- claims,
- rank systems,
- advanced progression analytics.

---

# Phase 6 — Dashboard and Streaks

**Status:** Complete

## Goal

Provide an authenticated dashboard that summarizes current progression and calculates daily and weekly streaks from persisted history.

## Final Implemented Scope

Phase 6 delivered:

- configurable week starts,
- shared calendar-period calculation,
- overall progression on the dashboard,
- today activity,
- daily execution,
- effective-dated habit configuration history,
- scheduled habit rule changes,
- pending configuration responses,
- completion-to-configuration linkage,
- auditable completion undo,
- append-only XP reversals,
- active-completion uniqueness,
- per-habit daily streaks,
- per-habit weekly streaks,
- historical weekly targets,
- frequency-segment streak resets,
- dashboard streak aggregation,
- frontend streak cards,
- dashboard refresh after all relevant habit actions,
- backend unit tests,
- HTTP integration tests,
- frontend API and component tests,
- manual browser validation.

Phase 6 did not add stored streak counters.

Streaks remain derived values.

---

## 6.1 Calendar Preferences

### Implemented

- Added `WeekStartsOn` to `UserSettings`.
- Added controlled week-start values.
- Defaulted new users to Monday.
- Added `CalendarPeriodCalculator`.
- Calculated weekly periods from:
  - the user-local date,
  - the user’s configured first day of the week.

### Decision

Calendar periods are user-specific.

Weekly calculations must not assume Monday when a different setting is stored.

The client does not calculate authoritative week boundaries.

---

## 6.2 Initial Dashboard Progression

### Implemented

Added authenticated dashboard behavior through:

```text
GET /api/dashboard
```

Added `DashboardService`.

The first dashboard contract returned:

### Overall progression

- total XP,
- current overall level,
- XP inside the current level,
- XP needed for the next level.

### Today activity

- current user-local date,
- active completion count today,
- XP earned today.

### Daily execution

- completed active daily habits,
- total active daily habits.

### Decision

The dashboard is an aggregate backend response.

React does not combine unrelated API records to infer progression.

`DashboardService` owns the aggregation.

---

## 6.3 Effective-Dated Habit Configuration

### Reason for the Change

Editing category, frequency, target count, or difficulty could otherwise reinterpret historical completions and streak periods.

That would make earlier XP and streak results depend on current habit settings rather than the rules active when the event occurred.

### Implemented

Added:

- `HabitConfigurationVersion`,
- configuration version numbering,
- effective start dates,
- optional exclusive end dates,
- configuration history on each habit,
- pending configuration responses,
- next-week rule scheduling.

Versioned fields:

- category,
- frequency,
- target count,
- difficulty.

### Effective Period Rule

Configuration periods use half-open intervals:

```text
EffectiveFromDate <= date < EffectiveToDateExclusive
```

The current open-ended version has:

```text
EffectiveToDateExclusive = null
```

### Scheduled Edit Rules

Changes to category, frequency, target count, or difficulty:

- do not take effect immediately,
- begin at the next user-local week boundary,
- remain visible as pending until effective.

Name and description changes remain immediate because they do not affect historical progression interpretation.

Only one pending version is maintained for the next week boundary.

If a pending change is edited again, the same pending version is updated.

If requested rules are changed back to the current effective rules:

- the pending version is removed,
- the current configuration becomes open-ended again.

### Decision Added to the MVP

Effective-dated rule history became part of the MVP because accurate XP, undo, and streak behavior could not be guaranteed without it.

---

## 6.4 Completion-to-Configuration Linkage

### Implemented

Added `HabitConfigurationVersionId` to `HabitCompletion`.

Every new completion records the configuration version effective on its local completion date.

### Purpose

The linkage preserves:

- the category active at completion time,
- the difficulty active at completion time,
- the frequency active at completion time,
- the weekly target active at completion time.

Later edits do not reinterpret earlier completions.

### Decision Added to the MVP

Historical completion meaning is explicit rather than inferred from the current `Habit` row.

---

## 6.5 Auditable Undo

### Previous Behavior

Undo initially removed the active completion and reversed its progression.

That supported the user-facing workflow but did not preserve a complete action history.

### Implemented Phase 6 Behavior

Added `UndoneAtUtc` to `HabitCompletion`.

Undo now:

- keeps the original completion,
- records the undo timestamp,
- keeps the original positive XP transactions,
- appends matching negative XP transactions,
- reverses `UserAttribute` totals,
- recalculates dashboard values,
- recalculates streaks.

### XP Ledger Rule

`XpTransaction` is append-only.

Positive entries represent awarded XP.

Negative entries represent reversals.

Existing transactions are not edited or deleted to represent undo.

### Active Completion Uniqueness

The database uniqueness rule applies only to rows where:

```text
UndoneAtUtc IS NULL
```

A user may therefore:

1. complete a habit,
2. undo it,
3. complete the same habit again on the same local date.

The earlier undone record remains available for audit.

### Decision Added to the MVP

Undo became an auditable reversal rather than a history deletion.

---

## 6.6 Streak Model

### Approved Scope

Streaks are calculated per habit.

The dashboard does not expose an artificial combined streak across unrelated habits.

Each active habit returns:

- `CurrentStreak`,
- `LongestStreak`.

Units depend on the current effective frequency:

- daily habits use days,
- weekly habits use weeks.

### Persistence Decision

Streaks are not stored.

They are derived from:

- configuration history,
- active non-undone completions,
- current user-local date,
- configured week start.

This avoids synchronizing mutable streak counters after edits, undo, or configuration changes.

---

## 6.7 Daily Streak Rules

A day is successful when an active, non-undone completion exists for that local date.

Rules:

- Completing today counts immediately.
- An incomplete current day does not break the streak before that day ends.
- When today is incomplete, the current streak may continue through yesterday.
- A missed past day breaks the current streak.
- The longest streak is the longest consecutive run of successful eligible days.
- Dates before the current frequency segment are ignored.
- Future dates are ignored.
- Undone completions are ignored.

Example:

```text
Monday     completed
Tuesday    completed
Wednesday  incomplete, still current day
```

Current streak on Wednesday:

```text
2 days
```

---

## 6.8 Weekly Streak Rules

A week is successful when its active completion count reaches the target effective for that week.

Rules:

- Week boundaries follow `UserSettings.WeekStartsOn`.
- Reaching the weekly target early counts immediately.
- An incomplete current week does not break the streak before the deadline.
- A failed completed week breaks the current streak.
- Historical weeks use historical target values.
- A later target change does not reinterpret earlier weeks.
- The first partial week is a grace period when unsuccessful.
- A successful first partial week still counts.
- Future completion dates are ignored.
- Undone completions are ignored.

### Historical Target Example

```text
Weeks 1–3 target: 3
Weeks 4+ target:   5
```

Weeks 1–3 continue to require three completions.

The new target applies only from its effective boundary.

---

## 6.9 Frequency Changes

Category and difficulty changes do not reset a streak.

Target changes do not reset a weekly streak series.

A frequency change begins a new streak series:

```text
Daily → Weekly
Weekly → Daily
```

The current streak calculation uses only the current contiguous frequency segment.

Completions from an earlier frequency segment remain stored but do not carry into the new series.

### Decision Added to the MVP

Frequency history affects streak continuity.

Current habit settings alone are not sufficient to calculate a correct streak.

---

## 6.10 `StreakService`

### Implemented

Added `StreakService` with deterministic calculation methods for:

- daily streaks,
- weekly streaks,
- habit streaks across configuration history.

Added supporting value objects for:

- streak results,
- weekly historical targets,
- dashboard streak summaries.

### Responsibilities

`StreakService`:

- identifies the configuration effective on the current date,
- identifies the current contiguous frequency segment,
- filters undone and future completions,
- applies daily or weekly rules,
- returns current and longest values,
- returns the current frequency for presentation.

It does not:

- query another user’s habits,
- calculate ownership,
- write streak state to the database,
- render labels,
- calculate values in React.

---

## 6.11 Dashboard Streak Aggregation

### Implemented

`DashboardService` now:

- loads the user’s time zone,
- loads the user’s week-start preference,
- calculates the current local date,
- calculates overall progression,
- calculates today activity,
- calculates daily execution,
- loads active owned habits,
- loads configuration history,
- loads completion history,
- invokes `StreakService`,
- maps per-habit streak DTOs.

Dashboard streak responses include:

- habit identifier,
- habit name,
- current effective frequency,
- current streak,
- longest streak.

Inactive habits are excluded from the active dashboard response.

Their history remains persisted.

### Daily Execution Correction

Daily execution uses the frequency configuration effective on the current local date.

It does not rely only on a stale original frequency value.

---

## 6.12 Frontend Dashboard Contract

### Implemented

Added frontend types for:

- dashboard streak responses,
- updated dashboard response shape.

Updated:

- dashboard API tests,
- component fixtures,
- habit-section fixtures,
- dashboard rendering tests.

### Decision

The frontend consumes the backend-calculated streak result.

React only chooses presentational labels such as:

```text
1 day
2 days
1 week
3 weeks
```

The numeric streak value remains authoritative backend data.

---

## 6.13 Habit Streak Presentation

### Implemented

Added a dedicated streak presentation component.

The functional dashboard displays:

- habit name,
- effective frequency,
- current streak,
- longest streak,
- daily or weekly units,
- an empty state when there are no active habits.

Added frontend tests for:

- daily units,
- weekly units,
- singular labels,
- plural labels,
- empty state,
- dashboard integration.

---

## 6.14 Dashboard Refresh Behavior

### Issue Found During Manual Preparation

The dashboard refreshed after completion and undo but did not initially refresh after:

- habit creation,
- habit editing,
- habit deactivation.

That would leave streak cards temporarily stale.

### Implemented Fix

The frontend now refreshes dashboard data after:

- habit creation,
- habit editing,
- habit deactivation,
- habit completion,
- completion undo.

Habit-list and dashboard refresh keys remain separate, but relevant mutations update both.

### Known Minor Side Effect

Create, edit, and deactivate also refresh the attribute request because dashboard and attributes currently share the progression refresh key.

This is acceptable in the current structure and avoids a broader refresh-state redesign during Phase 6.

---

## 6.15 Testing and Validation

### Backend Unit Coverage

Added coverage for:

- empty daily history,
- current-day grace,
- missed past days,
- historical longest streak,
- incomplete current week,
- first partial-week grace,
- historical target changes,
- daily-to-weekly-to-daily transitions,
- undone completions,
- dashboard streak mapping,
- current effective frequency.

### HTTP Integration Coverage

Added coverage for:

- authenticated dashboard streak responses,
- DTO serialization,
- dependency injection registration,
- real controller-to-service execution,
- active habit streak data.

### Frontend Coverage

Added coverage for:

- updated dashboard API response,
- streak response types,
- daily and weekly labels,
- empty streak state,
- dashboard integration,
- refresh after habit creation,
- refresh after deactivation.

### Manual Smoke Test

The running application was verified against the real API and PostgreSQL.

The test covered:

1. login,
2. daily habit creation,
3. initial zero streak,
4. daily completion,
5. XP refresh,
6. attribute refresh,
7. current and longest streak update,
8. undo,
9. XP reversal,
10. streak recalculation,
11. completing again,
12. habit rename propagation,
13. weekly habit creation,
14. weekly streak calculation,
15. habit deactivation,
16. streak-card removal,
17. refresh behavior without manually reloading the page.

No unhandled frontend or backend errors were observed.

---

## 6.16 Phase 6 Scope Decisions

The following additions became part of the MVP because they were required for correctness:

- configurable week start,
- effective-dated habit configurations,
- pending next-week rule changes,
- completion-to-configuration linkage,
- auditable undo,
- append-only XP reversal,
- active-completion uniqueness,
- per-habit streaks,
- historical weekly targets,
- frequency-based streak segmentation.

The following items were previously suggested for Phase 6 but were not added to the completed dashboard contract:

- recent completion activity list,
- recent XP transaction list,
- seven-day completion chart,
- weekly XP summary,
- advanced weekly progress visualization.

These features are not required for the completed Phase 6 behavior and remain unimplemented until separately approved.

They must not be presented as current functionality.

---

## Definition of Done

Phase 6 is complete because:

- `DashboardService` aggregates progression data.
- `StreakService` owns streak calculations.
- The dashboard returns overall progression.
- The dashboard returns today activity.
- The dashboard returns daily execution.
- The dashboard returns current and longest streaks for active habits.
- Daily and weekly streak rules are documented and tested.
- Historical configuration affects calculations correctly.
- Weekly targets use effective history.
- Frequency changes start new streak series.
- Undo preserves audit history.
- Undone completions are ignored by active calculations.
- XP reversals are append-only.
- Dashboard values refresh after relevant mutations.
- Backend tests pass.
- HTTP integration tests pass.
- Frontend tests pass.
- Lint passes.
- Production frontend build passes.
- Entity Framework reports no unintended pending model changes after the final streak-only changes.
- The full flow was manually verified.

---

## Not Included

- stored streak counters,
- aggregate cross-habit streak,
- advanced charts,
- predictive analytics,
- AI recommendations,
- notifications,
- reminders,
- social features,
- leaderboards,
- public profiles,
- avatars,
- quests,
- inventory,
- currencies,
- partial completion,
- complex analytics.

---

# Phase 7 — Game UI Polish

**Status:** Current

## Goal

Replace the functional interface with a cohesive application shell and polished presentation without changing backend ownership.

Phase 7 is a presentation and interaction phase.

It must not introduce new progression rules or move existing business logic into React.

## Scope

### Application shell

- desktop-first application shell,
- persistent navigation,
- top-level page structure,
- responsive content region,
- consistent page headers,
- authenticated layout.

### Dashboard presentation

- improve information hierarchy,
- preserve today activity and daily execution,
- improve overall progression presentation,
- improve streak presentation,
- improve attribute presentation,
- preserve loading and error behavior,
- keep primary actions visible.

### Habit workspace

- improve active habit presentation,
- improve creation and editing flows,
- improve pending configuration presentation,
- improve completion and undo feedback,
- improve deactivation confirmation,
- preserve ownership and backend contracts.

### Attribute presentation

- consistent attribute identity,
- clearer level and XP display,
- accessible progress bars,
- responsive card layout.

### Shared interface components

Introduce shared components only where current screens need them.

Possible components include:

- `Panel`,
- `Card`,
- `Button`,
- `IconButton`,
- `Tabs`,
- `Badge`,
- `ProgressBar`,
- `Modal`,
- `ConfirmationDialog`,
- `LoadingSkeleton`,
- `EmptyState`,
- `ErrorPanel`,
- `Toast`.

Do not build a speculative component framework before real usage exists.

### Interaction

- consistent pending states,
- consistent disabled states,
- clear success feedback,
- clear error feedback,
- visible keyboard focus,
- semantic actions,
- restrained transitions,
- reduced-motion support.

### Responsive behavior

The interface must remain usable on:

- ordinary desktop displays,
- laptop widths,
- narrower browser windows.

Phase 7 does not include a native mobile application.

## Architecture Constraints

- React remains responsible for presentation and interaction.
- Backend services remain responsible for business rules.
- Existing DTOs remain authoritative.
- Streaks are not recalculated in React.
- XP is not recalculated in React.
- Configuration scheduling is not reproduced in React.
- Undo behavior remains backend-owned.
- No duplicate source of truth is introduced.

## Definition of Done

- The application has a consistent shell.
- Dashboard, habits, and attributes share a coherent visual system.
- Loading, empty, pending, success, and error states are consistent.
- Daily actions remain easy to find.
- Progression values remain readable.
- Habit streaks remain readable.
- Keyboard focus is visible.
- Color is not the only method of communicating state.
- Reduced-motion preferences are respected where motion exists.
- Layouts remain usable at desktop and laptop widths.
- Existing automated tests continue to pass.
- New interaction behavior has appropriate frontend tests.
- No backend logic is moved into React.

## Not Included

- new progression formulas,
- new streak rules,
- avatar system,
- theme marketplace,
- social interface,
- leaderboard,
- public profile,
- quests,
- inventory,
- currencies,
- cinematic systems,
- copyrighted characters or branding.

---

# Phase 8 — Deployment and Project Polish

**Status:** Planned

## Goal

Deploy the application and complete release-readiness work.

## Scope

### Production hosting

- deploy the frontend,
- deploy the ASP.NET Core API,
- deploy PostgreSQL,
- define the production origin model,
- configure production environment variables,
- protect secrets.

### Database deployment

- apply migrations safely,
- document migration execution,
- verify application startup behavior,
- define backup expectations,
- define recovery expectations.

### Security verification

- verify production cookie settings,
- verify `Secure`,
- verify `HttpOnly`,
- verify `SameSite`,
- verify antiforgery behavior,
- verify CORS when separate origins are used,
- verify no secrets are committed.

### Operations

- review application logging,
- review exception handling,
- verify health checks,
- verify production startup,
- verify failure behavior,
- document required services.

### Documentation

- review all setup instructions,
- document environment variables,
- document production configuration,
- add accurate screenshots,
- confirm all documented features exist,
- remove stale implementation notes,
- prepare release notes.

## Definition of Done

- The application runs locally from documented instructions.
- The application is deployed.
- Production authentication works correctly.
- Antiforgery protection works in the deployed environment.
- Database migrations are repeatable.
- Deployment secrets are not exposed.
- Required environment variables are documented.
- Health behavior is verified.
- Screenshots show the real application.
- Documentation matches the deployed behavior.
- Final automated validation passes.

## Not Included

- unplanned product features,
- architecture rewrites,
- framework replacements,
- native mobile application,
- social-platform expansion,
- payment system,
- administration panel.

---

# Post-MVP Backlog

The following work is intentionally deferred until the current application is stable and deployed.

## Progression and Achievement

- `Milestone`,
- `UserMilestone`,
- achievements,
- milestone badges,
- advanced streak badges,
- level-up events,
- titles,
- ranks,
- claimable rewards.

## Personal Identity

- avatars,
- character portraits,
- profile customization,
- cosmetic themes,
- public profiles.

## Expanded Game Systems

- quests,
- inventories,
- currencies,
- collectibles,
- challenge systems.

## Social Systems

- friends,
- leaderboards,
- social comparison,
- shared challenges,
- accountability groups.

## Engagement Systems

- reminders,
- notifications,
- scheduled nudges,
- activity center.

## Extended Tracking

- advanced scheduling,
- sleep tracking,
- bad-habit tracking,
- journal entries,
- partial completion,
- complex analytics,
- AI recommendations.

## Platform Expansion

- native mobile application,
- public API,
- external integrations,
- calendar integration,
- payments,
- administration panel.

Every post-MVP feature requires:

1. a defined user problem,
2. an approved product decision,
3. clear backend ownership,
4. a documented data model,
5. an API contract,
6. test coverage,
7. a separate implementation phase or milestone.

---

# Phase Transition Checklist

A phase is not complete until the applicable requirements are satisfied.

## Behavior

- The user-visible flow works through the real frontend.
- Backend services own the rules.
- PostgreSQL stores authoritative state.
- Ownership boundaries are enforced.
- Undo or reversal behavior is verified where applicable.
- Historical behavior is preserved where applicable.

## Automated Validation

- focused backend unit tests pass,
- HTTP integration tests pass,
- frontend tests pass,
- lint passes,
- formatting checks pass,
- production builds pass,
- browser coverage is updated when critical journeys change,
- CI passes on the final implementation.

## Manual Validation

- the feature is tested through the browser,
- refresh behavior is verified,
- loading and error states are checked,
- local-date behavior is checked when relevant,
- another user’s data remains protected,
- database effects are understood,
- application logs contain no unexpected failures.

## Documentation

- `README.md` reflects current capabilities,
- `PROJECT_OVERVIEW.md` reflects current decisions,
- `ROADMAP.md` reflects phase status,
- `ARCHITECTURE.md` reflects responsibility and flow,
- `DATA_MODEL.md` reflects persistence and history,
- `API_CONTRACT.md` reflects actual endpoints and responses,
- `NAMING_CONVENTIONS.md` reflects canonical names.

## Git Hygiene

- the working tree is inspected,
- unrelated changes are removed or separated,
- intended files are staged explicitly,
- staged filenames are reviewed,
- the commit message describes one coherent change,
- the branch is pushed,
- local and remote branches are synchronized.

---

# Final Rule

The roadmap defines sequence.

The architecture defines responsibility.

The data model preserves state and history.

The API exposes backend-owned behavior.

PostgreSQL stores the truth.

ASP.NET Core calculates and validates the system.

React presents and interacts with the system.

Later phases may improve presentation and deployment without weakening those boundaries.
