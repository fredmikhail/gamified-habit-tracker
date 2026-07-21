# Gamified Habit Tracker — Architecture

**Status:** Current through Phase 5
**Next architectural phase:** Phase 6 — Dashboard and Streaks
**Architecture style:** Separate React frontend and ASP.NET Core Web API with PostgreSQL persistence
**Primary rule:** The backend owns application and business behavior; the frontend presents authoritative data

---

## 1. Purpose

This document describes the current architecture of the Gamified Habit Tracker and the approved direction for the next phases.

It is intended to help:

- the project owner understand how the system fits together,
- future developers locate responsibilities quickly,
- future ChatGPT project chats preserve established decisions,
- code reviews detect architectural drift,
- new features support the approved UI vision without prematurely building post-MVP systems.

This document distinguishes between:

- **implemented architecture,**
- **Phase 6 architecture that is next,**
- **later UI architecture planned for Phase 7,**
- **post-MVP ideas that remain deferred.**

The architecture may evolve through deliberate decisions. It must not drift silently during unrelated feature work or debugging.

---

## 2. Fixed Technology Stack

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

No library, framework, or infrastructure change should be introduced without first explaining:

1. the problem it solves,
2. why the current stack cannot reasonably solve it,
3. the tradeoffs,
4. the migration cost,
5. the effect on the roadmap.

---

## 3. High-Level Architecture

```text
Browser
   |
   | React user interface
   | typed HTTP / JSON requests
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

The frontend calls the backend through HTTP.

The backend owns:

- authentication,
- authorization,
- validation,
- ownership,
- completion rules,
- local-date rules,
- XP rules,
- level formulas,
- attribute persistence,
- future streak rules,
- future dashboard aggregation.

PostgreSQL is the persistent source of truth.

React renders the system. React does not invent the system.

---

## 4. Browser-Facing Origin Model

During local development:

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

Vite proxies `/api` requests to the ASP.NET Core backend.

This provides a same-origin browser experience while keeping the frontend and backend as separate applications.

Current local behavior:

- frontend origin: `http://localhost:5173`
- backend HTTPS origin: `https://localhost:7287`
- backend HTTP origin: `http://localhost:5167`
- Vite default API proxy target: `https://localhost:7287`
- the proxy accepts the local development certificate
- browser credentials are included by the frontend API client

The backend also has a development CORS policy for the Vite origin. The same-origin proxy remains the normal browser development path.

The preferred production shape is:

```text
Public application origin
├── React application
└── /api -> ASP.NET Core API
```

A reverse proxy or hosting platform may provide the shared public origin.

If deployment requires separate frontend and API origins, Phase 8 must deliberately configure:

- exact allowed origins,
- credentialed CORS,
- HTTPS,
- cookie `Secure` behavior,
- compatible `SameSite` behavior,
- antiforgery behavior,
- deployment secrets.

---

## 5. Current Repository Structure

The repository is organized approximately as follows:

```text
gamified-habit-tracker/
├── .config/
│   └── dotnet-tools.json
├── .github/
│   └── workflows/
│       └── ci.yml
├── client/
│   ├── e2e/
│   ├── src/
│   │   ├── api/
│   │   ├── auth/
│   │   ├── components/
│   │   │   ├── attributes/
│   │   │   ├── auth/
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

Folders should be created when a real feature requires them.

Do not create speculative folders merely to resemble a hypothetical finished architecture.

---

## 6. Core Responsibility Boundaries

## 6.1 Frontend Responsibilities

The frontend owns:

- page layout,
- component rendering,
- user interaction,
- form state,
- selected state,
- loading state,
- pending state,
- success and error presentation,
- typed API calls,
- browser routing when introduced,
- accessible visual feedback,
- display-only formatting,
- presentational percentages derived from authoritative progress values.

The frontend may perform light client-side input checks for immediate feedback.

The backend remains authoritative even when similar checks exist in React.

The frontend must not own:

- password verification,
- authorization,
- ownership,
- completion validity,
- duplicate completion prevention,
- local completion date,
- reward totals,
- category-to-attribute mapping,
- XP allocation,
- attribute level formulas,
- overall level formulas,
- streak continuity,
- dashboard business aggregation.

## 6.2 Backend Responsibilities

The backend owns:

- HTTP API endpoints,
- authentication,
- authorization,
- antiforgery enforcement,
- important validation,
- user ownership,
- local-date calculation,
- habit rules,
- completion rules,
- XP rules,
- level rules,
- attribute persistence,
- transaction history,
- future streak rules,
- future dashboard aggregation,
- mapping domain state into DTOs,
- consistent status codes,
- Problem Details responses.

The backend is the authority for application behavior.

## 6.3 Database Responsibilities

PostgreSQL stores persistent state and protects important invariants.

Current persistent concepts:

- users,
- user settings,
- habits,
- habit completions,
- automatic habit reward mappings,
- user attribute XP,
- XP transactions,
- EF Core migration history.

PostgreSQL constraints and indexes protect rules where practical even when services also validate the same rule for friendlier errors.

Examples include:

- unique normalized email,
- unique normalized username,
- one settings row per user,
- one completion per habit and local date,
- relationship integrity,
- configured check constraints.

---

## 7. Backend Structure

## 7.1 Controllers

Controllers expose HTTP endpoints.

Controllers should remain thin.

A controller normally:

1. receives a route and request DTO,
2. relies on ASP.NET Core request-model validation,
3. reads authenticated identity from claims,
4. calls one application service,
5. translates the service result into an HTTP response.

Controllers should not:

- perform direct database queries,
- calculate XP,
- calculate levels,
- calculate streaks,
- normalize domain data,
- enforce ownership independently of services,
- construct dashboard business state.

Authentication-cookie creation and removal are HTTP concerns and therefore remain in `AuthController`.

## 7.2 Services

Canonical service names:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

Current implementation status:

| Service | Status |
|---|---|
| `AuthService` | Implemented |
| `HabitService` | Implemented |
| `CompletionService` | Implemented |
| `XpService` | Implemented |
| `AttributeService` | Implemented |
| `StreakService` | Phase 6 |
| `DashboardService` | Phase 6 |

A service should have a clear application responsibility.

Do not create a new service solely because a UI mockup contains a new card or panel.

## 7.3 Data Layer

The `Data` area contains:

- `AppDbContext`,
- entity configurations,
- EF Core migrations.

`AppDbContext` currently exposes:

- `Users`,
- `UserSettings`,
- `Habits`,
- `HabitCompletions`,
- `UserAttributes`,
- `HabitAttributeRewards`,
- `XpTransactions`.

Entity configurations define:

- table and column behavior,
- required values,
- maximum lengths,
- relationships,
- delete behavior,
- indexes,
- check constraints,
- generated identifiers.

The Npgsql provider uses snake-case naming conventions.

Physical PostgreSQL identifiers use `snake_case`.

## 7.4 Domain Layer

The `Domain` area contains:

- entities,
- enums,
- value objects.

Implemented entities:

- `User`
- `UserSettings`
- `Habit`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

Reserved post-MVP entities:

- `Milestone`
- `UserMilestone`

Implemented domain enums include:

- habit category,
- habit difficulty,
- habit frequency type,
- attribute type.

The `LevelProgress` value object represents calculated progression data without persisting a separate level column.

## 7.5 DTO Layer

DTOs define API request and response contracts.

DTOs prevent:

- database entities from becoming public contracts,
- password hashes from being exposed,
- navigation properties from leaking,
- database schema changes from automatically breaking the frontend,
- clients from setting backend-owned properties.

Implemented authentication DTOs include:

- `RegisterRequest`
- `LoginRequest`
- `AntiforgeryTokenResponse`
- `AuthResponse`
- `CurrentUserResponse`

Implemented habit and completion DTOs include:

- `CreateHabitRequest`
- `UpdateHabitRequest`
- `HabitResponse`
- `CompleteHabitRequest`
- `HabitCompletionResponse`
- `CompleteHabitResponse`
- `HabitAttributeRewardResponse`

Implemented attribute DTOs include:

- `UserAttributeResponse`

Planned Phase 6 DTOs include:

- `DashboardResponse`
- focused nested dashboard response DTOs only where they improve clarity.

A planned name is not permission to add an empty class before its behavior is designed.

## 7.6 Exception Handling

Expected application failures use specific exception types.

`ApiExceptionHandler` maps known failures to Problem Details and appropriate HTTP status codes.

Examples include:

- invalid credentials,
- registration conflicts,
- invalid time zone,
- inactive habit completion,
- duplicate habit completion,
- validation failures.

Unexpected exceptions should not be disguised as expected failures.

They continue through ASP.NET Core exception handling and should be logged and investigated.

---

## 8. Backend Startup and Dependency Registration

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
- PostgreSQL through EF Core.

Current service lifetimes are scoped for application services that depend on request-level persistence.

JSON enums are serialized as camel-case strings.

Integer enum values are rejected from API JSON contracts.

The application resolves the PostgreSQL connection string when `AppDbContext` is created.

During normal startup:

1. ASP.NET Core builds the service provider.
2. The application creates a scope.
3. `AppDbContext` is resolved.
4. the connection string is required,
5. the database connection is checked,
6. startup fails when PostgreSQL cannot be reached.

This keeps production startup fail-fast while allowing integration tests to replace the database registration before it is resolved.

Schema changes are applied through EF Core migrations.

The repository-local `dotnet-ef` tool manifest makes the command-line tool version repeatable.

---

## 9. Authentication Architecture

The browser uses encrypted cookie authentication.

The frontend does not:

- receive an access token,
- store an access token,
- decode authentication credentials,
- attach bearer credentials manually.

After registration or login, ASP.NET Core creates the authentication cookie.

The browser sends it automatically with later requests.

```text
React
   |
   | credentials included
   v
ASP.NET Core authentication middleware
   |
   | authenticated ClaimsPrincipal
   v
Controller
   |
   | user identifier from ClaimTypes.NameIdentifier
   v
Application service
   |
   v
PostgreSQL
```

## 9.1 Authentication Cookie

Current cookie behavior:

- name: `HabitTracker.Auth`
- `HttpOnly`
- `SameSite=Lax`
- secure outside Development
- 12-hour maximum ticket lifetime by default
- fixed 30-day persistent session only when `rememberMe` is selected
- no sliding expiration during MVP

Registration creates a non-persistent session.

Unauthorized API requests return `401`.

Forbidden API requests return `403`.

They do not redirect to HTML login pages.

## 9.2 AuthController

`AuthController` owns authentication-related HTTP concerns:

- registration endpoint,
- login endpoint,
- logout endpoint,
- current-user endpoint,
- antiforgery-token endpoint,
- creating the authentication session,
- removing the authentication session,
- reading the current claim identifier.

It does not:

- query users directly,
- normalize credentials,
- hash passwords,
- verify passwords,
- validate account availability,
- create entities directly.

## 9.3 AuthService

`AuthService` owns authentication application behavior:

- account validation,
- IANA time-zone validation,
- trimming and normalization,
- duplicate pre-checks,
- password hashing,
- password verification,
- atomic user and settings creation,
- database uniqueness-race handling,
- last-login updates,
- response DTO mapping.

## 9.4 Ownership

Authenticated identity is derived from `ClaimTypes.NameIdentifier`.

User-owned requests do not accept a client-provided `userId`.

Controllers pass the authenticated identifier to services.

Services include that identifier in owned queries.

Missing and foreign-owned habit identifiers both return `404 Not Found`.

This avoids confirming that another user’s resource exists.

## 9.5 Antiforgery

State-changing browser requests require antiforgery protection because browsers attach authentication cookies automatically.

Current antiforgery behavior:

- request header: `X-CSRF-TOKEN`
- cookie: `HabitTracker.Antiforgery`
- `HttpOnly`
- `SameSite=Lax`
- secure outside Development
- automatic validation for POST, PUT, PATCH, and DELETE controller actions.

The frontend API client:

1. requests a token from `GET /api/auth/csrf-token`,
2. caches the request token in memory,
3. attaches it to state-changing calls,
4. includes browser credentials,
5. clears the cached token when authentication identity changes,
6. obtains a new token lazily.

Antiforgery behavior is centralized in the frontend API layer.

---

## 10. Habit Architecture

```text
React habit component
   |
   | typed request through habitsApi
   v
HabitsController
   |
   | authenticated user identifier
   v
HabitService
   |
   | owned EF Core query and backend validation
   v
PostgreSQL
```

`HabitService` owns:

- trimming,
- blank-to-null normalization,
- daily and weekly target validation,
- enum-backed validation,
- ownership filtering,
- active and include-inactive behavior,
- deterministic ordering,
- owned retrieval,
- updates,
- soft deactivation,
- automatic reward synchronization,
- response DTO mapping.

The frontend owns:

- form values,
- editing state,
- confirmation state,
- pending state,
- error display.

After create, update, or deactivation, the frontend reloads the authoritative habit list.

The local React array is not treated as persistent truth.

## 10.1 Automatic Habit Rewards

A habit’s category and difficulty determine two automatic attribute rewards.

`HabitService` synchronizes persisted `HabitAttributeReward` rows when:

- a habit is created,
- category changes,
- difficulty changes,
- legacy or incomplete reward data must be repaired.

Clients do not submit reward allocations.

Habit responses expose the calculated or persisted reward breakdown for display.

This supports future habit reward previews without moving reward rules into React.

---

## 11. Completion, XP, and Attribute Architecture

The Phase 5 vertical slice is implemented.

```text
User clicks Mark complete
   |
   v
HabitCompletionButton
   |
   | POST /api/habits/{habitId}/completions
   v
CompletionsController
   |
   | authenticated user identifier
   v
CompletionService
   |
   | validates ownership, active state, local date, duplicate state
   | ensures automatic habit rewards exist
   v
AttributeService
   |
   | updates UserAttribute rows
   | adds XpTransaction rows
   v
AppDbContext.SaveChangesAsync
   |
   | completion + attributes + XP transactions
   v
PostgreSQL
   |
   v
CompleteHabitResponse
   |
   | completion + exact awarded rewards
   v
React
   |
   | updates completed state
   | displays earned XP
   | reloads attribute progress
```

## 11.1 CompletionService

`CompletionService` owns orchestration for:

- completing a habit,
- undoing today’s completion,
- owned habit lookup,
- active-habit validation,
- user time-zone lookup,
- local completed-date calculation,
- duplicate checks,
- note normalization,
- reward repair when needed,
- applying XP,
- reversing XP,
- completion response mapping.

`CompletionService` uses `TimeProvider`.

This makes date-sensitive behavior testable.

The client cannot submit `CompletedDate`.

## 11.2 Local Date Ownership

The completion timestamp is captured in UTC.

The backend loads the user’s stored IANA time zone.

`LocalDateCalculator` derives the user-local `CompletedDate`.

The date stored for completion behavior is therefore controlled by the backend.

The browser cannot choose a different completion date.

## 11.3 Duplicate Protection

The service checks for an existing completion before inserting.

PostgreSQL also protects the unique combination of:

- habit identifier,
- completed local date.

The friendly service check improves the normal error response.

The database constraint protects against concurrent requests that pass the first check.

## 11.4 XpService

`XpService` is a calculation service.

It does not access the database.

It owns:

- difficulty XP totals,
- category-to-attribute mapping,
- the 70/30 split,
- attribute-level calculations,
- overall-level calculations.

Difficulty totals:

- Easy: 10
- Medium: 20
- Hard: 30
- Elite: 50

Attribute level curve:

- Level 1 to 2 requires 100 XP.
- Each next level requires 25 more XP than the previous level.

Overall level curve:

- Level 1 to 2 requires 200 XP.
- Each next level requires 50 more XP than the previous level.

Levels are calculated values.

They are not currently stored as mutable database columns.

## 11.5 AttributeService

`AttributeService` owns attribute persistence and attribute response behavior.

It:

- loads all stored attributes for the current user,
- returns all supported attributes,
- supplies zero-XP responses for attributes without stored rows,
- calculates progress through `XpService`,
- creates missing `UserAttribute` rows when XP is first awarded,
- increments attribute XP,
- creates one `XpTransaction` per awarded attribute,
- reverses XP from the original transactions,
- removes reversed XP transactions.

Undo uses the original persisted transactions rather than recalculating what the reward would be today.

This preserves auditability if reward rules change later.

## 11.6 Atomic Save Behavior

Completion changes are staged in one `AppDbContext`:

- new `HabitCompletion`,
- `UserAttribute` increments,
- new `XpTransaction` rows.

They are persisted by one `SaveChangesAsync` call.

Undo changes are also staged in one context:

- `UserAttribute` decrements,
- original `XpTransaction` removals,
- `HabitCompletion` removal.

They are persisted by one `SaveChangesAsync` call.

With the relational PostgreSQL provider, the changes in each save operation commit or roll back together.

No frontend state is considered authoritative until the backend request succeeds.

## 11.7 Completion Response

A successful completion returns:

- completion identity and timestamps,
- the exact attribute rewards that were awarded.

React uses those returned rewards for immediate feedback.

React does not recalculate them.

After completion or undo, the attribute section reloads `GET /api/attributes` so displayed totals remain authoritative.

---

## 12. Attribute Read Architecture

```text
AttributeSection
   |
   | GET /api/attributes
   v
AttributesController
   |
   | authenticated user identifier
   v
AttributeService.GetUserAttributesAsync
   |
   | stored UserAttribute rows
   | XpService progression calculation
   v
UserAttributeResponse[]
   |
   v
React attribute cards
```

The endpoint returns all eight attributes in enum order.

An attribute without a stored database row appears as:

- 0 total XP,
- Level 1,
- 0 XP inside the current level,
- 100 XP needed for the next attribute level.

The backend supplies:

- total XP,
- level,
- XP inside the current level,
- XP needed for the next level.

React may calculate a CSS progress-bar percentage from those values.

React must not reproduce the level curve.

---

## 13. Frontend Architecture

## 13.1 API Layer

The `client/src/api` folder contains feature-focused HTTP functions.

Current modules include:

- `apiClient.ts`
- `authApi.ts`
- `habitsApi.ts`
- `attributesApi.ts`
- `healthApi.ts`
- `readApiError.ts`

`apiClient.ts` centralizes:

- browser credentials,
- state-changing method detection,
- antiforgery acquisition,
- antiforgery headers.

Feature API modules own:

- endpoint paths,
- request serialization,
- response parsing,
- API-specific fallback errors.

React components should not duplicate these concerns.

## 13.2 Authentication State

The `auth` area contains:

- `AuthContext`
- `AuthProvider`
- `useAuth`
- authentication tests.

`AuthProvider` owns:

- current authenticated user state,
- session restoration on startup,
- registration transition,
- login transition,
- logout transition,
- authentication loading and error state.

It does not own:

- password policy,
- credential verification,
- cookie contents,
- ownership.

## 13.3 Habit Components

Current habit components include:

- `HabitSection`
- `HabitForm`
- `HabitList`
- `HabitEditForm`
- `HabitCompletionButton`
- `HabitDeactivateButton`

`HabitSection` coordinates separate refresh signals for:

- habit-list data,
- attribute-progress data.

Creation, update, and deactivation reload the habit list.

Completion and undo refresh attribute progress.

The completion button uses the backend completion response for earned-XP feedback.

## 13.4 Attribute Components

Current attribute components include:

- `AttributeSection`.

`AttributeSection` owns:

- loading the attribute endpoint,
- loading state,
- error state,
- rendering cards,
- accessible progress bars,
- presentational progress width.

It does not own XP or level rules.

Phase 7 may extract reusable components such as `AttributeCard` and `ProgressBar` when the real design system requires them.

## 13.5 TypeScript API Types

The `types` folder mirrors backend API concepts.

Current examples include:

- authentication request and response types,
- habit request and response types,
- completion request and response types,
- `HabitAttributeRewardResponse`,
- `AttributeType`,
- `UserAttributeResponse`.

When the backend response contract changes, matching TypeScript types and all affected fixtures must be updated in the same slice.

## 13.6 Pages and Routing

The application does not need speculative route-level files before navigation requires them.

Phase 6 will introduce the first real dashboard screen.

Phase 7 will establish the approved application shell and navigation.

Potential route-level screens:

- Dashboard
- Habits
- Attributes
- Weekly Review
- Settings

A route should not exist only to display fake or placeholder data.

---

## 14. Phase 6 Architecture Direction

Phase 6 adds:

- `StreakService`,
- `DashboardService`,
- dashboard DTOs,
- an authenticated dashboard endpoint,
- a functional dashboard screen.

## 14.1 StreakService

Before implementation, streak semantics must be written and approved.

The design must define:

- whether streaks are per habit, overall, or both,
- what makes a day successful,
- how daily and weekly habits participate,
- whether an incomplete current day breaks the displayed streak,
- how user-local dates are applied,
- how inactive habits affect history,
- the meaning of current streak,
- the meaning of longest streak.

The UI mockups are not the source of streak rules.

`StreakService` owns the rules after approval.

React only displays returned values.

## 14.2 DashboardService

`DashboardService` will aggregate screen-oriented data.

Expected responsibilities:

- today’s habits,
- completed and remaining counts,
- completion percentage,
- total XP,
- overall-level progress,
- XP gained today,
- current streak,
- longest streak,
- compact attribute progress,
- recent completions,
- recent XP activity,
- basic seven-day summaries.

The exact first `DashboardResponse` should remain focused.

Do not add every future chart field at once.

## 14.3 Overall XP Source

Phase 6 must explicitly define and test how total user XP is derived.

The design should avoid adding a duplicate mutable `TotalXp` column unless a demonstrated performance or consistency need justifies it.

Likely authoritative sources already exist:

- persisted `UserAttribute.CurrentXp`,
- persisted `XpTransaction` history.

The chosen derivation must:

- agree with undo,
- avoid double counting,
- be testable,
- remain backend-owned,
- be documented in the API and data-model documents.

## 14.4 Dashboard Endpoint

The controller should:

1. read the authenticated user identifier,
2. call `DashboardService`,
3. return `DashboardResponse`.

The controller should not separately query habits, attributes, completions, and transactions.

## 14.5 Dashboard Frontend

The frontend dashboard should consume one purpose-built response where practical.

It may own:

- layout,
- loading state,
- error state,
- local interaction state,
- display-only chart preparation.

It must not own:

- streak rules,
- level formulas,
- completion percentage rules when supplied by the backend,
- ownership filtering,
- recent-activity business selection.

The first Phase 6 UI may remain visually functional.

The complete dark game-style redesign belongs to Phase 7.

---

## 15. UI Vision Alignment

The approved visual target is:

> A premium personal-development command center that presents real habit data as elegant character progression.

Architecture supports that destination through:

- stable entities,
- deterministic enums,
- backend-derived rewards,
- backend-derived progression,
- auditable XP transactions,
- purpose-built DTOs,
- future dashboard aggregation,
- reusable frontend components.

Architecture must not support the UI vision by prematurely adding:

- avatar tables,
- currencies,
- fake ranks,
- notification infrastructure,
- quest infrastructure,
- empty achievement systems,
- generic analytics engines.

Phase 7 may create presentation components such as:

- `AppShell`
- `Sidebar`
- `TopBar`
- `Panel`
- `Card`
- `ProgressBar`
- `LevelBadge`
- `StreakBadge`
- `AttributeCard`
- `RewardPreview`
- `StatCard`
- `EmptyState`
- `ErrorPanel`

These components should be introduced through real screen needs, not generated as an unused framework.

---

## 16. Testing Architecture

Different test layers protect different risks.

## 16.1 Backend Unit Tests

Project:

- `HabitTracker.Tests`

Purpose:

- focused service and calculation behavior,
- request validation,
- fast feedback,
- no HTTP dependency when HTTP is not under test.

Current protected behavior includes:

- authentication rules,
- habit rules,
- completion rules,
- XP rewards,
- level curves,
- attribute application and reversal,
- attribute response behavior.

## 16.2 Backend HTTP Integration Tests

Project:

- `HabitTracker.IntegrationTests`

Infrastructure:

- `WebApplicationFactory`,
- the real ASP.NET Core pipeline,
- EF Core InMemory provider substituted only inside the integration host.

Purpose:

- routes,
- status codes,
- authentication cookies,
- authorization,
- antiforgery,
- Problem Details,
- controller/service integration,
- response serialization,
- ownership.

The integration host removes the production `AppDbContext` registration before adding the test database provider.

Integration tests do not replace the need for PostgreSQL-backed end-to-end tests.

## 16.3 Frontend Tests

Tools:

- Vitest,
- React Testing Library.

Purpose:

- API modules,
- authentication transitions,
- form behavior,
- component behavior,
- pending states,
- errors,
- accessible state,
- local UI updates,
- attribute refresh behavior,
- earned-XP feedback.

Files use `.test.ts` or `.test.tsx`.

Tests should verify observable behavior rather than component implementation details.

## 16.4 Browser End-to-End Tests

Tool:

- Playwright with Chromium.

Purpose:

- real browser,
- real ASP.NET Core backend,
- real Vite frontend,
- real PostgreSQL,
- complete user journeys.

The project currently includes authentication journey coverage.

End-to-end coverage should be expanded when a new cross-layer user journey becomes important enough to justify the cost.

Phase 6 should consider a PostgreSQL-backed dashboard journey after the contract stabilizes.

## 16.5 Continuous Integration

GitHub Actions runs:

```text
Backend
Frontend
End-to-end
```

Backend job:

- restores .NET dependencies,
- builds in Release,
- runs backend tests.

Frontend job:

- installs exact npm dependencies,
- checks formatting,
- runs linting,
- builds,
- runs Vitest.

End-to-end job:

- starts disposable PostgreSQL,
- restores repository-local EF tooling,
- applies real migrations,
- installs frontend dependencies,
- installs Chromium,
- runs Playwright.

Temporary CI credentials belong only to the disposable service.

Secrets for local or production databases are not committed.

---

## 17. Verification Workflow

For each vertical slice:

### Before coding

- inspect current files,
- identify the likely owning service,
- confirm current phase scope,
- confirm DTO impact,
- list affected tests.

### During coding

- make the smallest coherent change,
- keep controllers thin,
- keep formulas in backend services,
- use typed frontend contracts,
- update fixtures when contracts change.

### Verification order

1. focused backend or frontend tests,
2. related integration tests,
3. full backend tests,
4. full frontend tests,
5. formatting,
6. linting,
7. production build,
8. manual browser verification,
9. PostgreSQL-backed verification when persistence is involved.

### Before commit

Run:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Check for:

- missing intended files,
- unrelated modifications,
- accidental file overwrites,
- duplicated test contents,
- generated files,
- formatting-only changes mixed into feature work.

---

## 18. Architecture Change Rules

Architecture changes must be deliberate.

Before changing an established decision:

1. describe the current behavior,
2. explain the problem,
3. identify affected layers,
4. compare the smallest valid options,
5. explain tradeoffs,
6. confirm whether the change affects stack, naming, scope, or phase,
7. update documentation,
8. implement in a focused commit.

Do not rename established entities, services, folders, routes, DTOs, or tables for cosmetic preference.

Do not add a library for behavior already supported by the current stack unless the benefit clearly exceeds the cost.

Do not rewrite an entire feature to fix a small defect.

---

## 19. Stable Architecture Rules

- Frontend and backend remain separate applications.
- Frontend communicates through HTTP.
- PostgreSQL remains the source of truth.
- EF Core remains the database access layer.
- DTOs cross API boundaries.
- Database entities are not API responses.
- Controllers remain thin.
- Services own application behavior.
- Authenticated identity comes from backend claims.
- User-owned requests do not accept client-provided user identifiers.
- Completion dates are backend-owned.
- XP rules are backend-owned.
- Attribute progression is backend-owned.
- Streak rules will be backend-owned.
- Dashboard aggregation will be backend-owned.
- React owns presentation and interaction.
- Historical XP reversals use persisted transactions.
- Stable domain names are preserved.
- New architecture appears only when a real phase requires it.
- Post-MVP systems remain deferred until explicitly approved.

---

## 20. Final Rule

The database stores the truth.

The backend owns the rules.

DTOs carry the contract.

The frontend presents the experience.

The UI vision defines the destination.

The roadmap controls the sequence.

No future developer, chat, or tool should silently move those responsibilities.
