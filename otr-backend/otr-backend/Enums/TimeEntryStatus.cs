namespace otr_backend.Enums;

/// <summary>Lifecycle of a time entry.</summary>
public enum TimeEntryStatus
{
    /// <summary>The timer is still running.</summary>
    Running,

    /// <summary>The timer was stopped.</summary>
    Completed,

    /// <summary>A correction request for the entry is awaiting review.</summary>
    PendingCorrection
}
