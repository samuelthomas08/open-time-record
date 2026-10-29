using otr_backend.Models;

namespace otr_backend.Dtos;

/// <summary>A user together with everything that references them, for the detail view.</summary>
public class EnrichedUserDto
{
    public uint Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }

    // n:n via mapping tables
    public List<Role> Roles { get; set; } = [];
    public List<Team> Teams { get; set; } = [];
    public List<Project> Projects { get; set; } = [];

    /// <summary>Reverse n:1 - teams whose <c>ManagerId</c> points at this user.</summary>
    public List<Team> ManagedTeams { get; set; } = [];

    // 1:n - this user is the foreign key target
    public List<TimeEntry> TimeEntries { get; set; } = [];
    public List<WorkSchedule> WorkSchedules { get; set; } = [];
    public List<LeaveBalance> LeaveBalances { get; set; } = [];
    public List<Notification> Notifications { get; set; } = [];
    public List<TimeEntryAuditLog> AuditLogsMade { get; set; } = [];
    public List<TimeEntryCorrectionRequest> CorrectionRequestsSubmitted { get; set; } = [];
    public List<TimeEntryCorrectionRequest> CorrectionRequestsReviewed { get; set; } = [];
    public List<LeaveRequest> LeaveRequestsSubmitted { get; set; } = [];
    public List<LeaveRequest> LeaveRequestsReviewed { get; set; } = [];
    public List<UserInvitation> InvitationsSent { get; set; } = [];
}
