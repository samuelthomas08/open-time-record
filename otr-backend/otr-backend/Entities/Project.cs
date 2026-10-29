namespace otr_backend.Models;

/// <summary>A unit of work that time entries can be booked against.</summary>
public class Project
{
    public uint Id { get; set; }

    /// <summary>Stable technical identifier.</summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>Human-readable label shown in the UI.</summary>
    public string ProjectDisplayName { get; set; } = string.Empty;

    public uint? ClientId { get; set; }

    /// <summary>Inactive projects can no longer be selected for new time entries.</summary>
    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Client? Client { get; set; }
}
