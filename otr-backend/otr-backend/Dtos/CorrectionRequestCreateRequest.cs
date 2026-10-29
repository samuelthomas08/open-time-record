namespace otr_backend.Dtos;

/// <summary>Request body for asking to correct the times of an existing entry.</summary>
public class CorrectionRequestCreateRequest
{
    public uint TimeEntryId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
}
