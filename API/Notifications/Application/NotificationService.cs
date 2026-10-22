using Microsoft.EntityFrameworkCore;
using Npgsql;
using Notifications.Infrastructure.Messaging;
using Notifications.Infrastructure.Persistence;
using Notifications.Contracts.Events;
using Notifications.Domain;

namespace Notifications.Application;

public sealed class NotificationService(
    NotificationsDbContext context,
    ILogger<NotificationService> logger)
{
    public const string ConsumerName = "notifications";

    private static readonly HashSet<string> Severities = ["low", "medium", "high", "critical"];
    private static readonly HashSet<string> DuplicateConstraints =
        ["pk_processed_events", "uq_notifications_alert_event_id"];

    public async Task<ProcessingResult> ProcessAsync(AlertRaisedEvent message, CancellationToken cancellationToken)
    {
        Validate(message);

        var processedEvent = new ProcessedEvent { EventId = message.EventId, Consumer = ConsumerName };
        var notification = new Notification
        {
            AlertId = message.AlertId,
            AlertEventId = message.EventId,
            PropertyId = message.PropertyId,
            Severity = message.Severity,
            Title = message.Title,
            Message = message.Message
        };

        try
        {
            context.ProcessedEvents.Add(processedEvent);
            context.Notifications.Add(notification);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                                                   {
                                                       SqlState: PostgresErrorCodes.UniqueViolation
                                                   } postgres
                                                   && DuplicateConstraints.Contains(postgres.ConstraintName ?? ""))
        {
            context.ChangeTracker.Clear();
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
        if (message.AlertId == Guid.Empty) errors.Add("alertId is empty");
        if (message.PropertyId == Guid.Empty) errors.Add("propertyId is empty");
        if (!Severities.Contains(message.Severity)) errors.Add("severity is unknown");
        if (string.IsNullOrWhiteSpace(message.Title)) errors.Add("title is empty");
        if (string.IsNullOrWhiteSpace(message.Message)) errors.Add("message is empty");

        if (errors.Count > 0)
        {
            throw new InvalidMessageException($"Invalid alert.raised event: {string.Join("; ", errors)}");
        }
    }
}
