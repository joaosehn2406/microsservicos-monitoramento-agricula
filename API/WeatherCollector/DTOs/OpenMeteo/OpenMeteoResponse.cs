using System.Text.Json.Serialization;

namespace WeatherCollector.DTOs.OpenMeteo;

public sealed record OpenMeteoResponse(
    [property: JsonPropertyName("current")] OpenMeteoCurrent? Current);

public sealed record OpenMeteoCurrent(
    [property: JsonPropertyName("time")] string? Time,
    [property: JsonPropertyName("temperature_2m")] double? TemperatureCelsius,
    [property: JsonPropertyName("relative_humidity_2m")] double? RelativeHumidityPercent,
    [property: JsonPropertyName("precipitation")] double? PrecipitationMillimeters,
    [property: JsonPropertyName("soil_moisture_0_to_1cm")] double? SoilMoisture);
