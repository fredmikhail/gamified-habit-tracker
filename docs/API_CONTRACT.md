# Gamified Habit Tracker — API Contract

**Status:** Current through Phase 7

**Base path:** `/api`

**Authentication:** ASP.NET Core encrypted cookie

**State-changing security:** antiforgery token in `X-CSRF-TOKEN`

## Contract Rules

- JSON properties use camel case.
- Public enums use camel-case strings.
- Numeric enum values are rejected.
- GUID values are strings and must be treated as opaque.
- User-local dates use `YYYY-MM-DD`.
- UTC timestamps use ISO 8601 and property names ending in `Utc`.
- Optional text may be `null`.
- Collection properties return arrays rather than `null`.
- `pendingConfiguration` is omitted when no pending configuration exists.
- Database entities are not API response models.
- Returned progression, level, date, reward, streak, and summary values are authoritative.

## Browser Security

The browser uses an encrypted cookie named:

```text
HabitTracker.Auth
```

Properties:

- `HttpOnly`;
- `SameSite=Lax`;
- secure outside Development;
- no sliding expiration;
- 12-hour default lifetime;
- up to 30 days when `rememberMe` is true.

Frontend requests include browser credentials.

State-changing verbs require antiforgery validation:

- POST
- PUT
- PATCH
- DELETE

Token endpoint:

```text
GET /api/auth/csrf-token
```

Request header:

```text
X-CSRF-TOKEN
```

The client caches the request token in memory and clears it when authentication identity changes.

## Ownership

Protected operations derive `userId` from authenticated claims.

User-owned request bodies do not accept authoritative ownership identifiers.

A missing resource and another user’s resource generally return the same:

```text
404 Not Found
```

## Public Enums

### `HabitFrequencyType`

- `daily`
- `weekly`

Rules:

```text
daily  -> targetCount = 1
weekly -> 1 <= targetCount <= 7
```

### `HabitDifficulty`

- `easy`
- `medium`
- `hard`
- `elite`

### `AttributeType`

- `discipline`
- `fitness`
- `vitality`
- `focus`
- `mind`
- `resilience`
- `social`
- `purpose`

### `HabitCategory`

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

`WeekStartDay` is currently stored internally but is not exposed through a user-settings update endpoint.

## Endpoint Summary

| Method | Route                                     | Auth | Antiforgery | Success |
| ------ | ----------------------------------------- | ---- | ----------- | ------- |
| GET    | `/api/health`                             | No   | No          | `200`   |
| GET    | `/api/auth/csrf-token`                    | No   | No          | `200`   |
| POST   | `/api/auth/register`                      | No   | Yes         | `201`   |
| POST   | `/api/auth/login`                         | No   | Yes         | `200`   |
| POST   | `/api/auth/logout`                        | No   | Yes         | `204`   |
| GET    | `/api/auth/me`                            | Yes  | No          | `200`   |
| POST   | `/api/habits`                             | Yes  | Yes         | `201`   |
| GET    | `/api/habits`                             | Yes  | No          | `200`   |
| GET    | `/api/habits/{habitId}`                   | Yes  | No          | `200`   |
| PUT    | `/api/habits/{habitId}`                   | Yes  | Yes         | `200`   |
| POST   | `/api/habits/{habitId}/activate`          | Yes  | Yes         | `200`   |
| DELETE | `/api/habits/{habitId}`                   | Yes  | Yes         | `200`   |
| POST   | `/api/habits/{habitId}/completions`       | Yes  | Yes         | `201`   |
| DELETE | `/api/habits/{habitId}/completions/today` | Yes  | Yes         | `204`   |
| GET    | `/api/attributes`                         | Yes  | No          | `200`   |
| GET    | `/api/attributes/overview`                | Yes  | No          | `200`   |
| GET    | `/api/dashboard`                          | Yes  | No          | `200`   |

## Authentication DTOs

### `RegisterRequest`

```json
{
  "email": "fred@example.com",
  "username": "fred",
  "password": "a sufficiently long password",
  "timeZone": "America/Toronto"
}
```

Rules:

- email is required and normalized for uniqueness;
- username is required, 3–30 characters, letters/numbers/underscores;
- password is required and must satisfy the configured minimum length;
- time zone must be a valid accepted IANA identifier.

### `LoginRequest`

```json
{
  "email": "fred@example.com",
  "password": "a sufficiently long password",
  "rememberMe": false
}
```

### `CurrentUserResponse`

```json
{
  "id": "019ba123-4567-789a-bcde-f0123456789a",
  "email": "fred@example.com",
  "username": "fred",
  "displayName": "Fred",
  "timeZone": "America/Toronto"
}
```

### `AuthResponse`

```json
{
  "user": {
    "id": "019ba123-4567-789a-bcde-f0123456789a",
    "email": "fred@example.com",
    "username": "fred",
    "displayName": "Fred",
    "timeZone": "America/Toronto"
  }
}
```

### `AntiforgeryTokenResponse`

```json
{
  "requestToken": "token"
}
```

## Authentication Endpoints

### `GET /api/auth/csrf-token`

Returns an antiforgery request token and stores the related cookie.

Success:

```text
200 OK
```

### `POST /api/auth/register`

Creates `User` and `UserSettings`, signs in the new user with the default non-persistent session, and returns `AuthResponse`.

Success:

```text
201 Created
```

Expected failures:

- `400` invalid input or time zone;
- `409` email or username conflict.

### `POST /api/auth/login`

Verifies credentials, updates login state, creates the authentication cookie, and returns `AuthResponse`.

Success:

```text
200 OK
```

Expected failure:

```text
401 Unauthorized
```

### `POST /api/auth/logout`

Signs out the current cookie session.

Success:

```text
204 No Content
```

Logout is idempotent and may be called anonymously.

### `GET /api/auth/me`

Returns the current user.

Success:

```text
200 OK
```

Missing or invalid session:

```text
401 Unauthorized
```

## Habit DTOs

### `CreateHabitRequest` and `UpdateHabitRequest`

```json
{
  "name": "Read C#",
  "description": "Read one chapter.",
  "category": "learningAndSkills",
  "frequencyType": "daily",
  "targetCount": 1,
  "difficulty": "medium"
}
```

Rules:

- name: required, trimmed, maximum 100;
- description: optional, trimmed, maximum 500, blank becomes `null`;
- category: required controlled value;
- frequency: required controlled value;
- target count: daily 1, weekly 1–7;
- difficulty: required controlled value.

### `PendingHabitConfigurationResponse`

```json
{
  "effectiveFromDate": "2026-07-27",
  "category": "learningAndSkills",
  "frequencyType": "weekly",
  "targetCount": 3,
  "difficulty": "hard"
}
```

### `HabitAttributeRewardResponse`

```json
{
  "attributeType": "mind",
  "xpAmount": 14
}
```

### `HabitResponse`

```json
{
  "id": "019ba123-4567-789a-bcde-f0123456789a",
  "name": "Read C#",
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
  "createdAtUtc": "2026-07-23T12:00:00Z",
  "updatedAtUtc": "2026-07-23T12:00:00Z"
}
```

When a future rule change exists, the response also includes:

```json
{
  "pendingConfiguration": {
    "effectiveFromDate": "2026-07-27",
    "category": "learningAndSkills",
    "frequencyType": "weekly",
    "targetCount": 3,
    "difficulty": "hard"
  }
}
```

## Habit Endpoints

### `POST /api/habits`

Creates:

- the habit;
- version 1 of its configuration;
- current reward mappings.

Success:

```text
201 Created
```

Body: `HabitResponse`.

### `GET /api/habits`

Query:

```text
includeInactive=false
```

Default behavior returns active habits only.

`includeInactive=true` returns active and inactive habits.

Service ordering:

1. active before inactive;
2. newest creation first;
3. identifier as final deterministic tie-breaker.

Success:

```text
200 OK
```

Body: `HabitResponse[]`.

### `GET /api/habits/{habitId}`

Success:

```text
200 OK
```

Missing or foreign-owned:

```text
404 Not Found
```

### `PUT /api/habits/{habitId}`

Name and description update immediately.

Category, frequency, target count, and difficulty are compared with the current effective configuration.

When rules differ:

- a configuration is scheduled for the next user-local week boundary;
- the response includes `pendingConfiguration`.

Editing the pending rules updates the same pending version.

Returning requested rules to current effective rules removes the pending version.

Success:

```text
200 OK
```

### `POST /api/habits/{habitId}/activate`

Sets an owned inactive habit to active.

Calling it for an already active habit is idempotent.

History is preserved.

Success:

```text
200 OK
```

### `DELETE /api/habits/{habitId}`

Soft-deactivates an owned habit.

The habit and its history remain stored.

Calling it for an already inactive habit is idempotent.

Success:

```text
200 OK
```

## Completion DTOs

### `CompleteHabitRequest`

```json
{
  "notes": null
}
```

The client does not submit a completion date.

### `HabitCompletionResponse`

```json
{
  "id": "019ba123-4567-789a-bcde-f0123456789a",
  "habitId": "019ba123-4567-789a-bcde-f0123456789b",
  "completedDate": "2026-07-23",
  "completedAtUtc": "2026-07-23T14:30:00Z",
  "notes": null
}
```

### `CompleteHabitResponse`

```json
{
  "completion": {
    "id": "019ba123-4567-789a-bcde-f0123456789a",
    "habitId": "019ba123-4567-789a-bcde-f0123456789b",
    "completedDate": "2026-07-23",
    "completedAtUtc": "2026-07-23T14:30:00Z",
    "notes": null
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

## Completion Endpoints

### `POST /api/habits/{habitId}/completions`

The backend:

- verifies ownership;
- verifies active state;
- calculates the user-local date;
- selects the effective configuration;
- prevents another active completion for that date;
- creates reward transactions and attribute updates atomically.

Success:

```text
201 Created
```

Expected failures:

- `404` missing or foreign-owned habit;
- `409` inactive habit;
- `409` already completed for the current local date.

### `DELETE /api/habits/{habitId}/completions/today`

The backend:

- finds the active completion for the current user-local date;
- records the undo timestamp;
- appends negative XP transactions;
- reverses current attribute XP.

Success:

```text
204 No Content
```

No active completion or no matching owned habit:

```text
404 Not Found
```

## Attribute DTOs

### `UserAttributeResponse`

```json
{
  "attributeType": "mind",
  "currentXp": 240,
  "level": 2,
  "xpIntoCurrentLevel": 40,
  "xpNeededForNextLevel": 200
}
```

The API returns all eight attributes, including zero-value attributes.

### `AttributeLevelUpResponse`

```json
{
  "attributeType": "focus",
  "currentLevel": 2,
  "xpRemaining": 25
}
```

### `XpTransactionResponse`

```json
{
  "id": "019ba123-4567-789a-bcde-f0123456789a",
  "habitName": "Read C#",
  "attributeType": "mind",
  "amount": 14,
  "reason": "Habit completion",
  "createdAtUtc": "2026-07-23T14:30:00Z"
}
```

### `AttributeOverviewResponse`

```json
{
  "attributes": [
    {
      "attributeType": "discipline",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    },
    {
      "attributeType": "fitness",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    },
    {
      "attributeType": "vitality",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    },
    {
      "attributeType": "focus",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    },
    {
      "attributeType": "mind",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    },
    {
      "attributeType": "resilience",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    },
    {
      "attributeType": "social",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    },
    {
      "attributeType": "purpose",
      "currentXp": 0,
      "level": 1,
      "xpIntoCurrentLevel": 0,
      "xpNeededForNextLevel": 100
    }
  ],
  "totalAttributeXp": 0,
  "balanceScore": 0,
  "strongestAttribute": null,
  "needsFocusAttribute": null,
  "closestToLevelUp": [
    {
      "attributeType": "discipline",
      "currentLevel": 1,
      "xpRemaining": 100
    },
    {
      "attributeType": "fitness",
      "currentLevel": 1,
      "xpRemaining": 100
    },
    {
      "attributeType": "vitality",
      "currentLevel": 1,
      "xpRemaining": 100
    }
  ],
  "recentXpTransactions": []
}
```

`attributes` contains all eight `UserAttributeResponse` values.

`strongestAttribute` and `needsFocusAttribute` are nullable.

The closest queue contains at most three items.

Recent XP contains at most six transactions, newest first with identifier tie-breaking.

## Attribute Endpoints

### `GET /api/attributes`

Returns:

```text
UserAttributeResponse[]
```

Success:

```text
200 OK
```

### `GET /api/attributes/overview`

Returns the backend-calculated attribute command-center read model.

The backend owns:

- total XP;
- balance score;
- strongest and needs-focus selection;
- closest-to-level-up ordering;
- recent transaction selection.

Success:

```text
200 OK
```

## Dashboard DTOs

### `DashboardResponse`

```json
{
  "overallProgress": {
    "totalXp": 300,
    "level": 2,
    "xpIntoCurrentLevel": 100,
    "xpNeededForNextLevel": 250
  },
  "todayActivity": {
    "localDate": "2026-07-23",
    "completions": 2,
    "xpEarned": 40
  },
  "todayExecution": {
    "completedDailyHabits": 2,
    "totalDailyHabits": 4
  },
  "todayHabits": [],
  "attributes": [],
  "habitStreaks": []
}
```

### `DashboardHabitResponse`

```json
{
  "id": "019ba123-4567-789a-bcde-f0123456789a",
  "name": "Read C#",
  "category": "learningAndSkills",
  "frequencyType": "daily",
  "targetCount": 1,
  "difficulty": "medium",
  "attributeRewards": [],
  "isCompletedToday": false,
  "currentStreak": 2,
  "longestStreak": 5
}
```

### `HabitStreakResponse`

Contains:

- `habitId`;
- `habitName`;
- `frequencyType`;
- `currentStreak`;
- `longestStreak`.

## Dashboard Endpoint

### `GET /api/dashboard`

Returns one authenticated aggregate.

`DashboardService` owns aggregation.

`StreakService` owns streak values.

Inactive habits are excluded from active dashboard habit and streak responses.

Success:

```text
200 OK
```

React may sort or paginate the returned display collection, but it does not recalculate XP, level, completion eligibility, or streak continuity.

## Status Codes

| Status | Meaning                           |
| -----: | --------------------------------- |
|  `200` | Success with body                 |
|  `201` | Resource or completion created    |
|  `204` | Success without body              |
|  `400` | Invalid request or domain input   |
|  `401` | Missing or invalid authentication |
|  `403` | Authenticated but forbidden       |
|  `404` | No matching owned resource        |
|  `409` | Conflict with current state       |
|  `500` | Unexpected server failure         |

## Error Responses

Expected application failures are normally mapped to RFC 7807 Problem Details.

Representative response:

```json
{
  "title": "Habit already completed",
  "status": 409,
  "detail": "This habit has already been completed for today.",
  "instance": "/api/habits/019ba123-4567-789a-bcde-f0123456789a/completions"
}
```

Request-model validation returns validation Problem Details.

Direct controller results such as `NotFound()` or `Unauthorized()` may return only a status code.

Unexpected exceptions return a generic server response. Internal exception details are not part of the public contract.

## Authoritative Refresh Semantics

After a successful mutation, the frontend updates immediate interaction state where appropriate and refetches affected authoritative resources.

Progression animations react only to differences between consecutive API responses.

The client must not predict:

- awarded XP;
- level changes;
- streak changes;
- effective configuration;
- balance score;
- closest-to-level ordering.
