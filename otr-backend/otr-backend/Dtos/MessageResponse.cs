namespace otr_backend.Dtos;

/// <summary>A response that only carries a human-readable message.</summary>
public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}
