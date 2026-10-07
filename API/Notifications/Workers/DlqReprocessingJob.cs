using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Notifications.Configurations;
using Notifications.Messaging.Infrastructure;

namespace Notifications.Workers;

/// <summary>
/// Periodically drains the consumer DLQ (SPEC 5.6): reprocessable messages go back to the
/// main queue, the others are put back at the end of the DLQ. Uses its own channel.
/// </summary>
public sealed class DlqReprocessingJob(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IOptions<DlqReprocessOptions> dlqOptions,
    ILogger<DlqReprocessingJob> logger) : BackgroundService
{
    public const int MaxDlqReprocessCount = 5;
    private const string ReprocessLimitReached = "reprocess-limit-reached";

    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var queue = rabbitMqOptions.Value.Queue;
        var interval = TimeSpan.FromMinutes(Math.Max(1, dlqOptions.Value.IntervalMinutes));
        logger.LogInformation("DLQ reprocessing of {Queue} scheduled every {Interval}",
            MessagingTopology.DeadLetterQueue(queue), interval);

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await RunOnceAsync(queue, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "DLQ reprocessing of {Queue} failed; retrying on the next run",
                    MessagingTopology.DeadLetterQueue(queue));
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_channel is null)
        {
            return;
        }

        try
        {
            if (_channel.IsOpen)
            {
                await _channel.CloseAsync();
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error while closing the DLQ reprocessing channel");
        }

        await _channel.DisposeAsync();
    }

    private async Task RunOnceAsync(string queue, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(queue, cancellationToken);
        var deadLetterQueue = MessagingTopology.DeadLetterQueue(queue);

        var pending = (await channel.QueueDeclarePassiveAsync(deadLetterQueue, cancellationToken)).MessageCount;
        int reprocessed = 0, kept = 0, exhausted = 0;

        for (var i = 0; i < pending; i++)
        {
            var message = await channel.BasicGetAsync(deadLetterQueue, autoAck: false, cancellationToken);
            if (message is null)
            {
                break;
            }

            var headers = message.BasicProperties.Headers;
            var eventId = message.BasicProperties.MessageId ?? "unknown";
            var reprocessable = MessageHeaders.GetBool(headers, MessageHeaders.Reprocessable) ?? true;
            var reprocessCount = MessageHeaders.GetInt(headers, MessageHeaders.DlqReprocessCount);

            string exchange;
            Action<IDictionary<string, object?>>? mutateHeaders;
            Action countOutcome;

            if (!reprocessable)
            {
                exchange = MessagingTopology.DeadLetterExchange;
                mutateHeaders = null;
                countOutcome = () => kept++;
            }
            else if (reprocessCount >= MaxDlqReprocessCount)
            {
                var reason = MessageHeaders.GetString(headers, MessageHeaders.FailureReason);
                exchange = MessagingTopology.DeadLetterExchange;
                mutateHeaders = h =>
                {
                    h[MessageHeaders.Reprocessable] = false;
                    h[MessageHeaders.FailureReason] = string.IsNullOrEmpty(reason)
                        ? ReprocessLimitReached
                        : $"{reason}; {ReprocessLimitReached}";
                };
                countOutcome = () => exhausted++;
            }
            else
            {
                exchange = MessagingTopology.DefaultExchange;
                mutateHeaders = h =>
                {
                    h[MessageHeaders.DlqReprocessCount] = reprocessCount + 1;
                    h[MessageHeaders.RetryCount] = 0;
                };
                countOutcome = () => reprocessed++;
            }

            try
            {
                await MessageRepublisher.PublishAsync(channel, exchange, queue, message.BasicProperties,
                    message.Body, mutateHeaders, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception,
                    "Republishing DLQ message {EventId} to {Exchange} failed; message requeued and run aborted",
                    eventId, exchange);
                await channel.BasicNackAsync(message.DeliveryTag, multiple: false, requeue: true,
                    CancellationToken.None);
                break;
            }

            await channel.BasicAckAsync(message.DeliveryTag, multiple: false, CancellationToken.None);
            countOutcome();
            logger.LogDebug(
                "DLQ message {EventId} (x-dlq-reprocess-count {ReprocessCount}) republished to {Exchange}",
                eventId, reprocessCount, exchange == MessagingTopology.DefaultExchange ? queue : exchange);
        }

        logger.LogInformation(
            "DLQ reprocessing of {Queue} finished: {Pending} pending, {Reprocessed} reprocessed, {Kept} kept, " +
            "{Exhausted} reached the reprocess limit",
            deadLetterQueue, pending, reprocessed, kept, exhausted);
    }

    private async Task<IChannel> GetChannelAsync(string queue, CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        var connection = await connectionProvider.GetConnectionAsync(cancellationToken);
        _channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);

        await MessagingTopology.DeclareConsumerTopologyAsync(
            _channel, rabbitMqOptions.Value.Exchange, queue, rabbitMqOptions.Value.BindingKey, cancellationToken);

        return _channel;
    }
}
