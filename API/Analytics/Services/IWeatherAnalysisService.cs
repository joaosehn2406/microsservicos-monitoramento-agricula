using Analytics.Messaging.Contracts;
using Analytics.Messaging.Infrastructure;

namespace Analytics.Services;

public interface IWeatherAnalysisService
{
    Task<ProcessingResult> ProcessAsync(WeatherReadingEvent message, CancellationToken cancellationToken);
}
