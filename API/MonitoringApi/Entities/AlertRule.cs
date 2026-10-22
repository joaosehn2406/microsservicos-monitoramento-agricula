namespace MonitoringApi.Entities;

public sealed class AlertRule : BaseEntity
{
    public Guid PropertyId { get; set; }
    public string Variable { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}
