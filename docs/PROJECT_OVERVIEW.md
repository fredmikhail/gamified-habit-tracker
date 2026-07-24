# Gamified Habit Tracker — Project Overview

**Status:** Phases 1–7 complete

**Current phase:** Phase 8 — Deployment and Project Polish

## Purpose

Gamified Habit Tracker is a full-stack web application for managing habits and turning completed actions into persistent progression.

A completion may produce:

- an auditable completion record;
- positive XP transactions;
- updates to two mapped attributes;
- overall and attribute-level progress;
- recalculated dashboard totals;
- recalculated per-habit streaks.

Undo preserves the original event and appends matching reversal transactions.

## Product Principles

- Actions should be simple at the interface.
- Progression must be deterministic and explainable.
- Historical events must keep their original meaning.
- The backend owns rules and validation.
- PostgreSQL stores durable truth.
- React displays authoritative state and manages interaction.
- Gamification should reinforce consistency without becoming noisy or casino-like.
- Deferred features do not receive placeholder controls or schema.

## Current User Flow

An authenticated user can:

1. create a daily or weekly habit;
2. choose a controlled category and difficulty;
3. complete the habit for the current local date;
4. receive backend-calculated rewards;
5. see dashboard and attribute progression update;
6. undo the completion and reverse its XP;
7. edit the habit;
8. inspect future scheduled rule changes;
9. deactivate the habit;
10. reactivate it without deleting history.

## Authentication

Implemented authentication behavior includes:

- registration;
- login;
- logout;
- current-user restoration;
- temporary sessions;
- remembered sessions;
- password hashing;
- encrypted ASP.NET Core cookie authentication;
- `HttpOnly` cookies;
- `SameSite=Lax`;
- secure cookies outside Development;
- non-sliding expiration;
- antiforgery validation on state-changing requests;
- authenticated identity resolved from backend claims.

The client does not receive or store bearer tokens.

Registration creates `User` and `UserSettings` in one unit of work.

## User Settings

`UserSettings` currently stores:

- `DisplayName`;
- IANA `TimeZone`;
- `WeekStartsOn`;
- creation and update timestamps.

New accounts default to Monday for `WeekStartsOn`.

The time zone and week-start preference affect:

- current local-date calculation;
- completion and undo;
- scheduled configuration boundaries;
- weekly streak periods.

The appearance theme is different: it is a browser UI preference managed by `ThemeProvider`, not a PostgreSQL user setting.

## Habit Model

A habit contains:

- name;
- optional description;
- category;
- frequency;
- target count;
- difficulty;
- active state;
- lifecycle timestamps.

Supported frequencies:

- Daily, with `TargetCount = 1`;
- Weekly, with `TargetCount` from 1 through 7.

Supported difficulties:

- Easy;
- Medium;
- Hard;
- Elite.

Normal removal is soft deactivation. Reactivation changes the lifecycle state back to active and preserves existing configuration, completion, and XP history.

## Effective-Dated Habit Configuration

`HabitConfigurationVersion` preserves the rules active at different dates.

Versioned fields:

- category;
- frequency;
- target count;
- difficulty.

Effective periods use:

```text
EffectiveFromDate <= date < EffectiveToDateExclusive
```

The open-ended configuration has no exclusive end date.

Name and description changes apply immediately.

Changes to category, frequency, target count, or difficulty begin at the next user-local week boundary. A later edit updates the one pending next-boundary version. Returning the requested rules to the current effective rules removes the pending version.

Every new completion records the configuration version effective on its completion date.

## Completion and Undo

The backend determines the current local date from the current UTC timestamp and the stored IANA time zone.

The client cannot submit the authoritative completion date.

A habit may have only one active, non-undone completion for the same local date.

Completion:

- checks ownership;
- checks active state;
- prevents a duplicate active completion;
- stores the local date and UTC timestamp;
- links the effective configuration;
- creates XP transactions;
- updates attributes;
- commits the work atomically.

Undo:

- keeps the original completion;
- records `UndoneAtUtc`;
- keeps the original positive transactions;
- appends negative transactions;
- reverses current attribute totals;
- causes dashboard and streak values to be recalculated.

An undone completion no longer blocks a replacement completion for the same date.

## XP and Attributes

Canonical attributes:

- Discipline;
- Fitness;
- Vitality;
- Focus;
- Mind;
- Resilience;
- Social;
- Purpose.

Difficulty determines the total reward:

| Difficulty |  XP |
| ---------- | --: |
| Easy       |  10 |
| Medium     |  20 |
| Hard       |  30 |
| Elite      |  50 |

Each category maps to a primary and secondary attribute.

Allocation:

- primary: 70%;
- secondary: 30%.

`XpService` owns reward and level calculations.

`AttributeService` owns attribute reads, reward persistence, reversal, and the attribute overview read model.

Levels and progress are calculated from XP. They are not stored independently.

## Streaks

Streaks are per habit and are not stored as mutable counters.

`StreakService` derives them from:

- effective-dated configuration history;
- active, non-undone completions;
- current user-local date;
- configured week start.

Daily habits use days. Weekly habits use weeks.

An incomplete current day or current week does not break a streak before that period ends.

Historical weekly periods use the target effective during that week.

A frequency change begins a new streak series. Category, difficulty, and weekly target changes do not carry the same reset rule.

## Dashboard

`GET /api/dashboard` returns one authenticated aggregate owned by `DashboardService`.

The response includes:

- overall progression;
- today activity;
- daily execution;
- active habits with current completion state and reward details;
- all eight attributes;
- per-habit current and longest streaks.

The Phase 7 dashboard presents that contract as an action-first command center with:

- a progression summary rail;
- Today’s Habits;
- Completed Today;
- Protect the Chain;
- attribute overview cards;
- attribute XP distribution;
- adaptive pagination;
- small-screen panel switching;
- loading and error states.

Completing or undoing a habit refreshes the authoritative aggregate. Habit create, edit, deactivate, and activate operations also refresh affected workspace resources.

## Habit Workspace

The Phase 7 habit workspace includes:

- Active Habits and Inactive Habits tabs;
- search by name, description, or category label;
- category, frequency, and difficulty filters;
- sorting by name, streak, reward, or creation time;
- viewport-aware pagination;
- selected-habit inspector;
- create and edit flows;
- pending-configuration notice;
- completion and undo;
- guarded deactivation;
- reactivation.

Search, filtering, sorting, selection, and pagination are interface concerns. Ownership, validation, lifecycle changes, completion state, and progression remain backend-owned.

## Attribute Command Center

`GET /api/attributes/overview` supports the Phase 7 attribute page.

The backend response contains:

- all eight attributes;
- total attribute XP;
- balance score;
- strongest attribute;
- needs-focus attribute;
- three attributes closest to a level-up;
- six recent XP transactions.

The frontend presents:

- compact attribute cards;
- a native SVG radar;
- an equilibrium summary;
- recent XP activity;
- XP distribution;
- the level-up queue.

The overview is a derived read model. Phase 7 did not add a persistence table for it.

## Interface System

The routed application shell contains:

- persistent desktop navigation;
- fixed mobile navigation;
- global XP and level progress;
- current route heading;
- account status;
- appearance selection;
- sign out;
- routed page content;
- global footer.

Appearance options:

- Abyss;
- Obsidian;
- Lumen;
- system preference.

The theme choice is persisted in browser storage.

The visual system uses restrained energy, semantic status colors, stable attribute colors, visible focus styles, semantic labels, responsive layouts, and reduced-motion handling.

Progression feedback compares previously rendered values with newly returned API values:

- XP gain plays only after XP increases;
- level-up feedback plays only after a returned level increases;
- the initial render does not animate;
- reduced-motion preferences disable the effects.

## Testing and Automation

Backend unit tests protect service behavior and calculations.

HTTP integration tests protect routes, authentication, antiforgery, status codes, serialization, and service/controller integration.

Frontend tests protect API modules, providers, component behavior, responsive layout rules, loading, errors, actions, and progression feedback.

Playwright covers the authentication lifecycle against a running frontend, API, and PostgreSQL database.

GitHub Actions runs Backend, Frontend, and End-to-end jobs on pushes and pull requests.

## Current MVP Scope

Implemented:

- secure authentication;
- private user-owned data;
- habit creation, editing, deactivation, and reactivation;
- daily and weekly habits;
- effective-dated configuration;
- local-date completion and auditable undo;
- deterministic XP and attributes;
- levels;
- daily and weekly per-habit streaks;
- dashboard and attribute read models;
- routed, responsive application UI;
- automated checks and continuous integration.

Deferred:

- advanced scheduling;
- reminders and notifications;
- social systems;
- public profiles;
- leaderboards;
- AI recommendations;
- avatars and currencies;
- advanced analytics;
- mobile application;
- payments;
- administration;
- calendar integration;
- milestones and achievements.

## Current Phase

Phase 8 is responsible for release readiness:

- production hosting;
- production PostgreSQL;
- environment and migration documentation;
- production cookie, antiforgery, and origin verification;
- logging and error-handling review;
- deployment validation;
- screenshots;
- final stability and release preparation.

Phase 8 is not an additional feature phase.
