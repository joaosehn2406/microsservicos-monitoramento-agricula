using Notifications.Entities;
using Notifications.Exceptions;
using Notifications.Mappers;
using Notifications.Messaging.Contracts;
using Notifications.Messaging.Infrastructure;
using Notifications.Repositories;

namespace Notifications.Services;

public sealed class NotificationService(
    INotificationRepository repository,
    ILogger<NotificationService> logger) : INotificationService
{
    public const string ConsumerName = "notifications";
    public const string ExpectedEventType = "alert.raised";

    private static readonly HashSet<string> Operators = [">", "<", ">=", "<="];
    private static readonly HashSet<string> Severities = ["low", "medium", "high", "critical"];

    public async Task<ProcessingResult> ProcessAsync(AlertRaisedEvent message, CancellationToken cancellationToken)
    {
        Validate(message);

        var processedEvent = new ProcessedEvent { EventId = message.EventId, Consumer = ConsumerName };
        var notification = NotificationMapper.ToEntity(message);

        var added = await repository.TryAddAsync(processedEvent, notification, cancellationToken);
        if (!added)
        {
            logger.LogInformation(
                "Alert event {EventId} (alert {AlertId}, severity {Severity}) was already processed; ignored",
                message.EventId, message.AlertId, message.Severity);
            return ProcessingResult.Duplicate;
        }

        logger.LogInformation(
            "Notification {NotificationId} recorded for alert {AlertId} (event {EventId}, severity {Severity}, " +
            "property {PropertyId})",
            notification.Id, message.AlertId, message.EventId, message.Severity, message.PropertyId);
        return ProcessingResult.Processed;
    }

    private static void Validate(AlertRaisedEvent message)
    {
        var errors = new List<string>();

        if (message.EventId == Guid.Empty) errors.Add("eventId is empty");
        if (message.EventType != ExpectedEventType) errors.Add($"eventType must be '{ExpectedEventType}'");
        if (string.IsNullOrWhiteSpace(message.Source)) errors.Add("source is empty");
        if (message.AlertId == Guid.Empty) errors.Add("alertId is empty");
        if (message.SourceEventId == Guid.Empty) errors.Add("sourceEventId is empty");
        if (message.RuleId == Guid.Empty) errors.Add("ruleId is empty");
        if (message.PropertyId == Guid.Empty) errors.Add("propertyId is empty");
        if (string.IsNullOrWhiteSpace(message.PropertyName)) errors.Add("propertyName is empty");
        if (!NotificationMapper.IsKnownVariable(message.Variable)) errors.Add("variable is unknown");
        if (!Operators.Contains(message.Operator)) errors.Add("operator is unknown");
        if (!Severities.Contains(message.Severity)) errors.Add("severity is unknown");
        if (!double.IsFinite(message.Threshold)) errors.Add("threshold is not a number");
        if (!double.IsFinite(message.MeasuredValue)) errors.Add("measuredValue is not a number");
        if (message.OccurredAt.Kind == DateTimeKind.Unspecified) errors.Add("occurredAt has no UTC offset");
        if (message.ObservedAt.Kind == DateTimeKind.Unspecified) errors.Add("observedAt has no UTC offset");

        if (errors.Count > 0)
        {
            throw new InvalidMessageException($"Invalid alert.raised event: {string.Join("; ", errors)}");
        }
    }
}
