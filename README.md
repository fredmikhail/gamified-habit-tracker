# Gamified Habit Tracker

A full-stack habit tracking application with RPG-style progression, habit completions, streaks, XP, and character attributes.

The application is being built in deliberate phases, beginning with a secure multi-user foundation and then moving through the core habit-tracking and progression loop. Milestones and achievements are planned as post-MVP features.

## Current Status

- Phase 1 — Project Setup: complete
- Phase 2 — Authentication: complete
- Phase 3 — Habit CRUD: complete
- Phase 4 — Habit Completion: complete
- Next phase: Phase 5 — XP and Attributes

The current application supports registration, login, logout, authenticated-session restoration, antiforgery protection, private user-owned habit management, and daily habit completion. Authenticated users can create, list, edit, soft-deactivate, complete, and undo today's completion through the React frontend, ASP.NET Core API, Entity Framework Core, and PostgreSQL. The backend derives each completion date from the user's stored time zone and exposes the current completion state through `isCompletedToday`.

The current frontend is functional scaffolding. Phase 7 includes a substantial game-UI redesign with a modern, futuristic, intuitive visual system, responsive layouts, progression feedback, and purposeful animations while preserving backend ownership of business rules.

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

- xUnit backend unit tests
- ASP.NET Core integration tests
- Vitest
- React Testing Library
- Playwright browser tests
- Prettier formatting
- Oxlint linting
- GitHub Actions continuous integration

## Architecture

- The frontend and backend are separate applications.
- The frontend calls backend APIs through HTTP.
- The backend owns authentication, validation, ownership, and business rules.
- PostgreSQL is the source of truth for persistent data.
- DTOs define the public API boundary.
- Database entities are not returned directly as API responses.
- Controllers stay thin and delegate application logic to services.

## Documentation

- [Project Overview](docs/PROJECT_OVERVIEW.md)
- [Roadmap](docs/ROADMAP.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Data Model](docs/DATA_MODEL.md)
- [API Contract](docs/API_CONTRACT.md)
- [Naming Conventions](docs/NAMING_CONVENTIONS.md)
