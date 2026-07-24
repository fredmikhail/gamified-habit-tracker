# Gamified Habit Tracker — Data Model

**Status:** Current through Phase 7

**Database:** PostgreSQL

**ORM:** Entity Framework Core

## Scope

The schema stores:

- user accounts and settings;
- user-owned habits;
- effective-dated habit configuration;
- completion and undo history;
- habit reward mappings;
- current attribute XP;
- append-only XP transactions.

Phase 7 added derived API read models and presentation behavior. It did not add a persistence table.

## Principles

### PostgreSQL is authoritative

Persistent application state belongs in PostgreSQL.

React does not persist authoritative:

- identity;
- ownership;
- habit configuration;
- completion state;
- XP;
- attributes;
- dashboard summaries;
- streaks.

### Identifiers

Implemented entities use application-generated UUID version 7 values.

EF Core configures identifiers with:

```csharp
ValueGeneratedNever()
```

Clients treat identifiers as opaque.

### Historical meaning

History is preserved through:

- soft habit deactivation;
- effective-dated configuration versions;
- completion-to-configuration linkage;
- undo timestamps;
- signed XP transactions.

Later edits do not reinterpret earlier completions.

### Derived values

The database does not independently store:

- attribute level;
- overall level;
- XP within a level;
- XP required for the next level;
- balance score;
- strongest or needs-focus attribute;
- closest-to-level-up ranking;
- current streak;
- longest streak;
- dashboard cards;
- radar coordinates.

Backend services derive these values.

## Implemented Entities

- `User`
- `UserSettings`
- `Habit`
- `HabitConfigurationVersion`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`

Reserved post-MVP names:

- `Milestone`
- `UserMilestone`

Reserved entities are not implemented.

## Tables

```text
users
user_settings
habits
habit_configuration_versions
habit_completions
habit_attribute_rewards
user_attributes
xp_transactions
__ef_migrations_history
```

Physical identifiers use lowercase `snake_case`.

## Relationship Overview

```text
User
├── 1 UserSettings
├── many Habits
├── many HabitCompletions
├── many UserAttributes
└── many XpTransactions

Habit
├── belongs to 1 User
├── many HabitConfigurationVersions
├── many HabitCompletions
└── many HabitAttributeRewards

HabitConfigurationVersion
├── belongs to 1 Habit
└── many HabitCompletions

HabitCompletion
├── belongs to 1 User
├── belongs to 1 Habit
├── belongs to 1 HabitConfigurationVersion
└── many XpTransactions

HabitAttributeReward
└── belongs to 1 Habit

UserAttribute
└── belongs to 1 User

XpTransaction
├── belongs to 1 User
└── belongs to 1 HabitCompletion
```

## `User`

Purpose: application account and ownership root.

Fields:

- `Id`
- `Email`
- `NormalizedEmail`
- `Username`
- `NormalizedUsername`
- `PasswordHash`
- `CreatedAtUtc`
- `LastLoginAtUtc`

Rules:

- email is required and limited to 254 characters;
- username contains 3 through 30 characters;
- normalized email and username use invariant uppercase;
- normalized values are unique;
- passwords are stored only as hashes;
- `LastLoginAtUtc` is nullable until login.

## `UserSettings`

Purpose: user-specific behavior settings.

Fields:

- `Id`
- `UserId`
- `DisplayName`
- `TimeZone`
- `WeekStartsOn`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Rules:

- one settings row per user;
- display name is required, maximum 50;
- time zone is required, maximum 100;
- time zone must be a valid IANA identifier accepted by the backend;
- `WeekStartsOn` is required;
- new accounts default to Monday.

Allowed `WeekStartDay` values:

- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday
- Sunday

`TimeZone` affects local dates.

`WeekStartsOn` affects:

- weekly periods;
- pending habit configuration boundaries;
- weekly streaks.

The browser theme preference is not stored in `UserSettings`.

## `Habit`

Purpose: user-owned habit identity and lifecycle state.

Fields:

- `Id`
- `UserId`
- `Name`
- `Description`
- `Category`
- `FrequencyType`
- `TargetCount`
- `Difficulty`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`

Rules:

- name is trimmed, required, maximum 100;
- optional description is trimmed, maximum 500, and blank becomes `null`;
- daily frequency requires target 1;
- weekly frequency requires target 1 through 7;
- active state defaults to true.

The current table still contains category, frequency, target, and difficulty columns introduced before configuration versioning.

Date-sensitive behavior uses `HabitConfigurationVersion`.

`Habit` remains authoritative for:

- ownership;
- name;
- description;
- active state;
- lifecycle timestamps.

### Deactivation and activation

Deactivation sets:

```text
IsActive = false
```

Activation sets:

```text
IsActive = true
```

Neither operation deletes configuration, completion, reward, or XP history.

## `HabitConfigurationVersion`

Purpose: effective-dated habit rules.

Fields:

- `Id`
- `HabitId`
- `VersionNumber`
- `Category`
- `FrequencyType`
- `TargetCount`
- `Difficulty`
- `EffectiveFromDate`
- `EffectiveToDateExclusive`
- `CreatedAtUtc`

Effective period:

```text
EffectiveFromDate <= date
AND
(
    EffectiveToDateExclusive IS NULL
    OR date < EffectiveToDateExclusive
)
```

Rules:

- version numbers begin at 1 and are unique per habit;
- only one open-ended current version exists;
- date ranges must not overlap;
- target constraints match the frequency;
- a pending version begins at the next user-local week boundary.

Every habit starts with version 1 effective on its creation local date.

## `HabitCompletion`

Purpose: a persisted completion event.

Fields:

- `Id`
- `UserId`
- `HabitId`
- `HabitConfigurationVersionId`
- `CompletedDate`
- `CompletedAtUtc`
- `UndoneAtUtc`
- `Notes`

Rules:

- `CompletedDate` is calculated by the backend;
- `CompletedAtUtc` records the exact UTC event time;
- optional notes are normalized;
- each completion links to the effective configuration;
- undone records remain stored.

A partial unique index permits only one completion for a habit and local date where:

```text
undone_at_utc IS NULL
```

An undone completion therefore does not block a later replacement completion for the same date.

## `HabitAttributeReward`

Purpose: current reward mapping for a habit.

Fields:

- `Id`
- `HabitId`
- `AttributeType`
- `XpAmount`

Rules:

- a habit maps to a primary and secondary attribute;
- XP amounts follow backend difficulty and category rules;
- the current mapping supports immediate completion behavior;
- the completion’s configuration link preserves the historical rule context.

## `UserAttribute`

Purpose: current XP total for one user and attribute.

Fields:

- `Id`
- `UserId`
- `AttributeType`
- `CurrentXp`
- `UpdatedAtUtc`

Rules:

- `(UserId, AttributeType)` is unique;
- XP cannot be negative;
- rows may be sparse;
- API reads still return all eight attributes, using zero when a row does not yet exist.

Levels are calculated from `CurrentXp`.

## `XpTransaction`

Purpose: signed progression ledger entry.

Fields:

- `Id`
- `UserId`
- `HabitCompletionId`
- `AttributeType`
- `Amount`
- `Reason`
- `CreatedAtUtc`

Rules:

- positive amounts award XP;
- negative amounts reverse XP;
- zero amounts are invalid;
- each transaction belongs to a completion;
- normal undo appends negative rows;
- original rows are not edited or deleted to represent reversal.

Current reasons include:

- `Habit completion`
- `Habit completion undo`

## Controlled Values

### `HabitFrequencyType`

- Daily
- Weekly

### `HabitDifficulty`

- Easy
- Medium
- Hard
- Elite

### `AttributeType`

- Discipline
- Fitness
- Vitality
- Focus
- Mind
- Resilience
- Social
- Purpose

### `HabitCategory`

- FitnessAndMovement
- HealthAndRecovery
- LearningAndSkills
- WorkAndCareer
- DailyLifeAndOrganization
- MoneyAndFinance
- RelationshipsAndCommunity
- EmotionalWellBeing
- SpiritualityAndPurpose
- CreativityAndRecreation
- SelfControlAndBoundaries
- GeneralGrowth

Enums are stored as strings with check constraints where configured.

## Delete Behavior

Normal product actions use soft lifecycle changes rather than deletion.

Configured relationship deletes protect ownership cleanup if a user is removed.

Feature code must not casually delete:

- completion history;
- configuration history;
- XP history.

## Time Model

Use `DateTime` or `DateTimeOffset` values ending in `Utc` for exact timestamps.

Use `DateOnly` for user-local calendar dates.

The backend derives a local date from:

```text
current UTC instant + stored IANA time zone
```

The client cannot choose the completion date.

## Atomicity

Registration creates `User` and `UserSettings` together.

Habit creation creates the habit, initial configuration, and reward mapping together.

Completion creates the completion, positive XP transactions, and attribute updates in one EF Core unit of work.

Undo records the undo, negative XP transactions, and attribute reversals in one unit of work.

## Derived Read Models

### Dashboard

`DashboardResponse` combines:

- overall progression;
- today activity;
- daily execution;
- active habit actions and reward details;
- attributes;
- per-habit streaks.

No dashboard table exists.

### Attribute overview

`AttributeOverviewResponse` combines:

- all attributes;
- total attribute XP;
- balance score;
- strongest attribute;
- needs-focus attribute;
- closest-to-level-up queue;
- recent XP transactions.

No attribute-overview, radar, balance, or ranking table exists.

## Index and Constraint Summary

The schema protects:

- normalized email uniqueness;
- normalized username uniqueness;
- one settings row per user;
- user ownership relationships;
- habit active-query access;
- unique configuration version numbers;
- valid effective periods;
- daily and weekly target rules;
- one current user-attribute row per type;
- non-negative current attribute XP;
- non-zero XP transaction amounts;
- one active completion per habit and local date.

## Migrations

Schema changes are made through EF Core migrations.

Before release:

```cmd
dotnet ef migrations has-pending-model-changes --project server\HabitTracker.Api\HabitTracker.Api.csproj --startup-project server\HabitTracker.Api\HabitTracker.Api.csproj
```

Generated migrations are historical schema artifacts. They are not rewritten during unrelated feature work.

A database must receive all committed migrations before it runs current application code.
