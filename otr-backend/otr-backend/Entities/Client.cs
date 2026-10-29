namespace otr_backend.Models;

/// <summary>A customer (or internal cost center) that projects can be billed to.</summary>
public class Client
{
    public uint Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
