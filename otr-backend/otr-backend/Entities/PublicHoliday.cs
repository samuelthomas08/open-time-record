namespace otr_backend.Models;

/// <summary>A public holiday on which no working time is expected.</summary>
public class PublicHoliday
{
    public uint Id { get; set; }
    public DateOnly Date { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Region the holiday applies to; <c>null</c> means it applies everywhere.</summary>
    public string? Region { get; set; }
}
