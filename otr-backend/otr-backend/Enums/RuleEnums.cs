namespace otr_backend.Enums;

/// <summary>Defines who a <c>Rule</c> applies to.</summary>
public enum RuleScope
{
    /// <summary>Everyone.</summary>
    Global,

    /// <summary>All members of one team.</summary>
    Team,

    /// <summary>A single user.</summary>
    User
}

/// <summary>The kind of constraint a <c>Rule</c> enforces.</summary>
public enum RuleType
{
    /// <summary>Stops a running timer once it exceeds a maximum duration.</summary>
    MaxDurationAutoStop,

    /// <summary>Forbids time entries that overlap each other.</summary>
    NoOverlap,

    /// <summary>Only allows tracking within configured working hours.</summary>
    WorkingHoursOnly
}
