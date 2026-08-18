namespace otr_backend.Models;

public class Notification
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public string? LinkUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
