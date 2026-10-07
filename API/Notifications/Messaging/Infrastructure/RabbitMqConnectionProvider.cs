using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Notifications.Configurations;

namespace Notifications.Messaging.Infrastructure;

/// <summary>
/// Owns the single RabbitMQ connection of the service. The first caller connects,
/// retrying every 5 s until the broker answers; later callers reuse the connection.
/// </summary>
public sealed class RabbitMqConnectionProvider(
    IOptions<RabbitMqOptions> options,
    ILogger<RabbitMqConnectionProvider> logger) : IAsyncDisposable
{
    private static readonly TimeSpan ConnectRetryDelay = TimeSpan.FromSeconds(5);

    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            _connection ??= await ConnectAsync(cancellationToken);
            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<IConnection> ConnectAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var factory = new ConnectionFactory
        {
            HostName = settings.Host,
            Port = settings.Port,
            UserName = settings.Username,
            Password = settings.Password,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = $"notifications-{Environment.MachineName}"
        };

        while (true)
        {
            try
            {
                var connection = await factory.CreateConnectionAsync(cancellationToken);
                logger.LogInformation("Connected to RabbitMQ at {Host}:{Port}", settings.Host, settings.Port);
                return connection;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(
                    "RabbitMQ at {Host}:{Port} is not reachable ({Reason}); retrying in {Delay}s",
                    settings.Host, settings.Port, exception.Message, ConnectRetryDelay.TotalSeconds);
                await Task.Delay(ConnectRetryDelay, cancellationToken);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is null)
        {
            return;
        }

        try
        {
            await _connection.CloseAsync();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Error while closing the RabbitMQ connection");
        }

        await _connection.DisposeAsync();
        _connection = null;
        _lock.Dispose();
    }
}
