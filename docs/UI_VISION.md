# Gamified Habit Tracker — Canonical UI Vision

**Status:** Approved visual and product direction
**Scope:** Desktop-first web application
**Current development phase:** Phase 5 complete; Phase 6 next
**Role:** Visual and UX source of truth
**Implementation principle:** Future-compatible, not future-built

---

## 1. Purpose

This document records the approved long-term visual and product direction for the Gamified Habit Tracker.

The project’s approved UI reference images are the canonical visual target. Future frontend, backend, DTO, service, database, analytics, and documentation decisions should move the application toward this experience while preserving the fixed technology stack, architecture, naming conventions, phased roadmap, and MVP scope.

The target is not merely a dark interface with game terminology.

The target is:

> A premium personal-development command center that presents real habit data as elegant character progression.

The application should feel like sophisticated desktop productivity software interpreted through an original, restrained anime-inspired leveling system.

It should remain attractive and usable to:

- productivity-focused users,
- gamers,
- anime fans,
- casual users,
- friends and family who do not normally use gamified applications.

The visual atmosphere may be dramatic. The interaction design must remain clear, calm, readable, and practical.

---

## 2. Product Experience

The application should help users feel that consistent real-world action develops a persistent character.

The core product loop is:

1. The user creates meaningful habits.
2. The user completes those habits in real life.
3. The backend validates and records the completion.
4. The completion awards deterministic XP.
5. XP develops the appropriate character attributes.
6. Levels, streaks, summaries, and progress visualize long-term growth.
7. The interface makes that growth feel rewarding without becoming manipulative.

The application should communicate:

- control,
- momentum,
- discipline,
- growth,
- identity,
- quiet power,
- precision,
- confidence.

It should not feel:

- childish,
- casino-like,
- noisy,
- cluttered,
- manipulative,
- excessively animated,
- dependent on fake currencies,
- dependent on unsupported scores.

Gamification should reinforce genuine progress rather than distract from it.

---

## 3. Current Product Reality

Documentation must clearly distinguish between implemented behavior, the next phase, later MVP work, and post-MVP ideas.

### 3.1 Implemented

The working application currently supports:

- user registration,
- user login and logout,
- authenticated user-specific data,
- habit creation,
- habit editing,
- habit deactivation,
- daily habit completion,
- undoing today’s completion,
- duplicate completion prevention,
- time-zone-aware completion dates,
- automatic category-based attribute rewards,
- difficulty-based XP totals,
- persisted XP transactions,
- persisted user attribute XP,
- XP reversal when today’s completion is undone,
- backend-calculated attribute levels,
- backend-calculated next-level progress,
- eight character attributes,
- frontend attribute progress cards,
- immediate earned-XP feedback after completion.

### 3.2 Phase 6 — Next

Phase 6 will implement:

- `DashboardService`,
- `StreakService`,
- an aggregated dashboard API,
- overall user XP and level presentation,
- today’s completion summary,
- current streak,
- longest streak,
- recent activity,
- basic weekly progress,
- the first real dashboard screen.

### 3.3 Phase 7 — Planned MVP work

Phase 7 will transform the functional frontend into the approved game-style interface through:

- an application shell,
- sidebar navigation,
- a top application bar,
- a dark visual system,
- reusable panels and cards,
- a refined habit-management workspace,
- progression components,
- responsive desktop behavior,
- restrained animation,
- polished loading, empty, success, and error states.

### 3.4 Deferred post-MVP

Features such as avatars, currencies, quests, leaderboards, notifications, social systems, public profiles, and AI recommendations are not implemented and must not be presented as current capabilities.

---

## 4. Golden Implementation Principle

### Future-compatible, not future-built

Every implementation choice should consider the approved UI direction, but the project must not prematurely build post-MVP systems.

Prepare for the target UI through:

- stable domain entities,
- stable enum values,
- purpose-built DTOs,
- thin controllers,
- business logic in backend services,
- auditable XP transactions,
- reusable React components,
- aggregate dashboard responses,
- server-owned calculations,
- clear phase boundaries.

Do not prepare for the target UI by adding speculative:

- database columns,
- placeholder tables,
- fake currencies,
- unsupported ranks,
- empty endpoints,
- avatar infrastructure,
- social infrastructure,
- notification systems,
- generalized plugin systems,
- abstract configuration engines.

The architecture should leave useful doors open without constructing unused rooms.

---

## 5. Canonical Visual Identity

### 5.1 Emotional tone

The interface should communicate:

- control,
- momentum,
- discipline,
- personal growth,
- mystery,
- quiet power,
- precision,
- calm confidence.

It should not feel:

- childish,
- cluttered,
- casino-like,
- noisy,
- cartoonish,
- manipulative,
- overloaded with effects.

### 5.2 Visual blend

The approved direction combines two influences.

#### Premium productivity software

- clean alignment,
- generous spacing,
- clear hierarchy,
- strong readability,
- calm surfaces,
- obvious primary actions,
- efficient desktop workflows.

#### Subtle anime progression atmosphere

- dark midnight surfaces,
- electric blue and violet energy,
- crest-like level symbols,
- restrained fantasy silhouettes,
- glowing progression bars,
- XP and levels,
- attributes and streaks,
- occasional achievement moments.

The anime influence should shape the atmosphere, not turn the interface into a character-heavy game screen.

### 5.3 Color language

| Role | Visual direction |
|---|---|
| Page background | Near-black navy |
| Sidebar | Deep charcoal blue |
| Panels | Slate-black |
| Borders | Thin blue-gray |
| Selected surface | Faint cobalt blue |
| Primary action | Electric blue |
| Progression | Blue-to-violet |
| Success | Teal or emerald |
| Streak and achievement | Gold or amber |
| Warning | Amber |
| Danger | Restrained red |
| Primary text | Cool white |
| Secondary text | Muted gray-blue |

**Rule:** Glow is a reward, not wallpaper.

Glow should be reserved for:

- selected items,
- active progression,
- earned rewards,
- level-up moments,
- important interactive elements.

Constant glow across every panel would weaken hierarchy and reduce readability.

### 5.4 Shapes

#### Rectangular panels

Use for:

- page sections,
- dashboard summaries,
- habit containers,
- attribute cards,
- analytics panels.

Preferred treatment:

- subtle radius,
- one-pixel border,
- dark interior,
- occasional soft internal gradient,
- generous padding.

#### Pills

Use for:

- tabs,
- filters,
- statuses,
- categories,
- difficulty,
- date ranges.

#### Circles

Use for:

- completion controls,
- icons,
- status indicators,
- donut charts,
- profile imagery.

#### Crests, shields, and hexagons

Reserve for:

- level identity,
- streak milestones,
- achievements,
- meaningful progression,
- high-value status.

They should not replace ordinary buttons or controls.

### 5.5 Typography

Use a modern, highly readable sans-serif font with a slightly futuristic character.

Suitable candidates include:

- Inter,
- Geist,
- Manrope.

Typography hierarchy:

- page title: large and medium-weight,
- important number: large and bold,
- card label: small and muted,
- body text: readable medium gray,
- metadata: smaller gray-blue,
- active values: semantic accent color.

Use tabular numerals where possible for:

- XP,
- levels,
- percentages,
- streaks,
- dates,
- chart values.

---

## 6. Canonical Application Shell

### 6.1 Sidebar

The sidebar is the permanent application spine.

Recommended structure:

- original product crest,
- `GAMIFIED HABIT TRACKER`,
- short slogan,
- primary navigation,
- compact user progression summary,
- subtle original fantasy silhouette artwork.

The artwork must be original and must not copy copyrighted anime characters, franchise symbols, or game logos.

### 6.2 MVP navigation

The intended MVP navigation is:

- Dashboard
- Habits
- Attributes
- Weekly Review
- Settings

A screen should not appear in navigation until it has real behavior and real data.

### 6.3 Future navigation concepts

These concepts are visually acceptable but remain deferred unless separately approved:

- Today as a separate page,
- Quests,
- Inventory,
- Journal,
- Achievements,
- Leaderboard,
- Notifications,
- public rank,
- avatar customization.

### 6.4 Top application bar

Potential elements include:

- current page title,
- current date or selected date range,
- quiet global search,
- current level,
- overall XP progress,
- profile control,
- future notification control.

The top bar must only show values backed by implemented domain behavior.

It must not display decorative currencies, fabricated ranks, placeholder notifications, or unsupported scores.

---

## 7. Canonical Screens

## 7.1 Dashboard

### Purpose

> The daily command center where the user acts first and reviews progress second.

### Primary information

- today’s habits,
- complete and undo controls,
- today’s completion summary,
- current streak,
- overall XP and level,
- XP gained today,
- recent completions,
- recent XP activity,
- compact attribute overview,
- basic weekly progress.

### Priority order

1. Today’s habits
2. Completion controls
3. Completion progress
4. Overall XP and level
5. Streak information
6. Attribute summary
7. Recent activity
8. Charts and secondary analysis

Charts must not push primary daily actions below the fold on normal laptop screens.

### Backend ownership

Primary service:

- `DashboardService`

Supporting services:

- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`

Primary response:

- `DashboardResponse`

React should not reconstruct dashboard business meaning by independently combining many unrelated API responses.

The backend should aggregate the data and return a purpose-built dashboard response.

---

## 7.2 Habits

### Purpose

> The user’s desktop habit-management workspace.

### Approved UX

- Active and Inactive tabs,
- search,
- category filter,
- frequency filter,
- difficulty filter,
- sorting,
- optional table or card presentation,
- prominent Create Habit action,
- selected-habit detail drawer,
- completion state,
- Edit,
- Deactivate,
- Complete,
- Undo,
- automatic reward preview.

### Action hierarchy

1. Complete Habit
2. Edit Habit
3. Deactivate Habit

Deactivation is destructive and should remain visually secondary.

### Reward presentation

Reward preview must use authoritative backend data.

Current reward rules:

- Easy: 10 total XP
- Medium: 20 total XP
- Hard: 30 total XP
- Elite: 50 total XP
- primary attribute: 70%
- secondary attribute: 30%

The frontend may display reward values returned by the API.

The frontend must not calculate authoritative XP from category or difficulty.

A dedicated reward preview on habit cards remains planned frontend presentation work. It is not required to change backend reward rules.

---

## 7.3 Attributes

### Purpose

> The user’s personal character sheet.

### Approved attributes

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

### Attribute cards

Each card may show:

- icon,
- stable accent color,
- current level,
- total XP,
- XP inside the current level,
- XP required for the next level,
- progress bar,
- progress percentage.

### Deeper attribute views

Future views may show:

- radar chart,
- strongest attribute,
- lowest attribute,
- overall balance,
- attribute explanation,
- recent related XP transactions,
- XP grouped by source.

These views must be based on deterministic backend data.

### Stable color identity

| Attribute | Suggested accent |
|---|---|
| Discipline | Emerald |
| Fitness | Orange or gold |
| Vitality | Red |
| Focus | Electric blue |
| Mind | Violet |
| Resilience | Amber or shield gold |
| Social | Teal |
| Purpose | Rose-violet |

The same attribute should retain the same label, icon, and accent across every screen.

### Backend ownership

Primary service:

- `AttributeService`

Current response:

- `UserAttributeResponse`

The backend calculates:

- level,
- XP inside the current level,
- XP needed for the next level.

React only visualizes those values.

---

## 7.4 Weekly Review

### Purpose

> Help the user understand behavior without overwhelming, shaming, or judging them.

### Approved information

- weekly completion rate,
- weekly XP gained,
- habits completed,
- current streak,
- longest streak,
- completion heatmap,
- category comparison,
- completion donut,
- simple trend comparison,
- strongest category,
- most frequently missed habit,
- deterministic insight summaries.

Example:

> Learning & Skills had your highest completion rate this week at 88%.

Narrative insights should initially be deterministic summaries, not AI recommendations.

Possible backend ownership:

- `DashboardService`,
- focused summary methods added only when responsibility becomes distinct.

No new analytics service should be introduced merely to match a visual mockup.

---

## 7.5 Settings

### Purpose

> Allow the user to manage account and display behavior without mixing settings into feature screens.

MVP-relevant settings may include:

- display name,
- time zone,
- account information,
- future visual preferences when implemented.

The time zone is domain-significant because it determines the user’s local completion date.

Visual preferences should only be persisted after the interface actually supports them.

---

## 8. Reusable Frontend Direction

The final interface should use reusable components instead of styling each page independently.

### 8.1 Application layout

Potential components:

- `AppShell`
- `Sidebar`
- `TopBar`
- `PageContent`
- `PageHeader`
- `Toolbar`
- `DetailDrawer`

### 8.2 Foundation components

Potential components:

- `Panel`
- `Card`
- `SectionHeader`
- `Button`
- `IconButton`
- `Tabs`
- `Badge`
- `Pill`
- `ProgressBar`
- `SearchInput`
- `Select`
- `Tooltip`
- `Modal`
- `ConfirmationDialog`

### 8.3 Habit components

Potential components:

- `HabitTable`
- `HabitRow`
- `HabitCard`
- `HabitFilters`
- `DifficultyIndicator`
- `HabitCompletionButton`
- `HabitDeactivateButton`
- `HabitDetailPanel`
- `RewardPreview`
- `HabitForm`
- `HabitEditForm`

### 8.4 Progression components

Potential components:

- `XpProgressBar`
- `LevelBadge`
- `StreakBadge`
- `AttributeCard`
- `AttributeGrid`
- `XpTransactionList`

### 8.5 Analytics components

Potential components:

- `StatCard`
- `DonutChart`
- `LineChart`
- `RadarChart`
- `Heatmap`
- `CategoryProgressList`
- `InsightCard`

### 8.6 Application-state components

Potential components:

- `LoadingSkeleton`
- `EmptyState`
- `ErrorPanel`
- `PendingButton`
- `Toast`
- `InlineValidationMessage`

These names describe the intended design system. They are not permission to create every component in advance.

Components should be introduced when a real screen needs them.

React components may own:

- presentation,
- interaction,
- local display state,
- pending state,
- selected state,
- typed API orchestration.

React components must not own:

- XP formulas,
- reward mapping,
- level formulas,
- streak rules,
- date ownership,
- completion validation,
- authorization,
- database-like aggregation.

---

## 9. Required UI States

Major interactive components should support states appropriate to their behavior.

### 9.1 General states

- default,
- hover,
- keyboard focus,
- active,
- selected,
- disabled,
- loading,
- success,
- warning,
- error,
- empty.

### 9.2 Habit states

- active,
- inactive,
- completed today,
- incomplete today,
- completion pending,
- undo pending,
- selected,
- unavailable because inactive,
- API error.

### 9.3 Attribute states

- no XP,
- normal progress,
- near level-up,
- level-up,
- highest attribute,
- lowest attribute,
- recently changed.

### 9.4 Chart states

- loading,
- no data,
- partial period,
- complete period,
- tooltip interaction,
- accessible text summary.

### 9.5 Motion

Motion should be:

- fast,
- subtle,
- meaningful,
- optional.

Good uses:

- progress movement,
- completion confirmation,
- XP increment,
- restrained level-up celebration,
- panel transition.

Avoid:

- constant pulsing,
- excessive particles,
- repeated full-screen animation,
- moving backgrounds that reduce readability.

Reduced-motion preferences must be respected during Phase 7 polish.

---

## 10. Architecture Alignment

The visual target must preserve the current architecture.

### 10.1 Frontend

React, TypeScript, Vite, and Tailwind CSS own:

- layout,
- rendering,
- interaction,
- temporary display state,
- typed API calls.

### 10.2 Backend

ASP.NET Core services own:

- authentication behavior,
- authorization,
- validation,
- habit rules,
- completion rules,
- XP calculation,
- attribute calculation,
- streak calculation,
- dashboard aggregation.

### 10.3 Database

PostgreSQL remains the source of truth for:

- users,
- settings,
- habits,
- completions,
- stored reward mappings,
- user attribute XP,
- XP transactions.

### 10.4 DTOs

DTOs define the boundary between backend behavior and frontend presentation.

Database entities must not be returned directly.

---

## 11. Service Responsibilities

Canonical services remain:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

### `HabitService`

Owns:

- habit creation,
- habit editing,
- retrieval,
- deactivation,
- habit configuration validation,
- automatic reward synchronization when category or difficulty changes.

### `CompletionService`

Owns:

- completing a habit for today,
- undoing today’s completion,
- ownership checks,
- active-habit checks,
- duplicate completion prevention,
- local-date behavior,
- orchestration of XP application and reversal.

### `XpService`

Owns:

- difficulty XP totals,
- category-to-attribute mapping,
- reward calculation,
- attribute level formulas,
- overall level formulas.

### `AttributeService`

Owns:

- applying completion rewards,
- reversing completion rewards,
- user attribute retrieval,
- attribute progress responses,
- persisted XP updates.

### `StreakService`

Will own:

- current streak,
- longest streak,
- streak continuity,
- treatment of local dates,
- deterministic streak rules.

### `DashboardService`

Will own:

- aggregated dashboard state,
- today summary,
- overall progression,
- streak summary,
- recent activity,
- weekly summary data.

The frontend should not reconstruct these concepts from raw records.

---

## 12. DTO and API Principles

Future API work should support the approved UI with purpose-built response shapes.

Current and planned examples:

- `HabitResponse`
- `HabitAttributeRewardResponse`
- `CompleteHabitResponse`
- `UserAttributeResponse`
- `XpTransactionResponse`
- `DashboardResponse`

Rules:

1. Return only data required by implemented screens.
2. Do not return database entities directly.
3. Keep enum values stable.
4. Keep calculations authoritative on the server.
5. Use aggregate responses when they reduce unnecessary frontend orchestration.
6. Preserve auditability for completion, XP, and undo.
7. Do not add speculative fields because a mockup displays them.
8. Add fields when their behavior is implemented and tested.
9. Keep APIs usable by future clients without moving business logic into clients.
10. Do not generalize current endpoints for imaginary future systems.

When a backend DTO changes, review all of the following together:

- backend response DTO,
- backend service mapping,
- backend unit tests,
- HTTP integration tests,
- frontend TypeScript response type,
- frontend API test fixtures,
- component test fixtures,
- consuming components.

---

## 13. Analytics Direction

### 13.1 MVP and planned core analytics

- today completion count,
- today completion percentage,
- total XP,
- XP gained today,
- current overall level,
- next-level progress,
- current streak,
- longest streak,
- recent completions,
- recent XP transactions,
- weekly completion rate,
- weekly XP gain,
- category completion comparison,
- attribute progress.

### 13.2 Future analytics requiring explicit design

- partial completion,
- Focus Score,
- balance score,
- trend scoring,
- difficulty effectiveness,
- predictive insights,
- AI recommendations,
- social comparison,
- public rank,
- economy analytics.

No score should be introduced without:

- a precise definition,
- a clear user benefit,
- a deterministic formula,
- backend ownership,
- automated tests,
- documentation.

---

## 14. Approved Post-MVP Backlog

The following ideas are visually approved but deferred.

### 14.1 Progression and achievement

- achievement cards,
- milestone badges,
- elaborate streak badges,
- level-up celebrations,
- title and rank system,
- claimable milestone rewards.

### 14.2 Personal identity

- avatar system,
- character portrait,
- profile customization,
- cosmetic themes,
- title selection,
- public-facing profile.

### 14.3 Expanded game systems

- quests,
- inventory,
- currencies,
- collectibles,
- challenge systems.

### 14.4 Social systems

- leaderboard,
- public rank,
- friends,
- social comparison,
- shared challenges,
- public profiles.

### 14.5 Engagement systems

- notifications,
- reminders,
- scheduled nudges,
- inbox or activity center.

### 14.6 Extended personal tracking

- journal,
- advanced weekly review,
- advanced analytics,
- AI-generated recommendations,
- partial completion,
- focus score.

Each requires an explicit product and architecture decision after the MVP is stable.

---

## 15. Mockup-to-System Corrections

The reference images define visual direction, not exact domain contracts.

The real application must:

- use the approved habit categories,
- use the approved eight attributes,
- use exact difficulty XP totals,
- use the approved 70/30 reward split,
- keep XP authoritative on the backend,
- keep completion dates authoritative on the backend,
- avoid partial completion unless explicitly designed,
- avoid unsupported Focus Score calculations,
- avoid fake currencies,
- avoid fabricated ranks,
- avoid copied anime characters and franchise logos,
- use original decorative artwork,
- preserve accessibility,
- preserve readability,
- display only implemented behavior as real.

When visual placeholder content conflicts with the domain model, preserve the visual language and replace the placeholder with accurate application data.

---

## 16. Accessibility and Desktop Usability

The visual target must remain usable beyond its appearance.

Requirements:

- sufficient text contrast,
- no color-only status communication,
- visible keyboard focus,
- keyboard-friendly navigation,
- readable text sizing,
- semantic HTML,
- accessible chart summaries,
- descriptive button labels,
- responsive laptop layouts,
- reduced-motion support,
- clear errors,
- loading feedback,
- pending feedback,
- confirmation for destructive actions.

Responsive desktop behavior:

- sidebar may collapse at narrower widths,
- detail panels may become drawers,
- charts may stack,
- critical daily actions must remain above secondary analytics,
- dense tables must remain horizontally usable.

Accessibility should be designed into reusable components during Phase 7, not added as a final patch.

---

## 17. Phase Mapping

### Phase 6 — Dashboard and Streaks

Build the real data and behavior required by:

- dashboard summary,
- overall progression,
- streaks,
- weekly progress,
- recent activity.

The first Phase 6 screens may remain visually simple while contracts and business logic are established.

### Phase 7 — Game UI Polish

Implement the approved visual system:

- application shell,
- dark color system,
- sidebar,
- top bar,
- reusable panels,
- progression cards,
- habit-management workspace,
- dashboard composition,
- responsive behavior,
- subtle motion,
- polished empty, loading, and error states.

Phase 7 must not move business logic into React.

### Phase 8 — Deployment and Project Polish

Focus on:

- production configuration,
- deployment,
- environment documentation,
- screenshots,
- portfolio presentation,
- stability,
- final documentation,
- project demonstration.

Phase 8 should not become an excuse to add unrelated product complexity.

---

## 18. Development Guardrails

The following rules exist to prevent phase drift and documentation/code mismatches.

### 18.1 Before editing

- Inspect the actual current file.
- Confirm whether behavior is implemented, next, planned, or deferred.
- Identify the exact owning service, DTO, API module, and React component.
- Avoid assuming a file contains changes that were never applied.

### 18.2 When an API response changes

Review:

- backend response DTO,
- backend service mapping,
- backend tests,
- frontend TypeScript response type,
- frontend API test fixture,
- component test fixtures,
- consuming components.

### 18.3 During frontend changes

- Use backend-returned XP and progress values.
- Do not duplicate backend formulas.
- Keep local state limited to display and interaction.
- Run focused tests first.
- Run the full frontend test suite afterward.
- Run formatting, linting, and the production build.

### 18.4 Before committing

Run:

```text
git status --short
git diff --check
git diff --cached --name-only
```

Confirm:

- no unrelated file was modified,
- every intended file is staged,
- no accidental duplicate file replacement occurred,
- generated output is not staged,
- the commit represents one understandable change.

### 18.5 At phase completion

- manually verify the complete feature loop,
- run all relevant tests,
- update the roadmap,
- update affected architecture, API, and data-model documentation,
- record implemented behavior separately from planned behavior,
- identify the exact first step of the next phase.

---

## 19. Reference Image Set

The approved reference images define the visual source of truth for:

- Dashboard,
- Habits,
- Attributes,
- Weekly Review and Insights.

Use them to preserve:

- layout proportions,
- information hierarchy,
- dark visual language,
- component shapes,
- semantic colors,
- navigation style,
- chart style,
- subtle progression atmosphere,
- desktop-first usability.

The images should guide design decisions without forcing fake or unsupported data into the application.

---

## 20. Canonical Design Brief

> A desktop-first, dark premium habit-tracking command center with electric blue, cyan, violet, and restrained gold accents. It should feel like an original anime progression system interpreted through elegant modern productivity software. The interface uses structured data panels, strong numbers, thin borders, subtle glow, atmospheric silhouettes, clear primary actions, and reusable progression components. Gamification should feel rewarding and mysterious, never childish, noisy, manipulative, or casino-like.

Engineering translation:

> React and Tailwind render a reusable visual system. Backend DTOs provide authoritative data. Services calculate XP, attributes, streaks, and dashboard summaries. PostgreSQL stores the truth. The interface visualizes the system; it does not invent the system.

---

## 21. Final Rule

The UI references define where the product is heading.

The roadmap defines when each capability may be built.

The architecture defines where each responsibility belongs.

Documentation must always distinguish between:

- implemented,
- next,
- planned,
- post-MVP.

The architecture must support the destination without skipping the journey.
