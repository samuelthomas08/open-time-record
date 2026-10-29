namespace otr_backend.Enums;

/// <summary>
/// A kind of data or feature that access can be granted on. Every new value is automatically
/// granted to the Superadmin role on the next application start (see <c>Program.cs</c>).
/// </summary>
public enum PermissionResource
{
    TimeEntries,
    Projects,
    Users,
    Rules,
    Roles,
    Teams,
    CorrectionRequests,
    Invitations,
    SmtpSettings,
    AppSettings,
    Clients,
    ProjectTasks,
    Tags,
    WorkSchedules,
    LeaveTypes,
    LeaveRequests,
    LeaveBalances,
    PublicHolidays,
    Notifications,
    AuditLogs,
    ProfilePicture,
}

/// <summary>
/// How much a role may do with a <see cref="PermissionResource"/>. Levels are ordered:
/// a higher level includes everything a lower level allows.
/// </summary>
public enum PermissionLevel
{
    /// <summary>No access.</summary>
    None,

    /// <summary>View only.</summary>
    Read,

    /// <summary>View and modify.</summary>
    Write,

    /// <summary>Full control, including destructive and administrative actions.</summary>
    Admin
}
