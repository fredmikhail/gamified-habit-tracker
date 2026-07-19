# Architecture

This document describes the current and planned architecture for the Gamified Habit Tracker app.

The application is a full-stack web app with a separate frontend, backend, and PostgreSQL database.

---

## High-Level Architecture

```text
React + TypeScript Frontend
        |
        | HTTP / JSON
        v
ASP.NET Core Web API
        |
        | Entity Framework Core
        v
PostgreSQL Database
```

During local development and browser end-to-end testing, the frontend calls `/api` through the Vite development server. Vite proxies those requests to the ASP.NET Core backend.

This keeps the browser-facing requests same-origin while the frontend and backend remain separate applications.

---

## Current Repository Structure

The repository currently contains:

```text
gamified-habit-tracker/
├── .github/
│   └── workflows/
│       └── ci.yml
├── client/
│   ├── e2e/
│   ├── src/
│   │   ├── api/
│   │   ├── auth/
│   │   ├── components/
│   │   │   ├── auth/
│   │   │   └── habits/
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
│   │   │   └── Entities/
│   │   ├── DTOs/
│   │   ├── ExceptionHandling/
│   │   ├── Exceptions/
│   │   ├── Services/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── HabitTracker.Tests/
│   └── HabitTracker.IntegrationTests/
└── dotnet-tools.json
```

Folders for later phases should be added only when the related feature requires them.

---

## Main Responsibilities

### Frontend

The frontend is responsible for:

- Displaying pages and components.
- Handling user interactions.
- Calling backend API endpoints.
- Showing loading, success, and error states.
- Holding temporary UI state.
- Rendering habit cards, progress bars, attributes, and dashboard UI.

The frontend should not contain authoritative application rules such as:

- authentication credential validation
- user ownership enforcement
- XP calculation
- streak calculation
- habit completion validation
- duplicate completion prevention
- database rules

The frontend may perform basic input checks for faster user feedback, but the backend remains authoritative.

---

### Backend

The backend is responsible for:

- Exposing HTTP API endpoints.
- Validating important user actions.
- Authenticating requests.
- Enforcing user-specific data access.
- Running application and business logic.
- Calculating XP rewards.
- Calculating streaks.
- Managing habit completions.
- Reading and writing data through Entity Framework Core.
- Returning DTOs rather than database entities.
- Producing consistent HTTP status codes and error responses.

The backend owns the core rules of the application.

---

### Database

PostgreSQL is responsible for storing persistent data, including:

- users
- settings
- habits
- habit completions
- attribute progress
- XP transactions
- milestone data when the post-MVP milestone feature is introduced

The database is the source of truth.

Database constraints should protect important invariants where practical, even when the backend also validates the same rule for a friendlier response.

---

## Backend Structure

The backend uses ASP.NET Core Web API.

### Controllers

Controllers expose API endpoints.

They should stay thin.

A controller should usually:

1. Receive the HTTP request.
2. Rely on ASP.NET Core for request-shape validation.
3. Read authenticated identity or other HTTP context when needed.
4. Call the appropriate service.
5. Translate the result into an HTTP response.

Controllers should not contain complex business logic or direct database queries.

Authentication-specific HTTP work such as creating or removing the authentication cookie belongs in `AuthController`.

### Services

Services contain application and business logic.

Stable service names:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

Services should handle decisions such as:

- whether an account can be created
- how account identifiers are normalized
- whether credentials are valid
- whether a habit belongs to the authenticated user
- whether a habit can be completed
- how much XP is awarded
- how attributes are updated
- how streaks are calculated
- what data the dashboard needs

### Data

The `Data` folder contains database-related code.

It includes:

- `AppDbContext`
- entity configuration classes
- Entity Framework Core migrations

`AppDbContext` represents the connection between C# entity classes and PostgreSQL tables.

Entity configuration classes define database lengths, required properties, relationships, indexes, delete behavior, and application-generated identifiers.

Physical PostgreSQL identifiers use `snake_case`.

### Domain

The `Domain` folder contains core domain types.

It includes or will include:

- entities
- enums

Implemented entities:

- `User`
- `UserSettings`
- `Habit`

Planned MVP entities:

- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

Reserved post-MVP entities:

- `Milestone`
- `UserMilestone`

Enums will represent fixed domain values such as:

- habit difficulty
- habit frequency type
- attribute type

### DTOs

DTOs define the data that moves between frontend and backend.

DTO stands for Data Transfer Object.

The API returns DTOs instead of database entities. This keeps the database model separate from the public API shape and prevents sensitive properties such as password hashes from being exposed.

Implemented authentication DTOs include:

- `RegisterRequest`
- `LoginRequest`
- `AntiforgeryTokenResponse`
- `AuthResponse`
- `CurrentUserResponse`

Implemented habit DTOs include:

- `CreateHabitRequest`
- `UpdateHabitRequest`
- `HabitResponse`

Planned feature DTOs include:

- `CompleteHabitRequest`
- `CompleteHabitResponse`
- `DashboardResponse`

### Exceptions and Exception Handling

Expected application failures use specific exception types.

Examples include:

- invalid credentials
- account conflict
- invalid IANA time zone

`ApiExceptionHandler` maps known application exceptions to Problem Details responses and appropriate HTTP status codes.

Unexpected exceptions continue through ASP.NET Core's normal exception handling rather than being disguised as expected authentication failures.

---

## Backend Startup and Database Configuration

The production API registers PostgreSQL through `AddDbContext`.

The connection string is resolved when `AppDbContext` is created rather than during immediate top-level service registration. This allows integration tests to replace the production database registration before it is resolved.

During normal application startup:

1. ASP.NET Core builds the service provider.
2. The application creates a scope.
3. `AppDbContext` is resolved.
4. The configured connection string is required.
5. The application verifies that PostgreSQL can be reached.
6. Startup fails when the connection string is missing or the database cannot be reached.

This preserves fail-fast production behavior while keeping integration tests replaceable.

Database schema changes are applied through EF Core migrations. The repository-local `dotnet-ef` tool manifest keeps the EF Core command-line tool version repeatable.

---

## Authentication Architecture

The browser application uses encrypted cookie-based authentication.

The frontend does not receive, store, decode, or manually attach authentication credentials.

After successful registration or login, ASP.NET Core creates the authentication cookie. The browser sends it automatically with later requests.

The authentication flow is:

```text
React frontend
        |
        | HTTP request with browser-managed cookie
        v
ASP.NET Core authentication middleware
        |
        | authenticated ClaimsPrincipal
        v
AuthController
        |
        v
AuthService
        |
        | Entity Framework Core
        v
PostgreSQL
```

### Authentication Cookie

The current authentication cookie:

- is named `HabitTracker.Auth`
- is `HttpOnly`
- uses `SameSite=Lax`
- is secure outside the Development environment
- has a fixed 12-hour maximum ticket lifetime by default
- becomes persistent for a fixed 30 days only when `rememberMe` is selected during login
- does not use sliding expiration during the MVP

Registration creates a non-persistent 12-hour session.

Unauthorized and forbidden API requests return `401` and `403` status codes instead of browser redirects.

### AuthController Responsibilities

`AuthController` handles authentication-related HTTP concerns.

It:

- receives registration and login requests
- calls `AuthService`
- creates the authentication session after successful registration or login
- ends the authentication session during logout
- generates antiforgery request tokens
- reads the authenticated user identifier from claims
- returns request and response DTOs with appropriate status codes

It does not:

- query users directly
- normalize email or username
- hash or verify passwords
- decide whether registration data is available
- create `User` or `UserSettings` directly
- contain account business rules

### AuthService Responsibilities

`AuthService` owns authentication application logic.

It:

- validates account-related rules not handled by DTO validation
- validates IANA time-zone identifiers
- trims and normalizes email and username
- checks normalized email and username availability
- hashes passwords during registration
- verifies passwords during login
- creates `User` and `UserSettings` together
- handles database uniqueness races
- updates `LastLoginAtUtc` after successful login
- returns user data through response DTOs

### Authenticated Identity and Ownership

ASP.NET Core authentication middleware validates the encrypted authentication cookie and creates the authenticated request identity.

Authorization middleware runs after authentication.

Controllers derive the current `User` identifier from `ClaimTypes.NameIdentifier` and pass it to services that perform user-owned operations.

Services must not trust a `UserId` supplied by the frontend.

User-owned requests must not accept a client-provided `UserId`.

Phase 3 implemented the first resource-ownership rules and tests through the Habit resource. Missing and foreign-owned habit identifiers both produce `404 Not Found`, and habit requests never accept a client-supplied `UserId`.

### Antiforgery Protection

Because browsers send authentication cookies automatically, state-changing browser requests use antiforgery protection.

The current antiforgery configuration uses:

- request header `X-CSRF-TOKEN`
- cookie `HabitTracker.Antiforgery`
- `HttpOnly`
- `SameSite=Lax`
- secure cookies outside Development

Global automatic antiforgery validation applies to controller actions using:

- POST
- PUT
- PATCH
- DELETE

The frontend API client:

1. requests a token from `GET /api/auth/csrf-token`
2. caches the request token in memory
3. attaches it to state-changing requests
4. includes browser credentials on API requests
5. clears the cached token after registration, login, or logout
6. obtains a replacement lazily before the next state-changing request

Antiforgery handling is centralized in the frontend API layer rather than repeated inside pages or components.

---

## Registration Persistence

Registration creates `User` and `UserSettings` before one call to `SaveChangesAsync`.

The relationship is configured as required one-to-one with cascade delete.

The database has unique indexes for normalized email, normalized username, and `UserSettings.UserId`.

The service performs a friendly duplicate pre-check and also catches matching PostgreSQL unique-constraint violations. This protects against two concurrent requests passing the pre-check at the same time.

Because the related entities are saved as one EF Core unit of work, registration persists both records or fails without intentionally leaving only one of them.

---

## Habit CRUD Architecture

Phase 3 follows the same frontend/backend/database boundaries as authentication.

```text
React habit component
        |
        | typed HTTP request through habitsApi and apiClient
        v
HabitsController
        |
        | authenticated user identifier from claims
        v
HabitService
        |
        | owned EF Core query and backend validation
        v
PostgreSQL habits table
```

`HabitsController` stays thin. It receives the route and DTO, derives the authenticated user identifier, calls `HabitService`, and translates the result into the documented HTTP response.

`HabitService` owns:

- text trimming and blank-to-null normalization
- Daily and Weekly target validation
- difficulty and frequency validation through enum-backed contracts
- user ownership filtering
- active-only and include-inactive list behavior
- deterministic list ordering
- owned get-by-ID behavior
- updates for active or inactive owned habits
- idempotent soft deactivation
- DTO mapping

The frontend owns only temporary interaction state such as form values, loading, editing, confirmation, pending, and error display.

After create, update, or deactivation succeeds, `HabitSection` triggers a fresh `GET /api/habits` request. The frontend does not manually treat its local array as the persistent source of truth.

The normal list excludes inactive habits. `includeInactive=true` is available when a later UI needs to display archived habits.

---

## Frontend Structure

The frontend uses React, TypeScript, Vite, Tailwind CSS, Prettier, and Oxlint.

### api

The `api` folder contains functions that call backend endpoints.

Current files include:

- `apiClient.ts`
- `authApi.ts`
- `habitsApi.ts`
- `healthApi.ts`
- `readApiError.ts`

`apiClient.ts` centralizes:

- browser credentials
- state-changing method detection
- antiforgery token acquisition
- antiforgery request headers

Feature API files own endpoint paths, request serialization, response parsing, and API-specific error handling.

Components should not duplicate those details.

### auth

The `auth` folder contains shared authentication state and access.

Current files include:

- `AuthContext.ts`
- `AuthProvider.tsx`
- `useAuth.ts`
- authentication provider tests

`AuthProvider` owns:

- loading the current user when the app starts
- authenticated-user state
- registration, login, and logout state transitions
- authentication loading states
- authentication error state

It does not own password rules, credential validation, cookie contents, or user ownership rules.

### components

The `components` folder contains reusable UI pieces.

Current authentication components include:

- `LoginForm`
- `RegisterForm`

Current habit components include:

- `HabitForm`
- `HabitList`
- `HabitSection`
- `HabitEditForm`
- `HabitDeactivateButton`

Later examples include:

- attribute cards
- progress bars
- dashboard layout components

Components focus on display, local form state, user interaction, and calling the relevant context or API abstraction.

### pages

Route-level pages may be introduced when the application needs navigation between larger screens.

Planned examples:

- dashboard page
- habits page
- settings page

Routing should not be added merely to satisfy a hypothetical structure before it is needed.

### types

The `types` folder contains TypeScript request and response types.

Where a frontend type represents an API contract, it should use the same conceptual name and shape as the backend DTO.

### hooks

Reusable hooks may be added when they provide a clear shared abstraction.

The current authentication access hook is `useAuth`.

Hooks may manage frontend state and API loading behavior, but they must not contain backend business rules.

---

## Testing Architecture

Different test layers protect different risks.

### Backend Unit Tests

Project:

- `HabitTracker.Tests`

Purpose:

- test focused backend behavior and rules
- keep tests fast
- avoid depending on HTTP or a real database when those layers are not under test

### Backend HTTP Integration Tests

Project:

- `HabitTracker.IntegrationTests`

Infrastructure:

- ASP.NET Core `WebApplicationFactory`
- test server hosting the real API pipeline
- EF Core InMemory database substituted only inside the integration host

Purpose:

- test controller routes and status codes
- test authentication cookies and protected endpoints
- test antiforgery behavior
- test Problem Details responses
- test interactions between controller, service, middleware, and persistence boundaries

The integration host removes the production `AppDbContext` registration before adding the test database provider.

### Frontend Tests

Tools:

- Vitest
- React Testing Library

Purpose:

- test form behavior
- test authentication state transitions
- test provider behavior
- test observable UI behavior without a real browser or backend

Frontend test files use `.test.ts` or `.test.tsx`.

### Browser End-to-End Tests

Tool:

- Playwright with Chromium

Purpose:

- exercise the real browser
- start the ASP.NET Core backend and Vite frontend
- use a real PostgreSQL database
- verify the anonymous authentication screen
- verify registration, refresh, logout, and login as one user journey

Local E2E testing uses an isolated ignored environment file and a separate `habit_tracker_e2e` PostgreSQL database so browser tests do not modify the normal development database.

Each E2E run currently creates a unique account. A deliberate cleanup or reset strategy may be added later if accumulated test data becomes inconvenient.

### Continuous Integration

GitHub Actions runs three independent jobs:

```text
Backend
Frontend
End-to-end
```

The Backend job:

- restores .NET dependencies
- builds the solution in Release
- runs backend unit and integration tests

The Frontend job:

- installs exact npm dependencies
- checks formatting
- runs linting
- builds the frontend
- runs Vitest tests

The End-to-end job:

- starts a disposable PostgreSQL service container
- restores the repository-local `dotnet-ef` tool
- restores backend dependencies
- applies the real EF Core migration
- installs frontend dependencies
- installs Chromium and its Linux dependencies
- runs Playwright tests

Temporary CI database credentials belong only to the disposable service container. Local or production database secrets are not stored in the workflow.

---

## Data Flow Example

Completing a habit should follow this flow:

```text
User clicks "Complete"
        |
        v
React calls backend API
        |
        v
CompletionsController receives request
        |
        v
CompletionService validates ownership and completion rules
        |
        v
XpService applies XP rewards
        |
        v
EF Core saves changes to PostgreSQL
        |
        v
Backend returns response DTO
        |
        v
React updates the UI
```

This completion flow is planned for later phases. The same separation already exists in the implemented Phase 2 authentication and Phase 3 habit-management flows.

---

## Browser-Facing Origin and Deployment

The frontend and backend remain separate applications.

Where practical, they should be presented through the same browser-facing origin.

During local development, Vite proxies `/api` requests to ASP.NET Core.

The current local CORS policy allows the Vite development origin. Browser authentication works through the same-origin proxy, so the current development path does not require cross-origin credential handling.

During deployment, the preferred model is:

```text
Public origin
├── React application
└── /api → ASP.NET Core backend
```

A reverse proxy or hosting platform may provide that shared origin.

If deployment requires separate public origins, Phase 8 must deliberately configure:

- the exact allowed frontend origin
- credentialed CORS
- compatible cookie `SameSite` and `Secure` behavior
- HTTPS
- antiforgery behavior
- deployment secrets

The exact deployment provider will be decided later.

---

## Key Architecture Rules

- Keep frontend and backend separate.
- Keep business logic in backend services.
- Keep controllers thin.
- Use DTOs for API requests and responses.
- Do not expose database entities directly.
- Use PostgreSQL as the source of truth.
- Use EF Core for database access.
- Derive user identity from authenticated backend claims.
- Do not accept client-provided user identifiers for user-owned operations.
- Centralize cross-cutting frontend API behavior.
- Write tests alongside the behavior they protect.
- Preserve stable domain names.
- Build in phases.
- Avoid adding out-of-scope features during MVP development.

---

## Architecture Change Rules

Changes to the architecture should be deliberate rather than introduced as unrelated parts of feature work or debugging.

Before changing an established architectural decision:

1. Explain what problem the change solves.
2. Identify the affected frontend, backend, database, API, test, and documentation areas.
3. Consider whether the current architecture can support the requirement without a major change.
4. Document the decision before or alongside the implementation.
5. Make the change in a focused commit.

The architecture may evolve, but it should not drift silently.
