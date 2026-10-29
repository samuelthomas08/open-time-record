namespace otr_backend.Models;

/// <summary>A sub-division of a <see cref="Project"/> that time can be booked against.</summary>
public class ProjectTask
{
    public uint Id { get; set; }
    public uint ProjectId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
}
