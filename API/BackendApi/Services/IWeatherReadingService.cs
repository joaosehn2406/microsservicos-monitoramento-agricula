using BackendApi.DTOs.Requests;
using BackendApi.DTOs.Responses;

namespace BackendApi.Services;

public interface IWeatherReadingService
{
    Task<PagedResponse<WeatherReadingResponse>> ListAsync(ReadingQuery query, CancellationToken cancellationToken);

    Task<IReadOnlyList<WeatherReadingResponse>> ListLatestAsync(Guid? propertyId, CancellationToken cancellationToken);
}
