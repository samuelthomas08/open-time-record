using otr_backend.Enums;

namespace otr_backend.Dtos;

/// <summary>A correction request flattened with the names of the people and times involved.</summary>
public class CorrectionRequestDto
{
    public uint Id { get; set; }
    public uint TimeEntryId { get; set; }

    /// <summary>Current start time of the entry the request refers to.</summary>
    public DateTime TimeEntryStartTime { get; set; }

    /// <summary>Current end time of the entry the request refers to.</summary>
    public DateTime? TimeEntryEndTime { get; set; }

    public uint RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public string? ReviewNote { get; set; }
    public uint? ReviewedByUserId { get; set; }
    public string? ReviewedByUserName { get; set; }
    public string? ReviewedByUserEmail { get; set; }
    public string? ReviewedByProfilePictureUrl { get; set; }
}
