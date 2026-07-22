# Gamified Habit Tracker

A full-stack habit tracking application that converts completed habits into persistent XP, attribute progression, levels, and streaks.

The system is designed around a backend-owned progression model. React displays the current state and sends user actions, while ASP.NET Core validates those actions, applies the domain rules, and persists the resulting history in PostgreSQL.

## Current Status

Completed:

- Phase 1 — Project Setup
- Phase 2 — Authentication
- Phase 3 — Habit CRUD
- Phase 4 — Habit Completion
- Phase 5 — XP and Attributes
- Phase 6 — Dashboard and Streaks

Next:

- Phase 7 — Game UI Polish

The main application flow is functional end to end:

1. Register or log in.
2. Create a daily or weekly habit.
3. Complete the habit for the current local date.
4. Receive backend-calculated XP.
5. Increase the habit’s mapped attributes.
6. View overall level progress, daily execution, and habit streaks.
7. Undo the completion without deleting its audit history.
8. Recalculate XP, attributes, dashboard values, and streaks immediately.

The current interface is functional and fully connected to the backend. Phase 7 will replace the existing visual scaffolding with the final application shell and design system.

## Implemented Functionality

### Authentication

- User registration
- Login and logout
- Authenticated-session restoration after refresh
- Optional remembered sessions
- Encrypted authentication cookies
- `HttpOnly` cookies
- `SameSite=Lax`
- Secure cookies outside local development
- Non-sliding session expiration
- Antiforgery protection for state-changing requests
- Antiforgery tokens sent through the `X-CSRF-TOKEN` header
- User identity resolved from authenticated backend claims
- User-owned data isolation

### Habit Management

- Create habits
- View active habits
- Edit habits
- Soft-deactivate habits
- Daily and weekly frequencies
- Controlled habit categories
- Easy, Medium, Hard, and Elite difficulties
- Daily target validation
- Weekly target validation
- Ownership validation on every habit operation
- Immediate name and description updates
- Scheduled rule changes
- Display of pending habit configuration changes

Habit rule changes are effective-dated instead of rewriting history.

The following fields are versioned:

- category,
- frequency,
- target count,
- difficulty.

A rule edit is scheduled for the next user-local week boundary. Until then, the current configuration remains active and the new configuration is exposed as pending.

Each configuration version uses a half-open effective period:

```text
EffectiveFromDate <= date < EffectiveToDateExclusive
```

The current version has no end date. Historical versions retain their original effective periods.

### Habit Configuration History

`HabitConfigurationVersion` preserves the rules that were active at different points in a habit’s lifetime.

This allows the backend to answer historical questions correctly:

- Which frequency was active when a completion occurred?
- What weekly target applied during a past week?
- Did a later difficulty change affect previously awarded XP?
- Did a daily-to-weekly change begin a new streak series?
- Which configuration should be used when a completion is undone?

Each `HabitCompletion` stores the `HabitConfigurationVersionId` that was effective when the completion was created.

Historical completions therefore remain tied to the rule set that produced them.

### Habit Completion

- Complete an active habit for the current user-local date
- Prevent completion of another user’s habit
- Prevent completion of inactive habits
- Prevent more than one active completion per habit and local date
- Calculate the local completion date in the backend
- Ignore client-supplied completion dates
- Store the UTC completion timestamp
- Link each completion to its effective configuration version
- Award XP and attributes atomically with the completion
- Return earned XP and attribute changes to the frontend

Completion uniqueness applies only to active, non-undone completions. A user may undo a completion and complete the same habit again for the same local date.

### Auditable Undo

Undo is implemented as a state change, not a deletion.

When a completion is undone:

- the `HabitCompletion` row remains in the database,
- `UndoneAtUtc` records when the undo occurred,
- the original positive XP transactions remain,
- matching negative XP transactions are appended,
- user attribute totals are reversed,
- overall XP is reversed,
- today’s activity is recalculated,
- daily execution is recalculated,
- streaks are recalculated.

The XP ledger is append-only. Historical transactions are not edited or deleted to represent an undo.

This preserves an auditable record of the original action and its reversal.

### XP and Attributes

Habit completion awards XP according to:

- habit difficulty,
- primary attribute mapping,
- secondary attribute mapping.

The eight attributes are:

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

Each habit category maps to two attributes:

- primary attribute: 70% of the reward,
- secondary attribute: 30% of the reward.

The backend owns all reward calculations. The frontend does not calculate XP, attribute rewards, levels, or reversals.

### Dashboard

The authenticated dashboard is returned by:

```text
GET /api/dashboard
```

The response includes:

- total user XP,
- current overall level,
- XP earned within the current level,
- XP required for the next level,
- the user’s current local date,
- today’s active completion count,
- XP earned today,
- completed active daily habits,
- total active daily habits,
- current streak for each active habit,
- longest streak for each active habit,
- the current effective frequency for each streak.

`DashboardService` aggregates the persisted progression state.

`StreakService` calculates streaks from:

- effective-dated habit configurations,
- active non-undone completions,
- the user’s local date,
- the user’s configured week start.

Streak values are derived when requested. They are not stored as mutable counters.

The frontend refreshes dashboard data after:

- habit creation,
- habit editing,
- habit deactivation,
- habit completion,
- completion undo.

### Calendar Settings

Each user has calendar settings stored in `UserSettings`:

- IANA time zone,
- preferred first day of the week.

The default week start is Monday.

The backend derives the current local date from:

```text
UTC timestamp + stored IANA time zone
```

The client does not decide which calendar date receives a completion.

Weekly periods are calculated from the user’s `WeekStartsOn` preference through the shared calendar-period logic.

## Streak Rules

Streaks are calculated per habit. The application does not invent a single combined streak across unrelated habits.

Each active habit returns:

- `CurrentStreak`
- `LongestStreak`

Daily streaks use days as their unit. Weekly streaks use weeks.

### Daily Habits

A daily period is successful when the habit has an active, non-undone completion on that user-local date.

Rules:

- Completing the habit today immediately counts toward the current streak.
- An incomplete current day does not break the streak before that day has ended.
- When today is incomplete, the current streak may continue through yesterday.
- A missed past day breaks the current streak.
- The longest streak is the longest consecutive run of successful eligible days.
- Completion dates before the active frequency segment are ignored.
- Future completion dates are ignored.

### Weekly Habits

A weekly period is successful when the number of active completions in that week reaches the target that was effective for that week.

Rules:

- Week boundaries follow the user’s configured first day of the week.
- Reaching the weekly target early counts immediately.
- An incomplete current week does not break the streak before the week ends.
- A failed completed week breaks the streak.
- Historical weeks use their historical target counts.
- A later target change does not reinterpret earlier weeks.
- The first partial week is treated as a grace period when its target is not reached.
- A successful first partial week still counts toward the streak.
- Future completion dates are ignored.

### Configuration Changes and Streaks

Category and difficulty changes do not reset a streak.

Weekly target changes preserve the existing streak series, while each week continues to use the target that was effective during that period.

A frequency change starts a new streak series:

```text
Daily → Weekly
Weekly → Daily
```

Earlier streak history remains available in the persisted completion and configuration records, but it does not carry into the new frequency segment.

### Undo and Deactivation

Undone completions never count toward streak calculations.

Undoing a completion immediately recalculates both the current and longest streak.

Deactivated habits are excluded from the active dashboard and therefore do not expose an active current streak. Their completion and configuration history remains stored.

## Reward Rules

Difficulty determines the total XP awarded for a completion:

| Difficulty |  XP |
| ---------- | --: |
| Easy       |  10 |
| Medium     |  20 |
| Hard       |  30 |
| Elite      |  50 |

The total is divided between the mapped attributes:

| Reward role         | Share |
| ------------------- | ----: |
| Primary attribute   |   70% |
| Secondary attribute |   30% |

XP allocation, transaction creation, attribute updates, and undo reversals occur in the backend.

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

The main architectural rules are:

- The frontend and backend are separate applications.
- The frontend communicates with the backend through HTTP APIs.
- The backend owns validation and business rules.
- PostgreSQL is the source of truth.
- Entity Framework Core handles database access.
- DTOs define the HTTP boundary.
- Database entities are not returned directly from controllers.
- Controllers remain thin.
- Services contain application and domain behavior.
- User identity comes from authenticated backend claims.
- XP, attributes, levels, completion dates, configuration history, undo behavior, and streak calculations stay out of React.

## Core Backend Services

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

## Technology

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

### Testing and Quality

- xUnit
- ASP.NET Core `WebApplicationFactory`
- EF Core InMemory provider for isolated service tests
- PostgreSQL-backed integration and browser testing
- Vitest
- React Testing Library
- Playwright
- Prettier
- Oxlint
- GitHub Actions

## Repository Structure

```text
gamified-habit-tracker/
├── client/
│   ├── src/
│   │   ├── api/                    Frontend API clients
│   │   ├── components/             React components
│   │   ├── types/                  TypeScript API contracts
│   │   └── test/                   Frontend test setup
│   └── tests/                      Playwright browser tests
├── server/
│   ├── HabitTracker.Api/
│   │   ├── Controllers/            HTTP endpoints
│   │   ├── Data/                   EF Core context, mappings, and migrations
│   │   ├── Domain/                 Entities, enums, and value objects
│   │   ├── DTOs/                   API request and response contracts
│   │   ├── Exceptions/             Application exceptions
│   │   └── Services/               Application and domain services
│   ├── HabitTracker.Tests/         Backend unit tests
│   └── HabitTracker.IntegrationTests/
├── docs/                            Technical documentation
├── .github/workflows/               Continuous integration
└── dotnet-tools.json                Local .NET tool manifest
```

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

The API expects a connection string named:

```text
ConnectionStrings:DefaultConnection
```

For a temporary Windows Command Prompt session:

```cmd
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=habit_tracker;Username=habit_tracker_app;Password=YOUR_PASSWORD
```

Replace the values with the local PostgreSQL configuration.

### 3. Restore backend tools and dependencies

```cmd
dotnet tool restore
dotnet restore
```

### 4. Apply database migrations

```cmd
dotnet ef database update --project server\HabitTracker.Api --startup-project server\HabitTracker.Api
```

### 5. Install frontend dependencies

```cmd
cd client
npm install
cd ..
```

### 6. Start the backend

```cmd
dotnet run --project server\HabitTracker.Api --launch-profile https
```

The HTTPS development endpoint is:

```text
https://localhost:7287
```

### 7. Start the frontend

Open a second terminal:

```cmd
cd client
npm run dev
```

The Vite development server normally starts at:

```text
http://localhost:5173
```

Vite proxies `/api` requests to the ASP.NET Core HTTPS endpoint.

## Validation

### Backend tests

From the repository root:

```cmd
dotnet test server\HabitTracker.Tests\HabitTracker.Tests.csproj
dotnet test server\HabitTracker.IntegrationTests\HabitTracker.IntegrationTests.csproj
```

### Backend build

```cmd
dotnet build server\HabitTracker.Api\HabitTracker.Api.csproj
```

### Migration consistency

```cmd
dotnet ef migrations has-pending-model-changes --project server\HabitTracker.Api --startup-project server\HabitTracker.Api
```

### Frontend checks

```cmd
cd client
npm run format:check
npm run lint
npm run test
npm run build
```

### Browser tests

```cmd
cd client
npm run test:e2e
```

The browser test environment uses Playwright with a PostgreSQL-backed API.

## Continuous Integration

GitHub Actions validates changes on pushes and pull requests.

The automated checks cover:

- backend restore and build,
- backend unit tests,
- backend integration tests,
- frontend dependency installation,
- formatting checks,
- linting,
- frontend component and API tests,
- production frontend build,
- browser-level authentication and application workflows,
- PostgreSQL-backed test execution.

## Roadmap

### Phase 7 — Game UI Polish

Planned work:

- application shell,
- sidebar and top navigation,
- final color and typography system,
- dashboard layout refinement,
- habit workspace refinement,
- attribute presentation,
- responsive layouts,
- accessible interaction states,
- loading and empty states,
- restrained transitions and motion.

The Phase 7 work will use the existing backend contracts and progression rules rather than moving business logic into React.

### Phase 8 — Deployment and Project Polish

Planned work:

- production hosting,
- production PostgreSQL configuration,
- environment-variable documentation,
- deployment validation,
- production cookie and antiforgery verification,
- logging and error-handling review,
- final documentation review,
- application screenshots,
- release preparation.

## MVP Boundaries

The following remain outside the MVP:

- avatars,
- quests,
- inventory,
- currencies,
- leaderboards,
- social features,
- notifications,
- reminders,
- AI recommendations,
- public profiles,
- advanced scheduling,
- sleep tracking,
- bad-habit tracking,
- mobile applications,
- payments,
- administration panel,
- complex analytics,
- theme marketplace,
- calendar integration.

Milestones and achievements remain reserved for post-MVP work.

## Documentation

- [Project Overview](docs/PROJECT_OVERVIEW.md)
- [Roadmap](docs/ROADMAP.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Data Model](docs/DATA_MODEL.md)
- [API Contract](docs/API_CONTRACT.md)
- [Naming Conventions](docs/NAMING_CONVENTIONS.md)
