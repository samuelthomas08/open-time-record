# API reference

The backend exposes a REST API under `/api`. In development an interactive documentation (Scalar) is available at `http://localhost:5005/scalar` and the raw OpenAPI document at `http://localhost:5005/openapi/v1.json`.

Unless stated otherwise every endpoint requires a valid JWT (see [Authentication and permissions](authentication-and-permissions.md)). Request and response bodies are JSON. Error responses are plain text messages (German, matching the user interface) with the usual status codes: `400` validation, `401` not logged in, `403` not allowed, `404` not found, `409` conflict.

**Legend** – *Auth*: **public** (no login), **user** (any logged-in user), **owner** (only the owner of the record), **Superadmin**, **perm** (permission system, see the table in the permissions document).

- [Auth](#auth)
- [Time tracking](#time-tracking)
- [Users](#users)
- [Invitations](#invitations)
- [Roles and teams](#roles-and-teams)
- [Projects](#projects)
- [Work schedules](#work-schedules)
- [Settings](#settings)
- [Scaffolded resources](#scaffolded-resources)

## Auth

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| POST | `/api/Auth/register` | public | Register. The first user becomes Superadmin; an invitation code skips verification. |
| POST | `/api/Auth/activate` | public | Confirm the e-mail address with the emailed code and receive a JWT. |
| POST | `/api/Auth/resend-activation` | public | Send a new verification code. |
| POST | `/api/Auth/login` | public | Log in and receive a JWT. |

## Time tracking

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/TimeEntry` | user | All entries. |
| GET | `/api/TimeEntry/mine` | user | Own entries with breaks, newest first. |
| GET | `/api/TimeEntry/team` | user | Entries of managed users (everyone else for Superadmins). |
| GET | `/api/TimeEntry/{id}` | user | One entry with its breaks. |
| POST | `/api/TimeEntry` | user | Create an entry for the caller from supplied values. |
| POST | `/api/TimeEntry/start` | user | Start the timer. `409` if one is already running. |
| POST | `/api/TimeEntry/{id}/stop` | owner | Stop the timer; an open break is ended too. |
| POST | `/api/TimeEntry/{id}/breaks` | owner | Start a break (`reason` mandatory if configured). |
| PUT | `/api/TimeEntry/{id}/breaks/{breakId}/stop` | owner | End a break. |
| PUT | `/api/TimeEntry/{id}` | owner | Change project, task and description. |
| DELETE | `/api/TimeEntry/{id}` | owner | Delete an entry. |
| GET | `/api/TimeEntryCorrectionRequest` | user | All correction requests. |
| GET | `/api/TimeEntryCorrectionRequest/mine` | user | Own requests. |
| GET | `/api/TimeEntryCorrectionRequest/pending` | Superadmin | Requests awaiting review. |
| GET | `/api/TimeEntryCorrectionRequest/{id}` | user | One request. |
| POST | `/api/TimeEntryCorrectionRequest` | owner | File a request for an own entry. |
| PUT | `/api/TimeEntryCorrectionRequest/{id}/approve` | Superadmin | Approve and apply the proposed times. |
| PUT | `/api/TimeEntryCorrectionRequest/{id}/reject` | Superadmin | Reject, optionally with a note. |
| GET | `/api/TimeEntryAuditLog`, `/{id}` | user | Read-only audit log. |

## Users

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/User` | user | All users. |
| POST | `/api/User` | perm `Users:Admin` | Create a verified account. |
| GET | `/api/User/{id}` | user | One user. |
| GET | `/api/User/{id}/enriched` | user | A user with all related records. |
| PUT | `/api/User/{id}` | self, or perm `Users:Write` | Update name and e-mail. Only with `Users:Write` can the active flag be changed. |
| GET | `/api/User/managed` | user | Users the caller manages. |
| GET | `/api/User/me/permissions` | user | Effective permission level per resource. |
| GET | `/api/User/{id}/roles` | user | Roles of a user. |
| POST / DELETE | `/api/User/{id}/roles/{roleId}` | Superadmin | Assign / remove a role. |
| GET | `/api/User/{id}/teams` | user | Teams of a user. |
| POST / DELETE | `/api/User/{id}/teams/{teamId}` | Superadmin | Add to / remove from a team. |
| POST | `/api/User/{id}/profile-picture` | owner + perm `ProfilePicture:Write` | Upload a picture (multipart, PNG/JPEG/GIF/WEBP, max. 2 MB). |
| DELETE | `/api/User/{id}/profile-picture` | owner + perm `ProfilePicture:Write` | Remove the picture. |

Uploaded pictures are served from `/profile-pictures/{id}.{ext}`.

## Invitations

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/UserInvitation` | perm `Invitations:Read` | Open (not accepted, not archived) invitations. |
| POST | `/api/UserInvitation` | perm `Invitations:Write` | Create an invitation, valid for 7 days. With `sendEmail = false` the code is returned instead of mailed. |
| POST | `/api/UserInvitation/{id}/archive` | perm `Invitations:Write` | Hide an invitation from the list. |

## Roles and teams

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/Role`, `/api/Role/{id}`, `/api/Role/by-name/{roleName}` | user | Read roles. |
| POST / PUT | `/api/Role`, `/api/Role/{id}` | Superadmin | Create / update a role. |
| GET | `/api/Role/{id}/permissions` | user | Permissions granted by a role. |
| PUT | `/api/Role/{id}/permissions` | Superadmin | Replace all permissions of a role. |
| GET | `/api/Team`, `/api/Team/{id}`, `/api/Team/by-name/{teamName}` | user | Read teams. |
| POST / PUT | `/api/Team`, `/api/Team/{id}` | Superadmin | Create / update a team. |

## Projects

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/Project`, `/api/Project/{id}` | user | Read projects. |
| POST / PUT | `/api/Project`, `/api/Project/{id}` | Superadmin | Create / update a project. |

## Work schedules

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/WorkSchedule/mine` | user | Own schedule periods. |
| GET | `/api/WorkSchedule/managed` | user | Periods of managed users. |
| POST | `/api/WorkSchedule` | manager of the user, Superadmin, or the user if nobody manages them | Create a period. Each weekday must be between 0 and 24 hours. |
| PUT / DELETE | `/api/WorkSchedule/{id}` | same as above | Update / delete a period. |

## Settings

| Method | Path | Auth | Description |
| --- | --- | --- | --- |
| GET | `/api/AppSettings` | user | Feature toggles. |
| PUT | `/api/AppSettings` | Superadmin | Update the toggles. |
| GET / PUT | `/api/SmtpSettings` | Superadmin | Read / update the mail server. The password is never returned. |

## Scaffolded resources

These controllers only offer list, get-by-id and create. They are placeholders for upcoming features (see [Data model](data-model.md#planned-features)).

`/api/Client`, `/api/ProjectTask`, `/api/Tag`, `/api/Rule`, `/api/LeaveType`, `/api/LeaveRequest`, `/api/LeaveBalance`, `/api/PublicHoliday`, `/api/Notification`
