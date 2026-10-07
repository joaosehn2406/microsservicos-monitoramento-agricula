using Notifications.Entities;

namespace Notifications.Repositories;

public interface INotificationRepository
{
    /// <summary>
    /// Inserts the processed-event marker and the notification in one transaction (SPEC 3.3).
    /// Returns false when the event was already processed.
    /// </summary>
    Task<bool> TryAddAsync(ProcessedEvent processedEvent, Notification notification, CancellationToken cancellationToken);
}
