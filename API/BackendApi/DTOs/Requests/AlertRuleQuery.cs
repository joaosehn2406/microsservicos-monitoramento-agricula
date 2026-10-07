namespace BackendApi.DTOs.Requests;

public sealed class AlertRuleQuery
{
    public Guid? PropertyId { get; init; }
    public string? Variable { get; init; }
    public bool? IsActive { get; init; }
}
