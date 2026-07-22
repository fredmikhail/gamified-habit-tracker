# Gamified Habit Tracker — Architecture

**Status:** Current through Phase 6

**Current phase:** Phase 7 — Game UI Polish

**Architecture style:** Separate React client and ASP.NET Core Web API with PostgreSQL persistence

**Primary rule:** ASP.NET Core owns application behavior; React presents authoritative API data

---

## 1. Purpose

This document defines the current system architecture and the responsibility boundaries that must remain stable as the application evolves.

It describes:

- runtime topology,
- frontend and backend ownership,
- persistence rules,
- authentication and antiforgery behavior,
- habit configuration history,
- completion and undo workflows,
- XP and attribute progression,
- dashboard aggregation,
- streak calculation,
- testing boundaries,
- Phase 7 constraints.

The architecture may change through a deliberate decision. It must not change indirectly as part of unrelated feature work.

---

## 2. Technology Stack

### Frontend

- React
- TypeScript
- Vite
- Tailwind CSS
- Vitest
- React Testing Library
- Playwright
- Prettier
- Oxlint

### Backend

- C#
- ASP.NET Core Web API
- Entity Framework Core
- xUnit

### Database

- PostgreSQL

### Automation

- GitHub Actions
- repository-local `dotnet-ef` tool manifest

A framework, library, database, or hosting-model change requires a documented reason and an explicit decision.

---

## 3. System Topology

```text
Browser
   |
   | React interface
   | typed HTTP / JSON requests
   v
Vite development server
or production web origin
   |
   | /api proxy or reverse proxy
   v
ASP.NET Core Web API
   |
   | controllers
   v
Application services
   |
   | Entity Framework Core
   v
PostgreSQL
```

The frontend and backend are separate applications.

The frontend communicates with the backend through HTTP.

The backend owns:

- authentication,
- authorization,
- antiforgery enforcement,
- ownership,
- validation,
- calendar interpretation,
- completion rules,
- habit configuration history,
- XP rules,
- attribute rules,
- level formulas,
- undo behavior,
- dashboard aggregation,
- streak calculation.

PostgreSQL stores persistent application truth.

React renders returned state and sends user actions. It does not recreate backend domain rules.

---

## 4. Browser Origin Model

### Local development

```text
Browser
   |
   | http://localhost:5173
   v
Vite
   |
   | proxies /api
   v
https://localhost:7287
```

Current local endpoints:

- frontend: `http://localhost:5173`
- backend HTTPS: `https://localhost:7287`
- backend HTTP: `http://localhost:5167`

Vite proxies `/api` to the HTTPS backend.

The proxy:

- preserves the frontend/backend application boundary,
- provides a same-origin browser path during development,
- accepts the local ASP.NET Core development certificate.

The frontend API client includes browser credentials.

The backend also permits the Vite development origin through its development CORS policy, but the Vite proxy is the normal local browser path.

### Production preference

The preferred production topology is:

```text
Public application origin
├── React application
└── /api -> ASP.NET Core API
```

A shared public origin simplifies:

- authentication cookies,
- antiforgery behavior,
- browser credentials,
- CORS configuration.

If Phase 8 deploys the frontend and API on separate origins, production configuration must explicitly define:

- allowed origins,
- credentialed CORS,
- HTTPS,
- cookie `Secure` behavior,
- `SameSite` behavior,
- antiforgery behavior,
- secret management.

---

## 5. Repository Structure

```text
gamified-habit-tracker/
├── .config/
│   └── dotnet-tools.json
├── .github/
│   └── workflows/
├── client/
│   ├── e2e/
│   ├── src/
│   │   ├── api/
│   │   ├── auth/
│   │   ├── components/
│   │   │   ├── attributes/
│   │   │   ├── auth/
│   │   │   ├── dashboard/
│   │   │   └── habits/
│   │   ├── test/
│   │   ├── types/
│   │   ├── App.tsx
│   │   └── main.tsx
│   ├── playwright.config.ts
│   └── vite.config.ts
├── docs/
├── server/
│   ├── HabitTracker.Api/
│   │   ├── Controllers/
│   │   ├── Data/
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   ├── Enums/
│   │   │   └── ValueObjects/
│   │   ├── DTOs/
│   │   ├── ExceptionHandling/
│   │   ├── Exceptions/
│   │   ├── Services/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── HabitTracker.Tests/
│   └── HabitTracker.IntegrationTests/
└── GamifiedHabitTracker.sln
```

Folders are added when implemented behavior requires them.

The repository does not maintain empty layers or speculative feature folders.

---

## 6. Responsibility Boundaries

### 6.1 Frontend responsibilities

The frontend owns:

- page and component layout,
- form state,
- local interaction state,
- loading state,
- pending state,
- error presentation,
- success presentation,
- confirmation dialogs,
- typed API calls,
- accessible interaction feedback,
- display formatting,
- CSS progress widths calculated from authoritative values,
- singular and plural display labels.

Examples of valid frontend calculations:

```text
xpIntoCurrentLevel / xpNeededForNextLevel
completedDailyHabits / totalDailyHabits
1 day vs 2 days
1 week vs 3 weeks
```

These are presentational calculations.

The frontend does not own:

- password validation authority,
- authentication credentials,
- authorization,
- ownership checks,
- completion eligibility,
- completion dates,
- effective habit configuration,
- weekly boundaries,
- XP totals,
- reward allocation,
- attribute mappings,
- level curves,
- undo reversals,
- streak continuity,
- dashboard aggregation.

### 6.2 Backend responsibilities

The backend owns:

- HTTP endpoints,
- authentication,
- authorization,
- antiforgery validation,
- resource ownership,
- request validation,
- domain validation,
- local-date calculation,
- week-period calculation,
- habit rule scheduling,
- configuration-history interpretation,
- completion creation,
- completion undo,
- XP transactions,
- attribute progression,
- level calculation,
- dashboard aggregation,
- streak calculation,
- entity-to-DTO mapping,
- status-code selection,
- Problem Details responses.

### 6.3 Database responsibilities

PostgreSQL stores persistent state and protects invariants that should remain valid under concurrent requests.

Persistent concepts include:

- users,
- user settings,
- habits,
- habit configuration versions,
- habit completions,
- habit attribute rewards,
- user attributes,
- XP transactions,
- EF Core migration history.

Database constraints and indexes protect rules such as:

- normalized email uniqueness,
- normalized username uniqueness,
- one settings row per user,
- relationship integrity,
- controlled target ranges,
- one active completion per habit and local date.

Service validation provides usable application errors.

Database constraints remain the final protection against invalid concurrent writes.

---

## 7. Backend Layering

```text
HTTP request
   |
   v
Controller
   |
   v
Application service
   |
   v
AppDbContext / EF Core
   |
   v
PostgreSQL
```

Supporting calculation services and value objects may be called by application services.

The API does not expose the EF Core entity model directly.

---

## 8. Controllers

Controllers define the HTTP surface.

A controller normally:

1. receives route values and a request DTO,
2. relies on ASP.NET Core request validation,
3. resolves the authenticated user from claims,
4. calls the owning service,
5. returns the appropriate response DTO and status code.

Controllers do not:

- query `AppDbContext` directly,
- calculate XP,
- calculate attributes,
- calculate levels,
- select habit configurations,
- calculate streaks,
- aggregate dashboard sections,
- enforce resource ownership independently of services.

Authentication session creation and removal remain controller responsibilities because they are HTTP concerns.

---

## 9. Services

Canonical application services:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

All are implemented.

| Service             | Primary responsibility                                                      |
| ------------------- | --------------------------------------------------------------------------- |
| `AuthService`       | Registration, credential verification, account lookup, current-user mapping |
| `HabitService`      | Habit creation, retrieval, editing, deactivation, rule scheduling           |
| `CompletionService` | Completion and undo orchestration                                           |
| `XpService`         | Reward and level calculations                                               |
| `AttributeService`  | Attribute reads and XP persistence                                          |
| `StreakService`     | Daily and weekly streak calculation                                         |
| `DashboardService`  | Aggregated authenticated dashboard response                                 |

A new service is introduced only when it has a distinct application responsibility.

A screen, card, or visual component does not automatically require a new backend service.

---

## 10. Data Layer

The `Data` area contains:

- `AppDbContext`,
- entity configurations,
- EF Core migrations.

`AppDbContext` exposes persistent sets for:

- `User`
- `UserSettings`
- `Habit`
- `HabitConfigurationVersion`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

Entity configurations define:

- required values,
- maximum lengths,
- relationships,
- delete behavior,
- indexes,
- unique indexes,
- partial indexes,
- check constraints,
- generated identifiers.

The Npgsql provider uses snake-case naming conventions.

Physical PostgreSQL identifiers use `snake_case`.

Schema changes are applied through EF Core migrations.

Generated migrations are treated as historical schema artifacts. They are not reformatted or edited during unrelated feature work.

---

## 11. Domain Layer

The `Domain` area contains:

- entities,
- enums,
- value objects.

### Implemented entities

- `User`
- `UserSettings`
- `Habit`
- `HabitConfigurationVersion`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

### Reserved entities

- `Milestone`
- `UserMilestone`

The reserved entities are post-MVP names and are not currently implemented.

### Implemented enums

Domain enums include:

- `HabitCategory`
- `HabitDifficulty`
- `HabitFrequencyType`
- `AttributeType`
- `WeekStartDay`

### Calculation value objects

Value objects include calculated data that does not require persistence.

Current examples include:

- `LevelProgress`
- `WeekPeriod`
- `HabitStreakResult`
- `HabitStreakSummary`
- `WeeklyStreakTarget`

Streak and level value objects represent calculated results. They are not database tables.

---

## 12. DTO Layer

DTOs define the external API contract.

They prevent:

- password hashes from being exposed,
- navigation properties from leaking,
- database entities from becoming public contracts,
- clients from setting backend-owned values,
- database changes from automatically becoming API changes.

### Authentication DTOs

- `RegisterRequest`
- `LoginRequest`
- `AntiforgeryTokenResponse`
- `AuthResponse`
- `CurrentUserResponse`

### Habit DTOs

- `CreateHabitRequest`
- `UpdateHabitRequest`
- `HabitResponse`
- `HabitAttributeRewardResponse`
- `PendingHabitConfigurationResponse`

### Completion DTOs

- `CompleteHabitRequest`
- `HabitCompletionResponse`
- `CompleteHabitResponse`

### Attribute DTOs

- `UserAttributeResponse`

### Dashboard DTOs

- `DashboardResponse`
- `OverallProgressResponse`
- `TodayActivityResponse`
- `TodayExecutionResponse`
- `HabitStreakResponse`

The dashboard response is intentionally focused.

It currently does not expose:

- recent activity lists,
- XP transaction history,
- seven-day charts,
- advanced analytics,
- weekly review summaries.

Those fields are not added until corresponding behavior is approved and implemented.

---

## 13. Startup and Dependency Registration

`Program.cs` configures:

- controllers,
- JSON enum serialization,
- Problem Details,
- exception handling,
- antiforgery,
- cookie authentication,
- authorization,
- `TimeProvider.System`,
- application services,
- password hashing,
- CORS,
- OpenAPI in Development,
- PostgreSQL through Entity Framework Core.

Application services are registered with scoped lifetimes.

Current registrations include:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

JSON enums are serialized as camel-case strings.

Integer enum values are rejected from JSON contracts.

During normal startup:

1. ASP.NET Core builds the application.
2. The application creates a service scope.
3. `AppDbContext` is resolved.
4. the configured connection string is required,
5. database connectivity is checked,
6. startup fails when PostgreSQL cannot be reached.

This provides fail-fast behavior outside tests.

Integration tests replace the production database registration before the application context is resolved.

---

## 14. Authentication Architecture

The browser uses ASP.NET Core encrypted cookie authentication.

The frontend does not:

- receive an access token,
- store an access token,
- decode credentials,
- manually attach bearer tokens.

```text
Registration or login
   |
   v
AuthController
   |
   v
AuthService
   |
   v
PostgreSQL
   |
   v
ASP.NET Core creates authentication cookie
   |
   v
Browser sends cookie automatically
```

For later protected requests:

```text
Browser request
   |
   | credentials included
   v
Authentication middleware
   |
   | authenticated ClaimsPrincipal
   v
Controller
   |
   | user ID from ClaimTypes.NameIdentifier
   v
Application service
```

### Authentication cookie

Current behavior:

- cookie name: `HabitTracker.Auth`
- encrypted by ASP.NET Core
- `HttpOnly`
- `SameSite=Lax`
- secure outside Development
- maximum 12-hour session by default
- fixed 30-day persistent session when `rememberMe` is selected
- sliding expiration disabled

Unauthorized API requests return `401`.

Forbidden API requests return `403`.

API authentication failures do not redirect to an HTML login page.

---

## 15. Antiforgery Architecture

Cookie-authenticated browsers automatically attach credentials, so state-changing requests require antiforgery protection.

Current behavior:

- antiforgery request header: `X-CSRF-TOKEN`
- antiforgery cookie: `HabitTracker.Antiforgery`
- `HttpOnly`
- `SameSite=Lax`
- secure outside Development
- automatic validation on state-changing controller actions.

The frontend API layer:

1. requests a token from `GET /api/auth/csrf-token`,
2. caches the request token in memory,
3. attaches the token to POST, PUT, PATCH, and DELETE calls,
4. includes browser credentials,
5. clears the cached token when authentication identity changes,
6. obtains another token when needed.

Antiforgery behavior is centralized in `apiClient.ts`.

Feature components do not manage antiforgery tokens.

---

## 16. Ownership Architecture

Authenticated identity is derived from:

```text
ClaimTypes.NameIdentifier
```

User-owned API requests do not accept a client-provided `userId`.

Controllers pass the authenticated identifier to services.

Services include the identifier in owned database queries.

For owned resources, missing and foreign-owned identifiers normally produce the same result:

```text
404 Not Found
```

This avoids revealing whether another user’s resource exists.

Ownership filtering applies to:

- habits,
- completions,
- attributes,
- dashboard data,
- configuration history reached through owned habits.

`StreakService` does not perform ownership queries itself. It receives already-owned configuration and completion data from `DashboardService`.

---

## 17. User Calendar Architecture

`UserSettings` stores calendar behavior required by completion and streak rules.

Current calendar settings:

- IANA time-zone identifier,
- preferred first day of the week.

The default week start is Monday.

### Local dates

The backend calculates the current user-local date from:

```text
Current UTC timestamp
+ stored IANA time zone
= user-local DateOnly
```

`LocalDateCalculator` owns this conversion.

The client cannot choose the completion date.

### Week periods

`CalendarPeriodCalculator` calculates a `WeekPeriod` from:

- a local date,
- `WeekStartDay`.

A week period contains:

- inclusive start date,
- inclusive end date.

The same calculation is reused for:

- pending habit rule boundaries,
- weekly streak periods,
- dashboard behavior that depends on the user’s week.

React does not calculate authoritative week boundaries.

---

## 18. Habit Architecture

```text
React habit component
   |
   | typed request through habitsApi
   v
HabitsController
   |
   | authenticated user ID
   v
HabitService
   |
   | owned EF Core query
   | validation
   | configuration selection or scheduling
   v
PostgreSQL
```

`HabitService` owns:

- trimming,
- blank-to-null normalization,
- category validation,
- frequency validation,
- target validation,
- difficulty validation,
- ownership filtering,
- active and inactive listing,
- deterministic ordering,
- creation,
- retrieval,
- editing,
- soft deactivation,
- automatic reward synchronization,
- configuration-version management,
- response DTO mapping.

### Target rules

Daily:

```text
TargetCount = 1
```

Weekly:

```text
1 <= TargetCount <= 7
```

The backend is authoritative even when the frontend applies matching input constraints.

### Soft deactivation

Deactivation sets the habit inactive without deleting:

- the habit,
- configuration history,
- completion history,
- XP history.

Inactive habits are excluded from the normal active list and active dashboard streak response.

---

## 19. Habit Configuration Architecture

`HabitConfigurationVersion` preserves rule history.

Versioned fields:

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

### Effective-period model

Configuration periods use half-open intervals:

```text
EffectiveFromDate <= date < EffectiveToDateExclusive
```

The current open-ended version has:

```text
EffectiveToDateExclusive = null
```

This model avoids overlapping inclusive end dates and makes configuration selection deterministic.

### Initial configuration

Creating a habit also creates configuration version 1.

Its effective date is the current user-local date.

### Immediate fields

The following fields take effect immediately:

- name,
- description.

They do not affect reward or streak history.

### Scheduled rule fields

The following fields are scheduled:

- category,
- frequency,
- target count,
- difficulty.

A changed rule set becomes effective at the next user-local week boundary.

Until then:

- the current version remains effective,
- the next version is returned as pending,
- completions continue using the current version,
- streak calculations continue using the current version.

### Pending-version behavior

Only one pending version is maintained for the next boundary.

When rules are edited again before that date:

- the pending version is updated.

When the requested rules are changed back to the current effective rules:

- the pending version is removed,
- the current version becomes open-ended again.

### Historical authority

For date-sensitive behavior, configuration history is authoritative.

Current mutable values must not be used to reinterpret a historical date.

---

## 20. Automatic Habit Reward Architecture

A habit category and difficulty determine two automatic attribute rewards.

Each category maps to:

- one primary attribute,
- one secondary attribute.

Difficulty determines the total reward.

`XpService` calculates:

- total XP,
- attribute mapping,
- 70/30 allocation.

`HabitService` synchronizes persisted `HabitAttributeReward` rows when required.

Clients do not submit reward allocations.

Habit responses expose reward information for presentation.

The frontend does not determine which attributes receive XP.

---

## 21. Completion Architecture

```text
User selects Complete
   |
   v
HabitCompletionButton
   |
   | POST /api/habits/{habitId}/completions
   v
CompletionsController
   |
   | authenticated user ID
   v
CompletionService
   |
   | owned habit lookup
   | active-state validation
   | local-date calculation
   | effective configuration selection
   | duplicate check
   | reward application
   v
AppDbContext.SaveChangesAsync
   |
   v
PostgreSQL
   |
   v
CompleteHabitResponse
   |
   v
React updates presentation
```

`CompletionService` owns:

- owned habit lookup,
- active-state validation,
- user settings lookup,
- local-date calculation,
- effective configuration selection,
- active-completion duplicate detection,
- note normalization,
- completion creation,
- configuration linkage,
- XP application,
- attribute application,
- undo orchestration,
- response mapping.

`CompletionService` uses `TimeProvider`.

The client does not submit `CompletedDate`.

---

## 22. Completion-to-Configuration Linkage

Each completion stores:

```text
HabitConfigurationVersionId
```

The selected version is the configuration effective on the completion’s local date.

This preserves:

- the category active at completion time,
- the difficulty active at completion time,
- the frequency active at completion time,
- the weekly target active at completion time.

Later habit edits do not change the meaning of existing completions.

The linkage is used by:

- XP application,
- undo,
- historical interpretation,
- streak segmentation,
- weekly target lookup.

---

## 23. Active Completion Uniqueness

The business rule is:

```text
One active completion per HabitId + CompletedDate
```

An active completion is one where:

```text
UndoneAtUtc IS NULL
```

The service checks for an active duplicate before inserting.

PostgreSQL also enforces the rule through a partial unique index.

The database index protects against concurrent requests that pass the service-level pre-check.

An undone completion remains in history and does not prevent a later replacement completion for the same habit and date.

---

## 24. Auditable Undo Architecture

Undo is a reversal, not deletion.

```text
User selects Undo
   |
   | DELETE /api/habits/{habitId}/completions/today
   v
CompletionsController
   |
   | authenticated user ID
   v
CompletionService
   |
   | finds active owned completion for today
   | records UndoneAtUtc
   | reverses attribute totals
   | appends negative XP transactions
   v
AppDbContext.SaveChangesAsync
   |
   v
PostgreSQL
```

Undo preserves:

- the original `HabitCompletion`,
- the original completion timestamp,
- the original configuration linkage,
- the original positive XP transactions.

Undo adds:

- `UndoneAtUtc`,
- matching negative XP transactions.

Undo updates:

- `UserAttribute.CurrentXp`.

The original completion and transactions are not deleted.

### Append-only XP ledger

`XpTransaction` is append-only for completion and undo history.

Positive transaction:

```text
XP awarded
```

Negative transaction:

```text
XP reversal
```

The full history therefore records both the original event and its reversal.

### Active calculations

Undone completions are excluded from:

- `isCompletedToday`,
- today completion counts,
- daily execution,
- daily streaks,
- weekly streaks.

---

## 25. Transaction and Save Behavior

Completion changes are staged in one `AppDbContext`:

- new `HabitCompletion`,
- `UserAttribute` updates,
- positive `XpTransaction` rows.

Undo changes are staged in one `AppDbContext`:

- `HabitCompletion.UndoneAtUtc`,
- `UserAttribute` reductions,
- negative `XpTransaction` rows.

Each workflow uses one `SaveChangesAsync` operation.

With PostgreSQL, EF Core executes the save as one relational transaction.

The completion or undo either persists as a consistent unit or fails as a unit.

The frontend does not treat a mutation as successful until the backend confirms it.

---

## 26. XP Architecture

`XpService` is a calculation service.

It does not query PostgreSQL.

It owns:

- XP totals by difficulty,
- category-to-attribute mappings,
- the primary and secondary split,
- attribute-level calculations,
- overall-level calculations.

Difficulty rewards:

| Difficulty |  XP |
| ---------- | --: |
| Easy       |  10 |
| Medium     |  20 |
| Hard       |  30 |
| Elite      |  50 |

Reward allocation:

| Role                | Share |
| ------------------- | ----: |
| Primary attribute   |   70% |
| Secondary attribute |   30% |

### Attribute level curve

- Level 1 to 2 requires 100 XP.
- Each following level requires 25 XP more than the previous level.

### Overall level curve

- Level 1 to 2 requires 200 XP.
- Each following level requires 50 XP more than the previous level.

Levels are calculated values.

They are not stored as mutable level columns.

---

## 27. Attribute Architecture

`AttributeService` owns:

- returning the authenticated user’s attribute state,
- creating missing attribute rows when XP is first awarded,
- applying attribute XP,
- reversing attribute XP,
- mapping attribute response DTOs.

The eight attributes are:

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

`GET /api/attributes` returns all supported attributes in stable enum order.

An attribute without a stored row is returned as:

- 0 total XP,
- Level 1,
- 0 XP within the current level,
- 100 XP required for the next level.

React may calculate a progress-bar width from these returned values.

React does not reproduce the level curve.

---

## 28. Streak Architecture

Streaks are derived values.

There is no streak table and no mutable streak counter.

```text
DashboardService
   |
   | owned active habits
   | configuration histories
   | completion histories
   | local date
   | configured week start
   v
StreakService
   |
   v
HabitStreakSummary
```

`StreakService` is a deterministic calculation service.

It does not:

- access `AppDbContext`,
- perform ownership checks,
- write database state,
- know about React,
- render display labels.

It receives already-owned domain data from `DashboardService`.

### Inputs

Streak calculation uses:

- current user-local date,
- configured first day of the week,
- effective-dated habit configuration history,
- habit completion history.

### Filtered data

The calculation ignores:

- undone completions,
- future completions,
- completions outside the current frequency segment.

### Result

Each active habit returns:

- current effective frequency,
- current streak,
- longest streak.

Daily results use days.

Weekly results use weeks.

---

## 29. Daily Streak Architecture

A day succeeds when an active, non-undone completion exists on that local date.

### Current streak

The current calculation begins from:

- today, when today is complete,
- yesterday, when today is incomplete.

This prevents an unfinished current day from breaking an existing streak prematurely.

A missed past day breaks the current streak.

### Longest streak

The longest streak is calculated from consecutive successful eligible dates.

### Frequency boundary

Only completions within the current contiguous daily-frequency segment are considered.

A previous weekly segment does not contribute to a new daily streak.

---

## 30. Weekly Streak Architecture

A week succeeds when its active completion count reaches the target effective during that week.

`CalendarPeriodCalculator` determines week boundaries.

Historical targets are represented for calculation through `WeeklyStreakTarget`.

### Current week

Reaching the target early counts immediately.

Failing to reach the target during an unfinished current week does not break the current streak.

### Completed weeks

A failed completed week resets the running streak.

### First partial week

When a habit or weekly frequency segment begins during a week:

- an unsuccessful partial week is ignored as a grace period,
- a successful partial week counts.

### Historical targets

Each week uses the target from the configuration effective during that period.

A later target change does not reinterpret earlier weeks.

---

## 31. Streak Continuity Across Configuration Changes

The current frequency segment is the contiguous sequence of effective configurations that share the same frequency.

### Changes that do not reset the streak

- category,
- difficulty,
- weekly target.

Weekly target changes preserve continuity while each historical week continues to use its own effective target.

### Changes that start a new streak series

```text
Daily -> Weekly
Weekly -> Daily
```

A frequency change creates a new frequency segment.

Earlier completions remain persisted but do not carry into the current streak series.

### Undo

Because streaks are derived, undo recalculates them immediately.

Undoing a historical completion may reduce:

- current streak,
- longest streak.

No stored streak row needs repair.

### Deactivation

Inactive habits are excluded from the active dashboard streak response.

Their configuration and completion history remains persisted.

---

## 32. Dashboard Architecture

```text
OverallProgressSection
   |
   | GET /api/dashboard
   v
DashboardController
   |
   | authenticated user ID
   v
DashboardService
   |
   | settings
   | XP transactions
   | active completions
   | active habits
   | configuration histories
   | completion histories
   | StreakService
   v
DashboardResponse
```

`DashboardController` remains thin.

It:

1. resolves the authenticated user,
2. calls `DashboardService`,
3. returns `DashboardResponse`.

It does not issue separate dashboard queries or calculate metrics.

### `DashboardService` responsibilities

`DashboardService`:

- loads the user’s time zone,
- loads the user’s week-start preference,
- calculates the current local date,
- derives total XP,
- calculates overall-level progress,
- loads active completions for today,
- calculates XP earned today,
- loads active owned habits,
- loads configuration history,
- loads completion history,
- invokes `StreakService`,
- maps the aggregated response.

### Overall progression

Overall total XP is derived from signed `XpTransaction.Amount` values.

Positive and negative transactions therefore contribute naturally to the current total.

`XpService` converts the total into:

- current level,
- XP within the current level,
- XP required for the next level.

No duplicate mutable `TotalXp` column is used.

### Today activity

Today activity contains:

- user-local date,
- active completion count,
- XP earned by those active completions.

Undone completions are excluded.

### Daily execution

Daily execution contains:

- completed active daily habits,
- total active daily habits.

Daily classification uses the configuration effective on the current local date.

It does not rely only on an original or stale habit frequency value.

### Habit streaks

Each active habit maps to:

- habit identifier,
- habit name,
- current effective frequency,
- current streak,
- longest streak.

The dashboard does not return a combined cross-habit streak.

---

## 33. Frontend API Architecture

The `client/src/api` folder contains feature-specific API modules.

Current modules include:

- `apiClient.ts`
- `authApi.ts`
- `habitsApi.ts`
- `attributesApi.ts`
- `dashboardApi.ts`
- `healthApi.ts`
- `readApiError.ts`

`apiClient.ts` centralizes:

- browser credentials,
- state-changing method detection,
- antiforgery token acquisition,
- antiforgery headers.

Feature API modules own:

- endpoint paths,
- request serialization,
- response parsing,
- feature-specific fallback errors.

React components do not call `fetch` directly when a feature API module exists.

---

## 34. Frontend Authentication State

The authentication area contains:

- `AuthContext`
- `AuthProvider`
- `useAuth`

`AuthProvider` owns:

- current authenticated user state,
- startup session restoration,
- registration transitions,
- login transitions,
- logout transitions,
- authentication loading state,
- authentication error state.

It does not own:

- password policy,
- credential verification,
- cookie contents,
- ownership checks.

---

## 35. Frontend Habit Coordination

Current habit components include:

- `HabitSection`
- `HabitForm`
- `HabitList`
- `HabitEditForm`
- `HabitCompletionButton`
- `HabitDeactivateButton`

`HabitSection` coordinates refresh signals.

Current refresh behavior:

| Action           |        Habit list | Dashboard |                             Attributes |
| ---------------- | ----------------: | --------: | -------------------------------------: |
| Create habit     |           Refresh |   Refresh | Refresh through shared progression key |
| Edit habit       |           Refresh |   Refresh | Refresh through shared progression key |
| Deactivate habit |           Refresh |   Refresh | Refresh through shared progression key |
| Complete habit   | Local habit state |   Refresh |                                Refresh |
| Undo completion  | Local habit state |   Refresh |                                Refresh |

The dashboard and attribute sections currently share the progression refresh signal.

This causes a harmless extra attribute request after create, edit, and deactivate.

A broader refresh-state system is not introduced during Phase 6 because the current behavior is correct and small.

---

## 36. Frontend Dashboard Architecture

Current dashboard components include:

- `OverallProgressSection`
- `HabitStreakSection`

`OverallProgressSection` owns:

- loading `GET /api/dashboard`,
- loading state,
- API error state,
- overall-level presentation,
- today activity presentation,
- daily execution presentation,
- passing streak data to `HabitStreakSection`.

`HabitStreakSection` is presentational.

It owns:

- active streak card layout,
- daily and weekly unit labels,
- singular and plural labels,
- empty-state presentation.

It does not calculate:

- current streak,
- longest streak,
- frequency segments,
- target history,
- week boundaries.

---

## 37. TypeScript Contracts

The frontend `types` folder mirrors API response concepts.

Relevant dashboard types include:

- `DashboardResponse`
- `HabitStreakResponse`
- `OverallProgressResponse`
- `TodayActivityResponse`
- `TodayExecutionResponse`

Relevant habit types include:

- `HabitResponse`
- `PendingHabitConfigurationResponse`
- `HabitFrequencyType`
- `HabitCategory`
- `HabitDifficulty`

When a backend response changes, the matching TypeScript type, API tests, fixtures, and consuming components are updated in the same vertical slice.

---

## 38. Error Handling

Expected application failures use specific exceptions.

`ApiExceptionHandler` maps known failures into Problem Details responses.

Examples include:

- invalid credentials,
- duplicate registration values,
- invalid time zones,
- invalid habit names,
- invalid target counts,
- inactive habit completion,
- duplicate active completion,
- invalid completion state.

Expected failures receive intentional status codes and messages.

Unexpected exceptions are not disguised as expected errors.

They continue through ASP.NET Core exception handling and should be logged and investigated.

---

## 39. Testing Architecture

Different test layers protect different risks.

### 39.1 Backend unit tests

Project:

```text
HabitTracker.Tests
```

Backend unit tests use isolated contexts and focused service construction.

They protect:

- authentication rules,
- habit validation,
- configuration scheduling,
- effective-period behavior,
- completion rules,
- active-completion uniqueness,
- auditable undo,
- XP calculations,
- level calculations,
- attribute application,
- attribute reversal,
- dashboard aggregation,
- daily streak rules,
- weekly streak rules,
- target-history rules,
- frequency transitions,
- undone-completion filtering.

### 39.2 HTTP integration tests

Project:

```text
HabitTracker.IntegrationTests
```

Infrastructure:

- `WebApplicationFactory`,
- real ASP.NET Core middleware and routing,
- test database registration replacing PostgreSQL in the integration host.

They protect:

- routes,
- authentication cookies,
- authorization,
- antiforgery,
- status codes,
- Problem Details,
- ownership boundaries,
- controller-to-service wiring,
- dependency injection,
- DTO serialization,
- dashboard response shape,
- streak results through HTTP.

The integration test database substitution does not replace PostgreSQL-backed browser testing.

### 39.3 Frontend tests

Tools:

- Vitest,
- React Testing Library.

They protect:

- API modules,
- authentication transitions,
- forms,
- pending states,
- errors,
- accessible state,
- completion interaction,
- undo interaction,
- dashboard rendering,
- streak units,
- streak empty states,
- refresh coordination.

Tests verify observable behavior instead of component internals.

### 39.4 Browser tests

Tool:

- Playwright with Chromium.

The end-to-end environment uses:

- a real browser,
- the Vite frontend,
- the ASP.NET Core backend,
- PostgreSQL,
- real EF Core migrations.

Browser tests protect complete cross-layer user journeys.

### 39.5 Manual validation

Phase 6 was manually verified through the running application.

The smoke test covered:

- daily habit creation,
- weekly habit creation,
- initial zero streaks,
- completion,
- XP refresh,
- attribute refresh,
- streak updates,
- undo,
- append-only reversal behavior through the API flow,
- completion after undo,
- habit rename propagation,
- weekly streak units,
- habit deactivation,
- streak removal,
- dashboard refresh without a manual page reload.

---

## 40. Continuous Integration

GitHub Actions runs separate backend, frontend, and end-to-end validation.

### Backend job

- restores .NET dependencies,
- builds the backend,
- runs backend unit tests,
- runs backend integration tests.

### Frontend job

- installs exact npm dependencies,
- checks formatting,
- runs Oxlint,
- runs Vitest,
- builds the production frontend.

### End-to-end job

- starts a disposable PostgreSQL service,
- restores repository-local EF tooling,
- applies migrations,
- starts the backend,
- starts the frontend,
- installs Chromium,
- runs Playwright.

Temporary CI database credentials are limited to the disposable service.

Local and production secrets are not committed.

---

## 41. Phase 7 Architecture Constraints

Phase 7 changes presentation and interaction.

It may introduce:

- application shell,
- navigation,
- page layouts,
- shared visual components,
- responsive behavior,
- accessible interaction patterns,
- loading skeletons,
- standardized empty states,
- standardized error panels,
- restrained transitions.

Phase 7 must not:

- move XP calculations into React,
- move level calculations into React,
- move streak calculations into React,
- recreate effective-configuration selection in React,
- recreate week-boundary logic in React,
- change undo history,
- create a second source of truth,
- introduce unsupported product systems,
- replace the HTTP boundary with direct database access.

Existing backend DTOs remain authoritative unless a deliberate contract change is approved.

Shared components should be extracted from real usage rather than created as an unused framework.

---

## 42. Architecture Change Process

Before changing an established architecture decision:

1. describe the current behavior,
2. identify the concrete problem,
3. identify the affected layers,
4. compare the smallest valid options,
5. explain consistency and migration effects,
6. determine whether the change affects stack, naming, scope, or phase,
7. update the relevant documentation,
8. implement the change in a focused commit.

Do not rename established entities, services, routes, DTOs, tables, or folders for cosmetic preference.

Do not add a dependency for behavior already supported cleanly by the current stack.

Do not rewrite an entire subsystem to correct a local defect.

---

## 43. Stable Architecture Rules

- Frontend and backend remain separate applications.
- Frontend communication occurs through HTTP.
- PostgreSQL remains the persistent source of truth.
- Entity Framework Core remains the database access layer.
- DTOs cross API boundaries.
- Database entities are not API responses.
- Controllers remain thin.
- Services own application behavior.
- Authenticated identity comes from backend claims.
- User-owned requests do not accept client-provided user identifiers.
- Local completion dates are backend-owned.
- Week boundaries are backend-owned.
- Habit configuration history is backend-owned.
- Historical completions retain their effective configuration.
- XP rules are backend-owned.
- Attribute progression is backend-owned.
- Level formulas are backend-owned.
- Undo preserves the original event.
- XP reversals are append-only.
- Active completion uniqueness excludes undone records.
- Streaks are derived rather than stored.
- Streak rules are backend-owned.
- Dashboard aggregation is backend-owned.
- React owns presentation and interaction.
- Stable domain names are preserved.
- New architecture appears only when implemented behavior requires it.
- Post-MVP systems remain deferred until explicitly approved.

---

## 44. Summary

The architecture follows four primary boundaries:

```text
PostgreSQL stores persistent truth.
ASP.NET Core validates and calculates application behavior.
DTOs expose deliberate HTTP contracts.
React presents the returned state and sends user actions.
```

Phase 6 extended those boundaries without changing them:

- habit rules became effective-dated,
- completions became linked to historical configurations,
- undo became auditable,
- XP reversal became append-only,
- streaks became backend-derived,
- dashboard state became backend-aggregated.

Phase 7 may substantially change how the application looks and behaves visually, but it must preserve these ownership and persistence rules.
