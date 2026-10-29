namespace otr_backend.Dtos;

/// <summary>Request body for changing the descriptive fields of a time entry.</summary>
public class TimeEntryUpdateRequest
{
    public uint? ProjectId { get; set; }
    public uint? ProjectTaskId { get; set; }
    public string? Description { get; set; }
}
