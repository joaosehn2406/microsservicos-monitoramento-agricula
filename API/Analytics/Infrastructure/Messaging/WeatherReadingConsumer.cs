using Microsoft.Extensions.Options;
using Analytics.Application;
using Analytics.Contracts.Events;

namespace Analytics.Infrastructure.Messaging;

public sealed class WeatherReadingConsumer(
    RabbitMqConnectionProvider connectionProvider,
    IServiceScopeFactory scopeFactory,
    IOptions<RabbitMqOptions> options,
    ILogger<WeatherReadingConsumer> logger)
    : RabbitMqConsumerBase<WeatherReadingEvent>(connectionProvider, scopeFactory, options, logger)
{
    private readonly RabbitMqOptions _settings = options.Value;

    protected override string QueueName => _settings.Queue;

    protected override string BindingKey => _settings.BindingKey;

    protected override Guid GetEventId(WeatherReadingEvent message) => message.EventId;

    protected override Task<ProcessingResult> ProcessAsync(
        WeatherReadingEvent message,
        IServiceProvider services,
        CancellationToken cancellationToken) =>
        services.GetRequiredService<WeatherAnalysisService>().ProcessAsync(message, cancellationToken);
}
