using System.Globalization;
using Microsoft.Extensions.Options;
using WeatherCollector.Application.Exceptions;
using WeatherCollector.Contracts.Events;
using WeatherCollector.Infrastructure.Clients;
using WeatherCollector.Infrastructure.Messaging;
using WeatherCollector.Infrastructure.Options;

namespace WeatherCollector.Infrastructure.Workers;

public sealed class ClientCollectorJob(
    FarmClient client,
    int clientIndex,
    IOptions<CollectorOptions> collectorOptions,
    OpenMeteoClient openMeteoClient,
    WeatherPublisher publisher,
    TimeProvider timeProvider,
    ILogger<ClientCollectorJob> logger) : BackgroundService
{
    private readonly FarmClient _client = client;
    private readonly int _clientIndex = clientIndex;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(collectorOptions.Value.IntervalSeconds);
    private readonly OpenMeteoClient _openMeteoClient = openMeteoClient;
    private readonly WeatherPublisher _publisher = publisher;
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
                var weatherEvent = ToEvent(
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

    private static WeatherReadingEvent ToEvent(
        OpenMeteoResponse response,
        FarmClient client,
        FarmProperty property,
        DateTimeOffset collectedAt)
    {
        var current = response.Current
            ?? throw new OpenMeteoException("A resposta da Open-Meteo não contém dados atuais.");

        if (!DateTimeOffset.TryParse(
                current.Time,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var observedAt))
        {
            throw new OpenMeteoException("O horário atual retornado pela Open-Meteo é inválido.");
        }

        return new WeatherReadingEvent(
            Guid.NewGuid(),
            collectedAt.ToUniversalTime(),
            client.Id,
            property.Id,
            property.Name,
            property.Latitude,
            property.Longitude,
            observedAt,
            current.TemperatureCelsius
                ?? throw new OpenMeteoException("Temperatura ausente na resposta."),
            current.RelativeHumidityPercent
                ?? throw new OpenMeteoException("Umidade relativa ausente na resposta."),
            current.PrecipitationMillimeters
                ?? throw new OpenMeteoException("Precipitação ausente na resposta."),
            current.SoilMoisture
                ?? throw new OpenMeteoException("Umidade do solo ausente na resposta."));
    }
}
