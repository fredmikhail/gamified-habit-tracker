# Gamified Habit Tracker — Architecture

**Status:** Current through Phase 7

**Current phase:** Phase 8 — Deployment and Project Polish

## System Boundary

```text
Browser
   |
   | React interface and typed HTTP requests
   v
Vite development server or production web origin
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

The architectural boundary is:

```text
PostgreSQL stores persistent state.
ASP.NET Core owns validation and application behavior.
DTOs define the HTTP boundary.
React owns presentation and interaction.
```

## Technology

### Frontend

- React
- TypeScript
- Vite
- Tailwind CSS
- React Router
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

### Database and automation

- PostgreSQL
- GitHub Actions
- repository-local `dotnet-ef`

A stack change requires an explicit architecture decision.

## Responsibility Boundaries

### React owns

- routing and page composition;
- form state;
- selection, tabs, filters, sorting, and pagination;
- loading, pending, success, and error presentation;
- confirmation interaction;
- accessible labels and focus behavior;
- display formatting;
- chart and progress rendering from returned values;
- browser appearance preference;
- short-lived animation state.

React may calculate presentational ratios such as:

```text
xpIntoCurrentLevel / xpNeededForNextLevel
completedDailyHabits / totalDailyHabits
```

React does not own:

- user identity;
- authorization;
- ownership;
- completion eligibility;
- local completion dates;
- week boundaries;
- effective habit configuration;
- XP totals or reward allocation;
- attribute mapping;
- level curves;
- undo reversal;
- streak continuity;
- dashboard or attribute aggregate calculations.

### ASP.NET Core owns

- HTTP routes and status codes;
- authentication and authorization;
- antiforgery enforcement;
- user ownership;
- request and domain validation;
- local-date and week-period calculation;
- habit lifecycle;
- effective-dated configuration scheduling;
- completion and undo;
- XP transactions;
- attribute progression;
- level calculation;
- streak calculation;
- dashboard aggregation;
- attribute overview aggregation;
- entity-to-DTO mapping;
- expected-error mapping.

### PostgreSQL owns

- persistent application facts;
- relationship integrity;
- uniqueness;
- constrained enum-backed values;
- active-completion uniqueness;
- append-only XP history;
- EF Core migration history.

Service checks provide usable errors. Database constraints remain the final protection against invalid concurrent writes.

## Backend Structure

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

Controllers:

- accept route values and request DTOs;
- resolve the user identifier from claims;
- call the owning service;
- map the result to an HTTP response.

Controllers do not query `AppDbContext` directly or calculate progression.

Canonical services:

| Service             | Responsibility                                                                   |
| ------------------- | -------------------------------------------------------------------------------- |
| `AuthService`       | Registration, credential verification, account lookup, current-user mapping      |
| `HabitService`      | Habit creation, reads, edits, activation, deactivation, configuration scheduling |
| `CompletionService` | Completion and undo orchestration                                                |
| `XpService`         | Reward and level formulas                                                        |
| `AttributeService`  | Attribute reads, reward persistence, reversal, attribute overview                |
| `StreakService`     | Daily and weekly per-habit streak calculation                                    |
| `DashboardService`  | Authenticated dashboard aggregation                                              |

Supporting calculation components include:

- `LocalDateCalculator`;
- `CalendarPeriodCalculator`;
- level and streak value objects.

## Persistent Domain Model

Implemented entities:

- `User`;
- `UserSettings`;
- `Habit`;
- `HabitConfigurationVersion`;
- `HabitCompletion`;
- `HabitAttributeReward`;
- `UserAttribute`;
- `XpTransaction`.

Reserved post-MVP names:

- `Milestone`;
- `UserMilestone`.

Reserved entities are not implemented.

The Npgsql provider maps physical PostgreSQL identifiers to `snake_case`.

Schema changes are applied through EF Core migrations.

## DTO Boundary

Request and response DTOs are separate from database entities.

DTOs prevent:

- password hashes from being exposed;
- navigation properties from leaking;
- persistence-only fields from becoming public contracts;
- request bodies from accepting authoritative ownership values;
- controllers from returning tracked entities.

JSON uses camel-case properties and camel-case enum strings.

## Frontend Structure

### Application entry and providers

`main.tsx` establishes shared providers.

Key providers:

- `ThemeProvider`;
- `AuthProvider`;
- router;
- `WorkspaceDataProvider` inside the authenticated workspace.

### Authentication

`AuthProvider` owns the browser’s current authentication state.

It:

- restores the current user;
- calls registration, login, and logout APIs;
- stores the returned `CurrentUserResponse` in memory;
- exposes loading and error state.

The encrypted authentication cookie remains unreadable to React.

### Routed workspace

`AuthenticatedWorkspace` defines:

- `/dashboard`;
- `/habits`;
- `/attributes`;
- index and unknown-route redirects to `/dashboard`.

`AppShell` is the shared layout and renders route content through `Outlet`.

### Shared workspace data

`WorkspaceDataProvider` owns reusable frontend resources for:

- dashboard;
- habits;
- attribute overview.

Resources expose:

- current data;
- initial loading;
- refreshing;
- error state;
- load and refresh operations;
- controlled cache updates.

Mutations invalidate or refresh the resources whose authoritative data may have changed. The provider is not a second persistence layer.

### Page ownership

- `DashboardPage` renders the aggregate dashboard and primary completion actions.
- `HabitSection` and `HabitList` own the management workspace.
- `AttributeSection` renders the attribute overview.
- `AppShell` owns global navigation and account/progression presentation.
- `ThemeProvider` owns browser appearance preference.
- `useProgressionFeedback` owns short-lived visual comparison of consecutive authoritative responses.

### Shared presentation

Current shared primitives include:

- `CommandPanel`;
- `MetricPanel`;
- progression and attribute cards;
- theme controls;
- layout and navigation components.

Components are introduced because current screens use them. The project does not maintain a speculative component framework.

## Key Workflows

### Registration

```text
RegisterForm
  -> POST /api/auth/register
  -> AuthService creates User and UserSettings
  -> one EF Core save
  -> controller creates authentication cookie
  -> AuthResponse
  -> AuthProvider stores CurrentUserResponse
  -> authenticated workspace renders
```

Registration defaults `WeekStartsOn` to Monday.

### Habit creation

```text
Habit editor
  -> POST /api/habits
  -> HabitService validates and normalizes
  -> current local date calculated
  -> Habit + initial HabitConfigurationVersion + rewards
  -> one EF Core save
  -> HabitResponse
  -> workspace cache and aggregate refresh
```

### Habit edit

```text
PUT /api/habits/{id}
  -> immediate name/description update
  -> rule comparison against effective configuration
  -> create, update, or remove next-week pending version
  -> HabitResponse with optional pendingConfiguration
```

### Deactivation and activation

Deactivation and activation modify `Habit.IsActive`.

They do not delete:

- the habit;
- configuration history;
- completion history;
- XP transactions.

### Completion

```text
POST /api/habits/{id}/completions
  -> ownership and active-state checks
  -> local date and effective configuration
  -> duplicate active-completion check
  -> HabitCompletion
  -> positive XpTransactions
  -> UserAttribute updates
  -> one transaction / save
  -> CompleteHabitResponse
  -> frontend refetches authoritative aggregates
```

### Undo

```text
DELETE /api/habits/{id}/completions/today
  -> find active completion for current local date
  -> set UndoneAtUtc
  -> append negative XpTransactions
  -> reverse UserAttribute totals
  -> save
  -> 204 No Content
  -> frontend refetches authoritative aggregates
```

### Dashboard read

```text
GET /api/dashboard
  -> DashboardService
  -> settings + active habits + configurations + completions + attributes
  -> StreakService
  -> DashboardResponse
  -> DashboardPage
```

### Attribute overview read

```text
GET /api/attributes/overview
  -> AttributeService
  -> current attribute state + recent XP transactions
  -> backend summaries and ranking
  -> AttributeOverviewResponse
  -> AttributeSection
```

The overview is a derived read model. Phase 7 added no table for balance, ranking, radar values, or recent-activity panels.

## Historical Integrity

`HabitConfigurationVersion` uses half-open periods:

```text
EffectiveFromDate <= date < EffectiveToDateExclusive
```

Each completion stores the configuration version that was effective on its local completion date.

Undo preserves the completion and original award transactions.

`XpTransaction` is append-only for normal product behavior:

- positive values award XP;
- negative values reverse an award.

Streaks use historical configuration and active, non-undone completions. They are not stored counters.

## Browser Security

Authentication uses an encrypted ASP.NET Core cookie.

Cookie behavior:

- `HttpOnly`;
- `SameSite=Lax`;
- secure outside Development;
- no sliding expiration;
- 12-hour default session;
- up to 30 days when remembered.

State-changing requests use an antiforgery token in:

```text
X-CSRF-TOKEN
```

Frontend requests include browser credentials.

The backend resolves ownership from authenticated claims. Request DTOs do not accept authoritative `userId` values.

## Origin Model

Local development normally uses:

```text
Browser -> http://localhost:5173 -> Vite /api proxy -> ASP.NET Core
```

The preferred production topology is one public origin serving the frontend and proxying `/api`.

A separate-origin deployment requires explicit decisions for:

- credentialed CORS;
- cookie domain and `SameSite`;
- HTTPS;
- antiforgery;
- allowed origins;
- secrets.

That production decision belongs to Phase 8.

## Testing Architecture

### Backend unit tests

Protect service and calculation behavior with isolated dependencies where appropriate.

### HTTP integration tests

Use `WebApplicationFactory` to protect:

- routing;
- authentication;
- antiforgery;
- status codes;
- serialization;
- controller/service integration;
- ownership boundaries.

### Frontend tests

Use Vitest and React Testing Library for:

- API modules;
- providers;
- forms and actions;
- loading and error states;
- route shell;
- adaptive dashboard pagination;
- habit layout;
- attribute command center;
- progression feedback.

### Browser tests

Playwright starts a real frontend and API against PostgreSQL.

Current browser coverage protects the authentication lifecycle and session restoration.

### Continuous integration

GitHub Actions runs:

- Backend;
- Frontend;
- End-to-end.

The E2E job creates PostgreSQL, applies migrations, installs Chromium, and runs Playwright.

## Phase 8 Constraints

Deployment work may change hosting and environment configuration.

It must not silently change:

- the domain model;
- progression formulas;
- authentication behavior;
- API contracts;
- canonical naming;
- ownership boundaries.

Production topology decisions must be documented before they alter cookie, CORS, or antiforgery behavior.
