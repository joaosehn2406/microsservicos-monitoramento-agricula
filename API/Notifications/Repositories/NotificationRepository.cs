using Microsoft.EntityFrameworkCore;
using Notifications.Data;
using Notifications.Entities;
using Npgsql;

namespace Notifications.Repositories;

public sealed class NotificationRepository(NotificationsDbContext context) : INotificationRepository
{
    private static readonly HashSet<string> DuplicateConstraints =
        ["pk_processed_events", "uq_notifications_alert_event_id"];

    /// <remarks>
    /// A foreign key violation on <c>alert_id</c> (alert not visible yet) is deliberately not caught:
    /// it reaches the consumer as a transient error and goes through the retry.
    /// </remarks>
    public async Task<bool> TryAddAsync(
        ProcessedEvent processedEvent,
        Notification notification,
        CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            context.ProcessedEvents.Add(processedEvent);
            await context.SaveChangesAsync(cancellationToken);

            context.Notifications.Add(notification);
            await context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                                                   {
                                                       SqlState: PostgresErrorCodes.UniqueViolation
                                                   } postgres
                                                   && DuplicateConstraints.Contains(postgres.ConstraintName ?? ""))
        {
            await transaction.RollbackAsync(CancellationToken.None);
            context.ChangeTracker.Clear();
            return false;
        }
    }
}
