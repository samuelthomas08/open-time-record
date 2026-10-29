# Data model

OTR stores its data in PostgreSQL through Entity Framework Core. The entities live in `otr-backend/otr-backend/Entities`; the schema is defined by the migrations in `Migrations/` and applied automatically on start-up.

All primary keys are unsigned integers (`uint`). Timestamps are stored in UTC.

- [Overview](#overview)
- [Users, roles and teams](#users-roles-and-teams)
- [Time tracking](#time-tracking)
- [Onboarding and configuration](#onboarding-and-configuration)
- [Planned features](#planned-features)
- [Delete behavior](#delete-behavior)

## Overview

```mermaid
erDiagram
    USER ||--o{ USER_ROLE_MAPPING : has
    ROLE ||--o{ USER_ROLE_MAPPING : "assigned via"
    ROLE ||--o{ ROLE_PERMISSION_MAPPING : grants
    USER ||--o{ USER_TEAM_MAPPING : "member of"
    TEAM ||--o{ USER_TEAM_MAPPING : contains
    USER ||--o{ TEAM : manages
    USER ||--o{ USER : "manages (ManagerId)"
    USER ||--o{ TIME_ENTRY : tracks
    PROJECT ||--o{ TIME_ENTRY : "booked on"
    PROJECT_TASK ||--o{ TIME_ENTRY : "booked on"
    PROJECT ||--o{ PROJECT_TASK : contains
    CLIENT ||--o{ PROJECT : owns
    TIME_ENTRY ||--o{ TIME_ENTRY_BREAK : "interrupted by"
    TIME_ENTRY ||--o{ TIME_ENTRY_CORRECTION_REQUEST : "corrected by"
    USER ||--o{ TIME_ENTRY_CORRECTION_REQUEST : "requests / reviews"
```

## Users, roles and teams

```mermaid
erDiagram
    USER {
        uint Id PK
        string FirstName
        string LastName
        string Email
        string HashedPassword "never serialized"
        bool IsActive
        bool IsEmailVerified
        string ProfilePictureUrl "nullable"
        uint ManagerId FK "nullable, self reference"
        datetime CreatedAt
        datetime LastLogin
    }
    ROLE {
        uint Id PK
        string RoleName "unique"
        string RoleDisplayName
        bool IsActive
        datetime CreatedAt
    }
    ROLE_PERMISSION_MAPPING {
        uint RoleId PK, FK
        PermissionResource Resource PK
        PermissionLevel Level
    }
    USER_ROLE_MAPPING {
        uint UserId PK, FK
        uint RoleId PK, FK
    }
    TEAM {
        uint Id PK
        string TeamName
        string TeamDisplayName
        uint ManagerId "the managing user"
        bool IsActive
        datetime CreatedAt
    }
    USER_TEAM_MAPPING {
        uint UserId PK, FK
        uint TeamId PK, FK
    }

    USER ||--o{ USER_ROLE_MAPPING : ""
    ROLE ||--o{ USER_ROLE_MAPPING : ""
    ROLE ||--o{ ROLE_PERMISSION_MAPPING : ""
    USER ||--o{ USER_TEAM_MAPPING : ""
    TEAM ||--o{ USER_TEAM_MAPPING : ""
```

- A user can have several roles and belong to several teams (many-to-many through the mapping tables).
- `User.ManagerId` points at the user's direct manager. Together with `Team.ManagerId` it determines who a manager is responsible for; see `ManagedUsersService`.
- `ROLE_PERMISSION_MAPPING` has one row per role and resource. A missing row means no access.

## Time tracking

```mermaid
erDiagram
    TIME_ENTRY {
        uint Id PK
        uint UserId FK
        uint ProjectId FK "nullable"
        uint ProjectTaskId FK "nullable"
        datetime StartTime
        datetime EndTime "null while running"
        TimeEntryStatus Status "Running, Completed, PendingCorrection"
        string Description "nullable"
    }
    TIME_ENTRY_BREAK {
        uint Id PK
        uint TimeEntryId FK
        datetime StartTime
        datetime EndTime "null while running"
        string Reason "nullable"
    }
    TIME_ENTRY_CORRECTION_REQUEST {
        uint Id PK
        uint TimeEntryId FK
        uint RequestedByUserId FK
        uint ReviewedByUserId FK "nullable"
        ApprovalStatus Status "Pending, Approved, Rejected"
        string Reason
        string ReviewNote "nullable"
        datetime ProposedStartTime "nullable"
        datetime ProposedEndTime "nullable"
    }
    PROJECT {
        uint Id PK
        string ProjectName
        string ProjectDisplayName
        uint ClientId FK "nullable"
        bool IsActive
    }

    TIME_ENTRY ||--o{ TIME_ENTRY_BREAK : ""
    TIME_ENTRY ||--o{ TIME_ENTRY_CORRECTION_REQUEST : ""
    PROJECT ||--o{ TIME_ENTRY : ""
```

**Net worked time** of an entry is `EndTime - StartTime` minus the duration of all completed breaks.

### Life cycle of a time entry

```mermaid
stateDiagram-v2
    [*] --> Running: POST /api/TimeEntry/start
    Running --> Running: start / stop a break
    Running --> Completed: POST /api/TimeEntry/{id}/stop<br/>(an open break is closed too)
    Completed --> [*]
```

Only one entry per user may be in the `Running` state, and only one break per entry may be open. The status `PendingCorrection` exists in the enum but is not set by the current code; pending corrections are tracked on the correction request instead.

### Life cycle of a correction request

```mermaid
stateDiagram-v2
    [*] --> Pending: user files request<br/>(only one pending per entry)
    Pending --> Approved: Superadmin approves<br/>proposed times are applied to the entry
    Pending --> Rejected: Superadmin rejects<br/>entry stays unchanged
    Approved --> [*]
    Rejected --> [*]
```

## Onboarding and configuration

| Entity | Purpose |
| --- | --- |
| `EmailVerificationCode` | Six-digit code sent after self-registration, valid for 24 hours. |
| `UserInvitation` | Invitation for an e-mail address with a preassigned role, optional team and manager, valid for 7 days. |
| `SmtpSettings` | Single row with the mail server configuration. The password is stored encrypted. |
| `AppSettings` | Single row with the feature toggles (profile pictures, mandatory break reason). |
| `WorkSchedule` | Target hours per weekday for a user, valid from/to a date. Used for the target line in the reports. |

## Planned features

The following entities and API endpoints exist as scaffolding, but no user interface or business logic uses them yet:

`Client`, `ProjectTask`, `Tag` / `TimeEntryTagMapping`, `ProjectMember`, `Rule`, `LeaveType`, `LeaveRequest`, `LeaveBalance`, `PublicHoliday`, `Notification`, `RefreshToken`, `PasswordResetToken` and `TimeEntryAuditLog`.

Their permission resources are already part of the permission model so that roles can be prepared in advance.

## Delete behavior

Foreign keys that point at a user use `DeleteBehavior.Restrict`. A user with time entries, audit log entries, correction requests, leave requests or direct reports therefore cannot be deleted by accident. Deactivate the account instead (`User.IsActive = false`).
