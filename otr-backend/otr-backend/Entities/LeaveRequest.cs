using otr_backend.Enums;

namespace otr_backend.Models;

/// <summary>A request for time off that has to be approved or rejected by a reviewer.</summary>
public class LeaveRequest
{
    public uint Id { get; set; }
    public uint RequestedByUserId { get; set; }
    public uint LeaveTypeId { get; set; }

    /// <summary>First day of leave (inclusive).</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Last day of leave (inclusive).</summary>
    public DateOnly EndDate { get; set; }

    public ApprovalStatus Status { get; set; }
    public uint? ReviewedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public User RequestedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
    public LeaveType LeaveType { get; set; } = null!;
}
