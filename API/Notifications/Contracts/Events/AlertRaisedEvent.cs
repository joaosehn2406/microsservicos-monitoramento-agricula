namespace Notifications.Contracts.Events;

public sealed record AlertRaisedEvent
{
    public required Guid EventId { get; init; }
    public required Guid AlertId { get; init; }
    public required Guid PropertyId { get; init; }
    public required string Severity { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
}
