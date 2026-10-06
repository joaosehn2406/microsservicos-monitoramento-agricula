using Storage.Messaging.Contracts;
using Storage.Messaging.Infrastructure;

namespace Storage.Services;

public interface IWeatherReadingService
{
    Task<ProcessingResult> ProcessAsync(WeatherReadingEvent message, CancellationToken cancellationToken);
}
