using BackendApi.DTOs.Requests;
using BackendApi.Entities;

namespace BackendApi.Repositories;

public interface IWeatherReadingRepository
{
    Task<(IReadOnlyList<WeatherReading> Items, int TotalItems)> ListAsync(
        ReadingQuery query,
        CancellationToken cancellationToken);

    /// <summary>Most recent reading (by observed_at) of each property.</summary>
    Task<IReadOnlyList<WeatherReading>> ListLatestAsync(Guid? propertyId, CancellationToken cancellationToken);
}
