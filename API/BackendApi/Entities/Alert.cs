namespace BackendApi.Entities;

public sealed class Alert
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid SourceEventId { get; set; }
    public Guid? RuleId { get; set; }
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public string Variable { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public double MeasuredValue { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime ObservedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
