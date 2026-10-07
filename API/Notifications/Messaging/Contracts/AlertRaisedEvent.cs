namespace Notifications.Messaging.Contracts;

/// <summary><c>alert.raised</c> event (SPEC 5.7), published with routing key <c>alert.&lt;variable&gt;</c>.</summary>
public sealed record AlertRaisedEvent
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTime OccurredAt { get; init; }
    public required string Source { get; init; }
    public required Guid AlertId { get; init; }
    public required Guid SourceEventId { get; init; }
    public required Guid RuleId { get; init; }
    public required Guid PropertyId { get; init; }
    public required string PropertyName { get; init; }
    public required string Variable { get; init; }
    public required string Operator { get; init; }
    public required double Threshold { get; init; }
    public required double MeasuredValue { get; init; }
    public required string Severity { get; init; }
    public required DateTime ObservedAt { get; init; }
}
