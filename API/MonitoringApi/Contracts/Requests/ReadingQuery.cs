namespace MonitoringApi.Contracts.Requests;

public sealed class ReadingQuery : PageQuery
{
    public Guid? PropertyId { get; init; }
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
}
