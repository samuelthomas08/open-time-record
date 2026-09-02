using System.Text.Json.Serialization;

namespace otr_backend.Models;

public class TimeEntryBreak
{
    public uint Id { get; set; }
    public uint TimeEntryId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Reason { get; set; }

    [JsonIgnore]
    public TimeEntry TimeEntry { get; set; } = null!;
}
