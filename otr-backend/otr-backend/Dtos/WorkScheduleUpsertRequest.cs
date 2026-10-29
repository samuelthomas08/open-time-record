namespace otr_backend.Dtos;

/// <summary>Request body for creating or replacing a work schedule period.</summary>
public class WorkScheduleUpsertRequest
{
    public uint UserId { get; set; }

    /// <summary>Target hours per weekday; each value must be between 0 and 24.</summary>
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
