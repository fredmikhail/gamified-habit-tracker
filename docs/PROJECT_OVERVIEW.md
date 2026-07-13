# Project Overview

## Vision

Gamified Habit Tracker is a full-stack habit tracking application that helps users build consistency through habit completion, streaks, XP, and character attributes.

Milestones and achievements are planned as post-MVP progression features.

The app combines personal productivity with RPG-style progression to make daily habit tracking more rewarding and visually engaging.

---

## Core Objectives

- Provide a clean habit tracking experience.
- Support multiple users with private habit history and progress.
- Use gamification to make consistency more rewarding.
- Track habits, completions, XP, attributes, and streaks.
- Support milestones and achievements after the MVP.
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
- Backend tests should be added alongside the business rules they protect.

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

## Planned Entities

### MVP Entities

- User
- UserSettings
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
