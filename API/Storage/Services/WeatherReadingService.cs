using Storage.Entities;
using Storage.Exceptions;
using Storage.Mappers;
using Storage.Messaging.Contracts;
using Storage.Messaging.Infrastructure;
using Storage.Repositories;

namespace Storage.Services;

public sealed class WeatherReadingService(IWeatherReadingRepository repository) : IWeatherReadingService
{
    public const string ConsumerName = "storage";
    public const string ExpectedEventType = "weather.reading";

    public async Task<ProcessingResult> ProcessAsync(WeatherReadingEvent message, CancellationToken cancellationToken)
    {
        Validate(message);

        var processedEvent = new ProcessedEvent { EventId = message.EventId, Consumer = ConsumerName };
        var reading = WeatherReadingMapper.ToEntity(message);

        var added = await repository.TryAddAsync(processedEvent, reading, cancellationToken);
        return added ? ProcessingResult.Processed : ProcessingResult.Duplicate;
    }

    private static void Validate(WeatherReadingEvent message)
    {
        var errors = new List<string>();

        if (message.EventId == Guid.Empty) errors.Add("eventId is empty");
        if (message.EventType != ExpectedEventType) errors.Add($"eventType must be '{ExpectedEventType}'");
        if (string.IsNullOrWhiteSpace(message.Source)) errors.Add("source is empty");
        if (string.IsNullOrWhiteSpace(message.ClientId)) errors.Add("clientId is empty");
        if (message.PropertyId == Guid.Empty) errors.Add("propertyId is empty");
        if (string.IsNullOrWhiteSpace(message.PropertyName)) errors.Add("propertyName is empty");
        if (message.OccurredAt.Kind == DateTimeKind.Unspecified) errors.Add("occurredAt has no UTC offset");
        if (message.ObservedAt.Kind == DateTimeKind.Unspecified) errors.Add("observedAt has no UTC offset");
        if (!InRange(message.Latitude, -90, 90)) errors.Add("latitude is out of range");
        if (!InRange(message.Longitude, -180, 180)) errors.Add("longitude is out of range");
        if (!double.IsFinite(message.TemperatureCelsius)) errors.Add("temperatureCelsius is not a number");
        if (!InRange(message.RelativeHumidityPercent, 0, 100)) errors.Add("relativeHumidityPercent is out of range");
        if (!InRange(message.PrecipitationMm, 0, double.MaxValue)) errors.Add("precipitationMm is out of range");
        if (!InRange(message.SoilMoisture, 0, 1)) errors.Add("soilMoisture is out of range");

        if (errors.Count > 0)
        {
            throw new InvalidMessageException($"Invalid weather.reading event: {string.Join("; ", errors)}");
        }
    }

    private static bool InRange(double value, double min, double max) =>
        double.IsFinite(value) && value >= min && value <= max;
}
