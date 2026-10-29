namespace otr_backend.Models;

/// <summary>A free-form label that can be attached to time entries.</summary>
public class Tag
{
    public uint Id { get; set; }
    public string TagName { get; set; } = string.Empty;

    /// <summary>Display color as hex string, e.g. <c>#ff8800</c>.</summary>
    public string? ColorHex { get; set; }
}
