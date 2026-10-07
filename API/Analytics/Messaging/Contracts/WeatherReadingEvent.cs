namespace Analytics.Messaging.Contracts;

/// <summary><c>weather.reading</c> event (SPEC 5.7). Every field is required.</summary>
public sealed record WeatherReadingEvent
{
    public required Guid EventId { get; init; }
    public required string EventType { get; init; }
    public required DateTime OccurredAt { get; init; }
    public required string Source { get; init; }
    public required string ClientId { get; init; }
    public required Guid PropertyId { get; init; }
    public required string PropertyName { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required DateTime ObservedAt { get; init; }
    public required double TemperatureCelsius { get; init; }
    public required double RelativeHumidityPercent { get; init; }
    public required double PrecipitationMm { get; init; }
    public required double SoilMoisture { get; init; }
}
