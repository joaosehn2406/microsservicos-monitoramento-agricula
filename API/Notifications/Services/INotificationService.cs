using Notifications.Messaging.Contracts;
using Notifications.Messaging.Infrastructure;

namespace Notifications.Services;

public interface INotificationService
{
    Task<ProcessingResult> ProcessAsync(AlertRaisedEvent message, CancellationToken cancellationToken);
}
