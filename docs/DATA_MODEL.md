# Gamified Habit Tracker — Data Model

**Status:** Current through Phase 6

**Database:** PostgreSQL

**ORM:** Entity Framework Core

**Naming:** C# uses PascalCase; PostgreSQL uses `snake_case`

---

## 1. Purpose

This document defines the persistent model used by the Gamified Habit Tracker.

The schema supports:

- authenticated user ownership,
- user-local calendar behavior,
- daily and weekly habits,
- effective-dated habit rules,
- scheduled configuration changes,
- habit completion history,
- auditable completion undo,
- deterministic XP awards,
- append-only XP reversals,
- current attribute XP,
- overall progression,
- dashboard aggregation,
- daily and weekly streak calculation.

The database stores facts and durable history.

Levels, dashboard summaries, and streaks are derived by backend services rather than stored as mutable counters.

---

## 2. Data-Model Principles

### 2.1 PostgreSQL is authoritative

Persistent product state belongs in PostgreSQL.

React may hold temporary interface state, but it does not permanently own:

- authenticated identity,
- user settings,
- habit ownership,
- habit configuration,
- completion state,
- XP history,
- attribute totals,
- dashboard values,
- streak values.

### 2.2 Application-generated identifiers

Implemented entities use application-generated Guid version 7 identifiers.

Entity Framework Core configures these identifiers with:

```csharp
ValueGeneratedNever()
```

PostgreSQL therefore preserves the identifiers created by the application.

### 2.3 Historical meaning is preserved

Past events must retain the rules that applied when they occurred.

The model preserves history through:

- soft habit deactivation,
- effective-dated habit configuration versions,
- completion-to-configuration linkage,
- completion undo timestamps,
- signed append-only XP transactions.

A later habit edit does not reinterpret an earlier completion.

### 2.4 Calculated values are not stored unnecessarily

The schema does not store independent columns for:

- attribute level,
- overall level,
- XP within the current level,
- XP required for the next level,
- current streak,
- longest streak,
- completion percentage,
- dashboard summaries.

These values are calculated from persisted state.

### 2.5 Undo is not deletion

Normal completion undo does not delete:

- the completion,
- the original XP award transactions,
- the configuration used by the completion.

Undo records:

- when the completion was undone,
- signed XP reversal transactions,
- the resulting current attribute totals.

### 2.6 Deferred systems do not receive placeholder schema

The current schema does not include speculative tables for:

- avatars,
- currencies,
- quests,
- reminders,
- notifications,
- social relationships,
- leaderboards,
- public profiles,
- AI recommendations,
- complex analytics.

A new persistent concept requires an implemented behavior and a deliberate schema decision.

---

## 3. Implemented Entities

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

The reserved entities are not currently implemented.

---

## 4. PostgreSQL Tables

Application tables:

- `users`
- `user_settings`
- `habits`
- `habit_configuration_versions`
- `habit_completions`
- `habit_attribute_rewards`
- `user_attributes`
- `xp_transactions`

Entity Framework Core migration history:

- `__ef_migrations_history`

Physical PostgreSQL identifiers use lowercase `snake_case`, including:

- table names,
- column names,
- primary keys,
- foreign keys,
- indexes,
- check constraints.

---

## 5. Relationship Overview

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

Reserved post-MVP relationships:

```text
User
└── many UserMilestones

Milestone
└── many UserMilestones

UserMilestone
├── belongs to 1 User
└── belongs to 1 Milestone
```

---

## 6. Entity: `User`

Represents an application account and the root ownership boundary.

### Fields

- `Id`
- `Email`
- `NormalizedEmail`
- `Username`
- `NormalizedUsername`
- `PasswordHash`
- `CreatedAtUtc`
- `LastLoginAtUtc`

### Rules

- `Id` is a backend-generated Guid version 7.
- `Email` is required and limited to 254 characters.
- `NormalizedEmail` is required and limited to 254 characters.
- `Username` is required and contains 3 through 30 characters.
- `NormalizedUsername` is required and limited to 30 characters.
- `PasswordHash` is required.
- `CreatedAtUtc` is required.
- `LastLoginAtUtc` remains nullable until a successful login.

### Normalization

Before persistence and lookup:

- email is trimmed,
- username is trimmed,
- normalized forms use invariant uppercase casing,
- passwords are not trimmed or normalized.

The accepted display forms of email and username are retained separately from their normalized lookup forms.

### Relationships

A user may own:

- one `UserSettings`,
- many `Habit` rows,
- many `HabitCompletion` rows,
- many `UserAttribute` rows,
- many `XpTransaction` rows.

### Constraints and indexes

- primary key on `id`,
- unique index on `normalized_email`,
- unique index on `normalized_username`,
- configured maximum lengths,
- application-supplied identifier.

### Ownership

Authenticated API operations derive the user identifier from backend claims.

User-owned requests do not accept an authoritative client-supplied `UserId`.

Missing and foreign-owned resources normally produce the same non-revealing response.

---

## 7. Entity: `UserSettings`

Stores user-specific settings that affect application behavior.

### Fields

- `Id`
- `UserId`
- `DisplayName`
- `TimeZone`
- `WeekStartsOn`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### Rules

- `Id` is a backend-generated Guid version 7.
- `UserId` is required.
- `DisplayName` is required and limited to 50 characters.
- `TimeZone` is required and limited to 100 characters.
- `WeekStartsOn` is required.
- `CreatedAtUtc` is required.
- `UpdatedAtUtc` is required.

### `WeekStartsOn`

`WeekStartsOn` is stored as a string-backed `WeekStartDay` value.

Allowed values:

- `Monday`
- `Tuesday`
- `Wednesday`
- `Thursday`
- `Friday`
- `Saturday`
- `Sunday`

The default is:

```text
Monday
```

A database check constraint rejects unsupported values.

### Relationships

- one `UserSettings` belongs to one `User`,
- `UserId` has a unique index,
- deleting the related user cascades to the settings row.

The application creates the user and settings together during registration.

### Registration defaults

Registration initializes:

- `DisplayName` from the accepted username,
- `TimeZone` from the validated request,
- `WeekStartsOn` as Monday,
- matching creation and update timestamps.

### Calendar significance

`TimeZone` is used for:

- current local-date calculation,
- habit completion,
- completion undo,
- habit configuration effective dates,
- pending configuration boundaries,
- dashboard activity,
- streak calculation.

`WeekStartsOn` is used for:

- weekly calendar periods,
- scheduled habit rule changes,
- weekly streak boundaries.

Changing calendar settings does not rewrite historical dates already stored in the database.

---

## 8. Entity: `Habit`

Represents a user-owned habit and its non-versioned lifecycle state.

### Fields

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

### Rules

- `Id` is a backend-generated Guid version 7.
- `UserId` is required.
- `Name` is required, trimmed, and limited to 100 characters.
- `Description` is optional, trimmed, and limited to 500 characters.
- blank descriptions are stored as `null`.
- `Category` is required.
- `FrequencyType` is required.
- `TargetCount` is required.
- `Difficulty` is required.
- `IsActive` is required and defaults to `true`.
- creation and update timestamps are required.

### Relationships

A habit:

- belongs to one user,
- has many configuration versions,
- has many completions,
- has many persisted reward rows.

### Indexes and constraints

- primary key on `id`,
- index on `(user_id, is_active)`,
- required foreign key to `users`,
- cascade delete from `User`,
- string-backed category,
- string-backed frequency,
- string-backed difficulty.

Difficulty constraint:

```text
difficulty IN ('Easy', 'Medium', 'Hard', 'Elite')
```

Frequency and target constraint:

```text
(frequency_type = 'Daily' AND target_count = 1)
OR
(frequency_type = 'Weekly' AND target_count BETWEEN 1 AND 7)
```

### Rule-column authority

The current `habits` schema still contains:

- `category`,
- `frequency_type`,
- `target_count`,
- `difficulty`.

These columns were introduced before effective-dated configuration history.

Date-sensitive application behavior now selects rules from `HabitConfigurationVersion`.

The `Habit` row remains authoritative for:

- ownership,
- name,
- description,
- active state,
- lifecycle timestamps.

`HabitConfigurationVersion` is authoritative for determining which category, frequency, target, and difficulty apply on a specific date.

### Soft deactivation

Normal product behavior deactivates a habit by setting:

```text
IsActive = false
```

It does not delete:

- the habit,
- its configurations,
- its completions,
- its XP history.

Inactive habits are excluded from normal active queries and active dashboard streak responses.

---

## 9. Entity: `HabitConfigurationVersion`

Stores an effective-dated version of a habit’s progression and scheduling rules.

### Fields

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

### Rules

- `Id` is a backend-generated Guid version 7.
- `HabitId` is required.
- `VersionNumber` is required and begins at 1.
- `Category` is required and stored as a string.
- `FrequencyType` is required and stored as a string.
- `TargetCount` is required.
- `Difficulty` is required and stored as a string.
- `EffectiveFromDate` is required.
- `EffectiveToDateExclusive` is nullable.
- `CreatedAtUtc` is required.

### Effective-period model

A configuration applies when:

```text
EffectiveFromDate <= date
AND
(
    EffectiveToDateExclusive IS NULL
    OR date < EffectiveToDateExclusive
)
```

This is a half-open interval:

```text
[EffectiveFromDate, EffectiveToDateExclusive)
```

An open-ended version has:

```text
EffectiveToDateExclusive = null
```

### Relationships

- one configuration version belongs to one habit,
- one configuration version may be referenced by many completions,
- deleting the habit cascades to its configuration versions,
- deleting a configuration referenced by a completion is restricted.

The restrictive completion relationship prevents historical configuration identity from being removed while completions depend on it.

### Indexes

Unique index on:

```text
(HabitId, VersionNumber)
```

Unique index on:

```text
(HabitId, EffectiveFromDate)
```

Partial unique index on:

```text
HabitId
WHERE EffectiveToDateExclusive IS NULL
```

The partial index permits at most one open-ended configuration per habit.

### Check constraints

Version number:

```text
VersionNumber >= 1
```

Frequency and target:

```text
(FrequencyType = Daily AND TargetCount = 1)
OR
(FrequencyType = Weekly AND TargetCount BETWEEN 1 AND 7)
```

Difficulty:

```text
Difficulty IN (Easy, Medium, Hard, Elite)
```

Date range:

```text
EffectiveToDateExclusive IS NULL
OR EffectiveToDateExclusive > EffectiveFromDate
```

### Initial version

Creating a habit creates version 1.

Its `EffectiveFromDate` is the current user-local date.

### Scheduled rule changes

Changes to these fields are effective-dated:

- category,
- frequency,
- target count,
- difficulty.

A changed rule set begins at the next user-local week boundary.

The current version receives:

```text
EffectiveToDateExclusive = next week start
```

The pending version receives:

```text
EffectiveFromDate = next week start
EffectiveToDateExclusive = null
```

### Pending configuration representation

A pending configuration is not a separate entity and does not require a status flag.

It is a configuration version whose:

```text
EffectiveFromDate > current local date
```

Only one pending next-boundary version is maintained by the application.

Editing the pending rules updates that version.

Changing the requested rules back to the current effective rules:

- removes the unused pending version,
- restores the current version to an open-ended range.

### Historical significance

Configuration history preserves:

- category-to-attribute mapping,
- completion difficulty,
- frequency,
- weekly target,
- frequency transitions,
- streak segment boundaries.

---

## 10. Entity: `HabitCompletion`

Represents one recorded completion event on a user-local date.

### Fields

- `Id`
- `UserId`
- `HabitId`
- `HabitConfigurationVersionId`
- `CompletedDate`
- `CompletedAtUtc`
- `UndoneAtUtc`
- `Notes`

### Rules

- `Id` is a backend-generated Guid version 7.
- `UserId` is required.
- `HabitId` is required.
- `HabitConfigurationVersionId` is required.
- `CompletedDate` is required and stored as `DateOnly`.
- `CompletedAtUtc` is required.
- `UndoneAtUtc` is nullable.
- `Notes` is optional, trimmed, and limited to 500 characters.
- blank notes are stored as `null`.

### Relationships

A completion belongs to:

- one user,
- one habit,
- one habit configuration version.

A completion has many XP transactions.

Delete behavior:

- deleting the user cascades to completions,
- deleting the habit cascades to completions,
- deleting the referenced configuration version is restricted,
- deleting a completion cascades to its XP transactions.

Normal undo does not use deletion.

### PostgreSQL indexes

Partial unique index:

```text
(HabitId, CompletedDate)
WHERE UndoneAtUtc IS NULL
```

Non-unique index:

```text
(UserId, CompletedDate)
```

Index:

```text
HabitConfigurationVersionId
```

### Active completion definition

A completion is active when:

```text
UndoneAtUtc IS NULL
```

An undone completion remains historical but is not treated as currently applied.

### Active completion uniqueness

The business rule is:

```text
one active completion per habit and local date
```

The partial unique index allows this sequence:

1. complete the habit,
2. undo the completion,
3. complete the habit again on the same local date.

The first completion remains in the database with `UndoneAtUtc` populated.

The replacement completion is the new active record.

### Date distinction

`CompletedDate` is the user-local habit date.

Example:

```text
2026-07-22
```

`CompletedAtUtc` is the exact UTC timestamp when the action was recorded.

Example:

```text
2026-07-23T01:15:00Z
```

The two fields serve different purposes.

Use `CompletedDate` for:

- daily completion state,
- daily execution,
- weekly grouping,
- streak calculations,
- local-date summaries.

Use `CompletedAtUtc` for:

- exact ordering,
- auditing,
- diagnostics,
- activity timestamps.

### Configuration linkage

`HabitConfigurationVersionId` identifies the rule set effective on `CompletedDate`.

It preserves:

- category,
- difficulty,
- frequency,
- target count.

A later habit edit cannot change the configuration referenced by the completion.

### Creation workflow

`CompletionService`:

1. loads the owned habit,
2. verifies that it is active,
3. loads the user’s time zone,
4. obtains current UTC time through `TimeProvider`,
5. derives the user-local date,
6. checks for an active completion,
7. selects the configuration effective on that date,
8. normalizes optional notes,
9. creates the completion,
10. calculates rewards from the selected configuration,
11. applies attribute XP,
12. creates positive XP transactions,
13. saves the unit atomically.

The client cannot supply `CompletedDate`.

### Undo workflow

Undo:

1. verifies habit ownership,
2. calculates the current user-local date,
3. finds the active completion for that date,
4. reads its original positive XP transactions,
5. validates current attribute totals,
6. subtracts the awarded attribute XP,
7. appends negative XP transactions,
8. sets `UndoneAtUtc`,
9. saves the unit atomically.

The completion row remains.

---

## 11. Entity: `HabitAttributeReward`

Stores a habit-level reward projection.

### Fields

- `Id`
- `HabitId`
- `AttributeType`
- `XpAmount`

### Rules

- `Id` is a backend-generated Guid version 7.
- `HabitId` is required.
- `AttributeType` is required.
- `XpAmount` is required and greater than zero.

### Relationships

- one reward belongs to one habit,
- deleting the habit cascades to its reward rows.

### Indexes and constraints

Unique index:

```text
(HabitId, AttributeType)
```

XP constraint:

```text
XpAmount > 0
```

Attribute values are restricted to the eight canonical attributes.

### Current role

Two reward rows are initialized when a habit is created:

- primary attribute reward,
- secondary attribute reward.

The table originated with the Phase 5 habit-level reward model.

Phase 6 introduced effective-dated configuration history.

Current date-sensitive behavior uses:

```text
HabitConfigurationVersion
+
XpService reward calculation
```

Specifically:

- habit response rewards are calculated from the effective configuration,
- completion rewards are calculated from the configuration linked to the completion.

`HabitAttributeReward` is not linked to a configuration version and must not be used to reinterpret historical completion rewards.

The exact applied reward remains recorded by the related `XpTransaction` rows.

---

## 12. Entity: `UserAttribute`

Stores the current net XP for one user and one canonical attribute.

### Fields

- `Id`
- `UserId`
- `AttributeType`
- `CurrentXp`
- `UpdatedAtUtc`

### Rules

- `Id` is a backend-generated Guid version 7.
- `UserId` is required.
- `AttributeType` is required.
- `CurrentXp` is required.
- `CurrentXp` cannot be negative.
- `UpdatedAtUtc` is required.

### Relationships

- one attribute row belongs to one user,
- deleting the user cascades to their attribute rows.

### Indexes and constraints

Unique index:

```text
(UserId, AttributeType)
```

XP constraint:

```text
CurrentXp >= 0
```

Attribute type is restricted to the canonical eight values.

### Sparse rows

A new user does not require eight physical zero-XP rows.

`AttributeService` returns all eight attributes.

When a stored row does not exist, the response uses:

```text
CurrentXp = 0
Level = 1
XpIntoCurrentLevel = 0
XpNeededForNextLevel = 100
```

A row is created when XP is first awarded to that attribute.

### Aggregate role

`CurrentXp` is a synchronized current aggregate.

It should equal the signed sum of the user’s XP transactions for the same attribute.

It provides efficient attribute reads while `XpTransaction` preserves the event history.

### Calculated values

The database does not store:

- attribute level,
- XP within the current level,
- XP needed for the next level.

`XpService` calculates those values from `CurrentXp`.

---

## 13. Entity: `XpTransaction`

Represents one signed XP ledger entry associated with a habit completion and attribute.

### Fields

- `Id`
- `UserId`
- `HabitCompletionId`
- `AttributeType`
- `Amount`
- `Reason`
- `CreatedAtUtc`

### Rules

- `Id` is a backend-generated Guid version 7.
- `UserId` is required.
- `HabitCompletionId` is required.
- `AttributeType` is required.
- `Amount` is required.
- `Amount` cannot be zero.
- `Reason` is required and limited to 100 characters.
- `CreatedAtUtc` is required.

### Relationships

- one transaction belongs to one user,
- one transaction belongs to one completion,
- deleting the user cascades to the transaction,
- deleting the completion cascades to the transaction.

Normal undo does not delete the completion or its transactions.

### Indexes

Unique index:

```text
(HabitCompletionId, AttributeType, Reason)
```

Non-unique chronological index:

```text
(UserId, CreatedAtUtc)
```

Including `Reason` in the unique index permits one award and one reversal for the same completion and attribute.

### Amount constraint

```text
Amount <> 0
```

Positive and negative values are valid.

### Current reasons

Award:

```text
Habit completion
```

Undo reversal:

```text
Habit completion undo
```

### Award transaction

A successful completion creates one positive transaction for each affected attribute.

Example:

```text
+14 Mind
+6 Focus
```

### Reversal transaction

Undo keeps the positive transactions and appends matching negative transactions.

Example:

```text
+14 Mind    Habit completion
+6 Focus    Habit completion
-14 Mind    Habit completion undo
-6 Focus    Habit completion undo
```

Net result:

```text
0 XP
```

The original event and reversal remain visible.

### Append-only behavior

Completion and undo history is append-only at the XP ledger level.

Undo does not:

- modify positive amounts,
- change original reasons,
- delete original transactions.

### Uses

XP transactions support:

- exact reward auditing,
- exact reversal,
- overall XP,
- XP earned today,
- attribute consistency checks,
- future activity views,
- grouped reporting,
- diagnostics.

---

## 14. Canonical Enums

### 14.1 `AttributeType`

Values:

- `Discipline`
- `Fitness`
- `Vitality`
- `Focus`
- `Mind`
- `Resilience`
- `Social`
- `Purpose`

Used by:

- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`
- reward DTOs
- attribute DTOs

These values are persisted as strings.

Changing them requires a deliberate data migration and contract update.

### 14.2 `HabitDifficulty`

Values:

- `Easy`
- `Medium`
- `Hard`
- `Elite`

Difficulty determines total completion XP:

| Difficulty |  XP |
| ---------- | --: |
| Easy       |  10 |
| Medium     |  20 |
| Hard       |  30 |
| Elite      |  50 |

### 14.3 `HabitFrequencyType`

Values:

- `Daily`
- `Weekly`

Rules:

```text
Daily  -> TargetCount = 1
Weekly -> TargetCount between 1 and 7
```

One active completion is permitted per local date.

A weekly target therefore represents distinct successful completion dates within the week.

### 14.4 `WeekStartDay`

Values:

- `Monday`
- `Tuesday`
- `Wednesday`
- `Thursday`
- `Friday`
- `Saturday`
- `Sunday`

Used by `UserSettings.WeekStartsOn`.

### 14.5 `HabitCategory`

Values:

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

Categories are controlled values rather than free text.

---

## 15. Category-to-Attribute Mapping

| Habit category              | Primary attribute | Secondary attribute |
| --------------------------- | ----------------- | ------------------- |
| Fitness and Movement        | Fitness           | Discipline          |
| Health and Recovery         | Vitality          | Discipline          |
| Learning and Skills         | Mind              | Focus               |
| Work and Career             | Focus             | Discipline          |
| Daily Life and Organization | Discipline        | Focus               |
| Money and Finance           | Mind              | Discipline          |
| Relationships and Community | Social            | Resilience          |
| Emotional Well-Being        | Resilience        | Vitality            |
| Spirituality and Purpose    | Purpose           | Resilience          |
| Creativity and Recreation   | Mind              | Vitality            |
| Self-Control and Boundaries | Discipline        | Resilience          |
| General Growth              | Discipline        | Mind                |

Reward allocation:

```text
Primary attribute   70%
Secondary attribute 30%
```

The supported difficulty totals produce whole-number allocations.

The mapping is implemented by `XpService`.

It is not configured by the client.

---

## 16. XP and Level Rules

### 16.1 Attribute progression

`UserAttribute.CurrentXp` stores current net XP for one attribute.

XP required to advance from the current attribute level:

```text
100 + 25 × (current level - 1)
```

Examples:

| Current level | XP required |
| ------------: | ----------: |
|             1 |         100 |
|             2 |         125 |
|             3 |         150 |
|             4 |         175 |

Attribute level is calculated from cumulative current XP.

It is not stored.

### 16.2 Overall progression

Overall level uses a separate curve.

XP required to advance from the current overall level:

```text
200 + 50 × (current level - 1)
```

Examples:

| Current level | XP required |
| ------------: | ----------: |
|             1 |         200 |
|             2 |         250 |
|             3 |         300 |
|             4 |         350 |

Overall level is calculated and not stored.

### 16.3 Overall XP

Current overall XP is derived from signed transaction amounts:

```text
SUM(XpTransaction.Amount)
```

Positive completion transactions increase the total.

Negative undo transactions reduce it.

No separate mutable `TotalXp` column exists.

### 16.4 Aggregate invariant

At the user level:

```text
SUM(UserAttribute.CurrentXp)
=
SUM(XpTransaction.Amount)
```

At the user-and-attribute level:

```text
UserAttribute.CurrentXp
=
SUM(XpTransaction.Amount for that AttributeType)
```

Undo safety checks prevent an attribute from being reduced below zero.

---

## 17. Completion and XP Atomicity

Completion and reward application form one persistence unit.

Before saving a completion, the context tracks:

- the new `HabitCompletion`,
- any new `UserAttribute` rows,
- updated `UserAttribute.CurrentXp` values,
- positive `XpTransaction` rows.

One `SaveChangesAsync` call persists the unit.

Undo also forms one persistence unit.

Before saving undo, the context tracks:

- `HabitCompletion.UndoneAtUtc`,
- reduced `UserAttribute.CurrentXp` values,
- negative `XpTransaction` rows.

One `SaveChangesAsync` call persists the reversal.

With PostgreSQL, EF Core commits or rolls back the relational save as a unit.

### Reversal safety

Undo fails rather than intentionally corrupting progression when:

- a required attribute row is missing,
- an attribute contains less XP than the original positive transaction.

---

## 18. Date and Time Model

### UTC timestamps

UTC timestamps are used for exact event time:

- `CreatedAtUtc`
- `UpdatedAtUtc`
- `LastLoginAtUtc`
- `CompletedAtUtc`
- `UndoneAtUtc`
- `XpTransaction.CreatedAtUtc`

### Local dates

`DateOnly` values represent user-calendar dates:

- `HabitCompletion.CompletedDate`
- `HabitConfigurationVersion.EffectiveFromDate`
- `HabitConfigurationVersion.EffectiveToDateExclusive`

### Local-date calculation

The backend calculates the local date from:

```text
TimeProvider UTC timestamp
+
UserSettings.TimeZone
```

The client cannot submit an authoritative completion date.

### Historical dates

Changing a user’s time zone does not rewrite:

- previous completion dates,
- previous configuration effective dates,
- previous streak periods.

Those dates retain their original stored calendar meaning.

### Week boundaries

Weekly periods are calculated from:

- a user-local date,
- `UserSettings.WeekStartsOn`.

The same calendar rule is used for:

- scheduled habit configuration changes,
- weekly streak calculation.

---

## 19. Configuration Timeline Example

Consider a weekly habit created on July 8 with target 3.

Version 1:

```text
VersionNumber:             1
FrequencyType:             Weekly
TargetCount:               3
EffectiveFromDate:         2026-07-08
EffectiveToDateExclusive:  2026-07-13
```

On July 10, the user schedules a target change to 5 for the next Monday.

Version 2:

```text
VersionNumber:             2
FrequencyType:             Weekly
TargetCount:               5
EffectiveFromDate:         2026-07-13
EffectiveToDateExclusive:  null
```

Results:

- dates before July 13 use target 3,
- dates on or after July 13 use target 5,
- earlier completions remain linked to version 1,
- later completions link to version 2,
- earlier weekly streak periods continue using target 3.

---

## 20. Frequency Transition Example

Consider this configuration history:

```text
Version 1: Daily  from July 1 through July 6
Version 2: Weekly from July 7 through July 20
Version 3: Daily  from July 21 onward
```

The database preserves all three periods.

`StreakService` treats them as separate frequency segments:

```text
Daily segment 1
Weekly segment
Daily segment 2
```

The current daily streak beginning July 21 does not inherit completions from the earlier daily segment.

No explicit streak-reset row is required.

The frequency history itself defines the segment boundaries.

---

## 21. Streak Data Model

There is no `Streak` table.

There are no persisted columns for:

- `CurrentStreak`
- `LongestStreak`

Streaks are derived from existing facts.

### Daily streak inputs

- `Habit.IsActive`
- configuration frequency history,
- configuration effective dates,
- active completion dates,
- current user-local date,
- `UndoneAtUtc`.

### Weekly streak inputs

- configuration frequency history,
- historical `TargetCount`,
- configuration effective ranges,
- active completion dates,
- `WeekStartsOn`,
- current user-local date,
- `UndoneAtUtc`.

### Derived rules

The following are calculation rules rather than stored state:

- an incomplete current day does not break a daily streak,
- an incomplete current week does not break a weekly streak,
- the first partial weekly period may receive grace,
- reaching a weekly target early counts immediately,
- a frequency change starts a new streak series,
- category and difficulty changes do not reset a streak,
- target changes retain continuity while using historical targets,
- future dates are ignored,
- undone completions are ignored.

### Benefits of derivation

Derived streaks automatically remain consistent after:

- completion,
- undo,
- target changes,
- frequency changes,
- week-start changes,
- habit deactivation.

No separate counter requires repair.

---

## 22. Dashboard Data Model

There is no `Dashboard` table.

Dashboard values are derived through `DashboardService`.

### Overall progression

Derived from:

```text
XpTransaction.Amount
```

### Today activity

Derived from active completions where:

```text
CompletedDate = current local date
AND UndoneAtUtc IS NULL
```

### XP earned today

Derived from XP transactions associated with active completions for the current local date.

### Daily execution

Derived from:

- active habits,
- configuration effective on the current date,
- effective frequency equal to Daily,
- active completion state for the current date.

### Habit streaks

Derived from:

- active habits,
- configuration histories,
- completion histories,
- current local date,
- configured week start.

Dashboard responses are transient DTOs.

They are not persisted.

---

## 23. Source-of-Truth Hierarchy

### Account identity

```text
User
```

### Calendar settings

```text
UserSettings
```

### Habit identity and lifecycle

```text
Habit
```

### Date-sensitive habit rules

```text
HabitConfigurationVersion
```

### Completion event history

```text
HabitCompletion
```

### Currently active completion

```text
HabitCompletion where UndoneAtUtc IS NULL
```

### Reward formula

```text
XpService
+
effective HabitConfigurationVersion
```

### Exact applied XP history

```text
XpTransaction
```

### Current attribute aggregate

```text
UserAttribute.CurrentXp
```

### Attribute and overall levels

```text
backend calculations
```

### Current and longest streaks

```text
StreakService derived results
```

### Dashboard summaries

```text
DashboardService derived response
```

---

## 24. Delete Behavior

Normal application workflows use:

- soft deactivation for habits,
- timestamped reversal for completion undo.

Hard deletion is not part of normal habit or completion behavior.

Configured relational delete behavior includes:

### User deletion

Cascades to:

- `UserSettings`
- `Habit`
- `HabitCompletion`
- `UserAttribute`
- `XpTransaction`

### Habit deletion

Cascades to:

- `HabitConfigurationVersion`
- `HabitCompletion`
- `HabitAttributeReward`

### Completion deletion

Cascades to:

- `XpTransaction`

### Configuration deletion

A configuration version referenced by a completion cannot be deleted because the completion relationship uses restrictive delete behavior.

This protects historical interpretation.

---

## 25. Implemented Database Constraints

### Users

- unique normalized email,
- unique normalized username,
- required identity fields,
- configured maximum lengths.

### User settings

- unique `user_id`,
- required user relationship,
- valid `week_starts_on`,
- required time zone and display name.

### Habits

- required user relationship,
- index on `(user_id, is_active)`,
- valid difficulty,
- valid frequency and target pairing,
- configured text lengths.

### Habit configuration versions

- unique `(habit_id, version_number)`,
- unique `(habit_id, effective_from_date)`,
- one open-ended version per habit,
- version number of at least 1,
- valid frequency and target pairing,
- valid difficulty,
- valid effective-date range.

### Habit completions

- one active completion per `(habit_id, completed_date)`,
- index on `(user_id, completed_date)`,
- index on `habit_configuration_version_id`,
- required configuration linkage,
- maximum note length.

### Habit rewards

- unique `(habit_id, attribute_type)`,
- positive reward amount,
- canonical attribute values.

### User attributes

- unique `(user_id, attribute_type)`,
- non-negative current XP,
- canonical attribute values.

### XP transactions

- unique `(habit_completion_id, attribute_type, reason)`,
- index on `(user_id, created_at_utc)`,
- non-zero signed amount,
- canonical attribute values,
- maximum reason length.

---

## 26. Migration Rules

Schema changes are applied through EF Core migrations.

Before adding a migration:

1. identify the behavior requiring persistence,
2. inspect the current entity,
3. inspect its EF Core configuration,
4. review relationships,
5. review delete behavior,
6. review indexes,
7. review check constraints,
8. review compatibility with existing rows,
9. update affected services,
10. update DTOs where required,
11. add tests,
12. update documentation.

After a model change:

```cmd
dotnet ef migrations has-pending-model-changes --project server\HabitTracker.Api --startup-project server\HabitTracker.Api
```

must report no unrepresented model changes once the migration is added.

Generated migration files are historical schema artifacts.

They are not reformatted or rewritten during unrelated feature work.

---

## 27. Reserved Post-MVP Entities

### 27.1 `Milestone`

Reserved for a future achievement or milestone definition.

A future design may include fields such as:

- `Id`
- `Name`
- `Description`
- `MilestoneType`
- `RequiredValue`
- `CreatedAtUtc`

No schema should be added until milestone evaluation and unlock behavior are defined.

### 27.2 `UserMilestone`

Reserved for a future user unlock record.

A future design may include:

- `Id`
- `UserId`
- `MilestoneId`
- `UnlockedAtUtc`

Likely uniqueness:

```text
(UserId, MilestoneId)
```

This remains outside the current MVP.

---

## 28. Explicitly Deferred Models

The following persistent models are outside the current scope:

- sleep logs,
- bad-habit logs,
- journal entries,
- reminders,
- notifications,
- quests,
- inventory,
- currencies,
- avatar customization,
- public profiles,
- leaderboards,
- friends,
- shared challenges,
- AI recommendation records,
- payment records,
- administration records,
- calendar-integration records,
- complex analytics snapshots.

A deferred model requires:

1. a defined behavior,
2. an owning backend service,
3. a persistence design,
4. an API contract,
5. migration planning,
6. automated tests,
7. roadmap approval.

---

## 29. Data-Model Change Checklist

Before modifying an implemented entity:

1. Describe the behavior that requires the change.
2. Confirm that the current phase permits it.
3. Inspect the entity and configuration.
4. Review source-of-truth ownership.
5. Review effective-dated history.
6. Review relationships and delete behavior.
7. Review indexes and constraints.
8. Review service behavior.
9. Review DTO and frontend contract impact.
10. Add or update the migration.
11. Add unit and integration tests.
12. Verify PostgreSQL behavior.
13. Update this document.

Do not:

- delete migrations to hide a model mismatch,
- drop the database without understanding the data impact,
- rename stable entities casually,
- add nullable placeholder fields for future screens,
- store a derived counter only because the UI displays it,
- delete historical completion or XP records to implement normal undo.

---

## 30. Summary

The current model separates durable facts from calculated progression:

```text
User and UserSettings define identity and calendar behavior.

Habit defines ownership and lifecycle.

HabitConfigurationVersion preserves dated rules.

HabitCompletion records actions and undo state.

XpTransaction records signed progression events.

UserAttribute stores current net attribute totals.

Dashboard and streak values are derived.
```

The database preserves enough history for the backend to calculate current state without rewriting the past.
