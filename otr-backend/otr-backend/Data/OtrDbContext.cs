using Microsoft.EntityFrameworkCore;
using otr_backend.Models;

namespace otr_backend.Data;

public class OtrDbContext : DbContext
{
    public OtrDbContext(DbContextOptions<OtrDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite keys for pure join/mapping tables
        modelBuilder.Entity<RolePermissionMapping>()
            .HasKey(rpm => new { rpm.RoleId, rpm.Resource });

        modelBuilder.Entity<UserRoleMapping>()
            .HasKey(urm => new { urm.UserId, urm.RoleId });

        modelBuilder.Entity<UserTeamMapping>()
            .HasKey(utm => new { utm.UserId, utm.TeamId });

        modelBuilder.Entity<ProjectMember>()
            .HasKey(pm => new { pm.ProjectId, pm.UserId });

        modelBuilder.Entity<TimeEntryTagMapping>()
            .HasKey(tetm => new { tetm.TimeEntryId, tetm.TagId });

        // TimeEntryCorrectionRequest has two FKs to User (requester/reviewer) -
        // EF can't infer which nav property pairs with which FK by convention alone.
        modelBuilder.Entity<TimeEntryCorrectionRequest>()
            .HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeEntryCorrectionRequest>()
            .HasOne(r => r.ReviewedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Same situation for LeaveRequest
        modelBuilder.Entity<LeaveRequest>()
            .HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LeaveRequest>()
            .HasOne(r => r.ReviewedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deleting a user must not silently cascade-delete their historical time-tracking records
        modelBuilder.Entity<TimeEntry>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeEntryAuditLog>()
            .HasOne(l => l.ChangedByUser)
            .WithMany()
            .HasForeignKey(l => l.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing, no nav property on either side — deleting a manager must not
        // cascade-delete their reports.
        modelBuilder.Entity<User>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(u => u.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Rule> Rules { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }
    public DbSet<RolePermissionMapping> RolePermissionMappings { get; set; }
    public DbSet<UserRoleMapping> UserRoleMappings { get; set; }
    public DbSet<UserTeamMapping> UserTeamMappings { get; set; }

    public DbSet<TimeEntryAuditLog> TimeEntryAuditLogs { get; set; }
    public DbSet<TimeEntryCorrectionRequest> TimeEntryCorrectionRequests { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<WorkSchedule> WorkSchedules { get; set; }

    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TimeEntryTagMapping> TimeEntryTagMappings { get; set; }
    public DbSet<LeaveType> LeaveTypes { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<PublicHoliday> PublicHolidays { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public DbSet<SmtpSettings> SmtpSettings { get; set; }
    public DbSet<AppSettings> AppSettings { get; set; }
    public DbSet<TimeEntryBreak> TimeEntryBreaks { get; set; }
    public DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    public DbSet<UserInvitation> UserInvitations { get; set; }
}
