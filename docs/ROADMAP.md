# Gamified Habit Tracker — Development Roadmap

**Current status:** Phases 1–7 complete

**Current phase:** Phase 8 — Deployment and Project Polish

## Roadmap Rules

- React presents data and sends actions.
- ASP.NET Core owns application and domain behavior.
- PostgreSQL is the persistent source of truth.
- Entity Framework Core owns database access.
- DTOs define the HTTP boundary.
- Controllers stay thin.
- Services own validation, ownership, calendar rules, XP, completion, undo, levels, dashboard aggregation, and streaks.
- Historical events must keep the configuration and rewards that applied when they occurred.
- Later phases must not silently reinterpret completed work.
- Each commit should represent one coherent change.

Before committing:

```text
git status --short
git diff --check
git diff --cached --name-only
```

## Phase 1 — Project Setup

**Status:** Complete

### Goal

Establish the frontend, backend, database, and test foundation.

### Delivered

- ASP.NET Core Web API;
- React, TypeScript, and Vite client;
- Tailwind CSS;
- PostgreSQL and Entity Framework Core;
- xUnit test project;
- health endpoint;
- local HTTPS and development proxy behavior;
- frontend-to-backend health request;
- repository-local .NET tool manifest.

## Phase 2 — Authentication

**Status:** Complete

### Goal

Provide secure registration, login, logout, session restoration, and authenticated ownership.

### Delivered

- `User` and `UserSettings`;
- password hashing;
- normalized email and username uniqueness;
- registration, login, logout, and current-user endpoints;
- encrypted cookie authentication;
- temporary and remembered sessions;
- antiforgery protection;
- frontend authentication state and forms;
- unit, integration, frontend, and Playwright coverage;
- GitHub Actions validation.

### Deferred

- email verification;
- password reset;
- account recovery;
- external identity providers.

## Phase 3 — Habit CRUD

**Status:** Complete

### Goal

Allow authenticated users to create and manage their own habits.

### Delivered

- `Habit`;
- controlled categories, frequencies, and difficulties;
- create, list, retrieve, edit, and soft-deactivate behavior;
- ownership protection;
- deterministic list ordering;
- frontend create, edit, list, and deactivation flows;
- unit, integration, and frontend coverage.

## Phase 4 — Habit Completion

**Status:** Complete

### Goal

Allow an authenticated user to complete an active owned habit for the current local date and undo the active completion.

### Delivered

- `HabitCompletion`;
- backend-owned local-date calculation;
- IANA time-zone handling;
- ownership and active-state validation;
- duplicate active-completion prevention;
- completion and undo endpoints;
- frontend pending, success, and error behavior;
- unit, integration, and frontend coverage.

Phase 6 later strengthened undo and uniqueness without changing the user-facing action.

## Phase 5 — XP and Attributes

**Status:** Complete

### Goal

Apply deterministic progression when a habit is completed.

### Delivered

- `HabitAttributeReward`;
- `UserAttribute`;
- `XpTransaction`;
- `XpService`;
- `AttributeService`;
- eight canonical attributes;
- difficulty totals;
- 70/30 primary-secondary allocation;
- XP award and reversal;
- attribute and overall level calculation;
- `GET /api/attributes`;
- frontend attribute progress;
- automated coverage.

## Phase 6 — Dashboard and Streaks

**Status:** Complete

### Goal

Provide an authenticated progression summary and correct per-habit daily and weekly streaks.

### Delivered

- `WeekStartsOn`, defaulting new users to Monday;
- shared calendar-period calculation;
- `HabitConfigurationVersion`;
- effective-dated configuration history;
- next-week rule scheduling;
- completion-to-configuration linkage;
- `UndoneAtUtc`;
- append-only XP reversals;
- active-completion partial uniqueness;
- `StreakService`;
- `DashboardService`;
- daily streak rules;
- weekly streak rules;
- historical weekly targets;
- frequency-segment streak resets;
- `GET /api/dashboard`;
- frontend dashboard and streak presentation;
- authoritative refresh after relevant mutations;
- automated and manual validation.

### Decisions Added for Correctness

- Configuration periods use half-open date ranges.
- Name and description edits are immediate.
- Rule-bearing edits begin at the next user-local week boundary.
- Undo preserves history rather than deleting it.
- Streaks remain derived values.
- A frequency change begins a new streak series.
- Historical weeks use the target effective during that week.

## Phase 7 — Game UI Polish

**Status:** Complete

### Goal

Replace the functional interface with a cohesive, responsive command center without moving backend behavior into React.

### Delivered

#### Appearance foundation

- Abyss, Obsidian, Lumen, and system appearance modes;
- browser-persisted appearance preference;
- semantic theme tokens;
- stable attribute colors;
- visible global focus treatment;
- reduced-motion support.

#### Routed application shell

- `/dashboard`, `/habits`, and `/attributes`;
- authenticated `AppShell`;
- persistent desktop navigation;
- fixed mobile navigation;
- top-level route headings;
- global XP and level progress;
- account identity and sign out;
- global footer;
- full-height viewport behavior;
- shared workspace data resources.

#### Dashboard command center

- action-first composition;
- Today’s Habits;
- Completed Today;
- progression summary rail;
- Protect the Chain;
- attribute overview;
- attribute XP distribution;
- adaptive panel pagination through `ResizeObserver`;
- small-screen panel switching;
- completion actions backed by dashboard refetches;
- explicit loading, error, and empty states.

#### Habit workspace

- Active Habits and Inactive Habits tabs;
- search, filtering, sorting, and adaptive pagination;
- responsive table and inspector layout;
- create and edit flows;
- current and pending configuration presentation;
- reward, difficulty, frequency, and streak presentation;
- completion and undo;
- guarded deactivation;
- reactivation without deleting history;
- footer-boundary and row-sizing corrections.

#### Attribute command center

- `GET /api/attributes/overview`;
- backend-calculated balance score;
- strongest and needs-focus attributes;
- recent XP transactions;
- backend-ranked closest-to-level-up queue;
- eight compact progression cards;
- native SVG radar;
- equilibrium and XP distribution presentation;
- recent XP support panels;
- responsive level-up strip.

The attribute overview is a derived read model. Phase 7 added no persistence table.

#### Adaptive polish

- container-aware typography and dimensions;
- laptop and desktop layout scaling;
- dashboard pagination based on measured height;
- compact attribute cards;
- habit rows constrained above the footer boundary;
- aligned habit table headings and values.

#### Progression feedback

- XP-gain pulse after returned XP increases;
- level-up feedback after the backend-provided level increases;
- no initial-render animation;
- bounded animation durations;
- reduced-motion fallback.

#### Validation

- focused frontend tests for the shell, dashboard, habits, attributes, themes, and feedback;
- full frontend checks;
- backend checks;
- PostgreSQL-backed Playwright authentication journey;
- GitHub Actions confirmation.

### Important Scope Decisions

The following do not block Phase 7 completion:

- weekly review screen;
- heatmap;
- advanced charts;
- advanced analytics;
- notification surfaces;
- avatar systems;
- cinematic effects.

They are not part of the current Phase 7 definition of done.

### Key Phase 7 Checkpoints

- `18995c1` — Add configurable appearance foundation
- `0315ca8` — Add routed app shell with persistent workspace data
- `a8df0eb` — Build action-first dashboard command center
- `be378e9` — Add habit reactivation workflow
- `5f9e9ef` — Build responsive habit workspace and progression shell
- `413a48d` — Add attribute overview API
- `4ded1b1` — Build attribute progression command center
- `1977cad` — Refine adaptive interface layout
- `a9a9c2c` — Revitalize dashboard progression summary
- `6e3806c` — Add progression feedback animations
- `0ced56d` — Update authentication E2E shell assertion

## Phase 8 — Deployment and Project Polish

**Status:** Current

### Goal

Prepare the application for reliable production use and release.

### Planned Work

- choose and configure production hosting;
- deploy the React application;
- deploy the ASP.NET Core API;
- deploy PostgreSQL;
- define production environment variables;
- define migration execution;
- configure secret storage;
- verify production cookie behavior;
- verify antiforgery behavior;
- decide the public frontend/API origin model;
- configure credentialed CORS only if separate origins are required;
- review startup database behavior;
- review logging and unexpected-error handling;
- add application screenshots;
- verify local setup from a clean environment;
- review test coverage around release-critical journeys;
- perform final stability and documentation review.

### Definition of Done

- the application is deployed;
- the production database is migrated;
- secrets are not committed or exposed;
- authentication and antiforgery work through the production origin model;
- local setup instructions work;
- release-critical checks pass;
- screenshots represent the real application;
- the repository is ready for a tagged release or equivalent milestone.

### Not Included

Phase 8 must not become a new feature phase.

It does not add:

- social systems;
- leaderboards;
- reminders or notifications;
- AI recommendations;
- avatars;
- currencies;
- advanced scheduling;
- complex analytics;
- mobile applications;
- payments;
- administration;
- calendar integration;
- milestones or achievements.

## Post-MVP

Post-MVP work requires a separate product and architecture decision.

Reserved ideas include:

- milestones and achievements;
- advanced weekly review;
- advanced analytics;
- reminders and notifications;
- social accountability;
- public profiles;
- additional scheduling models;
- mobile clients.
