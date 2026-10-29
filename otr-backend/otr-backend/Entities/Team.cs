namespace otr_backend.Models;

/// <summary>A group of users that share a manager.</summary>
public class Team
{
    public uint Id { get; set; }

    /// <summary>Stable technical identifier.</summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>Human-readable label shown in the UI.</summary>
    public string TeamDisplayName { get; set; } = string.Empty;

    /// <summary>Id of the user who manages the team and may review its members' data.</summary>
    public uint ManagerId { get; set; }

    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}
