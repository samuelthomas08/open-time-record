namespace otr_backend.Enums;

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

public enum PermissionLevel
{
    None,
    Read,
    Write,
    Admin
}
