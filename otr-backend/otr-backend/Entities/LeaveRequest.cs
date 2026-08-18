using otr_backend.Enums;

namespace otr_backend.Models;

public class LeaveRequest
{
    public uint Id { get; set; }
    public uint RequestedByUserId { get; set; }
    public uint LeaveTypeId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public ApprovalStatus Status { get; set; }
    public uint? ReviewedByUserId { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public User RequestedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
    public LeaveType LeaveType { get; set; } = null!;
}
