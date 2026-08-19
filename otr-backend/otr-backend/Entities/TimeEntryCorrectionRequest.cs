using otr_backend.Enums;

namespace otr_backend.Models;

public class TimeEntryCorrectionRequest
{
    public uint Id { get; set; }
    public uint TimeEntryId { get; set; }
    public uint RequestedByUserId { get; set; }
    public uint? ReviewedByUserId { get; set; }
    public ApprovalStatus Status { get; set; }
    public string Reason { get; set; }
    public string? ReviewNote { get; set; }
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public TimeEntry TimeEntry { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}
