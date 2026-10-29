namespace otr_backend.Enums;

/// <summary>Kind of change recorded in the time entry audit log.</summary>
public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    CorrectionRequested,
    CorrectionApproved,
    CorrectionRejected
}
