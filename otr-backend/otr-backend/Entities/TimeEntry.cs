using otr_backend.Enums;

namespace otr_backend.Models;

/// <summary>One contiguous block of tracked working time.</summary>
public class TimeEntry
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public uint? ProjectId { get; set; }
    public uint? ProjectTaskId { get; set; }

    /// <summary>UTC start of the entry.</summary>
    public DateTime StartTime { get; set; }

    /// <summary>UTC end of the entry; <c>null</c> while the timer is still running.</summary>
    public DateTime? EndTime { get; set; }

    public TimeEntryStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Project? Project { get; set; }
    public ProjectTask? ProjectTask { get; set; }
    public List<TimeEntryBreak> Breaks { get; set; } = [];
}
