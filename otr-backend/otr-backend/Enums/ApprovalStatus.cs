namespace otr_backend.Enums;

/// <summary>Lifecycle of a request that needs a reviewer's decision.</summary>
public enum ApprovalStatus
{
    /// <summary>Waiting for a reviewer.</summary>
    Pending,

    /// <summary>Accepted by a reviewer.</summary>
    Approved,

    /// <summary>Declined by a reviewer.</summary>
    Rejected
}
