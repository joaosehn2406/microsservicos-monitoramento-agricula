namespace WeatherCollector.Configurations;

public sealed class OpenMeteoOptions
{
    public const string SectionName = "OpenMeteo";

    public string BaseUrl { get; init; } = "https://api.open-meteo.com";
    public double TimeoutSeconds { get; init; } = 10;
    public OpenMeteoLimitsOptions Limits { get; init; } = new();
}

public sealed class OpenMeteoLimitsOptions
{
    public double RequestsPerMinute { get; init; } = 600;
    public double RequestsPerHour { get; init; } = 5000;
    public double RequestsPerDay { get; init; } = 10000;
}
