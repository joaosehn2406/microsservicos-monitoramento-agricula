using Analytics.Messaging.Contracts;

namespace Analytics.Messaging.Publishers;

public interface IAlertPublisher
{
    /// <summary>
    /// Publishes to farm.events with routing key alert.&lt;variable&gt; and waits for the confirm.
    /// Returns false (already logged) when every attempt failed or the message was returned.
    /// </summary>
    Task<bool> PublishAsync(AlertRaisedEvent alertEvent, CancellationToken cancellationToken);
}
