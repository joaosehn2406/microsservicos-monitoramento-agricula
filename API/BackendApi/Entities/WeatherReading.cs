namespace BackendApi.Entities;

public sealed class WeatherReading
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public Guid PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime ObservedAt { get; set; }
    public DateTime CollectedAt { get; set; }
    public double TemperatureCelsius { get; set; }
    public double RelativeHumidityPercent { get; set; }
    public double PrecipitationMm { get; set; }
    public double SoilMoisture { get; set; }
    public DateTime CreatedAt { get; set; }
}
