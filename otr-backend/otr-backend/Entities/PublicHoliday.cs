namespace otr_backend.Models;

public class PublicHoliday
{
    public uint Id { get; set; }
    public DateOnly Date { get; set; }
    public string Name { get; set; }
    public string? Region { get; set; }
}
