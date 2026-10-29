namespace otr_backend.Dtos;

/// <summary>Request body for starting a break.</summary>
public class BreakStartRequest
{
    /// <summary>Why the break is taken. Mandatory if <c>BreakReasonRequired</c> is enabled.</summary>
    public string? Reason { get; set; }
}
