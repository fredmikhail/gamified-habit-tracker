# API Contract

This document defines the planned HTTP API contract between the React frontend and the ASP.NET Core backend.

It establishes consistent routes, request shapes, response shapes, status codes, and error behavior.

The contract may evolve as features are implemented, but changes should be deliberate and documented.

---

## Base Path

All application API routes begin with:

```text
/api
```

Examples:

```text
/api/auth/login
/api/habits
/api/dashboard/today
```

---

## General Conventions

### Data Format

Requests and responses use JSON unless otherwise documented.

JSON property names use camelCase.

Enum values are serialized as camelCase strings.

Examples:

- `daily`
- `weekly`
- `medium`
- `strength`

Numeric enum values are not part of the public API contract.

Example:

```json
{
  "frequencyType": "weekly",
  "targetCount": 3
}
```

---

### Identifiers

Entity identifiers are treated as opaque values by API clients.

The frontend should not make assumptions about how identifiers are generated.

The exact identifier type will be confirmed when the backend entities are implemented.

---

### Dates and Timestamps

Date-only values use:

```text
YYYY-MM-DD
```

Example:

```text
2026-07-10
```

UTC timestamps use ISO 8601 format.

Example:

```text
2026-07-10T21:42:10Z
```

Properties containing UTC timestamps should end with `Utc`.

Examples:

- `createdAtUtc`
- `updatedAtUtc`
- `completedAtUtc`
- `unlockedAtUtc`

---

### Authentication

Protected endpoints require an authenticated user.

The browser application uses secure cookie-based authentication.

After successful registration or login, the backend creates an encrypted authentication cookie. The browser sends this cookie automatically with later authenticated requests.

The authentication cookie is:

- `HttpOnly` so frontend JavaScript cannot read it
- `Secure` in production so it is sent only over HTTPS
- configured with `SameSite=Lax`
- temporary by default, with a maximum authentication lifetime of 12 hours
- persistent for a fixed maximum of 30 days only when the user explicitly selects `rememberMe`
- configured without sliding expiration during the MVP

Frontend API requests include credentials so the browser can send and receive authentication cookies.

The web frontend and backend should be presented through the same public origin where practical.

During local development, the Vite development server proxies `/api` requests to the ASP.NET Core backend so browser authentication remains same-origin.

State-changing browser requests use antiforgery protection.

This applies to browser requests using:

- `POST`
- `PUT`
- `PATCH`
- `DELETE`

The frontend obtains an antiforgery request token from the backend and sends it through the documented antiforgery header.

The backend determines the authenticated user's identity from the authentication context.

Request bodies must not accept a user-provided `userId` for user-owned operations.

This prevents a user from attempting to access or modify another user's data by submitting a different identifier.

---

### User Data Ownership

Every protected operation must verify that the requested data belongs to the authenticated user.

For example:

- a user may only view their own habits
- a user may only edit their own habits
- a user may only complete their own habits
- a user may only view their own dashboard
- a user may only view their own XP and attributes

The frontend is not responsible for enforcing ownership security.

The backend enforces it.

---

## Standard HTTP Status Codes

The API should use standard HTTP status codes consistently.

- `200 OK` — Request succeeded.
- `201 Created` — A resource was created.
- `204 No Content` — Request succeeded and no response body is needed.
- `400 Bad Request` — Request data is invalid.
- `401 Unauthorized` — Authentication is missing or invalid.
- `403 Forbidden` — The user is authenticated but cannot perform the action.
- `404 Not Found` — The requested resource does not exist for the current user.
- `409 Conflict` — The request conflicts with existing state.
- `500 Internal Server Error` — An unexpected server error occurred.

A duplicate habit completion should normally return:

```text
409 Conflict
```

The request may be valid in shape, but it conflicts with an existing completion for the same habit and date.

---

## Error Responses

The API should use a consistent error format.

ASP.NET Core `ProblemDetails` is the planned error response format.

Example:

```json
{
  "type": "https://example.com/errors/habit-already-completed",
  "title": "Habit already completed",
  "status": 409,
  "detail": "This habit has already been completed for today.",
  "instance": "/api/habits/example-id/completions"
}
```

Validation errors may include field-specific details.

Example:

```json
{
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "name": [
      "Habit name is required."
    ],
    "targetCount": [
      "Target count must be greater than zero."
    ]
  }
}
```

Internal exception details and stack traces must not be exposed to users in production responses.

---

# Authentication Endpoints

Authentication endpoints are implemented during Phase 2.

Successful registration or login creates an encrypted authentication cookie.

The frontend does not receive, store, or manually attach the authentication credential.

The `user` property of `AuthResponse` uses the `CurrentUserResponse` shape.

Registration, login, and logout use antiforgery protection because they change authentication state.

After registration or login changes the authenticated identity, the frontend obtains a new antiforgery request token for later state-changing requests.

---

## Get Antiforgery Token

```text
GET /api/auth/csrf-token
```

Returns an antiforgery request token for browser requests.

### Authentication Required

No.

The endpoint may be called before registration or login and again after the authenticated user changes.

The backend also creates the corresponding antiforgery cookie when required.

### Response

`AntiforgeryTokenResponse`

```json
{
  "requestToken": "antiforgery-token-value"
}
```

The frontend sends `requestToken` through:

```text
X-CSRF-TOKEN
```

for state-changing browser requests.

### Possible Status Codes

- `200 OK`

---

## Register User

```text
POST /api/auth/register
```

Creates a new user account.

### Authentication Required

No.

### Request

`RegisterRequest`

```json
{
  "email": "fred@example.com",
  "username": "fred",
  "password": "example-password",
  "timeZone": "America/Toronto"
}
```

`timeZone` is required and must contain a valid application-supported time-zone identifier.

Registration creates both the User and their default UserSettings.

DisplayName initially defaults to Username.

### Response

Successful registration creates the authentication cookie and returns `AuthResponse`.

```json
{
  "user": {
    "id": "user-id",
    "email": "fred@example.com",
    "username": "fred",
    "displayName": "fred",
    "timeZone": "America/Toronto"
  }
}
```

The authentication credential is not included in the JSON response body.

The registration session is temporary by default.

### Possible Status Codes

- `201 Created`
- `400 Bad Request`
- `409 Conflict`

`409 Conflict` may be returned when the email or username is already registered.

### Validation Rules

The backend validates:

- `email` is required, has a valid email shape, and is at most 254 characters
- `username` is required and contains between 3 and 30 characters
- `username` contains only letters, numbers, and underscores
- `password` is required and contains between 15 and 128 characters
- `timeZone` is required, is at most 100 characters, and contains a valid IANA time-zone identifier

Email and username comparisons are case-insensitive.

Leading and trailing whitespace is removed from email and username before normalization.

Password whitespace is preserved.

Registration returns `409 Conflict` when the normalized email or normalized username is already registered.

The conflict response should not reveal more account information than necessary.

---

## Login User

```text
POST /api/auth/login
```

Authenticates an existing user.

### Authentication Required

No.

### Request

`LoginRequest`

```json
{
  "email": "fred@example.com",
  "password": "example-password",
  "rememberMe": true
}
```

`rememberMe` is optional and defaults to `false`. It controls whether the authentication cookie persists after the browser session ends.

When `rememberMe` is `false`, the authentication cookie is temporary.

When `rememberMe` is `true`, the authentication cookie may persist for up to 30 days.

### Response

Successful login creates the authentication cookie and returns `AuthResponse`.

```json
{
  "user": {
    "id": "user-id",
    "email": "fred@example.com",
    "username": "fred",
    "displayName": "fred",
    "timeZone": "America/Toronto"
  }
}
```

The authentication credential is not included in the JSON response body.

### Possible Status Codes

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`

### Validation Rules

The backend validates:

- `email` is required and is at most 254 characters
- `password` is required and is at most 128 characters
- `rememberMe` is optional and defaults to `false`

The backend normalizes the submitted email before looking up the user.

Incorrect credentials return `401 Unauthorized` with a generic error message.

The response must not reveal whether the email address or password was incorrect.

---

## Logout User

```text
POST /api/auth/logout
```

Signs out the authenticated user by removing the authentication cookie.

### Authentication Required

No.

Logout is idempotent. The endpoint returns success even when no active authenticated session exists.

The request must include a valid antiforgery token.

### Request

No request body.

### Response

No response body.

After logout succeeds, the frontend clears its current user state and obtains a new antiforgery request token for unauthenticated requests.

### Possible Status Codes

- `204 No Content`

---

## Get Current User

```text
GET /api/auth/me
```

Returns basic information about the authenticated user.

### Authentication Required

Yes.

### Response

`CurrentUserResponse`

```json
{
  "id": "user-id",
  "email": "fred@example.com",
  "username": "fred",
  "displayName": "fred",
  "timeZone": "America/Toronto"
}
```

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`

---

# Habit Endpoints

Habit endpoints are implemented during Phase 3.

---

## Habit DTO Evolution by Phase

Habit DTOs expand as later phases introduce new behavior.

### Phase 3

`HabitResponse` contains the basic habit fields:

- id
- name
- description
- category
- frequencyType
- targetCount
- difficulty
- isActive
- createdAtUtc
- updatedAtUtc

### Phase 4

`HabitResponse` adds:

- isCompletedToday

### Phase 5

Habit request and response DTOs add:

- attributeRewards

Examples in this document that contain all of these properties represent the final MVP contract shape.

## Get User Habits

```text
GET /api/habits
```

Returns habits belonging to the authenticated user.

### Authentication Required

Yes.

### Optional Query Parameter

```text
includeInactive=false
```

Example:

```text
GET /api/habits?includeInactive=true
```

### Response

An array of `HabitResponse`.

```json
[
  {
    "id": "habit-id",
    "name": "Go to gym",
    "description": "Complete a planned gym session.",
    "category": "Fitness",
    "frequencyType": "weekly",
    "targetCount": 3,
    "difficulty": "medium",
    "isActive": true,
    "isCompletedToday": false,
    "attributeRewards": [
      {
        "attributeType": "strength",
        "xpAmount": 20
      },
      {
        "attributeType": "discipline",
        "xpAmount": 10
      }
    ],
    "createdAtUtc": "2026-07-10T15:00:00Z",
    "updatedAtUtc": "2026-07-10T15:00:00Z"
  }
]
```

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`

---

## Get Habit by ID

```text
GET /api/habits/{habitId}
```

Returns one habit belonging to the authenticated user.

### Authentication Required

Yes.

### Response

`HabitResponse`

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

A habit owned by another user should not be returned.

Returning `404 Not Found` avoids revealing whether another user's resource exists.

---

## Create Habit

```text
POST /api/habits
```

Creates a habit for the authenticated user.

### Authentication Required

Yes.

### Request

`CreateHabitRequest`

```json
{
  "name": "Go to gym",
  "description": "Complete a planned gym session.",
  "category": "Fitness",
  "frequencyType": "weekly",
  "targetCount": 3,
  "difficulty": "medium"
}
```

The backend assigns the authenticated user's identity.

The request does not include `userId`.

During Phase 5, `CreateHabitRequest` adds:

```json
{
  "attributeRewards": [
    {
      "attributeType": "strength",
      "xpAmount": 20
    },
    {
      "attributeType": "discipline",
      "xpAmount": 10
    }
  ]
}
```

### Response

`HabitResponse`

### Possible Status Codes

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`

### Validation Notes

During Phase 3, the backend should validate:

- habit name
- supported frequency type
- Daily habits use TargetCount of 1
- Weekly TargetCount is between 1 and 7
- supported difficulty

During Phase 5, the backend should additionally validate:

- supported attribute types
- XP amounts greater than zero
- duplicate attribute rewards within the same habit

The exact maximum XP limits will be decided when XP rules are implemented.

---

## Update Habit

```text
PUT /api/habits/{habitId}
```

Updates an existing habit belonging to the authenticated user.

### Authentication Required

Yes.

### Request

`UpdateHabitRequest`

```json
{
  "name": "Go to gym",
  "description": "Complete a full planned gym session.",
  "category": "Fitness",
  "frequencyType": "weekly",
  "targetCount": 4,
  "difficulty": "hard"
}
```

The request does not include `userId`.

During Phase 5, `UpdateHabitRequest` adds:

```json
{
  "attributeRewards": [
    {
      "attributeType": "strength",
      "xpAmount": 20
    },
    {
      "attributeType": "discipline",
      "xpAmount": 10
    }
  ]
}
```

### Response

`HabitResponse`

### Possible Status Codes

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

---

## Deactivate Habit

```text
PATCH /api/habits/{habitId}/deactivate
```

Deactivates a habit without deleting its history.

### Authentication Required

Yes.

### Request

No request body.

### Response

Updated `HabitResponse`.

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

The MVP does not permanently delete habits.

---

# Habit Completion Endpoints

Habit completion endpoints are implemented during Phase 4.

---

## Complete Habit

```text
POST /api/habits/{habitId}/completions
```

Completes a habit for the authenticated user's current date.

### Authentication Required

Yes.

### Request

`CompleteHabitRequest`

```json
{
  "notes": "Completed after work."
}
```

The MVP does not accept a client-selected completion date.

The backend determines CompletedDate by converting the current UTC timestamp into the authenticated user's local date using their stored time zone.

### Response

`CompleteHabitResponse`

```json
{
  "completion": {
    "id": "completion-id",
    "habitId": "habit-id",
    "completedDate": "2026-07-10",
    "completedAtUtc": "2026-07-10T21:42:10Z",
    "notes": "Completed after work."
  },
  "rewards": [
    {
      "attributeType": "strength",
      "xpAwarded": 20,
      "currentXp": 240,
      "level": 3,
      "didLevelUp": false
    },
    {
      "attributeType": "discipline",
      "xpAwarded": 10,
      "currentXp": 315,
      "level": 4,
      "didLevelUp": true
    }
  ]
}
```

### Possible Status Codes

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`
- `409 Conflict`

`409 Conflict` is returned when the habit is inactive or already completed for the same date.

The example above represents the Phase 5 response shape.

During Phase 4, `CompleteHabitResponse` contains only the `completion` property.

During Phase 5, the `rewards` property is added.

The backend calculates `currentXp`, `level`, and `didLevelUp`. The frontend does not calculate authoritative progression values.

---

## Undo Today's Habit Completion

```text
DELETE /api/habits/{habitId}/completions/today
```

Removes or reverses today's completion for the authenticated user.

### Authentication Required

Yes.

### Request

No request body.

### Response

No response body.

### Possible Status Codes

- `204 No Content`
- `401 Unauthorized`
- `404 Not Found`

The backend determines today's date from the authenticated user's stored time zone.

During Phase 4, undo removes today's HabitCompletion.

Once XP is introduced in Phase 5, undo performs the following changes:

1. Find the XpTransactions linked to the HabitCompletion.
2. Subtract their amounts from the corresponding UserAttributes.
3. Remove the related XpTransactions.
4. Remove the HabitCompletion.

All changes must succeed or fail together in one database transaction.

`204 No Content` is returned only after the transaction succeeds.

Negative reversal transactions and immutable undo history are deferred until after the MVP.

---

# Attribute Endpoints

Attribute endpoints are implemented during Phase 5.

---

## Get User Attributes

```text
GET /api/attributes
```

Returns the authenticated user's character attributes.

### Authentication Required

Yes.

### Response

An array of `UserAttributeResponse`.

```json
[
  {
    "attributeType": "discipline",
    "currentXp": 315,
    "level": 4,
    "xpRequiredForNextLevel": 500
  },
  {
    "attributeType": "strength",
    "currentXp": 240,
    "level": 3,
    "xpRequiredForNextLevel": 350
  }
]
```

`currentXp` is the persisted current net XP balance for the attribute.

`level` and `xpRequiredForNextLevel` are calculated by the backend from `currentXp`.

They are returned through the response DTO but are not stored as separate UserAttribute database fields during the MVP.

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`

---

# Dashboard Endpoints

Dashboard endpoints are implemented during Phase 6.

---

## Get Today's Dashboard

```text
GET /api/dashboard/today
```

Returns the data needed to render the authenticated user's main dashboard.

### Authentication Required

Yes.

### Response

`DashboardResponse`

```json
{
  "date": "2026-07-10",
  "user": {
    "displayName": "fred",
    "level": 4,
    "totalXp": 380,
    "xpRequiredForNextLevel": 500
  },
  "habitSummary": {
    "completedCount": 2,
    "totalCount": 4
  },
  "habits": [],
  "attributes": [],
  "streaks": []
}
```

`totalXp` is derived from the sum of the user's currently applied XpTransaction amounts.

Overall `level` and `xpRequiredForNextLevel` are calculated by the backend from `totalXp`.

These progression values are not authoritative frontend calculations.

The exact nested shapes will reuse documented response types where practical.

For example:

- `habits` may contain `HabitResponse`
- `attributes` may contain `UserAttributeResponse`

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`

---

## Get Weekly Dashboard Summary

```text
GET /api/dashboard/week
```

Returns a basic weekly progress summary.

### Authentication Required

Yes.

The MVP endpoint returns the authenticated user's current week.

The backend determines the current local date from the user's stored time zone.

The MVP week begins on Monday and ends on Sunday.

Selecting historical weeks is deferred until the data model can accurately preserve historical habit configuration and scheduling changes.

### Response

```json
{
  "weekStart": "2026-07-06",
  "weekEnd": "2026-07-12",
  "completedCount": 18,
  "scheduledCount": 24,
  "completionRate": 75,
  "xpEarned": 240,
  "topAttribute": "discipline"
}
```

Response field meanings:

- `weekStart` is the Monday of the user's current local week.
- `weekEnd` is the following Sunday.
- `completedCount` is the number of applicable HabitCompletions in that week.
- `scheduledCount` is the total weekly completion target calculated by DashboardService.
- `completionRate` is calculated by the backend from completedCount and scheduledCount.
- `xpEarned` is the sum of currently applied XpTransactions linked to completions in the week.
- `topAttribute` is the attribute that received the most XP in the week and may be `null` when no XP was earned.

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`

---

# Future API Extension Points

The following API areas are intentionally not part of the initial MVP contract:

- advanced scheduling
- sleep tracking
- bad habit tracking
- dashboard customization
- gamification visibility settings
- reminders
- notifications
- social features
- leaderboards
- public profiles
- AI recommendations
- milestones and achievements
- historical weekly summaries

Possible future settings may include:

- enabling or disabling gamification UI
- hiding XP or levels
- selecting visible dashboard sections
- changing dashboard widget order

These should be added through deliberate contract updates rather than being embedded prematurely in the MVP endpoints.

---

# Contract Change Rules

When changing an existing API contract:

1. Explain why the current contract is insufficient.
2. Identify affected frontend and backend code.
3. Identify affected DTOs.
4. Identify affected tests.
5. Update this document.
6. Avoid breaking existing clients without a clear reason.
7. Make the change in a focused commit.

The frontend and backend must agree on request and response shapes.

A contract change is not complete until both sides are updated and verified.

---

# Endpoint Phase Summary

## Phase 2

- `GET /api/auth/csrf-token`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`

## Phase 3

- `GET /api/habits`
- `GET /api/habits/{habitId}`
- `POST /api/habits`
- `PUT /api/habits/{habitId}`
- `PATCH /api/habits/{habitId}/deactivate`

## Phase 4

- `POST /api/habits/{habitId}/completions`
- `DELETE /api/habits/{habitId}/completions/today`

## Phase 5

- `GET /api/attributes`

## Phase 6

- `GET /api/dashboard/today`
- `GET /api/dashboard/week`