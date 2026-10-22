namespace WeatherCollector.Contracts.Events;

public sealed record WeatherReadingEvent(
    Guid EventId,
    DateTimeOffset CollectedAt,
    string ClientId,
    Guid PropertyId,
    string PropertyName,
    double Latitude,
    double Longitude,
    DateTimeOffset ObservedAt,
    double TemperatureCelsius,
    double RelativeHumidityPercent,
    double PrecipitationMm,
    double SoilMoisture);
