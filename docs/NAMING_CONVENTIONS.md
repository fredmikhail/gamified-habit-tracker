# Naming Conventions

This document defines naming conventions for the Gamified Habit Tracker app.

Consistent naming makes the codebase easier to understand, search, debug, and maintain.

Names should describe the purpose of the code clearly. Abbreviations and alternate names should be avoided unless they are widely understood.

---

## General Rules

- Use one stable name for each domain concept.
- Prefer clear names over short names.
- Avoid unnecessary abbreviations.
- Avoid generic names such as `Data`, `Manager`, `Helper`, or `Item` when a more specific name is available.
- Use terminology consistently across the frontend, backend, API, and documentation.
- Renaming a core domain concept should be treated as a deliberate project decision.
- File names should normally match the main class, component, or type they contain.

---

## Core Domain Names

The following entity names are stable:

- `User`
- `UserSettings`
- `Habit`
- `HabitCompletion`
- `HabitAttributeReward`
- `UserAttribute`
- `XpTransaction`
- `Milestone`
- `UserMilestone`

`User` through `XpTransaction` are MVP entities.

`Milestone` and `UserMilestone` are reserved post-MVP entity names.

Post-MVP status does not make those names less stable. They should not be replaced with alternate names when the milestone feature is implemented.

Avoid alternate names for the same concepts.

Examples of names that should not replace the established domain names:

- `HabitLog`
- `CompletedHabit`
- `CharacterStat`
- `Stat`
- `RewardLog`
- `ExperienceEvent`
- `Preferences`

---

## Core Service Names

The following service names are stable:

- `AuthService`
- `HabitService`
- `CompletionService`
- `XpService`
- `AttributeService`
- `StreakService`
- `DashboardService`

Service names should describe the responsibility they own.

Avoid vague names such as:

- `DataService`
- `AppManager`
- `HabitManager`
- `UtilityService`
- `GeneralService`

---

## Core Attribute Names

The initial character attributes are:

- `Discipline`
- `Strength`
- `Focus`
- `Recovery`
- `Career`
- `Mind`
- `Social`
- `Spirituality`

These names should remain consistent in:

- C# enums
- API responses
- TypeScript types
- UI labels
- tests
- documentation

---

## C# Naming Conventions

### Classes, Records, Enums, and Interfaces

Use PascalCase.

Examples:

- `Habit`
- `HabitService`
- `CreateHabitRequest`
- `HabitDifficulty`
- `IHabitService`

Interfaces should begin with `I` when interfaces are introduced.

Example:

- `IHabitService`

Do not create an interface automatically for every class. Add one when it provides a clear testing, abstraction, or dependency-injection benefit.

---

### Methods

Use PascalCase.

Asynchronous methods should end with `Async`.

Examples:

- `CreateHabitAsync`
- `GetUserHabitsAsync`
- `CompleteHabitAsync`
- `CalculateCurrentStreak`

A method name should describe an action.

---

### Properties

Use PascalCase.

Examples:

- `Id`
- `UserId`
- `Name`
- `CompletedDate`
- `CreatedAtUtc`
- `IsActive`

Boolean properties should normally begin with words such as:

- `Is`
- `Has`
- `Can`
- `Should`

Examples:

- `IsActive`
- `HasCompletedToday`
- `CanBeCompleted`

---

### Local Variables and Parameters

Use camelCase.

Examples:

- `userId`
- `habitId`
- `completedDate`
- `xpAmount`

Names should describe what the value represents.

Avoid unclear names such as:

- `x`
- `data`
- `temp`
- `obj`
- `thing`

Short names may be acceptable in very small and obvious scopes, but clarity is preferred.

---

### Private Fields

Use an underscore followed by camelCase.

Examples:

- `_dbContext`
- `_habitService`
- `_completionService`

---

### Constants

Use PascalCase.

Examples:

- `MaximumHabitNameLength`
- `DefaultXpAmount`

Constants should be used for values that are fixed and meaningful to the application.

---

## Entity Property Conventions

Primary key properties should use:

- `Id`

Foreign key properties should use the related entity name followed by `Id`.

Examples:

- `UserId`
- `HabitId`
- `HabitCompletionId`
- `MilestoneId`

A foreign key name should identify the entity it directly references.

For example, `XpTransaction` uses `HabitCompletionId` because it belongs to the exact completion that created the XP award. It should not use `HabitId` as a substitute for that direct relationship.

UTC timestamp properties should end with `Utc`.

Examples:

- `CreatedAtUtc`
- `UpdatedAtUtc`
- `CompletedAtUtc`
- `UnlockedAtUtc`

Date-only business properties should use names that describe the date being represented.

Example:

- `CompletedDate`

Collections should use plural names.

Examples:

- `Habits`
- `HabitCompletions`
- `UserAttributes`
- `XpTransactions`

---

## DTO Naming Conventions

DTOs define the data sent to and returned from the API.

Request DTOs should describe the requested action and end with `Request`.

Examples:

- `RegisterRequest`
- `LoginRequest`
- `CreateHabitRequest`
- `UpdateHabitRequest`
- `CompleteHabitRequest`

Response DTOs should describe the returned resource and end with `Response`.

Examples:

- `AuthResponse`
- `CurrentUserResponse`
- `HabitResponse`
- `CompleteHabitResponse`
- `DashboardResponse`
- `UserAttributeResponse`
- `XpTransactionResponse`

Do not expose database entities directly through API endpoints.

Response DTOs may contain calculated properties that are not stored as database entity fields.

Examples:

- `Level`
- `TotalXp`
- `XpRequiredForNextLevel`
- `DidLevelUp`

Calculated progression values should use clear domain names and should be produced by the backend.

The presence of a property in a response DTO does not require a matching property on an entity.

Do not add both `Dto` and `Request` or `Response` unnecessarily.

Preferred:

- `CreateHabitRequest`

Avoid:

- `CreateHabitRequestDto`

---

### DTO Evolution

A DTO may gain properties when a later phase expands the same API concept.

Examples:

- `HabitResponse` adds `IsCompletedToday` during Phase 4.
- Habit request and response DTOs add `AttributeRewards` during Phase 5.
- `CompleteHabitResponse` adds reward information during Phase 5.

Do not rename a DTO only because later phases add fields to the same conceptual request or response.

Contract changes should remain deliberate and should be updated in `API_CONTRACT.md`.

---

## Controller Naming Conventions

Controller class names should end with `Controller`.

Examples:

- `AuthController`
- `HabitsController`
- `CompletionsController`
- `DashboardController`
- `AttributesController`

Controllers representing collections should normally use plural resource names.

Controllers should remain thin and delegate application logic to services.

---

## API Route Naming Conventions

API routes should:

- use lowercase letters
- use plural resource names where appropriate
- use hyphens for multi-word route segments
- avoid verbs when standard HTTP methods already describe the action
- remain stable once used by the frontend

Examples:

- `/api/habits`
- `/api/habits/{habitId}`
- `/api/habits/{habitId}/completions`
- `/api/dashboard/today`
- `/api/auth/register`
- `/api/auth/login`

### API Enum Value Naming

C# enum members use PascalCase.

Examples:

- `Daily`
- `Weekly`
- `Medium`
- `Strength`

Their public JSON values use camelCase strings.

Examples:

- `daily`
- `weekly`
- `medium`
- `strength`

Numeric enum values are not part of the public API contract.

HTTP methods should communicate the operation:

- `GET` retrieves data
- `POST` creates data or performs an action
- `PUT` replaces or fully updates data
- `PATCH` partially updates data
- `DELETE` removes or reverses data

Exact endpoint contracts are documented in `API_CONTRACT.md`.

---

## React and TypeScript Naming Conventions

### React Components

Use PascalCase.

Component file names should match the component name.

Examples:

- `HabitCard.tsx`
- `AttributeCard.tsx`
- `DashboardPage.tsx`
- `ProgressBar.tsx`

---

### TypeScript Variables and Functions

Use camelCase.

Examples:

- `habitId`
- `completedHabits`
- `fetchHabits`
- `completeHabit`

---

### TypeScript Types

Use PascalCase.

Where a frontend type represents an API response, use the same conceptual name as the backend response DTO.

Examples:

- `HabitResponse`
- `DashboardResponse`
- `UserAttributeResponse`

Do not rename the same API concept differently in the frontend without a clear reason.

---

### React Hooks

Custom hook names must begin with `use`.

Examples:

- `useAuth`
- `useHabits`
- `useDashboard`

---

### Boolean Values

Boolean variables and properties should use clear prefixes.

Examples:

- `isLoading`
- `isAuthenticated`
- `hasCompletedToday`
- `canSubmit`

---

## Frontend File and Folder Naming

Planned top-level source folders use lowercase names:

- `api`
- `components`
- `pages`
- `hooks`
- `types`
- `utils`

React component files use PascalCase:

- `HabitCard.tsx`
- `DashboardPage.tsx`

Non-component TypeScript files normally use camelCase:

- `habitsApi.ts`
- `authApi.ts`
- `dateUtils.ts`

---

## Backend File Naming

A C# file should normally match the main type it contains.

Examples:

- `Habit.cs`
- `HabitService.cs`
- `HabitsController.cs`
- `CreateHabitRequest.cs`
- `CurrentUserResponse.cs`
- `CompleteHabitResponse.cs`
- `HabitDifficulty.cs`
- `AppDbContext.cs`

Avoid placing several unrelated public classes in the same file.

---

## Database Naming

C# entity and property names follow C# naming conventions.

Physical PostgreSQL identifiers use `snake_case`.

Examples:

- `UserSettings` maps to `user_settings`
- `NormalizedEmail` maps to `normalized_email`
- `CreatedAtUtc` maps to `created_at_utc`
- primary-key, foreign-key, and index names also use `snake_case`

The EF Core database configuration applies the naming strategy consistently across the model.

Database names should not be changed casually after migrations and deployed data exist.

---

## Test Naming Conventions

The backend test project is named:

- `HabitTracker.Tests`

The test project remains separate from the production API project:

- `HabitTracker.Api`
- `HabitTracker.Tests`

Backend test classes should identify the class or behavior under test.

Examples:

- `HabitServiceTests`
- `CompletionServiceTests`
- `StreakServiceTests`

Test method names should describe:

1. the method or behavior being tested
2. the condition
3. the expected result

Example:

- `CompleteHabitAsync_WhenHabitAlreadyCompleted_ThrowsConflictException`

Tests should prioritize clarity over brevity.

---

## Naming Change Rules

Before changing a core name:

1. Identify why the current name is inadequate.
2. Identify every affected layer.
3. Check entities, DTOs, services, controllers, routes, frontend types, tests, and documentation.
4. Explain the migration or compatibility impact.
5. Update documentation before or alongside the code change.
6. Make the rename in one focused commit.

Core names should not be changed as an unrelated part of debugging.