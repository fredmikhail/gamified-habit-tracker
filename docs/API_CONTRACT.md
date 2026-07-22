# Gamified Habit Tracker — API Contract

**Status:** Current through Phase 6

**Current phase:** Phase 7 — Game UI Polish

**Frontend:** React, TypeScript, Vite, Tailwind CSS

**Backend:** ASP.NET Core Web API

**Authentication:** Encrypted cookie authentication with antiforgery protection

**Contract rule:** API responses contain authoritative application values; clients present them

---

## 1. Purpose

This document defines the HTTP contract between the React client and the ASP.NET Core API.

It records:

- implemented routes,
- authentication requirements,
- antiforgery requirements,
- request bodies,
- response bodies,
- enum values,
- date and timestamp formats,
- ownership behavior,
- validation behavior,
- status codes,
- error behavior,
- effective-dated habit updates,
- completion and undo behavior,
- dashboard aggregation,
- streak semantics,
- deferred API areas.

Only implemented and tested behavior belongs in the active contract.

Database entities are not public response models.

---

## 2. Base Path

Application routes begin with:

```text
/api
```

Examples:

```text
/api/health
/api/auth/login
/api/habits
/api/attributes
/api/dashboard
```

---

## 3. General Conventions

### 3.1 Content type

Requests and responses use JSON unless an endpoint explicitly returns no content.

Typical request header:

```text
Content-Type: application/json
```

### 3.2 Property naming

JSON properties use camel case.

Examples:

```text
createdAtUtc
isCompletedToday
pendingConfiguration
attributeRewards
xpIntoCurrentLevel
```

### 3.3 Enum serialization

Enums are serialized as camel-case strings.

Numeric enum values are rejected.

Valid example:

```json
{
  "frequencyType": "weekly",
  "difficulty": "medium",
  "category": "learningAndSkills"
}
```

Invalid example:

```json
{
  "frequencyType": 2
}
```

### 3.4 Identifiers

Identifiers are JSON strings containing GUID values.

Implemented entities use application-generated UUID version 7 identifiers.

Clients must treat identifiers as opaque.

Example:

```json
{
  "id": "019ba123-4567-789a-bcde-f0123456789a"
}
```

Clients must not derive:

- creation time,
- sorting,
- ownership,
- application behavior

from the identifier format.

### 3.5 Date-only values

Calendar dates use:

```text
YYYY-MM-DD
```

Example:

```json
{
  "completedDate": "2026-07-22"
}
```

These dates represent the user-local calendar date calculated by the backend.

### 3.6 UTC timestamps

UTC timestamps use ISO 8601.

Example:

```json
{
  "completedAtUtc": "2026-07-22T19:42:15Z"
}
```

UTC timestamp properties end with `Utc`.

Examples:

- `createdAtUtc`
- `updatedAtUtc`
- `completedAtUtc`
- `checkedAtUtc`

### 3.7 Optional text

Optional text may return `null`.

Blank optional text is normalized to `null` where documented.

Example:

```json
{
  "description": null,
  "notes": null
}
```

### 3.8 Optional object properties

`pendingConfiguration` is omitted from habit JSON when no pending configuration exists.

It is not written as:

```json
{
  "pendingConfiguration": null
}
```

Clients must therefore treat both absence and their own uninitialized state appropriately.

### 3.9 Empty collections

Implemented collection properties return arrays rather than `null`.

Example:

```json
{
  "habitStreaks": []
}
```

---

## 4. Public Enum Contracts

### 4.1 `HabitFrequencyType`

Supported JSON values:

- `daily`
- `weekly`

Rules:

```text
daily  -> targetCount must equal 1
weekly -> targetCount must be between 1 and 7
```

Only one active, non-undone completion may exist for a habit on a user-local date.

A weekly target therefore represents distinct completion dates within the week.

### 4.2 `HabitDifficulty`

Supported JSON values:

- `easy`
- `medium`
- `hard`
- `elite`

Difficulty determines total XP:

| Difficulty | Total XP |
| ---------- | -------: |
| Easy       |       10 |
| Medium     |       20 |
| Hard       |       30 |
| Elite      |       50 |

The client submits difficulty as part of habit creation and editing.

The backend calculates the resulting rewards.

### 4.3 `HabitCategory`

Supported JSON values:

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

Category is required and is not free text.

The backend uses category to select the primary and secondary attributes.

### 4.4 `AttributeType`

Supported JSON values:

- `discipline`
- `fitness`
- `vitality`
- `focus`
- `mind`
- `resilience`
- `social`
- `purpose`

These values appear in:

- habit reward responses,
- completion reward responses,
- user attribute responses.

### 4.5 Week-start values

The backend stores a `WeekStartDay` value with the following possible values:

- Monday
- Tuesday
- Wednesday
- Thursday
- Friday
- Saturday
- Sunday

The current public API does not expose a week-start request or response field.

New users receive Monday as the default.

The stored value is still used internally for:

- pending habit configuration boundaries,
- weekly streak calculation.

A user-settings update endpoint is not currently implemented.

---

## 5. Authentication and Browser Security

Protected endpoints require an authenticated user.

The browser application uses ASP.NET Core encrypted cookie authentication.

The client does not:

- receive an access token in JSON,
- read the authentication cookie,
- decode authentication credentials,
- store a bearer token,
- attach a bearer token manually.

After registration or login, ASP.NET Core creates the authentication cookie.

The browser sends it automatically with later credentialed requests.

### 5.1 Authentication cookie

The authentication cookie:

- is named `HabitTracker.Auth`,
- is encrypted by ASP.NET Core,
- is `HttpOnly`,
- uses `SameSite=Lax`,
- is secure outside Development,
- is non-persistent by default,
- has a maximum 12-hour lifetime by default,
- may persist for up to 30 days when `rememberMe` is `true`,
- does not use sliding expiration.

Registration creates the default non-persistent session.

### 5.2 Browser credentials

Frontend API calls use:

```javascript
credentials: "include";
```

### 5.3 Antiforgery protection

State-changing controller requests require antiforgery validation:

- `POST`
- `PUT`
- `PATCH`
- `DELETE`

Antiforgery cookie:

```text
HabitTracker.Antiforgery
```

Antiforgery request header:

```text
X-CSRF-TOKEN
```

The client:

1. requests a token from `GET /api/auth/csrf-token`,
2. caches the request token in memory,
3. attaches it to state-changing requests,
4. includes browser credentials,
5. clears its cached token when authentication identity changes,
6. obtains a new token when required.

### 5.4 Local development origin

During local development, the browser calls `/api` through Vite.

Vite proxies the request to the ASP.NET Core HTTPS endpoint.

Typical origins:

```text
Frontend: http://localhost:5173
Backend:  https://localhost:7287
```

### 5.5 Authenticated identity

The backend derives the authenticated user identifier from the claims principal.

User-owned request bodies do not accept `userId`.

The client cannot choose which user owns or receives a resource.

---

## 6. Ownership and Privacy

Every protected operation is scoped to the authenticated user.

A user may only:

- list their own habits,
- retrieve their own habits,
- edit their own habits,
- deactivate their own habits,
- complete their own habits,
- undo their own completions,
- view their own attributes,
- view their own dashboard.

For owned resources, missing and foreign-owned identifiers generally produce the same response:

```text
404 Not Found
```

This avoids confirming that another user’s resource exists.

Ownership is enforced by backend queries and services.

Hiding a frontend control is not an authorization boundary.

---

## 7. Status Codes

Common status codes:

| Status                      | Meaning                                    |
| --------------------------- | ------------------------------------------ |
| `200 OK`                    | Request succeeded and returns a body       |
| `201 Created`               | A resource or completion was created       |
| `204 No Content`            | Request succeeded without a body           |
| `400 Bad Request`           | Request validation or domain input failed  |
| `401 Unauthorized`          | Authentication is missing or invalid       |
| `403 Forbidden`             | Authentication exists but access is denied |
| `404 Not Found`             | No matching owned resource exists          |
| `409 Conflict`              | The request conflicts with current state   |
| `500 Internal Server Error` | An unexpected failure occurred             |

Examples of `409 Conflict`:

- duplicate normalized email,
- duplicate normalized username,
- completing an inactive habit,
- creating a second active completion for the same habit and date.

---

## 8. Error Responses

### 8.1 Domain errors

Known application exceptions are mapped to Problem Details responses.

Current mappings include:

| Condition                   | Status | Title                        |
| --------------------------- | -----: | ---------------------------- |
| Invalid IANA time zone      |    400 | `Invalid time zone`          |
| Invalid habit name          |    400 | `Invalid habit name`         |
| Invalid target count        |    400 | `Invalid habit target count` |
| Invalid credentials         |    401 | `Invalid credentials`        |
| Duplicate account value     |    409 | `Account conflict`           |
| Inactive habit completion   |    409 | `Inactive habit`             |
| Duplicate active completion |    409 | `Habit already completed`    |

Representative response:

```json
{
  "title": "Habit already completed",
  "status": 409,
  "detail": "This habit has already been completed for today.",
  "instance": "/api/habits/habit-id/completions"
}
```

### 8.2 Request-model validation

ASP.NET Core request validation returns a validation problem response.

Representative response:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "name": ["The Name field is required."]
  }
}
```

### 8.3 Direct controller results

Not every non-success result is guaranteed to contain Problem Details.

Direct controller results such as:

- `NotFound()`,
- `Unauthorized()`

may return only the status code with no application-specific body.

Clients must use the HTTP status as the primary signal.

### 8.4 Information exposure

Production responses must not expose:

- stack traces,
- database connection strings,
- SQL statements,
- password hashes,
- server secrets,
- internal exception details not intended for clients.

Invalid-login responses do not reveal whether the email or password was incorrect.

---

# Health

## 9. Get API Health

```text
GET /api/health
```

### Authentication

Not required.

### Antiforgery

Not required.

### Response

`HealthResponse`

```json
{
  "status": "Healthy",
  "checkedAtUtc": "2026-07-22T19:42:15Z"
}
```

### Status codes

- `200 OK`

The endpoint confirms that the API process is responding.

Normal application startup separately checks configured PostgreSQL connectivity and fails when the production database cannot be reached.

---

# Authentication

## 10. Get Antiforgery Token

```text
GET /api/auth/csrf-token
```

### Authentication

Not required.

The endpoint may be called before or after login.

### Antiforgery

Not required.

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

## 11. Register User

```text
POST /api/auth/register
```

### Authentication

Not required.

### Antiforgery

Required.

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

### Validation

| Property   | Rule                                                              |
| ---------- | ----------------------------------------------------------------- |
| `email`    | Required, valid email format, maximum 254 characters              |
| `username` | Required, 3–30 characters, letters/numbers/underscores            |
| `password` | Required, 15–128 characters                                       |
| `timeZone` | Required, maximum 100 characters, valid supported IANA identifier |

Email and username are trimmed.

Password whitespace is preserved.

### Behavior

Registration:

- normalizes email and username for case-insensitive uniqueness,
- validates the time zone,
- hashes the password,
- creates `User`,
- creates `UserSettings`,
- sets display name from username,
- sets week start to Monday,
- saves user and settings atomically,
- creates a non-persistent authenticated session.

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

No authentication credential appears in the response body.

`weekStartsOn` is not currently exposed in `CurrentUserResponse`.

### Status codes

- `201 Created`
- `400 Bad Request`
- `409 Conflict`

---

## 12. Login User

```text
POST /api/auth/login
```

### Authentication

Not required.

### Antiforgery

Required.

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

## 13. Logout User

```text
POST /api/auth/logout
```

### Authentication

Not required.

Logout is idempotent.

### Antiforgery

Required.

### Request

No body.

### Response

No body.

### Status codes

- `204 No Content`

---

## 14. Get Current User

```text
GET /api/auth/me
```

### Authentication

Required.

### Antiforgery

Not required.

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

When the authenticated account no longer exists, the backend invalidates the session and returns `401 Unauthorized`.

---

# Habits

## 15. `HabitAttributeRewardResponse`

```json
{
  "attributeType": "mind",
  "xpAmount": 14
}
```

Properties:

| Property        | Type            | Meaning                      |
| --------------- | --------------- | ---------------------------- |
| `attributeType` | `AttributeType` | Attribute receiving XP       |
| `xpAmount`      | integer         | XP awarded to that attribute |

The backend calculates these values.

The client does not submit reward allocations.

---

## 16. `PendingHabitConfigurationResponse`

A pending configuration describes a rule set scheduled to begin at a future user-local week boundary.

```json
{
  "effectiveFromDate": "2026-07-27",
  "category": "learningAndSkills",
  "frequencyType": "weekly",
  "targetCount": 4,
  "difficulty": "hard"
}
```

Properties:

| Property            | Type                 | Meaning                                  |
| ------------------- | -------------------- | ---------------------------------------- |
| `effectiveFromDate` | date                 | First local date using the pending rules |
| `category`          | `HabitCategory`      | Scheduled category                       |
| `frequencyType`     | `HabitFrequencyType` | Scheduled frequency                      |
| `targetCount`       | integer              | Scheduled target                         |
| `difficulty`        | `HabitDifficulty`    | Scheduled difficulty                     |

The response does not expose:

- configuration identifier,
- version number,
- effective end date,
- pending reward calculation.

---

## 17. `HabitResponse`

Habit-returning endpoints use the following shape:

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

When a future configuration is scheduled, the response includes:

```json
{
  "id": "habit-id",
  "name": "Read C# textbook",
  "description": "Read one chapter.",
  "category": "learningAndSkills",
  "frequencyType": "daily",
  "targetCount": 1,
  "difficulty": "medium",
  "pendingConfiguration": {
    "effectiveFromDate": "2026-07-27",
    "category": "learningAndSkills",
    "frequencyType": "weekly",
    "targetCount": 4,
    "difficulty": "hard"
  },
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
  "updatedAtUtc": "2026-07-22T18:00:00Z"
}
```

### Effective rule fields

These top-level fields describe the configuration effective on the current user-local date:

- `category`
- `frequencyType`
- `targetCount`
- `difficulty`

They do not switch to future pending values before `effectiveFromDate`.

### `pendingConfiguration`

`pendingConfiguration` appears only when a future configuration exists.

It is omitted when none exists.

### `attributeRewards`

`attributeRewards` corresponds to the currently effective category and difficulty.

It does not preview the pending configuration.

Rewards are ordered by:

1. higher XP amount,
2. attribute enum order as a tie-breaker.

### `isCompletedToday`

The backend calculates `isCompletedToday` using:

- the authenticated user’s time zone,
- the current UTC time,
- the resulting local date,
- an active completion where `undoneAtUtc` is absent internally.

The client does not submit this value.

An undone completion does not make `isCompletedToday` true.

---

## 18. List Habits

```text
GET /api/habits
```

### Authentication

Required.

### Antiforgery

Not required.

### Query parameters

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

### Ordering

Default:

- inactive habits are excluded.

When inactive habits are included:

1. active habits appear first,
2. newer habits appear first within each active state,
3. identifier order breaks timestamp ties.

### Status codes

- `200 OK`
- `401 Unauthorized`

---

## 19. Get Habit

```text
GET /api/habits/{habitId}
```

### Authentication

Required.

### Antiforgery

Not required.

### Response

`HabitResponse`

### Status codes

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

Missing and foreign-owned habits return `404 Not Found`.

---

## 20. Create Habit

```text
POST /api/habits
```

### Authentication

Required.

### Antiforgery

Required.

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

The request must not contain authoritative values such as:

- `userId`
- `attributeRewards`
- `pendingConfiguration`
- `isActive`
- `isCompletedToday`
- timestamps

### Validation

| Property        | Rule                                            |
| --------------- | ----------------------------------------------- |
| `name`          | Required after trimming, maximum 100 characters |
| `description`   | Optional, maximum 500 characters                |
| `category`      | Required, supported enum value                  |
| `frequencyType` | Required, supported enum value                  |
| `targetCount`   | Required, integer from 1 through 7              |
| `difficulty`    | Required, supported enum value                  |

Cross-field target validation:

```text
daily  -> targetCount = 1
weekly -> targetCount between 1 and 7
```

### Backend behavior

The backend:

- derives ownership from claims,
- trims the name,
- trims the description,
- converts blank description to `null`,
- derives the current user-local date,
- validates the rule set,
- creates configuration version 1 effective on that local date,
- calculates the current reward mapping,
- stores the habit and related data.

### Response

`HabitResponse`

A new habit has no pending configuration.

### Status codes

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`

---

## 21. Update Habit

```text
PUT /api/habits/{habitId}
```

### Authentication

Required.

### Antiforgery

Required.

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

All fields represent the desired complete habit state rather than a partial patch.

### Immediate fields

Changes to these fields take effect immediately:

- `name`
- `description`

### Scheduled fields

Changes to these fields are scheduled for the next user-local week boundary:

- `category`
- `frequencyType`
- `targetCount`
- `difficulty`

The boundary is calculated from:

- the authenticated user’s local date,
- the stored `WeekStartsOn` setting.

### Response behavior

After scheduling a rule change:

- the top-level rule fields still show the currently effective values,
- `pendingConfiguration` shows the future values,
- `attributeRewards` still reflects the effective configuration,
- `updatedAtUtc` reflects the edit request.

Example:

```json
{
  "category": "fitnessAndMovement",
  "frequencyType": "weekly",
  "targetCount": 3,
  "difficulty": "hard",
  "pendingConfiguration": {
    "effectiveFromDate": "2026-07-27",
    "category": "fitnessAndMovement",
    "frequencyType": "weekly",
    "targetCount": 4,
    "difficulty": "elite"
  }
}
```

The actual response contains the complete `HabitResponse`.

### Repeated edits before the boundary

When another rule update occurs before the pending version becomes effective:

- the existing pending configuration is updated,
- another pending object is not added.

### Cancelling a pending rule change

When the requested rules are changed back to the currently effective values:

- the pending configuration is cancelled,
- `pendingConfiguration` is omitted from the response.

### Configuration history

The public response does not expose the full configuration-version history.

The API exposes only:

- the effective current rule set,
- the single pending rule set, when present.

### Status codes

- `200 OK`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`

Owned active and inactive habits may be updated.

Missing and foreign-owned habits return `404 Not Found`.

---

## 22. Deactivate Habit

```text
DELETE /api/habits/{habitId}
```

### Authentication

Required.

### Antiforgery

Required.

### Request

No body.

### Response

Full updated `HabitResponse`.

Relevant field:

```json
{
  "isActive": false
}
```

### Behavior

- The habit is soft-deactivated.
- Habit history remains stored.
- Configuration history remains stored.
- Completion history remains stored.
- XP history remains stored.
- Repeating deactivation for an already inactive owned habit succeeds.
- An idempotent repeat does not update `updatedAtUtc` again.
- The habit disappears from the active dashboard streak list.

### Status codes

- `200 OK`
- `401 Unauthorized`
- `404 Not Found`

Missing and foreign-owned habits return `404 Not Found`.

---

# Completions

## 23. Complete Habit

```text
POST /api/habits/{habitId}/completions
```

### Authentication

Required.

### Antiforgery

Required.

### Request

`CompleteHabitRequest`

```json
{
  "notes": "Completed after work."
}
```

Notes may be omitted or null:

```json
{
  "notes": null
}
```

The request must not contain:

- `userId`,
- `completedDate`,
- `completedAtUtc`,
- `habitConfigurationVersionId`,
- reward values,
- XP amounts.

### Backend behavior

The backend:

1. verifies ownership,
2. verifies that the habit is active,
3. loads the user’s stored time zone,
4. obtains the current UTC time,
5. calculates the current user-local date,
6. checks for an active completion on that date,
7. selects the habit configuration effective on that date,
8. normalizes notes,
9. calculates rewards from that effective category and difficulty,
10. creates the completion,
11. links it internally to the effective configuration version,
12. updates user attributes,
13. creates positive XP transactions,
14. saves the completion and rewards together.

### Active duplicate rule

The conflict check applies only to a non-undone completion.

This sequence is valid:

1. complete,
2. undo,
3. complete again on the same local date.

The original undone completion remains in history.

### Response

`CompleteHabitResponse`

```json
{
  "completion": {
    "id": "completion-id",
    "habitId": "habit-id",
    "completedDate": "2026-07-22",
    "completedAtUtc": "2026-07-22T19:42:15Z",
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

`rewards` contains the exact XP applied by the backend.

The response does not expose:

- the internal configuration-version identifier,
- XP transaction identifiers,
- `undoneAtUtc`.

### Status codes

- `201 Created`
- `400 Bad Request`
- `401 Unauthorized`
- `404 Not Found`
- `409 Conflict`

`404 Not Found`:

- no matching habit exists,
- the habit belongs to another user.

`409 Conflict`:

- the habit is inactive,
- an active completion already exists for the current local date.

---

## 24. Undo Today’s Completion

```text
DELETE /api/habits/{habitId}/completions/today
```

### Authentication

Required.

### Antiforgery

Required.

### Request

No body.

### Response

No body.

### Backend behavior

The backend:

1. verifies that the owned habit exists,
2. calculates the authenticated user’s current local date,
3. locates the active completion for that date,
4. loads the original positive XP transactions,
5. validates current attribute totals,
6. subtracts the original amounts from user attributes,
7. appends matching negative XP transactions,
8. sets the completion’s internal `UndoneAtUtc`,
9. saves the reversal together.

Undo does not delete:

- the completion,
- its original positive XP transactions,
- its configuration linkage.

The resulting XP ledger contains both the award and reversal.

Example internal effect:

```text
+14 Mind     Habit completion
+6 Focus     Habit completion
-14 Mind     Habit completion undo
-6 Focus     Habit completion undo
```

These transaction records are not currently returned by the endpoint.

### Habit state

Undo is permitted when the owned habit has been deactivated after completion.

### Repeat behavior

After a successful undo, no active completion remains for today.

Repeating the same undo request therefore returns `404 Not Found`.

### Status codes

- `204 No Content`
- `401 Unauthorized`
- `404 Not Found`

`404 Not Found` is returned when:

- the habit is missing,
- the habit belongs to another user,
- no active completion exists for the current local date.

---

# Attributes

## 25. Get User Attributes

```text
GET /api/attributes
```

### Authentication

Required.

### Antiforgery

Not required.

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

### Ordering

The response contains all eight attributes in stable enum order:

1. `discipline`
2. `fitness`
3. `vitality`
4. `focus`
5. `mind`
6. `resilience`
7. `social`
8. `purpose`

### Missing stored rows

A physical `UserAttribute` row is created only after the user first earns XP in that attribute.

When no row exists, the API still returns:

```json
{
  "attributeType": "discipline",
  "currentXp": 0,
  "level": 1,
  "xpIntoCurrentLevel": 0,
  "xpNeededForNextLevel": 100
}
```

### Backend-owned values

The backend calculates:

- `level`,
- `xpIntoCurrentLevel`,
- `xpNeededForNextLevel`.

The client may calculate a visual bar width from the returned values.

The client must not implement the authoritative level curve.

### Status codes

- `200 OK`
- `401 Unauthorized`

---

# Dashboard

## 26. Get Dashboard

```text
GET /api/dashboard
```

### Authentication

Required.

### Antiforgery

Not required.

### Response

`DashboardResponse`

```json
{
  "overallProgress": {
    "totalXp": 380,
    "level": 2,
    "xpIntoCurrentLevel": 180,
    "xpNeededForNextLevel": 250
  },
  "todayActivity": {
    "localDate": "2026-07-22",
    "completions": 2,
    "xpEarned": 40
  },
  "todayExecution": {
    "completedDailyHabits": 1,
    "totalDailyHabits": 3
  },
  "habitStreaks": [
    {
      "habitId": "daily-habit-id",
      "habitName": "Read C# textbook",
      "frequencyType": "daily",
      "currentStreak": 4,
      "longestStreak": 9
    },
    {
      "habitId": "weekly-habit-id",
      "habitName": "Go to gym",
      "frequencyType": "weekly",
      "currentStreak": 2,
      "longestStreak": 6
    }
  ]
}
```

### Status codes

- `200 OK`
- `401 Unauthorized`

---

## 27. `OverallProgressResponse`

```json
{
  "totalXp": 380,
  "level": 2,
  "xpIntoCurrentLevel": 180,
  "xpNeededForNextLevel": 250
}
```

### `totalXp`

`totalXp` is the signed sum of the authenticated user’s XP transactions.

Positive completion transactions increase the total.

Negative undo transactions reduce the total.

The backend does not expose or depend on a separate mutable total-XP field.

### Level values

The backend calculates:

- `level`,
- `xpIntoCurrentLevel`,
- `xpNeededForNextLevel`.

The client may visualize these values.

It must not reproduce the overall-level curve as authoritative logic.

---

## 28. `TodayActivityResponse`

```json
{
  "localDate": "2026-07-22",
  "completions": 2,
  "xpEarned": 40
}
```

### `localDate`

The backend derives `localDate` from:

- the current UTC time,
- the authenticated user’s stored IANA time zone.

### `completions`

`completions` counts non-undone completion records on `localDate`.

It includes daily and weekly habits.

It represents recorded activity rather than the number of currently active habits.

Deactivating a habit after completing it does not erase the completion from today’s activity.

### `xpEarned`

`xpEarned` sums transactions attached to the non-undone completions counted for the local date.

An undone completion is excluded.

A completion that was undone and then replaced by another completion contributes only through the current active completion.

---

## 29. `TodayExecutionResponse`

```json
{
  "completedDailyHabits": 1,
  "totalDailyHabits": 3
}
```

### `totalDailyHabits`

Counts active habits whose configuration effective on the current local date has:

```text
frequencyType = daily
```

Weekly habits are excluded.

### `completedDailyHabits`

Counts those active daily habits that have a non-undone completion on the current local date.

The client may calculate a display percentage from:

```text
completedDailyHabits / totalDailyHabits
```

The stored counts remain authoritative.

When `totalDailyHabits` is zero, the client must avoid division by zero.

---

## 30. `HabitStreakResponse`

```json
{
  "habitId": "habit-id",
  "habitName": "Read C# textbook",
  "frequencyType": "daily",
  "currentStreak": 4,
  "longestStreak": 9
}
```

Properties:

| Property        | Meaning                                           |
| --------------- | ------------------------------------------------- |
| `habitId`       | Active owned habit identifier                     |
| `habitName`     | Current habit name                                |
| `frequencyType` | Frequency effective on the current local date     |
| `currentStreak` | Current streak within the active frequency series |
| `longestStreak` | Longest streak within the active frequency series |

Daily values use days.

Weekly values use weeks.

Units are not included as a separate response property.

The frontend chooses presentational labels:

```text
1 day
2 days
1 week
3 weeks
```

### Collection scope

`habitStreaks` contains active owned habits only.

Inactive habits are omitted.

Historical data for inactive habits remains stored, but the current dashboard does not expose their historical longest streak.

### Ordering

Habit streaks are ordered by:

1. habit name,
2. habit identifier as a tie-breaker.

### Empty state

When the user has no active habits:

```json
{
  "habitStreaks": []
}
```

---

## 31. Daily Streak Contract

A daily date is successful when a non-undone completion exists for that local date.

Rules represented by `currentStreak` and `longestStreak`:

- Completing today counts immediately.
- An incomplete current day does not break the streak before the day ends.
- When today is incomplete, the current streak may continue through yesterday.
- A missed past date breaks the current streak.
- The longest streak is the longest consecutive run of successful eligible dates.
- Future completion dates are ignored.
- Undone completions are ignored.
- Dates outside the current contiguous daily-frequency segment are ignored.

Example:

```text
Monday     completed
Tuesday    completed
Wednesday  incomplete and still current
```

Dashboard result on Wednesday:

```json
{
  "frequencyType": "daily",
  "currentStreak": 2
}
```

---

## 32. Weekly Streak Contract

A weekly period is successful when its active completion count reaches the target effective during that period.

Rules:

- Week boundaries use the stored `WeekStartsOn` setting.
- Reaching the target before the week ends counts immediately.
- An incomplete current week does not break the streak before the period ends.
- A failed completed week breaks the current streak.
- Historical weeks use their historical target counts.
- A later target change does not reinterpret earlier weeks.
- An unsuccessful first partial week is treated as a grace period.
- A successful first partial week counts.
- Future completion dates are ignored.
- Undone completions are ignored.

Example:

```text
Historical target: 3
Current target:    5
```

Weeks that occurred under target 3 continue to require three completions.

The new target applies only from its effective configuration date.

---

## 33. Configuration Changes and Streaks

The following changes do not reset streak continuity:

- category,
- difficulty,
- weekly target.

A weekly target change preserves the streak series while each historical period continues to use its own target.

The following changes start a new streak series:

```text
daily -> weekly
weekly -> daily
```

Earlier completions remain stored but do not carry into the current frequency series.

The dashboard does not expose separate archived streak series.

---

## 34. Undo and Streaks

Because streaks are derived from completion history, undo affects them immediately.

An undone completion does not count toward:

- daily current streak,
- daily longest streak,
- weekly target progress,
- weekly current streak,
- weekly longest streak.

Undoing a historical completion may reduce `longestStreak`.

No separate streak mutation endpoint is required.

---

## 35. Dashboard Consumer Behavior

The dashboard endpoint is a read model.

Mutating endpoints do not push updated dashboard data to the client.

The frontend refreshes `GET /api/dashboard` after:

- habit creation,
- habit editing,
- habit deactivation,
- habit completion,
- completion undo.

This keeps:

- progression,
- today activity,
- daily execution,
- streak cards

synchronized with backend state.

---

## 36. Intentionally Absent Dashboard Fields

The current `DashboardResponse` does not include:

- display name,
- habit list,
- attribute list,
- recent completion activity,
- recent XP transaction activity,
- seven-day chart data,
- weekly XP summary,
- weekly completion percentage,
- combined cross-habit streak,
- rank,
- currency,
- notifications,
- quests,
- avatar state,
- achievement claims.

These fields must not be documented or consumed as implemented behavior.

Existing endpoints remain responsible for:

```text
Habits:     GET /api/habits
Attributes: GET /api/attributes
User:       GET /api/auth/me
```

---

# Deferred API Areas

## 37. Deferred Endpoints and Contracts

The following are outside the current API:

- user-settings update endpoints,
- advanced scheduling,
- completion-history browsing,
- XP transaction-history browsing,
- recent-activity feeds,
- weekly-review endpoints,
- historical streak browsing,
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
- payments,
- administration endpoints,
- calendar integration.

These require a separate product, data-model, architecture, and contract decision.

---

# Contract Maintenance

## 38. Contract Change Review

When an existing response changes, review together:

- backend response DTO,
- service mapping,
- controller behavior,
- backend unit tests,
- HTTP integration tests,
- frontend TypeScript type,
- frontend API function,
- frontend API tests,
- component fixtures,
- consuming components,
- this document.

A backend-only response change is incomplete when the frontend consumes the contract.

---

## 39. Contract Design Rules

1. Backend responses use purpose-built DTOs.
2. Database entities are not public API responses.
3. User identity comes from authenticated claims.
4. User-owned request bodies do not accept `userId`.
5. Enum values use stable camel-case strings.
6. Numeric enum JSON is rejected.
7. Date-sensitive behavior uses backend-calculated local dates.
8. Historical habit rules come from effective-dated configurations.
9. XP and level calculations remain backend-owned.
10. Undo preserves the original event.
11. Streak calculations remain backend-owned.
12. Aggregate page responses are used when they reduce unnecessary client orchestration.
13. Speculative fields are not added.
14. Expected application failures use deliberate status codes.
15. Contract changes require automated coverage.
16. Implemented and deferred contracts remain clearly separated.

---

# Endpoint Summary

## 40. Public Endpoints

### Health

```text
GET /api/health
```

### Authentication

```text
GET  /api/auth/csrf-token
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
GET  /api/auth/me
```

### Habits

```text
GET    /api/habits
GET    /api/habits/{habitId}
POST   /api/habits
PUT    /api/habits/{habitId}
DELETE /api/habits/{habitId}
```

### Completions

```text
POST   /api/habits/{habitId}/completions
DELETE /api/habits/{habitId}/completions/today
```

### Attributes

```text
GET /api/attributes
```

### Dashboard

```text
GET /api/dashboard
```

---

## 41. Authentication Matrix

| Endpoint                                         | Authentication | Antiforgery |
| ------------------------------------------------ | -------------: | ----------: |
| `GET /api/health`                                |             No |          No |
| `GET /api/auth/csrf-token`                       |             No |          No |
| `POST /api/auth/register`                        |             No |         Yes |
| `POST /api/auth/login`                           |             No |         Yes |
| `POST /api/auth/logout`                          |             No |         Yes |
| `GET /api/auth/me`                               |            Yes |          No |
| `GET /api/habits`                                |            Yes |          No |
| `GET /api/habits/{habitId}`                      |            Yes |          No |
| `POST /api/habits`                               |            Yes |         Yes |
| `PUT /api/habits/{habitId}`                      |            Yes |         Yes |
| `DELETE /api/habits/{habitId}`                   |            Yes |         Yes |
| `POST /api/habits/{habitId}/completions`         |            Yes |         Yes |
| `DELETE /api/habits/{habitId}/completions/today` |            Yes |         Yes |
| `GET /api/attributes`                            |            Yes |          No |
| `GET /api/dashboard`                             |            Yes |          No |

---

## 42. Summary

The current contract exposes four main application areas:

```text
Authentication establishes identity.

Habit endpoints manage effective and pending habit state.

Completion endpoints apply and reverse progression.

Dashboard and attribute endpoints expose calculated current state.
```

The backend remains authoritative for:

- ownership,
- local dates,
- calendar periods,
- effective habit configuration,
- XP,
- levels,
- completion state,
- undo history,
- dashboard aggregation,
- streaks.

The frontend sends actions and presents the resulting contract.
