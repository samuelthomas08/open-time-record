namespace otr_backend.Models;

public class Team
{
    public uint Id { get; set; }
    public string TeamName { get; set; }
    public string TeamDisplayName { get; set; }
    public uint ManagerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}