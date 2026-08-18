using otr_backend.Models;

namespace otr_backend.Dtos;

public class EnrichedUserDto
{
    public uint Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLogin { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }

    // n:n via mapping tables
    public List<Role> Roles { get; set; } = new();
    public List<Team> Teams { get; set; } = new();
    public List<Project> Projects { get; set; } = new();

    // Reverse n:1 (Team.ManagerId points at this user)
    public List<Team> ManagedTeams { get; set; } = new();

    // 1:n, this user is the FK target
    public List<TimeEntry> TimeEntries { get; set; } = new();
    public List<WorkSchedule> WorkSchedules { get; set; } = new();
    public List<LeaveBalance> LeaveBalances { get; set; } = new();
    public List<Notification> Notifications { get; set; } = new();
    public List<TimeEntryAuditLog> AuditLogsMade { get; set; } = new();
    public List<TimeEntryCorrectionRequest> CorrectionRequestsSubmitted { get; set; } = new();
    public List<TimeEntryCorrectionRequest> CorrectionRequestsReviewed { get; set; } = new();
    public List<LeaveRequest> LeaveRequestsSubmitted { get; set; } = new();
    public List<LeaveRequest> LeaveRequestsReviewed { get; set; } = new();
    public List<UserInvitation> InvitationsSent { get; set; } = new();
}
