# Gamified Habit Tracker — Project Bible

## Vision

A full-stack gamified habit tracking app that helps users build better habits through RPG-style progression, streaks, XP, milestones, and character attributes.

The app should be useful as a personal productivity tool and strong enough to showcase full-stack development skills for junior software engineering roles.

---

## Core Objectives

1. Build a working full-stack application.
2. Strengthen fundamentals in C#, .NET, React, TypeScript, PostgreSQL, and full-stack architecture.
3. Create a clean portfolio project that can be explained clearly in interviews.
4. Build a real tool that helps users track habits and improve consistency.

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

- xUnit for backend tests
- Frontend tests may be added later

---

## Architecture Rules

1. Frontend and backend are separate applications.
2. Backend owns business logic.
3. Frontend displays data and sends user actions to the API.
4. PostgreSQL is the source of truth for persistent data.
5. Entity Framework Core is used for database access.
6. DTOs are used between frontend and backend.
7. Database entities should not be exposed directly as API responses.
8. Business logic should live in backend services, not React components.

---

## MVP Scope

The MVP should include:

- User registration
- User login
- User-specific habit data
- Create habits
- Edit habits
- Deactivate habits
- Complete habits for today
- Undo today's completion
- Prevent duplicate completions for the same habit/date
- XP rewards
- Character attributes
- User level
- Streak calculation
- Today dashboard
- Basic weekly progress summary

---

## Core Entities

- User
- UserSettings
- Habit
- HabitCompletion
- HabitAttributeReward
- UserAttribute
- XpTransaction
- Milestone
- UserMilestone

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

## Anti-Drift Rules

1. Do not change the tech stack without documenting the reason.
2. Do not rename entities, DTOs, services, folders, or API routes casually.
3. Do not move business logic into the frontend.
4. Do not add features outside the current roadmap phase unless explicitly approved.
5. Prefer the smallest fix during debugging.
6. Every bug fix should preserve the existing architecture.
7. Every new feature should have a clear user story and definition of done.
8. If an AI suggestion conflicts with this document, reject it or ask for a compliant alternative.

---

## Out of Scope for MVP

These features should not be built until the core app works:

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

---

## First Milestone

A logged-in user can create a habit, complete it for today, gain XP, and see attribute progress increase on the dashboard.
