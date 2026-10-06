using Storage.Entities;
using Storage.Messaging.Contracts;

namespace Storage.Mappers;

public static class WeatherReadingMapper
{
    public static WeatherReading ToEntity(WeatherReadingEvent message) => new()
    {
        Id = Guid.CreateVersion7(),
        EventId = message.EventId,
        ClientId = message.ClientId,
        PropertyId = message.PropertyId,
        PropertyName = message.PropertyName,
        Latitude = message.Latitude,
        Longitude = message.Longitude,
        ObservedAt = message.ObservedAt.ToUniversalTime(),
        CollectedAt = message.OccurredAt.ToUniversalTime(),
        TemperatureCelsius = message.TemperatureCelsius,
        RelativeHumidityPercent = message.RelativeHumidityPercent,
        PrecipitationMm = message.PrecipitationMm,
        SoilMoisture = message.SoilMoisture
    };
}
