using RabbitMQ.Client;

namespace Storage.Messaging.Infrastructure;

/// <summary>
/// Republishes an existing message (same body and properties) with adjusted headers.
/// The channel must have publisher confirms with tracking enabled: the call only returns
/// after the broker confirms, and throws on nack, return (mandatory) or timeout.
/// </summary>
public static class MessageRepublisher
{
    private static readonly TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(10);

    public static async Task PublishAsync(
        IChannel confirmChannel,
        string exchange,
        string routingKey,
        IReadOnlyBasicProperties originalProperties,
        ReadOnlyMemory<byte> body,
        Action<IDictionary<string, object?>>? mutateHeaders,
        CancellationToken cancellationToken)
    {
        var properties = new BasicProperties(originalProperties)
        {
            Persistent = true
        };

        var headers = originalProperties.Headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(originalProperties.Headers);
        mutateHeaders?.Invoke(headers);
        properties.Headers = headers;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ConfirmTimeout);

        await confirmChannel.BasicPublishAsync(exchange, routingKey, mandatory: true, properties, body,
            timeout.Token);
    }
}
