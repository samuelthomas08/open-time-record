namespace otr_backend.Models;

public class Client
{
    public uint Id { get; set; }
    public string ClientName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
