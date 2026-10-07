using Microsoft.Extensions.Options;
using Notifications.Configurations;
using Notifications.Messaging.Contracts;
using Notifications.Messaging.Infrastructure;
using Notifications.Services;

namespace Notifications.Messaging.Consumers;

public sealed class AlertRaisedConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<AlertRaisedConsumer> logger)
    : RabbitMqConsumerBase<AlertRaisedEvent>(connectionProvider, scopeFactory, options, logger)
{
    private readonly RabbitMqOptions _settings = options.Value;

    protected override string QueueName => _settings.Queue;

    protected override string BindingKey => _settings.BindingKey;

    protected override Guid GetEventId(AlertRaisedEvent message) => message.EventId;

    protected override Task<ProcessingResult> ProcessAsync(
        AlertRaisedEvent message,
        IServiceProvider services,
        CancellationToken cancellationToken) =>
        services.GetRequiredService<INotificationService>().ProcessAsync(message, cancellationToken);
}
