namespace otr_backend.Models;

/// <summary>Join entity: a tag attached to a time entry.</summary>
public class TimeEntryTagMapping
{
    public uint TimeEntryId { get; set; }
    public uint TagId { get; set; }

    public TimeEntry TimeEntry { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
