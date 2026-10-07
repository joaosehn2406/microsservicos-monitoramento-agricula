namespace BackendApi.DTOs.Responses;

public sealed record WeatherReadingResponse(
    Guid Id,
    Guid EventId,
    string ClientId,
    Guid PropertyId,
    string PropertyName,
    double Latitude,
    double Longitude,
    DateTime ObservedAt,
    DateTime CollectedAt,
    double TemperatureCelsius,
    double RelativeHumidityPercent,
    double PrecipitationMm,
    double SoilMoisture,
    DateTime CreatedAt);
