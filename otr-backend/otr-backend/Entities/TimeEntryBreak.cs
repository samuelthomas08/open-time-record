using System.Text.Json.Serialization;

namespace otr_backend.Models;

/// <summary>A pause within a <see cref="TimeEntry"/>; not counted as working time.</summary>
public class TimeEntryBreak
{
    public uint Id { get; set; }
    public uint TimeEntryId { get; set; }
    public DateTime StartTime { get; set; }

    /// <summary>UTC end of the break; <c>null</c> while the break is still running.</summary>
    public DateTime? EndTime { get; set; }

    public string? Reason { get; set; }

    /// <summary>Ignored during serialization to avoid a circular reference with <see cref="Models.TimeEntry.Breaks"/>.</summary>
    [JsonIgnore]
    public TimeEntry TimeEntry { get; set; } = null!;
}
