namespace BackendApi.Entities;

public sealed class Notification
{
    public Guid Id { get; set; }
    public Guid AlertId { get; set; }
    public Guid AlertEventId { get; set; }
    public Guid PropertyId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
