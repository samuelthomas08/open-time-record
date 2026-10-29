using otr_backend.Enums;

namespace otr_backend.Models;

/// <summary>An append-only record of a change made to a <see cref="TimeEntry"/>.</summary>
public class TimeEntryAuditLog
{
    public uint Id { get; set; }
    public uint TimeEntryId { get; set; }
    public uint ChangedByUserId { get; set; }
    public AuditAction Action { get; set; }

    /// <summary>Name of the changed field, if the action concerned a single field.</summary>
    public string? FieldName { get; set; }

    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; }

    public TimeEntry TimeEntry { get; set; } = null!;
    public User ChangedByUser { get; set; } = null!;
}
