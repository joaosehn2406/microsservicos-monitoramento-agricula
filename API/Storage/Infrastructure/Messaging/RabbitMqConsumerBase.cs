using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Storage.Infrastructure.Messaging;

public abstract class RabbitMqConsumerBase<TMessage>(
    RabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger logger) : BackgroundService
{
    private const string DeadLetterExchange = "farm.dlx";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true
    };

    protected abstract string QueueName { get; }
    protected abstract string BindingKey { get; }
    protected abstract Guid GetEventId(TMessage message);
    protected abstract Task<ProcessingResult> ProcessAsync(
        TMessage message, IServiceProvider services, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await connectionProvider.GetConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.BasicQosAsync(0, options.Value.PrefetchCount, global: false, stoppingToken);

        await channel.ExchangeDeclareAsync(options.Value.Exchange, ExchangeType.Topic,
            durable: true, autoDelete: false, cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Direct,
            durable: true, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = DeadLetterExchange,
                ["x-dead-letter-routing-key"] = QueueName
            }, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(QueueName, options.Value.Exchange, BindingKey,
            cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync($"{QueueName}.dlq", durable: true, exclusive: false,
            autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync($"{QueueName}.dlq", DeadLetterExchange, QueueName,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, delivery) => HandleAsync(channel, delivery, stoppingToken);
        await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Consuming {Queue} with binding {BindingKey}", QueueName, BindingKey);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken stoppingToken)
    {
        var eventId = delivery.BasicProperties.MessageId ?? "unknown";
        try
        {
            var message = JsonSerializer.Deserialize<TMessage>(delivery.Body.Span, JsonOptions)
                ?? throw new InvalidMessageException("Message body is null.");
            eventId = GetEventId(message).ToString();

            await using var scope = scopeFactory.CreateAsyncScope();
            var result = await ProcessAsync(message, scope.ServiceProvider, stoppingToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, CancellationToken.None);
            logger.LogInformation("Message {EventId} from {Queue}: {Result}",
                eventId, QueueName, result);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            await RejectAsync(channel, delivery, eventId, requeue: true);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Message {EventId} from {Queue} sent to DLQ", eventId, QueueName);
            await RejectAsync(channel, delivery, eventId, requeue: false);
        }
    }

    private async Task RejectAsync(IChannel channel, BasicDeliverEventArgs delivery, string eventId, bool requeue)
    {
        try
        {
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Could not reject message {EventId} from {Queue}", eventId, QueueName);
        }
    }
}

public enum ProcessingResult
{
    Processed,
    Duplicate
}

public sealed class InvalidMessageException(string message) : Exception(message);
