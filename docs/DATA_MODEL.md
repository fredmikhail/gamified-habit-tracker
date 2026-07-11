# Data Model

This document describes the planned data model for the Gamified Habit Tracker app.

The data model defines the main entities, their responsibilities, and how they relate to each other.

This document is a planning reference. The final database schema will be created through C# entity classes, Entity Framework Core, and PostgreSQL migrations.

---

## Data Model Goals

The data model should support:

- multiple users
- private user-specific habit data
- habit creation and management
- daily habit completions
- XP rewards
- character attributes
- streak calculations
- future milestones and achievements
- dashboard summaries
- future feature expansion

---

## Planned Entity List

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

The post-MVP entity names are reserved, but those features are not implemented during the initial MVP phases.

---

## Entity: User

Represents an application user.

Each user owns their own habits, completions, attributes, XP transactions, and settings.

Post-MVP, a user may also own milestone unlock records.

### Planned Fields

- Id
- Email
- Username
- PasswordHash
- CreatedAtUtc
- LastLoginAtUtc

### Relationships

- One User has one UserSettings.
- One User has many Habits.
- One User has many HabitCompletions.
- One User has many UserAttributes.
- One User has many XpTransactions.

Post-MVP relationship:

- One User may have many UserMilestones.


### Notes

User-specific data must always be protected.

A user should not be able to access another user's habits, completions, XP, or dashboard data.

---

## Entity: UserSettings

Stores user preferences and customization options.

### Planned Fields

- Id
- UserId
- DisplayName
- TimeZone
- CreatedAtUtc
- UpdatedAtUtc

### Relationships

- One UserSettings belongs to one User.

### Notes

The TimeZone field is required because habit tracking depends on the user's local date.

Default UserSettings are created during registration.

The backend uses the stored time zone to convert the current UTC timestamp into the user's local date for completion and dashboard logic.

DisplayName may initially default to Username.

Changing a user's time zone should not rewrite historical CompletedDate values.

---

## Entity: Habit

Represents a habit created by a user.

Examples:

- Go to gym
- Read C# textbook
- Meditate
- No nicotine
- Sleep before 11 PM

### Planned Fields

- Id
- UserId
- Name
- Description
- Category
- FrequencyType
- TargetCount
- Difficulty
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

### Relationships

- One Habit belongs to one User.
- One Habit has many HabitCompletions.
- One Habit has many HabitAttributeRewards.
- One Habit may have many XpTransactions.

### Notes

Habits should be deactivated instead of hard deleted.

This preserves completion history and XP history.

IsActive allows old habits to stop appearing in the active habit list without destroying past data.

For the MVP:

- A Daily habit uses TargetCount of 1 and is expected once per local date.
- A Weekly habit uses TargetCount to represent the number of distinct days it should be completed during the week.
- Weekly TargetCount must be between 1 and 7.
- A habit may be completed at most once per local date.

Supporting multiple completions of the same habit on one date would require a later data-model and API change.

---

## Entity: HabitCompletion

Represents a record that a habit was completed on a specific date.

### Planned Fields

- Id
- UserId
- HabitId
- CompletedDate
- CompletedAtUtc
- Notes

### Relationships

- One HabitCompletion belongs to one User.
- One HabitCompletion belongs to one Habit.
- One HabitCompletion may create many XpTransactions during Phase 5.

### Important Date Distinction

CompletedDate represents the habit date.

Example:

- 2026-07-10

CompletedAtUtc represents the exact timestamp when the completion was recorded.

Example:

- 2026-07-10 23:42:10 UTC

### Notes

The app should prevent duplicate completions for the same HabitId and CompletedDate.

HabitCompletion is the source of truth for completion history.

Streaks should be calculated from completion records instead of being manually trusted as the main source of truth.

The backend determines CompletedDate from the current UTC timestamp and the user's stored time zone.

The MVP completion request does not accept a client-selected date.

HabitCompletion.UserId must match the owner of the related Habit.

---

## Entity: HabitAttributeReward

Defines how much XP a habit gives to a specific character attribute.

Example:

Go to gym may reward:

- Strength +20 XP
- Discipline +10 XP

### Planned Fields

- Id
- HabitId
- AttributeType
- XpAmount

### Relationships

- One HabitAttributeReward belongs to one Habit.
- One Habit can have many HabitAttributeRewards.

### Notes

This entity allows one habit to reward multiple attributes.

This keeps the XP system flexible without hardcoding all rewards directly into the habit entity.

---

## Entity: UserAttribute

Represents a user's progress in one character attribute.

Initial attributes:

- Discipline
- Strength
- Focus
- Recovery
- Career
- Mind
- Social
- Spirituality

### Planned Fields

- Id
- UserId
- AttributeType
- CurrentXp
- UpdatedAtUtc

### Relationships

- One UserAttribute belongs to one User.

### Notes

Each user should have their own attribute progress.

Two users can both have a Discipline attribute, but their XP values are separate.

CurrentXp represents the current net XP balance for that attribute.

Attribute level and XP required for the next level are calculated from CurrentXp by backend progression rules.

Level is not stored as a separate database field during the MVP.

---

## Entity: XpTransaction

Represents a historical record of XP gained.

Examples:

- +20 Strength XP from completing Go to gym
- +10 Focus XP from completing Read C# textbook

### Planned Fields

- Id
- UserId
- HabitCompletionId
- AttributeType
- Amount
- Reason
- CreatedAtUtc

### Relationships

- One XpTransaction belongs to one User.
- One XpTransaction belongs to one HabitCompletion during the MVP.

### Notes

XpTransaction records the XP awards currently applied to a user's attributes.

Instead of only storing the current XP total, the app records which completion created each award.

This supports debugging, dashboard summaries, and consistent undo behavior.

HabitCompletionId identifies the exact completion that created the XP transaction.

The related Habit can be found through HabitCompletion, so a separate HabitId is not stored on XpTransaction during the MVP.

XpTransaction.UserId must match the owner of the related HabitCompletion.

Once XP is introduced in Phase 5, completion creation, related XpTransaction creation, and UserAttribute updates must succeed or fail together in one database transaction.

Once XP is introduced in Phase 5, undoing a completion removes its related XpTransactions, subtracts their amounts from the corresponding UserAttributes, and removes the HabitCompletion.

All undo changes must succeed or fail together in one database transaction.

Negative reversal transactions and immutable undo history are deferred until after the MVP.

---

## Post-MVP Entity: Milestone

This entity is reserved for the post-MVP milestone and achievement feature.

Represents a milestone that can be unlocked.

Examples:

- First Habit Completed
- 3-Day Streak
- 7-Day Streak
- 100 Total XP
- First Perfect Day

### Planned Fields

- Id
- Name
- Description
- MilestoneType
- RequiredValue
- Icon
- CreatedAtUtc

### Relationships

- One Milestone can be linked to many UserMilestones.

### Notes

Milestones define the available achievements in the system.

A milestone exists independently from whether a specific user has unlocked it.

---

## Post-MVP Entity: UserMilestone

This entity is reserved for the post-MVP milestone and achievement feature.

Represents a milestone unlocked by a specific user.

### Planned Fields

- Id
- UserId
- MilestoneId
- UnlockedAtUtc

### Relationships

- One UserMilestone belongs to one User.
- One UserMilestone belongs to one Milestone.

### Notes

This entity connects users to the milestones they have unlocked.

It allows the app to show each user's personal achievement history.

---

## Initial Enums

Enums represent fixed sets of values used by the domain model.

---

## Enum: AttributeType

Initial values:

- Discipline
- Strength
- Focus
- Recovery
- Career
- Mind
- Social
- Spirituality

Used by:

- UserAttribute
- HabitAttributeReward
- XpTransaction

---

## Enum: HabitDifficulty

Initial values:

- Easy
- Medium
- Hard

Possible future value:

- Elite

Used by:

- Habit

---

## Enum: HabitFrequencyType

Initial MVP values:

- Daily
- Weekly

Possible future values:

- CustomDays
- TimesPerWeek
- Monthly

Used by:

- Habit

---

## Post-MVP Enum: MilestoneType

Possible values:

- TotalXp
- HabitCompletionCount
- StreakLength
- PerfectDay
- AttributeLevel

Used by:

- Milestone

---

## Relationship Summary

### MVP User Relationships

- User has one UserSettings.
- User has many Habits.
- User has many HabitCompletions.
- User has many UserAttributes.
- User has many XpTransactions.

### MVP Habit and Completion Relationships

- Habit belongs to one User.
- Habit has many HabitCompletions.
- Habit has many HabitAttributeRewards.
- HabitCompletion belongs to one Habit.
- HabitCompletion may create many XpTransactions.
- XpTransaction belongs to one HabitCompletion.

### Post-MVP Milestone Relationships

- User may have many UserMilestones.
- Milestone may have many UserMilestones.
- UserMilestone connects one User to one Milestone.

---

## Important Constraints

The database should prevent invalid or duplicate data where possible.

Planned constraints:

- User email should be unique using a consistent normalized comparison.
- User username should be unique using a consistent normalized comparison.
- UserSettings should be unique per UserId.
- A HabitCompletion should be unique per HabitId and CompletedDate.
- HabitAttributeReward should be unique per HabitId and AttributeType.
- UserAttribute should be unique per UserId and AttributeType.
- Daily habits should use TargetCount of 1.
- Weekly TargetCount should be between 1 and 7.
- HabitAttributeReward.XpAmount should be greater than zero.
- UserMilestone should be unique per UserId and MilestoneId when milestones are implemented.
- Required fields should not allow null values unless intentionally optional.

---

## Date and Time Rules

Date and time handling is important because the app tracks daily habits.

General rules:

- Store timestamps in UTC.
- Require a valid time-zone identifier in UserSettings.
- Create default UserSettings during registration.
- Convert the current UTC timestamp into the user's local date before creating a HabitCompletion.
- Store the habit completion date separately from its exact timestamp.
- Use CompletedDate for daily completion, weekly progress, and streak logic.
- Use CompletedAtUtc for audit and history purposes.
- Do not accept a client-selected completion date during the MVP.
- Do not rewrite historical CompletedDate values when a user's time zone changes.
- Use Monday as the beginning of the week during the MVP.

---

## Progression Calculation Rules

CurrentXp stores the current net XP balance for one user attribute.

Attribute level is calculated from UserAttribute.CurrentXp by AttributeService.

Total user XP is derived from the sum of the user's current XpTransaction amounts.

Overall user level and XP required for the next level are calculated from total user XP by backend progression rules.

Level thresholds are not stored separately on User or UserAttribute during the MVP.

The frontend receives calculated progression values through response DTOs and does not calculate authoritative levels itself.

---

## Source of Truth Rules

HabitCompletion is the source of truth for whether a habit was completed on a date.

XpTransaction is the source of truth for XP awards currently applied to the user.

UserAttribute.CurrentXp is a synchronized aggregate used for efficient attribute display.

UserAttribute and XpTransaction changes must be saved consistently within the same database transaction.

Attribute levels and overall user level are calculated values rather than independent sources of truth.

PostgreSQL is the source of truth for persistent data.

The frontend should not invent or permanently store business-critical state.

---

## Other Future Data Model Additions

Possible future entities:

- SleepLog
- JournalEntry
- DailyQuest
- Reminder
- UserTheme
- PublicProfile

These are intentionally outside the MVP and should only be added after the core app is stable.