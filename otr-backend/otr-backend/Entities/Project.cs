namespace otr_backend.Models;

public class Project
{
    public uint Id { get; set; }
    public string ProjectName { get; set; }
    public string ProjectDisplayName { get; set; }
    public uint? ClientId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Client? Client { get; set; }
}
