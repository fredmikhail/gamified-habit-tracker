# Gamified Habit Tracker — Project Overview

**Project status:** Active development
**Completed phases:** 1 through 5
**Next phase:** Phase 6 — Dashboard and Streaks
**Primary goal:** Build a polished, portfolio-ready, full-stack habit tracker with meaningful game progression
**UI direction:** Premium desktop productivity software with restrained anime-inspired progression
**Implementation principle:** Future-compatible, not future-built

---

## 1. Vision

Gamified Habit Tracker is a full-stack habit-tracking application that turns real-world consistency into persistent character growth.

Users create habits, complete them in daily life, earn deterministic XP, and develop character attributes that reflect the kinds of actions they repeatedly perform.

The product combines:

- personal productivity,
- long-term habit consistency,
- backend-owned progression,
- character attributes,
- overall levels,
- streaks,
- weekly review,
- premium game-inspired presentation.

The intended final experience is:

> A personal-development command center that presents real habit data as elegant character progression.

The interface should feel sophisticated, focused, rewarding, and original.

It should not feel childish, casino-like, noisy, manipulative, or dependent on fake systems.

---

## 2. Current Project Status

The project has completed:

- Phase 1 — Project Setup
- Phase 2 — Authentication
- Phase 3 — Habit CRUD
- Phase 4 — Habit Completion
- Phase 5 — XP and Attributes

The next phase is:

- Phase 6 — Dashboard and Streaks

The current application already supports the complete core progression loop:

1. A user registers.
2. The user logs in.
3. The user creates a habit.
4. The backend calculates the habit’s automatic XP rewards.
5. The user completes the habit for today.
6. PostgreSQL stores the completion.
7. XP transactions are created.
8. User attribute XP is updated.
9. The frontend shows the exact XP earned.
10. Attribute progress updates.
11. Refresh preserves authoritative state.
12. Undo reverses the completion and XP.

The application is currently functional rather than visually final.

Phase 6 builds the dashboard and streak behavior.

Phase 7 applies the approved game-style UI system.

---

## 3. Product Principles

### 3.1 Real behavior before visual decoration

The interface should only display data backed by implemented and tested behavior.

Do not display fake:

- currencies,
- ranks,
- notifications,
- quests,
- avatars,
- leaderboards,
- Focus Scores,
- social metrics.

### 3.2 Backend authority

The backend owns:

- authentication,
- authorization,
- ownership,
- validation,
- completion rules,
- local-date calculation,
- XP rewards,
- attribute progression,
- overall progression,
- future streak calculations,
- future dashboard aggregation.

React visualizes the system.

React does not invent the system.

### 3.3 PostgreSQL as the source of truth

Persistent state belongs in PostgreSQL.

The frontend may hold temporary display state, but it must not become the authoritative source for:

- habits,
- completions,
- XP,
- attributes,
- streaks,
- dashboard summaries.

### 3.4 Future-compatible, not future-built

The architecture should support the approved product direction without prematurely implementing post-MVP features.

Good preparation includes:

- stable entities,
- stable enums,
- clear services,
- DTO boundaries,
- auditable XP transactions,
- reusable React components,
- aggregate dashboard responses.

Bad preparation includes:

- unused tables,
- fake currencies,
- placeholder APIs,
- empty systems,
- speculative abstraction layers.

---

## 4. Core Objectives

- Provide a clear and useful habit-tracking experience.
- Support multiple users with private data.
- Preserve trustworthy completion history.
- Reward habits with deterministic XP.
- Develop character attributes from real user action.
- Display levels and progression clearly.
- Calculate streaks in the backend.
- Provide a useful daily dashboard.
- Provide a basic weekly review.
- Build a distinctive premium UI.
- Maintain clean frontend/backend boundaries.
- Keep the project testable and understandable.
- Produce a strong portfolio project.
- Preserve a beginner-friendly learning workflow.

---

## 5. Fixed Technology Stack

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

### Backend testing

- xUnit
- ASP.NET Core `WebApplicationFactory`
- EF Core InMemory substitution inside HTTP integration tests

### Frontend testing

- Vitest
- React Testing Library

### Browser testing

- Playwright
- Chromium
- Real PostgreSQL end-to-end environment

### Automation

- GitHub Actions
- Backend CI job
- Frontend CI job
- End-to-end CI job
- Repository-local `dotnet-ef` tool manifest

The fixed stack must not be changed silently.

---

## 6. High-Level Architecture

```text
React + TypeScript frontend
            |
            | HTTP / JSON
            v
ASP.NET Core Web API
            |
            | Entity Framework Core
            v
PostgreSQL
```

Frontend and backend are separate applications.

During local development:

```text
Browser
   |
   | /api
   v
Vite development server
   |
   | proxy
   v
ASP.NET Core
```

The proxy keeps browser requests same-origin while preserving separate applications.

---

## 7. Architecture Principles

- Frontend and backend remain separate.
- Backend owns business logic.
- Frontend displays data and sends user actions.
- PostgreSQL stores persistent truth.
- Entity Framework Core handles database access.
- DTOs cross the API boundary.
- Database entities are not returned directly.
- Controllers remain thin.
- Services contain application behavior.
- Ownership comes from authenticated claims.
- User-owned requests do not accept client-supplied `userId`.
- Business calculations remain on the server.
- Tests are added alongside protected behavior.
- Architecture and scope changes require explicit approval.
- Stable names are preserved.
- Development follows the roadmap phases.

---

## 8. Canonical Entities

### Implemented MVP entities

- `User`
- `UserSettings`
- `Habit`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

### Reserved post-MVP entities

- `Milestone`
- `UserMilestone`

The reserved post-MVP names remain canonical even though those features are not implemented yet.

---

## 9. Canonical Services

### Implemented

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`

### Planned for Phase 6

- `StreakService`
- `DashboardService`

These names are stable.

Do not replace them with vague alternatives such as:

- `GameService`
- `ProgressService`
- `StatsService`
- `HabitManager`
- `DataManager`

---

## 10. Canonical Character Attributes

The eight character attributes are:

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

Earlier names such as Strength, Recovery, Career, and Spirituality are obsolete and must not be reintroduced.

The same attribute must keep the same identity across:

- backend enum values,
- API JSON,
- frontend types,
- UI labels,
- tests,
- documentation,
- future charts.

---

## 11. Canonical Habit Categories

Supported habit categories:

- Fitness & Movement
- Health & Recovery
- Learning & Skills
- Work & Career
- Daily Life & Organization
- Money & Finance
- Relationships & Community
- Emotional Well-Being
- Spirituality & Purpose
- Creativity & Recreation
- Self-Control & Boundaries
- General Growth

The domain enum names are:

- `FitnessAndMovement`
- `HealthAndRecovery`
- `LearningAndSkills`
- `WorkAndCareer`
- `DailyLifeAndOrganization`
- `MoneyAndFinance`
- `RelationshipsAndCommunity`
- `EmotionalWellBeing`
- `SpiritualityAndPurpose`
- `CreativityAndRecreation`
- `SelfControlAndBoundaries`
- `GeneralGrowth`

Habit categories are controlled values, not free text.

---

## 12. Difficulty and Reward Rules

Supported difficulties:

- Easy
- Medium
- Hard
- Elite

Total XP by difficulty:

| Difficulty | Total XP |
|---|---:|
| Easy | 10 |
| Medium | 20 |
| Hard | 30 |
| Elite | 50 |

Each category maps to:

- one primary attribute,
- one secondary attribute.

Reward split:

- primary: 70%
- secondary: 30%

The backend calculates the rewards.

The frontend may display returned values but must not reproduce the authoritative calculation.

---

## 13. Implemented Authentication Foundation

Phase 2 implemented:

- `User`,
- `UserSettings`,
- PostgreSQL schema and constraints,
- case-insensitive email uniqueness,
- case-insensitive username uniqueness,
- Guid version 7 identifiers,
- password hashing,
- registration,
- login,
- logout,
- session restoration,
- optional `rememberMe`,
- encrypted authentication cookies,
- antiforgery protection,
- authenticated ownership identity,
- Problem Details errors,
- frontend authentication state,
- login and registration UI,
- backend unit tests,
- HTTP integration tests,
- frontend tests,
- Playwright browser tests,
- PostgreSQL-backed end-to-end testing,
- GitHub Actions CI.

### Authentication model

The browser manages the encrypted authentication cookie.

The frontend does not receive or store the authentication credential.

The default session:

- lasts up to 12 hours,
- is non-persistent.

With `rememberMe`:

- the session may persist for up to 30 days.

Sliding expiration is disabled during MVP.

---

## 14. Implemented Habit Management

Phase 3 implemented:

- `Habit`,
- `HabitCategory`,
- `HabitFrequencyType`,
- `HabitDifficulty`,
- EF Core configuration,
- PostgreSQL migration,
- ownership constraints,
- indexes,
- check constraints,
- create habit,
- list habits,
- get habit,
- update habit,
- soft-deactivate habit,
- active-only default listing,
- optional inactive inclusion,
- deterministic ordering,
- frontend create form,
- frontend edit form,
- frontend list,
- guarded deactivation,
- backend tests,
- integration tests,
- frontend tests,
- manual PostgreSQL-backed verification.

The frontend reloads authoritative habit data after create, update, and deactivation.

---

## 15. Implemented Habit Completion

Phase 4 implemented:

- `HabitCompletion`,
- local-date storage,
- exact UTC completion timestamp,
- one completion per habit and local date,
- backend-owned time-zone conversion,
- `TimeProvider`,
- completion notes,
- complete-today behavior,
- undo-today behavior,
- inactive-habit protection,
- duplicate-completion protection,
- ownership protection,
- frontend completion state,
- frontend undo state,
- pending and error behavior,
- backend tests,
- integration tests,
- frontend tests,
- manual browser verification.

The client cannot choose `completedDate`.

The backend derives the user-local date from:

- current UTC time,
- the user’s stored IANA time zone.

---

## 16. Implemented XP and Attributes

Phase 5 implemented:

- `HabitAttributeReward`,
- `UserAttribute`,
- `XpTransaction`,
- `XpService`,
- `AttributeService`,
- automatic reward calculation,
- automatic reward synchronization,
- completion XP application,
- undo XP reversal,
- persisted attribute XP,
- persisted XP transactions,
- backend-calculated attribute levels,
- backend-calculated overall-level formulas,
- `GET /api/attributes`,
- reward data in habit responses,
- applied reward data in completion responses,
- frontend attribute progress cards,
- immediate earned-XP feedback,
- completion-triggered attribute refresh,
- undo-triggered attribute refresh,
- backend tests,
- HTTP integration tests,
- frontend API tests,
- frontend component tests,
- manual verification.

### Completion transaction behavior

Completing a habit creates or updates together:

- `HabitCompletion`,
- `UserAttribute`,
- `XpTransaction`.

Undo reverses together:

- attribute XP,
- XP transactions,
- completion.

These changes use one EF Core unit of work and one save operation for the request.

---

## 17. Implemented API Surface

### Health

- `GET /api/health`

### Authentication

- `GET /api/auth/csrf-token`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`

### Habits

- `GET /api/habits`
- `GET /api/habits/{habitId}`
- `POST /api/habits`
- `PUT /api/habits/{habitId}`
- `DELETE /api/habits/{habitId}`

### Completion

- `POST /api/habits/{habitId}/completions`
- `DELETE /api/habits/{habitId}/completions/today`

### Attributes

- `GET /api/attributes`

### Planned Phase 6 direction

- `GET /api/dashboard`

The exact dashboard contract will be approved before implementation.

---

## 18. Current Frontend Experience

The current frontend is intentionally functional scaffolding.

It includes:

- authentication forms,
- authentication restoration,
- habit creation,
- habit editing,
- habit deactivation,
- habit completion,
- completion undo,
- attribute cards,
- progress bars,
- earned-XP feedback,
- loading states,
- pending states,
- error states,
- frontend tests.

The current interface proves the full data and behavior loop.

It is not the final visual design.

Phase 7 performs the main UI transformation.

---

## 19. Approved UI Vision

The approved final direction is:

> A desktop-first, dark premium habit-tracking command center with electric blue, cyan, violet, and restrained gold accents.

The interface should combine:

### Premium productivity software

- clear hierarchy,
- generous spacing,
- readable typography,
- efficient actions,
- clean alignment,
- calm surfaces.

### Restrained progression atmosphere

- midnight surfaces,
- blue-violet progression,
- crest-like level identity,
- attribute cards,
- XP bars,
- streak highlights,
- subtle original fantasy artwork,
- occasional earned glow.

**Glow is a reward, not wallpaper.**

The app must not copy copyrighted characters, game logos, or franchise artwork.

---

## 20. Canonical Screens

### Dashboard

Purpose:

- daily command center.

Priority:

1. today’s habits,
2. completion actions,
3. completion summary,
4. overall XP and level,
5. streaks,
6. attributes,
7. recent activity,
8. weekly progress.

### Habits

Purpose:

- desktop management workspace.

Planned UI:

- Active and Inactive tabs,
- search,
- filters,
- sorting,
- create action,
- complete,
- edit,
- deactivate,
- reward preview,
- selected-habit details.

### Attributes

Purpose:

- character sheet.

Planned UI:

- stable colors,
- level,
- current XP,
- next threshold,
- progress bar,
- deeper summaries based on real data.

### Weekly Review

Purpose:

- explain behavior without judging the user.

Planned information:

- completion rate,
- XP earned,
- current streak,
- longest streak,
- heatmap,
- category comparison,
- deterministic insight summaries.

### Settings

Purpose:

- manage account and display behavior.

MVP-relevant settings include:

- display name,
- time zone,
- account information.

---

## 21. Phase 6 — Dashboard and Streaks

Phase 6 is next.

It will add:

- `StreakService`,
- `DashboardService`,
- approved streak rules,
- current streak,
- longest streak,
- overall user XP,
- overall user level,
- overall level progress,
- today completion summary,
- today XP summary,
- recent activity,
- basic weekly progress,
- aggregated dashboard response,
- functional dashboard screen,
- backend tests,
- integration tests,
- frontend tests,
- manual acceptance verification.

### Important Phase 6 rule

Streak rules must be approved before coding.

The project must define:

- what streak is being measured,
- how daily habits count,
- how weekly habits count,
- whether today may remain incomplete without breaking a streak,
- how local dates are used,
- how inactive habits affect history,
- what `CurrentStreak` means,
- what `LongestStreak` means.

React must not calculate streaks.

---

## 22. Phase 7 — Game UI Polish

Phase 7 will apply the approved visual system.

Expected work:

- `AppShell`,
- sidebar,
- top bar,
- responsive page layout,
- premium dark theme,
- reusable panels,
- buttons,
- badges,
- progress components,
- dashboard composition,
- habit-management workspace,
- attribute presentation,
- weekly-review presentation,
- consistent loading and error states,
- subtle motion,
- reduced-motion support,
- keyboard focus,
- accessibility polish.

Phase 7 must not move business logic into React.

---

## 23. Phase 8 — Deployment and Project Polish

Phase 8 will focus on:

- production configuration,
- secure deployment,
- database deployment,
- migrations,
- environment variables,
- production cookies,
- production antiforgery behavior,
- CORS if required,
- secrets,
- screenshots,
- README,
- local setup instructions,
- project demonstration,
- portfolio presentation,
- final documentation,
- stability.

Phase 8 must not become an unplanned feature phase.

---

## 24. MVP Scope

The MVP includes:

- registration,
- login,
- logout,
- session restoration,
- user settings,
- private user-owned data,
- habit creation,
- habit editing,
- habit deactivation,
- daily completion,
- undo,
- duplicate protection,
- automatic XP rewards,
- character attributes,
- attribute levels,
- overall user level,
- current streak,
- longest streak,
- today dashboard,
- recent activity,
- basic weekly progress,
- premium game-style UI,
- deployment,
- portfolio polish.

---

## 25. Post-MVP Scope

The following are intentionally deferred:

- advanced scheduling,
- sleep tracking,
- bad-habit tracking,
- journal entries,
- reminders,
- notifications,
- AI recommendations,
- social features,
- leaderboards,
- public profiles,
- avatar systems,
- themes marketplace,
- quests,
- inventory,
- currencies,
- advanced analytics,
- partial completion,
- Focus Score,
- mobile application,
- payments,
- admin panel,
- calendar integration,
- milestones and achievements.

Post-MVP features require separate product and architecture decisions.

---

## 26. Testing Strategy

### Backend unit tests

Protect:

- service rules,
- XP calculations,
- attribute calculations,
- validation,
- ownership,
- future streak logic.

### HTTP integration tests

Protect:

- routes,
- authentication,
- antiforgery,
- controller/service integration,
- status codes,
- Problem Details,
- ownership boundaries.

### Frontend tests

Protect:

- forms,
- provider behavior,
- API modules,
- loading states,
- pending states,
- errors,
- observable component behavior.

### Browser tests

Protect:

- real user journeys,
- frontend/backend interaction,
- real PostgreSQL persistence,
- authentication flow,
- critical MVP workflows.

### CI

GitHub Actions runs:

- Backend,
- Frontend,
- End-to-end.

---

## 27. Development Workflow

The project uses small vertical slices.

A typical slice:

1. Inspect current files.
2. Identify the owning service.
3. Define or update the DTO.
4. Implement the smallest backend behavior.
5. Add focused tests.
6. Add the endpoint.
7. Add integration tests.
8. Add frontend types.
9. Add frontend API behavior.
10. Add React presentation.
11. Add frontend tests.
12. Run the full relevant suite.
13. Manually verify.
14. Inspect Git changes.
15. Commit one coherent change.

### Before committing

Run:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Every staged file must be intentional.

---

## 28. Documentation Role

The documentation set should answer different questions.

### `PROJECT_OVERVIEW.md`

What is the product and what currently exists?

### `ROADMAP.md`

What is complete, what is next, and what is deferred?

### `ARCHITECTURE.md`

Where does each responsibility belong?

### `DATA_MODEL.md`

What persistent entities and relationships exist?

### `API_CONTRACT.md`

What requests and responses cross the HTTP boundary?

### `NAMING_CONVENTIONS.md`

What names are canonical?

### `UI_VISION.md`

What should the finished product look and feel like?

### `README.md`

How does an outside developer understand, run, and evaluate the project?

Documentation must distinguish:

- implemented,
- next,
- planned,
- post-MVP.

---

## 29. Current Product Milestone

The first complete progression milestone has been achieved:

> A registered and authenticated user can create a habit, complete it for today, receive backend-calculated XP, see updated character attributes, refresh without losing progress, and undo the completion with XP reversal.

This proves the full browser-to-database loop:

```text
React
  → HTTP API
  → ASP.NET Core services
  → Entity Framework Core
  → PostgreSQL
  → response DTOs
  → React
```

The next milestone is:

> A user opens the dashboard and immediately understands what to do today, how much progress they have made, their overall level, their streaks, and their recent momentum.

---

## 30. Final Rule

The UI vision defines the destination.

The roadmap defines the order.

The architecture defines ownership.

The data model preserves history.

The API exposes authoritative behavior.

The backend calculates the system.

The frontend presents the system.

The project should become more polished without becoming less trustworthy.
