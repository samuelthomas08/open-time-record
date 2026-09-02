namespace otr_backend.Dtos;

public class CorrectionRequestCreateRequest
{
    public uint TimeEntryId { get; set; }
    public string Reason { get; set; }
    public DateTime? ProposedStartTime { get; set; }
    public DateTime? ProposedEndTime { get; set; }
}
