namespace WeatherCollector.Messaging.Contracts;

public sealed record WeatherReadingEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
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
