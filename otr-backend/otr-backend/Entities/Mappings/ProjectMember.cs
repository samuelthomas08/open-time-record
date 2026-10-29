namespace otr_backend.Models;

/// <summary>Join entity: a user taking part in a project.</summary>
public class ProjectMember
{
    public uint ProjectId { get; set; }
    public uint UserId { get; set; }

    /// <summary>Whether the member may book time on the project.</summary>
    public bool CanLog { get; set; }

    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
}
