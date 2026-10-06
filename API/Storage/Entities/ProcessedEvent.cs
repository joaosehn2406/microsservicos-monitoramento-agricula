namespace Storage.Entities;

public sealed class ProcessedEvent
{
    public Guid EventId { get; set; }
    public string Consumer { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
