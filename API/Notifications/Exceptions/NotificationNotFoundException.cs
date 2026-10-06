namespace Notifications.Exceptions;

public sealed class NotificationNotFoundException : Exception
{
    public NotificationNotFoundException()
        : base("Notification was not found.")
    {
    }
}
