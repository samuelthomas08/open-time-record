namespace otr_backend.Models;

/// <summary>A user's yearly leave entitlement for one leave type.</summary>
public class LeaveBalance
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public uint LeaveTypeId { get; set; }

    /// <summary>Calendar year the balance applies to.</summary>
    public int Year { get; set; }

    /// <summary>Days granted for the year.</summary>
    public decimal AllocatedDays { get; set; }

    /// <summary>Days already taken in the year.</summary>
    public decimal UsedDays { get; set; }

    public User User { get; set; } = null!;
    public LeaveType LeaveType { get; set; } = null!;
}
