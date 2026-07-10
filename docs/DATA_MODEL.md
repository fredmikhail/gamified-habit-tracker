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
- milestones
- dashboard summaries
- future feature expansion

---

## Core Entity List

Initial core entities:

- User
- UserSettings
- Habit
- HabitCompletion
- HabitAttributeReward
- UserAttribute
- XpTransaction
- Milestone
- UserMilestone

---

## Entity: User

Represents an application user.

Each user owns their own habits, completions, attributes, XP transactions, settings, and milestones.

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
- One User has many UserMilestones.

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

The TimeZone field is important because habit tracking depends on dates.

The app may start with simple date handling, but the model should support proper user time zones later.

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
- Level
- UpdatedAtUtc

### Relationships

- One UserAttribute belongs to one User.

### Notes

Each user should have their own attribute progress.

Two users can both have a Discipline attribute, but their XP and level values are separate.

---

## Entity: XpTransaction

Represents a historical record of XP gained.

Examples:

- +20 Strength XP from completing Go to gym
- +10 Focus XP from completing Read C# textbook

### Planned Fields

- Id
- UserId
- HabitId
- AttributeType
- Amount
- Reason
- CreatedAtUtc

### Relationships

- One XpTransaction belongs to one User.
- One XpTransaction may be linked to one Habit.

### Notes

XpTransaction provides an audit trail for XP.

Instead of only storing the current XP total, the app records how XP was earned.

This helps with debugging, history views, dashboard summaries, and future analytics.

---

## Entity: Milestone

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

## Entity: UserMilestone

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

## Enum: MilestoneType

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

User relationships:

- User has one UserSettings.
- User has many Habits.
- User has many HabitCompletions.
- User has many UserAttributes.
- User has many XpTransactions.
- User has many UserMilestones.

Habit relationships:

- Habit belongs to one User.
- Habit has many HabitCompletions.
- Habit has many HabitAttributeRewards.
- Habit may have many XpTransactions.

Milestone relationships:

- Milestone has many UserMilestones.
- UserMilestone connects one User to one Milestone.

---

## Important Constraints

The database should prevent invalid or duplicate data where possible.

Planned constraints:

- User email should be unique.
- A habit completion should be unique per HabitId and CompletedDate.
- User attributes should be unique per UserId and AttributeType.
- User milestones should be unique per UserId and MilestoneId.
- Required fields should not allow null values unless intentionally optional.

---

## Date and Time Rules

Date and time handling is important because the app tracks daily habits.

General rules:

- Store timestamps in UTC.
- Store habit completion date separately from completion timestamp.
- Use CompletedDate for habit streaks and daily completion logic.
- Use CompletedAtUtc for audit/history purposes.
- Support user time zones through UserSettings.

---

## Source of Truth Rules

HabitCompletion is the source of truth for whether a habit was completed.

XpTransaction is the source of truth for XP history.

UserAttribute stores the current attribute progress for faster display.

PostgreSQL is the source of truth for persistent data.

The frontend should not invent or permanently store business-critical state.

---

## Future Data Model Additions

Possible future entities:

- SleepLog
- JournalEntry
- DailyQuest
- Reminder
- UserTheme
- PublicProfile

These are intentionally outside the MVP and should only be added after the core app is stable.