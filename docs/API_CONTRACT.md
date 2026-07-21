# Gamified Habit Tracker — API Contract

**Current implementation status:** Phase 5 complete
**Next contract work:** Phase 6 — Dashboard and Streaks
**Frontend:** React, TypeScript, Vite, Tailwind CSS
**Backend:** ASP.NET Core Web API
**Authentication:** Encrypted cookie authentication with antiforgery protection
**Contract rule:** The backend returns authoritative values; the frontend displays them

---

## 1. Purpose

This document defines the HTTP contract between the React frontend and the ASP.NET Core backend.

It records:

- implemented routes,
- authentication requirements,
- request shapes,
- response shapes,
- enum values,
- status codes,
- ownership behavior,
- validation behavior,
- error behavior,
- planned Phase 6 contract direction.

The contract must distinguish clearly between:

- implemented,
- proposed next,
- deferred.

A field shown in a UI reference image is not part of the API contract unless its behavior is implemented, tested, and documented here.

---

## 2. Base Path

Application API routes begin with:

```text
/api
```

Examples:

```text
/api/health
/api/auth/login
/api/habits
/api/attributes
```

---

## 3. General Conventions

### 3.1 Data format

Requests and responses use JSON unless otherwise documented.

JSON properties use camel case.

Examples:

```text
createdAtUtc
isCompletedToday
attributeRewards
xpNeededForNextLevel
```

### 3.2 Enum serialization

Enums are serialized as camel-case strings.

Numeric enum values are rejected and are not part of the public contract.

Examples:

```json
{
  "frequencyType": "weekly",
  "difficulty": "medium",
  "category": "learningAndSkills"
}
```

### 3.3 Identifiers

Identifiers are JSON strings containing `Guid` values.

Currently implemented entities use application-generated UUID version 7 identifiers.

Clients must treat identifiers as opaque values.

The frontend must not derive ordering, dates, or business behavior from the identifier format.

Example:

```json
{
  "id": "019ba123-4567-789a-bcde-f0123456789a"
}
```

### 3.4 Dates

Date-only values use:

```text
YYYY-MM-DD
```

Example:

```text
2026-07-21
```

### 3.5 UTC timestamps

UTC timestamps use ISO 8601.

Example:

```text
2026-07-21T03:33:20Z
```

Properties containing UTC timestamps end with `Utc`.

Examples:

- `createdAtUtc`
- `updatedAtUtc`
- `completedAtUtc`
- `checkedAtUtc`

### 3.6 Null values

Optional text fields may return `null`.

Blank optional text submitted by the client is normalized to `null` where documented.

Example:

```json
{
  "description": null,
  "notes": null
}
```

---

## 4. Implemented Enum Contracts

## 4.1 `HabitFrequencyType`

Supported values:

- `daily`
- `weekly`

Rules:

- Daily habits require `targetCount` of `1`.
- Weekly habits require `targetCount` from `1` through `7`.
- A habit may be completed at most once per local date.

---

## 4.2 `HabitDifficulty`

Supported values:

- `easy`
- `medium`
- `hard`
- `elite`

Difficulty determines total XP:

| Difficulty | Total XP |
|---|---:|
| Easy | 10 |
| Medium | 20 |
| Hard | 30 |
| Elite | 50 |

The client submits difficulty.

The backend calculates rewards from difficulty and category.

---

## 4.3 `HabitCategory`

Supported values:

- `fitnessAndMovement`
- `healthAndRecovery`
- `learningAndSkills`
- `workAndCareer`
- `dailyLifeAndOrganization`
- `moneyAndFinance`
- `relationshipsAndCommunity`
- `emotionalWellBeing`
- `spiritualityAndPurpose`
- `creativityAndRecreation`
- `selfControlAndBoundaries`
- `generalGrowth`

Category is required.

It is not free text.

The backend uses category to select the primary and secondary attributes for automatic rewards.

---

## 4.4 `AttributeType`

Supported values:

- `discipline`
- `fitness`
- `vitality`
- `focus`
- `mind`
- `resilience`
- `social`
- `purpose`

These values are stable across:

- habit reward responses,
- completion reward responses,
- user attribute responses,
- XP transactions,
- future dashboard responses.

---

## 5. Authentication and Browser Security

Protected endpoints require an authenticated user.

The browser application uses encrypted cookie authentication.

The frontend does not:

- receive an authentication token in JSON,
- read the authentication cookie,
- decode authentication credentials,
- manually attach an authorization bearer token.

After registration or login, ASP.NET Core creates the authentication cookie and the browser sends it automatically on later requests.

### 5.1 Authentication cookie

The cookie:

- is named `HabitTracker.Auth`,
- is `HttpOnly`,
- uses `SameSite=Lax`,
- is secure outside Development,
- expires after 12 hours by default,
- is non-persistent by default,
- may persist for 30 days when `rememberMe` is true,
- does not use sliding expiration during MVP.

Registration always creates the default non-persistent session.

### 5.2 Frontend credentials

Frontend requests use:

```javascript
credentials: "include"
```

### 5.3 Antiforgery protection

State-changing controller requests require antiforgery validation:

- `POST`
- `PUT`
- `PATCH`
- `DELETE`

The antiforgery cookie is:

```text
HabitTracker.Antiforgery
```

The antiforgery request header is:

```text
X-CSRF-TOKEN
```

The frontend:

1. requests a token from `GET /api/auth/csrf-token`,
2. caches the request token in memory,
3. attaches it to state-changing requests,
4. clears the cached token after registration, login, or logout,
5. obtains a replacement lazily when another state-changing request occurs.

### 5.4 Local browser origin

During local development, the browser calls `/api` through the Vite development server.

Vite proxies those requests to ASP.NET Core.

This preserves separate frontend and backend applications while presenting same-origin browser requests.

### 5.5 Authenticated identity

The backend derives the current user identifier from the authenticated claims principal.

User-owned request bodies must not accept `userId`.

The frontend cannot choose which user owns a resource.

---

## 6. Ownership and Privacy

Every protected operation must limit data to the authenticated user.

A user may only:

- view their own habits,
- edit their own habits,
- deactivate their own habits,
- complete their own habits,
- undo their own completions,
- view their own attributes,
- view their own future dashboard.

For user-owned resources, missing and foreign-owned identifiers are generally treated the same:

```text
404 Not Found
```

This avoids revealing whether another user’s resource exists.

Ownership is enforced by backend queries and services, not by hiding UI controls.

---

## 7. Standard Status Codes

- `200 OK` — Request succeeded and returns a response body.
- `201 Created` — A resource or completion was created.
- `204 No Content` — Request succeeded with no response body.
- `400 Bad Request` — Request shape or domain input is invalid.
- `401 Unauthorized` — Authentication is missing or invalid.
- `403 Forbidden` — The user is authenticated but access is denied.
- `404 Not Found` — The resource does not exist for the authenticated user.
- `409 Conflict` — The request conflicts with current state.
- `500 Internal Server Error` — An unexpected failure occurred.

Examples of `409 Conflict`:

- duplicate normalized email,
- duplicate normalized username,
- completing an inactive habit,
- completing a habit already completed for the same local date.

---

## 8. Error Responses

Expected errors use ASP.NET Core Problem Details.

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

Validation errors may include field-specific messages.

Example:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "name": [
      "The Name field is required."
    ]
  }
}
```

Production responses must not expose:

- stack traces,
- database connection details,
- SQL,
- password hashes,
- internal exception messages not intended for users.

Invalid login credentials use a generic response that does not reveal whether the email or password was incorrect.

---

# Implemented Endpoints

## 9. Health Endpoint

## 9.1 Get API health

```text
GET /api/health
```

### Authentication required

No.

### Response

`HealthResponse`

```json
{
  "status": "Healthy",
  "checkedAtUtc": "2026-07-21T03:33:20Z"
}
```

### Status codes

- `200 OK`

This endpoint confirms that the API process is responding.

Application startup already fails when the configured PostgreSQL database cannot be reached.

---

# Authentication Endpoints

## 10. Get antiforgery token

```text
GET /api/auth/csrf-token
```

### Authentication required

No.

The endpoint may be called before or after authentication.

### Response

`AntiforgeryTokenResponse`

```json
{
  "requestToken": "antiforgery-token-value"
}
```

### Status codes

- `200 OK`

---

## 11. Register user

```text
POST /api/auth/register
```

### Authentication required

No.

### Antiforgery required

Yes.

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

### Behavior

Registration:

- trims email and username,
- normalizes email and username for case-insensitive lookup,
- validates the IANA time zone,
- hashes the password,
- creates `User`,
- creates default `UserSettings`,
- uses username as the initial display name,
- saves user and settings together,
- creates a temporary authenticated session.

Password whitespace is preserved.

### Response

`AuthResponse`

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

No authentication credential appears in the JSON body.

### Validation summary

- `email`: required, valid email shape, maximum 254 characters
- `username`: required, 3–30 characters, letters/numbers/underscores
- `password`: required, 15–128 characters
- `timeZone`: required, maximum 100 characters, valid supported IANA identifier

### Status codes

- `201 Created`
- `400 Bad Request`
- `409 Conflict`

---

## 12. Login user

```text
POST /api/auth/login
```

### Authentication required

No.

### Antiforgery required

Yes.

### Request

`LoginRequest`

```json
{
  "email": "fred@example.com",
  "password": "example-password",
  "rememberMe": true
}
```

`rememberMe` defaults to `false`.

### Response

`AuthResponse`

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

### Status codes

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`

---

## 13. Logout user

```text
POST /api/auth/logout
```

### Authentication required

No.

Logout is idempotent.

### Antiforgery required

Yes.

### Request

No request body.

### Response

No response body.

### Status codes

- `204 No Content`

---

## 14. Get current user

```text
GET /api/auth/me
```

### Authentication required

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

### Status codes

- `200 OK`
- `401 Unauthorized`

When the authenticated user record no longer exists, the backend ends the invalid session and returns `401 Unauthorized`.

---

# Habit Endpoints

## 15. `HabitResponse`

All implemented habit-returning endpoints use this response shape.

```json
{
  "id": "habit-id",
  "name": "Read C# textbook",
  "description": "Read one chapter.",
  "category": "learningAndSkills",
  "frequencyType": "daily",
  "targetCount": 1,
  "difficulty": "medium",
  "attributeRewards": [
    {
      "attributeType": "mind",
      "xpAmount": 14
    },
    {
      "attributeType": "focus",
      "xpAmount": 6
    }
  ],
  "isActive": true,
  "isCompletedToday": false,
  "createdAtUtc": "2026-07-19T12:00:00Z",
  "updatedAtUtc": "2026-07-19T12:00:00Z"
}
```

### `attributeRewards`

`attributeRewards` is always backend-produced.

The client does not submit this property during create or update.

For a valid current habit, the response contains the two automatic rewards derived from category and difficulty.

Rewards are returned in deterministic order:

1. higher XP first,
2. attribute enum order as the tie-breaker.

### `isCompletedToday`

The backend calculates `isCompletedToday` from:

- stored `HabitCompletion` records,
- the authenticated user’s stored time zone,
- the current UTC time.

The client does not submit this property.

---

## 16. Get user habits

```text
GET /api/habits
```

### Authentication required

Yes.

### Optional query

```text
includeInactive=false
```

Example:

```text
GET /api/habits?includeInactive=true
```

### Response

Array of `HabitResponse`.

```json
[
  {
    "id": "habit-id",
    "name": "Go to gym",
    "description": "Complete a planned gym session.",
    "category": "fitnessAndMovement",
    "frequencyType": "weekly",
    "targetCount": 3,
    "difficulty": "hard",
    "attributeRewards": [
      {
        "attributeType": "fitness",
        "xpAmount": 21
      },
      {
        "attributeType": "discipline",
        "xpAmount": 9
      }
    ],
    "isActive": true,
    "isCompletedToday": false,
    "createdAtUtc": "2026-07-20T15:00:00Z",
    "updatedAtUtc": "2026-07-20T15:00:00Z"
  }
]
```

Default behavior:

- inactive habits are excluded,
- active habits are ordered before inactive habits when included,
- newer habits appear first within each active state,
- stable identifier ordering resolves timestamp ties.

### Status codes

- `200 OK`
- `401 Unauthorized`

---

## 17. Get habit by ID

```text
GET /api/habits/{habitId}
```

### Authentication required

Yes.

### Response

`HabitResponse`

### Status codes

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

Missing and foreign-owned habits return `404 Not Found`.

---

## 18. Create habit

```text
POST /api/habits
```

### Authentication required

Yes.

### Antiforgery required

Yes.

### Request

`CreateHabitRequest`

```json
{
  "name": "Go to gym",
  "description": "Complete a planned gym session.",
  "category": "fitnessAndMovement",
  "frequencyType": "weekly",
  "targetCount": 3,
  "difficulty": "hard"
}
```

The request must not contain:

- `userId`,
- `attributeRewards`,
- `isActive`,
- `isCompletedToday`,
- timestamps.

### Backend behavior

The backend:

- derives the owner from claims,
- trims `name`,
- trims `description`,
- converts blank description to `null`,
- validates category,
- validates frequency and target count together,
- validates difficulty,
- calculates the two automatic rewards,
- stores the reward rows with the habit.

### Validation summary

- `name`: required after trimming, maximum 100 characters
- `description`: optional, maximum 500 characters
- `category`: required and supported
- `frequencyType`: required and supported
- `targetCount`: required, range 1–7
- Daily requires `targetCount` 1
- Weekly requires `targetCount` 1–7
- `difficulty`: required and supported

### Response

`HabitResponse`

### Status codes

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`

---

## 19. Update habit

```text
PUT /api/habits/{habitId}
```

### Authentication required

Yes.

### Antiforgery required

Yes.

### Request

`UpdateHabitRequest`

```json
{
  "name": "Go to gym",
  "description": "Complete the full planned session.",
  "category": "fitnessAndMovement",
  "frequencyType": "weekly",
  "targetCount": 4,
  "difficulty": "elite"
}
```

The request uses the same validation rules as creation.

The request must not include `attributeRewards`.

When category or difficulty changes, the backend synchronizes the stored automatic rewards.

### Response

Updated `HabitResponse`.

### Status codes

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

Owned active and inactive habits may be updated.

Missing and foreign-owned habits return `404 Not Found`.

---

## 20. Deactivate habit

```text
DELETE /api/habits/{habitId}
```

### Authentication required

Yes.

### Antiforgery required

Yes.

### Request

No request body.

### Response

Updated `HabitResponse` with:

```json
{
  "isActive": false
}
```

The actual response includes the full `HabitResponse` shape.

### Status codes

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

### Behavior

- The habit is soft-deactivated.
- The row and history are preserved.
- Repeating the request for an already inactive owned habit is successful.
- Idempotent repeat deactivation does not update `updatedAtUtc` again.
- Missing and foreign-owned habits return `404 Not Found`.

---

# Completion Endpoints

## 21. Complete habit

```text
POST /api/habits/{habitId}/completions
```

### Authentication required

Yes.

### Antiforgery required

Yes.

### Request

`CompleteHabitRequest`

```json
{
  "notes": "Completed after work."
}
```

The client may send:

```json
{
  "notes": null
}
```

The request must not contain:

- `userId`,
- `completedDate`,
- `completedAtUtc`,
- reward values,
- XP amounts.

### Backend behavior

The backend:

1. verifies ownership,
2. verifies the habit is active,
3. loads the user’s stored time zone,
4. obtains current UTC time,
5. calculates the user-local `completedDate`,
6. rejects an existing completion for the same habit and local date,
7. normalizes notes,
8. ensures the automatic habit reward mapping exists,
9. updates the corresponding user attributes,
10. creates XP transactions,
11. creates the completion,
12. saves the changes together.

### Response

`CompleteHabitResponse`

```json
{
  "completion": {
    "id": "completion-id",
    "habitId": "habit-id",
    "completedDate": "2026-07-21",
    "completedAtUtc": "2026-07-21T03:33:20Z",
    "notes": "Completed after work."
  },
  "rewards": [
    {
      "attributeType": "mind",
      "xpAmount": 14
    },
    {
      "attributeType": "focus",
      "xpAmount": 6
    }
  ]
}
```

`rewards` contains the exact backend-applied XP.

The frontend displays these values and must not recalculate them.

### Status codes

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`
- `409 Conflict`

`404 Not Found`:

- habit does not exist,
- habit belongs to another user.

`409 Conflict`:

- habit is inactive,
- habit is already completed for the current local date.

---

## 22. Undo today’s completion

```text
DELETE /api/habits/{habitId}/completions/today
```

### Authentication required

Yes.

### Antiforgery required

Yes.

### Request

No request body.

### Response

No response body.

### Backend behavior

The backend:

1. verifies that the owned habit exists,
2. calculates the authenticated user’s current local date,
3. finds today’s completion,
4. loads XP transactions linked to that completion,
5. subtracts their values from the corresponding user attributes,
6. removes the XP transactions,
7. removes the completion,
8. saves the changes together.

Undo is permitted when the owned habit has since been deactivated.

### Status codes

- `204 No Content`
- `401 Unauthorized`
- `404 Not Found`

`404 Not Found` is returned when:

- the habit is missing,
- the habit belongs to another user,
- no completion exists for the current local date.

The current MVP deletes the applied XP transactions during undo.

Immutable reversal history and negative reversal transactions are deferred.

---

# Attribute Endpoints

## 23. Get user attributes

```text
GET /api/attributes
```

### Authentication required

Yes.

### Response

Array of `UserAttributeResponse`.

```json
[
  {
    "attributeType": "discipline",
    "currentXp": 99,
    "level": 1,
    "xpIntoCurrentLevel": 99,
    "xpNeededForNextLevel": 100
  },
  {
    "attributeType": "fitness",
    "currentXp": 225,
    "level": 3,
    "xpIntoCurrentLevel": 0,
    "xpNeededForNextLevel": 150
  }
]
```

### Behavior

The response contains all eight supported attributes in enum order:

1. discipline
2. fitness
3. vitality
4. focus
5. mind
6. resilience
7. social
8. purpose

When a `UserAttribute` row does not yet exist, the API returns a zero-XP response for that attribute.

The backend calculates:

- `level`,
- `xpIntoCurrentLevel`,
- `xpNeededForNextLevel`.

Only `currentXp` is persisted on `UserAttribute`.

The frontend may calculate a visual percentage from the returned progress values.

The frontend must not implement the authoritative level curve.

### Status codes

- `200 OK`
- `401 Unauthorized`

---

# Phase 6 Proposed Contract

## 24. Status

Dashboard and streak endpoints are not implemented yet.

The following direction is approved for Phase 6, but exact DTO names and final response fields must be confirmed in the first Phase 6 contract slice before coding.

The planned approach is one aggregated dashboard endpoint rather than forcing React to combine many unrelated requests.

---

## 25. Proposed dashboard endpoint

```text
GET /api/dashboard
```

### Authentication required

Yes.

### Purpose

Return the authoritative data needed by the main daily dashboard.

### Proposed response direction

`DashboardResponse`

```json
{
  "date": "2026-07-21",
  "displayName": "fred",
  "overallProgress": {
    "totalXp": 380,
    "level": 2,
    "xpIntoCurrentLevel": 180,
    "xpNeededForNextLevel": 250
  },
  "todaySummary": {
    "completedCount": 2,
    "remainingCount": 2,
    "totalCount": 4,
    "completionPercentage": 50,
    "xpEarnedToday": 40
  },
  "streak": {
    "currentStreak": 3,
    "longestStreak": 8
  },
  "habits": [],
  "attributes": [],
  "recentActivity": [],
  "weeklySummary": {
    "weekStart": "2026-07-20",
    "weekEnd": "2026-07-26",
    "completedCount": 8,
    "targetCount": 14,
    "completionPercentage": 57,
    "xpEarned": 120
  }
}
```

This example establishes product direction, not a frozen implementation contract.

Before implementation, Phase 6 must approve:

- exact streak meaning,
- daily versus weekly habit treatment,
- whether streak is overall, per-habit, or both,
- today’s treatment when the day is still in progress,
- exact weekly target calculation,
- recent-activity shape,
- whether existing `HabitResponse` and `UserAttributeResponse` are reused directly,
- exact route and nested DTO names.

### Backend ownership

The backend must calculate:

- total XP,
- overall level,
- overall level progress,
- completed and remaining counts,
- completion percentage,
- XP earned today,
- current streak,
- longest streak,
- weekly totals,
- weekly percentages.

React must not calculate authoritative dashboard business values from raw records.

### Planned status codes

- `200 OK`
- `401 Unauthorized`

---

## 26. Dashboard contract guardrails

Phase 6 must not add fields merely because a mockup displays them.

Do not add unsupported:

- rank,
- currency,
- focus score,
- notifications,
- quests,
- avatar state,
- social comparison,
- achievement claims.

Dashboard DTOs should contain only implemented and tested behavior.

A separate weekly endpoint should be introduced only when a real screen or performance need justifies it.

The initial dashboard should prefer one aggregate response to avoid duplicated frontend orchestration.

---

# Deferred API Areas

## 27. Post-MVP endpoints

The following are outside the current MVP contract:

- advanced scheduling,
- sleep tracking,
- bad-habit tracking,
- journal entries,
- reminders,
- notifications,
- social features,
- leaderboards,
- public profiles,
- avatars,
- quests,
- inventory,
- currencies,
- AI recommendations,
- milestone and achievement management,
- historical weekly-review selection,
- payments,
- admin endpoints,
- calendar integration.

These require explicit product and architecture decisions before contract design.

---

# Contract Change Process

## 28. Required review

When an existing response changes, review:

- backend response DTO,
- backend service mapping,
- controller behavior,
- unit tests,
- HTTP integration tests,
- JSON contract tests,
- frontend TypeScript type,
- frontend API parser,
- frontend API tests,
- component fixtures,
- consuming components,
- this document.

### Example

Adding `rewards` to `CompleteHabitResponse` requires all affected backend and frontend layers to change together.

A backend-only response change is incomplete.

---

## 29. Contract design rules

1. Backend responses are purpose-built DTOs.
2. Database entities are never returned directly.
3. User identifiers are derived from claims.
4. User-owned request bodies do not accept `userId`.
5. Enum values remain stable camel-case strings.
6. Numeric enums are rejected.
7. Business calculations remain authoritative on the server.
8. The frontend may format and visualize returned values.
9. Aggregate page responses are preferred when they reduce needless orchestration.
10. Speculative fields are not added.
11. Errors use consistent status codes and Problem Details.
12. Contract changes require automated tests.
13. Implemented and proposed contracts must remain clearly separated.

---

## 30. UI alignment

The API exists to provide real data for the approved interface.

It should support:

- today’s dashboard,
- habit management,
- automatic reward previews,
- attribute progression,
- streaks,
- weekly review,
- recent activity.

It must not force React to:

- reconstruct domain aggregates,
- calculate XP,
- calculate levels,
- calculate streaks,
- infer ownership,
- invent unsupported game systems.

The UI visualizes the application system.

The API defines the authoritative application system.

---

# Endpoint Summary

## 31. Implemented — Phase 1

- `GET /api/health`

## 32. Implemented — Phase 2

- `GET /api/auth/csrf-token`
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`

## 33. Implemented — Phase 3

- `GET /api/habits`
- `GET /api/habits/{habitId}`
- `POST /api/habits`
- `PUT /api/habits/{habitId}`
- `DELETE /api/habits/{habitId}`

## 34. Implemented — Phase 4

- `POST /api/habits/{habitId}/completions`
- `DELETE /api/habits/{habitId}/completions/today`

## 35. Implemented — Phase 5

- `GET /api/attributes`
- automatic `attributeRewards` in `HabitResponse`
- applied `rewards` in `CompleteHabitResponse`

## 36. Proposed — Phase 6

- `GET /api/dashboard`

The exact Phase 6 response contract must be approved before implementation.
