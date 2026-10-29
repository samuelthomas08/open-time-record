namespace otr_backend.Models;

/// <summary>A category of absence, e.g. "Vacation" or "Sick leave".</summary>
public class LeaveType
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Whether the absence counts as paid working time.</summary>
    public bool IsPaid { get; set; }
}
