namespace otr_backend.Models;

public class ProjectMember
{
    public uint ProjectId { get; set; }
    public uint UserId { get; set; }
    public bool CanLog { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
