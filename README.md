# Gamified Habit Tracker

A full-stack habit tracking application with RPG-style progression, habit completions, streaks, XP, and character attributes.

The application is being built in deliberate phases, beginning with the secure multi-user foundation and then moving through the core habit-tracking loop. Milestones and achievements are planned as post-MVP features.

## Current Status

- Phase 1 — Project Setup: complete
- Phase 2 — Authentication: complete
- Next phase: Phase 3 — Habit CRUD

The current application supports registration, login, logout, browser-managed authentication sessions, antiforgery protection, authenticated user restoration, PostgreSQL persistence, automated tests, browser end-to-end tests, and continuous integration.

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
- GitHub Actions continuous integration

## Architecture

- The frontend and backend are separate applications.
- The frontend calls backend APIs through HTTP.
- The backend owns authentication and business rules.
- PostgreSQL is the source of truth for persistent data.
- DTOs define the public API boundary.
- Database entities are not returned directly as API responses.

## Documentation

- [Project Overview](docs/PROJECT_OVERVIEW.md)
- [Roadmap](docs/ROADMAP.md)
- [Architecture](docs/ARCHITECTURE.md)
- [Data Model](docs/DATA_MODEL.md)
- [API Contract](docs/API_CONTRACT.md)
- [Naming Conventions](docs/NAMING_CONVENTIONS.md)
