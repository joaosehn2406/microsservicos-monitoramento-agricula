namespace MonitoringApi.Entities;

public sealed class Notification : BaseEntity
{
    public Guid AlertId { get; set; }
    public Guid AlertEventId { get; set; }
    public Guid PropertyId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
