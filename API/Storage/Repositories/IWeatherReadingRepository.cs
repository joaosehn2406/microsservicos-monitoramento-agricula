using Storage.Entities;

namespace Storage.Repositories;

public interface IWeatherReadingRepository
{
    /// <summary>
    /// Inserts the processed-event marker and the reading in one transaction (SPEC 3.3).
    /// Returns false when the event was already processed.
    /// </summary>
    Task<bool> TryAddAsync(ProcessedEvent processedEvent, WeatherReading reading, CancellationToken cancellationToken);
}
