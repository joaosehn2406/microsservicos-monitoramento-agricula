using BackendApi.DTOs.Responses;
using BackendApi.Entities;

namespace BackendApi.Mappers;

public static class WeatherReadingMapper
{
    public static WeatherReadingResponse ToResponse(WeatherReading reading) => new(
        reading.Id,
        reading.EventId,
        reading.ClientId,
        reading.PropertyId,
        reading.PropertyName,
        reading.Latitude,
        reading.Longitude,
        reading.ObservedAt,
        reading.CollectedAt,
        reading.TemperatureCelsius,
        reading.RelativeHumidityPercent,
        reading.PrecipitationMm,
        reading.SoilMoisture,
        reading.CreatedAt);
}
