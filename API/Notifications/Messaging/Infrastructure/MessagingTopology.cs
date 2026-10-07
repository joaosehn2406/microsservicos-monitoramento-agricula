using RabbitMQ.Client;

namespace Notifications.Messaging.Infrastructure;

/// <summary>
/// Consumer topology from SPEC 5.1. Every declaration is durable and idempotent and must
/// use exactly the same arguments in every service.
/// </summary>
public static class MessagingTopology
{
    public const string RetryExchange = "farm.retry";
    public const string DeadLetterExchange = "farm.dlx";
    public const string DefaultExchange = "";
    public const int RetryDelayMilliseconds = 30000;

    public static string RetryQueue(string queue) => $"{queue}.retry";

    public static string DeadLetterQueue(string queue) => $"{queue}.dlq";

    public static async Task DeclareConsumerTopologyAsync(
        IChannel channel,
        string eventsExchange,
        string queue,
        string bindingKey,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(eventsExchange, ExchangeType.Topic, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(RetryExchange, ExchangeType.Direct, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Direct, durable: true, autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = DeadLetterExchange,
                ["x-dead-letter-routing-key"] = queue
            },
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue, eventsExchange, bindingKey, cancellationToken: cancellationToken);

        var retryQueue = RetryQueue(queue);
        await channel.QueueDeclareAsync(retryQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-message-ttl"] = RetryDelayMilliseconds,
                ["x-dead-letter-exchange"] = DefaultExchange,
                ["x-dead-letter-routing-key"] = queue
            },
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(retryQueue, RetryExchange, queue, cancellationToken: cancellationToken);

        var deadLetterQueue = DeadLetterQueue(queue);
        await channel.QueueDeclareAsync(deadLetterQueue, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(deadLetterQueue, DeadLetterExchange, queue, cancellationToken: cancellationToken);
    }
}
