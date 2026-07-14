# Architecture

This document describes the planned architecture for the Gamified Habit Tracker app.

The application is built as a full-stack web app with a separate frontend, backend, and database.

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

---

## Main Responsibilities

### Frontend

The frontend is responsible for:

- Displaying pages and components.
- Handling user interactions.
- Calling backend API endpoints.
- Showing loading, success, and error states.
- Rendering habit cards, progress bars, attributes, and dashboard UI.

The frontend should not contain core business logic such as:

- XP calculation
- streak calculation
- habit completion validation
- duplicate completion prevention
- database rules

---

### Backend

The backend is responsible for:

- Exposing HTTP API endpoints.
- Validating important user actions.
- Enforcing user-specific data access.
- Running business logic.
- Calculating XP rewards.
- Calculating streaks.
- Managing habit completions.
- Reading and writing data through Entity Framework Core.

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

---

## Backend Structure

The backend will use ASP.NET Core Web API.

Planned structure:

```text
server/
├── HabitTracker.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── Domain/
│   │   ├── Entities/
│   │   └── Enums/
│   ├── DTOs/
│   ├── Services/
│   ├── Program.cs
│   └── appsettings.json
│
└── HabitTracker.Tests/
    └── HabitTracker.Tests.csproj
```

### Controllers

Controllers expose API endpoints.

They should stay thin.

A controller should usually:

1. Receive the HTTP request.
2. Validate basic request shape.
3. Call the appropriate service.
4. Return an HTTP response.

Controllers should not contain complex business logic.

---

### Services

Services contain application and business logic.

Examples:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

Services should handle decisions such as:

- whether a habit can be completed
- how much XP is awarded
- how attributes are updated
- how streaks are calculated
- what data the dashboard needs

---

### Data

The `Data` folder contains database-related code.

It will include:

- `AppDbContext`
- Entity Framework Core configuration
- migrations

`AppDbContext` represents the connection between C# entity classes and PostgreSQL tables.

---

### Domain

The `Domain` folder contains core domain types.

It includes:

- entities
- enums

Entities represent important business objects such as:

- `User`
- `UserSettings`
- `Habit`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

Enums represent fixed sets of values such as:

- habit difficulty
- habit frequency type
- attribute type

---

### DTOs

DTOs define the data that moves between frontend and backend.

DTO stands for Data Transfer Object.

The API should return DTOs instead of database entities.

This keeps the database model separate from the public API shape.

Examples:

- `CreateHabitRequest`
- `UpdateHabitRequest`
- `HabitResponse`
- `DashboardResponse`
- `AuthResponse`

---

### Tests

The `HabitTracker.Tests` project contains backend tests written with xUnit.

The test project should focus on important application and business rules, including:

- duplicate habit completion prevention
- undo completion behavior
- XP calculations
- attribute updates
- streak calculations
- user data ownership rules where appropriate

The test project is created during Phase 1.

Tests for specific business rules are added alongside the phases that implement those rules.

Tests should verify observable behavior rather than depend heavily on private implementation details.

---

## Authentication Architecture

The browser application uses secure cookie-based authentication.

The frontend does not receive, store, decode, or manually attach authentication credentials.

After successful registration or login, ASP.NET Core creates an encrypted authentication cookie. The browser sends the cookie automatically with later requests.

The authentication flow is:

```text
React frontend
        |
        | HTTP request with browser-managed cookie
        v
ASP.NET Core authentication middleware
        |
        | authenticated user identity
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

### AuthController Responsibilities

`AuthController` handles authentication-related HTTP concerns.

It should:

- receive registration and login requests
- call `AuthService`
- create the authentication session after successful registration or login
- end the authentication session during logout
- receive request DTOs and return response DTOs
- return appropriate HTTP status codes

It should not:

- query users directly
- normalize email or username
- hash or verify passwords
- decide whether registration data is available
- create User or UserSettings directly
- contain account business rules

### AuthService Responsibilities

`AuthService` owns authentication application logic.

It should:

- validate account-related rules not handled by basic DTO validation
- normalize email and username
- check email and username availability
- hash passwords during registration
- verify passwords during login
- create User and UserSettings together
- update LastLoginAtUtc after successful login
- return user data through response DTOs

### Authentication Middleware

ASP.NET Core authentication middleware validates the encrypted authentication cookie and creates the authenticated request identity.

Authorization middleware runs after authentication.

Controllers derive the current User identifier from the authenticated request context and pass it to services that perform user-owned operations.

Services must not trust a UserId supplied by the frontend.

User-owned requests must not accept a client-provided UserId.

### Antiforgery Protection

Because browsers send authentication cookies automatically, state-changing browser requests use antiforgery protection.

The frontend obtains an antiforgery request token from the backend and sends it through a request header for:

- POST
- PUT
- PATCH
- DELETE

Antiforgery handling is centralized in the frontend API layer rather than repeated inside pages or components.

### Browser-Facing Origin

The frontend and backend remain separate applications.

Where practical, they are presented through the same browser-facing origin.

During local development, the Vite development server proxies `/api` requests to the ASP.NET Core backend.

In deployment, a reverse proxy or hosting configuration may expose the React frontend and `/api` backend through one public origin.

---

## Frontend Structure

The frontend will use React, TypeScript, Vite, and Tailwind CSS.

Planned structure:

```text
client/
└── src/
    ├── api/
    ├── components/
    ├── pages/
    ├── types/
    ├── hooks/
    ├── utils/
    ├── App.tsx
    └── main.tsx
```

### api

The `api` folder contains functions that call backend endpoints.

Examples:

- `authApi.ts`
- `habitsApi.ts`
- `dashboardApi.ts`

These files centralize HTTP calls so components do not directly manage endpoint details everywhere.

---

### components

The `components` folder contains reusable UI pieces.

Examples:

- habit cards
- attribute cards
- progress bars
- buttons
- layout components

Components should focus on display and user interaction.

---

### pages

The `pages` folder contains route-level screens.

Examples:

- `LoginPage.tsx`
- `RegisterPage.tsx`
- `DashboardPage.tsx`
- `HabitsPage.tsx`
- `SettingsPage.tsx`

Pages combine components and call hooks/API functions.

---

### types

The `types` folder contains TypeScript types used by the frontend.

These types should match backend DTOs where appropriate.

Examples:

- `HabitResponse`
- `DashboardResponse`
- `UserAttributeResponse`

---

### hooks

The `hooks` folder contains reusable React logic.

Examples:

- `useAuth`
- `useDashboard`
- `useHabits`

Hooks can manage frontend state and API loading behavior, but they should not contain backend business rules.

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
CompletionService validates and completes habit
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

---

## Key Architecture Rules

- Keep frontend and backend separate.
- Keep business logic in backend services.
- Keep controllers thin.
- Use DTOs for API requests and responses.
- Do not expose database entities directly.
- Use PostgreSQL as the source of truth.
- Use EF Core for database access.
- Write backend tests alongside the business rules they protect.
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

---

## Initial Deployment Model

The app may eventually be deployed as separate services:

```text
Frontend hosting
    React static site

Backend hosting
    ASP.NET Core Web API

Database hosting
    PostgreSQL
```

The exact deployment provider will be decided later.