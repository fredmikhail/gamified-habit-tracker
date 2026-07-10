# Roadmap

This roadmap defines the planned development phases for the Gamified Habit Tracker app.

The goal is to build the application in small, understandable stages while keeping the architecture clean and the core habit-tracking loop stable.

---

## Phase 1 — Project Setup

### Goal

Set up the frontend, backend, and PostgreSQL database so the project has a working foundation.

### Scope

- Create the ASP.NET Core Web API backend.
- Create the React + TypeScript + Vite frontend.
- Add Tailwind CSS.
- Set up PostgreSQL.
- Connect the backend to PostgreSQL.
- Add Entity Framework Core.
- Add a simple backend health check endpoint.
- Confirm the frontend can call the backend.

### Definition of Done

- Backend runs locally.
- Frontend runs locally.
- PostgreSQL runs locally.
- Backend can connect to PostgreSQL.
- Frontend can call a backend test endpoint.

### Not Included Yet

- Authentication
- Habit CRUD
- Habit completion
- XP
- Streaks
- Dashboard

---

## Phase 2 — Authentication

### Goal

Allow users to register, log in, log out, and access private user-specific data.

### Scope

- Create the User entity.
- Add password hashing.
- Add registration endpoint.
- Add login endpoint.
- Add authentication token handling.
- Add protected backend endpoints.
- Add frontend login and register pages.
- Add basic frontend auth state.

### Definition of Done

- A new user can register.
- An existing user can log in.
- A user can log out.
- Protected backend endpoints require authentication.
- User-specific data cannot be accessed by another user.

### Not Included Yet

- Habit completion
- XP
- Streaks
- Dashboard
- Advanced user settings

---

## Phase 3 — Habit CRUD

### Goal

Allow authenticated users to create and manage their habits.

### Scope

- Create the Habit entity.
- Add habit request and response DTOs.
- Add HabitService.
- Add habit API endpoints.
- Create frontend habit list page.
- Create frontend habit form.
- Allow users to create habits.
- Allow users to edit habits.
- Allow users to deactivate habits.

### Definition of Done

- A logged-in user can create a habit.
- A logged-in user can view only their own habits.
- A logged-in user can edit their own habits.
- A logged-in user can deactivate a habit without deleting its history.

### Not Included Yet

- Habit completion
- XP rewards
- Streak calculation
- Advanced scheduling

---

## Phase 4 — Habit Completion

### Goal

Allow users to complete habits for the current day.

### Scope

- Create the HabitCompletion entity.
- Add CompletionService.
- Add endpoint for completing a habit.
- Add endpoint for undoing today's completion.
- Prevent duplicate completions for the same habit and date.
- Display completed and incomplete habit states in the frontend.

### Definition of Done

- A user can complete an active habit for today.
- A user cannot complete the same habit twice for the same date.
- A user can undo today's completion.
- Completion records are stored in PostgreSQL.
- Completion logic is handled by the backend.

### Not Included Yet

- XP rewards
- Streak calculation
- Advanced history views

---

## Phase 5 — XP and Attributes

### Goal

Reward users with XP and attribute progress when habits are completed.

### Scope

- Create UserAttribute entity.
- Create HabitAttributeReward entity.
- Create XpTransaction entity.
- Add XpService.
- Add AttributeService.
- Define the initial character attributes.
- Apply XP rewards when a habit is completed.
- Store XP transactions.
- Display attribute progress in the frontend.

### Initial Attributes

- Discipline
- Strength
- Focus
- Recovery
- Career
- Mind
- Social
- Spirituality

### Definition of Done

- Completing a habit creates XP transactions.
- Completing a habit updates the correct user attributes.
- Attribute XP is stored in PostgreSQL.
- The frontend displays attribute progress.
- XP and attribute logic is handled by the backend.

### Not Included Yet

- Milestone unlocking
- Complex XP multipliers
- Advanced analytics

---

## Phase 6 — Dashboard and Streaks

### Goal

Create the main dashboard where users can see daily progress, XP, attributes, and streaks.

### Scope

- Add DashboardService.
- Add StreakService.
- Add dashboard API endpoint.
- Calculate current streaks.
- Calculate basic weekly progress.
- Display today's habits.
- Display completed and remaining habits.
- Display user level.
- Display attribute cards.
- Display streak information.

### Definition of Done

- The dashboard shows today's habits.
- The dashboard shows completion progress.
- The dashboard shows user XP and level.
- The dashboard shows character attributes.
- The dashboard shows streak information.
- Streak logic is handled by the backend.

### Not Included Yet

- Advanced charts
- Advanced analytics
- Notifications
- Social features

---

## Phase 7 — Game UI Polish

### Goal

Make the app feel visually rewarding and game-like while preserving the existing backend architecture.

### Scope

- Improve the dashboard layout.
- Add modern dark UI styling.
- Add RPG-style habit cards.
- Add XP progress bars.
- Add attribute progress bars.
- Add simple completion animations.
- Add level-up feedback.
- Add milestone badge UI when milestones are implemented.

### Definition of Done

- The app feels visually polished.
- Habit completion gives clear visual feedback.
- XP and attribute progress are easy to understand.
- The UI works well on desktop and mobile.
- Visual polish does not move business logic into the frontend.

### Not Included Yet

- Avatar systems
- Theme marketplace
- Complex animations
- Social UI

---

## Phase 8 — Deployment and Project Polish

### Goal

Prepare the application for real usage and public demonstration.

### Scope

- Improve the README.
- Add screenshots.
- Add setup instructions.
- Add backend tests.
- Add seed/demo data if useful.
- Deploy the frontend.
- Deploy the backend.
- Deploy PostgreSQL database.
- Document environment variables.
- Document local setup steps.

### Definition of Done

- The app can be run locally from documented instructions.
- The app is deployed.
- The GitHub repository is organized.
- The README explains the project clearly.
- Core backend logic has test coverage.

---

## Future Add-ons

These features are intentionally outside the MVP and should only be added after the core app is stable.

- Advanced scheduling
- Sleep tracking
- Bad habit tracking
- Journal entries
- Daily quests
- Advanced milestones
- Reminders
- Notifications
- Social/accountability features
- Leaderboards
- Public profiles
- Advanced analytics
- Mobile app
- AI recommendations
- Admin panel