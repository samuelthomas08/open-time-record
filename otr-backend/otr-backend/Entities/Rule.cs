using otr_backend.Enums;

namespace otr_backend.Models;

public class Rule
{
    public uint Id { get; set; }
    public RuleType Type { get; set; }
    public RuleScope Scope { get; set; }
    public uint? TargetId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
