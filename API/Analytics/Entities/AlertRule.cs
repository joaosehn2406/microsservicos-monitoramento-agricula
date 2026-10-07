namespace Analytics.Entities;

/// <summary>Read-only here: rules are managed by the BackendApi.</summary>
public sealed class AlertRule
{
    public Guid Id { get; set; }
    public Guid PropertyId { get; set; }
    public string Variable { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public string Severity { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
