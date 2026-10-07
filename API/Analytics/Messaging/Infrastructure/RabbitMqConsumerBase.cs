using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Analytics.Configurations;
using Analytics.Exceptions;

namespace Analytics.Messaging.Infrastructure;

/// <summary>
/// Generic queue consumer (SPEC 5.3–5.5): declares the topology, consumes with manual ACK,
/// and routes failures to retry (transient) or DLQ (permanent / retries exhausted).
/// Subclasses only provide the queue, the binding key and the business processing.
/// </summary>
public abstract class RabbitMqConsumerBase<TMessage>(
    RabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger logger) : BackgroundService
{
    public const int MaxRetryCount = 3;

    private IChannel? _consumeChannel;
    private IChannel? _publishChannel;
    private string? _consumerTag;
    private CancellationToken _stoppingToken;

    protected abstract string QueueName { get; }

    protected abstract string BindingKey { get; }

    protected abstract Guid GetEventId(TMessage message);

    /// <summary>
    /// Processes one message inside its own DI scope. Throw <see cref="InvalidMessageException"/>
    /// for permanent errors; any other exception is treated as transient.
    /// </summary>
    protected abstract Task<ProcessingResult> ProcessAsync(
        TMessage message,
        IServiceProvider services,
        CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        var settings = options.Value;
        var connection = await connectionProvider.GetConnectionAsync(stoppingToken);

        _consumeChannel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await _consumeChannel.BasicQosAsync(0, settings.PrefetchCount, global: false, stoppingToken);

        _publishChannel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            stoppingToken);

        await MessagingTopology.DeclareConsumerTopologyAsync(
            _consumeChannel, settings.Exchange, QueueName, BindingKey, stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_consumeChannel);
        consumer.ReceivedAsync += OnReceivedAsync;
        _consumerTag = await _consumeChannel.BasicConsumeAsync(QueueName, autoAck: false, consumer, stoppingToken);

        logger.LogInformation(
            "Consuming {Queue} (binding {BindingKey}, prefetch {PrefetchCount})",
            QueueName, BindingKey, settings.PrefetchCount);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumeChannel is { IsOpen: true } && _consumerTag is not null)
        {
            try
            {
                await _consumeChannel.BasicCancelAsync(_consumerTag, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Error while cancelling the consumer of {Queue}", QueueName);
            }
        }

        await base.StopAsync(cancellationToken);

        await CloseChannelAsync(_consumeChannel);
        await CloseChannelAsync(_publishChannel);
        logger.LogInformation("Stopped consuming {Queue}", QueueName);
    }

    private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs delivery)
    {
        var headers = delivery.BasicProperties.Headers;
        var retryCount = MessageHeaders.GetInt(headers, MessageHeaders.RetryCount);
        var eventId = delivery.BasicProperties.MessageId ?? "unknown";

        try
        {
            var message = Deserialize(delivery.Body);
            eventId = GetEventId(message).ToString();

            await using var scope = scopeFactory.CreateAsyncScope();
            var result = await ProcessAsync(message, scope.ServiceProvider, _stoppingToken);

            await _consumeChannel!.BasicAckAsync(delivery.DeliveryTag, multiple: false, CancellationToken.None);
            logger.LogInformation(
                "Message {EventId} from {Queue} handled (x-retry-count {RetryCount}): {Outcome}",
                eventId, QueueName, retryCount, result == ProcessingResult.Processed ? "persisted" : "duplicate");
        }
        catch (InvalidMessageException exception)
        {
            var published = await RepublishAndAckAsync(delivery, eventId, retryCount,
                MessagingTopology.DeadLetterExchange, outcome: "dlq",
                mutateHeaders: h =>
                {
                    h[MessageHeaders.Reprocessable] = false;
                    h[MessageHeaders.FailureReason] = exception.Message;
                });
            if (published)
            {
                logger.LogWarning(
                    "Message {EventId} from {Queue} is invalid (x-retry-count {RetryCount}): {Outcome}. Reason: {Reason}",
                    eventId, QueueName, retryCount, "dlq", exception.Message);
            }
        }
        catch (OperationCanceledException) when (_stoppingToken.IsCancellationRequested)
        {
            await NackAsync(delivery, eventId);
            logger.LogInformation(
                "Message {EventId} from {Queue} requeued due to shutdown (x-retry-count {RetryCount}): {Outcome}",
                eventId, QueueName, retryCount, "requeued");
        }
        catch (Exception exception)
        {
            if (retryCount < MaxRetryCount)
            {
                var published = await RepublishAndAckAsync(delivery, eventId, retryCount,
                    MessagingTopology.RetryExchange, outcome: "retry",
                    mutateHeaders: h => h[MessageHeaders.RetryCount] = retryCount + 1);
                if (published)
                {
                    logger.LogWarning(exception,
                        "Message {EventId} from {Queue} failed (x-retry-count {RetryCount}): {Outcome}",
                        eventId, QueueName, retryCount, "retry");
                }
            }
            else
            {
                var published = await RepublishAndAckAsync(delivery, eventId, retryCount,
                    MessagingTopology.DeadLetterExchange, outcome: "dlq",
                    mutateHeaders: h =>
                    {
                        h[MessageHeaders.Reprocessable] = true;
                        h[MessageHeaders.FailureReason] = exception.Message;
                    });
                if (published)
                {
                    logger.LogError(exception,
                        "Message {EventId} from {Queue} exhausted its retries (x-retry-count {RetryCount}): {Outcome}",
                        eventId, QueueName, retryCount, "dlq");
                }
            }
        }
    }

    private static TMessage Deserialize(ReadOnlyMemory<byte> body)
    {
        try
        {
            return JsonSerializer.Deserialize<TMessage>(body.Span, MessagingJson.Options)
                   ?? throw new InvalidMessageException("Message body is null.");
        }
        catch (JsonException exception)
        {
            throw new InvalidMessageException($"Invalid message body: {exception.Message}", exception);
        }
    }

    /// <summary>
    /// Publishes a copy to <paramref name="exchange"/> (routing key = queue) and ACKs the
    /// original only after the confirm. If the publish fails the original is NACKed with requeue.
    /// </summary>
    private async Task<bool> RepublishAndAckAsync(
        BasicDeliverEventArgs delivery,
        string eventId,
        int retryCount,
        string exchange,
        string outcome,
        Action<IDictionary<string, object?>> mutateHeaders)
    {
        try
        {
            await MessageRepublisher.PublishAsync(_publishChannel!, exchange, QueueName,
                delivery.BasicProperties, delivery.Body, mutateHeaders, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception,
                "Republishing message {EventId} from {Queue} to {Exchange} failed (x-retry-count {RetryCount}); " +
                "{Outcome} not applied, message requeued",
                eventId, QueueName, exchange, retryCount, outcome);
            await NackAsync(delivery, eventId);
            return false;
        }

        try
        {
            await _consumeChannel!.BasicAckAsync(delivery.DeliveryTag, multiple: false, CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "ACK of message {EventId} from {Queue} failed", eventId, QueueName);
        }

        return true;
    }

    private async Task NackAsync(BasicDeliverEventArgs delivery, string eventId)
    {
        try
        {
            await _consumeChannel!.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: true,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "NACK of message {EventId} from {Queue} failed", eventId, QueueName);
        }
    }

    private async Task CloseChannelAsync(IChannel? channel)
    {
        if (channel is null)
        {
            return;
        }

        try
        {
            if (channel.IsOpen)
            {
                await channel.CloseAsync();
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error while closing a channel of {Queue}", QueueName);
        }

        await channel.DisposeAsync();
    }
}
