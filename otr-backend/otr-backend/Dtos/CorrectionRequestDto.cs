using otr_backend.Enums;

namespace otr_backend.Dtos;

public class CorrectionRequestDto
{
    public uint Id { get; set; }
    public uint TimeEntryId { get; set; }
    public DateTime TimeEntryStartTime { get; set; }
    public DateTime? TimeEntryEndTime { get; set; }
    public uint RequestedByUserId { get; set; }
    public string RequestedByUserName { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public string? ReviewNote { get; set; }
}
