namespace otr_backend.Dtos;

public class WorkScheduleUpsertRequest
{
    public uint UserId { get; set; }
    public decimal MondayHours { get; set; }
    public decimal TuesdayHours { get; set; }
    public decimal WednesdayHours { get; set; }
    public decimal ThursdayHours { get; set; }
    public decimal FridayHours { get; set; }
    public decimal SaturdayHours { get; set; }
    public decimal SundayHours { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}
