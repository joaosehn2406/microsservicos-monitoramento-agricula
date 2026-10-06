using WeatherCollector.Messaging.Contracts;

namespace WeatherCollector.Messaging.Publishers;

public interface IWeatherPublisher
{
    Task<bool> PublishAsync(
        WeatherReadingEvent weatherEvent,
        CancellationToken cancellationToken);
}
