namespace otr_backend.Models;

/// <summary>An in-app message addressed to a single user.</summary>
public class Notification
{
    public uint Id { get; set; }
    public uint UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    /// <summary>Optional frontend route the notification points to.</summary>
    public string? LinkUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
