namespace otr_backend.Models;

public class WorkSchedule
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public decimal WeeklyHours { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public User User { get; set; } = null!;
}
