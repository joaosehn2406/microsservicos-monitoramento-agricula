namespace WeatherCollector.Configurations;

public sealed class CollectorOptions
{
    public const string SectionName = "Collector";

    public double IntervalSeconds { get; init; } = 60;
}
