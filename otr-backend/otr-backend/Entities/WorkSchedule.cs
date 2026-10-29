namespace otr_backend.Models;

/// <summary>The target working hours per weekday of a user, valid for a period of time.</summary>
public class WorkSchedule
{
    public uint Id { get; set; }
    public uint UserId { get; set; }

    public decimal MondayHours { get; set; }
    public decimal TuesdayHours { get; set; }
    public decimal WednesdayHours { get; set; }
    public decimal ThursdayHours { get; set; }
    public decimal FridayHours { get; set; }
    public decimal SaturdayHours { get; set; }
    public decimal SundayHours { get; set; }

    /// <summary>First day the schedule is in effect.</summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>Last day the schedule is in effect; <c>null</c> means open-ended.</summary>
    public DateTime? EffectiveTo { get; set; }

    public User User { get; set; } = null!;
}
