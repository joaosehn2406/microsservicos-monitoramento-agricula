using Microsoft.Extensions.Options;
using Notifications.Application;
using Notifications.Contracts.Events;

namespace Notifications.Infrastructure.Messaging;

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
        services.GetRequiredService<NotificationService>().ProcessAsync(message, cancellationToken);
}
