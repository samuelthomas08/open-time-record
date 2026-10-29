namespace otr_backend.Models;

/// <summary>Join entity: a user belonging to a team.</summary>
public class UserTeamMapping
{
    public uint TeamId { get; set; }
    public uint UserId { get; set; }

    public User User { get; set; } = null!;
    public Team Team { get; set; } = null!;
}
