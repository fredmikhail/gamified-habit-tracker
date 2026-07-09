# Project Overview

## Vision

Gamified Habit Tracker is a full-stack habit tracking application that helps users build consistency through habit completion, streaks, XP, milestones, and character attributes.

The app combines personal productivity with RPG-style progression to make daily habit tracking more rewarding and visually engaging.

---

## Core Objectives

- Provide a clean habit tracking experience.
- Support multiple users with private habit history and progress.
- Use gamification to make consistency more rewarding.
- Track habits, completions, XP, attributes, streaks, and milestones.
- Maintain a modular architecture that can support future features.

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
- Frontend testing may be added later

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

---

## MVP Scope

The MVP includes:

- User registration
- User login
- User-specific habit data
- Habit creation
- Habit editing
- Habit deactivation
- Daily habit completion
- Undo habit completion
- Duplicate completion prevention
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

---

## First Milestone

A logged-in user can create a habit, complete it for today, gain XP, and see attribute progress increase on the dashboard.
