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

Example:

```json
{
  "habitId": "example-id",
  "completedDate": "2026-07-10"
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

The authentication mechanism will be finalized during Phase 2.

The initial contract assumes token-based authentication, but the exact token storage and transport strategy may be refined before implementation.

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
  "password": "example-password"
}
```

### Response

`AuthResponse`

```json
{
  "accessToken": "token-value",
  "expiresAtUtc": "2026-07-10T23:00:00Z",
  "user": {
    "id": "user-id",
    "email": "fred@example.com",
    "username": "fred"
  }
}
```

### Possible Status Codes

- `201 Created`
- `400 Bad Request`
- `409 Conflict`

`409 Conflict` may be returned when the email or username is already registered.

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
  "password": "example-password"
}
```

### Response

`AuthResponse`

```json
{
  "accessToken": "token-value",
  "expiresAtUtc": "2026-07-10T23:00:00Z",
  "user": {
    "id": "user-id",
    "email": "fred@example.com",
    "username": "fred"
  }
}
```

### Possible Status Codes

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`

---

## Get Current User

```text
GET /api/auth/me
```

Returns basic information about the authenticated user.

### Authentication Required

Yes.

### Response

```json
{
  "id": "user-id",
  "email": "fred@example.com",
  "username": "fred",
  "displayName": "Fred"
}
```

### Possible Status Codes

- `200 OK`
- `401 Unauthorized`

---

# Habit Endpoints

Habit endpoints are implemented during Phase 3.

---

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
  "difficulty": "medium",
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

The backend assigns the authenticated user's identity.

The request does not include `userId`.

### Response

`HabitResponse`

### Possible Status Codes

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`

### Validation Notes

The backend should validate:

- habit name
- supported frequency type
- positive target count
- supported difficulty
- supported attribute types
- valid XP amounts
- duplicate attribute rewards within the same habit

The exact XP limits will be decided when XP rules are implemented.

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
  "difficulty": "hard",
  "attributeRewards": [
    {
      "attributeType": "strength",
      "xpAmount": 25
    },
    {
      "attributeType": "discipline",
      "xpAmount": 15
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

The backend determines the correct habit date using the authenticated user's date and time settings.

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

`409 Conflict` is returned when the habit is already completed for the same date.

XP rewards are introduced during Phase 5. During Phase 4, the response may initially contain only completion data.

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

When XP is implemented, undoing a completion must also reverse its XP effects consistently.

The exact XP reversal strategy will be decided during Phase 5.

Possible strategies include:

- removing the related XP transactions
- creating negative reversal transactions

The chosen approach must preserve a trustworthy XP history.

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
    "displayName": "Fred",
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

### Optional Query Parameter

```text
weekStart=2026-07-06
```

If no week is supplied, the backend uses the authenticated user's current week.

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

### Possible Status Codes

- `200 OK`
- `400 Bad Request`
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

- `POST /api/auth/register`
- `POST /api/auth/login`
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