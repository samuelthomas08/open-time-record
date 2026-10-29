namespace otr_backend.Dtos;

/// <summary>Request body carrying the optional comment of a reviewer.</summary>
public class ReviewNoteRequest
{
    public string? Note { get; set; }
}
