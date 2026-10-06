namespace WeatherCollector.Workers;

public sealed class WeatherCollectionWorker : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}
