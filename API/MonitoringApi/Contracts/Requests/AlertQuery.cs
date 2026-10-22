namespace MonitoringApi.Contracts.Requests;

public sealed class AlertQuery : PageQuery
{
    public Guid? PropertyId { get; init; }
    public string? Variable { get; init; }
    public string? Severity { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}
