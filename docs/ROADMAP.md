# Gamified Habit Tracker — Development Roadmap

**Current status:** Phase 5 complete
**Next phase:** Phase 6 — Dashboard and Streaks
**Product direction:** Desktop-first premium habit-tracking command center with restrained game progression
**Implementation rule:** Future-compatible, not future-built

---

## 1. Purpose

This roadmap defines the approved development sequence for the Gamified Habit Tracker.

The project must be built in small, understandable phases while preserving:

- the fixed technology stack,
- the existing architecture,
- stable domain names,
- backend ownership of business rules,
- PostgreSQL as the source of truth,
- the learning-focused workflow,
- the approved UI vision,
- the MVP scope.

The roadmap determines **when** a capability may be built.

The architecture documentation determines **where** the responsibility belongs.

The UI vision determines **how the finished product should feel and present real data**.

No phase should silently redesign work from an earlier phase.

---

## 2. Roadmap Rules

### 2.1 Phase discipline

Before starting work, confirm:

1. the current phase,
2. the current feature,
3. the expected behavior,
4. the exact owning service,
5. the DTO or API contract involved,
6. the relevant frontend consumer,
7. the tests required,
8. the manual verification path.

Do not begin a later-phase feature merely because it appears in a mockup.

### 2.2 Architecture discipline

The following rules remain fixed:

- React displays data and sends user actions.
- ASP.NET Core owns validation and business logic.
- PostgreSQL is the persistent source of truth.
- Entity Framework Core owns database access.
- DTOs cross the frontend/backend boundary.
- Database entities are never returned directly as API responses.
- Controllers remain thin.
- Services own application and domain behavior.
- XP, level, streak, completion, ownership, and date rules never move into React.

### 2.3 UI discipline

The approved UI vision is a design destination, not permission to invent unsupported systems.

The application may visually prepare for:

- levels,
- XP,
- attributes,
- streaks,
- weekly review,
- recent activity,
- achievements.

The application must not display fake:

- currencies,
- ranks,
- notifications,
- focus scores,
- quests,
- social metrics,
- avatar systems,
- public profiles.

A visual element should not appear as real product behavior until the corresponding backend behavior exists and is tested.

### 2.4 Change-control discipline

When an API response changes, review together:

- backend response DTO,
- backend service mapping,
- backend unit tests,
- HTTP integration tests,
- frontend TypeScript response type,
- frontend API tests,
- component fixtures,
- consuming React components.

Before every commit, run:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Confirm that:

- no unrelated file is modified,
- every intended file is staged,
- no file was accidentally overwritten with another file’s contents,
- generated output is not staged,
- the commit represents one understandable change.

---

## Phase 1 — Project Setup

**Status: Complete**

### Goal

Set up the frontend, backend, PostgreSQL database, and test foundation.

### Implemented Scope

- Created the ASP.NET Core Web API backend.
- Created the React, TypeScript, and Vite frontend.
- Added Tailwind CSS.
- Set up PostgreSQL.
- Connected the backend to PostgreSQL.
- Added Entity Framework Core.
- Created the xUnit backend test project.
- Confirmed the test project runs successfully.
- Added a backend health-check endpoint.
- Confirmed the frontend can call the backend.
- Configured HTTPS and CORS for local development.

### Definition of Done

- Backend runs locally.
- Frontend runs locally.
- PostgreSQL runs locally.
- Backend connects to PostgreSQL.
- Backend tests run successfully.
- Frontend calls a backend endpoint successfully.

### Not Included

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

Allow users to register, log in, log out, restore authenticated sessions, and establish secure ownership for private user data.

### Implemented Scope

- Created the `User` entity.
- Created the `UserSettings` entity.
- Created default settings during registration.
- Stored display name and IANA time zone.
- Added password hashing and verification.
- Added registration.
- Added login.
- Added logout.
- Added current-user restoration.
- Added secure encrypted cookie authentication.
- Added antiforgery protection for state-changing browser requests.
- Added backend authorization.
- Added frontend authentication state.
- Added registration and login UI.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend tests.
- Added browser end-to-end coverage.
- Added GitHub Actions continuous integration.

### Authentication Decisions

- Application-generated Guid version 7 identifiers.
- Case-insensitive email and username normalization.
- Unique normalized email and username indexes.
- Atomic `User` and `UserSettings` registration.
- IANA time-zone validation.
- `HttpOnly` authentication cookie.
- 12-hour non-persistent session by default.
- Fixed 30-day persistent session only with `rememberMe`.
- No sliding expiration during MVP.
- `GET /api/auth/me` restores the current user.
- Authenticated identity comes from server-side claims.
- User-owned requests never accept a client-provided `userId`.
- Logout is idempotent.
- Expected API failures use Problem Details.
- Frontend HTTP credentials, antiforgery, and API error handling are centralized.
- Cached antiforgery tokens are cleared when authentication identity changes.

### Definition of Done

- A new user can register.
- Registration creates default settings.
- Registration is atomic.
- Time zone and display name are stored.
- An existing user can log in.
- The user may choose a temporary or remembered session.
- The user can log out.
- The frontend restores the authenticated user after refresh.
- Anonymous users cannot access protected endpoints.
- Passwords are stored only as hashes.
- State-changing browser requests require antiforgery protection.
- Authentication behavior is covered by backend, frontend, browser, and CI tests.

### Deferred

- Email verification
- Password reset
- Advanced user settings
- External identity providers

---

## Phase 3 — Habit CRUD

**Status: Complete**

### Goal

Allow authenticated users to create and manage their own habits.

### Implemented Scope

- Created the `Habit` entity.
- Added controlled enum values.
- Added Entity Framework Core configuration.
- Added PostgreSQL migration, indexes, foreign keys, and check constraints.
- Added `CreateHabitRequest`.
- Added `UpdateHabitRequest`.
- Added `HabitResponse`.
- Added `HabitService`.
- Added backend-owned validation and normalization.
- Added ownership protection.
- Added deterministic ordering.
- Added soft deactivation.
- Added authenticated create, list, get, update, and deactivate endpoints.
- Derived ownership from authentication claims.
- Returned `404 Not Found` for missing and foreign-owned habits.
- Added React habit list.
- Added habit creation form.
- Added habit editing form.
- Added guarded deactivation flow.
- Reloaded authoritative backend data after create, update, and deactivation.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend API and component tests.
- Manually verified create, edit, and deactivate behavior against PostgreSQL.
- Verified CI on the completed phase.

### Definition of Done

- A logged-in user can create a habit.
- A user sees only their own habits.
- A user edits only their own habits.
- A user deactivates only their own habits.
- Foreign-owned habits are not exposed.
- Deactivation preserves historical data.
- Ownership behavior is covered by automated tests.

### Deferred

- Habit completion
- XP
- Streaks
- Advanced scheduling
- Search and filter polish
- Full game-style habit workspace

---

## Phase 4 — Habit Completion

**Status: Complete**

### Goal

Allow users to complete active habits for their current local date and undo today’s completion safely.

### Implemented Scope

- Created the `HabitCompletion` entity.
- Added Entity Framework Core configuration.
- Added PostgreSQL migration and relationships.
- Added unique database protection for one completion per habit and local date.
- Added backend-owned local-date calculation.
- Used `TimeProvider`.
- Used the user’s stored IANA time zone.
- Added `CompletionService`.
- Added ownership checks.
- Added active-habit checks.
- Added duplicate prevention.
- Added note normalization.
- Added undo behavior.
- Added authenticated completion endpoint.
- Added authenticated undo endpoint.
- Returned `404 Not Found` for missing and foreign-owned habits.
- Returned `409 Conflict` for inactive or already-completed habits.
- Allowed today’s completion to be undone even if the habit was later deactivated.
- Extended `HabitResponse` with `isCompletedToday`.
- Displayed completed and incomplete states in React.
- Updated completion state locally after success.
- Avoided unnecessary full-list reloads.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend API and component tests.
- Manually verified completion and undo through React, ASP.NET Core, and PostgreSQL.
- Matched the default HTTPS backend launch profile to the Vite proxy target.

### Definition of Done

- A user completes an active habit for today.
- The same habit cannot be completed twice for the same local date.
- The user can undo today’s completion.
- Completion records are stored in PostgreSQL.
- Completion rules remain in the backend.
- Duplicate and undo behavior are covered by tests.

### Deferred

- XP rewards
- Streak calculation
- Completion history screen
- Partial completion
- Advanced scheduling

---

## Phase 5 — XP and Attributes

**Status: Complete**

### Goal

Reward habit completion with deterministic XP and persistent character-attribute growth.

### Implemented Scope

- Added `UserAttribute` persistence.
- Added `HabitAttributeReward` persistence.
- Added `XpTransaction` persistence.
- Added `XpService`.
- Added `AttributeService`.
- Defined the eight canonical character attributes.
- Calculated habit rewards automatically from category and difficulty.
- Synchronized stored rewards when category or difficulty changes.
- Applied XP when a habit is completed.
- Reversed XP when today’s completion is undone.
- Stored completion, attribute updates, and XP transactions atomically.
- Calculated attribute levels in the backend.
- Calculated next-level progress in the backend.
- Added overall-level progression formulas for later dashboard use.
- Added `GET /api/attributes`.
- Returned automatic reward details in habit responses.
- Returned awarded XP in completion responses.
- Displayed all attribute progress in React.
- Refreshed attribute progress after completion and undo.
- Displayed immediate earned-XP feedback after successful completion.
- Added backend unit tests.
- Added HTTP integration tests.
- Added frontend API tests.
- Added frontend component tests.
- Manually verified XP gain, persistence, refresh, level progress, and undo.

### Canonical Attributes

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

### Difficulty XP Totals

- Easy: 10 XP
- Medium: 20 XP
- Hard: 30 XP
- Elite: 50 XP

### Reward Allocation

Each habit category maps to a primary and secondary attribute.

- Primary attribute: 70%
- Secondary attribute: 30%

The backend owns:

- difficulty totals,
- category mapping,
- reward allocation,
- attribute-level formulas,
- overall-level formulas.

Clients may display returned values but may not calculate authoritative rewards.

### Definition of Done

- Completing a habit creates XP transactions.
- Completing a habit updates the correct attributes.
- Undoing today’s completion reverses XP and transactions.
- Attribute XP is stored in PostgreSQL.
- Habit responses expose reward breakdowns.
- Completion responses expose awarded XP.
- The frontend displays attribute progress.
- The frontend displays earned-XP feedback.
- XP, reward, level, and attribute behavior remains in backend services.
- Core scenarios are covered by automated tests.
- The complete vertical slice is manually verified.

### Deferred to Phase 6

- Overall user level display
- Dashboard summary
- Current streak
- Longest streak
- Weekly progress
- Recent XP activity

### Deferred beyond Phase 6

- Milestone unlocking
- Achievements
- Complex multipliers
- Advanced analytics
- Level-up cinematic polish

---

## Phase 6 — Dashboard and Streaks

**Status: Next**

### Goal

Create the first real application dashboard where users can act on today’s habits and understand overall progress, XP, attributes, streaks, and basic weekly performance.

### Product Direction

The dashboard is the daily command center.

Its information priority is:

1. today’s habits,
2. complete and undo actions,
3. today’s completion progress,
4. overall XP and level,
5. streak information,
6. attribute summary,
7. recent activity,
8. basic weekly progress.

Charts and secondary analytics must not push daily actions below the fold on ordinary laptop screens.

The first Phase 6 UI may remain visually simple. Phase 6 establishes correct behavior, contracts, aggregation, and tests. Phase 7 applies the full visual system.

### Architecture Direction

Primary services:

- `DashboardService`
- `StreakService`

Supporting services:

- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`

Primary response:

- `DashboardResponse`

Primary endpoint direction:

- authenticated dashboard endpoint under `/api/dashboard`.

The backend must return an aggregated dashboard response.

React must not build dashboard business meaning by combining raw database-like records or duplicating formulas.

### Approved Phase 6 Data

The dashboard may include:

- today’s active habits,
- completed-today state,
- completed count,
- remaining count,
- completion percentage,
- total XP,
- current overall level,
- XP inside the current level,
- XP needed for the next level,
- XP gained today,
- current streak,
- longest streak,
- compact attribute progress,
- recent completions,
- recent XP transactions,
- basic seven-day completion summary,
- basic weekly XP summary.

Only data with implemented backend behavior should appear.

### Streak Rules Must Be Designed Before Coding

Before implementing `StreakService`, document and approve:

- whether streaks are per habit, overall, or both,
- what counts as a successful day,
- how daily and weekly habits affect streaks,
- whether today may be incomplete without breaking a streak,
- how the user’s local time zone is applied,
- how inactive habits affect historical streaks,
- how future dates and missing dates are treated,
- the exact meaning of current streak,
- the exact meaning of longest streak.

Do not infer these rules from UI mockups.

Do not write frontend streak calculations.

### Recommended Implementation Order

#### Slice 1 — Dashboard contract and overall progression

- Inspect existing XP progression behavior.
- Define the first `DashboardResponse`.
- Define focused nested response DTOs only where they improve clarity.
- Return total XP and overall-level progress from backend-owned calculations.
- Add unit tests for overall progression.
- Add HTTP integration coverage.
- Do not build streaks in this slice.

#### Slice 2 — Streak rules and `StreakService`

- Write the approved streak rules in documentation.
- Implement the smallest deterministic streak calculation.
- Use user-local dates.
- Add focused unit tests for boundaries and gaps.
- Test ownership and inactive-history behavior as applicable.
- Keep `StreakService` independent from React.

#### Slice 3 — Dashboard aggregation

- Add `DashboardService`.
- Aggregate today’s habits, completion summary, progression, attributes, streaks, and recent activity.
- Keep the controller thin.
- Add authenticated dashboard endpoint.
- Add integration tests for ownership and response shape.

#### Slice 4 — Frontend dashboard API

- Add frontend dashboard response types.
- Add typed dashboard API module.
- Add API tests.
- Do not duplicate level, percentage, or streak formulas unless the value is purely presentational.

#### Slice 5 — Functional dashboard screen

- Display today’s habits.
- Preserve complete and undo behavior.
- Display completion summary.
- Display overall XP and level.
- Display streaks.
- Display compact attributes.
- Display recent activity.
- Display basic weekly progress.
- Add loading, empty, pending, and error states.
- Add component tests.

#### Slice 6 — Manual acceptance and phase closure

- Verify with a real PostgreSQL-backed user.
- Complete and undo habits.
- Confirm dashboard values change correctly.
- Confirm values survive refresh.
- Confirm another user’s data is not exposed.
- Confirm time-zone behavior.
- Run backend, frontend, integration, and end-to-end tests.
- Update documentation before starting Phase 7.

### Definition of Done

- The dashboard shows today’s habits.
- The dashboard supports complete and undo actions.
- The dashboard shows completed and remaining counts.
- The dashboard shows completion progress.
- The dashboard shows total XP.
- The dashboard shows current overall level.
- The dashboard shows next-level progress.
- The dashboard shows character attributes.
- The dashboard shows current streak.
- The dashboard shows longest streak.
- The dashboard shows basic weekly progress.
- The dashboard may show recent completion or XP activity when implemented.
- Dashboard aggregation is handled by `DashboardService`.
- Streak logic is handled by `StreakService`.
- Overall progression is calculated by backend services.
- React displays authoritative values.
- Core dashboard and streak scenarios are covered by tests.
- The complete flow is manually verified.

### Not Included

- Full game-style visual redesign
- Advanced charts
- Predictive analytics
- AI recommendations
- Notifications
- Reminders
- Social features
- Leaderboards
- Public profiles
- Avatars
- Quests
- Inventory
- Currencies
- Partial completion
- Focus Score

---

## Phase 7 — Game UI Polish

**Status: Planned**

### Goal

Transform the functional MVP into the approved premium, dark, game-inspired personal-development command center without changing backend ownership.

### Canonical Visual Direction

The interface should feel like:

> Elegant productivity software interpreted through an original, restrained anime progression system.

The final visual language uses:

- near-black navy backgrounds,
- deep charcoal-blue navigation,
- slate-black panels,
- thin blue-gray borders,
- electric blue primary actions,
- blue-to-violet progression,
- teal or emerald success,
- restrained gold for streaks and achievements,
- cool white primary text,
- muted gray-blue secondary text,
- subtle original fantasy silhouettes,
- limited glow for selected, active, earned, or important elements.

**Glow is a reward, not wallpaper.**

### Approved Application Shell

- `AppShell`
- persistent sidebar,
- top application bar,
- page header,
- responsive page content,
- desktop-first layout,
- collapsible behavior at narrower laptop widths.

Expected MVP navigation:

- Dashboard
- Habits
- Attributes
- Weekly Review
- Settings

A navigation item must not appear until its screen has real behavior.

### Approved Screen Direction

#### Dashboard

- actions first,
- today summary,
- overall level,
- streaks,
- attributes,
- recent activity,
- weekly summary,
- secondary charts below primary actions.

#### Habits

- Active and Inactive tabs,
- search and filters,
- sorting,
- table or card workspace,
- prominent create action,
- selected-habit details,
- complete,
- edit,
- deactivate,
- automatic reward preview.

Action hierarchy:

1. Complete
2. Edit
3. Deactivate

#### Attributes

- stable attribute colors,
- attribute icons,
- level,
- total XP,
- next threshold,
- progress bars,
- optional deeper summaries based on real data.

#### Weekly Review

- completion rate,
- XP gained,
- streak summary,
- heatmap,
- category comparison,
- deterministic written insights.

Narrative insights must be deterministic before any AI feature is considered.

### Reusable Component Direction

Introduce reusable components only when real screens need them.

Potential foundation:

- `Panel`
- `Card`
- `Button`
- `IconButton`
- `Tabs`
- `Badge`
- `Pill`
- `ProgressBar`
- `Modal`
- `ConfirmationDialog`
- `LoadingSkeleton`
- `EmptyState`
- `ErrorPanel`
- `Toast`

Potential progression components:

- `XpProgressBar`
- `LevelBadge`
- `StreakBadge`
- `AttributeCard`
- `AttributeGrid`
- `XpTransactionList`
- `RewardPreview`

Do not create a speculative design-system framework before actual usage exists.

### Interaction and Motion

Motion should be:

- fast,
- subtle,
- meaningful,
- optional.

Approved uses:

- completion confirmation,
- XP increment,
- progress movement,
- restrained level-up celebration,
- panel transitions.

Avoid:

- constant pulsing,
- repeated particle effects,
- moving backgrounds,
- blocking animation,
- casino-like feedback.

Support reduced motion.

### Accessibility

- sufficient contrast,
- visible keyboard focus,
- semantic HTML,
- descriptive actions,
- no color-only communication,
- accessible progress values,
- accessible chart summaries,
- readable text size,
- responsive laptop layouts,
- consistent loading and error feedback.

Accessibility must be built into reusable components rather than added after visual completion.

### Definition of Done

- The application has a distinctive premium visual identity.
- The application no longer resembles generic CRUD scaffolding.
- Daily actions remain immediately understandable.
- Dashboard, Habits, Attributes, Weekly Review, and Settings share a coherent shell.
- XP, levels, attributes, and streaks are readable at a glance.
- Habit completion provides satisfying but restrained feedback.
- Loading, empty, pending, success, and error states are consistent.
- Keyboard and reduced-motion requirements are respected.
- Desktop and laptop layouts remain usable.
- Business logic remains in backend services.
- React remains responsible for presentation and interaction only.

### Not Included

- Avatar system
- Theme marketplace
- Social UI
- Leaderboard
- Public profile
- Quests
- Inventory
- Currencies
- Large cinematic systems
- Copyrighted anime characters or franchise branding

---

## Phase 8 — Deployment and Project Polish

**Status: Planned**

### Goal

Make the application deployable, stable, understandable, demonstrable, and suitable as a portfolio project.

### Scope

- Review and improve the README.
- Add accurate screenshots.
- Add local setup instructions.
- Document environment variables.
- Review backend and frontend test coverage.
- Add demo or seed data only if useful and safe.
- Deploy the frontend.
- Deploy the backend.
- Deploy PostgreSQL.
- Configure production migrations.
- Configure production authentication-cookie security.
- Decide and document the production frontend/API origin model.
- Prefer a shared public origin when practical.
- Configure explicit production CORS when separate origins are required.
- Protect deployment secrets.
- Verify authentication and antiforgery behavior in production.
- Verify database backup and recovery expectations.
- Verify production startup and migration behavior.
- Improve portfolio-facing project explanation.
- Record architectural decisions and tradeoffs.
- Confirm documentation matches the deployed application.

### Definition of Done

- The application runs locally from documented instructions.
- The application is publicly deployed.
- The repository is organized.
- The README accurately explains the product.
- Screenshots show the real application.
- Core backend and frontend behavior has appropriate automated coverage.
- Production authentication works securely.
- Antiforgery behavior is verified.
- Deployment configuration does not expose secrets.
- Production migrations are understood and repeatable.
- Documentation distinguishes implemented and deferred features.

### Not Included

- Unplanned product features
- Architecture rewrites
- New frameworks
- Mobile application
- Social platform expansion
- Payment system
- Admin panel

---

## Post-MVP Backlog

These features are intentionally deferred until the core application is stable and deployed.

### Progression and Achievement

- `Milestone`
- `UserMilestone`
- achievement cards,
- milestone badges,
- advanced streak badges,
- level-up celebrations,
- title or rank system,
- claimable milestone rewards.

### Personal Identity

- avatar system,
- character portrait,
- profile customization,
- cosmetic themes,
- title selection,
- public-facing profile.

### Expanded Game Systems

- quests,
- inventory,
- currencies,
- collectibles,
- challenge systems.

### Social Systems

- friends,
- leaderboards,
- social comparison,
- shared challenges,
- public profiles,
- accountability groups.

### Engagement Systems

- reminders,
- notifications,
- scheduled nudges,
- activity center.

### Extended Tracking

- advanced scheduling,
- sleep tracking,
- bad-habit tracking,
- journal entries,
- partial completion,
- richer analytics,
- Focus Score,
- AI recommendations.

### Platform Expansion

- mobile application,
- public API,
- integrations,
- calendar integration,
- payments,
- admin panel.

Every post-MVP feature requires:

1. an explicit user problem,
2. an approved product decision,
3. clear architecture ownership,
4. a deterministic contract where applicable,
5. test coverage,
6. documentation,
7. a separate implementation phase or milestone.

---

## Phase Transition Checklist

A phase is not complete until all applicable items are true.

### Behavior

- The user story works through the real frontend.
- The backend validates and owns business rules.
- PostgreSQL stores authoritative state.
- Undo or reversal behavior is verified where applicable.
- Another user’s data is protected.

### Automated Verification

- Focused unit tests pass.
- HTTP integration tests pass.
- Frontend tests pass.
- Production build passes.
- End-to-end coverage is updated when the user journey changes.
- CI passes on the final implementation commit.

### Manual Verification

- The feature is tested through the browser.
- Refresh preserves authoritative state.
- Errors and pending states are checked.
- Local-date behavior is checked when relevant.
- The expected PostgreSQL changes are understood.

### Documentation

- `ROADMAP.md` status is updated.
- `ARCHITECTURE.md` reflects new ownership and flow.
- `DATA_MODEL.md` reflects new persistence.
- `API_CONTRACT.md` reflects actual endpoints and response shapes.
- `NAMING_CONVENTIONS.md` reflects approved names.
- `PROJECT_OVERVIEW.md` reflects current product capability.
- `UI_VISION.md` distinguishes implemented and planned UI.
- `README.md` is updated only when user-facing setup or capability changes warrant it.

### Git Hygiene

- Working tree is inspected.
- Unrelated modifications are removed or separated.
- Intended files are staged explicitly.
- Staged filenames are reviewed.
- Commit message describes one coherent change.
- Branch is pushed and synchronized.

---

## Final Rule

The UI vision defines the destination.

The roadmap defines the sequence.

The architecture defines responsibility.

The database stores the truth.

The backend calculates the system.

The frontend presents the system.

No future chat, developer, or tool should silently change those boundaries.
