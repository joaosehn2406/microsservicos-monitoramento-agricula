using Microsoft.EntityFrameworkCore;
using Npgsql;
using Storage.Infrastructure.Messaging;
using Storage.Infrastructure.Persistence;
using Storage.Contracts.Events;
using Storage.Domain;

namespace Storage.Application;

public sealed class WeatherReadingService(StorageDbContext context)
{
    public const string ConsumerName = "storage";
    private static readonly HashSet<string> DuplicateConstraints =
        ["pk_processed_events", "uq_weather_readings_event_id"];

    public async Task<ProcessingResult> ProcessAsync(WeatherReadingEvent message, CancellationToken cancellationToken)
    {
        Validate(message);

        var processedEvent = new ProcessedEvent { EventId = message.EventId, Consumer = ConsumerName };
        var reading = new WeatherReading
        {
            EventId = message.EventId,
            ClientId = message.ClientId,
            PropertyId = message.PropertyId,
            PropertyName = message.PropertyName,
            Latitude = message.Latitude,
            Longitude = message.Longitude,
            ObservedAt = message.ObservedAt.ToUniversalTime(),
            CollectedAt = message.CollectedAt.ToUniversalTime(),
            TemperatureCelsius = message.TemperatureCelsius,
            RelativeHumidityPercent = message.RelativeHumidityPercent,
            PrecipitationMm = message.PrecipitationMm,
            SoilMoisture = message.SoilMoisture
        };

        try
        {
            context.ProcessedEvents.Add(processedEvent);
            context.WeatherReadings.Add(reading);
            await context.SaveChangesAsync(cancellationToken);
            return ProcessingResult.Processed;
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
                                                   {
                                                       SqlState: PostgresErrorCodes.UniqueViolation
                                                   } postgres
                                                   && DuplicateConstraints.Contains(postgres.ConstraintName ?? ""))
        {
            context.ChangeTracker.Clear();
            return ProcessingResult.Duplicate;
        }
    }

    private static void Validate(WeatherReadingEvent message)
    {
        var errors = new List<string>();

        if (message.EventId == Guid.Empty) errors.Add("eventId is empty");
        if (string.IsNullOrWhiteSpace(message.ClientId)) errors.Add("clientId is empty");
        if (message.PropertyId == Guid.Empty) errors.Add("propertyId is empty");
        if (string.IsNullOrWhiteSpace(message.PropertyName)) errors.Add("propertyName is empty");
        if (message.CollectedAt.Kind == DateTimeKind.Unspecified) errors.Add("collectedAt has no UTC offset");
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
