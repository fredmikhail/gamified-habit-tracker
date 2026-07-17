# Project Overview

## Vision

Gamified Habit Tracker is a full-stack habit tracking application that helps users build consistency through habit completion, streaks, XP, and character attributes.

Milestones and achievements are planned as post-MVP progression features.

The app combines personal productivity with RPG-style progression to make daily habit tracking more rewarding and visually engaging.

---

## Current Project Status

Completed phases:

- Phase 1 — Project Setup
- Phase 2 — Authentication

Next phase:

- Phase 3 — Habit CRUD

The current application has a working React frontend, ASP.NET Core backend, PostgreSQL database, secure cookie-based authentication, antiforgery protection, automated tests, browser end-to-end tests, and GitHub Actions continuous integration.

---

## Core Objectives

- Provide a clean habit tracking experience.
- Support multiple users with private habit history and progress.
- Use gamification to make consistency more rewarding.
- Track habits, completions, XP, attributes, and streaks.
- Support milestones and achievements after the MVP.
- Maintain a modular architecture that can support future features.
- Protect user-owned data through backend authentication and ownership checks.
- Keep important application rules testable and continuously verified.

---

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

### Testing

- xUnit for backend unit tests
- ASP.NET Core integration tests with `WebApplicationFactory`
- Vitest for frontend tests
- React Testing Library for component and provider behavior
- Playwright for browser end-to-end tests
- Real PostgreSQL for the end-to-end authentication journey

### Automation

- GitHub Actions continuous integration
- Separate Backend, Frontend, and End-to-end CI jobs
- Repository-local `dotnet-ef` tool manifest for repeatable EF Core commands

---

## Architecture Principles

- Frontend and backend are separate applications.
- Backend owns business logic.
- Frontend displays data and sends user actions to the API.
- PostgreSQL is the source of truth for persistent data.
- Entity Framework Core is used for database access.
- DTOs are used between frontend and backend.
- Database entities are not exposed directly as API responses.
- Core business rules should be implemented in backend services.
- Controllers should remain thin.
- Authentication identity comes from backend-validated claims, not a client-provided user identifier.
- Backend and frontend tests should be added alongside the behavior they protect.
- Browser-facing state-changing requests use antiforgery protection.
- Architecture, naming, and scope changes should be deliberate rather than introduced silently.

---

## Implemented Authentication Foundation

Phase 2 added:

- `User` and `UserSettings` entities
- PostgreSQL migration and database constraints for authentication data
- case-insensitive email and username uniqueness
- application-generated Guid version 7 identifiers
- password hashing and verification
- registration with atomic `User` and `UserSettings` creation
- login with optional fixed 30-day `rememberMe` persistence
- logout
- current-user session restoration
- encrypted, browser-managed authentication cookies
- globally applied antiforgery validation for state-changing controller requests
- centralized frontend API and CSRF-token handling
- frontend authentication state, login UI, and registration UI
- consistent API error handling with Problem Details
- backend unit and HTTP integration tests
- frontend component and authentication-state tests
- Playwright authentication smoke and full journey tests
- real PostgreSQL end-to-end testing
- GitHub Actions continuous integration

The frontend never receives or stores the authentication credential directly.

---

## MVP Scope

The MVP includes:

- User registration
- User login
- User logout
- Default user settings with display name and time zone
- User-specific habit data
- Habit creation
- Habit editing
- Habit deactivation
- Daily habit completion
- Undo habit completion
- Duplicate completion prevention
- XP rewards
- Character attributes
- Calculated user level
- Streak calculation
- Today dashboard
- Basic weekly progress summary

---

## Entity Status

### Implemented MVP Entities

- User
- UserSettings

### Planned MVP Entities

- Habit
- HabitCompletion
- HabitAttributeReward
- UserAttribute
- XpTransaction

### Post-MVP Planned Entities

- Milestone
- UserMilestone

The post-MVP entity names are reserved, but milestone and achievement functionality is not part of the initial MVP.

---

## Core Attributes

Initial character attributes:

- Discipline
- Strength
- Focus
- Recovery
- Career
- Mind
- Social
- Spirituality

---

## Scope Control

The following features are outside the MVP and should be added only after the core habit loop is working:

- Advanced scheduling
- Sleep tracking module
- Bad habit tracking module
- Social features
- Leaderboards
- Reminders
- Notifications
- AI recommendations
- Mobile app
- Payments
- Admin panel
- Complex analytics
- Public profiles
- Avatar systems
- Theme marketplace
- Calendar integration
- Milestones and achievements

---

## First End-to-End Product Milestone

A registered and logged-in user can create a habit, complete it for today, receive XP, and see updated attribute progress on the dashboard.
