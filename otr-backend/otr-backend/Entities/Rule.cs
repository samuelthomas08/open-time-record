using otr_backend.Enums;

namespace otr_backend.Models;

/// <summary>A configurable constraint on how time may be tracked.</summary>
public class Rule
{
    public uint Id { get; set; }
    public RuleType Type { get; set; }

    /// <summary>Whether the rule applies globally, to a team or to a single user.</summary>
    public RuleScope Scope { get; set; }

    /// <summary>Id of the team or user the rule targets; <c>null</c> for global rules.</summary>
    public uint? TargetId { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
