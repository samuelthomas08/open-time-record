using otr_backend.Enums;

namespace otr_backend.Models;

/// <summary>A request to change the start and/or end time of an existing entry, subject to review.</summary>
public class TimeEntryCorrectionRequest
{
    public uint Id { get; set; }
    public uint TimeEntryId { get; set; }
    public uint RequestedByUserId { get; set; }
    public uint? ReviewedByUserId { get; set; }
    public ApprovalStatus Status { get; set; }

    /// <summary>Justification given by the requester.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Optional comment from the reviewer, e.g. why the request was rejected.</summary>
    public string? ReviewNote { get; set; }

    /// <summary>Requested new start time; <c>null</c> keeps the current value.</summary>
    public DateTime? ProposedStartTime { get; set; }

    /// <summary>Requested new end time; <c>null</c> keeps the current value.</summary>
    public DateTime? ProposedEndTime { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public TimeEntry TimeEntry { get; set; } = null!;
    public User RequestedByUser { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}
