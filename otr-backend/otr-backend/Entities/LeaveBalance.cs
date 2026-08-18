namespace otr_backend.Models;

public class LeaveBalance
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public uint LeaveTypeId { get; set; }
    public int Year { get; set; }
    public decimal AllocatedDays { get; set; }
    public decimal UsedDays { get; set; }

    public User User { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
