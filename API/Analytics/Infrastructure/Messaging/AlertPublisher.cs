using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Analytics.Contracts.Events;
using Analytics.Domain;

namespace Analytics.Infrastructure.Messaging;

public sealed class AlertPublisher(
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> options,
    ILogger<AlertPublisher> logger) : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int MaxAttempts = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ConfirmTimeout = TimeSpan.FromSeconds(10);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private IChannel? _channel;

    public async Task<bool> PublishAsync(AlertRaisedEvent alertEvent, CancellationToken cancellationToken)
    {
        var exchange = options.Value.Exchange;
        const string routingKey = "alert.raised";
        var body = JsonSerializer.SerializeToUtf8Bytes(alertEvent, JsonOptions);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    var channel = await GetChannelAsync(cancellationToken);
                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = alertEvent.EventId.ToString(),
                        ContentType = "application/json",
                    };

                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(ConfirmTimeout);

                    await channel.BasicPublishAsync(exchange, routingKey, mandatory: true, properties, body,
                        timeout.Token);
                    return true;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (PublishReturnException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogWarning(exception,
                        "Publishing alert {AlertId} (event {EventId}) to {RoutingKey} failed on attempt {Attempt} of {MaxAttempts}",
                        alertEvent.AlertId, alertEvent.EventId, routingKey, attempt, MaxAttempts);

                    if (attempt < MaxAttempts)
                    {
                        await Task.Delay(RetryDelay, cancellationToken);
                    }
                }
            }

            logger.LogError(
                "Alert {AlertId} (event {EventId}) was not published to {RoutingKey}",
                alertEvent.AlertId, alertEvent.EventId, routingKey);
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        if (_channel is not null)
        {
            _channel.BasicReturnAsync -= OnBasicReturnAsync;
            await _channel.DisposeAsync();
            _channel = null;
        }

        var connection = await connectionProvider.GetConnectionAsync(cancellationToken);
        var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(publisherConfirmationsEnabled: true, publisherConfirmationTrackingEnabled: true),
            cancellationToken);
        channel.BasicReturnAsync += OnBasicReturnAsync;

        await channel.ExchangeDeclareAsync(options.Value.Exchange, ExchangeType.Topic, durable: true,
            autoDelete: false, cancellationToken: cancellationToken);

        _channel = channel;
        return channel;
    }

    private Task OnBasicReturnAsync(object sender, BasicReturnEventArgs args)
    {
        logger.LogWarning(
            "RabbitMQ returned alert event {EventId}: {ReplyCode} {ReplyText} (exchange {Exchange}, routing key {RoutingKey})",
            args.BasicProperties.MessageId, args.ReplyCode, args.ReplyText, args.Exchange, args.RoutingKey);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            _channel.BasicReturnAsync -= OnBasicReturnAsync;
            try
            {
                if (_channel.IsOpen)
                {
                    await _channel.CloseAsync();
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Error while closing the alert publisher channel");
            }

            await _channel.DisposeAsync();
            _channel = null;
        }

        _lock.Dispose();
    }
}
