# Roadmap

This roadmap defines the development phases for the Gamified Habit Tracker app.

The goal is to build the application in small, understandable stages while keeping the architecture clean and the core habit-tracking loop stable.

---

## Phase 1 — Project Setup

**Status: Complete**

### Goal

Set up the frontend, backend, and PostgreSQL database so the project has a working foundation.

### Scope

- Create the ASP.NET Core Web API backend.
- Create the React + TypeScript + Vite frontend.
- Add Tailwind CSS.
- Set up PostgreSQL.
- Connect the backend to PostgreSQL.
- Add Entity Framework Core.
- Create the xUnit backend test project.
- Confirm the test project runs successfully.
- Add a simple backend health check endpoint.
- Confirm the frontend can call the backend.

### Definition of Done

- Backend runs locally.
- Frontend runs locally.
- PostgreSQL runs locally.
- Backend can connect to PostgreSQL.
- The xUnit test project runs successfully.
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

**Status: Complete**

### Goal

Allow users to register, log in, log out, restore their authenticated session, and establish the secure identity foundation for private user-specific data.

### Original Feature Scope

- Create the User entity.
- Create the UserSettings entity.
- Create default user settings during registration.
- Store the user's display name and time zone.
- Add password hashing.
- Add registration endpoint.
- Add login endpoint.
- Add secure cookie-based authentication session handling.
- Add antiforgery protection for state-changing requests.
- Add backend logout endpoint.
- Add protected backend endpoints.
- Add frontend login and registration UI.
- Add basic frontend authentication state.

### Implemented Authentication Details

- Use application-generated Guid version 7 identifiers.
- Normalize email and username for case-insensitive lookup and uniqueness.
- Enforce unique normalized email and username indexes in PostgreSQL.
- Create User and UserSettings together during registration.
- Validate IANA time-zone identifiers.
- Use ASP.NET Core password hashing and verification.
- Use an encrypted `HttpOnly` authentication cookie.
- Use a 12-hour non-persistent session by default.
- Use a fixed 30-day persistent session only when `rememberMe` is selected during login.
- Disable sliding authentication expiration during the MVP.
- Add `GET /api/auth/me` for current-user restoration.
- Derive the authenticated user identifier from backend claims.
- Do not accept client-provided user identifiers for user-owned operations.
- Make logout idempotent.
- Return expected API failures through Problem Details responses.
- Centralize frontend HTTP credentials, antiforgery tokens, and API error handling.
- Clear the cached antiforgery request token when authentication identity changes.

### Cross-Cutting Infrastructure Added During Phase 2

These items were not part of the smallest original authentication scope, but were added to make the feature reliable and continuously verifiable:

- Add the ASP.NET Core integration test project.
- Use `WebApplicationFactory` for HTTP integration tests.
- Replace PostgreSQL with EF Core InMemory only inside API integration tests.
- Add Vitest and React Testing Library.
- Add frontend authentication-state and form tests.
- Add Playwright browser testing.
- Add an isolated local PostgreSQL end-to-end database.
- Test the full registration, refresh, logout, and login journey.
- Add a repository-local `dotnet-ef` tool manifest.
- Add GitHub Actions continuous integration.
- Run separate Backend, Frontend, and End-to-end CI jobs.
- Run the end-to-end CI job against a disposable PostgreSQL service container.
- Apply real EF Core migrations before CI browser tests.

### Definition of Done

- A new user can register.
- Registration creates default settings for the new user.
- User and UserSettings registration data is saved atomically.
- The user's display name and time zone are stored.
- An existing user can log in.
- A user can choose a temporary or remembered login session.
- A user can log out.
- The frontend restores the current authenticated user after a refresh.
- Anonymous requests cannot access protected endpoints.
- Authenticated identity is derived from server-side claims rather than a client-provided user identifier.
- Passwords are stored only as password hashes.
- State-changing browser requests require a valid antiforgery token.
- Authentication behavior is covered by backend, frontend, and browser tests.
- GitHub Actions verifies backend, frontend, and PostgreSQL-backed end-to-end behavior.

### Not Included Yet

- Habit CRUD
- Habit completion
- XP
- Streaks
- Dashboard
- Advanced user settings
- General ownership tests for habit resources, which begin when those resources exist in Phase 3

---

## Phase 3 — Habit CRUD

**Status: Next**

### Goal

Allow authenticated users to create and manage their habits.

### Scope

- Create the Habit entity.
- Add habit request and response DTOs.
- Add HabitService.
- Add habit API endpoints.
- Create frontend habit list UI.
- Create frontend habit form.
- Allow users to create habits.
- Allow users to edit habits.
- Allow users to deactivate habits.
- Derive habit ownership from the authenticated user identity.
- Add tests proving users can only view and modify their own habits.

### Definition of Done

- A logged-in user can create a habit.
- A logged-in user can view only their own habits.
- A logged-in user can edit only their own habits.
- A logged-in user can deactivate only their own habit.
- A habit owned by another user is not exposed.
- A habit can be deactivated without deleting its history.
- Habit ownership behavior is covered by backend tests.

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
- Add backend tests for duplicate completion and undo behavior.
- Display completed and incomplete habit states in the frontend.

### Definition of Done

- A user can complete an active habit for today.
- A user cannot complete the same habit twice for the same date.
- A user can undo today's completion.
- Completion records are stored in PostgreSQL.
- Completion logic is handled by the backend.
- Duplicate completion and undo behavior are covered by backend tests.

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
- Extend habit request and response DTOs to support attribute rewards.
- Apply XP rewards when a habit is completed.
- Store XP transactions.
- Add backend tests for XP and attribute calculations.
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
- Habit DTOs support attribute reward configuration.
- The frontend displays attribute progress.
- XP and attribute logic is handled by the backend.
- Core XP and attribute calculations are covered by backend tests.

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
- Add backend tests for streak calculations.
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
- Core streak scenarios are covered by backend tests.

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
- Review and strengthen backend and frontend test coverage.
- Add seed or demo data if useful.
- Deploy the frontend.
- Deploy the backend.
- Deploy the PostgreSQL database.
- Document environment variables.
- Document local setup steps.
- Configure production authentication-cookie security.
- Decide and document the production frontend/API origin model.
- Prefer a shared public origin where practical.
- Configure explicit production CORS and credential behavior if separate public origins are required.
- Protect deployment secrets.
- Confirm production migrations and startup behavior.

### Definition of Done

- The app can be run locally from documented instructions.
- The app is deployed.
- The GitHub repository is organized.
- The README explains the project clearly.
- Core backend and frontend behavior has appropriate test coverage.
- Production authentication and antiforgery behavior is verified.
- Deployment configuration does not expose secrets.

---

## Future Add-ons

These features are intentionally outside the MVP and should only be added after the core app is stable.

- Advanced scheduling
- Sleep tracking
- Bad habit tracking
- Journal entries
- Daily quests
- Milestones and achievements
- Reminders
- Notifications
- Social/accountability features
- Leaderboards
- Public profiles
- Advanced analytics
- Mobile app
- AI recommendations
- Admin panel
