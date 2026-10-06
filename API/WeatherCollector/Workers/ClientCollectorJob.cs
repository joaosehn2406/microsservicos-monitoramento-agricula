using Microsoft.Extensions.Options;
using WeatherCollector.Clients;
using WeatherCollector.Configurations;
using WeatherCollector.Mappers;
using WeatherCollector.Messaging.Publishers;

namespace WeatherCollector.Workers;

public sealed class ClientCollectorJob(
    FarmClient client,
    int clientIndex,
    IOptions<CollectorOptions> collectorOptions,
    IOpenMeteoClient openMeteoClient,
    IWeatherPublisher publisher,
    TimeProvider timeProvider,
    ILogger<ClientCollectorJob> logger) : BackgroundService
{
    private readonly FarmClient _client = client;
    private readonly int _clientIndex = clientIndex;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(collectorOptions.Value.IntervalSeconds);
    private readonly IOpenMeteoClient _openMeteoClient = openMeteoClient;
    private readonly IWeatherPublisher _publisher = publisher;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<ClientCollectorJob> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var initialDelay = TimeSpan.FromTicks(_interval.Ticks * _clientIndex / 3);

        _logger.LogInformation(
            "Job do cliente {ClientId} iniciado com intervalo de {IntervalSeconds}s e atraso inicial de {InitialDelaySeconds}s",
            _client.Id,
            _interval.TotalSeconds,
            initialDelay.TotalSeconds);

        if (initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(initialDelay, _timeProvider, stoppingToken);
        }

        using var timer = new PeriodicTimer(_interval, _timeProvider);

        do
        {
            await CollectCycleAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CollectCycleAsync(CancellationToken cancellationToken)
    {
        foreach (var property in _client.Properties)
        {
            using var scope = _logger.BeginScope(
                new Dictionary<string, object>
                {
                    ["clientId"] = _client.Id,
                    ["propertyId"] = property.Id
                });

            try
            {
                var collectedAt = _timeProvider.GetUtcNow();
                var response = await _openMeteoClient.GetCurrentAsync(property, cancellationToken);
                var weatherEvent = WeatherReadingMapper.ToEvent(
                    response,
                    _client,
                    property,
                    collectedAt);

                var published = await _publisher.PublishAsync(weatherEvent, cancellationToken);
                if (published)
                {
                    _logger.LogInformation(
                        "Leitura meteorológica publicada com o evento {eventId}",
                        weatherEvent.EventId);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Falha ao coletar ou publicar a leitura; somente a propriedade atual será ignorada neste ciclo");
            }
        }
    }
}
