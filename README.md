# Gamified Habit Tracker

A full-stack habit tracker that turns real-world consistency into character progression.

I’m building this project to create something I would genuinely use while sharpening my C#, ASP.NET Core, React, TypeScript, database, and testing skills. The goal is not to throw game terminology on top of a to-do list. The goal is to make long-term progress feel visible and rewarding without making the app childish or gimmicky.

Users create habits, complete them, earn XP, and grow character attributes based on the kind of work they are doing.

## Current Status

Completed:

- Phase 1 — Project Setup
- Phase 2 — Authentication
- Phase 3 — Habit CRUD
- Phase 4 — Habit Completion
- Phase 5 — XP and Attributes

Next:

- Phase 6 — Dashboard and Streaks

The core progression loop is working end to end:

1. Register or log in.
2. Create a habit.
3. Complete it for today.
4. Receive backend-calculated XP.
5. See the correct character attributes increase.
6. Refresh without losing progress.
7. Undo the completion and reverse its XP.

The current frontend is functional scaffolding. The larger visual redesign comes in Phase 7 after the dashboard and streak data are in place.

## What Works Today

### Authentication

- Registration
- Login and logout
- Optional remembered sessions
- Authenticated-session restoration after refresh
- Encrypted `HttpOnly` authentication cookies
- Antiforgery protection for state-changing requests
- Private user-owned data

### Habits

- Create habits
- View active habits
- Edit habits
- Soft-deactivate habits
- Daily and weekly frequency rules
- Easy, Medium, Hard, and Elite difficulty
- Controlled habit categories
- Ownership protection

### Completion

- Complete an active habit for the user’s current local date
- Prevent duplicate completion for the same habit and date
- Undo today’s completion
- Preserve completion dates using the user’s stored IANA time zone
- Keep completion logic in the backend

### XP and Attributes

- Automatic rewards based on habit category and difficulty
- 70/30 primary and secondary attribute split
- Persistent XP transactions
- Persistent user attribute progress
- Backend-calculated levels
- Immediate earned-XP feedback
- Full XP reversal when today’s completion is undone

The eight character attributes are:

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

## Reward Rules

Difficulty determines the total XP awarded:

| Difficulty | XP |
|---|---:|
| Easy | 10 |
| Medium | 20 |
| Hard | 30 |
| Elite | 50 |

Each habit category maps to two attributes:

- Primary attribute: 70%
- Secondary attribute: 30%

The backend owns the reward calculation. The frontend only displays the values returned by the API.

## Product Direction

The long-term visual direction is a desktop-first personal-development command center.

Think premium productivity software with a restrained anime-inspired progression system:

- dark midnight surfaces,
- electric blue and violet progression,
- subtle gold for streaks and achievements,
- clear daily actions,
- strong typography,
- attribute cards,
- level and XP progress,
- original atmospheric artwork,
- restrained motion.

It should feel focused and rewarding, not noisy, casino-like, or overloaded with fake game systems.

The approved design direction is documented in [UI Vision](docs/UI_VISION.md).

## Tech Stack

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
- Vitest
- React Testing Library
- Playwright
- Prettier
- Oxlint
- GitHub Actions

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

The main rules are simple:

- The frontend displays data and sends user actions.
- The backend owns business logic.
- PostgreSQL is the source of truth.
- Controllers stay thin.
- Services own application behavior.
- DTOs define the API boundary.
- Database entities are never returned directly.
- User identity comes from authenticated backend claims.
- XP, completion, level, ownership, date, and future streak logic stay out of React.

## Core Backend Services

Implemented:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`

Planned for Phase 6:

- `StreakService`
- `DashboardService`

## Getting Started

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

### 2. Configure the PostgreSQL connection

The API expects a connection string named:

```text
ConnectionStrings:DefaultConnection
```

For a temporary Windows Command Prompt session:

```cmd
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=habit_tracker;Username=habit_tracker_app;Password=YOUR_PASSWORD
```

Use your own local PostgreSQL values.

### 3. Restore tools and apply migrations

```cmd
dotnet tool restore
dotnet restore
dotnet ef database update --project server\HabitTracker.Api --startup-project server\HabitTracker.Api
```

### 4. Start the backend

```cmd
dotnet run --project server\HabitTracker.Api --launch-profile https
```

The local API uses:

```text
https://localhost:7287
```

### 5. Start the frontend

Open a second terminal:

```cmd
cd client
npm install
npm run dev
```

The Vite development server normally runs at:

```text
http://localhost:5173
```

Vite proxies `/api` requests to the ASP.NET Core backend.

## Running Tests

### Backend

From the repository root:

```cmd
dotnet test
```

### Frontend

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

The end-to-end setup uses Playwright and a PostgreSQL-backed environment. GitHub Actions also runs the browser journey against a disposable PostgreSQL service container.

## Repository Structure

```text
gamified-habit-tracker/
├── client/                         React frontend
├── server/
│   ├── HabitTracker.Api/           ASP.NET Core API
│   ├── HabitTracker.Tests/         Backend unit tests
│   └── HabitTracker.IntegrationTests/
├── docs/                           Project documentation
├── .github/workflows/              Continuous integration
└── dotnet-tools.json               Local .NET tool manifest
```

## Roadmap

### Phase 6 — Dashboard and Streaks

Next up:

- overall user XP and level,
- current and longest streaks,
- today’s completion summary,
- recent activity,
- basic weekly progress,
- aggregated dashboard API,
- first functional dashboard screen.

Streak rules will be designed and documented before implementation. React will not calculate them.

### Phase 7 — Game UI Polish

- application shell,
- sidebar and top bar,
- premium dark visual system,
- polished dashboard,
- refined habit workspace,
- attribute character sheet,
- weekly review,
- responsive layouts,
- subtle motion,
- accessibility polish.

### Phase 8 — Deployment and Project Polish

- production deployment,
- environment documentation,
- screenshots,
- final README polish,
- portfolio presentation,
- production security verification.

## MVP Boundaries

The following are deliberately outside the MVP:

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
- mobile app,
- payments,
- admin panel.

Milestones and achievements are reserved for post-MVP work.

## Documentation

- [Project Overview](docs/PROJECT_OVERVIEW.md)
- [Roadmap](docs/ROADMAP.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Data Model](docs/DATA_MODEL.md)
- [API Contract](docs/API_CONTRACT.md)
- [Naming Conventions](docs/NAMING_CONVENTIONS.md)
- [UI Vision](docs/UI_VISION.md)

## Current Milestone

The first major product milestone is complete:

> A user can create a habit, complete it, earn XP, see attribute progress, refresh safely, and undo the completion with its XP reversal.

The next milestone is making all of that progress understandable at a glance from the dashboard.
