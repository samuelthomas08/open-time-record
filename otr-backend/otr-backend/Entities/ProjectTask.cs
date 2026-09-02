namespace otr_backend.Models;

public class ProjectTask
{
    public uint Id { get; set; }
    public uint ProjectId { get; set; }
    public string TaskName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Project Project { get; set; } = null!;
}
