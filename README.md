# Gamified Habit Tracker

A full-stack habit-tracking application that turns completed habits into persistent XP, attribute progression, levels, and streaks.

The backend owns the progression rules and calendar behavior. React presents authoritative API data and sends user actions. PostgreSQL stores the durable state and audit history.

## Current Status

Completed:

- Phase 1 — Project Setup
- Phase 2 — Authentication
- Phase 3 — Habit CRUD
- Phase 4 — Habit Completion
- Phase 5 — XP and Attributes
- Phase 6 — Dashboard and Streaks
- Phase 7 — Game UI Polish

Current:

- Phase 8 — Deployment and Project Polish

The implemented product flow supports:

1. registering or signing in;
2. creating daily or weekly habits;
3. completing a habit for the current user-local date;
4. receiving backend-calculated XP;
5. progressing the mapped attributes;
6. viewing overall level progress and per-habit streaks;
7. undoing a completion without deleting its audit history;
8. deactivating and later reactivating a habit without removing its history.

## Application Areas

### Dashboard

The dashboard is an action-first command center built from `GET /api/dashboard`.

It includes:

- today’s habits and completion actions;
- completed habits for the current local date;
- completion, streak, XP, level, and next-level summaries;
- per-habit streak protection priorities;
- eight attribute progress cards;
- attribute XP distribution;
- adaptive pagination based on available panel height;
- restrained XP-gain and level-up feedback.

### Habits

The habit workspace includes:

- Active Habits and Inactive Habits tabs;
- search;
- category, frequency, and difficulty filters;
- sorting;
- adaptive pagination;
- create and edit forms;
- selected-habit details;
- pending configuration information;
- complete and undo actions;
- guarded deactivation;
- reactivation with history preserved.

Rule-bearing changes to category, frequency, target count, or difficulty are scheduled for the next user-local week boundary. Name and description changes apply immediately.

### Attributes

The attributes command center is built from `GET /api/attributes/overview`.

It includes:

- all eight canonical attributes;
- current XP, level, and next-level progress;
- a native SVG balance radar;
- total attribute XP and balance score;
- strongest and needs-focus attributes;
- recent XP transactions;
- XP distribution;
- a backend-ranked closest-to-level-up queue.

## Architecture

```text
React + TypeScript
        |
        | HTTP / JSON
        v
ASP.NET Core Web API
        |
        | Entity Framework Core
        v
PostgreSQL
```

Core rules:

- The frontend and backend are separate applications.
- The backend owns authentication, authorization, validation, ownership, calendar interpretation, XP, levels, completion, undo, streaks, and aggregate read models.
- PostgreSQL is the persistent source of truth.
- Entity Framework Core handles database access.
- DTOs define the HTTP boundary.
- Controllers stay thin.
- Services own application behavior.
- Database entities are never returned directly.
- React may calculate display percentages from returned values, but it does not recreate domain formulas.

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
- repository-local `dotnet-ef` tool manifest

## Local Development

### Prerequisites

Install:

- Git
- .NET SDK
- Node.js and npm
- PostgreSQL

### 1. Clone the repository

```cmd
git clone https://github.com/fredmikhail/gamified-habit-tracker.git
cd gamified-habit-tracker
```

### 2. Configure PostgreSQL

The API requires:

```text
ConnectionStrings:DefaultConnection
```

For a temporary Windows Command Prompt session:

```cmd
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=habit_tracker;Username=habit_tracker_app;Password=YOUR_PASSWORD
```

Use local PostgreSQL values. Do not commit credentials.

### 3. Restore and migrate

```cmd
dotnet tool restore
dotnet restore server\HabitTracker.sln
dotnet ef database update --project server\HabitTracker.Api\HabitTracker.Api.csproj --startup-project server\HabitTracker.Api\HabitTracker.Api.csproj
```

### 4. Install frontend dependencies

```cmd
cd client
npm ci
cd ..
```

### 5. Start the application

On Windows, the repository launcher starts the API and frontend in separate Command Prompt windows:

```cmd
start-dev.bat
```

PostgreSQL must already be running and the connection string must be configured.

Manual alternative:

```cmd
dotnet watch run --project server\HabitTracker.Api\HabitTracker.Api.csproj
```

In a second terminal:

```cmd
cd client
npm run dev
```

The normal local browser path is:

```text
http://localhost:5173
```

Vite proxies `/api` requests to the ASP.NET Core development endpoint.

## Validation

### Backend

From the repository root:

```cmd
dotnet build server\HabitTracker.sln
dotnet test server\HabitTracker.Tests\HabitTracker.Tests.csproj
dotnet test server\HabitTracker.IntegrationTests\HabitTracker.IntegrationTests.csproj
dotnet ef migrations has-pending-model-changes --project server\HabitTracker.Api\HabitTracker.Api.csproj --startup-project server\HabitTracker.Api\HabitTracker.Api.csproj
```

### Frontend

```cmd
cd client
npm run format:check
npm run lint
npm test
npm run build
```

### Browser test

The Playwright configuration starts its own frontend and backend. Do not leave manual development servers running on the E2E ports.

Create an ignored `client\.env.e2e` file containing local test values:

```text
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=habit_tracker_e2e;Username=YOUR_TEST_USER;Password=YOUR_TEST_PASSWORD"
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5168
VITE_API_PROXY_TARGET=http://localhost:5168
```

The database and user must already exist. Apply migrations from `client`:

```cmd
for /f "usebackq eol=# tokens=1,* delims==" %A in (".env.e2e") do @set "%A=%~B"
dotnet ef database update --project ..\server\HabitTracker.Api\HabitTracker.Api.csproj --startup-project ..\server\HabitTracker.Api\HabitTracker.Api.csproj
npm run test:e2e
```

Local environment files are ignored by Git.

## Continuous Integration

GitHub Actions runs separate Backend, Frontend, and End-to-end jobs.

The pipeline covers:

- backend restore, build, unit tests, and integration tests;
- frontend formatting, linting, tests, and production build;
- EF Core migration execution against a disposable PostgreSQL service;
- the Playwright authentication lifecycle against the running application.

## Repository Structure

```text
gamified-habit-tracker/
├── client/
│   ├── e2e/
│   ├── src/
│   │   ├── api/
│   │   ├── auth/
│   │   ├── components/
│   │   │   ├── attributes/
│   │   │   ├── auth/
│   │   │   ├── dashboard/
│   │   │   ├── habits/
│   │   │   ├── layout/
│   │   │   ├── progression/
│   │   │   ├── theme/
│   │   │   └── ui/
│   │   ├── theme/
│   │   ├── types/
│   │   └── workspace/
│   └── playwright.config.ts
├── server/
│   ├── HabitTracker.Api/
│   │   ├── Controllers/
│   │   ├── Data/
│   │   ├── Domain/
│   │   ├── DTOs/
│   │   ├── Exceptions/
│   │   └── Services/
│   ├── HabitTracker.Tests/
│   └── HabitTracker.IntegrationTests/
├── docs/
├── .github/workflows/
├── start-dev.bat
└── server/HabitTracker.sln
```

## Scope Boundaries

The current MVP does not include:

- advanced scheduling;
- sleep or bad-habit tracking;
- reminders or notifications;
- AI recommendations;
- social features, leaderboards, or public profiles;
- avatars, quests, inventory, or currencies;
- theme marketplace;
- complex analytics;
- native mobile applications;
- payments or administration;
- calendar integration;
- milestones or achievements.

The interface contains no fake controls or fabricated data for deferred systems.

## Documentation

- [Project Overview](docs/PROJECT_OVERVIEW.md)
- [Roadmap](docs/ROADMAP.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Data Model](docs/DATA_MODEL.md)
- [API Contract](docs/API_CONTRACT.md)
- [Naming Conventions](docs/NAMING_CONVENTIONS.md)
