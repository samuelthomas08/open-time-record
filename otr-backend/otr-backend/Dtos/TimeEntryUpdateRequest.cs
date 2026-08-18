namespace otr_backend.Dtos;

public class TimeEntryUpdateRequest
{
    public uint? ProjectId { get; set; }
    public uint? ProjectTaskId { get; set; }
    public string? Description { get; set; }
}
