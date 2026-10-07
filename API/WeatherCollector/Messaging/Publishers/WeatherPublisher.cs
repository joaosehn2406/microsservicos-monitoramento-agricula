using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using WeatherCollector.Configurations;
using WeatherCollector.Messaging.Contracts;

namespace WeatherCollector.Messaging.Publishers;

public sealed class WeatherPublisher(
    IOptions<RabbitMqOptions> options,
    ILogger<WeatherPublisher> logger) : IWeatherPublisher, IHostedService, IAsyncDisposable
{
    private const string RoutingKey = "weather.reading";
    private const int MaximumPublishAttempts = 3;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options = options.Value;
    private readonly ILogger<WeatherPublisher> _logger = logger;
    private readonly SemaphoreSlim _publishLock = new(1, 1);

    private IConnection? _connection;
    private IChannel? _channel;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            await ConnectUntilAvailableAsync(cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async Task<bool> PublishAsync(
        WeatherReadingEvent weatherEvent,
        CancellationToken cancellationToken)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(weatherEvent, SerializerOptions);
        using var scope = _logger.BeginScope(
            new Dictionary<string, object>
            {
                ["clientId"] = weatherEvent.ClientId,
                ["propertyId"] = weatherEvent.PropertyId,
                ["eventId"] = weatherEvent.EventId
            });

        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            for (var attempt = 1; attempt <= MaximumPublishAttempts; attempt++)
            {
                try
                {
                    if (_connection?.IsOpen != true || _channel?.IsOpen != true)
                    {
                        await ResetConnectionAsync();
                        await ConnectUntilAvailableAsync(cancellationToken);
                    }

                    var properties = new BasicProperties
                    {
                        ContentType = "application/json",
                        DeliveryMode = DeliveryModes.Persistent,
                        MessageId = weatherEvent.EventId.ToString(),
                        Type = RoutingKey,
                        Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    };

                    await _channel!.BasicPublishAsync(
                        exchange: _options.Exchange,
                        routingKey: RoutingKey,
                        mandatory: true,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: cancellationToken);

                    _logger.LogDebug(
                        "Evento {eventId} confirmado pelo RabbitMQ na tentativa {Attempt}",
                        weatherEvent.EventId,
                        attempt);

                    return true;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (PublishException exception)
                {
                    _logger.LogWarning(
                        exception,
                        "RabbitMQ não confirmou ou devolveu o evento {eventId} na tentativa {Attempt} de {MaximumAttempts}",
                        weatherEvent.EventId,
                        attempt,
                        MaximumPublishAttempts);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Falha ao publicar o evento {eventId} na tentativa {Attempt} de {MaximumAttempts}",
                        weatherEvent.EventId,
                        attempt,
                        MaximumPublishAttempts);

                    await ResetConnectionAsync();
                }
            }

            _logger.LogError(
                "Evento {eventId} descartado após {MaximumAttempts} tentativas de publicação",
                weatherEvent.EventId,
                MaximumPublishAttempts);

            return false;
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _publishLock.WaitAsync(cancellationToken);
        try
        {
            await ResetConnectionAsync(cancellationToken);
        }
        finally
        {
            _publishLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ResetConnectionAsync();
        _publishLock.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task ConnectUntilAvailableAsync(CancellationToken cancellationToken)
    {
        var reconnectDelay = TimeSpan.FromSeconds(_options.ReconnectDelaySeconds);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    UserName = _options.Username,
                    Password = _options.Password,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = reconnectDelay,
                    ClientProvidedName = "weather-collector"
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(
                    new CreateChannelOptions(
                        publisherConfirmationsEnabled: true,
                        publisherConfirmationTrackingEnabled: true),
                    cancellationToken);

                _channel.BasicReturnAsync += OnBasicReturnAsync;

                await _channel.ExchangeDeclareAsync(
                    exchange: _options.Exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                _logger.LogInformation(
                    "Conectado ao RabbitMQ em {RabbitMqHost}:{RabbitMqPort}; exchange {Exchange} declarado",
                    _options.Host,
                    _options.Port,
                    _options.Exchange);

                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "RabbitMQ indisponível em {RabbitMqHost}:{RabbitMqPort}; nova tentativa em {ReconnectDelaySeconds}s",
                    _options.Host,
                    _options.Port,
                    reconnectDelay.TotalSeconds);

                await ResetConnectionAsync();
                await Task.Delay(reconnectDelay, cancellationToken);
            }
        }
    }

    private Task OnBasicReturnAsync(object sender, BasicReturnEventArgs args)
    {
        _logger.LogWarning(
            "RabbitMQ devolveu a mensagem {eventId}: código {ReplyCode}, motivo {ReplyText}, exchange {Exchange}, routing key {RoutingKey}",
            args.BasicProperties.MessageId,
            args.ReplyCode,
            args.ReplyText,
            args.Exchange,
            args.RoutingKey);

        return Task.CompletedTask;
    }

    private async Task ResetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is not null)
        {
            try
            {
                _channel.BasicReturnAsync -= OnBasicReturnAsync;
                if (_channel.IsOpen)
                {
                    await _channel.CloseAsync(cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Falha ao fechar o canal RabbitMQ durante o encerramento");
            }
            finally
            {
                try
                {
                    await _channel.DisposeAsync();
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(exception, "Falha ao liberar o canal RabbitMQ");
                }

                _channel = null;
            }
        }

        if (_connection is not null)
        {
            try
            {
                if (_connection.IsOpen)
                {
                    await _connection.CloseAsync(cancellationToken);
                }
            }
            catch (Exception exception)
            {
                _logger.LogDebug(exception, "Falha ao fechar a conexão RabbitMQ durante o encerramento");
            }
            finally
            {
                try
                {
                    await _connection.DisposeAsync();
                }
                catch (Exception exception)
                {
                    _logger.LogDebug(exception, "Falha ao liberar a conexão RabbitMQ");
                }

                _connection = null;
            }
        }
    }
}
